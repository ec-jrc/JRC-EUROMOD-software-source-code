using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EM_Common
{
    public static class UICommunicator
    {
        // the function CallStaticFunction (and its overload) allows for calling any public static function of the running!!! UI
        // currently the only public functions of the UI are gathered in the class EM_UI.PlugInService.UISessionInfo
        // these functions (providing info about paths, active countries, etc.) are replicated by EM_Common.dll for more convenient calling
        // for this purpose the class EM_Common.UISessionInfo uses the function CallStaticFunction below
        // in principle plug-ins can use CallStaticFunction themselves, provided we extend the UI by public static functions

        public static bool CallStaticFunction(string className, string functionName, out TSObject returnValue, out string errMsg, TSDictionary parameters = null)
        {
            returnValue = new TSObject(); errMsg = string.Empty; // initialise out-parameters
            try
            {
                Assembly assembly = GetUIAssembly();

                // search for class (must be unique match)
                Type classType = null; foreach (Type t in assembly.GetExportedTypes()) if (t.Name == className) classType = t;
                if (classType == null) throw new Exception($"class {className} not found");

                // searching for function is more complicated as there could be overloads
                List<MethodInfo> possibleMatches = new List<MethodInfo>();
                foreach (MethodInfo possibleMatch in classType.GetMethods())
                {
                    if (possibleMatch.Name != functionName) continue;
                    possibleMatches.Add(possibleMatch);
                }
                if (possibleMatches.Count == 0) throw new Exception($"function {functionName} not found");

                // now try calling all overloads of the function to hopefully find a matching one
                foreach (MethodInfo possibleMatch in possibleMatches)
                {
                    try
                    {
                        object[] outParameters = CompileParameterArray(possibleMatch, parameters); // put parameters from dictionary into array with correct order
                        object[] inParameters = new object[outParameters.Count()]; outParameters.CopyTo(inParameters, 0);

                        returnValue.SetValue(possibleMatch.Invoke(classType, outParameters));

                        // gather out- and ref-parameters for appropriate returning
                        if (parameters != null)
                        {
                            parameters.Clear();
                            ParameterInfo[] parameterInfo = possibleMatch.GetParameters();
                            for (int i = 0; i < parameterInfo.Count(); ++i)
                                if (parameterInfo[i].ParameterType.IsByRef || parameterInfo[i].IsOut)
                                    parameters.SetItem(parameterInfo[i].Name, outParameters[i]);
                        }
                        return true;
                    }
                    catch (Exception exception) // remark: try/catch seems the most secure way to check parameter-matching
                    {                           // "manual" check is quite problematic (consider: derived types, numeric parameters (is it int/long/double), etc.)
                        if (exception is TargetParameterCountException || exception is ArgumentException)
                            if (possibleMatch != possibleMatches.Last()) continue; // continue search as there may be an overload with matching parameters
                        throw new Exception("no overload with matching parameters found");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                errMsg = $"Calling UI-function {className}.{functionName} from other assembly failed: {exception.Message}";
                return false;
            }
        }

        private static Assembly GetUIAssembly()
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies != null && assemblies.Count() > 0)
                    foreach (Assembly ass in assemblies)
                        if (ass.FullName.ToLower().StartsWith("em_ui")) return ass;
                throw new Exception(); // no exception-message, because not having no idea what happened
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to access user interface's assembly." + Environment.NewLine + exception.Message);
            }
        }

        private static object[] CompileParameterArray(MethodInfo functionInfo, TSDictionary parameters)
        {
            Dictionary<string, object> parametersToCompile = new Dictionary<string, object>();
            if (parameters != null) // copy dictionary (because content is "cleared" below, but could be necessary for a 2nd call)
            {
                Dictionary<string, object> pCopy = parameters.GetCopyOfContent();
                foreach (string key in pCopy.Keys) parametersToCompile.Add(key, pCopy[key]);
            }
            ParameterInfo[] parameterInfo = functionInfo.GetParameters(); // obtain the info about the correct set of parameters for this function

            object[] paramArray = new object[parameterInfo.Count()];
            string errorReport = string.Empty;
            for (int i = 0; i < parameterInfo.Count(); ++i)
            {
                if (parameterInfo[i].IsOut)
                {
                    paramArray[i] = null; // setting out-parameters to anything else but null does not make sense, the input is anyway ignored
                    parametersToCompile[parameterInfo[i].Name] = Type.Missing; // this is for error handling (to recognise an unknown parameter, see below)
                }
                else if (parametersToCompile.ContainsKey(parameterInfo[i].Name))
                {
                    paramArray[i] = parametersToCompile[parameterInfo[i].Name];
                    parametersToCompile[parameterInfo[i].Name] = Type.Missing; // this is for error handling (to recognise an unknown parameter, see below)
                }
                else if (parameterInfo[i].IsOptional)
                    paramArray[i] = Type.Missing;
                else
                    errorReport += string.Format("{0}Missing parameter '{1}'.", Environment.NewLine, parameterInfo[i].Name);
            }
            foreach (string parameterName in parametersToCompile.Keys)
                if (parametersToCompile[parameterName] != Type.Missing)
                    errorReport += string.Format("{0}Unknown parameter '{1}'.", Environment.NewLine, parameterName);

            if (errorReport != string.Empty) throw new ArgumentException("Parameter mismatch: " + errorReport);
            return paramArray;
        }

        // a more convenient way to pass parameters to CallStaticFunction
        public static TSDictionary ComposeParameters(
            string pName1, object pValue1, string pName2 = "", object pValue2 = null, string pName3 = "", object pValue3 = null,
            string pName4 = "", object pValue4 = null, string pName5 = "", object pValue5 = null, string pName6 = "", object pValue6 = null,
            string pName7 = "", object pValue7 = null, string pName8 = "", object pValue8 = null, string pName9 = "", object pValue9 = null,
            string pName10 = "", object pValue10 = null, string pName11 = "", object pValue11 = null, string pName12 = "", object pValue12 = null,
            string pName13 = "", object pValue13 = null, string pName14 = "", object pValue14 = null, string pName15 = "", object pValue15 = null)
        {
            TSDictionary parameterDic = new TSDictionary();
            if (pName1 != string.Empty) parameterDic.SetItem(pName1, pValue1);
            if (pName2 != string.Empty) parameterDic.SetItem(pName2, pValue2);
            if (pName3 != string.Empty) parameterDic.SetItem(pName3, pValue3);
            if (pName4 != string.Empty) parameterDic.SetItem(pName4, pValue4);
            if (pName5 != string.Empty) parameterDic.SetItem(pName5, pValue5);
            if (pName6 != string.Empty) parameterDic.SetItem(pName6, pValue6);
            if (pName7 != string.Empty) parameterDic.SetItem(pName7, pValue7);
            if (pName8 != string.Empty) parameterDic.SetItem(pName8, pValue8);
            if (pName9 != string.Empty) parameterDic.SetItem(pName9, pValue9);
            if (pName10 != string.Empty) parameterDic.SetItem(pName10, pValue10);
            if (pName11 != string.Empty) parameterDic.SetItem(pName11, pValue11);
            if (pName12 != string.Empty) parameterDic.SetItem(pName12, pValue12);
            if (pName13 != string.Empty) parameterDic.SetItem(pName13, pValue13);
            if (pName14 != string.Empty) parameterDic.SetItem(pName14, pValue14);
            if (pName15 != string.Empty) parameterDic.SetItem(pName15, pValue15);
            return parameterDic;
        }

        public static TSDictionary ComposeParameters(Dictionary<string, object> parameters)
        {
            TSDictionary parameterDic = new TSDictionary();
            if (parameters != null)
                foreach (string key in parameters.Keys)
                    parameterDic.SetItem(key, parameters[key]);
            return parameterDic;
        }
    }
}
