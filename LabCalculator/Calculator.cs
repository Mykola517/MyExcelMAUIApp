using System;
using Antlr4.Runtime;

namespace LabCalculator
{
    public static class Calculator
    {
        private static LabCalculatorParser.CompileUnitContext Parse(string expression)
        {
            var listener = new ThrowExceptionErrorListener();

            var lexer = new LabCalculatorLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(listener);

            var parser = new LabCalculatorParser(new CommonTokenStream(lexer));
            parser.RemoveErrorListeners();
            parser.AddErrorListener(listener);

            return parser.compileUnit();
        }

        public static double Evaluate(string expression, Func<string, double> cellResolver)
        {
            var tree = Parse(expression);
            return new LabCalculatorVisitor(cellResolver).Visit(tree);
        }

        public static string? Validate(string expression)
        {
            try
            {
                Parse(expression);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}