using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CS_4000_Compiler
{
    /// <summary>
    /// A static class used for Syntax/Semantic analysis.
    /// </summary>
    static class SyntaxSemanticAnalyzer
    {
        public static LexicalAnalyzer lexicalAnalyzer;
        public static SymbolTable symbolTable;
        public static QuadTable quadTable;
        public static int nextToken;
        public static Stack<int> semanitcActionStack;
        public static List<int> locallyDefined = new List<int>(); //HERE!!! Making a list of all currently declared variables.

        public static Stack<string> errorStack = new Stack<string>();

        public static bool Analyze(LexicalAnalyzer la, SymbolTable st, QuadTable qt)
        {
            lexicalAnalyzer = la;
            symbolTable = st;
            quadTable = qt;
            semanitcActionStack = new Stack<int>();
            errorStack = new Stack<string>();
            locallyDefined = new List<int>();
            LEX();
            bool retval = false;

            try
            {
                retval = P();
            }
            catch(Exception exc)
            {
                errorStack.Push(exc.Message);
            }

            if (Program.DEBUG)
            {
                foreach (string error in errorStack)
                {
                    Console.WriteLine(error);
                }
            }
            else
            {
                if (!retval)
                {
                    //Console.WriteLine(errorStack.ToArray()[errorStack.Count - 1]);
                    Console.WriteLine("\t" + errorStack.ToArray()[0]);
                }
                else
                {
                    //Console.WriteLine("Writing the Symbol Table to the file \"SymbolTableOutput.txt\"");
                    //File.WriteAllText("SymbolTableOutput.txt", symbolTable.ToString());

                    //Console.WriteLine("Writing the Quad Table to the file \"QuadTableOutput.txt\"");
                    //File.WriteAllText("QuadTableOutput.txt", quadTable.ToString());
                }
            }

            return retval;
        }

        public static bool P()
        {
            if (checkNextToken("program"))
            {
                semanitcActionStack.Push(-1);
                locallyDefined = new List<int>();
                if (D())
                {
                    if (S())
                    {
                        if (checkNextToken("end"))
                        {
                            int id = semanitcActionStack.Pop();
                            while (id != -1)
                            {
                                int temp = semanitcActionStack.Pop();

                                List<int> idSize = new List<int>(symbolTable[id].Size);
                                bool idIsDeclared = symbolTable[id].IsDeclared;
                                SymbolTable.TableType idType = symbolTable[id].Type;

                                symbolTable[id].Size = new List<int>(symbolTable[temp].Size);
                                symbolTable[id].IsDeclared = symbolTable[temp].IsDeclared;
                                symbolTable[id].Type = symbolTable[temp].Type;

                                symbolTable[temp].Size = new List<int>(idSize);
                                symbolTable[temp].IsDeclared = idIsDeclared;
                                symbolTable[temp].Type = idType;

                                quadTable.GenQuad(QuadTable.Operation.SWAP, id, temp, 0);
                                id = semanitcActionStack.Pop();
                            }
                            if (checkNextToken("eof"))
                            {
                                quadTable.GenQuad(QuadTable.Operation.EXIT, 0, 0, 0);
                                return true;
                            }
                            else
                            {
                                errorStack.Push("Syntax Error: Invalid symbols found after 'end', 'EOF' expected! at line " + lexicalAnalyzer.LineNumber +" (CODE 000)");
                                return false;
                            }
                        }
                        else
                        {
                            errorStack.Push("Syntax Error: You Must End Your Program With 'end'! at line " + lexicalAnalyzer.LineNumber +" (CODE 001)");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: You Must Begin Your Program With 'program'! at line " + lexicalAnalyzer.LineNumber +" (CODE 002)");
                return false;
            }
        }

        public static bool D()
        {
            semanitcActionStack.Push(-1);
            if (IL())
            {
                List<int> idList = new List<int>();
                List<int> tempList = new List<int>();
                int id = semanitcActionStack.Pop();
                while (id != -1)
                {
                    if (locallyDefined.Contains(id))
                    {
                        errorStack.Push("Semantic Error: Variable '" + symbolTable[id].Name.Trim() + "' is already declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 050)");
                        throw new Exception(errorStack.ToArray()[0]);
                        return false;
                    }
                    int temp = symbolTable.GenTemp().index;
                    symbolTable[temp].Size = new List<int>(symbolTable[id].Size);
                    symbolTable[temp].IsDeclared = symbolTable[id].IsDeclared;
                    symbolTable[temp].Type = symbolTable[id].Type;
                    symbolTable[temp].Type = symbolTable[id].Type;
                    quadTable.GenQuad(QuadTable.Operation.SWAP, id, temp, 0);
                    idList.Add(id);
                    locallyDefined.Add(id);
                    tempList.Add(temp);
                    id = semanitcActionStack.Pop();
                }
                for (int i = 0; i < idList.Count; i++)
                {
                    semanitcActionStack.Push(tempList[i]);
                    semanitcActionStack.Push(idList[i]);
                }
                semanitcActionStack.Push(-1);
                for (int i = 0; i < idList.Count; i++)
                {
                    semanitcActionStack.Push(tempList[i]);
                    semanitcActionStack.Push(idList[i]);
                }
                if (D1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("begin"))
            {
                semanitcActionStack.Pop();
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting 'begin' to end data section! at line " + lexicalAnalyzer.LineNumber +" (CODE 003)");
                return false;
            }
        }

        public static bool D1()
        {
            if (checkNextToken("array"))
            {
                semanitcActionStack.Push(-1);
                if (checkNextToken("["))
                {
                    if (CL())
                    {
                        int constant = semanitcActionStack.Pop();
                        Stack<int> size = new Stack<int>();
                        while (constant != -1)
                        {
                            if (Int32.Parse(symbolTable[constant].Name) == 0)
                            {
                                errorStack.Push("Semantic Error: Cannot declare array of size 0! Line " + lexicalAnalyzer.LineNumber + " (CODE 062)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            size.Push(Int32.Parse(symbolTable[constant].Name));
                            constant = semanitcActionStack.Pop();
                        }
                        int array = semanitcActionStack.Pop();
                        while (array != -1)
                        {
                            int temp = semanitcActionStack.Pop();
                            //if (symbolTable[array].IsDeclared == true)
                            //{
                            //    errorStack.Push("Semantic Error: Variable '" + symbolTable[array].Name.Trim() + "' is already declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 050)");
                            //    throw new Exception(errorStack.ToArray()[0]);
                            //    return false;
                            //}
                            //symbolTable[temp].IsDeclared = true;
                            //symbolTable[temp].Type = SymbolTable.TableType.ARRAY;
                            //symbolTable[temp].Size = new List<int>(size);
                            symbolTable[array].IsDeclared = true;
                            symbolTable[array].Type = SymbolTable.TableType.ARRAY;
                            symbolTable[array].Size = new List<int>(size);
                            quadTable.GenQuad(QuadTable.Operation.ALLOCATE, array, 0, 0);
                            array = semanitcActionStack.Pop();
                        }
                        if (D())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting a '[' to define array size at line " + lexicalAnalyzer.LineNumber +" (CODE 004)");
                    return false;
                }
            }
            else if (checkNextToken("integer"))
            {
                #region Semantics

                int identifier = semanitcActionStack.Pop();
                while (identifier != -1)
                {
                    int temp = semanitcActionStack.Pop();
                    //if (symbolTable[identifier].IsDeclared == true)
                    //{
                    //    errorStack.Push("Semantic Error: Variable '" + symbolTable[identifier].Name.Trim() + "' is already declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 051)");
                    //    throw new Exception(errorStack.ToArray()[0]);
                    //    return false;
                    //}
                    //symbolTable[temp].IsDeclared = true;
                    //symbolTable[temp].Type = SymbolTable.TableType.INTEGER;
                    //symbolTable[temp].Size = new List<int>(new int[] { });
                    symbolTable[identifier].IsDeclared = true;
                    symbolTable[identifier].Type = SymbolTable.TableType.INTEGER;
                    symbolTable[identifier].Size = new List<int>(new int[] {});
                    quadTable.GenQuad(QuadTable.Operation.ALLOCATE, identifier, 0, 0);
                    identifier = semanitcActionStack.Pop();
                }

                #endregion
                if (D())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a type, array or integer! at line " + lexicalAnalyzer.LineNumber +" (CODE 005)");
                return false;
            }
        }

        public static bool IL()
        {
            if (isNextTokenIdentifier())
            {
                if (IL1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a valid identifier, '" + nextTokenValue + "' is not a valid identifier! at line " + lexicalAnalyzer.LineNumber +" (CODE 006)");
                return false;
            }
        }

        public static bool IL1()
        {
            if (checkNextToken(","))
            {
                if (IL())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken(":"))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a ',' or ':', found '" + nextTokenValue + "'! at line " + lexicalAnalyzer.LineNumber +" (CODE 007)");
                return false;
            }
        }

        public static bool CL()
        {
            if (isNextTokenConstant())
            {
                if (CL1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a constant, found '" + nextTokenValue + "'! at line " + lexicalAnalyzer.LineNumber +" (CODE 008)");
                return false;
            }
        }

        public static bool CL1()
        {
            if (checkNextToken(","))
            {
                if (CL())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("]"))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a ',' or ']', found '" + nextTokenValue + "'! at line " + lexicalAnalyzer.LineNumber +" (CODE 009)");
                return false;
            }
        }

        public static bool ID()
        {
            if (isNextTokenIdentifier())
            {
                if (ID1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a valid identifier, '" + nextTokenValue + "' is not a valid identifier! at line " + lexicalAnalyzer.LineNumber +" (CODE 010)");
                return false;
            }
        }

        public static bool ID1()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { ":=", "+", "-", "*", "/", "^", ")", ",", "<", "=", ">", "]", "and", "or", ";", "end", "fi", "od", "esac", "do", "then", "else" });
            if (checkNextToken("["))
            {
                int identifier = semanitcActionStack.Pop();
                if (symbolTable[identifier].Size.Count == 0)
                {
                    errorStack.Push("Semantic Error: Can't index into an Integer! Line " + lexicalAnalyzer.LineNumber + " (CODE 063)");
                    throw new Exception(errorStack.ToArray()[0]);
                }
                semanitcActionStack.Push(identifier);
                if (EL())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting ';', an operator,  or closing statement, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 011)");
                return false;
            }
        }

        public static bool EL()
        {
            if (E())
            {
                #region Semantics
                int offset = semanitcActionStack.Pop();
                int original = semanitcActionStack.Pop();
                if (symbolTable[original].Size.Count == 0)
                {
                    errorStack.Push("Semantic Error: Can't index into that many deminsions! Line " + lexicalAnalyzer.LineNumber + " (CODE 064)");
                    throw new Exception(errorStack.ToArray()[0]);
                }
                int temp = symbolTable.GenTemp(new List<int>(symbolTable[original].Size.GetRange(1, symbolTable[original].Size.Count - 1))).index;
                quadTable.GenQuad(QuadTable.Operation.OFFSET, original, offset, temp);
                semanitcActionStack.Push(temp);
                #endregion
                if (EL1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool EL1()
        {
            if (checkNextToken(","))
            {
                if (EL())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("]"))
            {
                #region Semantics

                #endregion
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a ',' or ']', found '" + nextTokenValue + "'! at line " + lexicalAnalyzer.LineNumber +" (CODE 012)");
                return false;
            }
        }

        public static bool IDL()
        {
            if (ID())
            {
                if (IDL1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IDL1()
        {
            if (checkNextToken(","))
            {
                if (IDL())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken(")"))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting a ',' or ')', found '" + nextTokenValue + "'! at line " + lexicalAnalyzer.LineNumber +" (CODE 013)");
                return false;
            }
        }

        public static bool E()
        {
            if (T())
            {
                if (E1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool E1()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { ")", ",", "<", "=", ">", "]", "and", "or", ";", "end", "fi", "od", "esac", "do", "then", "else" });
            if (checkNextToken("+"))
            {
                if (T())
                {
                    semanticBinaryOperation(QuadTable.Operation.ADDITION);
                    
                    if (E1())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("-"))
            {
                if (T())
                {
                    semanticBinaryOperation(QuadTable.Operation.SUBTRACTION);
                    if (E1())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting ';', an operator or closing structure, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 014)");
                return false;
            }
        }

        public static bool T()
        {
            if (B())
            {
                if (T1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool T1()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { "+", "-", ")", ",", "<", "=", ">", "]", "and", "or", ";", "end", "fi", "od", "esac", "do", "then", "else" });
            if (checkNextToken("*"))
            {
                if (B())
                {
                    semanticBinaryOperation(QuadTable.Operation.MULTIPLICATION);
                    if (T1())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("/"))
            {
                if (B())
                {
                    semanticBinaryOperation(QuadTable.Operation.DIVISION);
                    if (T1())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting ';', an operator or closing statement, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 015)");
                return false;
            }
        }

        public static bool B()
        {
            if (F())
            {
                if (B1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool B1()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { "+", "-", "*", "/", ")", ",", "<", "=", ">", "]", "and", "or", ";", "end", "fi", "od", "esac", "do", "then", "else" });
            if (checkNextToken("^"))
            {
                if (F())
                {
                    if (B1())
                    {
                        semanticBinaryOperation(QuadTable.Operation.EXPONENTIATION);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting ';', an operator or closing statement, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber + " (CODE 015)");
                return false;
            }
        }

        public static bool F()
        {
            if (isNextTokenConstant())
            {
                return true;
            }
            else if (ID())
            {
                return true;
            }
            else if (checkNextToken("exp"))
            {
                if (checkNextToken("("))
                {
                    if (E())
                    {
                        if (checkNextToken(","))
                        {
                            if (E())
                            {
                                if (checkNextToken(")"))
                                {
                                    semanticBinaryOperation(QuadTable.Operation.EXPONENTIATION);
                                    return true;
                                }
                                else
                                {
                                    errorStack.Push("Syntax Error: Expecting a ')', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 016)");
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            errorStack.Push("Syntax Error: Expecting a ',', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 017)");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting a '(', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 018)");
                    return false;
                }
            }
            else if (checkNextToken("("))
            {
                if (E())
                {
                    if (checkNextToken(")"))
                    {
                        return true;
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting ')', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 019)");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting the start of an expression, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 020)");
                return false;
            }
        }

        public static bool C()
        {
            if (X())
            {
                if (C1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool C1()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { "do", "then", ")", "]" });
            if (checkNextToken("or"))
            {
                if (X())
                {
                    semanticBinaryOperation(QuadTable.Operation.OR);
                    if (C1())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting 'or' or a closing structure, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 021)");
                return false;
            }
        }

        public static bool X()
        {
            if (Y())
            {
                if (X1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool X1()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { "or", "do", "then", ")", "]" });
            if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else if (checkNextToken("and"))
            {
                if (X())
                {
                    semanticBinaryOperation(QuadTable.Operation.AND);
                    if (C1())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting 'and', 'or',  or a closing structure, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 022)");
                return false;
            }
        }

        public static bool Y()
        {
            if (E())
            {
                if (Y1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("not"))
            {
                if (checkNextToken("("))
                {
                    if (C())
                    {
                        if (checkNextToken(")"))
                        {
                            semanticUnaryOperation(QuadTable.Operation.NOT);
                            return true;
                        }
                        else
                        {
                            errorStack.Push("Syntax Error: Expecting closing ')' for 'not', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 024)");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting '(' after 'not', found'" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 023)");
                    return false;
                }
            }
            else if (checkNextToken("["))
            {
                if (C())
                {
                    if (checkNextToken("]"))
                    {
                        return true;
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting closing ']' for condition, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 025)");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting the start of a condition, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 026)");
                return false;
            }
        }

        public static bool Y1()
        {
            if (checkNextToken("<"))
            {
                if (E())
                {
                    semanticBinaryOperation(QuadTable.Operation.LESS_THAN);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken(">"))
            {
                if (E())
                {
                    semanticBinaryOperation(QuadTable.Operation.GREATER_THAN);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("="))
            {
                if (E())
                {
                    semanticBinaryOperation(QuadTable.Operation.EQUAL_TO);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting relational operator, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 027)");
                return false;
            }
        }

        public static bool S()
        {
            if (ID())
            {
                if (checkNextToken(":="))
                {
                    if (E())
                    {
                        #region Semantics
                        int x = semanitcActionStack.Pop();
                        int y = semanitcActionStack.Pop();
                        if (!(symbolTable[x].IsDeclared && symbolTable[y].IsDeclared))
                        {
                            errorStack.Push("Semantic Error: Variable cannot be referenced before it is declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 055)");
                            throw new Exception(errorStack.ToArray()[0]);
                            return false;
                        }
                        if (symbolTable[x].Size.Count != symbolTable[y].Size.Count)
                        {
                            errorStack.Push("Semantic Error: Cannot assign between two unlike types! Line " + lexicalAnalyzer.LineNumber + " (CODE 056)");
                            throw new Exception(errorStack.ToArray()[0]);
                            return false;
                        }
                        for (int i = 0; i < symbolTable[x].Size.Count; i++)
                        {
                            if (symbolTable[x].Size[i] != symbolTable[y].Size[i])
                            {
                                errorStack.Push("Semantic Error: Cannot assign between two unlike types! Line " + lexicalAnalyzer.LineNumber + " (CODE 057)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                        }
                        quadTable.GenQuad(QuadTable.Operation.ASSIGNMENT, x, 0, y);
                        #endregion
                        if (S1())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting ':=', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 034)");
                    return false;
                }
            }
            else if (checkNextToken("read"))
            {
                if (checkNextToken("("))
                {
                    semanitcActionStack.Push(-1);
                    if (IDL())
                    {
                        #region Semantics
                        Stack<int> idListStack = new Stack<int>();
                        int id = semanitcActionStack.Pop();
                        while (id != -1)
                        {
                            if (symbolTable[id].Size.Count != 0)
                            {
                                errorStack.Push("Semantic Error: Can only read integers, not arrays! Line " + lexicalAnalyzer.LineNumber + " (CODE 058)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            else if (!symbolTable[id].IsDeclared)
                            {
                                errorStack.Push("Semantic Error: Can't reference variables before they are declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 071)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            idListStack.Push(id);
                            id = semanitcActionStack.Pop();
                        }
                        while (idListStack.Count > 0)
                        {
                            id = idListStack.Pop();
                            quadTable.GenQuad(QuadTable.Operation.READ, 0, 0, id);
                        }
                        #endregion
                        if (S1())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting '(' for 'read', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 035)");
                    return false;
                }
            }
            else if (checkNextToken("write"))
            {
                if (checkNextToken("("))
                {
                    semanitcActionStack.Push(-1);
                    if (IDL())
                    {
                        #region Semantics
                        Stack<int> idListStack = new Stack<int>();
                        int id = semanitcActionStack.Pop();
                        while (id != -1)
                        {
                            if (symbolTable[id].Size.Count != 0)
                            {
                                errorStack.Push("Semantic Error: Can only write integers, not arrays! Line " + lexicalAnalyzer.LineNumber + " (CODE 059)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            else if (!symbolTable[id].IsDeclared)
                            {
                                errorStack.Push("Semantic Error: Can't reference variables before they are declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 070)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            idListStack.Push(id);
                            id = semanitcActionStack.Pop();
                        }
                        while (idListStack.Count > 0)
                        {
                            id = idListStack.Pop();
                            quadTable.GenQuad(QuadTable.Operation.WRITE, id, 0, 0);
                        }
                        #endregion
                        if (S1())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting '(' for 'write', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber + " (CODE 036)");
                    return false;
                }
            }
            else if (checkNextToken("readln"))
            {
                if (checkNextToken("("))
                {
                    semanitcActionStack.Push(-1);
                    if (IDL())
                    {
                        #region Semantics
                        Stack<int> idListStack = new Stack<int>();
                        int id = semanitcActionStack.Pop();
                        while (id != -1)
                        {
                            if (symbolTable[id].Size.Count != 0)
                            {
                                errorStack.Push("Semantic Error: Can only read integers, not arrays! Line " + lexicalAnalyzer.LineNumber + " (CODE 060)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            else if (!symbolTable[id].IsDeclared)
                            {
                                errorStack.Push("Semantic Error: Can't reference variables before they are declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 069)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            idListStack.Push(id);
                            id = semanitcActionStack.Pop();
                        }
                        while (idListStack.Count > 0)
                        {
                            id = idListStack.Pop();
                            quadTable.GenQuad(QuadTable.Operation.READ_LINE, 0, 0, id);
                        }
                        #endregion

                        if (S1())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting '(' for 'readln', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 037)");
                    return false;
                }
            }
            else if (checkNextToken("writeln"))
            {
                if (checkNextToken("("))
                {
                    semanitcActionStack.Push(-1);
                    if (IDL())
                    {
                        #region Semantics
                        Stack<int> idListStack = new Stack<int>();
                        int id = semanitcActionStack.Pop();
                        while (id != -1)
                        {
                            if (symbolTable[id].Size.Count != 0)
                            {
                                errorStack.Push("Semantic Error: Can only write integers, not arrays! Line " + lexicalAnalyzer.LineNumber + " (CODE 061)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            else if (!symbolTable[id].IsDeclared)
                            {
                                errorStack.Push("Semantic Error: Can't reference variables before they are declared! Line " + lexicalAnalyzer.LineNumber + " (CODE 068)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            idListStack.Push(id);
                            id = semanitcActionStack.Pop();
                        }
                        while (idListStack.Count > 0)
                        {
                            id = idListStack.Pop();
                            quadTable.GenQuad(QuadTable.Operation.WRITE_LINE, id, 0, 0);
                        }
                        #endregion
                        if (S1())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting '(' for 'writeln', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 038)");
                    return false;
                }
            }
            else if (checkNextToken("case")) //TODO: Add Semantic Action for the Case Statement.
            {
                semanitcActionStack.Push(-1); //This is the delimiter so I know when to stop adding jumps.
                if (C())
                {
                    if (checkNextToken("do"))
                    {
                        #region Semantics
                        int condition = semanitcActionStack.Pop();
                        int falseJump = quadTable.GenQuad(QuadTable.Operation.JUMP_FALSE, condition, 0, 0);
                        semanitcActionStack.Push(falseJump);
                        #endregion
                        if (S())
                        {
                            #region Semantics
                            int finalJump = quadTable.GenQuad(QuadTable.Operation.JUMP, 0, 0, 0);
                            semanitcActionStack.Push(finalJump);
                            #endregion
                            if (M())
                            {
                                if (S1())
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting 'do', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 039)");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("while"))
            {
                int loopStart = quadTable.quads.Count;
                if (C())
                {
                    #region
                    int condition = semanitcActionStack.Pop();
                    int falseJump = quadTable.GenQuad(QuadTable.Operation.JUMP_FALSE, condition, 0, 0);
                    #endregion
                    if (checkNextToken("do"))
                    {
                        if (S())
                        {
                            #region Semantics
                            int loopBack = quadTable.GenQuad(QuadTable.Operation.JUMP, 0, loopStart, 0);
                            QuadTable.Quad temp = quadTable.quads[falseJump];
                            temp.operand2 = quadTable.quads.Count;
                            quadTable.quads.RemoveAt(falseJump);
                            quadTable.quads.Insert(falseJump, temp);
                            #endregion
                            if (checkNextToken("od"))
                            {
                                if (S1())
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                errorStack.Push("Syntax Error: Expecting 'od', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 040)");
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting 'do', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 041)");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("if"))
            {
                if (C())
                {
                    #region Semantics
                    int condition = semanitcActionStack.Pop();
                    int falseJump = quadTable.GenQuad(QuadTable.Operation.JUMP_FALSE, condition, 0, 0);
                    semanitcActionStack.Push(condition);
                    semanitcActionStack.Push(falseJump);
                    #endregion
                    if (checkNextToken("then"))
                    {
                        if (S())
                        {
                            if (S2())
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting 'then', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 042)");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("foreach"))
            {
                if (isNextTokenIdentifier())
                {
                    if (checkNextToken("in"))
                    {
                        if (ID())
                        {
                            int container = semanitcActionStack.Pop();
                            int value = semanitcActionStack.Pop();
                            if (!symbolTable[value].IsDeclared)
                            {
                                errorStack.Push("Semantic Error: Cannot reference variable '" + symbolTable[value].Name.Trim() + "' before it is declared at line " + lexicalAnalyzer.LineNumber + " (CODE 066)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            if (!symbolTable[container].IsDeclared)
                            {
                                errorStack.Push("Semantic Error: Cannot reference variable '" + symbolTable[container].Name.Trim() + "' before it is declared at line " + lexicalAnalyzer.LineNumber + " (CODE 067)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            if (symbolTable[value].Size.Count != symbolTable[container].Size.Count - 1)
                            {
                                errorStack.Push("Semantic Error: The first argument in a foreach loop must be one demension smaller than the second! Line " + lexicalAnalyzer.LineNumber + " (CODE 065)");
                                throw new Exception(errorStack.ToArray()[0]);
                                return false;
                            }
                            int max = symbolTable.GenTemp().index;
                            int counter = symbolTable.GenTemp().index;
                            quadTable.GenQuad(QuadTable.Operation.ALLOCATE, counter, 0, 0);
                            quadTable.GenQuad(QuadTable.Operation.ALLOCATE, max, symbolTable[container].Size[0], 0);
                            quadTable.GenQuad(QuadTable.Operation.OFFSET, container, symbolTable.IndexOf("0"), value);
                            int loopBack = quadTable.quads.Count;
                            if (checkNextToken("do"))
                            {
                                if (S())
                                {
                                    if (checkNextToken("od"))
                                    {
                                        quadTable.GenQuad(QuadTable.Operation.ADDITION, counter, symbolTable.IndexOf("1"), counter);
                                        int condition = symbolTable.GenTemp().index;
                                        quadTable.GenQuad(QuadTable.Operation.ALLOCATE, condition, 0, 0);
                                        quadTable.GenQuad(QuadTable.Operation.LESS_THAN, counter, max, condition);
                                        quadTable.GenQuad(QuadTable.Operation.JUMP_FALSE, condition, quadTable.quads.Count + 3, 0);
                                        quadTable.GenQuad(QuadTable.Operation.UNCHECKED_OFFSET, container, counter, value);
                                        quadTable.GenQuad(QuadTable.Operation.JUMP, 0, loopBack, 0);
                                        if (S1())
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        errorStack.Push("Syntax Error: Expecting 'od', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 043)");
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                errorStack.Push("Syntax Error: Expecting 'do', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 044)");
                                return false;
                            }
                        }
                        else
                        {
                            errorStack.Push("Syntax Error: Expecting a valid identifier, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 045)");
                            return false;
                        }
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting 'in', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 046)");
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting a valid identifier, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 047)");
                    return false;
                }
            }
            else if (checkNextToken("with"))
            {
                semanitcActionStack.Push(-1);
                locallyDefined = new List<int>();
                if (D())
                {
                    if (S())
                    {
                        if (checkNextToken("end"))
                        {
                            int id = semanitcActionStack.Pop();
                            while (id != -1)
                            {
                                int temp = semanitcActionStack.Pop();

                                List<int> idSize = new List<int>(symbolTable[id].Size);
                                bool idIsDeclared = symbolTable[id].IsDeclared;
                                SymbolTable.TableType idType = symbolTable[id].Type;

                                symbolTable[id].Size = new List<int>(symbolTable[temp].Size);
                                symbolTable[id].IsDeclared = symbolTable[temp].IsDeclared;
                                symbolTable[id].Type = symbolTable[temp].Type;

                                symbolTable[temp].Size = new List<int>(idSize);
                                symbolTable[temp].IsDeclared = idIsDeclared;
                                symbolTable[temp].Type = idType;

                                quadTable.GenQuad(QuadTable.Operation.SWAP, id, temp, 0);
                                id = semanitcActionStack.Pop();
                            }
                            if (S1())
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            errorStack.Push("Syntax Error: Expecting 'end' to close 'with', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 048)");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }

            }
            else
            {
                errorStack.Push("Syntax Error: Expecting the start of a statement, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 049)");
                return false;
            }
        }

        public static bool S1()
        {
            if (checkNextToken(";"))
            {
                if (S3())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting ';' at the end of the statement, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 028)");
                return false;
            }
        }

        public static bool S3()
        {
            List<string> epschilonSelectionSet = new List<string>(new string[] { "end", ":", "od", "fi", "esac", "else" });
            if (isNextTokenInSelectionSet(epschilonSelectionSet))
            {
                return true;
            }
            else if (S())
            {
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting closing structure or else, found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 029)");
                return false;
            }
        }

        public static bool S2()
        {
            if (checkNextToken("fi"))
            {
                #region Semantics
                int falseJump = semanitcActionStack.Pop();
                int condition = semanitcActionStack.Pop();
                QuadTable.Quad temp = quadTable.quads[falseJump];
                temp.operand2 = quadTable.quads.Count ;
                quadTable.quads.RemoveAt(falseJump);
                quadTable.quads.Insert(falseJump, temp);
                #endregion
                if (S1())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("else"))
            {
                #region Semantics
                int falseJump = semanitcActionStack.Pop();
                int condition = semanitcActionStack.Pop();
                QuadTable.Quad temp = quadTable.quads[falseJump];
                temp.operand2 = quadTable.quads.Count;
                quadTable.quads.RemoveAt(falseJump);
                quadTable.quads.Insert(falseJump, temp);
                int trueJump = quadTable.GenQuad(QuadTable.Operation.JUMP_TRUE, condition, 0, 0);
                
                #endregion
                if (S())
                {
                    if (checkNextToken("fi"))
                    {
                        #region Semantics
                        temp = quadTable.quads[trueJump];
                        temp.operand2 = quadTable.quads.Count;
                        quadTable.quads.RemoveAt(trueJump);
                        quadTable.quads.Insert(trueJump, temp);
                        #endregion
                        if (S1())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting 'fi' to close 'else', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 030)");
                        return false;
                    }
                }
                else
                {
                    errorStack.Push("Syntax Error: Expecting 'fi' or 'else', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 031)");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool M()
        {
            if (checkNextToken(":"))
            {
                if (C())
                {
                    if (checkNextToken("do"))
                    {
                        #region Semantics
                        int condition = semanitcActionStack.Pop();
                        int falseJump = quadTable.GenQuad(QuadTable.Operation.JUMP_FALSE, condition, 0, 0);
                        semanitcActionStack.Push(falseJump);
                        #endregion
                        if (S())
                        {
                            #region Semantics
                            int finalJump = quadTable.GenQuad(QuadTable.Operation.JUMP, 0, 0, 0);
                            semanitcActionStack.Push(finalJump);
                            #endregion
                            if (M())
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        errorStack.Push("Syntax Error: Expecting 'do' '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 032)");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (checkNextToken("esac"))
            {
                #region Semantics
                int falseJump = semanitcActionStack.Pop();
                while (falseJump != -1)
                {
                    QuadTable.Quad temp = quadTable.quads[falseJump];
                    temp.operand2 = quadTable.quads.Count;
                    quadTable.quads.RemoveAt(falseJump);
                    quadTable.quads.Insert(falseJump, temp); // Here!!
                    falseJump = semanitcActionStack.Pop();
                }
                #endregion
                return true;
            }
            else
            {
                errorStack.Push("Syntax Error: Expecting 'do' or 'esac', found '" + nextTokenValue + "' at line " + lexicalAnalyzer.LineNumber +" (CODE 033)");
                return false;
            }
        }

        public static void LEX()
        {
            nextToken = lexicalAnalyzer.NextToken.index;
            if (Program.DEBUG)
                Console.WriteLine(nextTokenValue);
        }

        private static bool checkNextToken(string toEvaluate)
        {
            bool retval = nextTokenValue.Equals(toEvaluate.Trim());
            if (retval)
            {
                LEX();
            }
            return retval;
        }
        private static bool isNextTokenConstant()
        {
            bool retval = symbolTable.IsConstant(nextTokenValue);
            if (retval)
            {
                semanitcActionStack.Push(nextToken);
                LEX();
            }
            return retval;
        }

        private static bool isNextTokenIdentifier()
        {
            bool retval = symbolTable.IsIdentifier(nextTokenValue);
            if (retval)
            {
                semanitcActionStack.Push(nextToken);
                LEX();
            }
            return retval;
        }

        private static bool isNextTokenInSelectionSet(List<string> selectionSet)
        {
            return selectionSet.Contains(nextTokenValue);
        }

        private static string nextTokenValue
        {
            get
            {
                return symbolTable[nextToken].Name.Trim();
            }
        }

        private static void semanticBinaryOperation(QuadTable.Operation operation)
        {
            int x = semanitcActionStack.Pop();
            int y = semanitcActionStack.Pop();
            if (!(symbolTable[x].IsDeclared && symbolTable[y].IsDeclared))
            {
                errorStack.Push("Semantic Error: Variable cannot be referenced before it is defined! Line " + lexicalAnalyzer.LineNumber + " (CODE 053)");
                throw new Exception(errorStack.ToArray()[0]);
            }
            int t = symbolTable.GenTemp().index;
            quadTable.GenQuad(QuadTable.Operation.ALLOCATE, t, 0, 0);
            quadTable.GenQuad(operation, y, x, t);
            semanitcActionStack.Push(t);
        }

        private static void semanticUnaryOperation(QuadTable.Operation operation)
        {
            int x = semanitcActionStack.Pop();
            if (!symbolTable[x].IsDeclared)
            {
                errorStack.Push("Semantic Error: Variable cannot be referenced before it is defined! Line " + lexicalAnalyzer.LineNumber + " (CODE 054)");
                throw new Exception(errorStack.ToArray()[0]);
            }
            int t = symbolTable.GenTemp().index;
            quadTable.GenQuad(QuadTable.Operation.ALLOCATE, t, 0, 0);
            quadTable.GenQuad(operation, x, 0, t);
            semanitcActionStack.Push(t);
        }
    }
}
