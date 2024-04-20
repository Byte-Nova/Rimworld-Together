using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace GameClient
{
	public class WorldGenStep_Roads : WorldGenStep
	{
		private struct Link
		{
			public float distance;

			public int indexA;

			public int indexB;
		}

		private class Connectedness
		{
			public WorldGenStep_Roads.Connectedness parent;

			public WorldGenStep_Roads.Connectedness Group()
			{
				if (this.parent == null)
				{
					return this;
				}
				return this.parent.Group();
			}
		}

		private static readonly FloatRange ExtraRoadNodesPer100kTiles = new FloatRange(30f, 50f);

		private static readonly IntRange RoadDistanceFromSettlement = new IntRange(-4, 4);

		private const float ChanceExtraNonSpanningTreeLink = 0.015f;

		private const float ChanceHideSpanningTreeLink = 0.1f;

		private const float ChanceWorldObjectReclusive = 0.05f;

		private const int PotentialSpanningTreeLinksPerSettlement = 8;

		public override int SeedPart
		{
			get
			{
				return 1538475135;
			}
		}

		public override void GenerateFresh(string seed)
		{
			this.GenerateRoadEndpoints();
			Rand.PushState();
			Rand.Seed = GenText.StableStringHash(seed);
			this.GenerateRoadNetwork();
			Rand.PopState();
		}

		public override void GenerateWithoutWorldData(string seed)
		{
			Rand.PushState();
			Rand.Seed = GenText.StableStringHash(seed);
			this.GenerateRoadNetwork();
			Rand.PopState();
		}

		private void GenerateRoadEndpoints()
		{
			List<int> list = (from wo in Find.WorldObjects.AllWorldObjects
			where Rand.Value > 0.05f
			select wo.Tile).ToList<int>();
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * WorldGenStep_Roads.ExtraRoadNodesPer100kTiles.RandomInRange);
			for (int i = 0; i < num; i++)
			{
				list.Add(TileFinder.RandomSettlementTileFor(null, false, null));
			}
			List<int> list2 = new List<int>();
			for (int j = 0; j < list.Count; j++)
			{
				int num2 = Mathf.Max(0, WorldGenStep_Roads.RoadDistanceFromSettlement.RandomInRange);
				int num3 = list[j];
				for (int k = 0; k < num2; k++)
				{
					Find.WorldGrid.GetTileNeighbors(num3, list2);
					num3 = list2.RandomElement<int>();
				}
				if (Find.WorldReachability.CanReach(list[j], num3))
				{
					list[j] = num3;
				}
			}
			list = list.Distinct<int>().ToList<int>();
			Find.World.genData.roadNodes = list;
		}

		private void GenerateRoadNetwork()
		{
			Find.WorldPathGrid.RecalculateAllPerceivedPathCosts(new int?(0));
			List<WorldGenStep_Roads.Link> linkProspective = this.GenerateProspectiveLinks(Find.World.genData.roadNodes);
			List<WorldGenStep_Roads.Link> linkFinal = this.GenerateFinalLinks(linkProspective, Find.World.genData.roadNodes.Count);
			this.DrawLinksOnWorld(linkFinal, Find.World.genData.roadNodes);
		}

		private List<WorldGenStep_Roads.Link> GenerateProspectiveLinks(List<int> indexToTile)
		{
			Dictionary<int, int> tileToIndexLookup = new Dictionary<int, int>();
			for (int i = 0; i < indexToTile.Count; i++)
			{
				tileToIndexLookup[indexToTile[i]] = i;
			}
			List<WorldGenStep_Roads.Link> linkProspective = new List<WorldGenStep_Roads.Link>();
			List<int> list = new List<int>();
			int srcIndex;
			for (srcIndex = 0; srcIndex < indexToTile.Count; srcIndex++)
			{
				int srcTile = indexToTile[srcIndex];
				list.Clear();
				list.Add(srcTile);
				int found = 0;
				Find.WorldPathFinder.FloodPathsWithCost(list, (int src, int dst) => Caravan_PathFollower.CostToMove(3300, src, dst, null, true, null, null), null, delegate(int tile, float distance)
				{
					if (tile != srcTile && tileToIndexLookup.ContainsKey(tile))
					{
						found++;
						linkProspective.Add(new WorldGenStep_Roads.Link
						{
							distance = distance,
							indexA = srcIndex,
							indexB = tileToIndexLookup[tile]
						});
					}
					return found >= 8;
				});
			}
			linkProspective.Sort((WorldGenStep_Roads.Link lhs, WorldGenStep_Roads.Link rhs) => lhs.distance.CompareTo(rhs.distance));
			return linkProspective;
		}

		private List<WorldGenStep_Roads.Link> GenerateFinalLinks(List<WorldGenStep_Roads.Link> linkProspective, int endpointCount)
		{
			List<WorldGenStep_Roads.Connectedness> list = new List<WorldGenStep_Roads.Connectedness>();
			for (int i = 0; i < endpointCount; i++)
			{
				list.Add(new WorldGenStep_Roads.Connectedness());
			}
			List<WorldGenStep_Roads.Link> list2 = new List<WorldGenStep_Roads.Link>();
			int j = 0;
			while (j < linkProspective.Count)
			{
				WorldGenStep_Roads.Link prospective = linkProspective[j];
				if (list[prospective.indexA].Group() != list[prospective.indexB].Group())
				{
					goto IL_A9;
				}
				if (Rand.Value <= 0.015f)
				{
					if (!list2.Any((WorldGenStep_Roads.Link link) => link.indexB == prospective.indexA && link.indexA == prospective.indexB))
					{
						goto IL_A9;
					}
				}
				IL_13B:
				j++;
				continue;
				IL_A9:
				if (Rand.Value > 0.1f)
				{
					list2.Add(prospective);
				}
				if (list[prospective.indexA].Group() != list[prospective.indexB].Group())
				{
					WorldGenStep_Roads.Connectedness parent = new WorldGenStep_Roads.Connectedness();
					list[prospective.indexA].Group().parent = parent;
					list[prospective.indexB].Group().parent = parent;
					goto IL_13B;
				}
				goto IL_13B;
			}
			return list2;
		}

		private void DrawLinksOnWorld(List<WorldGenStep_Roads.Link> linkFinal, List<int> indexToTile)
		{
			foreach (WorldGenStep_Roads.Link current in linkFinal)
			{
				WorldPath worldPath = Find.WorldPathFinder.FindPath(indexToTile[current.indexA], indexToTile[current.indexB], null, null);
				List<int> nodesReversed = worldPath.NodesReversed;
				RoadDef roadDef = (from rd in DefDatabase<RoadDef>.AllDefsListForReading
				where !rd.ancientOnly
				select rd).RandomElementWithFallback(null);
				for (int i = 0; i < nodesReversed.Count - 1; i++)
				{
					Find.WorldGrid.OverlayRoad(nodesReversed[i], nodesReversed[i + 1], roadDef);
				}
				worldPath.ReleaseToPool();
			}
		}
	}
}
