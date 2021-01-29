using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace EM_Common
{
    /// <summary>
	/// Base class that represents parsed expressions.
	/// </summary>
    /// <remarks>
    /// For example:
    /// <code>
    /// Expression expr = scriptEngine.Parse(strExpression);
    /// object val = expr.Evaluate();
    /// </code>
    /// </remarks>
	public class Calculator_Expression : IComparable<Calculator_Expression>
    {
        //---------------------------------------------------------------------------
        #region ** fields

        internal Token _token;
        static CultureInfo _ci = CultureInfo.InvariantCulture;

        #endregion

        //---------------------------------------------------------------------------
        #region ** ctors

        internal Calculator_Expression()
        {
            _token = new Token(null, TKID.ATOM, TKTYPE.IDENTIFIER);
        }

        internal Calculator_Expression(object value)
        {
            _token = new Token(value, TKID.ATOM, TKTYPE.LITERAL);
        }

        internal Calculator_Expression(Token tk)
        {
            _token = tk;
        }

        #endregion

        //---------------------------------------------------------------------------
        #region ** object model

        public virtual object Evaluate(Dictionary<string, double> operands)
        {
            if (_token.Type != TKTYPE.LITERAL)
            {
                throw new ArgumentException("Bad expression.");
            }
            return _token.Value;
        }

        public virtual Calculator_Expression Optimize()
        {
            return this;
        }

        #endregion

        //---------------------------------------------------------------------------
        #region ** implicit converters

        public static implicit operator string(Calculator_Expression x)
        {
            var v = x.Evaluate(null);
            return v == null ? string.Empty : v.ToString();
        }

        public double toDouble(Dictionary<string, double> operands)

//        public static implicit operator double(Expression x)
        {
            // evaluate
            var v = this.Evaluate(operands);

            // handle doubles
            if (v is double)
            {
                return (double)v;
            }

            // handle booleans
            if (v is bool)
            {
                return (bool)v ? 1 : 0;
            }

            // handle dates
            if (v is DateTime)
            {
                return ((DateTime)v).ToOADate();
            }

            // handle nulls
            if (v == null)
            {
                return 0;
            }

            // handle everything else
            return (double)Convert.ChangeType(v, typeof(double), _ci);
        }

        public bool toBool(Dictionary<string, double> operands)
//        public static implicit operator bool(Expression x)
        {
            // evaluate
            var v = this.Evaluate(operands);

            // handle booleans
            if (v is bool)
            {
                return (bool)v;
            }

            // handle nulls
            if (v == null)
            {
                return false;
            }

            // handle doubles
            if (v is double)
            {
                return (double)v == 0 || Double.IsNaN((double)v) ? false : true;
            }

            // handle everything else
            return this.toDouble(operands) == 0 ? false : true;
        }

        /*        public static implicit operator DateTime(Expression x)
                {
                    // evaluate
                    var v = x.Evaluate(null);

                    // handle dates
                    if (v is DateTime)
                    {
                        return (DateTime)v;
                    }

                    // handle doubles
                    if (v is double)
                    {
                        return DateTime.FromOADate((double)x);
                    }

                    // handle everything else
                    return (DateTime)Convert.ChangeType(v, typeof(DateTime), _ci);
                }
        */
        #endregion

        //---------------------------------------------------------------------------
        #region ** IComparable<Expression>

        public int CompareTo(Calculator_Expression other)
        {
            return 0;
        }
        public int CompareTo(Calculator_Expression other, Dictionary<string, double> operands)
        {
            // get both values
            var c1 = this.Evaluate(operands) as IComparable;
            var c2 = other.Evaluate(operands) as IComparable;

            // handle nulls
            if (c1 == null && c2 == null)
            {
                return 0;
            }
            if (c2 == null)
            {
                return -1;
            }
            if (c1 == null)
            {
                return +1;
            }

            // make sure types are the same
            if (c1.GetType() != c2.GetType())
            {
                c2 = Convert.ChangeType(c2, c1.GetType()) as IComparable;
            }

            // capture the case of either operand being NaN (should always return a "false" comparison)
            if (c1 is double && Double.IsNaN((double)c1) || Double.IsNaN((double)c2)) return int.MinValue;

            // compare
            return c1.CompareTo(c2);
        }

        #endregion
    }
    /// <summary>
    /// Unary expression, e.g. +123
    /// </summary>
	class UnaryExpression : Calculator_Expression
    {
        // ** fields
        internal Calculator_Expression _expr;

        // ** ctor
        public UnaryExpression(Token tk, Calculator_Expression expr) : base(tk)
        {
            _expr = expr;
        }

        // ** object model
        override public object Evaluate(Dictionary<string, double> operands)
        {
            switch (_token.ID)
            {
                case TKID.ADD:
                    return +_expr.toDouble(operands);
                case TKID.SUB:
                    return -_expr.toDouble(operands);
                case TKID.ABS:
                    return Math.Abs(_expr.toDouble(operands));
                case TKID.NOT:
                    return !_expr.toBool(operands);
            }
            throw new ArgumentException("Bad expression.");
        }

        public override Calculator_Expression Optimize()
        {
            _expr = _expr.Optimize();
            return _expr._token.Type == TKTYPE.LITERAL
                ? new Calculator_Expression(this.Evaluate(null))
                : this;
        }
    }
    /// <summary>
    /// Binary expression, e.g. 1+2
    /// </summary>
	class BinaryExpression : Calculator_Expression
    {
        // ** fields
        internal Calculator_Expression _lft;
        internal Calculator_Expression _rgt;

        // ** ctor
        public BinaryExpression(Token tk, Calculator_Expression exprLeft, Calculator_Expression exprRight) : base(tk)
        {
            _lft = exprLeft;
            _rgt = exprRight;
        }

        // ** object model
        override public object Evaluate(Dictionary<string, double> operands)
        {
            // handle comparisons
            if (_token.Type == TKTYPE.COMPARE)
            {
                var cmp = _lft.CompareTo(_rgt, operands);
                if (cmp == int.MinValue) return false;  // one of the operands was NaN
                switch (_token.ID)
                {
                    case TKID.GT: return cmp > 0;
                    case TKID.LT: return cmp < 0;
                    case TKID.GE: return cmp >= 0;
                    case TKID.LE: return cmp <= 0;
                    case TKID.EQ: return cmp == 0;
                    case TKID.NE: return cmp != 0;
                }
            }

            // handle everything else
            switch (_token.ID)
            {
                case TKID.ADD:
                    return _lft.toDouble(operands) + _rgt.toDouble(operands);
                case TKID.SUB:
                    return _lft.toDouble(operands) - _rgt.toDouble(operands);
                case TKID.MUL:
                    return _lft.toDouble(operands) * _rgt.toDouble(operands);
                case TKID.DIV:
                    double r = _rgt.toDouble(operands);
                    if (r == 0) return double.NaN;  // division by 0
                    else return _lft.toDouble(operands) / r;
                case TKID.DIVINT:
                    return (double)(int)(_lft.toDouble(operands) / _rgt.toDouble(operands));
                case TKID.MOD:
                    return (double)(int)(_lft.toDouble(operands) % _rgt.toDouble(operands));
                case TKID.POWER:
                    var a = _lft.toDouble(operands);
                    var b = _rgt.toDouble(operands);
                    if (b == 0.0) return 1.0;
                    if (b == 0.5) return Math.Sqrt(a);
                    if (b == 1.0) return a;
                    if (b == 2.0) return a * a;
                    if (b == 3.0) return a * a * a;
                    if (b == 4.0) return a * a * a * a;
                    return Math.Pow(_lft.toDouble(operands), _rgt.toDouble(operands));
                case TKID.MAX:
                    return Math.Max(_lft.toDouble(operands), _rgt.toDouble(operands));
                case TKID.MIN:
                    return Math.Min(_lft.toDouble(operands), _rgt.toDouble(operands));
                case TKID.AND:
                    return _lft.toBool(operands) && _rgt.toBool(operands);
                case TKID.OR:
                    return _lft.toBool(operands) || _rgt.toBool(operands);
            }
            throw new ArgumentException("Bad expression.");
        }

        public override Calculator_Expression Optimize()
        {
            _lft = _lft.Optimize();
            _rgt = _rgt.Optimize();
            return _lft._token.Type == TKTYPE.LITERAL && _rgt._token.Type == TKTYPE.LITERAL
                ? new Calculator_Expression(this.Evaluate(null))
                : this;
        }
    }

    /// <summary>
    /// Function call expression, e.g. sin(0.5)
    /// </summary>
    class FunctionExpression : Calculator_Expression
    {
        // ** fields
        FunctionDefinition _fn;
        internal List<Calculator_Expression> _parms;

        // ** ctor
        internal FunctionExpression()
        {
        }

        public FunctionExpression(FunctionDefinition function, List<Calculator_Expression> parms)
        {
            _fn = function;
            _parms = parms;
        }

        // ** object model
        override public object Evaluate(Dictionary<string, double> operands)
        {
            return _fn.Function(_parms, operands);
        }

        public override Calculator_Expression Optimize()
        {
            bool allLits = true;
            if (_parms != null)
            {
                for (int i = 0; i < _parms.Count; i++)
                {
                    var p = _parms[i].Optimize();
                    _parms[i] = p;
                    if (p._token.Type != TKTYPE.LITERAL)
                    {
                        allLits = false;
                    }
                }
            }
            return allLits
                ? new Calculator_Expression(this.Evaluate(null))
                : this;
        }
    }

    /// <summary>
    /// Simple variable reference.
    /// </summary>
    class VariableExpression : Calculator_Expression
    {
        IDictionary<string, double> _dct;
        internal string _name;

        public VariableExpression(IDictionary<string, double> dct, string name)
        {
            _dct = dct;
            _name = name;
        }

        public override object Evaluate(Dictionary<string, double> operands)
        {
            return operands[_name];
        }
    }

    public interface IValueObject
    {
        object GetValue();
    }
}
