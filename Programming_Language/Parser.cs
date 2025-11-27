// File: Parser.cs
// Target: C# 7.3
using System;
using System.Collections.Generic;

namespace SimpleLang
{
    public abstract class Node { }

    public class NumberNode : Node
    {
        public int Value { get; private set; }
        public NumberNode(int value) { Value = value; }
    }

    public class IdentifierNode : Node
    {
        public string Name { get; private set; }
        public IdentifierNode(string name) { Name = name; }
    }

    public class BinaryNode : Node
    {
        public Node Left { get; private set; }
        public Node Right { get; private set; }
        public TokenType Op { get; private set; }

        public BinaryNode(Node left, Node right, TokenType op)
        {
            Left = left;
            Right = right;
            Op = op;
        }
    }

    public class InputCallNode : Node { }


    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        private Token Current
        {
            get
            {
                if (_pos < _tokens.Count)
                    return _tokens[_pos];
                return new Token(TokenType.EndOfFile, "");
            }
        }

        private Token Consume()
        {
            Token t = Current;
            _pos++;
            return t;
        }

        private void Expect(TokenType type)
        {
            if (Current.Type != type)
                throw new Exception("Expected " + type + " but got " + Current.Type);
            _pos++;
        }

        public Node ParseExpression()
        {
            return ParseTerm();
        }

        private Node ParseTerm()
        {
            Node left = ParseFactor();

            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                Token op = Consume();
                Node right = ParseFactor();
                left = new BinaryNode(left, right, op.Type);
            }

            return left;
        }

        private Node ParseFactor()
        {
            Node left = ParsePower();

            while (Current.Type == TokenType.Multiply || Current.Type == TokenType.Divide)
            {
                Token op = Consume();
                Node right = ParsePower();
                left = new BinaryNode(left, right, op.Type);
            }

            return left;
        }

        private Node ParsePower()
        {
            Node left = ParsePrimary();

            while (Current.Type == TokenType.Power)
            {
                Token op = Consume();
                Node right = ParsePrimary();
                left = new BinaryNode(left, right, op.Type);
            }

            return left;
        }

        private Node ParsePrimary()
        {
            try
            {
                Token t = Current;

                if (t.Type == TokenType.Number)
                {
                    Consume();
                    int val = int.Parse(t.Text);
                    return new NumberNode(val);
                }

                if (t.Type == TokenType.Identifier)
                {
                    Consume();
                    return new IdentifierNode(t.Text);
                }

                if (t.Type == TokenType.LParen)
                {
                    Consume();
                    Node expr = ParseExpression();
                    Expect(TokenType.RParen);
                    return expr;
                }

                if (t.Type == TokenType.KeywordInput)
                {
                    Consume();                    // input
                    Expect(TokenType.LParen);     // (
                    Expect(TokenType.RParen);     // )
                    return new InputCallNode();   // AST node
                }

                throw new Exception("Unexpected token: " + t.Type);
            }
            catch (Exception ex)
            {
                throw new Exception("Parser error: " + ex.Message);
            }
        }

    }
}
