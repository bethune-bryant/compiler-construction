using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.IO;

namespace CS_4000_Compiler
{
    public class SymbolTable
    {
        public static List<string> reservedWords = new List<string>(new string[] {
            "program",
            "eof",
            "begin",
            "end",
            "array",
            "integer",
            "not",
            "and",
            "or",
            "read",
            "write",
            "readln",
            "writeln",
            "case",
            "do",
            "while",
            "od",
            "if",
            "then",
            "fi",
            "else",
            "foreach",
            "in",
            "esac",
            "with",
            "exp",
            ";",
            "[",
            "]",
            ",",
            "+",
            "-",
            "*",
            "/",
            "^",
            "(",
            ")",
            "<",
            ">",
            ":",
            "=",
            ":="
        });

        public const int TOKEN_LENGTH = 15;
        public enum TableType { UnknownForNow, TEMPORARY, RESERVED, INTEGER, ARRAY, CONSTANT};

        /// <summary>
        /// This STRUCT is used to hold the data that will be passed back about the token.
        /// Integer: Token Type
        /// Integer: Index in Symbol Table
        /// </summary>
        public struct TokenData
        {
            public int index;
            public int tokenType;

            public TokenData(int index, int tokenType)
            {
                this.index = index;
                this.tokenType = tokenType;
            }

            public override string ToString()
            {
                return "Token Index: " + index.ToString() + "\t | Token Type: " + tokenType.ToString();
            }
        }

        /// <summary>
        /// This class represents a "row" in the symbol table.
        /// It contains all the information required by a Token in the Symbol Table.
        /// </summary>
        public class TableEntry
        {
            private string name;
            private int tokenType;
            private bool declared;
            private TableType type;
            private List<int> size;
            private int location;

            #region Accessors

            /// <summary>
            /// The actual Token String.
            /// When setting, it confirms that the string is not longer than the maximum.
            /// Throws ArgumentOutOfRangeException.
            /// </summary>
            public string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    if (value.Length > TOKEN_LENGTH)
                    {
                        throw new ArgumentOutOfRangeException("Name", value, "The length of the token cannot be longer than " + TOKEN_LENGTH.ToString() + " characters!");
                    }
                    else
                    {
                        string tempval = value + "                    ";
                        tempval = tempval.Substring(0, TOKEN_LENGTH);
                        this.name = tempval;
                    }
                }
            }

            /// <summary>
            /// Integer representing Token Type.
            /// </summary>
            public int TokenType
            {
                get
                {
                    return this.tokenType;
                }
                set
                {
                    this.tokenType = value;
                }
            }

            /// <summary>
            /// Boolean indicating whether the the Identifier is declared.
            /// </summary>
            public bool IsDeclared
            {
                get
                {
                    return this.declared;
                }
                set
                {
                    this.declared = value;
                }
            }

            /// <summary>
            /// The Data Type of the token.
            /// </summary>
            public TableType Type
            {
                get
                {
                    return this.type;
                }
                set
                {
                    this.type = value;
                }
            }

            /// <summary>
            /// The size of an Array.
            /// </summary>
            public List<int> Size
            {
                get
                {
                    return this.size;
                }
                set
                {
                    this.size = new List<int>(value);
                }
            }

            /// <summary>
            /// The memory location of the data.
            /// </summary>
            public int Location
            {
                get
                {
                    return this.location;
                }
                set
                {
                    this.location = value;
                }
            }

            #endregion

            #region Constructors

            public TableEntry(string name, int tokenType, bool declared, TableType type,List<int> size, int location)
            {
                this.Name = name;
                this.tokenType = tokenType;
                this.declared = declared;
                this.type = type;
                this.size = new List<int>(size);
                this.location = location;
            }

            /// <summary>
            /// This is the constructor to be used by the Lexical Analyzer.
            /// It initializes only the Name and Token Type, which is all that is handled by the Lexical Analyzer.
            /// </summary>
            /// <param name="name">The Token String</param>
            /// <param name="tokenType">The type of token, for reserved words it's the index in the Symbol table. ID's = n + 1. CONST = n + 2.</param>
            public TableEntry(string name, int tokenType)
                : this(name, tokenType, false, 0, new List<int>(), 0)
            {

            }

            #endregion

            #region Overrides and Operators

            /// <summary>
            /// Gets the Integer representation of the Token String.
            /// </summary>
            /// <returns>Integer representation of the Token String.</returns>
            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            /// <summary>
            /// Compares this TambleEntry against another based entirely upon Token String.
            /// This is because, in theory, we will never have two items in the symbol table
            /// with the same token string.
            /// </summary>
            /// <param name="obj">The Table Entry to compare against.</param>
            /// <returns>True if they are equal. False otherwise.</returns>
            public override bool Equals(object obj)
            {
                return this.GetHashCode().Equals(obj.GetHashCode());
            }

