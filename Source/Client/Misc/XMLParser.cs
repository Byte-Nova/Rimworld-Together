using JetBrains.Annotations;
using Shared;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Verse;

namespace GameClient
{
    public static class XMLParser
    {
        public static string GetDataFromXML(string path, string elementName)
        {
            string dataToReturn = "";

            XmlReader reader = XmlReader.Create(path);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == elementName)
                {
                    dataToReturn = reader.ReadElementContentAsString();
                    dataToReturn = dataToReturn.Replace("\n", "");
                    reader.Close();
                    break;
                }
            }

            return dataToReturn;
        }

        public static void SetDataIntoXML(string path, string elementName, string replacement)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode docNode = GetChildrenNodesInNode(doc, "savegame");
            XmlNode gameNode = GetChildrenNodesInNode(docNode, "game");
            XmlNode worldNode = GetChildrenNodesInNode(gameNode, "world");
            XmlNode gridNode = GetChildrenNodesInNode(worldNode, "grid");

            foreach (XmlNode child in gridNode.ChildNodes)
            {
                if (child.Name == elementName)
                {
                    child.InnerText = replacement;
                    doc.Save(path);
                    break;
                }
            }
        }

        private static XmlNode GetChildrenNodesInNode(XmlNode node, string targetName)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == targetName) return child;
            }

            return null;
        }
    }
}