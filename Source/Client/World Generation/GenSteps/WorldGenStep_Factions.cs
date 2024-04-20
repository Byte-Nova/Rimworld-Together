using System;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace GameClient
{
	public class WorldGenStep_Factions : WorldGenStep
	{
		public override int SeedPart
		{
			get
			{
				return 777998381;
			}
		}

		public override void GenerateFresh(string seed)
		{
            FactionGenerator.GenerateFactionsIntoWorld(Current.CreatingWorld.info.factions);
		}

		public override void GenerateWithoutWorldData(string seed)
		{
		}
	}
}