            /// <summary>
            /// Compares this TambleEntry against another based entirely upon Token String.
            /// This is because, in theory, we will never have two items in the symbol table
            /// with the same token string.
            /// </summary>
            /// <param name="inputToken">The Token String to compare against.</param>
            /// <param name="inputEntry">The table entry to compare against.</param>
            /// <returns>True if they are equal. False otherwise.</returns>
            public static bool operator ==(string inputToken, TableEntry inputEntry)
            {
                return inputToken.Equals(inputEntry.Name);
            }

            /// <summary>
            /// Compares this TambleEntry against another based entirely upon Token String.
            /// This is because, in theory, we will never have two items in the symbol table
            /// with the same token string.
            /// </summary>
            /// <param name="inputToken">The Token String to compare against.</param>
            /// <param name="inputEntry">The table entry to compare against.</param>
            /// <returns>True if they are not equal. False otherwise.</returns>
            public static bool operator !=(string inputToken, TableEntry inputEntry)
            {
                return !inputToken.Equals(inputEntry.Name);
            }

            /// <summary>
            /// Compares this TambleEntry against another based entirely upon Token String.
            /// This is because, in theory, we will never have two items in the symbol table
            /// with the same token string.
            /// </summary>
            /// <param name="inputEntry">The table entry to compare against.</param>
            /// <param name="inputToken">The Token String to compare against.</param>
            /// <returns>True if they are equal. False otherwise.</returns>
            public static bool operator ==(TableEntry inputEntry, string inputToken)
            {
                return inputToken.Equals(inputEntry.Name);
            }

            /// <summary>
            /// Compares this TambleEntry against another based entirely upon Token String.
            /// This is because, in theory, we will never have two items in the symbol table
            /// with the same token string.
            /// </summary>
            /// <param name="inputEntry">The table entry to compare against.</param>
            /// <param name="inputToken">The Token String to compare against.</param>
            /// <returns>True if they are not equal. False otherwise.</returns>
            public static bool operator !=(TableEntry inputEntry, string inputToken)
            {
                return !inputToken.Equals(inputEntry.Name);
            }

            /// <summary>
            /// This returns the string representation of the Table Entry.
            /// </summary>
            /// <returns>
            /// String representation of the table entry.
            /// </returns>
            public override string ToString()
            {
                string retval = "";

                retval +=    "Name: " + this.name;
                retval += "\t|Token Type: " + this.tokenType;
                retval += "\t|Declared: " + this.declared;
                retval += "\t|Type: " + this.type;
                retval += "\t|Size: ";
                foreach (int i in this.size)
                {
                    retval += i.ToString() + ", ";
                }
                retval += "\t|Location: " + this.location;

                return retval;
            }

