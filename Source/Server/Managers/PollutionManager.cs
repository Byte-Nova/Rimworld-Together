using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class PollutionManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            PollutionData data = Serializer.ConvertBytesToObject<PollutionData>(packet.contents);
            AddPollutionToTile(data, client, true);
        }
        public static void AddPollutionToTile(PollutionData data,ServerClient client , bool shouldBroadcast = false)
        {
            try
            {
                if (Master.serverConfig.AllowBiotechPollutionModification)
                {
                    bool wasNull = false;
                    PollutionDetails toSearch = Master.worldValues.PollutedTiles.Where(T => T.tile == data._pollutionData.tile).FirstOrDefault();
                    if (toSearch == null)
                    {
                        wasNull = true;
                        toSearch = new PollutionDetails();
                    }
                    toSearch.tile = data._pollutionData.tile;
                    toSearch.quantity += data._pollutionData.quantity;
                    if (shouldBroadcast)
                    {
                        Packet packet = Packet.CreatePacketFromObject(nameof(PollutionManager), data);
                        NetworkHelper.SendPacketToAllClients(packet, client);
                    }
                    if (wasNull) 
                    {
                        List<PollutionDetails> temp = Master.worldValues.PollutedTiles.ToList();
                        temp.Add(toSearch);
                        Master.worldValues.PollutedTiles = temp.ToArray();
                    }
                    Main_.SaveValueFile(ServerFileMode.World, false);
                }
            } 
            catch 
            {
                Logger.Warning($"Could not add pollution to tile {data._pollutionData.tile}. Coming from {client.userFile.Username}");
                Logger.Warning($"Additional debugging info here:\n{StringUtilities.ToString(data)}", LogImportanceMode.Verbose);
            }
        }
    }
}
