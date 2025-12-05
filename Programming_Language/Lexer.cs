// File: Lexer.cs
// Target: C# 7.3
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleLang
{
    public class Lexer
    {
        private readonly string _line;
        private int _pos;

        public Lexer(string line)
        {
            _line = line ?? "";
            _pos = 0;
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (_pos < _line.Length)
            {
                char c = _line[_pos];

                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }

                if (char.IsLetter(c))
                {
                    string word = ReadWhile(ch => char.IsLetterOrDigit(ch));
                    tokens.Add(CreateKeywordOrIdentifier(word));
                    continue;
                }

                if (char.IsDigit(c) || c == '.')
                {
                    string number = ReadNumber();
                    tokens.Add(new Token(TokenType.Number, number));
                    continue;
                }

                switch (c)
                {
                    case '+': tokens.Add(new Token(TokenType.Plus, "+")); _pos++; break;
                    case '-': tokens.Add(new Token(TokenType.Minus, "-")); _pos++; break;
                    case '*': tokens.Add(new Token(TokenType.Multiply, "*")); _pos++; break;
                    case '/': tokens.Add(new Token(TokenType.Divide, "/")); _pos++; break;
                    case '%': tokens.Add(new Token(TokenType.Remainder, "%")); _pos++; break;
                    case '^': tokens.Add(new Token(TokenType.Power, "^")); _pos++; break;

                    case '(':
                        tokens.Add(new Token(TokenType.LParen, "(")); _pos++; break;
                    case ')':
                        tokens.Add(new Token(TokenType.RParen, ")")); _pos++; break;
                    case ',':
                        tokens.Add(new Token(TokenType.Comma, ",")); _pos++; break;

                    case '{':
                        tokens.Add(new Token(TokenType.LBrace, "{"));
                        _pos++;
                        break;

                    case '}':
                        tokens.Add(new Token(TokenType.RBrace, "}"));
                        _pos++;
                        break;


                    case '>':
                        Advance();
                        if (Current == '=') { tokens.Add(new Token(TokenType.GreaterOrEqual, ">=")); Advance(); }
                        else tokens.Add(new Token(TokenType.Greater, ">"));
                        break;

                    case '<':
                        Advance();
                        if (Current == '=') { tokens.Add(new Token(TokenType.LessOrEqual, "<=")); Advance(); }
                        else tokens.Add(new Token(TokenType.Less, "<"));
                        break;

                    case '=':
                        Advance();
                        if (Current == '=') { tokens.Add(new Token(TokenType.EqualEqual, "==")); Advance(); }
                        else tokens.Add(new Token(TokenType.Assign, "="));
                        break;

                    case '!':
                        Advance();
                        if (Current == '=') { tokens.Add(new Token(TokenType.NotEqual, "!=")); Advance(); }
                        else throw new Exception("Unexpected token '!'");
                        break;

                    default:
                        throw new Exception("Unexpected character: '" + c + "'");
                }

            }

            // tokens.Add(new Token(TokenType.EndOfLine, ""));
            tokens.Add(new Token(TokenType.EndOfFile, ""));

            return tokens;
        }

        private Token CreateKeywordOrIdentifier(string word)
        {
            switch (word.ToLowerInvariant())
            {
                case "let":
                    return new Token(TokenType.KeywordLet, word);
                case "input":
                    return new Token(TokenType.KeywordInput, word);
                case "output":
                    return new Token(TokenType.KeywordOutput, word);
                case "if":
                    return new Token(TokenType.KeywordIf, word);
                case "else":
                    return new Token(TokenType.KeywordIf, word);
                // можно добавить другие ключевые слова по мере необходимости
                default:
                    return new Token(TokenType.Identifier, word);
            }
        }




        private string ReadWhile(Func<char, bool> condition)
        {
            StringBuilder sb = new StringBuilder();
            while (_pos < _line.Length && condition(_line[_pos]))
            {
                sb.Append(_line[_pos]);
                _pos++;
            }
            return sb.ToString();
        }


        private string ReadNumber()
        {
            StringBuilder sb = new StringBuilder();
            bool hasDot = false;

            while (_pos < _line.Length)
            {
                char ch = _line[_pos];

                if (char.IsDigit(ch))
                {
                    sb.Append(ch);
                    _pos++;
                }
                else if (ch == '.' && !hasDot)
                {
                    hasDot = true;
                    sb.Append(ch);
                    _pos++;
                }
                else
                {
                    break;
                }
            }

            return sb.ToString();
        }

        private char Current => _pos < _line.Length ? _line[_pos] : '\0';

        private void Advance()
        {
            _pos++;
        }


    }
}
