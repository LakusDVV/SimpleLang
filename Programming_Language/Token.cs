// File: Token.cs
// Target: C# 7.3
namespace SimpleLang
{
    public enum TokenType
    {
        
        Number,
        Identifier,
        Plus,
        Minus,
        Multiply,
        Divide,
        Power,
        Remainder,
        Assign,
        LParen,
        RParen,
        Comma,
        LBrace,
        RBrace,
        EndOfLine,

        
        KeywordLet,
        KeywordInput,
        KeywordOutput,
        KeywordIf,
        KeywordElse,
        EndOfFile,

        
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual,
        EqualEqual,
        NotEqual
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
