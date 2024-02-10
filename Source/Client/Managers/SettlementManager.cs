using RimworldTogether.GameClient.Planet;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameClient.Managers
{
    public static class SettlementManager
    {
        public static void ParseSettlementPacket(Packet packet)
        {
            SettlementDetailsJSON settlementDetailsJSON = (SettlementDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch(int.Parse(settlementDetailsJSON.settlementStepMode))
            {
                case (int)CommonEnumerators.SettlementStepMode.Add:
                    PlanetBuilder.SpawnSingleSettlement(settlementDetailsJSON);
                    break;

                case (int)CommonEnumerators.SettlementStepMode.Remove:
                    PlanetBuilder.RemoveSingleSettlement(settlementDetailsJSON);
                    break;
            }
        }
    }
}
