using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace CS_4000_Compiler
{
    public static class Compiler
    {
        public static SymbolTable symbolTable;
        public static QuadTable quadTable;
        public static LexicalAnalyzer lexicalAnalyzer;

        public static bool Compile(string program, string name)
        {
            try
            {
                symbolTable = new SymbolTable();
                quadTable = new QuadTable();
                lexicalAnalyzer = new LexicalAnalyzer(program, symbolTable);

                if (SyntaxSemanticAnalyzer.Analyze(lexicalAnalyzer, symbolTable, quadTable))
                {
                    Console.WriteLine("\tCompilation Completed Successfully!");
                    File.WriteAllText(name + ".ALT", symbolTable.GetExecutableOutput() + "---" + quadTable.GetExecutableOutput());
                    return true;
                }
                else
                {
                    Console.WriteLine("\tCompile Error!");
                    return false;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("\tError: \"" + exc.Message + "\" Occurred!");
                return false;
            }
        }
    }
}
