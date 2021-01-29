using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Threading;

namespace EM_Common
{
    /// <summary>
	/// CalcEngine parses strings and returns Expression objects that can be evaluated.
	/// </summary>
    /// <remarks>
    /// <para>Use the <b>RegisterFunction</b> method to define custom functions.</para>
    /// </remarks>
    public partial class Calculator
    {
        public Calculator(double scale = 1, bool _isCond = false)
        {
            CultureInfo = CultureInfo.InvariantCulture;
            _tkTbl = GetSymbolTable();
            _fnTbl = GetFunctionTable();
            _idChars = "$_:#";       // allow ":" for cases such as head:idmother or partner:idmother, allow "#" so that variable footnotes can be handled outside the Calculator
            _vars = new Dictionary<string, double>();
            _optimize = true;
            periods = DefPeriod.GetPeriods(scale);
            isCond = _isCond;
        }

        readonly bool isCond;
        string _expr;                           // expression being parsed
        int _len;                               // length of the expression being parsed
        int _ptr;				                // current pointer into expression
        string _idChars;                        // valid characters in identifiers (besides alpha and digits)
        Token _token;				            // current token being parsed
        Dictionary<object, Token> _tkTbl;       // table with tokens (+, -, etc)
        Dictionary<string, FunctionDefinition> _fnTbl;      // table with constants and functions (max, min, etc)
        IDictionary<string, double> _vars;      // table with variables
        bool _optimize;                         // optimize expressions when parsing
        CultureInfo _ci;                        // culture info used to parse numbers/dates
        char _decimal, _listSep, _percent;      // localized decimal separator, list separator, percent sign
        Dictionary<string, double> periods;     // required for replacing periods and scaling monetary parameters
        

        // build/get token table
        Dictionary<object, Token> GetSymbolTable()
        {
            if (_tkTbl == null)
            {
                _tkTbl = new Dictionary<object, Token>();
                AddToken('+', TKID.ADD, TKTYPE.ADDSUB);
                AddToken('-', TKID.SUB, TKTYPE.ADDSUB);
                AddToken('–', TKID.SUB, TKTYPE.ADDSUB);
                AddToken('(', TKID.OPEN, TKTYPE.GROUP);
                AddToken(')', TKID.CLOSE, TKTYPE.GROUP);
                AddToken('{', TKID.LOPEN, TKTYPE.LGROUP);
                AddToken('}', TKID.LCLOSE, TKTYPE.LGROUP);
                AddToken('*', TKID.MUL, TKTYPE.MULDIV);
                AddToken('/', TKID.DIV, TKTYPE.MULDIV);
                AddToken('\\', TKID.DIVINT, TKTYPE.MULDIV);
                AddToken('=', TKID.EQ, TKTYPE.COMPARE);
                AddToken('>', TKID.GT, TKTYPE.COMPARE);
                AddToken('<', TKID.LT, TKTYPE.COMPARE);
                AddToken('^', TKID.POWER, TKTYPE.POWER);
                AddToken('!', TKID.NOT, TKTYPE.COMPARE);
                AddToken('|', TKID.OR, TKTYPE.ANDOR);
                AddToken("||", TKID.OR, TKTYPE.ANDOR);
                AddToken('&', TKID.AND, TKTYPE.ANDOR);
                AddToken("&&", TKID.AND, TKTYPE.ANDOR);
                AddToken("!=", TKID.NE, TKTYPE.COMPARE);
                AddToken(">=", TKID.GE, TKTYPE.COMPARE);
                AddToken("<=", TKID.LE, TKTYPE.COMPARE);
                AddToken("<min>", TKID.MIN, TKTYPE.OPERATORS);
                AddToken("<max>", TKID.MAX, TKTYPE.OPERATORS);
                AddToken("<abs>", TKID.ABS, TKTYPE.OPERATORS);
                // parsing # is required as an operator to catch & handle period footnotes (e.g. "yem#y + 300#y" => "yem#y + 300/12"), 
                // and to propagate _level footnotes in conditions (e.g. "{yem + bch > 400}#1" => "{yem#1 + bch#1 > 400}")...
                AddToken('#', TKID.FOOT, TKTYPE.FOOTNOTE);
            }
            return _tkTbl;
        }

        public bool IsSymbol(char c) { return GetSymbolTable().TryGetValue(c, out Token dummy); }

