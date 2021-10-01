using EM_Statistics;
using System.Collections.Generic;

namespace InDepthAnalysis
{
    internal interface ISettings
    {
        void UpdateSettings(Settings settings);
        void ModifyTemplate(TemplateApi templateApi, out List<Template.TemplateInfo.UserVariable> systemSpecificVars);
        void ShowDialog();
    }
}
