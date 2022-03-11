using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed.IO
{
    public interface IIniReader
    {
        string GetSetting(string sectionName, string settingName);
        string[] EnumSection(string sectionName);
    }
    
    public class IniParser : IIniReader
    {
        private Hashtable keyPairs = new Hashtable();
        private string iniFilePath;

        private struct SectionPair
        {
            public string Section;
            public string Key;
        }

        public static IEnumerator ReadAsync(string iniPath, Action<IIniReader> callback)
        {
            return (FileHelper.ReadAllLinesAsync(iniPath,(p, data) =>
            {
                IniParser parser = null;
                if (data != null)
                {
                    parser = new IniParser(iniPath);
                    parser.LoadData(data);
                }
                
                callback?.Invoke(parser);
            }));
        }

        public static IniParser Open(string iniPath, bool createIfNotExists)
        {
            IniParser iniParser = null;
            try
            {
                if (!FileHelper.Exists(iniPath))
                {
                    if (createIfNotExists)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(iniPath));
                        var stream = File.Create(iniPath);
                        stream.Close();
                    }
                    else
                    {
                        return null;
                    }
                }

                iniParser = new IniParser(iniPath);
                string[] lines = FileHelper.ReadAllLines(iniPath);
                iniParser.LoadData(lines);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open Ini file at {iniPath}. \n" + ex.ToString());
                iniParser = null;
                // throw ex;
            }
            return iniParser;
        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        private IniParser(string iniPath)
        {
            iniFilePath = iniPath;
        }

        private void LoadData(string[] lines)
        {
            string[] keyPair;
            string currentRoot = null;
            foreach (string line in lines)
            {
                string strLine = line.Trim();

                if (strLine != "")
                {
                    if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                    {
                        currentRoot = strLine.Substring(1, strLine.Length - 2);
                    }
                    else
                    {
                        keyPair = strLine.Split(new char[] {'='}, 2);

                        SectionPair sectionPair;
                        string value = null;

                        if (currentRoot == null)
                            currentRoot = "ROOT";

                        sectionPair.Section = currentRoot.ToUpper();
                        sectionPair.Key = keyPair[0].ToUpper();

                        if (keyPair.Length > 1)
                            value = keyPair[1];

                        keyPairs.Add(sectionPair, value);
                    }
                }
            }
        }

        public Dictionary<string, object> SettingsForSection(string section)
        {
            section = section.Trim().ToUpper();

            var keys = keyPairs.Keys.OfType<SectionPair>();
            var relevantKeys = keys.Where(k => k.Section == section);

            return relevantKeys.ToDictionary(k => k.Key, k => keyPairs[k]);
        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
        public string GetSetting(string sectionName, string settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            sectionPair.Key = settingName.ToUpper();

            return (string) keyPairs[sectionPair];
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public string[] EnumSection(string sectionName)
        {
            ArrayList tmpArray = new ArrayList();

            foreach (SectionPair pair in keyPairs.Keys)
            {
                if (pair.Section == sectionName.ToUpper())
                    tmpArray.Add(pair.Key);
            }

            return (string[]) tmpArray.ToArray(typeof(string));
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        /// <param name="settingValue">Value of key.</param>
        public void AddSetting(string sectionName, string settingName, string settingValue)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            sectionPair.Key = settingName.ToUpper();

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, settingValue);
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved with a null value.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void AddSetting(string sectionName, string settingName)
        {
            AddSetting(sectionName, settingName, null);
        }

        /// <summary>
        /// Remove a setting.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void DeleteSetting(string sectionName, string settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            sectionPair.Key = settingName.ToUpper();

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);
        }

        /// <summary>
        /// Save settings to new file.
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
        public void SaveSettings(string newFilePath)
        {
            ArrayList sections = new ArrayList();
            string tmpValue = "";
            string strToSave = "";

            foreach (SectionPair sectionPair in keyPairs.Keys)
            {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }

            foreach (string section in sections)
            {
                strToSave += ("[" + section + "]\r\n");

                foreach (SectionPair sectionPair in keyPairs.Keys)
                {
                    if (sectionPair.Section == section)
                    {
                        tmpValue = (string) keyPairs[sectionPair];

                        if (tmpValue != null)
                            tmpValue = "=" + tmpValue;

                        strToSave += (sectionPair.Key + tmpValue + "\r\n");
                    }
                }

                strToSave += "\r\n";
            }

            try
            {
                TextWriter tw = new StreamWriter(newFilePath);
                tw.Write(strToSave);
                tw.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ClearSettings()
        {
            keyPairs.Clear();
        }

        /// <summary>
        /// Save settings back to ini file.
        /// </summary>
        public void SaveSettings()
        {
            SaveSettings(iniFilePath);
        }
    }
}