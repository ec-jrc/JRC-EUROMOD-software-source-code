using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;
using System.Linq;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptPolProp(EM2Item pol)
        {
            pol.properties.Remove(EM2TAGS.SYSTEM_ID); // dispensable due to new structure
            pol.properties.Remove(EM2TAGS.TYPE);      // dispensable (sic/ben/tac/etc. is not even used in current UI)
            pol.properties.Remove(EM2TAGS.COLOR);     // only relevant for UI (rethink for future UI)
        }

        // reference policies do not hold information, thus functions and sys-pol can just link the original policy
        // see "explanation wrt new handling of reference policies" in Explanations.cs
        private void GatherRefPol(EM2Item pol, EM2Country.Content ctryContent)
        {
            if (pol.properties.ContainsKey(EM2TAGS.REFPOL_ID) && !string.IsNullOrEmpty(pol.properties[EM2TAGS.REFPOL_ID]))
                ctryContent.referencePolicies.Add(pol.id, pol.properties[EM2TAGS.REFPOL_ID]);
        }

        private void RemoveRefPol(EM2Country.Content ctryContent)
        {   // after remembering the ids (see above) the reference policies can now be remove
            foreach (string rp in ctryContent.referencePolicies.Keys) ctryContent.policies.Remove(rp);
        }

        // get ids of policies in the country that match the pattern of the global switch policy (the rules say it ought to be one, but the old exe seems to work with more than one)
        // e.g. get id of Hungarian policy BTA_hu if sw.switchPolID points to global policy-switch with pattern 'BTA_??'
        // see "explanation wrt new handling of policy switches" in Explanations.cs
        private List<string> GetExtensionPolIDs(string extensionID,
                                                List<List<MultiProp>> extensionInfo,
                                                Dictionary<string, EM2Item> ctryPol)
        {   
            string pattern = null;
            foreach (var ei in extensionInfo)
            {
                MultiProp idProp = (from g in ei where g.tag == EM2TAGS.ID select g).FirstOrDefault();
                if (idProp != null && idProp.content == extensionID)
                {
                    MultiProp patternProp = (from g in ei where g.tag == EM2TAGS.TAG_PATTERN select g).FirstOrDefault();
                    if (patternProp != null) pattern = XmlHelpers.RemoveCData(patternProp.content);
                    break;
                }
            }
            List<string> polIds = new List<string>();
            if (pattern != null) foreach (var pol in ctryPol) if (EM_Helpers.DoesValueMatchPattern(pattern, pol.Value.name)) polIds.Add(pol.Key);
            return polIds;
        }
    }
}
