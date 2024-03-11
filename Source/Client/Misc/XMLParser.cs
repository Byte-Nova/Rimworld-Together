using Shared;
using System.IO;
using System.Xml;

namespace GameClient
{
    public static class XmlParser
    {
        //Gets all the required details of the world from the save XML file
        //The data MUST BE ACCESSED IN PERFECT ORDER DUE TO XML LIMITATIONS

        public static WorldDetailsJSON GetWorldXmlData(WorldDetailsJSON worldDetailsJSON)
        {
            string filePath = Path.Combine(new string[] { Master.savesFolderPath, SaveManager.customSaveName + ".rws" });
            XmlReader reader = XmlReader.Create(filePath);

            worldDetailsJSON.tileBiomeDeflate = GetDataFromXml(reader, "tileBiomeDeflate");
            worldDetailsJSON.tileElevationDeflate = GetDataFromXml(reader, "tileElevationDeflate");
            worldDetailsJSON.tileHillinessDeflate = GetDataFromXml(reader, "tileHillinessDeflate");
            worldDetailsJSON.tileTemperatureDeflate = GetDataFromXml(reader, "tileTemperatureDeflate");
            worldDetailsJSON.tileRainfallDeflate = GetDataFromXml(reader, "tileRainfallDeflate");
            worldDetailsJSON.tileSwampinessDeflate = GetDataFromXml(reader, "tileSwampinessDeflate");
            worldDetailsJSON.tileFeatureDeflate = GetDataFromXml(reader, "tileFeatureDeflate");
            worldDetailsJSON.tilePollutionDeflate = GetDataFromXml(reader, "tilePollutionDeflate");
            worldDetailsJSON.tileRoadOriginsDeflate = GetDataFromXml(reader, "tileRoadOriginsDeflate");
            worldDetailsJSON.tileRoadAdjacencyDeflate = GetDataFromXml(reader, "tileRoadAdjacencyDeflate");
            worldDetailsJSON.tileRoadDefDeflate = GetDataFromXml(reader, "tileRoadDefDeflate");
            worldDetailsJSON.tileRiverOriginsDeflate = GetDataFromXml(reader, "tileRiverOriginsDeflate");
            worldDetailsJSON.tileRiverAdjacencyDeflate = GetDataFromXml(reader, "tileRiverAdjacencyDeflate");
            worldDetailsJSON.tileRiverDefDeflate = GetDataFromXml(reader, "tileRiverDefDeflate");

            reader.Close();

            return worldDetailsJSON;
        }

        //Gets the data from the specified XML element name

        public static string GetDataFromXml(XmlReader reader, string elementName)
        {
            string dataToReturn = "";

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == elementName)
                {
                    dataToReturn = reader.ReadElementContentAsString();
                    dataToReturn = dataToReturn.Replace("\n", "");
                    break;
                }
            }

            return dataToReturn;
        }

        //Modifies the existing XML file with the required details from the server

        public static void ModifyWorldXml(WorldDetailsJSON worldDetailsJSON)
        {
            string path = Path.Combine(new string[] { Master.savesFolderPath, SaveManager.customSaveName + ".rws" });

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode docNode = GetChildNodeInNode(doc, "savegame");
            XmlNode gameNode = GetChildNodeInNode(docNode, "game");
            XmlNode worldNode = GetChildNodeInNode(gameNode, "world");
            XmlNode gridNode = GetChildNodeInNode(worldNode, "grid");

            SetDataIntoXML(gridNode, "tileBiomeDeflate", worldDetailsJSON.tileBiomeDeflate);
            SetDataIntoXML(gridNode, "tileElevationDeflate", worldDetailsJSON.tileElevationDeflate);
            SetDataIntoXML(gridNode, "tileHillinessDeflate", worldDetailsJSON.tileHillinessDeflate);
            SetDataIntoXML(gridNode, "tileTemperatureDeflate", worldDetailsJSON.tileTemperatureDeflate);
            SetDataIntoXML(gridNode, "tileRainfallDeflate", worldDetailsJSON.tileRainfallDeflate);
            SetDataIntoXML(gridNode, "tileSwampinessDeflate", worldDetailsJSON.tileSwampinessDeflate);
            SetDataIntoXML(gridNode, "tileFeatureDeflate", worldDetailsJSON.tileFeatureDeflate);
            SetDataIntoXML(gridNode, "tilePollutionDeflate", worldDetailsJSON.tilePollutionDeflate);
            SetDataIntoXML(gridNode, "tileRoadOriginsDeflate", worldDetailsJSON.tileRoadOriginsDeflate);
            SetDataIntoXML(gridNode, "tileRoadAdjacencyDeflate", worldDetailsJSON.tileRoadAdjacencyDeflate);
            SetDataIntoXML(gridNode, "tileRoadDefDeflate", worldDetailsJSON.tileRoadDefDeflate);
            SetDataIntoXML(gridNode, "tileRiverOriginsDeflate", worldDetailsJSON.tileRiverOriginsDeflate);
            SetDataIntoXML(gridNode, "tileRiverAdjacencyDeflate", worldDetailsJSON.tileRiverAdjacencyDeflate);
            SetDataIntoXML(gridNode, "tileRiverDefDeflate", worldDetailsJSON.tileRiverDefDeflate);

            doc.Save(path);
        }

        //Sets the data of the specified XML node

        public static void SetDataIntoXML(XmlNode gridNode, string elementName, string replacement)
        {
            foreach (XmlNode child in gridNode.ChildNodes)
            {
                if (child.Name == elementName)
                {
                    child.InnerText = replacement;
                    break;
                }
            }
        }

        //Gets a specific child inside of the specified node's children

        private static XmlNode GetChildNodeInNode(XmlNode node, string targetName)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == targetName) return child;
            }

            return null;
        }
    }
}