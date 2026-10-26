using System;
using Antlr4.Runtime;

namespace LabCalculator
{
    public class ThrowExceptionErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol,
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException("Синтаксична помилка, позиція " + (charPositionInLine + 1) + ": " + msg, e);
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol,
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException("Недопустимий символ, позиція " + (charPositionInLine + 1) + ": " + msg, e);
        }
    }
}