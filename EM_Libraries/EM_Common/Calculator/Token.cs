using System;
using System.Collections.Generic;
using System.Text;

namespace EM_Common
{
    /// <summary>
	/// Represents a node in the formula expression tree.
    /// </summary>
    internal class Token
    {
        public TKID ID;
        public TKTYPE Type;
        public object Value;

        public Token(object value, TKID id, TKTYPE type)
        {
            Value = value;
            ID = id;
            Type = type;
        }
    }

    /// <summary>
    /// Token types (used when building expressions, sequence defines operator priority)
    /// </summary>
    internal enum TKTYPE
    {
        ANDOR,      // & |
        COMPARE,	// < > = <= >= !=
        ADDSUB,		// + -
        MULDIV,		// * /
        POWER,		// ^
        OPERATORS,  // <max>, <min>, <abs>
        GROUP,		// ( ) , 
        LGROUP,		// { }
        LITERAL,    // 123.32
        IDENTIFIER,  // functions, variables
        FOOTNOTE    // 300#y
    }
    /// <summary>
    /// Token ID (used when evaluating expressions)
    /// </summary>
    internal enum TKID
    {
        AND, OR,                            // LOGIC
        GT, LT, GE, LE, EQ, NE, NOT,        // COMPARE
        ADD, SUB,                           // ADDSUB
        MUL, DIV, DIVINT, MOD,              // MULDIV
        POWER, FOOT,                        // POWER
        MAX, MIN, ABS,                      // SPECIAL OPERATORS
        OPEN, CLOSE, LOPEN, LCLOSE, COMMA,  // GROUP
        ATOM,                               // LITERAL, IDENTIFIER
        END                                 // END OF FORMULA
    }
}
