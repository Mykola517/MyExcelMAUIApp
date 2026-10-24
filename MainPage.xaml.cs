using System;
using System.IO;
using LabCalculator;

namespace MyExcelMAUIApp
{
    public partial class MainPage : ContentPage
    {
        const int CountColumn = 10;
        const int CountRow = 15;

        Spreadsheet table = new Spreadsheet(CountRow, CountColumn);
        bool showValues = false;

        public MainPage()
        {
            InitializeComponent();
            RefreshGrid();
        }

        private void RecalculateIfNeeded()
        {
            if (showValues) table.Calculate();
        }

        private void RefreshGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            for (int col = 0; col < table.ColumnCount + 1; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                if (col > 0)
                {
                    var label = new Label
                    {
                        Text = Spreadsheet.GetColumnName(col),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);
                }
            }

            grid.RowDefinitions.Add(new RowDefinition());

            for (int row = 0; row < table.RowCount; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                var label = new Label
                {
                    Text = (row + 1).ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                Grid.SetRow(label, row + 1);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                for (int col = 0; col < table.ColumnCount; col++)
                {
                    var cell = table.GetCell(row, col);
                    var entry = new Entry
                    {
                        WidthRequest = 90,
                        IsReadOnly = showValues,
                        Text = showValues ? cell.DisplayValue : cell.Expression
                    };

                    if (showValues && cell.Error != null)
                    {
                        entry.TextColor = Colors.Red;
                    }

                    entry.Focused += Entry_Focused;
                    entry.Unfocused += Entry_Unfocused;
                    Grid.SetRow(entry, row + 1);
                    Grid.SetColumn(entry, col + 1);
                    grid.Children.Add(entry);
                }
            }
        }

        private void Entry_Focused(object? sender, FocusEventArgs e)
        {
            var entry = (Entry)sender!;
            var cell = table.GetCell(Grid.GetRow(entry) - 1, Grid.GetColumn(entry) - 1);

            if (showValues && cell.Error != null)
                textInput.Text = "Помилка: " + cell.Error;
            else
                textInput.Text = "Вираз: " + cell.Expression;
        }

        private void Entry_Unfocused(object? sender, FocusEventArgs e)
        {
            if (showValues) return;

            var entry = (Entry)sender!;
            var row = Grid.GetRow(entry) - 1;
            var col = Grid.GetColumn(entry) - 1;

            table.SetExpression(row, col, entry.Text ?? "");
        }

        private void CalculateButton_Clicked(object? sender, EventArgs e)
        {
            table.Calculate();

            showValues = true;
            modeButton.Text = "Режим: ЗНАЧЕННЯ";
            RefreshGrid();
        }

        private void ModeButton_Clicked(object? sender, EventArgs e)
        {
            showValues = !showValues;
            modeButton.Text = showValues ? "Режим: ЗНАЧЕННЯ" : "Режим: ВИРАЗ";

            RecalculateIfNeeded();
            RefreshGrid();
        }

        private async void SaveButton_Clicked(object? sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Збереження", "Введіть назву файлу:",
                "Зберегти", "Скасувати", "table1");
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), name + ".json");
                table.Save(path);
                await DisplayAlert("Збереження", "Таблицю збережено у файл:\n" + path, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", "Не вдалося зберегти файл: " + ex.Message, "OK");
            }
        }

        private async void ReadButton_Clicked(object? sender, EventArgs e)
        {
            try
            {
                var file = await FilePicker.Default.PickAsync(
                    new PickOptions { PickerTitle = "Оберіть файл з таблицею" });
                if (file == null) return;

                table.Load(file.FullPath);
                RecalculateIfNeeded();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", "Не вдалося прочитати файл: " + ex.Message, "OK");
            }
        }

        private async void ExitButton_Clicked(object? sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private async void HelpButton_Clicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Довідка",
               "Лабораторна робота 1. Миколи Друкачука", "OK");
        }

        private async void AboutButton_Clicked(object? sender, EventArgs e)
        {
            await DisplayAlert("Про програму",
                "Лабораторна робота 1. Електронні таблиці (варіант 13).\n" +
                "Операції: + - * /, inc, dec, max, min, mmax, mmin.",
                "OK");
        }

        private void AddRowButton_Clicked(object? sender, EventArgs e)
        {
            table.AddRow();
            RecalculateIfNeeded();
            RefreshGrid();
        }

        private void DeleteRowButton_Clicked(object? sender, EventArgs e)
        {
            table.DeleteRow();
            RecalculateIfNeeded();
            RefreshGrid();
        }

        private void AddColumnButton_Clicked(object? sender, EventArgs e)
        {
            table.AddColumn();
            RecalculateIfNeeded();
            RefreshGrid();
        }

        private void DeleteColumnButton_Clicked(object? sender, EventArgs e)
        {
            table.DeleteColumn();
            RecalculateIfNeeded();
            RefreshGrid();
        }
    }
}