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
            if (!Master.actionValues.EnablePollutionSpread)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            PollutionData data = Serializer.ConvertBytesToObject<PollutionData>(packet.contents);
            AddPollutionToTile(data, client, true);
        }

        public static void AddPollutionToTile(PollutionData data, ServerClient client, bool shouldBroadcast)
        {
            try
            {
                bool isNewPollutedTile = false;

                PollutionDetails toSearch = Master.worldValues.PollutedTiles.FirstOrDefault(T => T.tile == data._pollutionData.tile);
                if (toSearch == null)
                {
                    toSearch = new PollutionDetails();
                    isNewPollutedTile = true;
                }

                toSearch.tile = data._pollutionData.tile;
                toSearch.quantity += data._pollutionData.quantity;

                if (isNewPollutedTile)
                {
                    List<PollutionDetails> existingPollutedTiles = Master.worldValues.PollutedTiles.ToList();
                    existingPollutedTiles.Add(toSearch);
                    Master.worldValues.PollutedTiles = existingPollutedTiles.ToArray();
                }

                if (shouldBroadcast)
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(PollutionManager), data);
                    NetworkHelper.SendPacketToAllClients(packet, client);
                }

                Main_.SaveValueFile(ServerFileMode.World, false);
            }

            catch 
            {
                Logger.Warning($"Could not add pollution to tile {data._pollutionData.tile}. Coming from {client.userFile.Username}");
                Logger.Warning($"Additional debugging info here:\n{StringUtilities.ToString(data)}", LogImportanceMode.Verbose);
            }
        }
    }
}
