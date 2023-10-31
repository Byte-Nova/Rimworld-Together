using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameClient.Managers
{
    public static class SettlementManager
    {
        public enum SettlementStepMode { Add, Remove }

        public static void ParseSettlementPacket(Packet packet)
        {
            SettlementDetailsJSON settlementDetailsJSON = (SettlementDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch(int.Parse(settlementDetailsJSON.settlementStepMode))
            {
                case (int)SettlementStepMode.Add:
                    PlanetBuilder.SpawnSingleSettlement(settlementDetailsJSON);
                    break;

                case (int)SettlementStepMode.Remove:
                    PlanetBuilder.RemoveSingleSettlement(settlementDetailsJSON);
                    break;
            }
        }
    }
}
