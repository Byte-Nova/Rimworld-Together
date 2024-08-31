using System.Xml;

namespace GameServer
{
    public static class XmlParser
    {
        public static string[] GetChildContentFromParent(string xmlPath, string elementName, string parentElement)
        {
            List<string> result = new List<string>();

            try
            {
                XmlReader reader = XmlReader.Create(xmlPath);
                while (reader.Read())
                {
                    if (reader.Name == parentElement) 
                    {
                        reader.Read();
                        reader.ReadToNextSibling(elementName);
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == elementName)
                        {
                            result.Add(reader.ReadElementContentAsString());
                        }
                    }
                }

                reader.Close();
            }
            catch (Exception e) { Logger.Error($"Failed to parse mod at '{xmlPath}'. Exception: {e}"); }

            return result.ToArray();
        }
    }
}
