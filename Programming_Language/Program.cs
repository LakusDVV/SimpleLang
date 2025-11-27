using System;
using System.IO;
using System.Text;

namespace SimpleLang
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Поддержка UTF-8 для консоли
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch
            {
                // В некоторых средах изменение кодировки может быть недоступно — молча проигнорируем
            }

            string filePath = null;

            // Получаем путь к корневой папке проекта (где исполняемый файл)
            string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

            // Список файлов в корне проекта с расширением .txt
            string[] rootFiles = Directory.GetFiles(projectRoot, "*.txt");

            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Enter path to source file manually");
            if (rootFiles.Length > 0)
                Console.WriteLine("2. Use a file from project root folder");
            Console.WriteLine("Press Enter to run demo");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.WriteLine("Enter full path to the source file:");
                filePath = Console.ReadLine();
            }
            else if (choice == "2" && rootFiles.Length > 0)
            {
                Console.WriteLine("Available files in project root:");
                for (int i = 0; i < rootFiles.Length; i++)
                    Console.WriteLine($"{i + 1}: {Path.GetFileName(rootFiles[i])}");

                Console.WriteLine("Enter the number of the file to use:");
                string fileChoice = Console.ReadLine();
                if (int.TryParse(fileChoice, out int index) && index >= 1 && index <= rootFiles.Length)
                    filePath = rootFiles[index - 1];
            }
            else
            {
                // Demo mode
                filePath = null;
            }


            Interpreter interpreter = new Interpreter();

            if (filePath == null)
            {
                // Демонстрация встроенного примера программы
                Console.WriteLine("Running built-in demo program...\n");
                string[] demo =
                {
                "let x = 2 + 3 * (4 - 1)",
                "let y = x ^ 2",
                "let z = y + 10 / 2",
                "let a",
                "output(z)",
                "a = input()",
                "output(a + 5)"
                };


                foreach(var line in demo){
                    try
                    {
                        interpreter.ExecuteLine(line);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Demo error: " + ex.Message);
                    }
                }

                Console.WriteLine("\nDemo finished. Variables:");
                interpreter.PrintVariables();
                return;
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            try
            {
                // Читаем файл в UTF-8 и выполняем построчно
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        try
                        {
                            interpreter.ExecuteLine(line);
                        }
                        catch (Exception exLine)
                        {
                            Console.WriteLine(string.Format("Error at line {0}: {1}", lineNumber, exLine.Message));
                        }
                    }
                }

                Console.WriteLine("\nExecution finished. Variables:");
                interpreter.PrintVariables();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex.Message);
            }
        }
    }
}