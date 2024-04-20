using System;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace GameClient
{
	public class WorldGenStep_Features : WorldGenStep
	{
		public override int SeedPart
		{
			get
			{
				return 711240483;
			}
		}

		public override void GenerateFresh(string seed)
		{
			Find.World.features = new WorldFeatures();
			IOrderedEnumerable<FeatureDef> orderedEnumerable = from x in DefDatabase<FeatureDef>.AllDefsListForReading
			orderby x.order, x.index
			select x;
			foreach (FeatureDef current in orderedEnumerable)
			{
				try
				{
					current.Worker.GenerateWhereAppropriate();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not generate world features of def ",
						current,
						": ",
						ex
					}));
				}
			}
		}
	}
}
