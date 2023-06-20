namespace EM_Transformer
{
    /// <summary>
    /// class for EM2 XML-tags 
    /// it is important to use only these tags for EM2 reading and not those in EM_XmlHandler.TAGS, even if they are equal
    /// because we want to be free to change EM_XmlHandler.TAGS without jeopardising the EM2->EM3 tranformation
    /// note that the necessary E2->E3 translations are done in EM23Adapt.TranslateTag (called during the write-process)
    /// also note that the scope of this collection is slightly wider than the scope of EM_XmlHandler.TAGS, in the sense that it contains
    /// some outdated EM2 fun- and par-names (e.g. defconst, var_monetary, ...), which would in EM3 be found under EM_Common.Definitions 
    /// </summary>
    public class EM2TAGS
    {
        // GENERAL TAGS
        internal const string ID = "ID";
        internal const string NAME = "Name";
        internal const string VALUE = "Value";

        // COUNTRY FILE
        internal const string COUNTRY = "Country";
        internal const string SYSTEM = "System";
        internal const string POLICY = "Policy";
        internal const string FUNCTION = "Function";
        internal const string PARAMETER = "Parameter";
        internal const string ORDER = "Order";
        internal const string SWITCH = "Switch";
        internal const string UPINDEX = "UpratingIndex";
        internal const string EXSTAT = "ExternalStatistic";
        internal const string REFERENCE = "Reference";
        internal const string COUNTRY_ID = "CountryID";
        internal const string EXCHANGE_RATE_EURO = "ExchangeRateEuro";
        internal const string TYPE = "Type";
        internal const string COLOR = "Color";
        internal const string VALUE_TYPE = "ValueType";
        internal const string YEAR_VALUES = "YearValues";
        internal const string REFPOL_ID = "ReferencePolID";
        internal const string VAR_MONETARY = "var_monetary";
        internal const string CONST_MONETARY = "const_monetary"; // is actually invalid, but exists (e.g. Denmark)
        internal const string VAR_DATASET = "var_dataset";
        internal const string CONST_DATASET = "const_dataset";
        internal const string VAR_SYSTEMYEAR = "var_systemyear";
        internal const string CONST_SYSTEMYEAR = "const_systemyear";
        internal const string PRIVATE = "Private";
        internal const string SYSTEM_ID = "SystemID";
        internal const string POLICY_ID = "PolicyID";
        internal const string FUNCTION_ID = "FunctionID";
        internal const string PARAMETER_ID = "ParameterID";
        internal const string GROUP = "Group";
        internal const string VAR = "var";
        internal const string IL = "il";
        internal const string VARGROUP = "vargroup";
        internal const string ILGROUP = "ilgroup";
        internal const string LOOKGROUP = "LookGroup";
        internal const string LOOKGROUP_POLICY = "LookGroup_Policy";
        internal const string LOOKGROUP_FUNCTION = "LookGroup_Function";
        internal const string LOOKGROUP_PARAMETER = "LookGroup_Parameter";
        internal const string LOOKGROUP_ID = "LookGroupID";
        internal const string EXTENSION = "Extension";
        internal const string EXTENSION_POLICY = "Extension_Policy";
        internal const string EXTENSION_FUNCTION = "Extension_Function";
        internal const string EXTENSION_PARAMETER = "Extension_Parameter";
        internal const string EXTENSION_ID = "ExtensionID";
        internal const string BASEOFF = "BaseOff";
        internal const string SHORTNAME = "ShortName";
        internal const string LOOK = "Look";
        internal const string INDTAX = "IndirectTax";

        // DATA CONFIG FILE
        internal const string DATABASE = "DataBase";
        internal const string SYSCONFIG = "DBSystemConfig";
        internal const string SYSNAME = "SystemName";
        internal const string DATAID = "DataBaseID";
        internal const string POLSWITCH = "PolicySwitch";
        internal const string SWITCHPOLID = "SwitchablePolicyID";
        internal const string USE_COMMON_DEFAULT = "UseCommonDefault";
        internal const string UPRATE = "Uprate";
        internal const string USE_DEFAULT = "UseDefault";

        // GLOBAL FILES
        internal const string EXRATES = "ExchangeRates";
        internal const string HICP = "HICP";
        internal const string SWITCHPOL = "SwitchablePolicy";
        internal const string VALID_FOR = "ValidFor";
        internal const string NAME_PATTERN = "NamePattern";
        internal const string LONG_NAME = "LongName";
        internal const string TAG_PATTERN = "NamePattern";

        // VARIABLES FILE
        internal const string ACRO_TYPE = "AcronymType";
        internal const string ACRO_LEVEL = "AcronymLevel";
        internal const string ACRO = "Acronym";
        internal const string CATEGORY = "Category";
        internal const string ACRO_ID = "AcronymID";
        internal const string VARIABLE = "Variable";
        internal const string COUNTRY_LABEL = "CountryLabel";
        internal const string LABEL = "Label";
        internal const string VAR_ID = "VariableID";
        internal const string AUTO_LABEL = "AutoLabel";
    }
}
