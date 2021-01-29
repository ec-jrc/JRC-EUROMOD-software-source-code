namespace EM_Common
{
    public class DefFun
    {
        public const string ArithOp = "ArithOp";
        public const string Elig = "Elig";
        public const string BenCalc = "BenCalc";
        public const string SchedCalc = "SchedCalc";
        public const string Min = "Min";
        public const string Max = "Max";
        public const string DefVar = "DefVar";
        public const string DefConst = "DefConst";
        public const string DefIl = "DefIl";
        public const string DefTu = "DefTu";
        public const string UpdateTu = "UpdateTu";
        public const string Uprate = "Uprate";
        public const string DefOutput = "DefOutput";
        public const string Loop = "Loop";
        public const string UnitLoop = "UnitLoop";
        public const string Store = "Store";
        public const string Restore = "Restore";
        public const string IlVarOp = "IlVarOp";
        public const string Totals = "Totals";
        public const string Allocate = "Allocate";
        public const string RandSeed = "RandSeed";
        public const string SetDefault = "SetDefault";
        public const string CallProgramme = "CallProgramme";
        public const string DefInput = "DefInput";
        public const string DropUnit = "DropUnit";
        public const string KeepUnit = "KeepUnit";
        public const string ChangeParam = "ChangeParam";
        public const string ChangeSwitch = "ChangeSwitch"; /* new */
        public const string Scale = "Scale"; /* new */
        public const string AddHHMembers = "AddHHMembers"; /* new */
        public const string Break = "Break"; /* new */
        public const string AddOn_Applic = "AddOn_Applic";
        public const string AddOn_Pol = "AddOn_Pol";
        public const string AddOn_Func = "AddOn_Func";
        public const string AddOn_Par = "AddOn_Par";
        public const string AddOn_ExtensionSwitch = "AddOn_ExtensionSwitch";
        public const string IlArithOp = "IlArithOp";

        /// <summary>
        /// defines the default run-behaviour
        /// for example DefOutput's regular run-option is AFTER_SPINE (see DefinitionAdmin.Fun.defaultRunOption)
        /// but for a specific run there could be a DefOutput with run-option IN_SPINE (see FunBase.runOption),
        /// as a consequence the run must be sequential
        /// note that only changing from BEFORE_SPINE/AFTER_SPINE to IN_SPINE causes a sequential run
        /// (changing from AFTER_SPINE to BEFORE_SPINE would make sense for e.g. Totlas, and does not touch parallel issues)
        /// </summary>
        public enum RUN_MODE
        {
            OUTSIDE_SPINE,  // definput, dropunit, keepunit, defoutput, totals, callprogramme
            IN_SPINE,       // arithop, elig, bencalc, schedcalc, min, max, ilvarop, allocate, store, restore, defvar,
                            // uprate, setdefault, randseed
            NOT_APPLICABLE  // defil, deftu, changeparam, changeswitch, loop, unitloop
        }
    }
}
