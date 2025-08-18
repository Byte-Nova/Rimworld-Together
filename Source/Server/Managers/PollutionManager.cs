using GameServer.Core;
using GameServer.Misc;
using Shared;
using Shared.Files;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Managers
{

    public static class PollutionManager
    {
        [HandlesPacket(PacketHeader.PollutionManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnablePollutionSpread)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            PollutionData data = Serializer.ConvertBytesToObject<PollutionData>(bytes);
            AddPollutionToTile(data, client, true);
        }

        public static void AddPollutionToTile(PollutionData data, ServerClient client, bool shouldBroadcast)
        {
            try
            {
                bool isNewPollutedTile = false;

                PollutionDetails toSearch = Master.WorldValues.PollutedTiles.FirstOrDefault(T => T.Tile == data._pollutionData.Tile);
                if (toSearch == null)
                {
                    toSearch = new PollutionDetails();
                    isNewPollutedTile = true;
                }

                toSearch.Tile = data._pollutionData.Tile;
                toSearch.Quantity += data._pollutionData.Quantity;

                if (isNewPollutedTile)
                {
                    List<PollutionDetails> existingPollutedTiles = Master.WorldValues.PollutedTiles.ToList();
                    existingPollutedTiles.Add(toSearch);
                    Master.WorldValues.PollutedTiles = existingPollutedTiles.ToArray();
                }

                if (shouldBroadcast) ServerNetwork.Instance.SendPacketToAllClients(PacketHeader.PollutionManager, data, client);

                Master.WorldValues.Save();
            }

            catch
            {
                Printer.Warning($"Could not add pollution to tile {data}. Coming from {client.UserFile.Uid}");
            }
        }
    }
}
