using EM_Common;
using EM_Transformer;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace InDepthAnalysis
{
    // currently probably we could do without this class and just have a list of non-monetary variables (updated where necessary)
    // but this allows for easily upgrade to more extended variables info if necessary in the future (e.g. country specific descriptions, HH-level info, ...)
    internal class EMVarInfo 
    {                       
        internal string pathEuromodFiles = null; // this is just to verify whether info must be updated
        Dictionary<string, bool> variables = null;

        internal bool GetNonMonetaryVariables(Settings settings, string _pathEuromodFiles, out List<string> nonMonetaryVariables)
        {
            nonMonetaryVariables = new List<string>(); List<string> _nonMonetaryVariables = new List<string>();
            if (!Update(_pathEuromodFiles)) return false;
            _nonMonetaryVariables = (from v in variables where !v.Value select v.Key).ToList();
            foreach (BaselineReformPackage brp in settings.baselineReformPackages)
            {
                AddModGen(brp.baseline);
                foreach (BaselineReformPackage.BaselineOrReform reform in brp.reforms) AddModGen(reform);
            }
            nonMonetaryVariables = _nonMonetaryVariables;
            return true;

            void AddModGen(BaselineReformPackage.BaselineOrReform br)
            {
                if (br.systemInfo?.modGenVarInfos != null)
                    foreach (string nmv in from m in br.systemInfo.modGenVarInfos where !m.isMonetary select m.name)
                        if (!_nonMonetaryVariables.Contains(nmv, true)) _nonMonetaryVariables.Add(nmv);
            }
        }

        private bool Update(string _pathEuromodFiles)
        {
            try
            {
                if (variables != null && EMPath.IsSamePath(_pathEuromodFiles, pathEuromodFiles)) return true;
                pathEuromodFiles = _pathEuromodFiles;

                bool success = EM3Variables.Transform(pathEuromodFiles, out List<string> te);
                string errors = te == null || !te.Any() ? string.Empty : string.Join(Environment.NewLine, te);
                if (!success) { MessageBox.Show(errors); return false; }

                Communicator communicator = new Communicator()
                {
                    errorAction = new Action<Communicator.ErrorInfo>(ei =>
                       { errors += $"{(ei.isWarning ? "Warning" : "Error") } assessing EM-variables info: {ei.message}" + Environment.NewLine; })
                };

                variables = ExeXmlReader.ReadVars(new EMPath(pathEuromodFiles).GetVarFilePath(), communicator);

                if (!string.IsNullOrEmpty(errors)) MessageBox.Show(errors);
                if (communicator.errorCount == 0) return true;
                variables = new Dictionary<string, bool>(); return false;
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error assessing EM-variables info: {exception.Message}");
                variables = new Dictionary<string, bool>(); return false;
            }
        }
    }
}
