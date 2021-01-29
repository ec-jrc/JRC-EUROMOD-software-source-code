using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class HH
    {
        /// <summary> get the value of a variable for a person </summary>
        /// <param name="varIndex"> index of the varialbe in HH.personVarList </param>
        /// <param name="personIndex"> index of the person in HH, e.g. person 7703 in HH 77 -> 2 (zero base, i.e. 3rd person) </param>
        internal double GetPersonValue(int varIndex, int personIndex)
        {
            return personVarList[personIndex][varIndex];
        }

        /// <summary> set the value of a variable for a person </summary>
        /// <param name="value"> the value to set </param>
        /// <param name="varIndex"> index of the varialbe in HH.personVarList </param>
        /// <param name="personIndex"> index of the person in HH, e.g. person 7703 in HH 77 -> 2 (zero base, i.e. 3rd person) </param>
        /// <param name="addTo"> false: value overwrites current value, true: value is added to current value </param>
        internal void SetPersonValue(double value, int varIndex, int personIndex, bool addTo = false)
        {
            if (!addTo) personVarList[personIndex][varIndex] = value; else personVarList[personIndex][varIndex] += value;
        }

        /// <summary> get the TU-value of a variable, i.e. the sum over all TU-members, used for monetary variables </summary>
        internal double GetTUValue(int varIndex, List<Person> tu)
        {
            double value = 0; foreach (Person p in tu) value += personVarList[p.indexInHH][varIndex]; return value;
        }

        /// <summary>
        /// set the TU-value of a variable, that means:
        /// value is assigned to the head (set or added to)
        /// all other TU-members' variables are set to zero or, if addTo=true, left unchanged
        /// </summary>
        internal void SetTUValue(double value, int varIndex, List<Person> tu, bool addTo = false)
        {
            if (!addTo)
            {
                personVarList[tu[0].indexInHH][varIndex] = value;
                for (int i = 1; i < tu.Count; ++i) personVarList[tu[i].indexInHH][varIndex] = 0;
            }
            else personVarList[tu[0].indexInHH][varIndex] += value;
        }
       
        /// <summary> get the number of persons in the HH /// </summary>
        internal int GetPersonCount() { return personVarList.Count; }
    }
}
