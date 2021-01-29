using System;
using EM_Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Executable
{
    internal partial class FunDefOutput : FunOutOfSpineBase
    {
        protected override void DoFunWork() 
        {                                   
            if (!IsRunCondMet()) return;

            // assess currency conversion factor
            // remark: unlike for input currency conversion, it seems not possible (?) to efficiently separate conversion
            double convFactor = 1.0;
            if (infoStore.country.sys.isOutputEuro != infoStore.country.sys.areParamEuro)
                convFactor = infoStore.country.sys.isOutputEuro ? 1.0 / infoStore.exRate : infoStore.exRate;

            // in future probably replaced by Scale function, but currently still needed, in particular for PET
            convFactor *= multiplyMonetaryBy;

            // only apply conversion factor if it is not = 1 (to increase speed)
            doExchangeConversion = convFactor != 1;

            if (infoStore.runConfig.returnOutputInMemory)   // if returning the output in memory, put it in a big StringBuilder
            {
                try
                {
                    infoStore.output.Add(fileName, new StringBuilder());
                    // write header row
                    if (!append) infoStore.output[fileName].AppendLine(ProcessHeaders());

                    // if ForceSequentialRun or ForceSequentialOutput, write each household in order to save on memory usage
                    if (infoStore.runConfig.forceSequentialRun || infoStore.runConfig.forceSequentialOutput)
                    {
                        foreach (HH hh in infoStore.hhAdmin.hhs)
                        {
                            StringBuilder hhOutput = ProcessHHoutput(hh, convFactor);
                            if (hhOutput.Length > 0) infoStore.output[fileName].Append(hhOutput);
                        }
                    }
                    else
                    {
                        // else first prepare all HH in parallel
                        ConcurrentDictionary<HH, StringBuilder> allOutput = new ConcurrentDictionary<HH, StringBuilder>();
                        Parallel.ForEach<HH>(infoStore.hhAdmin.hhs, hh =>
                        {
                            StringBuilder hhOutput = ProcessHHoutput(hh, convFactor);
                            if (hhOutput.Length == 0) return;
                            if (!allOutput.TryAdd(hh, hhOutput))
                                throw new Exception($"Error preparing multi-threaded output!"); // this should never happen! (unless a HH is duplicate, which is impossible)
                        });
                        // and then write them to file in sequence
                        foreach (HH hh in infoStore.hhAdmin.hhs)
                            if (allOutput.ContainsKey(hh)) infoStore.output[fileName].Append(allOutput[hh]);
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"Error writing output to {fileName}.{Environment.NewLine}{exception.Message}");
                }
            }
            else    // else, write the output to a real file
            {
                try // note: maybe the extra try/catch is disputable (i.e. one could rely on general try/catch as all other functions instead)
                {   // but IO-problems seem more likely and serious and this will may help to better locate the error
                    using (StreamWriter streamWriter = new StreamWriter(fileName, append, new UTF8Encoding(false)))
                    {
                        // write header row
                        if (!append) streamWriter.WriteLine(ProcessHeaders());

                        // if ForceSequentialRun or ForceSequentialOutput, write each household in order to save on memory usage
                        if (infoStore.runConfig.forceSequentialRun || infoStore.runConfig.forceSequentialOutput)
                        {
                            foreach (HH hh in infoStore.hhAdmin.hhs)
                            {
                                StringBuilder hhOutput = ProcessHHoutput(hh, convFactor);
                                if (hhOutput.Length > 0) streamWriter.Write(hhOutput);
                            }
                        }
                        else
                        {
                            // else first prepare all HH in parallel
                            ConcurrentDictionary<HH, StringBuilder> allOutput = new ConcurrentDictionary<HH, StringBuilder>();
                            Parallel.ForEach<HH>(infoStore.hhAdmin.hhs, hh =>
                            {
                                StringBuilder hhOutput = ProcessHHoutput(hh, convFactor);
                                if (hhOutput.Length == 0) return;
                                if (!allOutput.TryAdd(hh, hhOutput))
                                    throw new Exception($"Error preparing multi-threaded output!"); // this should never happen! (unless a HH is duplicate, which is impossible)
                            });
                            // and then write them to file in sequence
                            foreach (HH hh in infoStore.hhAdmin.hhs)
                                if (allOutput.ContainsKey(hh)) streamWriter.Write(allOutput[hh]);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"Error writing output to {fileName}.{Environment.NewLine}{exception.Message}");
                }
            }
        }

        private string ProcessHeaders()
        {
            StringBuilder headerRow = new StringBuilder();
            List<string> orderedKeys = outList.OrderBy(p => int.Parse(p.Value.description.par.order)).Select(p => p.Key).ToList();    // try to output in UI order
            foreach (string outName in orderedKeys) headerRow.Append(outName).Append("\t");
            foreach (TUInfo tuInfoGroup in tuInfoGroups)
                foreach (string info in tuInfoGroup.getInfos.Keys)
                    headerRow.Append(tuInfoGroup.tuName).Append("_").Append(info).Append("\t"); // e.g. IsDepChild
            foreach (string stringVar in infoStore.operandAdmin.indexStringVars.Keys) headerRow.Append(stringVar).Append("\t");

            if (headerRow.Length > 0) headerRow.Length--;     // trim trailing tab
            return headerRow.ToString();
        }

        private StringBuilder ProcessHHoutput(HH hh, double convFactor)
        {
            StringBuilder hhResults = new StringBuilder();
            foreach (List<Person> tu in hh.GetTUs(coParTU.name))
            {
                if (!IsOutputElig(hh, tu)) continue;

                StringBuilder tuRow = new StringBuilder();


                List<ParVarIL> orderedValues = outList.OrderBy(p => int.Parse(p.Value.description.par.order)).Select(p => p.Value).ToList();    // try to output in UI order
                foreach (ParVarIL parOut in orderedValues)
                {
                    double value = parOut.GetValue(hh, tu);
                    // if value is "VOID", check if you need to replace the value
                    if (value == FunStore.UNITLOOP_VOID) {
                        if (doReplaceUnitLoopVoidBy) value = replaceUnitLoopVoidBy;
                        if (!suppressVoidMessage)
                        {
                            // old exec behaviour:
                            //                      std::string handling = "Zero is used as default value for the not defined variables.";
                            //						if(RepVoidBy) handling = CEMUtilities::DoubleToStr(RepVoidBy) + " is used as default value for the incomelist.";
                            //						if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+* iti+"'.", this, "", "", handling)) return 0;
                            string msg = $"{description.Get()}: VOID value found in '{parOut.GetName()}' for hhid '{infoStore.GetIDHH(hh)}'";
                            if (doReplaceUnitLoopVoidBy) msg += " and replaced by " + value;
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                            { isWarning = true, message = msg, runTimeErrorId = description.funID });
                        }
                    }
                    // if the value was not void, check if you need to apply an exchange rate
                    else if (doExchangeConversion && parOut.isMonetary) value *= convFactor; 
                    // print value 
                    tuRow.Append(value.ToString(formatDecimals));
                    tuRow.Append("\t");
                }
                foreach (TUInfo tuInfoGroup in tuInfoGroups) // see PrepareUnitInfoPar for description of TUInfo.getInfos
                {
                    List<Person> altTU = hh.GetAlternativeTU(tuInfoGroup.tuName, tu, description);

                    // get the status of the DefOutput-TU's head (usually one person-TU) in the UnitInfo-TU
                    Person headInAltTU = // note: head of DefOutput-TU must be member of altTU, otherwise GetAlternativeTU is buggy
                        (from p in altTU where p.indexInHH == tu[0].indexInHH select p).First();
                    foreach (Func<HH, List<Person>, Person, double> getInfo in tuInfoGroup.getInfos.Values)
                        tuRow.Append(getInfo(hh, altTU, headInAltTU)).Append("\t");
                }
                foreach (int indexStringVar in infoStore.operandAdmin.indexStringVars.Values)
                {
                    string value = hh.personStringVarList[tu[0].indexInHH][indexStringVar];
                    // print value 
                    tuRow.Append(EncodeStringForOutput(value));
                    tuRow.Append("\t");
                }
                if (tuRow.Length > 0) tuRow.Length--;     // trim trailing tab
                hhResults.AppendLine(tuRow.ToString());
            }
            return hhResults;
        }

        private static string EncodeStringForOutput(string value)
        {
            // * * *   T O D O   * * *
            // Perform encoding, to make sure that quotes or tabs or other special characters in the string does not break the output file structure
            return value;
        }

        private bool IsOutputElig(HH hh, List<Person> tu)
        {
            if (parWho == null) return true;
            return FunInSpineBase.IsCondMetByTU(hh, tu, indexEligVar, parWho.GetCateg());
        }
    }
}
