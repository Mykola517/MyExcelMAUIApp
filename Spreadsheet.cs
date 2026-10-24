using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using LabCalculator;

namespace MyExcelMAUIApp
{
    public class Cell
    {
        public string Expression = "";
        public double Value = 0;
        public string? Error = null;
        public int State = 0;

        public string DisplayValue
        {
            get
            {
                if (Error != null) return "#ПОМИЛКА";
                if (Expression.Trim() == "") return "";
                return Value.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    public class TableData
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<string> Expressions { get; set; } = new List<string>();
    }

    public class Spreadsheet
    {
        private List<List<Cell>> cells = new List<List<Cell>>();

        public int RowCount
        {
            get { return cells.Count; }
        }

        public int ColumnCount
        { 
            get
            {
                if (cells.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return cells[0].Count;
                }
            }
        }

        public Spreadsheet(int rows, int columns)
        {
            for (int r = 0; r < rows; r++) AddRow(columns);
        }

        public Cell GetCell(int row, int col)
        {
            return cells[row][col];
        }

        public void SetExpression(int row, int col, string text)
        {
            cells[row][col].Expression = text;
        }

        private void AddRow(int columns)
        {
            var row = new List<Cell>();
            for (int c = 0; c < columns; c++) row.Add(new Cell());
            cells.Add(row);
        }

        public void AddRow()
        {
            AddRow(ColumnCount);
        }

        public void DeleteRow()
        {
            if (RowCount > 1) cells.RemoveAt(RowCount - 1);
        }

        public void AddColumn()
        {
            foreach (var row in cells) row.Add(new Cell());
        }

        public void DeleteColumn()
        {
            if (ColumnCount > 1)
            {
                foreach (var row in cells) row.RemoveAt(row.Count - 1);
            }
        }

        public static string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

        private static bool TryParseName(string name, out int row, out int col)
        {
            row = 0;
            col = 0;
            int i = 0;

            while (i < name.Length && char.IsLetter(name[i]))
            {
                col = col * 26 + (char.ToUpper(name[i]) - 'A' + 1);
                if (col > 1000000) return false;
                i++;
            }

            if (i == 0 || i == name.Length) return false;
            if (!int.TryParse(name.Substring(i), out row)) return false;

            col = col - 1;
            row = row - 1;
            return row >= 0 && col >= 0;
        }

        private double ResolveCell(string name)
        {
            int r, c;
            TryParseName(name, out r, out c);
            return cells[r][c].Value;
        }

        public void Calculate()
        {
            foreach (var row in cells)
            {
                foreach (var cell in row)
                {
                    cell.State = 0;
                    cell.Error = null;
                    cell.Value = 0;
                }
            }

            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColumnCount; c++)
                {
                    CalculateCell(r, c);
                }
            }
        }

        private void CalculateCell(int row, int col)
        {
            var cell = cells[row][col];

            if (cell.State == 2) return;
            if (cell.State == 1) throw new Exception("Циклічне посилання");

            cell.State = 1;
            try
            {
                if (cell.Expression.Trim() != "")
                {
                    string? syntaxError = Calculator.Validate(cell.Expression);
                    if (syntaxError != null)
                    {
                        throw new Exception(syntaxError);
                    }

                    foreach (Match m in Regex.Matches(cell.Expression, "[a-zA-Z]+[1-9][0-9]*"))
                    {
                        int r, c;
                        if (!TryParseName(m.Value, out r, out c) || r >= RowCount || c >= ColumnCount)
                        {
                            throw new Exception("Клітинки " + m.Value + " немає в таблиці");
                        }

                        CalculateCell(r, c);

                        if (cells[r][c].Error != null)
                        {
                            throw new Exception("Помилка в клітинці " + m.Value);
                        }
                    }

                    cell.Value = Calculator.Evaluate(cell.Expression, ResolveCell);
                }
            }
            catch (Exception ex)
            {
                cell.Error = ex.Message;
            }
            cell.State = 2;
        }

        public void Save(string path)
        {
            var data = new TableData { Rows = RowCount, Columns = ColumnCount };
            foreach (var row in cells)
            {
                foreach (var cell in row)
                {
                    data.Expressions.Add(cell.Expression);
                }
            }

            File.WriteAllText(path, JsonSerializer.Serialize(data));
        }

        public void Load(string path)
        {
            var data = JsonSerializer.Deserialize<TableData>(File.ReadAllText(path));
            if (data == null || data.Rows < 1 || data.Columns < 1 ||
                data.Expressions.Count != data.Rows * data.Columns)
            {
                throw new Exception("Файл має неправильний формат");
            }

            cells.Clear();
            int k = 0;
            for (int r = 0; r < data.Rows; r++)
            {
                AddRow(data.Columns);
                for (int c = 0; c < data.Columns; c++)
                {
                    cells[r][c].Expression = data.Expressions[k];
                    k++;
                }
            }
        }
    }
}