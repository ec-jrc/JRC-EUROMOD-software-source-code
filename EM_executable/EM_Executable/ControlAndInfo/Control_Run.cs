using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    public partial class Control
    {
        /// <summary> contains all info shared by the different classes of the programme </summary>
        internal InfoStore infoStore = new InfoStore();

        /// <summary>
        /// runs the executable (after configuration is set by either "exe"- or "lib"-call)
        /// *** NOTE ON ERROR HANDLING ***
        /// each of the called functions is expected to handle errors as follows:
        /// - critical error (further programme execution is not possible or does not make sense): throw informative(!) exception
        /// - errors and warnings: use Communicator.ReportError (amongst others indicating whether it is an error or a warning)
        /// thus the caller gets the errors and warnings and can handle them as it wants (issue in a box, put to an error-log-file, ...)
        /// in addition the errors are counted by the Communicator, allowing the Run-function to stop, e.g. after reading, if there are errors
        /// </summary>
        internal bool Run()
        {
            EM_Executable_Test.infoStore = infoStore; // initialise the testing-class
            infoStore.communicator.maxRunTimeErrors = infoStore.runConfig.maxRunTimeErrors;
            // if output should be in memory, initiate the output dictionary
            if (infoStore.runConfig.returnOutputInMemory) infoStore.output = new Dictionary<string, System.Text.StringBuilder>();

            // READ COUNTRY FILE
            // note on case-sensitivity: parameter values are converted to lower-case, everything else is original-case (to keep open for more case-sensitivity in future)
            infoStore.country = ExeXmlReader.ReadCountry(
                infoStore.runConfig.pathHandler.GetCountryFilePath(infoStore.runConfig.countryShortName),
                infoStore.runConfig.sysID, infoStore.runConfig.dataID, // ids (or names) of system and data
                infoStore.runConfig.ignorePrivate, // ignore (do not read) private policies/functions/parameters
                infoStore.communicator); // see note on error-handling above
            if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_PAR_READ,
                 message = $"{infoStore.country.sys.name.ToUpper()} parameters read", detailedInfo = ComposeRunInfo() })) return false;

            // READ VARIABLES FILE (this is a prerequisite for formula interpretation)
            // note on case-sensitivity: the indexVarConfig-dictionary has a case-insensitive key-comparer
            infoStore.operandAdmin.indexVarConfig = ExeXmlReader.ReadVars(
                infoStore.runConfig.pathHandler.GetVarFilePath(),
                infoStore.communicator);
            if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_VAR_READ,
                message = $"{infoStore.operandAdmin.indexVarConfig.Count} definitions read from the Variables file" })) return false;

            // READ OTHER GLOBAL FILES
            infoStore.exRate = ExeXmlReader.ReadExRate( // note: returns -1 if not available (which may be ok if not used)
                infoStore.runConfig.pathHandler.GetExRatesFilePath(),
                infoStore.country.cao.shortName,
                infoStore.country.sys.name, // name of system allows selecting the suitable exchange rate
                infoStore.runConfig.dateExchangeRate, // e.g. TAGS.EXRATE_JUNE30 or TAGS.DEFAULT ...
                infoStore.communicator);

            // note on case-sensitivity: the upFacs-dictionary has a case-insensitive key-comparer
            ExeXml.UpIndDict hicp = ExeXmlReader.ReadHICP( // note: returns hicp.data/hicp.sys=null if not available (which may be ok if not used)
                infoStore.runConfig.pathHandler.GetHICPFilePath(),
                infoStore.country.cao.shortName,
                infoStore.country.sys.year,   // ids of system and data allow returning  
                infoStore.country.data.year,  // the relevant values
                infoStore.communicator);
            if (infoStore.country.upFacs.ContainsKey(DefPar.Value.HICP)) infoStore.country.upFacs[DefPar.Value.HICP] = hicp;
            else infoStore.country.upFacs.Add(DefPar.Value.HICP, hicp);
            if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_GLOB_PAR_READ,
                 message = $"Global parameters read" })) return false;

            // MAKE UPRATING FACTORS AVAILABLE AS GLOBAL VARIABLES, 1st step: generation (to be filled with values once data is read)
            foreach (var upFac in infoStore.country.upFacs) infoStore.operandAdmin.RegisterVar(upFac.Key.ToLower(),
                DefFun.DefConst, // put DefConst as "creator" mainly to allow overwritting by DefConst (done in CZ), but can be justified by it was like this in the old exe
                null, false, true, false, true);

            // MAKE INDIRECT TAXES AVAILABLE AS GLOBAL VARIABLES, 1st step (same procedure as for uprating factors)
            foreach (var indTax in infoStore.country.indTaxes) infoStore.operandAdmin.RegisterVar(indTax.Key.ToLower(), "IndirectTaxTable", null, false, true, false, true);

            // IF APPROPRIATE, READ ADD-ONS
            // note: read here to have extension switches (AddOn_ExtensionSwitch) at disposal, as they overwrite any other switches
            // even those set via run-tool, justification: the add-on creator probably knows why she wants sth on or off
            // add-ons are still integrated after "switching" (i.e. split from reading) as there may be still (old) "manual" switching (via ChangeParam)
            List<ExeXml.AddOn> xmlAddOns = new List<ExeXml.AddOn>();
            if (infoStore.runConfig.addOns.Count > 0)
            {
                foreach (var addOn in infoStore.runConfig.addOns)
                    xmlAddOns.Add(ExeXmlReader.ReadAddOn(
                        path: infoStore.runConfig.pathHandler.GetAddOnFilePath(addOn.Key),
                        addOnSysIdentifier: addOn.Value,
                        communicator: infoStore.communicator));
                if (infoStore.communicator.errorCount > 0) return false;
                if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_ADDON_READ,
                     message = $"Add-on info read" })) return false;
            }

            // if the "All Output in €" box is checked in the run-tool
            if (infoStore.runConfig.forceOutputEuro) infoStore.country.sys.isOutputEuro = true;

            // APPLY GLOBAL SWITCHES IN COMBINATION WITH ADD-ON- AND CONFIG-SETTINGS
            HandleExtensionSwitches(xmlAddOns);

            // IF APPROPRIATE, INTEGRATE ADD-ON
            if (xmlAddOns.Count > 0)
            {
                IntegrateAddOns(xmlAddOns);
                if (infoStore.communicator.errorCount > 0) return false;
                if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_ADDON_INTEG,
                     message = $"Add-on info integrated" })) return false;
            }

            // IF APPROPRIATE, HANDLE PARAMETER MODIFICATIONS VIA JSON-FILE
            if (!string.IsNullOrEmpty(infoStore.runConfig.pathParModifications)) HandleParModificationsByFile();

            // IF APPROPRIATE, HANDLE PARAMETER MODIFICATIONS VIA JSON-STRING
            if (!string.IsNullOrEmpty(infoStore.runConfig.stringParModifications)) HandleParModificationsByString();

            // IF APPROPRIATE, REPLACE =cc= AND =sys= BY COUNTRY-SHORTNAME RESPECTIVELY SYSTEM-NAME
            ReplaceCCSys(infoStore.country.cao.pols);

            // IF APPROPRIATE, APPLY ChangeParam AND ChangeSwitch
            ApplyChangeParam(); ApplyChangeSwitch();

            // SWITCH OFF SetDefaults AND Uprates IF THEY DO NOT APPLY TO THE CURRENT DATASET
            SwitchOffUprateSetDefault();

            // IF APPROPRIATE, handle break function
            if (!HandleBreak()) return false; // stop if the break is misdefined

            // DROP EVERYTHING THAT IS OFF OR N/A (with the intention to enhance run-time-speed by not having to ask for it)
            DropOff();

            // GET VARIABLES CONTAINED IN INPUT DATA (we need to do this upfront as the info is required for addressing variables with wildcards)
            HHAdmin.GetAllDataVariables(infoStore);

            // IF APPROPRIATE, REGISTER EXPENDITURE VARIABLES
            if (infoStore.country.data.readXVar) RegisterXVar(infoStore.allDataVariables);

            // IF APPROPRIATE, REGISTER STRING VARIABLES IN INPUT DATA, WHICH ARE TO BE TRANSFERRED TO OUTPUT
            RegisterOutputStringVar();

            // CHECK AND ANALYSE THE OBTAINED INFO AND PREPARE FOR RUN
            CheckAndPrepare();

            // check for availability of applicable Uprate functions
            if (infoStore.applicableUprateFunctions.Count != 1)
            {
                string m = string.Empty;
                foreach (Description d in infoStore.applicableUprateFunctions) m += d.Get() + " & ";
                if (m == string.Empty) m = $"No applicable {DefFun.Uprate} function found";
                else m = $"More than one applicable {DefFun.Uprate} function found: " + m.TrimEnd(new char[] { '&', ' ' });
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, message = m });
            }

            // ERROR-ASSESSMENT
            if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_PAR_CHECKED,
                 message = $"Parameters checked and prepared", detailedInfo = ComposeOutputInfo() })) return false;
            if (infoStore.communicator.errorCount > 0) return false;

            // GENERATION OF HOUSEHOLD DATA (READING AND PREPARING VARIABLES)
            infoStore.hhAdmin = new HHAdmin(infoStore);
            infoStore.hhAdmin.GenerateData();

            // MAKE UPRATING FACTORS AVAILABLE AS GLOBAL VARIABLES, 2nd step: fill with values
            // NOTE: Uprating factors will have the default system year value. The DBYearVar is not supported in this case! 
            // ( * T O D O * maybe we can support it with the use of conditional constants?)
            // * T O D O * this uprating factor logic shouldn't be here! especially as there is no warning if 1 is used
            foreach (var upFac in infoStore.country.upFacs)
                if (upFac.Value.Get(infoStore.country.data.year, out double dui) && upFac.Value.Get(infoStore.country.sys.year, out double sui))
                    infoStore.hhAdmin.GlobalSetVar(infoStore.operandAdmin.GetIndexInPersonVarList(upFac.Key.ToLower()), sui / dui);
                else
                    infoStore.hhAdmin.GlobalSetVar(infoStore.operandAdmin.GetIndexInPersonVarList(upFac.Key.ToLower()), 1);

            // MAKE INDIRECT TAXES AVAILABLE AS GLOBAL VARIABLES, 2nd step: fill with values
            foreach (var indTax in infoStore.country.indTaxes)
                infoStore.hhAdmin.GlobalSetVar(infoStore.operandAdmin.GetIndexInPersonVarList(indTax.Key.ToLower()), indTax.Value);

            foreach (FunBase fun in infoStore.spine.Values) // before reading data the HH.personVarList does not exist and var-parameters
                fun.ReplaceVarNameByIndex();                // cannot know their index inthere, now the index is available and must be spread

            // IF NECESSARY, CURRENCY CONVERSION
            // note: in terms of performance it seems to be irrelevant if this conversation is integrating into the reading data process
            // thus this "separate approach" seems appropriate as it is clearer
            if (infoStore.country.data.isEuro != infoStore.country.sys.areParamEuro)
            {
                double factor = infoStore.exRate;
                if (infoStore.country.sys.areParamEuro) factor = 1 / factor;
                if (factor != 1) infoStore.hhAdmin.GlobalScaleFileReadVars(factor);
            }

            // ERROR-ASSESSMENT
            if (infoStore.communicator.errorCount > 0) return false;
            if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_HH_READ,
                message = $"{infoStore.country.data.Name}: {infoStore.hhAdmin.hhs[0].personVarList[0].Count:N0} variables for {infoStore.hhAdmin.hhs.Count:N0} households ({infoStore.hhAdmin.hhs.Sum(x => x.GetPersonCount()):N0} individuals) read - largest household had {infoStore.hhAdmin.hhs.Max(x => x.GetPersonCount()):N0} members" })) return false;

            // PREPARE THE SPINE (what to run before/after/in spine and, if necessary, looping)
            SpineAdmin spineManager = new SpineAdmin(infoStore);
            bool runSequential = infoStore.runConfig.forceSequentialRun || infoStore.runConfig.forceAutoSequentialRun;
            spineManager.PrepareSpine(ref runSequential);

            // ERROR-ASSESSMENT
            if (infoStore.communicator.errorCount > 0) return false;
            if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_SPINE_DONE,
                 message = $"Sequence of calculations prepared" })) return false;

            // RUN THE WHOLE THING
            if (runSequential) { if (!RunSequential(spineManager)) return false; } // only if e.g. Totals is used or DefOutput is used in spine, ...
            else RunParallel(spineManager);
            infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_SPINE_DONE,
                message = $"Finished calculations" });

            // last ERROR-ASSESSMENT
            infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { id = Communicator.EXEPROG_FINISHED, // hopefully with 0 warnings
                message = $"FINISHED with {infoStore.communicator.warningCount + infoStore.communicator.errorCount} errors/warnings" });

            return true;
        }
    }
}
