using System.Xml;

namespace GameServer
{
    public static class XmlParser
    {
        public static string[] ChildContentFromParent(string xmlPath, string elementName, string parentElement)
        {
            List<string> result = new List<string>();
            //Convert the Parent element to lowercase
            elementName =  elementName.ToLower();
            parentElement = parentElement.ToLower();

            try
            {
                XmlReader reader = XmlReader.Create(xmlPath);
                while (reader.Read())
                {
                    if (reader.Name.ToLower() == parentElement) 
                    {
                        string childContent = InnerNodeCaseInsensitive(reader, elementName);
                        if (!String.IsNullOrEmpty(childContent)) {
                            result.Add(childContent);
                        }
                    }
                }

                reader.Close();

                return result.ToArray();
            }
            catch (Exception e) { Logger.Error($"Failed to parse mod at '{xmlPath}'. Exception: {e}"); }

            return result.ToArray();
        }

        // Iterate over the Inner elements in case insentitive mode
        // Return the value found in lowercase or empty
        public static string InnerNodeCaseInsensitive(XmlReader reader, string elementName)
        {
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Name.ToLower() != elementName)
                {
                    continue;
                }
                return reader.ReadElementContentAsString().ToLower();
            }

            return String.Empty;
        }

    }
}
