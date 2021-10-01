using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace InDepthAnalysis
{
    internal class BaselineReformPackage
    {
        internal class BaselineOrReform
        {
            internal string filePath = null;
            internal string label = string.Empty;
            internal SystemInfo systemInfo = null;
        }

        internal BaselineOrReform baseline = new BaselineOrReform();
        internal List<BaselineOrReform> reforms = new List<BaselineOrReform>();

        private const string XMLTAG_BASELINE = "Baseline";
        private const string XMLTAG_REFORM = "Reform";
        private const string XMLTAG_FILE_PATH = "FilePath";
        private const string XMLTAG_LABEL = "Label";

        private BaselineReformPackage() { }
        internal BaselineReformPackage(FilePackageContent filePackage)
        {
            baseline = new BaselineOrReform() { filePath = filePackage.PathBase };
            foreach (string pathReform in filePackage.PathsAlt) reforms.Add(new BaselineOrReform() { filePath = pathReform });
        }

        internal static BaselineReformPackage FromXml(XElement xElement, out string warnings)
        {
            BaselineReformPackage package = new BaselineReformPackage();
            string _warnings = string.Empty;
            foreach (XElement xe in xElement.Elements())
            {
                if (xe.Value == null) continue;
                switch (Settings.GetXEleName(xe))
                {
                    case XMLTAG_BASELINE: package.baseline = BriFromXml(xe); break;
                    case XMLTAG_REFORM + "s":
                        foreach (XElement xeSub in GetSubElements(xe)) package.reforms.Add(BriFromXml(xeSub));
                        break;
                    default: _warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                }
            }
            warnings = _warnings;
            return package;

            BaselineOrReform BriFromXml(XElement xe)
            {
                BaselineOrReform brf = new BaselineOrReform();
                foreach (XElement xeB in xe.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (Settings.GetXEleName(xeB))
                    {
                        case XMLTAG_FILE_PATH: brf.filePath = xeB.Value; break;
                        case XMLTAG_LABEL: brf.label = xeB.Value; break;
                        default: _warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                    }
                }
                if (string.IsNullOrEmpty(brf.filePath)) throw new Exception($"Setting {Settings.GetXEleName(xe)}: no sub-setting {XMLTAG_FILE_PATH} found.");
                return brf;
            }

            List<XElement> GetSubElements(XElement xe)
            {
                List<XElement> subElements = new List<XElement>();
                foreach (XElement xeSub in xe.Elements())
                {
                    if (xeSub.Value == null) continue;
                    if (Settings.GetXEleName(xeSub) != Settings.GetXEleName(xe).Substring(0, Settings.GetXEleName(xe).Length - 1)) // e.g. xe.Name=ReformFiles, xeSub.Name=ReformFile
                        _warnings += $"Unknown setting {Settings.GetXEleName(xeSub)} is ignored." + Environment.NewLine;
                    else subElements.Add(xeSub);
                }
                return subElements;
            }
        }

        internal void ToXml(XmlWriter xmlWriter)
        {
            if (baseline != null)
            {
                xmlWriter.WriteStartElement(XMLTAG_BASELINE);
                Settings.WriteElement(xmlWriter, XMLTAG_FILE_PATH, baseline.filePath);
                Settings.WriteElement(xmlWriter, XMLTAG_LABEL, baseline.label);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteStartElement(XMLTAG_REFORM + "s");
            foreach (BaselineOrReform reformFile in reforms)
            {
                xmlWriter.WriteStartElement(XMLTAG_REFORM);
                Settings.WriteElement(xmlWriter, XMLTAG_FILE_PATH, reformFile.filePath);
                Settings.WriteElement(xmlWriter, XMLTAG_LABEL, reformFile.label);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        internal bool GetFilePackageContent(out FilePackageContent fpc, out List<string> errors)
        {
            errors = new List<string>(); fpc = new FilePackageContent();
            if (File.Exists(baseline.filePath)) fpc.PathBase = baseline.filePath;
            else errors.Add($"Baseline file {baseline.filePath} does not exist.");
            
            foreach (BaselineOrReform reform in reforms)
            {
                if (File.Exists(reform.filePath)) fpc.PathsAlt.Add(reform.filePath);
                else errors.Add($"Reform file {reform.filePath} does not exist.");
            }
            if (!string.IsNullOrEmpty(fpc.PathBase) && !fpc.PathsAlt.Any())
                errors.Add($"No existing reform files found for baseline {Path.GetFileName(fpc.PathBase)}.");
            return !string.IsNullOrEmpty(fpc.PathBase) && fpc.PathsAlt.Any();
        }

        internal void UpdateSystemInfo(Settings settings, out List<string> errors)
        {
            errors = new List<string>();
            if (baseline != null)
            {
                baseline.systemInfo = SystemInfo.Get(settings, baseline.filePath, out string wb);
                if (!string.IsNullOrEmpty(wb)) errors.Add(wb);
            }
            foreach (BaselineOrReform reform in reforms)
            {
                reform.systemInfo = SystemInfo.Get(settings, reform.filePath, out string wb);
                if (!string.IsNullOrEmpty(wb)) errors.Add(wb);
            }
        }
    }
}
