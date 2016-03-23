using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CS_4000_Compiler
{
    class Program
    {
        public static bool DEBUG = false;

        static void Main(string[] args)
        {
            char menuSelection = ' ';

            Console.WriteLine("O--------------------------------------------O");
            Console.WriteLine("|                                            |");
            Console.WriteLine("|              CS4000 Compiler               |");
            Console.WriteLine("|             By: Bryant Nelson              |");
            Console.WriteLine("|                                            |");
            Console.WriteLine("O--------------------------------------------O\n");

            while (menuSelection != 'e' && menuSelection != 'x' && menuSelection != '3')
            {

                Console.WriteLine("Please select from the following menu:");
                Console.WriteLine("\t1. Compile Program");
                Console.WriteLine("\t2. Run Program");
                Console.WriteLine("\t3. Exit");
                Console.Write("\t:");
                menuSelection = getCharInput("123crex", "\t");
                Console.WriteLine();
                switch (menuSelection)
                {
                    case ('1'):
                    case ('c'):
                        string program = getValidFileInput("\tEnter the file name of your program: ", "\t");
                        string name = getStringInput("\tEnter the name of your program: ");
                        Console.WriteLine();
                        Compiler.Compile(program, name);
                        Console.WriteLine();
                        break;
                    case ('2'):
                    case ('r'):
                        name = getStringInput("\tEnter the name of your program: ");
                        string input = getStringInput("\tEnter the Input Source, for keyboard leave blank: ");
                        string output = getStringInput("\tEnter the Output Destination, for screen leave blank: ");
                        Console.WriteLine();
                        Interpreter.Interpret(name + ".ALT", input, output);
                        Console.WriteLine();
                        break;
                    case ('3'):
                    case ('e'):
                    case ('x'):
                        break;
                }
            }
        }

        public static char getCharInput(string acceptableInput, string pad)
        {
            string input = Console.ReadLine();
            input = input.Trim().ToLower();
            while (input.Length == 0 || !acceptableInput.Contains(input[0]))
            {
                Console.Write(pad + "'" + input + "' is invalid, try again:");
                input = Console.ReadLine();
                input = input.Trim().ToLower();
            }
            return input[0];
        }

        public static string getStringInput(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            return input;
        }

        public static string getValidFileInput(string prompt, string pad)
        {
            string input = getStringInput(prompt);
            while (!File.Exists(input))
            {
                input = getStringInput(pad + "File '" + input + "' doesn't exist, try again: ");
            }
            return input;
        }
    }
}
