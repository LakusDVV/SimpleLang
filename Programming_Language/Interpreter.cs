// File: Interpreter.cs
// Target: C# 7.3

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleLang
{
    public class Interpreter
    {
        private readonly KeyValueStorage<string, double> _vars =
            new KeyValueStorage<string, double>();

        public void ExecuteLines(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                List<string> blockLines = new List<string> { line };

                // Проверка на if или любой блок
                if (line.StartsWith("if") && line.EndsWith("{"))
                {
                    int openBraces = 1;

                    // Собираем все строки до закрывающей скобки
                    while (openBraces > 0 && i + 1 < lines.Count)
                    {
                        i++;
                        string nextLine = lines[i].Trim();
                        if (nextLine.Contains("{")) openBraces++;
                        if (nextLine.Contains("}")) openBraces--;

                        blockLines.Add(nextLine);
                    }
                }

                // Объединяем блок в одну строку для Lexer
                string combined = string.Join(" ", blockLines);

                try
                {
                    Lexer lexer = new Lexer(combined);
                    List<Token> tokens = lexer.Tokenize();
                    if (tokens.Count == 0) continue;

                    Parser parser = new Parser(tokens);
                    Node stmt = parser.ParseStatement(); // ParseStatement теперь умеет блоки
                    Execute(stmt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Demo error at '{line}': {ex.Message}");
                }
            }
        }


        private void Execute(Node node)
        {
            switch (node)
            {
                case LetNode letNode:
                    double letValue = letNode.Expr != null ? Evaluate(letNode.Expr) : 0;
                    _vars.Add(letNode.Name, letValue);
                    break;

                case AssignNode assignNode:
                    double assignValue = Evaluate(assignNode.Expr);
                    _vars.Update(assignNode.Name, assignValue);
                    break;

                case OutputNode outputNode:
                    Console.WriteLine(Evaluate(outputNode.Expr));
                    break;

                case InputCallNode inputNode:
                    Evaluate(inputNode); // просто возвращаем значение при вызове input()
                    break;

                case IfNode ifNode:
                    if (EvaluateCondition(ifNode.Condition))
                    {
                        foreach (var stmt in ifNode.Body)
                            Execute(stmt);
                    }
                    break;

                case IfElseNode ifElseNode:
                    if (EvaluateCondition(ifElseNode.Condition))
                    {
                        foreach (var stmt in ifElseNode.ThenBody)
                            Execute(stmt);
                    }
                    else
                    {
                        foreach (var stmt in ifElseNode.ElseBody)
                            Execute(stmt);
                    }
                    break;

                default:
                    // выражения без присваивания
                    Evaluate(node);
                    break;
                    
                
            }
        }



        private bool EvaluateCondition(Node cond)
        {
            double result = Evaluate(cond);
            return result != 0; // true если != 0
        }

        private double Evaluate(Node node)
        {
            if (node is NumberNode n)
                return n.Value;

            if (node is IdentifierNode id)
                return _vars.Get(id.Name);

            if (node is InputCallNode)
            {
                Console.Write("input: ");
                string raw = Console.ReadLine();
                double val;
                if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                    throw new Exception("Input must be a number.");
                return val;
            }

            if (node is BinaryNode b)
            {
                double left = Evaluate(b.Left);
                double right = Evaluate(b.Right);

                switch (b.Op)
                {
                    case TokenType.Plus: return left + right;
                    case TokenType.Minus: return left - right;
                    case TokenType.Multiply: return left * right;
                    case TokenType.Divide:
                        if (right == 0.0) throw new Exception("Division by zero");
                        return left / right;
                    case TokenType.Power: return Math.Pow(left, right);
                    case TokenType.Remainder: return left % right;

                    // логические операторы
                    case TokenType.EqualEqual: return left == right ? 1.0 : 0.0;
                    case TokenType.NotEqual: return left != right ? 1.0 : 0.0;
                    case TokenType.Greater: return left > right ? 1.0 : 0.0;
                    case TokenType.Less: return left < right ? 1.0 : 0.0;
                    case TokenType.GreaterOrEqual: return left >= right ? 1.0 : 0.0;
                    case TokenType.LessOrEqual: return left <= right ? 1.0 : 0.0;
                }
            }

            throw new Exception("Invalid expression node: " + node.GetType());
        }

        public void PrintVariables()
        {
            _vars.PrintAll();
        }
    }
}
