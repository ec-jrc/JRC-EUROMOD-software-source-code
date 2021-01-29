using EM_Common;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EM_Transformer
{
    public static class TransformEMConfig
    {

        /// <summary> translates EM2 configuration file to EM3 format </summary>
        /// <param name="path"> full path of EM2 configuration file </param>
        /// <param name="em3Config"> result: EM3 configuration dictionary </param>
        /// <param name="emContentPath"> result: content of tag EMCONTENTPATH, i.e. the folder where EM2-files are stored
        /// this is returned separately because the caller must handle it
        /// i.e. after translation to EM3, add CONFIG_PATH_EUROMODFILES=EM3-path to the dictionary
        /// </param>
        /// <param name="errorAction"> list of warnings and/or errors </param>
        /// <returns> false, if proceeding (running) is not possible </returns>
        public static bool Transform(string path, out Dictionary<string, string> em3Config, Action<string> errorAction = null)
        {
            return Transform(path, out List<KeyValuePair<string, string>> dummy, out em3Config, errorAction);
        }
        public static bool Transform(string path,
                                     out List<KeyValuePair<string, string>> em2Config,
                                     out Dictionary<string, string> em3Config,
                                     Action<string> errorAction = null)
        {
            em3Config = new Dictionary<string, string>(); em2Config = null;
            try
            {
                em2Config = ReadConfig(path);
                List<string> extensionSwitches = new List<string>(); string configPath = null;
                foreach (var em2Entry in em2Config)
                {
                    switch (em2Entry.Key)
                    {
                        case TAGS.EM2CONFIG_EMCONTENTPATH: em3Config.Add(TAGS.CONFIG_PATH_EUROMODFILES, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_COUNTRY_FILE: em3Config.Add(TAGS.CONFIG_COUNTRY, em2Entry.Value.ToLower().Replace(".xml", "")); break;
                        case TAGS.EM2CONFIG_CONFIGPATH: configPath = em2Entry.Value; break; // not necessary for new exe, but for translating extension switches (see below)

                        case TAGS.EM2CONFIG_OUTPUTPATH: em3Config.Add(TAGS.CONFIG_PATH_OUTPUT, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_DATAPATH: em3Config.Add(TAGS.CONFIG_PATH_DATA, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_SYSTEM_ID: em3Config.Add(TAGS.CONFIG_ID_SYSTEM, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_DATASET_ID: em3Config.Add(TAGS.CONFIG_ID_DATA, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_EXCHANGE_RATE_DATE:
                            em3Config.Add(TAGS.CONFIG_DATE_EXCHANGE_RATE, EM_Helpers.RemoveWhitespace(em2Entry.Value));
                            em3Config.Add(TAGS.CONFIG_FORCE_OUTPUT_EURO, DefPar.Value.YES); // the exchange-rate-date is only written to the config 
                            break;                                                          // if the "All Output in €" box is checked
                        case TAGS.EM2CONFIG_IGNORE_PRIVATE: em3Config.Add(TAGS.CONFIG_IGNORE_PRIVATE, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_POLICY_SWITCH: extensionSwitches.Add(em2Entry.Value); break;
                        case TAGS.EM2CONFIG_STARTHH: em3Config.Add(TAGS.CONFIG_FIRST_HH, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_LASTHH: em3Config.Add(TAGS.CONFIG_LAST_HH, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_FIRST_N_HH_ONLY: em3Config.Add(TAGS.CONFIG_FIRST_N_HH_ONLY, em2Entry.Value); break;
                        case TAGS.EM2CONFIG_OUTFILE_DATE: em3Config.Add(TAGS.CONFIG_DATE_OUTFILE, em2Entry.Value); break;

                        // must be handled by the caller
                        case TAGS.EM2CONFIG_ERRLOG_FILE: break;
                        case TAGS.EM2CONFIG_EMVERSION: break; // e.g. EurmodFiles_H2.0
                        case TAGS.EM2CONFIG_UIVERSION: break;
                        case TAGS.EM2CONFIG_HEADER_DATE: break;
                        case TAGS.EM2CONFIG_LOG_WARNINGS: break; // exe itself stops only on errors, i.e. caller would need to stop on warnings after reading parameters

                        // out-dated
                        case TAGS.EM2CONFIG_LOG_RUNTIME: break;
                        case TAGS.EM2CONFIG_DECSIGN_PARAM: break;
                        case TAGS.EM2CONFIG_LAST_RUN: break;
                        case TAGS.EM2CONFIG_ISPUBLICVERSION: break;
                        case TAGS.EM2CONFIG_DATACONFIG_FILE: break;
                        case TAGS.EM2CONFIG_PARAMPATH: break;  // subsumed in EM2_EMCONTENTPATH

                        default: em3Config.Add(em2Entry.Key, em2Entry.Value); break;
                    }
                }
                TranslateExtensionSwitches(em3Config);
                return true;

                // translate from "BTA_??=8b258cd1-804e-4c9d-adc2-2d3e609c162a=on" to "8b258cd1-804e-4c9d-adc2-2d3e609c162a=on"
                // (in EM2 the 2nd part was the system-id, but the UI-run-tool already replaces this by the extension-id for EM3)
                void TranslateExtensionSwitches(Dictionary<string, string> config)
                {
                    int counter = 0; // this is just because the config is a Dictionary and we cannot add two equal keys
                    foreach (string extensionSwitch in extensionSwitches)
                    {
                        string[] parts = extensionSwitch.Split('=');
                        if (parts.Length == 3) config.Add($"{TAGS.CONFIG_EXTENSION_SWITCH}{counter++}", $"{parts[1]}={parts[2]}");
                        else errorAction($"Failed to transform extension switch {extensionSwitch}");
                    }
                }
            }
            catch (Exception exception)
            {
                errorAction(exception.Message);
                return false;
            }
        }

        /// <summary> reads a configuration file, currently used for EM2-config reading and translating, but possibly usable for exe-call </summary>
        private static List<KeyValuePair<string, string>> ReadConfig(string path)
        {
            try
            {
                List<KeyValuePair<string, string>> config = new List<KeyValuePair<string, string>>();
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    reader.ReadToDescendant(TAGS.EM2CONFIG_EMCONFIG);
                    do
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.Name == TAGS.EM2CONFIG_EMCONFIG) continue;
                        config.Add(new KeyValuePair<string, string>(reader.Name, reader.ReadInnerXml()));
                    } while (reader.Read());
                }
                return config;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed reading configuration file {path}: {exception.Message}");
            }
        }
    }
}
