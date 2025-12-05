using System;
using System.Collections.Generic;
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

            // Получаем путь к корневой папке проекта
            string projectRoot = AppDomain.CurrentDomain.BaseDirectory;

            // Список файлов в корне проекта с расширением .txt
            string[] rootFiles = Directory.GetFiles(projectRoot, "*.txt", SearchOption.AllDirectories);


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
                    "let x = 3.14",
                    "if(x-1 >= 3){",
                    "   x = x + 0.06",
                    "}",                  
                    "output(x)",
                };

                interpreter.ExecuteLines(new List<string>(demo));

                Console.WriteLine("\nDemo finished. Variables:");
                interpreter.PrintVariables();
                Console.ReadKey();
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
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                interpreter.ExecuteLines(new List<string>(lines));

                Console.WriteLine("\nExecution finished. Variables:");
                interpreter.PrintVariables();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex.Message);
            }
        }
    }
}
