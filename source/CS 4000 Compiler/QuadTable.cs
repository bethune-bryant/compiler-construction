using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS_4000_Compiler
{
    public class QuadTable
    {
        public enum Operation
        {
            ASSIGNMENT, ADDITION, SUBTRACTION, MULTIPLICATION, DIVISION, EXPONENTIATION,
            LESS_THAN, GREATER_THAN, EQUAL_TO, OR, AND, NOT, JUMP_TRUE, JUMP_FALSE, JUMP,
            READ, WRITE, READ_LINE, WRITE_LINE, ALLOCATE, OFFSET, UNCHECKED_OFFSET, SWAP, EXIT
        };

        public struct Quad
        {
            public Operation operation;
            public int operand1;
            public int operand2;
            public int result;

            public Quad(Operation operation, int operand1, int operand2, int result)
            {
                this.operation = operation;
                this.operand1 = operand1;
                this.operand2 = operand2;
                this.result = result;
            }

            public override string ToString()
            {
                string pad = "                        ";
                string stringOperation = operation.ToString() + pad;
                stringOperation = stringOperation.Substring(0, pad.Length);
                string stringOperand1 = (operand1 + 1).ToString() + "       ";
                stringOperand1 = stringOperand1.Substring(0, 6);
                string stringOperand2 = (operand2 + 1).ToString() + "       ";
                stringOperand2 = stringOperand2.Substring(0, 6);
                return "Operation: " + stringOperation + "\t | Operand 1: " + stringOperand1 + "\t | Operand 2: " + stringOperand2 + "\t | Result: " + (result + 1).ToString();
            }
        }

        public List<Quad> quads;

        public QuadTable()
        {
            quads = new List<Quad>();
        }

        public QuadTable(string ExecutableOutput)
        {
            quads = new List<Quad>();
            List<string> lines = new List<string>(ExecutableOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            foreach (string line in lines)
            {
                List<string> entries = new List<string>(line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                quads.Add(new Quad((Operation)Enum.Parse(new Operation().GetType(), entries[0]), Int32.Parse(entries[1]), Int32.Parse(entries[2]), Int32.Parse(entries[3])));
            }
        }

        public int GenQuad(Operation operation, int operand1, int operand2, int result)
        {
            this.quads.Add(new Quad(operation, operand1, operand2, result));
            return this.quads.Count - 1;
        }

        public override string ToString()
        {
            string retval = "";

            foreach (Quad quad in quads)
            {
                retval += quad.ToString() + Environment.NewLine;
            }

            return retval;
        }

        public string GetExecutableOutput()
        {
            string retval = "";

            foreach (Quad quad in quads)
            {
                retval += quad.operation + " | " + quad.operand1 + " | " + quad.operand2 + " | " + quad.result + Environment.NewLine;
            }

            return retval;
        }



    }
}
