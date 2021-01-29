using EM_Common;
using EM_UI.GlobalAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_UI.DataSets
{
    internal class ExchangeRatesConfigFacade
    {
        private string _pathExchangeRatesConfig = string.Empty;

        private ExchangeRatesConfig _exchangeRatesConfig = null;

        internal ExchangeRatesConfigFacade(string pathExchangeRatesConfig)
        {
            _pathExchangeRatesConfig = pathExchangeRatesConfig;
        }


        internal ExchangeRatesConfigFacade()
        {
            _pathExchangeRatesConfig = new EMPath(EM_AppContext.FolderEuromodFiles).GetExRatesFilePath(true);
            LoadExchangeRatesConfig();
        }

        static readonly string[] _cDataElements = new string[] {};

        internal bool LoadExchangeRatesConfig()
        {
            try
            {
                if (_exchangeRatesConfig == null)
                {
                    _exchangeRatesConfig = new ExchangeRatesConfig();
                    if (File.Exists(_pathExchangeRatesConfig))
                        using (StreamReader streamReader = new StreamReader(_pathExchangeRatesConfig, DefGeneral.DEFAULT_ENCODING))
                            _exchangeRatesConfig.ReadXml(streamReader);
                    _exchangeRatesConfig.AcceptChanges();
                }
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal ExchangeRatesConfig GetExchangeRatesConfig()
        {
            return _exchangeRatesConfig;
        }

        internal bool WriteXML()
        {
            try
            {
                _exchangeRatesConfig.AcceptChanges();
                Stream fileStream = new FileStream(new EMPath(EM_AppContext.FolderEuromodFiles).GetExRatesFilePath(true), FileMode.Create);
                using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING, _cDataElements))
                    _exchangeRatesConfig.WriteXml(xmlWriter);
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal List<ExchangeRatesConfig.ExchangeRatesRow> GetExchangeRates(string country = null)
        {
            return _exchangeRatesConfig.ExchangeRates.Where(w => string.IsNullOrEmpty(country) || w.Country.ToLower() == country.ToLower())
                                                     .OrderBy(o => o.Country).ThenBy(o => o.ValidFor).ToList();
        }

        internal bool HasExchangeRates(string country) { return GetExchangeRates(country).Any(); }

        internal bool RefreshExchangeRates(List<ExchangeRate> exRates)
        {
            try
            {
                for (int i = 0; i < exRates.Count; ++i) // "make efficient", i.e. if country, rates and default are equal, gather the concerned systems in one rate
                    for (int j = i - 1; j >= 0; --j)
                        if (exRates[i].Equals(exRates[j])) { exRates[i].AddToValidFor(exRates[j].ValidFor); exRates[j].ValidFor = string.Empty; break; }

                _exchangeRatesConfig.ExchangeRates.Rows.Clear();
                _exchangeRatesConfig.AcceptChanges();
                foreach (ExchangeRate exRate in exRates)
                    if (exRate.ValidFor != string.Empty) _exchangeRatesConfig.ExchangeRates.AddExchangeRatesRow(
                        exRate.Country, exRate.June30, exRate.YearAverage, exRate.FirstSemester, exRate.SecondSemester, exRate.Default, exRate.ValidFor);
                _exchangeRatesConfig.AcceptChanges();
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal static ExchangeRatesConfig.ExchangeRatesRow CopyExchangeRatesFromAnotherConfig(ExchangeRatesConfig exchangeRatesConfig, ExchangeRatesConfig.ExchangeRatesRow originalExchangeRates)
        {
            ExchangeRatesConfig.ExchangeRatesRow exchangeRatesRow = exchangeRatesConfig.ExchangeRates.AddExchangeRatesRow(
                        originalExchangeRates.Country, originalExchangeRates.June30, originalExchangeRates.YearAverage, originalExchangeRates.FirstSemester, originalExchangeRates.SecondSemester, originalExchangeRates.Default, originalExchangeRates.ValidFor);

            return exchangeRatesRow;
        }

        internal void RemoveSystems(string countryShortName, List<string> systemNames)
        {
            try
            {
                bool store = false; List<ExchangeRatesConfig.ExchangeRatesRow> delRates = new List<ExchangeRatesConfig.ExchangeRatesRow>();
                foreach (string systemName in systemNames)
                    foreach (ExchangeRatesConfig.ExchangeRatesRow exchangeRate in from er in GetExchangeRates(countryShortName)
                                                                                  where ExchangeRate.ValidForToList(er.ValidFor).Contains(systemName.ToLower())
                                                                                  select er)
                    {
                        if (ExchangeRate.ValidForToList(exchangeRate.ValidFor).Count == 1) delRates.Add(exchangeRate);
                        else exchangeRate.ValidFor = ExchangeRate.RemoveFromValidFor(exchangeRate.ValidFor, systemName);
                        store = true; break;
                    }
                foreach (ExchangeRatesConfig.ExchangeRatesRow delRate in delRates) delRate.Delete();
                if (store) WriteXML();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "A problem occured while adapting global exchange rates file. Please update manually if necessary.", false);
            }
        }
    }
}
