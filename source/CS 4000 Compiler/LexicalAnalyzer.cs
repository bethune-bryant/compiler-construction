using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CS_4000_Compiler
{
    public class LexicalAnalyzer
    {
        public static string delimiters =
            ";" + Environment.NewLine +
            "[" + Environment.NewLine +
            "]" + Environment.NewLine +
            "," + Environment.NewLine +
            "+" + Environment.NewLine +
            "-" + Environment.NewLine +
            "*" + Environment.NewLine +
            "/" + Environment.NewLine +
            "^" + Environment.NewLine +
            "(" + Environment.NewLine +
            ")" + Environment.NewLine +
            "<" + Environment.NewLine +
            ">" + Environment.NewLine +
            ":" + Environment.NewLine +
            "=" + Environment.NewLine +
            ":="
        ;


        public SymbolTable symbolTable;

        private List<string> newTokens;
        private List<string> retrievedTokens;
        private List<int> lineNumbers;
        private int index;

        public LexicalAnalyzer(string inputFileName, string delimiterFileName, SymbolTable symbolTable)
        {
            this.newTokens = Tokenizer.tokenizeFileFromFile(inputFileName, delimiterFileName);
            lineNumbers = Tokenizer.lineNumbers;
            this.retrievedTokens = new List<string>();
            this.symbolTable = symbolTable;
            index = 0;

        }

        public LexicalAnalyzer(string inputFile, SymbolTable symbolTable)
        {
            this.newTokens = Tokenizer.tokenizeFile(inputFile, delimiters);
            lineNumbers = Tokenizer.lineNumbers;
            this.retrievedTokens = new List<string>();
            this.symbolTable = symbolTable;
            index = 0;

        }

        /// <summary>
        /// Adds the next token to the Symbol Table, and then returns the tokendata for that token.
        /// </summary>
        public SymbolTable.TokenData NextToken
        {
            get
            {
                if (HasToken)
                {
                    string currentToken = newTokens[0];
                    newTokens.RemoveAt(0);
                    retrievedTokens.Add(currentToken);
                    index++;
                    if (isIdentifier(currentToken))
                    {
                        return symbolTable.AddIdentifier(currentToken);
                    }
                    else
                    {
                        return symbolTable.AddConstant(currentToken);
                    }
                }
                else
                {
                    return new SymbolTable.TokenData(symbolTable.IndexOf("eof"), symbolTable["eof"].TokenType);
                }
            }
        }

        public int LineNumber
        {
            get
            {
                return lineNumbers[index -1];
            }
        }

        /// <summary>
        /// Determines whether there are anymore tokens.
        /// </summary>
        public bool HasToken
        {
            get
            {
                if (newTokens.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines whether the token is an identifier.
        /// </summary>
        /// <param name="token">Token to determine.</param>
        /// <returns>True if the token is a valid identifier.</returns>
        public static bool isIdentifier(string token)
        {
            if (token.Length < 1 || char.IsLetter(token[0]))
            {
                foreach (char letter in token)
                {
                    if (!(char.IsLetterOrDigit(letter) || letter == '~'))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the token is a constant.
        /// </summary>
        /// <param name="token">Token to determine.</param>
        /// <returns>True if the token is a valid constant.</returns>
        public static bool isConstant(string token)
        {
            if (token.Length < 1)
            {
                return false;
            }
            foreach (char letter in token)
            {
                if (!char.IsDigit(letter))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// A static class which is used to tokenize programs.
        /// </summary>
        public static class Tokenizer
        {
            public static List<int> lineNumbers;
            public static int localityCounter = 0;
            /// <summary>
            /// Static class which tokenizes a Program from a string with a delimiter list from a string.
            /// </summary>
            /// <param name="path">String of the Program.</param>
            /// <param name="delimiterPath">String of the delimiter list.</param>
            /// <returns>A list of tokens.</returns>
            public static List<string> tokenize(string toTokenize, string delimiters)
            {
                List<string> retval = new List<string>();
                lineNumbers = new List<int>();
                localityCounter = 0;
                int lineNumber = 1;

                bool constantFlag = false;
                bool commentFlag = false;

                string currentToken = "";

                toTokenize += " ";

                //TODO: Done Change to an index for-loop, so as to do a 1 character look ahead, to look for ':=' and '/*' '*/'

                for (int i = 0; i < toTokenize.Length; i++)
                {
                    char originalCharacter = toTokenize[i];
                    string character = originalCharacter.ToString().ToLower()[0].ToString();
                    if (originalCharacter == '\n')
                        lineNumber++;
                    if (!commentFlag)
                    {
                        if ((i + 1) < toTokenize.Length && toTokenize[i] == '/' && toTokenize[i + 1] == '*')
                        {
                            character = "/*";
                            i++;
                            if (currentToken.Length != 0)
                            {
                                if (!SymbolTable.reservedWords.Contains(currentToken.Trim()) && isIdentifier(currentToken))
                                {
                                    //currentToken += "~" + localityCounter.ToString();
                                }
                                else if (currentToken.Trim().Equals("with"))
                                {
                                    localityCounter++;
                                }
                                else if (currentToken.Trim().Equals("end"))
                                {
                                    localityCounter--;
                                }
                                retval.Add(currentToken);
                                lineNumbers.Add(lineNumber);
                            }
                            constantFlag = false;
                            commentFlag = true;
                            currentToken = "";
                        }
                        else
                        {
                            if ((i + 1) < toTokenize.Length && toTokenize[i] == ':' && toTokenize[i + 1] == '=')
                            {
                                character = ":=";
                                i++;
                            }
                            if (delimiters.Contains(character) || char.IsWhiteSpace(character[0]))
                            {
                                if (!char.IsWhiteSpace(character[0]))
                                {
                                    if (currentToken.Length != 0)
                                    {
                                        if (!SymbolTable.reservedWords.Contains(currentToken.Trim()) && isIdentifier(currentToken))
                                        {
                                            //currentToken += "~" + localityCounter.ToString();
                                        }
                                        else if (currentToken.Trim().Equals("with"))
                                        {
                                            localityCounter++;
                                        }
                                        else if (currentToken.Trim().Equals("end"))
                                        {
                                            localityCounter--;
                                        }
                                        retval.Add(currentToken);
                                        lineNumbers.Add(lineNumber);
                                    }
                                    currentToken = character.ToString();
                                    retval.Add(currentToken);
                                    lineNumbers.Add(lineNumber);
                                }
                                else if (currentToken.Length != 0)
                                {
                                    if (!SymbolTable.reservedWords.Contains(currentToken.Trim()) && isIdentifier(currentToken))
                                    {
                                        //currentToken += "~" + localityCounter.ToString();
                                    }
                                    else if (currentToken.Trim().Equals("with"))
                                    {
                                        localityCounter++;
                                    }
                                    else if (currentToken.Trim().Equals("end"))
                                    {
                                        localityCounter--;
                                    }
                                    retval.Add(currentToken);
                                    lineNumbers.Add(lineNumber);
                                }

                                constantFlag = false;
                                currentToken = "";
                            }
                            else if (currentToken.Length == SymbolTable.TOKEN_LENGTH)
                            {
                                throw new ArgumentException("Lexical Error at character '" + character + "' at line " + lineNumber + ": Invalid TokenLength");
                            }
                            else if (char.IsDigit(character[0]))
                            {
                                if (currentToken.Length == 0)
                                {
                                    currentToken += character;
                                    constantFlag = true;
                                }
                                else
                                {
                                    currentToken += character;
                                }
                            }
                            else if (char.IsLetter(character[0]))
                            {
                                if (constantFlag)
                                {
                                    throw new ArgumentException("Lexical Error at character '" + character + "' at line " + lineNumber + ": Invalid Constant");
                                }
                                else
                                {
                                    currentToken += character;
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Lexical Error at character '" + character + "' at line " + lineNumber + ": Invalid Character");
                            }
                        }
                    }
                    else
                    {
                        if ((i + 1) < toTokenize.Length && toTokenize[i] == '*' && toTokenize[i + 1] == '/')
                        {
                            character = "*/";
                            i++;
                            constantFlag = false;
                            commentFlag = false;
                            currentToken = "";
                        }
                    }
                }

                return retval;
            }

            /// <summary>
            /// Static class which tokenizes a Program from a File with a delimiter list.
            /// </summary>
            /// <param name="path">Location of the Program.</param>
            /// <param name="delimiterPath">The delimiter list.</param>
            /// <returns>A list of tokens.</returns>
            public static List<string> tokenizeFile(string path, string delimiters)
            {
                return tokenize(File.ReadAllText(path), delimiters);
            }

            /// <summary>
            /// Static class which tokenizes a Program from a string with a delimiter list from a file.
            /// </summary>
            /// <param name="path">String of the program.</param>
            /// <param name="delimiterPath">Location of the delimiter list.</param>
            /// <returns>A list of tokens.</returns>
            public static List<string> tokenizeFromFile(string toTokenize, string delimiterPath)
            {
                return tokenize(toTokenize, File.ReadAllText(delimiterPath));
            }

            /// <summary>
            /// Static class which tokenizes a Program from a File with a delimiter list from a file.
            /// </summary>
            /// <param name="path">Location of the Program.</param>
            /// <param name="delimiterPath">Location of the delimiter list.</param>
            /// <returns>A list of tokens.</returns>
            public static List<string> tokenizeFileFromFile(string path, string delimiterPath)
            {
                return tokenize(File.ReadAllText(path), File.ReadAllText(delimiterPath));
            }
        }
    }
}
