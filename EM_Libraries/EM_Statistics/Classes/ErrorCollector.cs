using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace EM_Statistics
{
    public class ErrorCollector
    {
        private List<string> errors = new List<string>();
        private bool debugMode = false;

        internal void AddError(string error) { errors.Add(error); }
        internal void AddDebugOnlyError(string error) { if (debugMode) AddError(error); }

        /// <summary>
        /// this function is called if the XmlElement 'TemplateInfo.DebugMode' exists and is set to true
        /// if the function is not called (i.e. as a default), errors which are probably not relevant (e.g. unknown elements, which do no harm)
        /// are not added to the error-list (neither as critical nor non-critical error)
        /// </summary>
        internal void SetDebugMode() { debugMode = true; }

        public bool HasErrors() { return errors.Count > 0; }

        public string GetErrorMessage()
        {
            return errors.Count == 0 ? string.Empty :
                $"{errors.Count()} error{(errors.Count == 1 ? string.Empty : "s")} found:" +
                Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine + Environment.NewLine, errors);
        }

        public void Clear() { errors.Clear(); }

        internal bool XEleGetBool(XElement xe, XElement xeParent, string nameParent = null)
        {
            if (bool.TryParse(xe.Value, out bool b)) return b; AddXmlTryParseError(xe, xeParent, nameParent); return false;
        }

        internal double XEleGetDouble(XElement xe, XElement xeParent, string nameParent = null)
        {
            if (double.TryParse(xe.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d)) return d; AddXmlTryParseError(xe, xeParent, nameParent); return 0.0;
        }

        internal int XEleGetInt(XElement xe, XElement xeParent, string nameParent = null)
        {
            if (int.TryParse(xe.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int i)) return i; AddXmlTryParseError(xe, xeParent, nameParent); return 0;
        }

        internal T XEleGetEnum<T>(XElement xe, XElement xeParent, string nameParent = null)
        {
            if (HardDefinitions.TryParseEnumType(xe.Value, out T e)) return e; AddXmlTryParseError(xe, xeParent, nameParent); return default(T);
        }

        internal void AddXmlUnkownEleError(XElement xeUnkown, XElement xeParent, string nameParent = null)
        {
            AddXmlError($"unknown element <{xeUnkown.Name}>", xeParent, nameParent, true); // last param, debugOnly=true, means only report unnecessary elements, if the template is still developed
        }

        private void AddXmlTryParseError(XElement xe, XElement xeParent, string nameParent)
        {
            AddXmlError($"<{xe.Name}> has invalid value '{xe.Value}'", xeParent, nameParent);
        }

        private void AddXmlError(string error, XElement xeParent, string nameParent, bool debugOnly = false)
        {
            string err = $"<{xeParent.Name}>{(string.IsNullOrEmpty(nameParent) ? string.Empty : $" '{nameParent}'")}: {error}.";
            if (!debugOnly) AddError(err); // note: Xml-errors are always critical
            else AddDebugOnlyError(err);
        }

        internal void AddXmlMissingPropError(string tag, string ident, string parentTag) { AddXmlMissingPropError(tag, string.IsNullOrEmpty(ident) ? null : new List<string> { ident }, string.IsNullOrEmpty(parentTag) ? null : new List<string> { parentTag }); }
        internal void AddXmlMissingPropError(string tag, List<string> idents, string parentTag) { AddXmlMissingPropError(tag, idents, string.IsNullOrEmpty(parentTag) ? null : new List<string> { parentTag }); }
        internal void AddXmlMissingPropError(string tag, string ident, List<string> parentTags) { AddXmlMissingPropError(tag, string.IsNullOrEmpty(ident) ? null : new List<string> { ident }, parentTags); }
        internal void AddXmlMissingPropError(string tag, List<string> idents, List<string> parentTags)
        {
            AddError($"{ComposeErrorHeader(idents, parentTags)}Missing property <{tag}>.");
        }

        internal void CheckXmlDoubleDefError(IEnumerable<string> defs, string tag, string ident, string parentTag) { CheckXmlDoubleDefError(defs, tag, string.IsNullOrEmpty(ident) ? null : new List<string> { ident }, string.IsNullOrEmpty(parentTag) ? null : new List<string> { parentTag }); }
        internal void CheckXmlDoubleDefError(IEnumerable<string> defs, string tag, List<string> idents, string parentTag) { CheckXmlDoubleDefError(defs, tag, idents, string.IsNullOrEmpty(parentTag) ? null : new List<string> { parentTag }); }
        internal void CheckXmlDoubleDefError(IEnumerable<string> defs, string tag, string ident, List<string> parentTags) { CheckXmlDoubleDefError(defs, tag, string.IsNullOrEmpty(ident) ? null : new List<string> { ident }, parentTags); }
        internal void CheckXmlDoubleDefError(IEnumerable<string> defs, string tag, List<string> idents, List<string> parentTags)
        {
            foreach (string def in defs.Distinct())
                if (!string.IsNullOrEmpty(def) && (from d in defs where d.ToLower() == def.ToLower() select d).Count() > 1)
                    AddError($"{ComposeErrorHeader(idents, parentTags)}Multiple usage of <{tag}> '{def}'.");
        }

        private string ComposeErrorHeader(List<string> idents, List<string> parentTags)
        {
            string spts = string.Empty, sids = string.Empty;
            if (parentTags != null && parentTags.Any())
                foreach (string p in parentTags) if (!string.IsNullOrEmpty(p)) spts += $"<{p}>";
            if (idents != null && idents.Any())
            {
                sids = string.Join("|", from i in idents where !string.IsNullOrEmpty(i) select i);
                if (sids != string.Empty) sids = $"'{sids}'";
            }
            string error = spts + (spts == string.Empty ? string.Empty : " ") + sids;
            return error == string.Empty ? string.Empty : error += ":" + Environment.NewLine;
        }
    }
}
