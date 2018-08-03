using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace EventNotifier
{
    public class SettingsManager
    {
        private string _fileName;

        private Dictionary<string, string> Settings {
            get;
            set;
        }

        public bool SettingsExist {
            get;
            set;
        }

        public SettingsManager(string fileName)
        {
            this._fileName = fileName;
            this.Settings = new Dictionary<string, string>();
            try
            {
                if (!File.Exists(this._fileName))
                {
                    this.SettingsExist = false;
                }
                else
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(this._fileName);
                    XmlNode xmlNodes = xmlDocument.SelectSingleNode("//configuration/appSettings");
                    if (xmlNodes != null)
                    {
                        if (xmlNodes.HasChildNodes)
                        {
                            foreach (XmlNode childNode in xmlNodes.ChildNodes)
                            {
                                if (!(childNode is XmlElement) || !(childNode.Name == "setting") || !((XmlElement)childNode).HasAttribute("key") || !((XmlElement)childNode).HasAttribute("value"))
                                {
                                    continue;
                                }
                                this.Settings.Add(childNode.Attributes["key"].Value, childNode.Attributes["value"].Value);
                            }
                        }
                        this.SettingsExist = true;
                    }
                    else
                    {
                        this.SettingsExist = false;
                        return;
                    }
                }
            }
            catch
            {
                this.SettingsExist = false;
            }
        }

        public string GetValue(string settingName, string defaultValue)
        {
            if (!this.Settings.ContainsKey(settingName))
            {
                return defaultValue;
            }
            return this.Settings[settingName];
        }

        public void Remove(string settingName)
        {
            if (this.Settings.ContainsKey(settingName))
            {
                this.Settings.Remove(settingName);
            }
        }

        public bool Save()
        {
            bool flag;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                if (!File.Exists(this._fileName))
                {
                    throw new Exception(string.Concat("Error saving settings. ", this._fileName, " does not exist."));
                }
                xmlDocument.Load(this._fileName);
                XmlNode xmlNodes = xmlDocument.SelectSingleNode("//configuration/appSettings");
                if (xmlNodes == null)
                {
                    XmlNode xmlNodes1 = xmlDocument.SelectSingleNode("//configuration");
                    if (xmlNodes1 == null)
                    {
                        throw new Exception(string.Concat("Error saving settings. Could not find root node at XPath //configuration in ", this._fileName, "."));
                    }
                    xmlNodes = xmlDocument.CreateElement("appSettings");
                    xmlNodes1.AppendChild(xmlNodes);
                }
                xmlNodes.RemoveAll();
                foreach (KeyValuePair<string, string> setting in this.Settings)
                {
                    XmlElement xmlElement = xmlDocument.CreateElement("setting");
                    XmlAttribute key = xmlDocument.CreateAttribute("key");
                    key.Value = setting.Key;
                    xmlElement.Attributes.Append(key);
                    key = xmlDocument.CreateAttribute("value");
                    key.Value = setting.Value;
                    xmlElement.Attributes.Append(key);
                    xmlNodes.AppendChild(xmlElement);
                }
                xmlDocument.Save(this._fileName);
                return true;
            }
            catch 
            {
                flag = false;
            }

            return flag;
        }

        public void SetValue(string settingName, string newValue)
        {
            if (this.Settings.ContainsKey(settingName))
            {
                this.Settings[settingName] = newValue;
                return;
            }
            this.Settings.Add(settingName, newValue);
        }
    }
}