        public bool IsVarChar(char c) 
        {
            var isLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '$';
            var isDigit = c >= '0' && c <= '9';

            return isLetter || isDigit || _idChars.IndexOf(c) > -1;
        }

        void AddToken(object symbol, TKID id, TKTYPE type)
        {
            var token = new Token(symbol, id, type);
            _tkTbl.Add(symbol, token);
        }

        /// <summary>
        /// Parses a string into an <see cref="Calculator_Expression"/>.
        /// </summary>
        /// <param name="expression">String to parse.</param>
        /// <returns>An <see cref="Calculator_Expression"/> object that can be evaluated.</returns>
        public Calculator_Expression CompileExpression(string expression)
        {
            // initialize
            _expr = expression;
            _len = _expr.Length;
            _ptr = 0;

            // skip leading equals sign
            if (_len > 0 && _expr[0] == '=')
            {
                _ptr++;
            }

            // parse the expression
            var expr = ParseExpression();

            // check for errors
            if (_token.ID != TKID.END)
            {
                Throw("Formula not properly closed.");
            }

            ResolvePeriods(ref expr);

            PropagateFootnotes(ref expr);

            // optimize expression
            if (_optimize)
            {
                // first perform the standard optimization
                expr = expr.Optimize();
                // then perform extra optimization, to optimize things like "x*1"=>"x" or cases that require rearranging of components (e.g. "3*x*4" => "12*x").
                OptimizeExpression(ref expr);
            }

            // done
            return expr;
        }

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <param name="operants">The formula variables with their values.</param>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The value of the expression.</returns>
        public double CalculateExpression(Dictionary<string, double> operands, Calculator_Expression expression)
        {
            var x = expression.Evaluate(operands);
            if (x is bool)
                return (bool)x ? 1.0 : 0.0;
            return (double)x;
        }

        // This function will go through a pre-compiled expression and resolve all periods (e.g. "yem#y + 300#y" => "yem#y + 300/12")
        void ResolvePeriods(ref Calculator_Expression exp)
        {
            if (exp is BinaryExpression)    // if binary, look for footnote
            {
                if (exp._token.ID == TKID.FOOT) // if there is one, copy it to the parts and continue deeper
                {
                    if ((exp as BinaryExpression)._rgt is VariableExpression)
                    {
                        string foot = ((exp as BinaryExpression)._rgt as VariableExpression)._name;
                        if (periods.ContainsKey("#" + foot))  // got a period footnote
                        {
                            // consider better handling or better message
                            if (!((exp as BinaryExpression)._lft is Calculator_Expression))
                                throw new Exception("Period footnotes are only allowed on numbers.");

                            // reduce the usage count
                            _vars[foot]--;
                            // if no usage left, then remove variable
                            if (_vars[foot] == 0) _vars.Remove(foot);

                            exp = (exp as BinaryExpression)._lft;
                            exp._token.Value = double.Parse(exp._token.Value.ToString()) * periods["#" + foot];
                        }
                        else
                            // consider better handling or better message
                            throw new Exception($"Unrecognised period footnote '{foot}'.");
                    }
                    else    // else dig deeper
                    {
                        ResolvePeriods(ref (exp as BinaryExpression)._lft);
                        ResolvePeriods(ref (exp as BinaryExpression)._rgt);
                    }
                }
                else    // else dig deeper
                {
                    ResolvePeriods(ref (exp as BinaryExpression)._lft);
                    ResolvePeriods(ref (exp as BinaryExpression)._rgt);
                }
            }
            else if (exp is UnaryExpression) // if not binary, just dig deeper
            {
                ResolvePeriods(ref (exp as UnaryExpression)._expr);
            }
            else if (exp is FunctionExpression)
            {
                for (int i = 0; i < (exp as FunctionExpression)._parms.Count; i++)
                {
                    Calculator_Expression expr = (exp as FunctionExpression)._parms[i];
                    ResolvePeriods(ref expr);
                    (exp as FunctionExpression)._parms[i] = expr;
                }
            }
        }

