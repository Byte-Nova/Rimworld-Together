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
    }
}