using System;
using System.Collections.Generic;
using System.Xml;

namespace Shared.Misc
{
    public static class XmlParser
    {
        public static string[] ParseDataFromXML(string xmlPath, string elementName)
        {
            List<string> result = new List<string>();

            try
            {
                XmlReader reader = XmlReader.Create(xmlPath);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == elementName)
                    {
                        result.Add(reader.ReadElementContentAsString());
                    }
                }

                reader.Close();

                return result.ToArray();
            }
            catch(Exception e) { Logger.WriteToConsole($"[Error] > Failed to parse mod at '{xmlPath}'. Exception: {e}", Logger.LogMode.Error); }

            return result.ToArray();
        }
    }
}
