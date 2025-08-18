using GameClient.Misc;
using GameClient.Values;
using TCPNetwork.Packets;
using RimWorld;
using RimWorld.Planet;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient.Managers
{
    public static class NPCManager
    {
        [HandlesPacket(PacketHeader.NPCManager)]
        private static void ParsePacket(byte[] bytes)
        {
            NPCSettlementData data = Serializer.ConvertBytesToObject<NPCSettlementData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

            switch (data._stepMode)
            {
                case SettlementStepMode.Add:
                    break;

                case SettlementStepMode.Remove:
                    RemoveNPCSettlementFromPacket(data._settlementData);
                    break;
            }
        }

        public static void AddSettlements(PlanetNPCSettlementDetails[] settlements)
        {
            foreach (PlanetNPCSettlementDetails settlement in settlements)
            {
                SpawnSingleSettlement(settlement);
            }
        }

        public static void SpawnSingleSettlement(PlanetNPCSettlementDetails toAdd)
        {
            if (Find.WorldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == toAdd.Tile) != null) return;
            else
            {
                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.Name = toAdd.Name;
                settlement.Tile = toAdd.Tile;

                List<Faction> factions = PlanetManagerHelper.GetNPCFactionFromDefName(toAdd.DefName);

                if (factions.Count == 0)
                {
                    Printer.Warning($"Could not find faction for settlement at tile {toAdd.Tile} with faction {toAdd.DefName}");
                    return;
                }

                else if (factions.Count == 1) settlement.SetFaction(factions.First());

                else if (factions.Count > 1)
                {
                    foreach (Faction faction in factions)
                    {
                        if (faction.Name == toAdd.FactionName) settlement.SetFaction(faction);
                    }

                    if (settlement.Faction == null) settlement.SetFaction(factions.First());
                }

                // Check if the settlement belongs to planet or space

                if (ModLister.OdysseyInstalled && settlement.Faction.def == FactionDefOf.TradersGuild)
                {
                    PlanetLayer orbitLayer = Find.World.grid.FirstLayerOfDef(PlanetLayerDefOf.Orbit);
                    Tile toFind = orbitLayer.Tiles.FirstOrDefault(fetch => fetch.tile.tileId == toAdd.Tile);
                    settlement.Tile = toFind.tile;
                }

                Find.WorldObjects.Add(settlement);

                NPCManagerH.TryRelinkQuest(settlement);
            }
        }

        public static void ClearAllSettlements()
        {
            Settlement[] settlements = Find.WorldObjects.Settlements.Where(fetch => !ClientValues.PlayerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (Settlement settlement in settlements) RemoveSingleSettlement(settlement, null);

            DestroyedSettlement[] destroyedSettlements = Find.WorldObjects.DestroyedSettlements.Where(fetch => !ClientValues.PlayerFactions.Contains(fetch.Faction) &&
                fetch.Faction != Faction.OfPlayer).ToArray();

            foreach (DestroyedSettlement settlement in destroyedSettlements) RemoveSingleSettlement(null, settlement);
        }

        public static void RemoveNPCSettlementFromPacket(PlanetNPCSettlementDetails data)
        {
            Settlement toRemove = Find.World.worldObjects.Settlements.FirstOrDefault(fetch => fetch.Tile == data.Tile &&
                fetch.Faction != Faction.OfPlayer);

            if (toRemove != null) RemoveSingleSettlement(toRemove, null);
        }

        public static void RemoveSingleSettlement(Settlement settlement, DestroyedSettlement destroyedSettlement)
        {
            if (settlement != null)
            {
                try
                {
                    if (!RimworldManager.CheckIfMapHasPlayerPawns(settlement.Map))
                    {
                        NPCManagerH.lastRemovedSettlement = settlement;
                        Find.WorldObjects.Remove(settlement);
                    }
                    else Printer.Warning($"Ignored removal of settlement at {settlement.Tile} because player was inside");
                }
                catch (Exception e) { Printer.Warning($"Failed to remove NPC settlement at {settlement.Tile}. Reason: {e}"); }
            }

            else if (destroyedSettlement != null)
            {
                try
                {
                    if (!RimworldManager.CheckIfMapHasPlayerPawns(destroyedSettlement.Map))
                    {
                        Find.WorldObjects.Remove(destroyedSettlement);
                    }
                    else Printer.Warning($"Ignored removal of settlement at {destroyedSettlement.Tile} because player was inside");
                }
                catch (Exception e) { Printer.Warning($"Failed to remove NPC settlement at {destroyedSettlement.Tile}. Reason: {e}"); }
            }
        }

        public static void RequestSettlementRemoval(Settlement settlement)
        {
            NPCSettlementData data = new NPCSettlementData();
            data._stepMode = SettlementStepMode.Remove;
            data._settlementData.Tile = settlement.Tile;

            ClientNetwork.Instance.ClientListener.EnqueuePacket(PacketHeader.NPCManager, data);
        }
    }

    public static class NPCManagerH
    {
        public static PlanetNPCSettlementDetails[] tempNPCSettlements;

        public static Settlement lastRemovedSettlement;

        private static Dictionary<int, List<QuestPart>> questToFixTemp = new Dictionary<int, List<QuestPart>>();

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            tempNPCSettlements = serverGlobalData._npcSettlements;
        }

        public static void SaveAllQuests() 
        {
            foreach (Quest quest in Find.QuestManager.QuestsListForReading.Where(x => !x.Historical)) 
            {
                TrySaveQuest(quest);
            }
        }

        public static void CleanupQuests() 
        {
            questToFixTemp.Clear();
        }

        private static void TrySaveQuest(Quest quest)
        {
            Printer.Warning($"Trying to save quest with id {quest.id}", LogImportanceMode.Verbose);

            IEnumerable<QuestPart> questPart = quest.PartsListForReading.Where(x => x is QuestPart_SpawnWorldObject || x is QuestPart_DisableTradeRequest
                || x is QuestPart_TradeRequestInactive || x is QuestPart_InitiateTradeRequest);

            foreach (QuestPart part in questPart) 
            {
                int tile = -1;

                if (part is QuestPart_SpawnWorldObject part2) 
                {
                    Printer.Warning($"Found {typeof(QuestPart_SpawnWorldObject).Name}!", LogImportanceMode.Verbose);
                    tile = part2.worldObject?.Tile ?? -1;
                }

                else if (part is QuestPart_DisableTradeRequest part3) 
                {
                    Printer.Warning($"Found {typeof(QuestPart_DisableTradeRequest).Name}!", LogImportanceMode.Verbose);
                    tile = part3.settlement?.Tile ?? -1;
                }

                else if (part is QuestPart_TradeRequestInactive part4) 
                {
                    Printer.Warning($"Found {typeof(QuestPart_TradeRequestInactive).Name}!", LogImportanceMode.Verbose);
                    tile = part4.settlement?.Tile ?? -1;
                }

                else if (part is QuestPart_InitiateTradeRequest part5)
                {
                    Printer.Warning($"Found {typeof(QuestPart_InitiateTradeRequest).Name}!", LogImportanceMode.Verbose);
                    tile = part5.settlement?.Tile ?? -1;
                }

                if (tile != -1)
                {
                    Printer.Warning($"Saved quest {quest.id}", LogImportanceMode.Verbose);
                    if (questToFixTemp.ContainsKey(tile))
                    {
                        questToFixTemp[tile].Add(part);
                    }
                    else
                    {
                        questToFixTemp[tile] = new List<QuestPart>() { part };
                    }
                }
            }
        }

        public static void TryRelinkQuest(WorldObject obj) 
        {
            try
            {
                if (questToFixTemp.TryGetValue(obj.Tile, out List<QuestPart> parts))
                {
                    Printer.Warning($"Found quest with id {parts.First().quest.id}", LogImportanceMode.Verbose);

                    foreach (QuestPart part in parts)
                    {
                        if (part is QuestPart_SpawnWorldObject part2)
                        {
                            Printer.Warning($"Found {typeof(QuestPart_SpawnWorldObject).Name}!", LogImportanceMode.Verbose);
                            part2.worldObject = obj;
                        }

                        else if (part is QuestPart_DisableTradeRequest part3)
                        {
                            Printer.Warning($"Found {typeof(QuestPart_DisableTradeRequest).Name}!", LogImportanceMode.Verbose);
                            part3.settlement = (Settlement)obj;
                        }

                        else if (part is QuestPart_TradeRequestInactive part4)
                        {
                            Printer.Warning($"Found {typeof(QuestPart_TradeRequestInactive).Name}!", LogImportanceMode.Verbose);
                            part4.settlement = (Settlement)obj;
                        }

                        else if (part is QuestPart_InitiateTradeRequest part5)
                        {
                            Printer.Warning($"Found {typeof(QuestPart_InitiateTradeRequest).Name}!", LogImportanceMode.Verbose);
                            part5.settlement = (Settlement)obj;
                        }
                    }

                    Printer.Warning($"Loaded quest with id {parts.First().quest.id} on tile {obj.Tile}.", LogImportanceMode.Verbose);
                }
            }

            catch (Exception ex)
            {
                Printer.Error($"Error while trying to relink quests\n{ex}");
            }
        }
    }
}
