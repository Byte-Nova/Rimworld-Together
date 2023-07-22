using Shared.JSON;
using Shared.Misc;

namespace RimworldTogether
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
