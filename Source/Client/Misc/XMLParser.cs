using Shared;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using UnityEngine;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class XmlParser
    {
        //  Gets each deflate from the player's world save
        //  This is typically used once during server creation
        public static void GetWorldXmlData(WorldData worldData)
        {
            string filePath = Path.Combine(new string[] { Master.savesFolderPath, SaveManager.customSaveName + ".rws" });

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //Navigate to the grid in the xml file
            XmlNode docNode = GetChildNodeInNode(doc, "savegame");
            XmlNode gameNode = GetChildNodeInNode(docNode, "game");
            XmlNode worldNode = GetChildNodeInNode(gameNode, "world");
            XmlNode gridNode = GetChildNodeInNode(worldNode, "grid");



            foreach (XmlNode deflateNode in gridNode.ChildNodes)
            {
                worldData.deflateDictionary.Add(deflateNode.Name, deflateNode.InnerText);
            }

            XmlNode worldObjectsNode = GetChildNodeInNode(worldNode, "worldObjects");

            worldData.WorldObjects = worldObjectsNode.InnerXml;
        }

        public static void parseGrid(WorldData worldData, Dictionary<string, byte[]> tileData)
        {

            //convert the world deflates into byte arrays


            foreach (string k in tileData.Keys.ToList())
            {
                if (worldData.deflateDictionary.ContainsKey(k))
                {
                    tileData[k] = CompressUtility.Decompress(Convert.FromBase64String(DataExposeUtility.RemoveLineBreaks(worldData.deflateDictionary[k])));
                }
                else if (worldData.deflateDictionary.ContainsKey(k + "Deflate"))
                {
                    tileData[k] = CompressUtility.Decompress(Convert.FromBase64String(DataExposeUtility.RemoveLineBreaks(worldData.deflateDictionary[k + "Deflate"])));
                }
            }


        }

        //Modifies the existing XML file with the required details from the server

        public static void ModifyWorldXml(WorldData worldData)
        {
            string filePath = Path.Combine(new string[] { Master.savesFolderPath, SaveManager.customSaveName + ".rws" });

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            //Navigate to grid in the xml file
            XmlNode docNode = GetChildNodeInNode(doc, "savegame");
            XmlNode gameNode = GetChildNodeInNode(docNode, "game");
            XmlNode worldNode = GetChildNodeInNode(gameNode, "world");
            XmlNode gridNode = GetChildNodeInNode(worldNode, "grid");

            //World Deflates are the layers of the world generation. Save them in a dictionary
            Dictionary<string, string> worldDeflates = worldData.deflateDictionary;

            //set each deflate in the player's save to the deflate from the server
            foreach (string deflateLabel in worldDeflates.Keys)
            {
                XmlNode deflateNode = gridNode[deflateLabel];
                
                Log.Warning($"{((deflateNode != null) ? ($"deflate node {deflateLabel} exists") : ($"deflate node {deflateLabel} was not found"))})");

                //if the player does not have that deflate, don't attempt to add it (it means its from a mod they dont have)
                if (deflateNode != null)
                {
                    gridNode[deflateLabel].InnerText = worldDeflates[deflateLabel];
                }
            }


            // replace every world object with the server copy of the world object
            // this ensures the objects are in the correct location with the correct settings.
            // Objects that only exist on the player's world will not be changed

            //grab player objects
            XmlNode localWorldObjects = GetChildNodeInNode(worldNode, "worldObjects");
            localWorldObjects = GetChildNodeInNode(localWorldObjects, "worldObjects");

            //grab server objects
            XmlNode ServerWorldObjectsDoc = new XmlDocument();
            ServerWorldObjectsDoc.InnerXml = worldData.WorldObjects;
            XmlNode ServerWorldObjects = GetChildNodeInNode(ServerWorldObjectsDoc, "worldObjects");


            //foreach server object
            foreach (XmlNode ServerNode in ServerWorldObjects.ChildNodes)
            {
                //find the player object with the same ID as the server Object
                foreach (XmlNode playerNode in localWorldObjects.ChildNodes)
                {
                    if (GetChildNodeInNode(playerNode, "ID").InnerText == GetChildNodeInNode(ServerNode, "ID").InnerText)
                    {
                        playerNode.InnerXml = ServerNode.InnerXml;
                        break;
                    }
                }


            }


            doc.Save(filePath);
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