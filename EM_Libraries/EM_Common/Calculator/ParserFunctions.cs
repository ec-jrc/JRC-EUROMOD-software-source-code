using System;
using System.Collections.Generic;
using System.Text;

namespace EM_Common
{
    class ParserFunctions
    {
        public static void Register(Calculator fp)
        {
            fp.RegisterFunction("ABS", 1, Abs);
            fp.RegisterFunction("MIN", 2, Min);
            fp.RegisterFunction("MAX", 2, Max);
            fp.RegisterFunction("LN", 1, Ln);
            fp.RegisterFunction("LOG", 1, 2, Log);
            fp.RegisterFunction("LOG10", 1, Log10);
            fp.RegisterFunction("EXP", 1, Exp);
            fp.RegisterFunction("SQRT", 1, Sqrt);
            fp.RegisterFunction("CEILING", 1, Ceiling);
            fp.RegisterFunction("FLOOR", 1, Floor);
            fp.RegisterFunction("ROUND", 1, 2, Round);
            fp.RegisterFunction("POWER", 2, Power);
            fp.RegisterFunction("IF", 3, If);
            /*
            fp.RegisterFunction("ACOS", 1, Acos);
            fp.RegisterFunction("ASIN", 1, Asin);
            fp.RegisterFunction("ATAN", 1, Atan);
            fp.RegisterFunction("ATAN2", 2, Atan2);
            fp.RegisterFunction("COS", 1, Cos);
            fp.RegisterFunction("COSH", 1, Cosh);
            fp.RegisterFunction("INT", 1, Int);
            fp.RegisterFunction("PI", 0, Pi);
            fp.RegisterFunction("RAND", 0, Rand);
            fp.RegisterFunction("RANDBETWEEN", 2, RandBetween);
            fp.RegisterFunction("SIGN", 1, Sign);
            fp.RegisterFunction("SIN", 1, Sin);
            fp.RegisterFunction("SINH", 1, Sinh);
            fp.RegisterFunction("SUM", 1, int.MaxValue, Sum);
            fp.RegisterFunction("SUMIF", 2, 3, SumIf);
            fp.RegisterFunction("TAN", 1, Tan);
            fp.RegisterFunction("TANH", 1, Tanh);
            fp.RegisterFunction("TRUNC", 1, Trunc);
            */
        }

        static object Abs(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Abs(p[0].toDouble(operands));
        }

        static object Min(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Min(p[0].toDouble(operands), p[1].toDouble(operands));
        }

        static object Max(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Max(p[0].toDouble(operands), p[1].toDouble(operands));
        }

        static object Ln(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Log(p[0].toDouble(operands));
        }

        static object Log(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            var lbase = p.Count > 1 ? p[1].toDouble(operands) : 10;
            return Math.Log(p[0].toDouble(operands), lbase);
        }

        static object Log10(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Log10(p[0].toDouble(operands));
        }

        static object Exp(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Exp(p[0].toDouble(operands));
        }

        static object Sqrt(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Sqrt(p[0].toDouble(operands));
        }

        static object Ceiling(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Ceiling(p[0].toDouble(operands));
        }

        static object Floor(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Floor(p[0].toDouble(operands));
        }

        static object Round(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            var digits = p.Count > 1 ? p[1].toDouble(operands) : 0;
            return Math.Round(p[0].toDouble(operands), (int)digits);
        }

        static object Power(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return Math.Pow(p[0].toDouble(operands), p[1].toDouble(operands));
        }

        static object If(List<Calculator_Expression> p, Dictionary<string, double> operands)
        {
            return p[0].toBool(operands) ? p[1].toDouble(operands) : p[2].toDouble(operands);
        }

