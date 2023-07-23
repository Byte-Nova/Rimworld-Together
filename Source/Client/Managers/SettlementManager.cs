using RimworldTogether.GameClient.Misc;
using RimworldTogether.GameClient.Planet;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameClient.Managers
{
    public static class SettlementManager
    {
        public enum SettlementStepMode { Add, Remove }

        public static void ParseSettlementPacket(Packet packet)
        {
            SettlementDetailsJSON settlementDetailsJSON = Serializer.SerializeFromString<SettlementDetailsJSON>(packet.contents[0]);

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
