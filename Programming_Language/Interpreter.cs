// File: Interpreter.cs
// Target: C# 7.3

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SimpleLang
{
    public class Interpreter
    {
        private readonly KeyValueStorage<string, int> _vars =
            new KeyValueStorage<string, int>();

        public void ExecuteLine(string line)
        {
            Lexer lexer = new Lexer(line);
            List<Token> tokens = lexer.Tokenize();
            if (tokens.Count == 0)
                return;

            

            if (tokens[0].Type == TokenType.KeywordLet)
            {
                ExecuteLetDeclaration(tokens);
                return;
            }

            if (tokens[0].Type == TokenType.Identifier && LookForAssign(tokens))
            {
                ExecuteAssignment(tokens);
                return;
            }

            if (tokens[0].Type == TokenType.KeywordOutput)
            {
                ExecuteOutput(tokens);
                return;
            }

            // Just an expression? Ignore.
        }

        private bool LookForAssign(List<Token> tokens)
        {
            if (tokens.Count < 3)
                return false;

            return tokens[1].Type == TokenType.Assign;
        }

        private void ExecuteLetDeclaration(List<Token> tokens)
        {
            // let x = expr   OR   let x
            string name = tokens[1].Text;

            if (_vars.ContainsKey(name))
                throw new Exception("Variable '" + name + "' already declared.");

            if (tokens.Count > 3 && tokens[2].Type == TokenType.Assign)
            {
                Parser parser = new Parser(tokens.GetRange(3, tokens.Count - 3));
                Node expr = parser.ParseExpression();

                int value = Evaluate(expr);
                _vars.Add(name, value);
            }
            else
            {
                _vars.Add(name, 0);
            }
        }        

        private void ExecuteAssignment(List<Token> tokens)
        {
            // x = expr
            string name = tokens[0].Text;

            if (!_vars.ContainsKey(name))
                throw new Exception("Variable '" + name + "' not declared.");

            Parser parser = new Parser(tokens.GetRange(2, tokens.Count - 2));
            Node expr = parser.ParseExpression();
            int value = Evaluate(expr);

            _vars.Update(name, value);
        }

        private void ExecuteOutput(List<Token> tokens)
        {
            // output(expr)
            // 0: output
            // 1: '('
            // ...
            int start = 2; // after "("
            int end = tokens.Count - 2; // before ")"

            List<Token> inner = tokens.GetRange(start, end - start + 1);
            Parser parser = new Parser(inner);
            Node expr = parser.ParseExpression();

            int value = Evaluate(expr);
            Console.WriteLine(value);
        }

        private int Evaluate(Node node)
        {
            if (node is InputCallNode)
            {
                Console.Write("input: ");
                string raw = Console.ReadLine();

                int value;
                if (!int.TryParse(raw, out value))
                    throw new Exception("Input must be an integer.");

                return value;
            }


            if (node is NumberNode)
            {
                return ((NumberNode)node).Value;
            }

            if (node is IdentifierNode)
            {
                string name = ((IdentifierNode)node).Name;
                return _vars.Get(name);
            }

            if (node is BinaryNode)
            {
                BinaryNode b = (BinaryNode)node;
                int left = Evaluate(b.Left);
                int right = Evaluate(b.Right);

                switch (b.Op)
                {
                    case TokenType.Plus: return left + right;
                    case TokenType.Minus: return left - right;
                    case TokenType.Multiply: return left * right;
                    case TokenType.Divide: return left / right;
                    case TokenType.Power: return Pow(left, right);
                }
            }

            throw new Exception("Invalid expression node.");
        }

        private int Pow(int a, int b)
        {
            int r = 1;
            for (int i = 0; i < b; i++)
                r *= a;
            return r;
        }

        public void PrintVariables()
        {
            _vars.PrintAll();
        }
    }
}
