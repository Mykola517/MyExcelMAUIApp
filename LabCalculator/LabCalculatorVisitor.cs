using System;
using System.Globalization;
using System.Linq;

namespace LabCalculator
{
    public class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        public static Dictionary<string, double> tableIdentifier = new Dictionary<string, double>();
        private readonly Func<string, double> _cellResolver;

        public LabCalculatorVisitor(Func<string, double> cellResolver)
        {
            _cellResolver = cellResolver;
        }

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return double.Parse(context.GetText(), CultureInfo.InvariantCulture);
        }

        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            string cellName = context.GetText();
            return _cellResolver(cellName);
        }

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                return left + right;
            }
            else
            {
                return left - right;
            }
        }

        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                return left * right;
            }
            else
            {
                if (right == 0)
                {
                    throw new DivideByZeroException("Division by zero!");
                }
                return left / right;
            }
        }

        public override double VisitIncExpr(LabCalculatorParser.IncExprContext context)
        {
            double value = Visit(context.expression());
            return value + 1;
        }

        public override double VisitDecExpr(LabCalculatorParser.DecExprContext context)
        {
            double value = Visit(context.expression());
            return value - 1;
        }

        public override double VisitMaxExpr(LabCalculatorParser.MaxExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));
            return Math.Max(left, right);
        }

        public override double VisitMinExpr(LabCalculatorParser.MinExprContext context)
        {
            double left = Visit(context.expression(0));
            double right = Visit(context.expression(1));
            return Math.Min(left, right);
        }

        public override double VisitMmaxExpr(LabCalculatorParser.MmaxExprContext context)
        {
            double maxValue = double.NegativeInfinity;

            foreach (var child in context.paramlist.children.OfType<LabCalculatorParser.ExpressionContext>())
            {
                double childValue = this.Visit(child);
                if (childValue > maxValue)
                {
                    maxValue = childValue;
                }
            }
            return maxValue;
        }
        public override double VisitMminExpr(LabCalculatorParser.MminExprContext context)
        {
            double minValue = double.PositiveInfinity;

            foreach (var child in context.paramlist.children.OfType<LabCalculatorParser.ExpressionContext>())
            {
                double childValue = this.Visit(child);
                if (childValue < minValue)
                {
                    minValue = childValue;
                }
            }
            return minValue;
        }
    }
}