        /*
        static object Acos(List<Expression> p)
        {
            return Math.Acos((double)p[0]);
        }
        static object Asin(List<Expression> p)
        {
            return Math.Asin((double)p[0]);
        }
        static object Atan(List<Expression> p)
        {
            return Math.Atan((double)p[0]);
        }
        static object Atan2(List<Expression> p)
        {
            return Math.Atan2((double)p[0], (double)p[1]);
        }
        static object Cos(List<Expression> p)
        {
            return Math.Cos((double)p[0]);
        }
        static object Cosh(List<Expression> p)
        {
            return Math.Cosh((double)p[0]);
        }
        static object Int(List<Expression> p)
        {
            return Math.Truncate((double)p[0]);
        }
        static object Pi(List<Expression> p)
        {
            return Math.PI;
        }
        static Random _rnd = new Random();
        static object Rand(List<Expression> p)
        {
            return _rnd.NextDouble();
        }
        static object RandBetween(List<Expression> p)
        {
            return _rnd.Next((int)(double)p[0], (int)(double)p[1]);
        }
        static object Sign(List<Expression> p)
        {
            return Math.Sign((double)p[0]);
        }
        static object Sin(List<Expression> p)
        {
            return Math.Sin((double)p[0]);
        }
        static object Sinh(List<Expression> p)
        {
            return Math.Sinh((double)p[0]);
        }
        static object Sum(List<Expression> p)
        {
            var tally = new Tally();
            foreach (Expression e in p)
            {
                tally.Add(e);
            }
            return tally.Sum();
        }
        static object SumIf(List<Expression> p)
        {
            // get parameters
            IEnumerable range = p[0] as IEnumerable;
            IEnumerable sumRange = p.Count < 3 ? range : p[2] as IEnumerable;
            var criteria = p[1].Evaluate();

            // build list of values in range and sumRange
            var rangeValues = new List<object>();
            foreach (var value in range)
            {
                rangeValues.Add(value);
            }
            var sumRangeValues = new List<object>();
            foreach (var value in sumRange)
            {
                sumRangeValues.Add(value);
            }

            // compute total
            var ce = new CalcEngine();
            var tally = new Tally();
            for (int i = 0; i < Math.Min(rangeValues.Count, sumRangeValues.Count); i++)
            {
                if (ValueSatisfiesCriteria(rangeValues[i], criteria, ce))
                {
                    tally.AddValue(sumRangeValues[i]);
                }
            }

            // done
            return tally.Sum();
        }
        static bool ValueSatisfiesCriteria(object value, object criteria, CalcEngine ce)
        {
            // safety...
            if (value == null)
            {
                return false;
            }

            // if criteria is a number, straight comparison
            if (criteria is double)
            {
                return (double)value == (double)criteria;
            }

            // convert criteria to string
            var cs = criteria as string;
            if (!string.IsNullOrEmpty(cs))
            {
                // if criteria is an expression (e.g. ">20"), use calc engine
                if (cs[0] == '=' || cs[0] == '<' || cs[0] == '>')
                {
                    // build expression
                    var expression = string.Format("{0}{1}", value, cs);

                    // add quotes if necessary
                    var pattern = @"(\w+)(\W+)(\w+)";
                    var m = Regex.Match(expression, pattern);
                    if (m.Groups.Count == 4)
                    {
                        double d;
                        if (!double.TryParse(m.Groups[1].Value, out d) ||
                            !double.TryParse(m.Groups[3].Value, out d))
                        {
                            expression = string.Format("\"{0}\"{1}\"{2}\"",
                                m.Groups[1].Value,
                                m.Groups[2].Value,
                                m.Groups[3].Value);
                        }
                    }

                    // evaluate
                    return (bool)ce.Evaluate(expression);
                }

                // if criteria is a regular expression, use regex
                if (cs.IndexOf('*') > -1)
                {
                    var pattern = cs.Replace(@"\", @"\\");
                    pattern = pattern.Replace(".", @"\");
                    pattern = pattern.Replace("*", ".*");
                    return Regex.IsMatch(value.ToString(), pattern, RegexOptions.IgnoreCase);
                }

                // straight string comparison 
                return string.Equals(value.ToString(), cs, StringComparison.OrdinalIgnoreCase);
            }

            // should never get here?
            System.Diagnostics.Debug.Assert(false, "failed to evaluate criteria in SumIf");
            return false;
        }
        static object Tan(List<Expression> p)
        {
            return Math.Tan((double)p[0]);
        }
        static object Tanh(List<Expression> p)
        {
            return Math.Tanh((double)p[0]);
        }
        static object Trunc(List<Expression> p)
        {
            return (double)Math.Truncate((double)p[0]);
        }
        */
    }
}
