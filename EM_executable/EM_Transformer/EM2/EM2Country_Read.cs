using EM_Common;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace EM_Transformer
{
    public partial class EM2Country
    {
        /// <summary> reads an EM2 country-XML-file into an EM2Country.Content structure </summary>
        /// <param name="fileName"> full path to the country-XML-file </param>
        /// <param name="errors"> stores critical and non-critical erros during the read-process, empty structure for no errors </param>
        /// <returns> Content structure on success, null on failure </returns>
        public static Content Read(string fileName, out List<string> errors)
        {
            Content content = new Content();
            errors = new List<string>();
            try
            {
                using (StreamReader streamReader = new StreamReader(fileName, DefGeneral.DEFAULT_ENCODING))
                using (XmlReader mainReader = XmlReader.Create(streamReader))
                {
                    bool firstSystem = true;
                    // structures which allow assigning the system-dependent values (order, switch/value) of
                    // EM2-policies/functions/parameters to the single remaining EM3-policy/function/parameter
                    // dic-key: policy-order[°function-order][°parameter-order]
                    // dic-value: the id of the remaining policy/function/parameter
                    Dictionary<string, string> polRefs = new Dictionary<string, string>();
                    Dictionary<string, string> funRefs = new Dictionary<string, string>();
                    Dictionary<string, string> parRefs = new Dictionary<string, string>();
                    // reference for all (old) ids, i.e. key = old id, value = new id, 
                    // for (after all reading is done) adapting parameter values which contain ids (e.g. Loop/First_Func, ChangeParam/Param_Id)
                    Dictionary<string, string> guidRef = new Dictionary<string, string>();

                    // *** READ COUNTRY ***
                    mainReader.ReadToDescendant(EM2TAGS.COUNTRY);
                    XmlReader countryReader = mainReader.ReadSubtree(); countryReader.ReadToDescendant(EM2TAGS.ID);
                    do
                    {
                        switch (GetElementType(countryReader)) // analyse the current tag: does it still belong to country
                        {                                      // or is the the first system
                            case ELEMENT_TYPE.NO: continue; // only XmlNodeType.Element is relevant
                            case ELEMENT_TYPE.PROPERTY: ReadProperty(countryReader, ref content.general); continue; // read country-properties (name, shortname)
                            case ELEMENT_TYPE.SUBTREE: break; // system-tag (for better readability not coded inside the switch-structure)
                            default: continue;
                        }

                        // *** READ SYSTEM ***
                        EM2Item sysProperties = new EM2Item(); // takes the system-properties (name, year, currency, ...)
                        XmlReader sysReader = countryReader.ReadSubtree(); sysReader.ReadToDescendant(EM2TAGS.ID);
                        do
                        {
                            switch (GetElementType(sysReader, true))
                            {
                                case ELEMENT_TYPE.NO: continue;
                                case ELEMENT_TYPE.PROPERTY: ReadProperty(sysReader, ref sysProperties); continue; // reads system-properties (name, year, currency, ...)
                                case ELEMENT_TYPE.SUBTREE: break; // policy-tag (for better readability not coded inside the switch-structure)
                                default: continue;
                            }

                            // *** READ POLICY ***
                            EM2Item polProperties = new EM2Item(); // takes the policy-properties (name, reference-policy, ...), is stored only for the first system
                            SysItem polSysVal = new SysItem(); // takes the system-dependent policy-values (switch, order), is stored for all systems
                            XmlReader polReader = sysReader.ReadSubtree(); polReader.ReadToDescendant(EM2TAGS.ID);
                            do
                            {
                                switch (GetElementType(polReader))
                                {
                                    case ELEMENT_TYPE.NO: continue;
                                    case ELEMENT_TYPE.PROPERTY: ReadProperty(polReader, ref polProperties); continue; // reads policy-properties (name, reference-policy, ...)
                                    case ELEMENT_TYPE.SYSVAL: ReadSysVal(polReader, ref polSysVal); continue; // reads system-dependent policy-values (switch, order)
                                    case ELEMENT_TYPE.SUBTREE: break; // function-tag (for better readability not coded inside the switch-structure)
                                    default: continue;
                                }

                                // *** READ FUNCTION *** (see descriptions for policy above)
                                EM2Item funProperties = new EM2Item(); SysItem funSysVal = new SysItem();
                                XmlReader funReader = polReader.ReadSubtree(); funReader.ReadToDescendant(EM2TAGS.ID);
                                do
                                {
                                    switch (GetElementType(funReader))
                                    {
                                        case ELEMENT_TYPE.NO: continue;
                                        case ELEMENT_TYPE.PROPERTY: ReadProperty(funReader, ref funProperties); continue;
                                        case ELEMENT_TYPE.SYSVAL: ReadSysVal(funReader, ref funSysVal); continue;
                                        case ELEMENT_TYPE.SUBTREE: break;
                                        default: continue;
                                    }

                                    // *** READ PARAMETER *** (see descriptions for policy above)
                                    EM2Item parProperties = new EM2Item(); SysItem parSysVal = new SysItem();
                                    XmlReader parReader = funReader.ReadSubtree(); parReader.ReadToDescendant(EM2TAGS.ID);
                                    do
                                    {
                                        switch (GetElementType(parReader))
                                        {
                                            case ELEMENT_TYPE.NO: continue;
                                            case ELEMENT_TYPE.PROPERTY: ReadProperty(parReader, ref parProperties); continue;
                                            case ELEMENT_TYPE.SYSVAL: ReadSysVal(parReader, ref parSysVal); continue;
                                            default: continue;
                                        }
                                    } while (parReader.Read());

                                    // *** STORE PARAMETER ***
                                    string parRef = polSysVal.order + "°" + funSysVal.order + "°" + parSysVal.order;
                                    if (firstSystem) // ... for 1st system: the properties
                                    {
                                        parProperties.order = long.Parse(parSysVal.order); // order and parentId are necessary for
                                        parProperties.partentId = funProperties.id;        // adapting to visual order (see EM23Adapt_Order.cs)
                                        content.parameters.Add(parProperties.id, parProperties);
                                        if (!parRefs.TryAdd(parRef, parProperties.id)) // add reference-item, to allow parameters of following systems to target this now unique parameter
                                            errors.Add(GenerateError($"Failed to add parameter to reference-list: {sysProperties.name}/{polProperties.name}/{funProperties.name}/{parProperties.name} (order: {parRef})"));
                                    }
                                    // ... for all systems: value and order
                                    if (!parRefs.TryGetValue(parRef, out parSysVal.itemID)) // get the id of the now unique parameter stored in firstSystem-branch
                                        errors.Add(GenerateError($"Failed to assign parameter: {sysProperties.name}/{polProperties.name}/{funProperties.name}/{parProperties.name} (order: {parRef}, id: {parProperties.id})"));
                                    parSysVal.sysID = sysProperties.id;
                                    if (parSysVal.itemID != null) // adding this request avoids a crash in order-setting and thus allows translating the country
                                        content.sysPar.Add(parSysVal);
                                    parReader.Close();
                                    // finally, store id-change-info (i.e. old id -> new id) for (below) adapting parameter values which contain ids (e.g. ChangeParam/Param_Id)
                                    guidRef.Add(parProperties.id, parSysVal.itemID);

                                } while (funReader.Read());

                                // *** STORE FUNCTION ***
                                string funRef = polSysVal.order + "°" + funSysVal.order;
                                if (firstSystem) // ... for 1st system: the properties
                                {
                                    funProperties.order = long.Parse(funSysVal.order); // order and parentId are necessary for
                                    funProperties.partentId = polProperties.id;        // adapting to visual order (see EM23Adapt_Order.cs)
                                    content.functions.Add(funProperties.id, funProperties);
                                    if (!funRefs.TryAdd(funRef, funProperties.id)) // see description parameter above
                                        errors.Add(GenerateError($"Failed to add function to reference-list: {sysProperties.name}/{polProperties.name}/{funProperties.name} (order: {funRef})"));
                                }
                                // ... for all systems: switch and order
                                if (!funRefs.TryGetValue(funRef, out funSysVal.itemID)) // see description parameter above
                                    errors.Add(GenerateError($"Failed to assign function {sysProperties.name}/{polProperties.name}/{funProperties.name} (order: {funRef}, id: {funProperties.id})"));
                                funSysVal.sysID = sysProperties.id;
                                if (funSysVal.itemID != null) // see comment on adding request above
                                    content.sysFun.Add(funSysVal);
                                funReader.Close();
                                // finally, store id-change-info (i.e. old id -> new id) for (below) adapting parameter values which contain ids (e.g. ChangeParam/Param_Id)
                                guidRef.Add(funProperties.id, funSysVal.itemID);

                            } while (polReader.Read());

                            // *** STORE POLICY ***
                            string polRef = polSysVal.order;
                            if (firstSystem) // ... for 1st system: the properties
                            {
                                polProperties.order = long.Parse(polSysVal.order); // order is necessary for adapting to visual order (see EM23Adapt_Order.cs)
                                content.policies.Add(polProperties.id, polProperties);
                                if (!polRefs.TryAdd(polRef, polProperties.id)) // see description parameter above
                                    errors.Add(GenerateError($"Failed to add policy to reference-list: {sysProperties.name}/{polProperties.name} (order: {polRef})"));
                            }
                            // ... for all systems: switch and order
                            if (!polRefs.TryGetValue(polRef, out polSysVal.itemID)) // see description parameter above
                                errors.Add(GenerateError($"Failed to assign policy {sysProperties.name}/{polProperties.name} (order: {polRef}, id: {polProperties.id})"));
                            polSysVal.sysID = sysProperties.id;
                            if (polSysVal.itemID != null) // see comment on adding request above
                                content.sysPol.Add(polSysVal);
                            polReader.Close();
                            // finally, store id-change-info (i.e. old id -> new id) for (below) adapting parameter values which contain ids (e.g. ChangeParam/Param_Id)
                            guidRef.Add(polProperties.id, polSysVal.itemID);

                        } while (sysReader.Read());

                        // *** STORE SYSTEM ***
                        content.systems.Add(sysProperties.id, sysProperties);
                        sysReader.Close();
                        firstSystem = false;

                    } while (countryReader.Read());
                    
                    countryReader.Close();

                    // adapt parameter values, which contain ids, e.g. ChangeParam/Param_Id, Loop/First_Func, ...
                    foreach (SysItem sysPar in content.sysPar)
                    {
                        if (EM_Helpers.IsGuid(XmlHelpers.RemoveCData(sysPar.value)))
                        {   // note: do not use 'guid.ToString' because for some reason that's lowercase(?)
                            if (guidRef.TryGetValue(XmlHelpers.RemoveCData(sysPar.value), out string newGuid)) sysPar.value = newGuid;
                            else if (content.parameters[sysPar.itemID].name.ToLower() != DefPar.AddOn_ExtensionSwitch.Extension_Id.ToLower())
                                errors.Add(GenerateError($"Unknown id found: {sysPar.value} as value of parameter with id {sysPar.itemID}"));
                        }
                    }

                    do
                    {
                        if (mainReader.NodeType != XmlNodeType.Element) continue;

                        // *** READ UPRATING INDICES ***
                        if (mainReader.Name == EM2TAGS.UPINDEX)
                        {
                            XmlReader upiReader = mainReader.ReadSubtree(); upiReader.ReadToDescendant(EM2TAGS.ID);
                            EM2Item upi = new EM2Item();
                            do
                            {
                                if (upiReader.NodeType == XmlNodeType.Element) ReadProperty(upiReader, ref upi);
                            } while (upiReader.Read());
                            upiReader.Close();
                            content.upInd.Add(upi.id, upi);
                        }
                        // *** READ EXTERNAL STATISTICS ***
                        else if (mainReader.Name == EM2TAGS.EXSTAT)
                        {
                            XmlReader xsiReader = mainReader.ReadSubtree(); xsiReader.ReadToDescendant(EM2TAGS.ID);
                            EM2Item xs = new EM2Item();
                            do
                            {
                                if (xsiReader.NodeType == XmlNodeType.Element) ReadProperty(xsiReader, ref xs);
                            } while (xsiReader.Read());
                            xsiReader.Close();
                            content.exStat.Add(xs.id, xs);
                        }
                        // *** READ LOOK GROUPS ***
                        else if (mainReader.Name == EM2TAGS.LOOKGROUP)
                        {
                            XmlReader lgReader = mainReader.ReadSubtree(); lgReader.ReadToDescendant(EM2TAGS.ID);
                            EM2Item lg = new EM2Item();
                            do
                            {
                                if (lgReader.NodeType != XmlNodeType.Element) continue;
                                if (lgReader.Name == EM2TAGS.LOOKGROUP_POLICY)
                                {
                                    XmlReader lgPReader = lgReader.ReadSubtree(); lgPReader.ReadToDescendant(EM2TAGS.LOOKGROUP_ID);
                                    string lgId = null, polId = null;
                                    do
                                    {
                                        if (lgPReader.NodeType != XmlNodeType.Element) continue;
                                        if (lgPReader.Name == EM2TAGS.LOOKGROUP_ID) lgId = lgPReader.ReadInnerXml();
                                        if (lgPReader.Name == EM2TAGS.POLICY_ID) polId = lgPReader.ReadInnerXml();
                                    } while (lgPReader.Read());
                                    if (lgId != null && polId != null &&
                                        content.policies.ContainsKey(polId)) // only add the content once, i.e. for the single kept policy
                                        content.lookGroupPol.Add(new Tuple<string, string>(lgId, polId));
                                    lgPReader.Close();
                                }
                                else if (lgReader.Name == EM2TAGS.LOOKGROUP_FUNCTION)
                                {
                                    XmlReader lgFReader = lgReader.ReadSubtree(); lgFReader.ReadToDescendant(EM2TAGS.LOOKGROUP_ID);
                                    string lgId = null, funId = null;
                                    do
                                    {
                                        if (lgFReader.NodeType != XmlNodeType.Element) continue;
                                        if (lgFReader.Name == EM2TAGS.LOOKGROUP_ID) lgId = lgFReader.ReadInnerXml();
                                        if (lgFReader.Name == EM2TAGS.FUNCTION_ID) funId = lgFReader.ReadInnerXml();
                                    } while (lgFReader.Read());
                                    if (lgId != null && funId != null &&
                                        content.functions.ContainsKey(funId)) // only add the content once, i.e. for the single kept function
                                        content.lookGroupFun.Add(new Tuple<string, string>(lgId, funId));
                                    lgFReader.Close();
                                }
                                else if (lgReader.Name == EM2TAGS.LOOKGROUP_PARAMETER)
                                {
                                    XmlReader lgPReader = lgReader.ReadSubtree(); lgPReader.ReadToDescendant(EM2TAGS.LOOKGROUP_ID);
                                    string lgId = null, parId = null;
                                    do
                                    {
                                        if (lgPReader.NodeType != XmlNodeType.Element) continue;
                                        if (lgPReader.Name == EM2TAGS.LOOKGROUP_ID) lgId = lgPReader.ReadInnerXml();
                                        if (lgPReader.Name == EM2TAGS.PARAMETER_ID) parId = lgPReader.ReadInnerXml();
                                    } while (lgPReader.Read());
                                    if (lgId != null && parId != null &&
                                        content.parameters.ContainsKey(parId)) // only add the content once, i.e. for the single kept parameter
                                        content.lookGroupPar.Add(new Tuple<string, string>(lgId, parId));
                                    lgPReader.Close();
                                }
                                else ReadProperty(lgReader, ref lg);
                            } while (lgReader.Read());
                            lgReader.Close();
                            if (lg.id != null) content.lookGroup.Add(lg.id, lg);
                        }
                        // *** READ EXTENSIONS ***
                        else if (mainReader.Name == EM2TAGS.EXTENSION_POLICY)
                        {
                            XmlReader exReader = mainReader.ReadSubtree(); exReader.ReadToDescendant(EM2TAGS.EXTENSION_ID);
                            string exId = null, polId = null, baseoff = null;
                            do
                            {
                                if (exReader.NodeType != XmlNodeType.Element) continue;
                                if (exReader.Name == EM2TAGS.EXTENSION_ID) exId = exReader.ReadInnerXml();
                                if (exReader.Name == EM2TAGS.POLICY_ID) polId = exReader.ReadInnerXml();
                                if (exReader.Name == EM2TAGS.BASEOFF) baseoff = exReader.ReadInnerXml();
                            } while (exReader.Read());
                            if (exId != null && polId != null && baseoff != null &&
                                content.policies.ContainsKey(polId)) // only add the content once, i.e. for the single kept policy
                                content.extensionPol.Add(new Tuple<string, string, string>(exId, polId, baseoff));
                            exReader.Close();
                        }
                        else if (mainReader.Name == EM2TAGS.EXTENSION_FUNCTION)
                        {
                            XmlReader exReader = mainReader.ReadSubtree(); exReader.ReadToDescendant(EM2TAGS.EXTENSION_ID);
                            string exId = null, funId = null, baseoff = null;
                            do
                            {
                                if (exReader.NodeType != XmlNodeType.Element) continue;
                                if (exReader.Name == EM2TAGS.EXTENSION_ID) exId = exReader.ReadInnerXml();
                                if (exReader.Name == EM2TAGS.FUNCTION_ID) funId = exReader.ReadInnerXml();
                                if (exReader.Name == EM2TAGS.BASEOFF) baseoff = exReader.ReadInnerXml();
                            } while (exReader.Read());
                            if (exId != null && funId != null && baseoff != null &&
                                content.functions.ContainsKey(funId)) // only add the content once, i.e. for the single kept function
                                content.extensionFun.Add(new Tuple<string, string, string>(exId, funId, baseoff));
                            exReader.Close();
                        }
                        else if (mainReader.Name == EM2TAGS.EXTENSION_PARAMETER)
                        {
                            XmlReader exReader = mainReader.ReadSubtree(); exReader.ReadToDescendant(EM2TAGS.EXTENSION_ID);
                            string exId = null, parId = null, baseoff = null;
                            do
                            {
                                if (exReader.NodeType != XmlNodeType.Element) continue;
                                if (exReader.Name == EM2TAGS.EXTENSION_ID) exId = exReader.ReadInnerXml();
                                if (exReader.Name == EM2TAGS.PARAMETER_ID) parId = exReader.ReadInnerXml();
                                if (exReader.Name == EM2TAGS.BASEOFF) baseoff = exReader.ReadInnerXml();
                            } while (exReader.Read());
                            if (exId != null && parId != null && baseoff != null &&
                                content.parameters.ContainsKey(parId)) // only add the content once, i.e. for the single kept parameter
                                content.extensionPar.Add(new Tuple<string, string, string>(exId, parId, baseoff));
                            exReader.Close();
                        }
                        else if (mainReader.Name == EM2TAGS.INDTAX)
                        {
                            XmlReader itReader = mainReader.ReadSubtree(); itReader.ReadToDescendant(EM2TAGS.ID);
                            EM2Item it = new EM2Item();
                            do
                            {
                                if (itReader.NodeType == XmlNodeType.Element) ReadProperty(itReader, ref it);
                            } while (itReader.Read());
                            itReader.Close();
                            content.indTax.Add(it.id, it);
                        }
                    } while (mainReader.Read());
                }
                return content;
            }
            catch (Exception exception)
            {
                errors.Add(GenerateError(exception.Message));
                return null;
            }

            ELEMENT_TYPE GetElementType(XmlReader reader, bool forSystem = false) // helping function: see usage above
            {
                if (reader.NodeType != XmlNodeType.Element) return ELEMENT_TYPE.NO;
                switch (reader.Name)
                {
                    case EM2TAGS.SYSTEM: case EM2TAGS.POLICY: case EM2TAGS.FUNCTION: case EM2TAGS.PARAMETER: return ELEMENT_TYPE.SUBTREE;
                    case EM2TAGS.SWITCH: case EM2TAGS.VALUE: return ELEMENT_TYPE.SYSVAL;
                    case EM2TAGS.ORDER: return forSystem ? ELEMENT_TYPE.PROPERTY : ELEMENT_TYPE.SYSVAL;
                    default: return ELEMENT_TYPE.PROPERTY;
                }
            }

            void ReadProperty(XmlReader reader, ref EM2Item item) // helping function: see usage above
            {
                switch (reader.Name)
                {
                    case EM2TAGS.ID: item.id = reader.ReadInnerXml(); break;
                    case EM2TAGS.NAME:
                    case EM2TAGS.REFERENCE: // uprating-factor-name is for some reason called reference (same for external statistics)
                        item.name = reader.ReadInnerXml(); break;
                    default: item.properties.Add(reader.Name, reader.ReadInnerXml()); break;
                }
            }

            void ReadSysVal(XmlReader reader, ref SysItem item) // helping function: see usage above
            {
                switch (reader.Name)
                {
                    case EM2TAGS.SWITCH:
                    case EM2TAGS.VALUE: item.value = reader.ReadInnerXml(); break;
                    case EM2TAGS.ORDER: item.order = reader.ReadInnerXml(); break;
                }
            }

            string GenerateError(string error)
            {
                return $"{(content.general.name != string.Empty ? content.general.name : fileName)}: {error}";
            }
        }
    }
}
