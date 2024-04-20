using System;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace GameClient
{
	public class WorldGenStep_AncientSites : WorldGenStep
	{
		public FloatRange ancientSitesPer100kTiles;

		public override int SeedPart
		{
			get
			{
				return 976238715;
			}
		}

		public override void GenerateFresh(string seed)
		{
			this.GenerateAncientSites();
		}

		private void GenerateAncientSites()
		{
			int num = GenMath.RoundRandom((float)Find.WorldGrid.TilesCount / 100000f * this.ancientSitesPer100kTiles.RandomInRange);
			for (int i = 0; i < num; i++)
			{
				Find.World.genData.ancientSites.Add(TileFinder.RandomSettlementTileFor(null, false, null));
			}
		}
	}
}