        // This function will go through a pre-compiled expression and propagate _level footnotes (e.g. "{yem + bch > 400}#1" => "{yem#1 + bch#1 > 400}")
        void PropagateFootnotes(ref Calculator_Expression exp)
        {
            if (exp is BinaryExpression)    // if binary, look for footnote
            {
                if (exp._token.ID == TKID.FOOT) // if there is one, copy it to the parts and continue deeper
                {
                    string foot = (exp as BinaryExpression)._rgt._token.Value.ToString();
                    exp = (exp as BinaryExpression)._lft;
                    CopyFootnote(ref exp, foot);
                    PropagateFootnotes(ref exp);
                }
                else    // else dig deeper
                {
                    PropagateFootnotes(ref (exp as BinaryExpression)._lft);
                    PropagateFootnotes(ref (exp as BinaryExpression)._rgt);
                }
            }
            else if (exp is UnaryExpression) // if not binary, just dig deeper
            {
                PropagateFootnotes(ref (exp as UnaryExpression)._expr);
            }
            else if (exp is FunctionExpression)
            {
                for (int i = 0; i < (exp as FunctionExpression)._parms.Count; i++)
                {
                    Calculator_Expression expr = (exp as FunctionExpression)._parms[i];
                    PropagateFootnotes(ref expr);
                    (exp as FunctionExpression)._parms[i] = expr;
                }
            }
        }

        // helper function for PropagateFootnotes
        void CopyFootnote(ref Calculator_Expression exp, string foot)
        {
            if (exp is VariableExpression)  // if variable, then copy footnote
            {
                // get the variable name
                string v = (exp as VariableExpression)._name;
                // reduce the usage count
                _vars[v]--;
                // if no usage left, then remove variable
                if (_vars[v] == 0) _vars.Remove(v);
                // build new name
                v += "#" + foot;
                // add new variable or increase usage count
                if (_vars.ContainsKey(v)) _vars[v]++;
                else _vars.Add(v, 1);
                // make sure the token is also updated
                (exp as VariableExpression)._name = v;
            }
            else if (exp is BinaryExpression)   // else dig deeper
            {
                Calculator_Expression lft = (exp as BinaryExpression)._lft;
                Calculator_Expression rgt = (exp as BinaryExpression)._rgt;
                if (lft is VariableExpression || lft is BinaryExpression || lft is UnaryExpression || lft is FunctionExpression) CopyFootnote(ref lft, foot);
                if (rgt is VariableExpression || rgt is BinaryExpression || rgt is UnaryExpression || rgt is FunctionExpression) CopyFootnote(ref rgt, foot);
            }
            else if (exp is UnaryExpression)
            {
                Calculator_Expression expr = (exp as UnaryExpression)._expr;
                if (expr is VariableExpression || expr is BinaryExpression || expr is UnaryExpression || expr is FunctionExpression) CopyFootnote(ref expr, foot);
            }
            else if (exp is FunctionExpression)
            {
                for (int i = 0; i < (exp as FunctionExpression)._parms.Count; i++)
                {
                    Calculator_Expression expr = (exp as FunctionExpression)._parms[i];
                    if (expr is VariableExpression || expr is BinaryExpression || expr is UnaryExpression || expr is FunctionExpression) CopyFootnote(ref expr, foot);
                }
            }
            else
            {
                Throw("Invalid use of footnote.");
            }
        }