            #endregion
        }

        public readonly List<TableEntry> symbols = new List<TableEntry>();

        private int identifierTokenType;
        private int constantTokenType;

        private int memoryIndex;

        /// <summary>
        /// Constructs a Symbol Table, reading all reserved words from a text file.
        /// </summary>
        /// <param name="path">Location of the text file where the reserved words are stored.</param>
        //public SymbolTable(string path)
        //{
        //    try
        //    {
        //        List<string> file = new List<string>(File.ReadAllLines(path));

        //        foreach (string line in file)
        //        {
        //            if (!line.Trim().Equals(""))
        //            {
        //                this.addReservedWord(line);
        //            }
        //        }
        //    }
        //    catch
        //    {
               
        //    }

        //    identifierTokenType = symbols.Count;
        //    constantTokenType = symbols.Count + 1;
        //    memoryIndex = 0;
        //}

        public SymbolTable(string ExecutableOutput)
        {
            symbols = new List<TableEntry>();
            List<string> lines = new List<string>(ExecutableOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            foreach (string line in lines)
            {
                List<string> entries = new List<string>(line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                List<string> stringSizes = new List<string>(entries[4].Trim(new char[] {'{', '}', ' '}).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                List<int> sizes = new List<int>();

                foreach (string size in stringSizes)
                {
                    sizes.Add(Int32.Parse(size));
                }

                symbols.Add(new TableEntry(entries[0].Trim(), Int32.Parse(entries[1]), bool.Parse(entries[2]), (TableType)Enum.Parse(new TableType().GetType(), entries[3]), sizes, Int32.Parse(entries[5])));
            }
        }

        /// <summary>
        /// Constructs a Symbol Table, from the static reserved words.
        /// </summary>
        public SymbolTable()
        {
            foreach (string line in reservedWords)
            {
                if (!line.Trim().Equals(""))
                {
                    this.addReservedWord(line);
                }
            }

            identifierTokenType = symbols.Count;
            constantTokenType = symbols.Count + 1;

            this.AddConstant("1");
            this.AddConstant("0");
            memoryIndex = 0;
        }

        /// <summary>
        /// This method is a private method used in the constructor to add reserved words.
        /// </summary>
        /// <param name="word">The reserved word to add to the Symbol Table.</param>
        private void addReservedWord(string word)
        {
            symbols.Add(new TableEntry(word, symbols.Count, false, TableType.RESERVED, new List<int>(), -1));
        }

        /// <summary>
        /// This function returns true if the Symbol Table already contains an entry for the 
        /// provided token. It returns false otherwise.
        /// </summary>
        /// <param name="token">The Token to check for existance within the Symbol Table.</param>
        /// <returns>True if the Symbol Table contains the token. False otherwise.</returns>
        public bool Contains(string token)
        {
            return this.symbols.Contains(new TableEntry(token, 0));
        }

        /// <summary>
        /// Returns a bool based on whether or not the token is a reserved word.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>True if the token is a reserved word, and false otherwise.</returns>
        public bool IsReservedWord(string token)
        {
            if (!this.Contains(token))
            {
                return false;
            }
            else if (this[token].TokenType != identifierTokenType && this[token].TokenType != constantTokenType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsIdentifier(string token)
        {
            if (!this.Contains(token))
            {
                return false;
            }
            else if (this[token].TokenType == identifierTokenType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsConstant(string token)
        {
            if (!this.Contains(token))
            {
                return false;
            }
            else if (this[token].TokenType == constantTokenType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDeclared(int index)
        {
            return this[index].IsDeclared;
        }

        /// <summary>
        /// If the constant already exists within the table, nothing is added.
        /// </summary>
        /// <param name="token">The token to add to the symbol table.</param>
        /// <returns>The TokenData for the token added(or alredy in) the table.</returns>
        public TokenData AddConstant(string token)
        {
            if (!this.Contains(token))
            {
                this.symbols.Add(new TableEntry(token, constantTokenType, true, TableType.CONSTANT, new List<int>(new int[] {}), 0));
            }

            return new TokenData(this.IndexOf(token), symbols[this.IndexOf(token)].TokenType);
        }

        /// <summary>
        /// If the identifier already exists within the table, nothing is added.
        /// </summary>
        /// <param name="token">The token to add to the symbol table.</param>
        /// <returns>The TokenData for the token added(or alredy in) the table.</returns>
        public TokenData AddIdentifier(string token)
        {
            if (!this.Contains(token))
            {
                this.symbols.Add(new TableEntry(token, identifierTokenType));
            }

            return new TokenData(this.IndexOf(token), symbols[this.IndexOf(token)].TokenType);
        }

        public TokenData GenTemp()
        {
            this.symbols.Add(new TableEntry("", identifierTokenType, true, TableType.TEMPORARY, new List<int>(new int[] { }), 0));

            return new TokenData(this.symbols.Count - 1, this.identifierTokenType);
        }

        public TokenData GenTemp(List<int> size)
        {
            this.symbols.Add(new TableEntry("", identifierTokenType, true, TableType.TEMPORARY, size, 0));

            return new TokenData(this.symbols.Count - 1, this.identifierTokenType);
        }

        /// <summary>
        /// Indexes the symbol table based on the numeric index.
        /// </summary>
        /// <param name="index">The integer index into the symbol table.</param>
        /// <returns>The table entry of the index.</returns>
        public TableEntry this[int index]
        {
            get
            {
                return this.symbols[index];
            }
        }

        /// <summary>
        /// Indexes the symbol table based on the string token.
        /// </summary>
        /// <param name="token">The token at which to index the table.</param>
        /// <returns>The table entry of the token.</returns>
        public TableEntry this[string token]
        {
            get
            {
                return this.symbols[this.symbols.IndexOf(new TableEntry(token, 0))];
            }
        }

        /// <summary>
        /// Gets the index of a token.
        /// </summary>
        /// <param name="token">The token to find the index of.</param>
        /// <returns>The index of the token.</returns>
        public int IndexOf(string token)
        {
            return this.symbols.IndexOf(new TableEntry(token, 0));
        }

        /// <summary>
        /// The number of elements in the symbol table.
        /// </summary>
        public int Size
        {
            get
            {
                return this.symbols.Count;
            }
        }

        /// <summary>
        /// Returns the string representation of the Symbol Table.
        /// </summary>
        /// <returns>Returns the string representation of the Symbol Table.</returns>
        public override string ToString()
        {
            string retval = "";

            foreach (TableEntry entry in this.symbols)
            {
                retval += entry.ToString() + Environment.NewLine;
            }

            return retval;
        }

        public string GetExecutableOutput()
        {
            string retval = "";

            foreach (TableEntry entry in this.symbols)
            {
                retval += entry.Name + " | " + entry.TokenType + " | " + entry.IsDeclared + " | " + entry.Type + " | {";
                foreach (int size in entry.Size)
                {
                    retval += size.ToString() + ",";
                }
                retval += "} | " + entry.Location + Environment.NewLine;
            }

            return retval;
        }
    }
}
