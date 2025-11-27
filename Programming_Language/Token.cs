// File: Token.cs
// Target: C# 7.3
namespace SimpleLang
{
    public enum TokenType
    {
        Identifier,
        Number,
        KeywordLet,
        KeywordInt,
        KeywordInput,
        KeywordOutput,

        Plus,
        Minus,
        Multiply,
        Divide,
        Power,

        Assign,
        LParen,
        RParen,
        Comma,

        EndOfLine,
        EndOfFile
    }

    public class Token
    {
        public TokenType Type { get; private set; }
        public string Text { get; private set; }

        public Token(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }

        public override string ToString()
        {
            return Type + " : " + Text;
        }
    }
}
