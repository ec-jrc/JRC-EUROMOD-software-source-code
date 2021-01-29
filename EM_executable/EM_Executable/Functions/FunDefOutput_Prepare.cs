using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_Executable
{
    internal partial class FunDefOutput : FunOutOfSpineBase
    {
        string formatDecimals = "";
        protected override void PrepareNonCommonPar()
        {
            ParBase parFileName = GetUniquePar<ParBase>(DefPar.DefOutput.File);
            if (parFileName != null) // if null the programme is stopped after reading parameters (missing compulsory parameter)
            {
                fileName = Path.Combine(infoStore.runConfig.pathOutput, parFileName.xmlValue);
                if (!fileName.ToLower().EndsWith(".txt")) fileName += ".txt";

                if (infoStore.runConfig.stdOutputFilenameSuffix != string.Empty) // e.g. uk_2017_std -> uk_2017_BTAoff
                    AdaptFileNameForNonStdExtensionSwitches(infoStore.runConfig.stdOutputFilenameSuffix);

                if (infoStore.runConfig.outFileDate != string.Empty) // e.g. hu_2017_std.txt -> hu_2017_std_201805301954.txt
                    fileName = fileName.Insert(fileName.Length - 4, "_" + infoStore.runConfig.outFileDate);

                // * * *   T O D O   * * *
                // check at this point if file is ready for writting, i.e. valid file-name, not used by another file, ...
                // to avoid running the whole thing and then cannot write (probably requires try-catch on attempt to open)
                // also if the file is available, lock it now?
            }

            // in future probably replaced by Scale function, but currently still needed, in particular for PET
            ParFormula parMultipleMonetaryBy = GetUniquePar<ParFormula>(DefPar.DefOutput.MultiplyMonetaryBy);
            if (parMultipleMonetaryBy != null)
            {
                multiplyMonetaryBy = parMultipleMonetaryBy.GetGlobalValue();
            }

            PrepareVarILPar();
            PrepareDefILPar();
            PrepareUnitInfoPar();
            append = GetParBoolValueOrDefault(DefFun.DefOutput, DefPar.DefOutput.Append);
            suppressVoidMessage = GetParBoolValueOrDefault(DefFun.DefOutput, DefPar.DefOutput.Suppress_Void_Message);

            nDecimals = (int)GetParNumberValueOrDefault(DefFun.DefOutput, DefPar.DefOutput.nDecimals);
            formatDecimals = "0."; for (int i = 0; i < nDecimals; i++) formatDecimals += "#";

            parWho = GetUniquePar<ParCateg>(DefPar.Common.Who_Must_Be_Elig);
            parEligVar = GetUniquePar<ParVar>(DefPar.Common.Elig_Var);

            ParNumber parReplaceUnitLoopVoidBy = GetUniquePar<ParNumber>(DefPar.DefOutput.Replace_Void_By);
            if (parReplaceUnitLoopVoidBy != null)
            {
                doReplaceUnitLoopVoidBy = true;
                replaceUnitLoopVoidBy = parReplaceUnitLoopVoidBy.GetValue();
            }
        }

        private void PrepareVarILPar()
        {
            foreach (ParVar p in GetNonUniquePar<ParVar>(DefPar.DefOutput.Var)) AddToOutList(p.xmlValue, p.description);
            foreach (ParIL p in GetNonUniquePar<ParIL>(DefPar.DefOutput.IL)) AddToOutList(p.xmlValue, p.description);
            // for each variable or IL that matches the pattern (note: "for each" refers to what's currenlty in the operandAdmin's
            // list of operands, which perfectly takes into account that DefOutput can run in spine, but usually comes last)
            // create an "artifical" ParIL/ParVar-parameter, which needs to be administrated by the fun,
            // e.g. take care for calling CheckAndPrepare, ReplaceVarNameByIndex, ...
            foreach (ParBase pattern in GetNonUniquePar<ParBase>(DefPar.DefOutput.VarGroup))
            {
                foreach (string outName in infoStore.operandAdmin.GetMatchingVar(pattern.xmlValue))
                    AddToOutList(outName, pattern.description);

                // check all variables in the input-data if they match the pattern, and if yes, look if there is an appropriate definition in the var-file
                // we may want to add sth like: if (infoStore.runConfig.ReadInputVariablesOnlyUsedInOutputVargroup)
                foreach (string potOutName in infoStore.allDataVariables)
                {
                    if (infoStore.operandAdmin.indexVarConfig.ContainsKey(potOutName) &&
                        EM_Helpers.DoesValueMatchPattern(pattern.xmlValue, potOutName))
                        AddToOutList(potOutName, pattern.description);
                }
            }
            foreach (ParBase pattern in GetNonUniquePar<ParBase>(DefPar.DefOutput.ILGroup))
                foreach (string outName in infoStore.operandAdmin.GetMatchingIL(pattern.xmlValue))
                    AddToOutList(outName, pattern.description);
        }

        private void PrepareDefILPar()
        {
            foreach (ParIL parIL in GetNonUniquePar<ParIL>(DefPar.DefOutput.DefIL))
            {   // see comment in PrepareOutVar upon creating a ParIL
                // note, that existence of IL and IL-content is checked in the "standard" parameter checking
                foreach (string v in ParIL.GetILComponents(parIL.name, infoStore.operandAdmin))
                    AddToOutList(v, parIL.description);
            }
        }

        private void AddToOutList(string outName, Description description)
        {
            if (outList.ContainsKey(outName)) return; // already in, avoid "double-printing"
            ParVarIL parOut = new ParVarIL(infoStore) { xmlValue = outName, description = description };
            parOut.CheckAndPrepare(this); // note that ParVarIL.CheckAndPrepare a.o. takes care of setting isGlobal, isTULevel
            outList.Add(outName, parOut);
        }

        private void PrepareUnitInfoPar()
        {
            // for each parameter-group UnitInfo_TU, n UnitInfo_Ids
            // create an TUInfo-object that contains info about the concerned TU (e.g. tu_family_cc)
            // and the requested info (IsDepChild, IsDepParent, ...)
            // for (hopefully) enhancing performance requested info is not only prepared as a string here but also as a get-function
            // thus the switch (see below) can be done outside the HH/TU-loop (i.e. once instead of per TU)
            foreach (var group in GetParGroups(DefPar.DefOutput.UnitInfo_XXX).Values)
            {
                TUInfo tuInfoGroup = new TUInfo();
                tuInfoGroup.tuName = GetUniqueGroupPar<ParTU>(DefPar.DefOutput.UnitInfo_TU, group).name;
                Dictionary<string, TUInfo> infos = new Dictionary<string, TUInfo>();
                List<ParCateg> orderedInfos = GetNonUniqueGroupPar<ParCateg>(DefPar.DefOutput.UnitInfo_Id, group).OrderBy(p => int.Parse(p.description.par.order)).ToList();    // try to output in UI order
                foreach (ParCateg info in orderedInfos)
                {
                    switch (info.GetCateg())
                    {
                        case DefPar.Value.UNITINFO_HEADID: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoGetHeadId); break;
                        case DefPar.Value.UNITINFO_ISPARTNER: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsPartner); break;
                        case DefPar.Value.UNITINFO_ISDEPCHILD:
                        case DefPar.Value.UNITINFO_ISDEPENDENTCHILD: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsChild); break;
                        case DefPar.Value.UNITINFO_ISOWNCHILD: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsOwnChild); break;
                        case DefPar.Value.UNITINFO_ISOWNDEPENDENTCHILD:
                        case DefPar.Value.UNITINFO_ISOWNDEPCHILD: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsOwnDepChild); break;
                        case DefPar.Value.UNITINFO_ISDEPPARENT: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsDepParent); break;
                        case DefPar.Value.UNITINFO_ISDEPRELATIVE: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsDepRelative); break;
                        case DefPar.Value.UNITINFO_ISLONEPARENT: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsLoneParent); break;
                        case DefPar.Value.UNITINFO_ISCOHABITING: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsCohabiting); break;
                        case DefPar.Value.UNITINFO_ISWITHPARTNER: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsWithPartner); break;
                        case DefPar.Value.UNITINFO_ISPARENT: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsParent); break;
                        case DefPar.Value.UNITINFO_ISPARENTOFDEPCHILD: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsParentOfDepChild); break;
                        case DefPar.Value.UNITINFO_ISINEDUCATION: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsInEducation); break;
                        case DefPar.Value.UNITINFO_ISLOOSEDEPCHILD: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsLooseDepChild); break;
                        case DefPar.Value.UNITINFO_ISLONEPARENTOFDEPCHILD: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoIsLoneParentOfDepChild); break;
                        case DefPar.Value.UNITINFO_NPERSINUNIT: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoNPersInUnit); break;
                        case DefPar.Value.UNITINFO_NDEPCHILDRENINTU: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoNDepChildrenInTu); break;
                        case DefPar.Value.UNITINFO_NDEPPARENTSINTU: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoNDepParentsInTu); break;
                        case DefPar.Value.UNITINFO_NDEPRELATIVESINTU: tuInfoGroup.getInfos.Add(info.GetCateg(), tuInfoNDepRelativesInTu); break;
                        default: throw new Exception($"{info.description.Get()}: unknown UnitInfo_Id: {info.GetCateg()}");
                    }
                }
                tuInfoGroups.Add(tuInfoGroup);
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (ParVarIL outItem in outList.Values) outItem.ReplaceVarNameByIndex();
            indexIDHH = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPERSON); // may be necessary for assessing UnitInfo_Id=HeadID
            indexEligVar = infoStore.operandAdmin.GetIndexInPersonVarList(parEligVar != null ? parEligVar.name : DefVarName.DEFAULT_ELIGVAR);
        }

        // the run-tool allows renaming of file-name that indicates that an extension is not set to its default-value
        // examples: uk_2017_std -> uk_2017_BTAoff, lv_2018_std_hh -> lv_2018_UAAon_hh
        // this is only done for DefOuput-functions inside the policies output_std_cc and output_std_hh_cc
        // and only if the filenames end with _std respectively _std_hh
        private void AdaptFileNameForNonStdExtensionSwitches(string suffix)
        {
            if (!DefStdOutput.IsOutputStdPol(description.pol.name) && !DefStdOutput.IsOutputStdHHPol(description.pol.name)) return;
            if (!DefStdOutput.IsOutputStdFileName(fileName) && !DefStdOutput.IsOutputStdHHFileName(fileName)) return;
            int i = fileName.LastIndexOf("_std"); fileName = fileName.Remove(i, 4).Insert(i, suffix);
        }

        internal string GetFileName() { return fileName; }
    }
}