        // This function will go through a pre-compiled expression and try to super-optimize it
        void OptimizeExpression(ref Calculator_Expression exp)
        {
            if (exp is BinaryExpression)        // optimize BinaryExpressions
            {
                Calculator_Expression lft = (exp as BinaryExpression)._lft;
                Calculator_Expression rgt = (exp as BinaryExpression)._rgt;

                OptimizeExpression(ref (exp as BinaryExpression)._lft);
                OptimizeExpression(ref (exp as BinaryExpression)._rgt);


                // if this is a multiplication try to optimize:
                //  - multiplications by 0
                //  - multiplications by 1 (e.g. due to #m footnotes) 
                //  - multiplications where the order can be changed to bring literals together (e.g. "2 * b * 3" = "2 * 3 * b" = "6 * b")
                if (exp._token.ID == TKID.MUL)
                {
                    if (lft._token.Type == TKTYPE.LITERAL)      // check if the left side is literal 
                    {
                        if ((double)lft._token.Value == 1)      // if 1, return the right side
                        {
                            exp = rgt;
                            return;
                        }
                        if ((double)lft._token.Value == 0)      // if 0, return 0
                        {
                            exp = new Calculator_Expression(new Token(0.0, TKID.ATOM, TKTYPE.LITERAL));
                            return;
                        }
                        if (rgt._token.ID == TKID.MUL)          // check if the right side is also a multiplication, maybe we can move literals around
                        {
                            if ((rgt as BinaryExpression)._lft._token.Type == TKTYPE.LITERAL)
                            {
                                (exp as BinaryExpression)._lft._token.Value = (double)lft._token.Value * (double)(rgt as BinaryExpression)._lft._token.Value;
                                (exp as BinaryExpression)._rgt = (rgt as BinaryExpression)._rgt;
                            }
                            if ((rgt as BinaryExpression)._rgt._token.Type == TKTYPE.LITERAL)
                            {
                                (exp as BinaryExpression)._lft._token.Value = (double)lft._token.Value * (double)(rgt as BinaryExpression)._rgt._token.Value;
                                (exp as BinaryExpression)._rgt = (rgt as BinaryExpression)._lft;
                            }
                        }
                    }
                    if (rgt._token.Type == TKTYPE.LITERAL)      // check if the right side is literal 
                    {
                        if ((double)rgt._token.Value == 1)      // if 1, return the right side
                        {
                            exp = lft;
                            return;
                        }
                        if ((double)rgt._token.Value == 0)      // if 0, return 0
                        {
                            exp = new Calculator_Expression(new Token(0.0, TKID.ATOM, TKTYPE.LITERAL));
                            return;
                        }
                        if (lft._token.ID == TKID.MUL)          // check if the left side is also a multiplication, maybe we can move literals around
                        {
                            if ((lft as BinaryExpression)._lft._token.Type == TKTYPE.LITERAL)
                            {
                                (exp as BinaryExpression)._rgt._token.Value = (double)rgt._token.Value * (double)(lft as BinaryExpression)._lft._token.Value;
                                (exp as BinaryExpression)._lft = (lft as BinaryExpression)._rgt;
                            }
                            if ((lft as BinaryExpression)._rgt._token.Type == TKTYPE.LITERAL)
                            {
                                (exp as BinaryExpression)._rgt._token.Value = (double)rgt._token.Value * (double)(lft as BinaryExpression)._rgt._token.Value;
                                (exp as BinaryExpression)._lft = (lft as BinaryExpression)._lft;
                            }
                        }
                    }
                }
            }
            exp.Optimize(); // finally, perform standard optimization again for any remaining calculations
        }

        /// <summary>
        /// Evaluates an expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>The value of the expression.</returns>
		public double Evaluate(Calculator_Expression expression)
        {
            return (double)expression.Evaluate(null);
        }
        /// <summary>
        /// Gets or sets whether the calc engine should optimize expressions when
        /// they are parsed.
        /// </summary>
        public bool OptimizeExpressions
        {
            get { return _optimize; }
            set { _optimize = value; }
        }
        /// <summary>
        /// Gets or sets a string that specifies special characters that are valid for identifiers.
        /// </summary>
        /// <remarks>
        /// Identifiers must start with a letter or an underscore, which may be followed by
        /// additional letters, underscores, or digits. This string allows you to specify
        /// additional valid characters such as ':' or '!' (used in Excel range references
        /// for example).
        /// </remarks>
        public string IdentifierChars
        {
            get { return _idChars; }
            set { _idChars = value; }
        }
        /// <summary>
        /// Registers a function that can be evaluated by this <see cref="Calculator"/>.
        /// </summary>
        /// <param name="functionName">Function name.</param>
        /// <param name="parmMin">Minimum parameter count.</param>
        /// <param name="parmMax">Maximum parameter count.</param>
        /// <param name="fn">Delegate that evaluates the function.</param>
        public void RegisterFunction(string functionName, int parmMin, int parmMax, CalcEngineFunction fn)
        {
            _fnTbl.Add(functionName, new FunctionDefinition(parmMin, parmMax, fn));
        }
        /// <summary>
        /// Registers a function that can be evaluated by this <see cref="Calculator"/>.
        /// </summary>
        /// <param name="functionName">Function name.</param>
        /// <param name="parmCount">Parameter count.</param>
        /// <param name="fn">Delegate that evaluates the function.</param>
        public void RegisterFunction(string functionName, int parmCount, CalcEngineFunction fn)
        {
            RegisterFunction(functionName, parmCount, parmCount, fn);
        }

