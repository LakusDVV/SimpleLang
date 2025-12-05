// File: Parser.cs
// Target: C# 7.3

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleLang
{
    public abstract class Node { }

    public class NumberNode : Node
    {
        public double Value { get; private set; }
        public NumberNode(double value) { Value = value; }
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

    public class LetNode : Node
    {
        public string Name { get; }
        public Node Expr { get; }
        public LetNode(string name, Node expr) { Name = name; Expr = expr; }
    }

    public class AssignNode : Node
    {
        public string Name { get; }
        public Node Expr { get; }
        public AssignNode(string name, Node expr) { Name = name; Expr = expr; }
    }

    public class OutputNode : Node
    {
        public Node Expr { get; }
        public OutputNode(Node expr) { Expr = expr; }
    }

    public class InputCallNode : Node { }

    public class IfNode : Node
    {
        public Node Condition { get; }
        public List<Node> Body { get; }
        public IfNode(Node condition, List<Node> body) { Condition = condition; Body = body; }
    }

    public class IfElseNode : Node
    {
        public Node Condition { get; }
        public List<Node> ThenBody { get; }
        public List<Node> ElseBody { get; }
        public IfElseNode(Node condition, List<Node> thenBody, List<Node> elseBody)
        {
            Condition = condition;
            ThenBody = thenBody;
            ElseBody = elseBody;
        }
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public Parser(List<Token> tokens) { _tokens = tokens; _pos = 0; }

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : new Token(TokenType.EndOfLine, "");

        private Token Consume() { Token t = Current; _pos++; return t; }

        private void Expect(TokenType type)
        {
            if (Current.Type != type) throw new Exception("Expected " + type + " but got " + Current.Type);
            _pos++;
        }

        // -------------------
        // Statements
        // -------------------
        public Node ParseStatement()
        {
            Token t = Current;
            switch (t.Type)
            {
                case TokenType.KeywordLet: return ParseLet();
                case TokenType.Identifier: return ParseAssignOrFunctionCall();
                case TokenType.KeywordIf: return ParseIf();
                case TokenType.KeywordOutput: return ParseOutput();
                default: throw new Exception("Unexpected token in statement: " + t.Type);
            }
        }

        private Node ParseLet()
        {
            Expect(TokenType.KeywordLet);
            string name = Current.Text;
            Expect(TokenType.Identifier);

            Node expr = null;
            if (Current.Type == TokenType.Assign)
            {
                Consume();
                expr = ParseExpression();
            }

            return new LetNode(name, expr);
        }

        private Node ParseAssignOrFunctionCall()
        {
            string name = Current.Text;
            Expect(TokenType.Identifier);

            if (Current.Type == TokenType.Assign)
            {
                Consume();
                Node expr = ParseExpression();
                return new AssignNode(name, expr);
            }

            throw new Exception("Unknown statement starting with identifier: " + name);
        }

        private Node ParseOutput()
        {
            Expect(TokenType.KeywordOutput);
            Expect(TokenType.LParen);
            Node expr = ParseExpression();
            Expect(TokenType.RParen);
            return new OutputNode(expr);
        }

        private Node ParseIf()
        {
            Expect(TokenType.KeywordIf);
            Expect(TokenType.LParen);
            Node condition = ParseExpression();
            Expect(TokenType.RParen);
            Expect(TokenType.LBrace);

            List<Node> thenBody = new List<Node>();
            while (Current.Type != TokenType.RBrace && Current.Type != TokenType.EndOfLine)
                thenBody.Add(ParseStatement());
            Expect(TokenType.RBrace);

            List<Node> elseBody = null;
            if (Current.Type == TokenType.KeywordElse)
            {
                Consume(); // пропускаем else
                Expect(TokenType.LBrace);
                elseBody = new List<Node>();
                while (Current.Type != TokenType.RBrace && Current.Type != TokenType.EndOfLine)
                    elseBody.Add(ParseStatement());
                Expect(TokenType.RBrace);
            }

            if (elseBody != null)
                return new IfElseNode(condition, thenBody, elseBody);
            else
                return new IfNode(condition, thenBody);
        }

        // -------------------
        // Expressions
        // -------------------
        public Node ParseExpression() => ParseComparison();

        private Node ParseComparison()
        {
            Node left = ParseAddSub();
            while (Current.Type == TokenType.EqualEqual ||
                   Current.Type == TokenType.NotEqual ||
                   Current.Type == TokenType.Greater ||
                   Current.Type == TokenType.Less ||
                   Current.Type == TokenType.GreaterOrEqual ||
                   Current.Type == TokenType.LessOrEqual)
            {
                Token op = Consume();
                Node right = ParseAddSub();
                left = new BinaryNode(left, right, op.Type);
            }
            return left;
        }

        private Node ParseAddSub()
        {
            Node left = ParseMulDivMod();
            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                Token op = Consume();
                Node right = ParseMulDivMod();
                left = new BinaryNode(left, right, op.Type);
            }
            return left;
        }

        private Node ParseMulDivMod()
        {
            Node left = ParsePower();
            while (Current.Type == TokenType.Multiply ||
                   Current.Type == TokenType.Divide ||
                   Current.Type == TokenType.Remainder)
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
            Token t = Current;
            switch (t.Type)
            {
                case TokenType.Number:
                    Consume();
                    return new NumberNode(double.Parse(t.Text, CultureInfo.InvariantCulture));
                case TokenType.Identifier:
                    Consume();
                    return new IdentifierNode(t.Text);
                case TokenType.KeywordInput:
                    Consume();
                    Expect(TokenType.LParen);
                    Expect(TokenType.RParen);
                    return new InputCallNode();
                case TokenType.LParen:
                    Consume();
                    Node expr = ParseExpression();
                    Expect(TokenType.RParen);
                    return expr;
                default:
                    throw new Exception("Unexpected token in expression: " + t.Type);
            }
        }
    }
}
