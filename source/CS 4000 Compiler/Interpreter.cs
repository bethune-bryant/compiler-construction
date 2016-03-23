using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace CS_4000_Compiler
{
    public static class Interpreter
    {
        private static List<long> memory;
        private static QuadTable quadTable;
        private static SymbolTable symbolTable;
        private static string inputData = "";
        private static string outputData = "";
        private static bool halt;
        private static int programCounter = 0;
        private static bool keyBoardInput = false;
        private static bool screenOutput = false;


        public static void Interpret(string program, string input, string output)
        {
            try
            {
                program = File.ReadAllText(program);
                //Console.WriteLine(program);
                List<string> executable = new List<string>(program.Split(new string[] { "---" }, StringSplitOptions.RemoveEmptyEntries));

                quadTable = new QuadTable(executable[1]);
                symbolTable = new SymbolTable(executable[0]);
            }
            catch
            {
                Console.WriteLine("\tInvalid program file!");
                return;
            }

            try
            {
                if (input.Trim().Length != 0)
                {
                    keyBoardInput = false;
                    inputData = File.ReadAllText(input);
                }
                else
                {
                    keyBoardInput = true;
                }
            }
            catch
            {
                Console.WriteLine("\tInput file does not exist.");
                return;
            }

            if (output.Trim().Length != 0)
            {
                screenOutput = false;
            }
            else
            {
                screenOutput = true;
            }

            //run();

            try
            {
                run();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                if (output.Trim().Length != 0)
                {
                    File.WriteAllText(output.Trim(), outputData);
                }
            }
            catch
            {
                Console.WriteLine("Could not write to output file!");
            }
            //string memoryString = "";
            //foreach (long value in memory)
            //{
            //    memoryString += value + Environment.NewLine;
            //}
            //Console.WriteLine("Writing the Memory to the file \"memory.txt\"");
            //File.WriteAllText("memory.txt", memoryString);

        }

        private static void run()
        {
            programCounter = -1;
            halt = false;
            memory = new List<long>();
            QuadTable.Operation opCode;
            int operand1;
            int operand2;
            int result;

            while (!halt)
            {
                programCounter++;

                opCode = getOpCode(programCounter);
                operand1 = getOperand1(programCounter);
                operand2 = getOperand2(programCounter);
                result = getResult(programCounter);

                switch (opCode)
                {
                    case(QuadTable.Operation.ALLOCATE):
                        Allocate(operand1, operand2);
                        break;
                    case(QuadTable.Operation.ASSIGNMENT):
                        Assignment(operand1, result);
                        break;
                    case(QuadTable.Operation.ADDITION):
                        Addition(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.SUBTRACTION):
                        Subtraction(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.MULTIPLICATION):
                        Multiplication(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.DIVISION):
                        Division(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.EXPONENTIATION):
                        Exponentiation(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.LESS_THAN):
                        LessThan(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.GREATER_THAN):
                        GreaterThan(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.EQUAL_TO):
                        EqualTo(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.OR):
                        Or(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.AND):
                        And(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.NOT):
                        Not(operand1, result);
                        break;
                    case (QuadTable.Operation.JUMP_TRUE):
                        JumpTrue(operand1, operand2);
                        break;
                    case (QuadTable.Operation.JUMP_FALSE):
                        JumpFalse(operand1, operand2);
                        break;
                    case (QuadTable.Operation.JUMP):
                        Jump(operand2);
                        break;
                    case(QuadTable.Operation.READ):
                        Read(result);
                        break;
                    case (QuadTable.Operation.READ_LINE):
                        ReadLine(result);
                        break;
                    case (QuadTable.Operation.WRITE):
                        Write(operand1);
                        break;
                    case (QuadTable.Operation.WRITE_LINE):
                        WriteLine(operand1);
                        break;
                    case (QuadTable.Operation.OFFSET):
                        Offset(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.UNCHECKED_OFFSET):
                        UncheckedOffset(operand1, operand2, result);
                        break;
                    case (QuadTable.Operation.SWAP):
                        Swap(operand1, operand2);
                        break;
                    case(QuadTable.Operation.EXIT):
                        Console.WriteLine();
                        Console.WriteLine("Program Completed!\n\n");
                        halt = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private static QuadTable.Operation getOpCode(int quadTableIndex)
        {
            return quadTable.quads[quadTableIndex].operation;
        }

        private static int getOperand1(int quadTableIndex)
        {
            return quadTable.quads[quadTableIndex].operand1;
        }

        private static int getOperand2(int quadTableIndex)
        {
            return quadTable.quads[quadTableIndex].operand2;
        }

        private static int getResult(int quadTableIndex)
        {
            return quadTable.quads[quadTableIndex].result;
        }

        private static long getValue(int symbolTableIndex)
        {
            if (symbolTable[symbolTableIndex].Type == SymbolTable.TableType.CONSTANT)
            {
                return long.Parse(symbolTable[symbolTableIndex].Name.Trim());
            }
            else
            {
                return memory[symbolTable[symbolTableIndex].Location];
            }
        }

        private static void Allocate(int operand, int value)
        {
            symbolTable[operand].Location = memory.Count;
            int totalSize = 1;
            foreach (int size in symbolTable[operand].Size)
            {
                totalSize *= size;
            }
            for (int i = 0; i < totalSize; i++)
            {
                memory.Add(value);
            }
        }

        private static void Assignment(int operand, int result)
        {
            if (symbolTable[operand].Size.Count != 0)
            {
                int size = 1;
                foreach (int i in symbolTable[operand].Size)
                {
                    size *= i;
                }
                for (int i = 0; i < size; i++)
                {
                    memory[symbolTable[result].Location + i] = memory[symbolTable[operand].Location + i];
                }
            }
            else
            {
                memory[symbolTable[result].Location] = getValue(operand);
            }
        }

        private static void Addition(int operand1, int operand2, int result)
        {
            memory[symbolTable[result].Location] = getValue(operand1) + getValue(operand2);
        }

        private static void Subtraction(int operand1, int operand2, int result)
        {
            memory[symbolTable[result].Location] = getValue(operand1) - getValue(operand2);
        }

        private static void Multiplication(int operand1, int operand2, int result)
        {
            memory[symbolTable[result].Location] = getValue(operand1) * getValue(operand2);
        }

        private static void Division(int operand1, int operand2, int result)
        {
            if (getValue(operand2) == 0)
            {
                throw new DivideByZeroException("Runtime Error: ZERO DIVISION ERROR");
            }
            memory[symbolTable[result].Location] = getValue(operand1) / getValue(operand2);
        }

        private static void Exponentiation(int operand1, int operand2, int result)
        {
            if (getValue(operand2) < 0)
            {
                throw new DivideByZeroException("Runtime Error: 0 RAISED TO A NEGATIVE POWER!");
            }
            else
            {
                memory[symbolTable[result].Location] = (long)Math.Pow(getValue(operand1), getValue(operand2));
            }
        }

        private static void LessThan(int operand1, int operand2, int result)
        {
            if (getValue(operand1) < getValue(operand2))
            {
                memory[symbolTable[result].Location] = 1;
            }
            else
            {
                memory[symbolTable[result].Location] = 0;
            }
        }

        private static void GreaterThan(int operand1, int operand2, int result)
        {
            if (getValue(operand1) > getValue(operand2))
            {
                memory[symbolTable[result].Location] = 1;
            }
            else
            {
                memory[symbolTable[result].Location] = 0;
            }
        }

        private static void EqualTo(int operand1, int operand2, int result)
        {
            if (getValue(operand1) == getValue(operand2))
            {
                memory[symbolTable[result].Location] = 1;
            }
            else
            {
                memory[symbolTable[result].Location] = 0;
            }
        }

        private static void Or(int operand1, int operand2, int result)
        {
            if (getValue(operand1) == 1 || getValue(operand2) == 1)
            {
                memory[symbolTable[result].Location] = 1;
            }
            else
            {
                memory[symbolTable[result].Location] = 0;
            }
        }

        private static void And(int operand1, int operand2, int result)
        {
            if (getValue(operand1) == 1 && getValue(operand2) == 1)
            {
                memory[symbolTable[result].Location] = 1;
            }
            else
            {
                memory[symbolTable[result].Location] = 0;
            }
        }

        private static void Not(int operand, int result)
        {
            if (getValue(operand) == 0)
            {
                memory[symbolTable[result].Location] = 1;
            }
            else
            {
                memory[symbolTable[result].Location] = 0;
            }
        }
        private static void JumpTrue(int operand1, int operand2)
        {
            if (getValue(operand1) != 0)
            {
                programCounter = operand2 - 1;
            }
        }

        private static void JumpFalse(int operand1, int operand2)
        {
            if (getValue(operand1) == 0)
            {
                programCounter = operand2 - 1;
            }
        }

        private static void Jump(int operand)
        {
            programCounter = operand - 1;
        }

        private static void Read(int result)
        {
            Console.Write("Awaiting input: ");

            try
            {
                if (keyBoardInput)
                {
                    long value = long.Parse(Console.ReadLine().Trim());
                    memory[symbolTable[result].Location] = value;
                }
                else
                {
                    long value = long.Parse(inputData.Split()[0]);
                    inputData = inputData.Substring(inputData.IndexOf(value.ToString()) + value.ToString().Length).Trim();
                    Console.WriteLine(value);
                    memory[symbolTable[result].Location] = value;
                }
            }
            catch
            {
                throw new Exception("Runtime Error: Input file formatted incorrectly.");
            }
        }

        private static void ReadLine(int result)
        {
            Console.Write("Awaiting input: ");
            try
            {
                if (keyBoardInput)
                {
                    long value = long.Parse(Console.ReadLine().Trim());
                    memory[symbolTable[result].Location] = value;
                }
                else
                {
                    long value = long.Parse(inputData.Split()[0]);
                    Console.WriteLine(value);
                    inputData = inputData.Substring(inputData.IndexOf('\n') + 1);
                    memory[symbolTable[result].Location] = value;
                }
            }
            catch
            {
                throw new Exception("Runtime Error: Input file formatted incorrectly.");
            }
        }

        private static void Write(int operand)
        {
            if (screenOutput)
            {
                Console.Write(getValue(operand).ToString() + " ");
            }
            else
            {
                outputData += getValue(operand).ToString() + " ";
            }
        }

        private static void WriteLine(int operand)
        {
            if (screenOutput)
            {
                Console.Write(getValue(operand).ToString());
                Console.WriteLine();
            }
            else
            {
                outputData += getValue(operand).ToString() + Environment.NewLine;
            }
        }

        private static void Offset(int operand1, int operand2, int result)
        {
            int size = (int)getValue(operand2);

            if (getValue(operand2) >= symbolTable[operand1].Size[0] || getValue(operand2) < 0)
            {
                throw new Exception("Runtime Error: Array index out of range.");
            }

            foreach (int i in symbolTable[operand1].Size.GetRange(1, symbolTable[operand1].Size.Count - 1))
            {
                size *= i;
            }

            symbolTable[result].Location = (int)(symbolTable[operand1].Location + size);
        }

        private static void UncheckedOffset(int operand1, int operand2, int result)
        {
            int size = (int)getValue(operand2);
            if (symbolTable[operand1].Size.Count > 0)
            {
                foreach (int i in symbolTable[operand1].Size.GetRange(1, symbolTable[operand1].Size.Count - 1))
                {
                    size *= i;
                }
            }
            symbolTable[result].Location = (int)(symbolTable[operand1].Location + size);
        }

        private static void Swap(int operand1, int operand2)
        {
            int tempLocation = symbolTable[operand1].Location;
            List<int> tempSize = new List<int>(symbolTable[operand1].Size);
            int tempTokenType = symbolTable[operand1].TokenType;
            SymbolTable.TableType tempType = symbolTable[operand1].Type;
            bool tempIsDeclared = symbolTable[operand1].IsDeclared;

            symbolTable[operand1].IsDeclared = symbolTable[operand2].IsDeclared;
            symbolTable[operand1].Location = symbolTable[operand2].Location;
            symbolTable[operand1].Size = new List<int>(symbolTable[operand2].Size);
            symbolTable[operand1].TokenType = symbolTable[operand2].TokenType;
            symbolTable[operand1].Type = symbolTable[operand2].Type;

            symbolTable[operand2].IsDeclared = tempIsDeclared;
            symbolTable[operand2].Location = tempLocation;
            symbolTable[operand2].Size = new List<int>(tempSize);
            symbolTable[operand2].TokenType = tempTokenType;
            symbolTable[operand2].Type = tempType;
        }
    }
}
