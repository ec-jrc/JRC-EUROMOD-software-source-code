using EM_Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    public partial class Control
    {
        void HandleParModificationsByFile()
        {
            if (!EM_Json.GetFileContent(infoStore.runConfig.pathParModifications,
                out List<Dictionary<string, string>> modifications, out string error)) { CreateParModWarning(error); return; }
            HandleParModifications(modifications);
        }

        private void CreateParModWarning(string msg)
        {
            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            {
                isWarning = true,
                message = $"Failure in interpreting parameter modification file: {msg} (modification is ignored)"
            });
        }

        void HandleParModificationsByString()
        {
            List<Dictionary<string, string>> modifications = (List<Dictionary<string, string>>)JsonConvert.DeserializeObject(infoStore.runConfig.stringParModifications, typeof(List<Dictionary<string, string>>));
            HandleParModifications(modifications);
        }

        void HandleParModifications(List<Dictionary<string, string>> modifications)
        {
            try
            {
                foreach (Dictionary<string, string> modification in modifications)
                {
                    if (!modification.ContainsKey(EM_Json.PARMOD_MODIFICATION_TYPE))
                        { CreateParModWarning($"{EM_Json.PARMOD_MODIFICATION_TYPE} not found"); continue; }

                    string funId, parId, parVal, parGroup; bool found = false;
                    switch (modification[EM_Json.PARMOD_MODIFICATION_TYPE])
                    {
                        case EM_Json.PARMOD_CHANGE_PARAMETER:
                            if (!modification.ContainsKey(EM_Json.PARMOD_FUNCTION_ID) || !modification.ContainsKey(EM_Json.PARMOD_PARAMETER_ID) ||
                                !modification.ContainsKey(EM_Json.PARMOD_PARAMETER_VALUE))
                                { CreateParModWarning($"{modification[EM_Json.PARMOD_MODIFICATION_TYPE]}: insufficient specification"); continue; }

                            funId = modification[EM_Json.PARMOD_FUNCTION_ID];
                            parId = modification[EM_Json.PARMOD_PARAMETER_ID];
                            parVal = EM_Helpers.RemoveWhitespace(modification[EM_Json.PARMOD_PARAMETER_VALUE]);
                            parGroup = modification.ContainsKey(EM_Json.PARMOD_GROUP) ? modification[EM_Json.PARMOD_GROUP] : null;

                            foreach (var pol in infoStore.country.cao.pols.Values)
                            {
                                if (pol.funs.ContainsKey(funId) && pol.funs[funId].pars.ContainsKey(parId))
                                {
                                    pol.funs[funId].pars[parId].val = parVal;
                                    if (parGroup != null) pol.funs[funId].pars[parId].Group = parGroup;
                                    found = true; break;
                                }
                            }

                            if (!found) CreateParModWarning($"Funcion {funId} or parameter {parId} not found");
                            break;

                        case EM_Json.PARMOD_ADD_PARAMETER:
                            if (!modification.ContainsKey(EM_Json.PARMOD_FUNCTION_ID) || !modification.ContainsKey(EM_Json.PARMOD_PARAMETER_NAME) ||
                                !modification.ContainsKey(EM_Json.PARMOD_PARAMETER_VALUE) || !modification.ContainsKey(EM_Json.PARMOD_GROUP))
                                    { CreateParModWarning($"{modification[EM_Json.PARMOD_MODIFICATION_TYPE]}: insufficient specification"); continue; }

                            funId = modification[EM_Json.PARMOD_FUNCTION_ID];
                            parVal = EM_Helpers.RemoveWhitespace(modification[EM_Json.PARMOD_PARAMETER_VALUE]);
                            string parName = modification[EM_Json.PARMOD_PARAMETER_NAME];
                            parGroup = modification[EM_Json.PARMOD_GROUP];

                            foreach (var pol in infoStore.country.cao.pols.Values)
                            {
                                if (pol.funs.ContainsKey(funId))
                                {
                                    pol.funs[funId].pars.Add(Guid.NewGuid().ToString(),
                                        new EM_XmlHandler.ExeXml.Par() { Name = parName, Group = parGroup, val = parVal });
                                    found = true; break;
                                }
                            }

                            if (!found) CreateParModWarning($"Function {funId} not found");
                            break;

                        case EM_Json.PARMOD_DELETE_PARAMETER:
                            if (!modification.ContainsKey(EM_Json.PARMOD_FUNCTION_ID) || !modification.ContainsKey(EM_Json.PARMOD_PARAMETER_ID))
                            { CreateParModWarning($"{modification[EM_Json.PARMOD_MODIFICATION_TYPE]}: insufficient specification"); continue; }

                            funId = modification[EM_Json.PARMOD_FUNCTION_ID];
                            parId = modification[EM_Json.PARMOD_PARAMETER_ID];

                            foreach (var pol in infoStore.country.cao.pols.Values)
                            {
                                if (pol.funs.ContainsKey(funId) && pol.funs[funId].pars.ContainsKey(parId))
                                {
                                    pol.funs[funId].pars.Remove(parId);
                                    found = true; break;
                                }
                            }

                            if (!found) CreateParModWarning($"Funcion {funId} or parameter {parId} not found");
                            break;

                        default: CreateParModWarning($"Unknown {EM_Json.PARMOD_MODIFICATION_TYPE}: {modification[EM_Json.PARMOD_MODIFICATION_TYPE]}"); break;
                    }
                }
            }
            catch (Exception exception) { CreateParModWarning(exception.Message); }
        }
    }
}

/*
 This is how the respective Json would look like:

[
  {
    "ModificationType": "ChangeParameter",
    "FunctionId": D3181097-11C6-465F-A1E6-A30FEA69D710,
    "ParameterId": "DC0EE457-FDBB-424E-AE6F-79AFDDCCE4C6", // increase child-benefit
    "ParameterValue": "400#m"
  },
  {
    "ModificationType": "ChangeParameter",
    "FunctionId": 50BDD632-7DD3-48F8-B374-C07527C1D5C5,
    "ParameterId": "18B73CDE-67A3-4679-8922-DB405C240D98", // increase social assistance for children
    "ParameterValue": "400#m"
  },
  {
    "ModificationType": "AddParameter",
    "FunctionId": "35382965-8E2C-4343-A263-BCBA0248DB7C", // income tax, add second band
    "ParameterName": "band_lowlim",
    "ParameterValue": "4000#m",
    "Group": "2"
  },
  {
    "ModificationType": "AddParameter",
    "FunctionId": "35382965-8E2C-4343-A263-BCBA0248DB7C",
    "ParameterName": "band_rate",
    "ParameterValue": "0.4",
    "Group": "2"
  },
  {
    "ModificationType": "AddParameter",
    "FunctionId": "35382965-8E2C-4343-A263-BCBA0248DB7C", // income tax, add third band
    "ParameterName": "band_lowlim",
    "ParameterValue": "6000#m",
    "Group": "3"
  },
  {
    "ModificationType": "AddParameter",
    "FunctionId": "35382965-8E2C-4343-A263-BCBA0248DB7C",
    "ParameterName": "band_rate",
    "ParameterValue": "0.8",
    "Group": "3"
  },
  {
    "ModificationType": "ChangeParameter",
    "FunctionId": 8A57DA06-021A-4506-A168-15FD0702764B,
    "ParameterId": "83A940BC-1B16-4B97-960A-20149846F96B",
    "ParameterValue": "sl_reform_std"
  }
]
     
 */