        /// <summary>
        /// Gets the dictionary that contains function definitions.
        /// </summary>
        public Dictionary<string, FunctionDefinition> Functions
        {
            get { return _fnTbl; }
        }
        /// <summary>
        /// Gets the dictionary that contains simple variables (not in the DataContext).
        /// </summary>
        public IDictionary<string, double> Variables
        {
            get { return _vars; }
            set { _vars = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> to use when parsing numbers and dates.
        /// </summary>
        public CultureInfo CultureInfo
        {
            get { return _ci; }
            set
            {
                _ci = value;
                var nf = _ci.NumberFormat;
                _decimal = nf.NumberDecimalSeparator[0];
                _percent = nf.PercentSymbol[0];
                _listSep = _ci.TextInfo.ListSeparator[0];
            }
        }

        // build/get keyword table
        Dictionary<string, FunctionDefinition> GetFunctionTable()
        {
            if (_fnTbl == null)
            {
                // create table
                _fnTbl = new Dictionary<string, FunctionDefinition>(StringComparer.InvariantCultureIgnoreCase);

                // register built-in functions (and constants)
                ParserFunctions.Register(this);
            }
            return _fnTbl;
        }

        #region Parsing Functions

        Calculator_Expression ParseExpression()
        {
            GetToken();
            return ParseOr();
        }
        Calculator_Expression ParseOr()
        {
            var x = ParseAnd();
            while (_token.ID == TKID.OR)
            {
                var t = _token;
                GetToken();
                var exprArg = ParseAnd();
                x = new BinaryExpression(t, x, exprArg);
            }
            return x;
        }
        Calculator_Expression ParseAnd()
        {
            var x = ParseCompare();
            while (_token.ID == TKID.AND)
            {
                var t = _token;
                GetToken();
                var exprArg = ParseCompare();
                x = new BinaryExpression(t, x, exprArg);
            }
            return x;
        }
        Calculator_Expression ParseCompare()
        {
            var x = ParseAddSub();
            while (_token.Type == TKTYPE.COMPARE)
            {
                var t = _token;
                GetToken();
                var exprArg = ParseAddSub();
                x = new BinaryExpression(t, x, exprArg);
            }
            return x;
        }
        Calculator_Expression ParseAddSub()
        {
            var x = ParseMulDiv();
            while (_token.Type == TKTYPE.ADDSUB)
            {
                var t = _token;
                GetToken();
                var exprArg = ParseMulDiv();
                x = new BinaryExpression(t, x, exprArg);
            }
            return x;
        }
        Calculator_Expression ParseMulDiv()
        {
            var x = ParsePower();
            while (_token.Type == TKTYPE.MULDIV)
            {
                var t = _token;
                GetToken();
                var a = ParsePower();
                x = new BinaryExpression(t, x, a);
            }
            return x;
        }
        Calculator_Expression ParsePower()
        {
            var x = ParseOperators();
            while (_token.Type == TKTYPE.POWER)
            {
                var t = _token;
                GetToken();
                var a = ParseOperators();
                x = new BinaryExpression(t, x, a);
            }
            return x;
        }

        Calculator_Expression ParseOperators()
        {
            var x = ParseUnary();
            while (_token.Type == TKTYPE.OPERATORS)
            {
                var t = _token;
                GetToken();
                var a = ParseUnary();
                x = new BinaryExpression(t, x, a);
            }
            return x;
        }

        Calculator_Expression ParseUnary()
        {
            // unary plus and minus
            if (_token.ID == TKID.ADD || _token.ID == TKID.SUB || _token.ID == TKID.NOT || _token.ID == TKID.ABS)
            {
                var t = _token;
                GetToken();
                var a = ParseFoot();
                return new UnaryExpression(t, a);
            }

            // not unary, return atom
            return ParseFoot();
        }

        Calculator_Expression ParseFoot()
        {
            var x = ParseAtom();
            if (_token.ID == TKID.FOOT)
            {
                var t = _token;
                GetToken();
                var a = ParseAtom();
                x = new BinaryExpression(t, x, a);
            }

            return x;
        }

        Calculator_Expression ParseAtom()
        {
            string id;
            Calculator_Expression x = null;
            FunctionDefinition fnDef = null;

            switch (_token.Type)
            {
                // literals
                case TKTYPE.LITERAL:
                    x = new Calculator_Expression(_token);
                    break;

                // identifiers
                case TKTYPE.IDENTIFIER:

                    // get identifier
                    id = (string)_token.Value;

                    // look for functions
                    if (_fnTbl.TryGetValue(id, out fnDef))
                    {
                        var p = GetParameters();
                        var pCnt = p == null ? 0 : p.Count;
                        if (fnDef.ParmMin != -1 && pCnt < fnDef.ParmMin)
                        {
                            Throw($"Too few parameters for function {id}.");
                        }
                        if (fnDef.ParmMax != -1 && pCnt > fnDef.ParmMax)
                        {
                            Throw($"Too many parameters for function {id}.");
                        }
                        x = new FunctionExpression(fnDef, p);
                        break;
                    }

                    if (!_vars.ContainsKey(id)) _vars.Add(id, 1);   // if variable does not exist, add with value 1
                    else _vars[id]++;                               // else increase value by 1. At this point, value counts usage.
                    x = new VariableExpression(_vars, id);
                    break;

                // sub-expressions
                case TKTYPE.GROUP:

                    // anything other than opening parenthesis is illegal here
                    if (_token.ID != TKID.OPEN)
                    {
                        Throw("Expression expected.");
                    }

                    // get expression
                    GetToken();
                    x = ParseOr();

                    // check that the parenthesis was closed
                    if (_token.ID != TKID.CLOSE)
                    {
                        Throw("Unbalanced parenthesis.");
                    }

                    break;

                // logical sub-expressions
                case TKTYPE.LGROUP:
                    if (!isCond)
                    {
                        Throw("Curly brackets are only allowed within condition formulas.");
                    }
                    // anything other than opening parenthesis is illegal here
                    if (_token.ID != TKID.LOPEN)
                    {
                        Throw("Expression expected.");
                    }

                    // get expression
                    GetToken();
                    x = ParseOr();

                    // check that the parenthesis was closed
                    if (_token.ID != TKID.LCLOSE)
                    {
                        Throw("Unbalanced parenthesis.");
                    }

                    break;
            }

            // make sure we got something...
            if (x == null)
            {
                Throw($"Unexpected token '{_token.Value}' found in position {_ptr} of the formula." );
            }

            // done
            GetToken();
            return x;
        }

        #endregion Parsing Functions

        #region Parser

        void GetToken()
        {
            // eat white space 
            while (_ptr < _len && _expr[_ptr] <= ' ')
            {
                _ptr++;
            }

            // are we done?
            if (_ptr >= _len)
            {
                _token = new Token(null, TKID.END, TKTYPE.GROUP);
                return;
            }

            // prepare to parse
            int i;
            var c = _expr[_ptr];

            // operators
            // this gets called a lot, so it's pretty optimized.
            // note that operators must start with non-letter/digit characters.
            var isLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '$';
            var isDigit = c >= '0' && c <= '9';
            if (!isLetter && !isDigit)
            {
                // if this is a number starting with a decimal, don't parse as operator
                var nxt = _ptr + 1 < _len ? _expr[_ptr + 1] : 0;
                bool isNumber = c == _decimal && nxt >= '0' && nxt <= '9';
                if (!isNumber)
                {
                    // look up localized list separator
                    if (c == _listSep)
                    {
                        _token = new Token(c, TKID.COMMA, TKTYPE.GROUP);
                        _ptr++;
                        return;
                    }

                    // look up single-char tokens on table
                    Token tk;
                    if (_tkTbl.TryGetValue(c, out tk))
                    {
                        // save token we found
                        _token = tk;
                        _ptr++;

                        // look for double-char tokens (special case)
                        if (_ptr < _len && (c == '>' || c == '<' || c == '!' || c == '|' || c == '&'))
                        {
                            if (_tkTbl.TryGetValue(_expr.Substring(_ptr - 1, 2), out tk))
                            {
                                _token = tk;
                                _ptr++;
                            }
                            // look for five-char tokens! (<max>, <min>, <abs>)
                            else if (_ptr < (_len - 3) && _tkTbl.TryGetValue(_expr.Substring(_ptr - 1, 5), out tk))
                            {
                                _token = tk;
                                _ptr += 4;
                            }
                        }

                        // found token on the table
                        return;
                    }
                }
            }

            // parse numbers
            if (isDigit || c == _decimal)
            {
                var sci = false;
                var pct = false;
                var div = -1.0; // use double, not int (this may get really big)
                var val = 0.0;
                for (i = 0; i + _ptr < _len; i++)
                {
                    c = _expr[_ptr + i];

                    // digits always OK
                    if (c >= '0' && c <= '9')
                    {
                        val = val * 10 + (c - '0');
                        if (div > -1)
                        {
                            div *= 10;
                        }
                        continue;
                    }

                    // one decimal is OK
                    if (c == _decimal && div < 0)
                    {
                        div = 1;
                        continue;
                    }

                    // scientific notation?
                    if ((c == 'E' || c == 'e') && !sci)
                    {
                        sci = true;
                        c = _expr[_ptr + i + 1];
                        if (c == '+' || c == '-') i++;
                        continue;
                    }

                    // percentage?
                    if (c == _percent)
                    {
                        pct = true;
                        i++;
                        break;
                    }

                    // end of literal
                    break;
                }

                // end of number, get value
                if (!sci)
                {
                    // much faster than ParseDouble
                    if (div > 1)
                    {
                        val /= div;
                    }
                    if (pct)
                    {
                        val /= 100.0;
                    }
                }
                else
                {
                    var lit = _expr.Substring(_ptr, i);
                    val = ParseDouble(lit, _ci);
                }

                // advance pointer
                _ptr += i;

                // build token
                _token = new Token(val, TKID.ATOM, TKTYPE.LITERAL);

                return;
            }

            // parse strings
            if (c == '\"')
            {
                // look for end quote, skip double quotes
                for (i = 1; i + _ptr < _len; i++)
                {
                    c = _expr[_ptr + i];
                    if (c != '\"') continue;
                    char cNext = i + _ptr < _len - 1 ? _expr[_ptr + i + 1] : ' ';
                    if (cNext != '\"') break;
                    i++;
                }

                // check that we got the end of the string
                if (c != '\"')
                {
                    Throw("Can't find final quote.");
                }

                // end of string
                var lit = _expr.Substring(_ptr + 1, i - 1);
                _ptr += i + 1;
                _token = new Token(lit.Replace("\"\"", "\""), TKID.ATOM, TKTYPE.LITERAL);
                return;
            }

            // identifiers (functions, objects) must start with alpha or $
            if (!isLetter && c != '$' && (_idChars == null || _idChars.IndexOf(c) < 0))
            {
                Throw("Identifier expected.");
            }

            // and must contain only letters/digits/_idChars
            for (i = 1; i + _ptr < _len; i++)
            {
                c = _expr[_ptr + i];
                isLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                isDigit = c >= '0' && c <= '9';
                if (!isLetter && !isDigit && (_idChars == null || _idChars.IndexOf(c) < 0))
                {
                    break;
                }
            }

            // got identifier
            var id = _expr.Substring(_ptr, i);
            _ptr += i;
            _token = new Token(id, TKID.ATOM, TKTYPE.IDENTIFIER);
        }

        double ParseDouble(string str, CultureInfo ci)
        {
            if (str.Length > 0 && str[str.Length - 1] == ci.NumberFormat.PercentSymbol[0])
            {
                str = str.Substring(0, str.Length - 1);
                return double.Parse(str, NumberStyles.Any, ci) / 100.0;
            }
            return double.Parse(str, NumberStyles.Any, ci);
        }

        List<Calculator_Expression> GetParameters() // e.g. myfun(a, b, c+2)
        {
            // check whether next token is a (, 
            // restore state and bail if it's not
            var pos = _ptr;
            var tk = _token;
            GetToken();
            if (_token.ID != TKID.OPEN)
            {
                _ptr = pos;
                _token = tk;
                return null;
            }

            // check for empty Parameter list
            pos = _ptr;
            GetToken();
            if (_token.ID == TKID.CLOSE)
            {
                return null;
            }
            _ptr = pos;

            // get Parameters until we reach the end of the list
            var parms = new List<Calculator_Expression>();
            var expr = ParseExpression();
            parms.Add(expr);
            while (_token.ID == TKID.COMMA)
            {
                expr = ParseExpression();
                parms.Add(expr);
            }

            // make sure the list was closed correctly
            if (_token.ID != TKID.CLOSE)
            {
                Throw("List not properly closed");
            }

            // done
            return parms;
        }

        #endregion Parser

        #region ** helpers

        static void Throw()
        {
            Throw("Syntax error.");
        }
        static void Throw(string msg)
        {
            throw new Exception("Formula error: "+msg);
        }

        #endregion

    }


    /// <summary>
    /// Delegate that represents formula functions.
    /// </summary>
    /// <param name="parms">List of <see cref="Calculator_Expression"/> objects that represent the
    /// parameters to be used in the function call.</param>
    /// <returns>The function result.</returns>
    public delegate object CalcEngineFunction(List<Calculator_Expression> parms, Dictionary<string, double> operands);
}
