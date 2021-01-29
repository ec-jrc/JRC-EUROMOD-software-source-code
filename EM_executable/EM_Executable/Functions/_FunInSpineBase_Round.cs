using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class FunInSpineBase : FunBase
    {
        private enum ROUND { UP, DOWN, TO, NOT };
        private ROUND roundTo = ROUND.NOT;
        private double roundBase;

        protected void PrepareRoundPar()
        {
            ParNumber parRound = GetUniquePar<ParNumber>(DefPar.Common.Round_to);
            if (parRound != null) { roundBase = parRound.GetValue(); roundTo = ROUND.TO; }
            else
            {
                parRound = GetUniquePar<ParNumber>(DefPar.Common.Round_Down);
                if (parRound != null) { roundBase = parRound.GetValue(); roundTo = ROUND.DOWN; }
                else
                {
                    parRound = GetUniquePar<ParNumber>(DefPar.Common.Round_Up);
                    if (parRound != null) { roundBase = parRound.GetValue(); roundTo = ROUND.UP; }
                }
            }
        }
        protected virtual double ApplyRounding(double val, HH hh, List<Person> tu)
        {
            if (roundTo == ROUND.NOT) return val;

            // this is a copy of the old exe's code
            double distance = (val - ((int)(val / roundBase)) * roundBase) / roundBase;
            if (distance <= 0.0000001 || distance >= 0.9999999) return val; // this is necessary because of values like 6.3000000000001 or 6.2999999999999
            if ((distance < .4999999 || roundTo == ROUND.DOWN) && roundTo != ROUND.UP)
                val = ((int)(val / roundBase)) * roundBase; // round to next lower full base
            else val = (((int)(val / roundBase)) + 1) * roundBase; //round to next higher full base
            return val;
        }
    }
}
