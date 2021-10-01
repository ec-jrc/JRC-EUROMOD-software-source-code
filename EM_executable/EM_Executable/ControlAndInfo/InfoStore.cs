using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

namespace EM_Executable
{
    /// <summary>
    /// contains all info shared by the different classes of the programme
    /// the single instance of the class is Control.infoStore (see Control_Run.cs)
    /// </summary>
    internal class InfoStore
    {
        internal InfoStore()
        {
            DefinitionAdmin.Init(); // initialise the common-lib's description of functions and parametes (once for the life-time of the lib)
            operandAdmin = new OperandAdmin(this);
        }

        internal Communicator communicator =             // communicater allowing to "talk" to the calling programme,
             new Communicator();                         // i.e. report progress or error
        internal ExeRunConfig runConfig =                // run-configuration
             new ExeRunConfig();                         // (set via config-file or calling options)
        internal ExeXml.Country country = null;          // the country as read by the XMLHandler
        internal OperandAdmin operandAdmin = null;       // administrates the "operands" used in the calculations:
                                                         // variables, incomelists, queries, ...
        internal SortedList<double, FunBase> spine =     // the functions in their run-order
             new SortedList<double, FunBase>();          // (all, even those not supposed to run in spine)
        internal HHAdmin hhAdmin = null;                 // contains and administrates the variables (of persons in households)
                                                         // (simply stated: the programme's data)
        internal double parScaleFactor = 1.0;            // set by FunScale for parameter-scaling (used in ParBase.GetPeriodFactor)
        internal IEnumerable<string> inputData = null;   // stores input data passed directly through memory
        internal Dictionary<string, FunDefTU> indexTUs = // the tax-unit definitions
             new Dictionary<string, FunDefTU>();
        internal Dictionary<string, StringBuilder> output = null;  // to be used only if runConfig.returnOutputInMemory is true, to store the output in memory instead of files

        private double _exRate = -1;  // exchange rate to euro:
        internal double exRate        // make sure that not availability is reported, but only on actual usage
        {
            set { _exRate = value; }
            get
            {
                if (_exRate != -1) return _exRate;
                communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                                         message = $"Missing or faulty definition of exchange rate to Euro: 1 is used as default", runTimeErrorId = "infostore_ex_rate" });
                return 1;
            }
        }

        internal List<Description> applicableUprateFunctions = new List<Description>(); // to check for no/more than one uprating function

        internal List<string> allDataVariables = new List<string>(); // all variables contained in input-data (i.e. the first line of the input-data-file)

        internal void RegisterTU(string name, FunDefTU tu)
        {
            if (!indexTUs.ContainsKey(name)) indexTUs.Add(name, tu);

            // note: double definition may legally happen due to reference policies
            // consider if we want to check for illegal double definition (the old exe does not)
        }

        internal bool IsUsedDatabase(string pattern)
        {
            string usedDB = country.data.Name; if (usedDB.ToLower().EndsWith(".txt")) usedDB = usedDB.Substring(0, usedDB.Length - 4);
            if (pattern.ToLower().EndsWith(".txt")) pattern = pattern.Substring(0, pattern.Length - 4);
            return EM_Helpers.DoesValueMatchPattern(pattern, usedDB);
        }

        internal double GetIDPerson(HH hh, int personIndex)
        {
            return hh.GetPersonValue(operandAdmin.GetIndexInPersonVarList(DefVarName.IDPERSON), personIndex);
        }

        internal double GetIDHH(HH hh)
        {
            return hh.GetPersonValue(operandAdmin.GetIndexInPersonVarList(DefVarName.IDHH), 0);
        }

        // allows for using a Store-variable in a loop "before" the Store is defined
        // example: 22.4 ArithOp: RunCond = { LoopCount_AnyLoop = 3 }, Formula = yem_AnyLoop * 17
        //          25.7 Store: PostLoop = AnyLoop, Var = yem
        //          note that Store comes after ArithOp in spine
        internal bool IsProspectiveStoreOperand(string operand)
        {
            foreach (FunStore funStore in from f in spine.Values where f is FunStore select f)
                if (funStore.IsProspectiveLoopOperand(operand)) return true;
            return false;
        }

        internal void setData(IEnumerable<string> _inputData)
        {
            inputData = _inputData;
        }

        internal bool hasData()
        {
            return inputData != null && inputData.Count() > 1;
        }
    }
}
