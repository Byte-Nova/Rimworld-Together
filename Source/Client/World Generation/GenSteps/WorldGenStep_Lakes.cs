using System;
using System.Collections.Generic;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace GameClient
{
	public class WorldGenStep_Lakes : WorldGenStep
	{
		private const int LakeMaxSize = 15;

		public override int SeedPart
		{
			get
			{
				return 401463656;
			}
		}

		public override void GenerateFresh(string seed)
		{
			this.GenerateLakes();
		}

		private void GenerateLakes()
		{
			WorldGrid grid = Find.WorldGrid;
			bool[] touched = new bool[grid.TilesCount];
			List<int> oceanChunk = new List<int>();
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (!touched[i])
				{
					if (grid[i].biome == BiomeDefOf.Ocean)
					{
						Find.WorldFloodFiller.FloodFill(i, (int tid) => grid[tid].biome == BiomeDefOf.Ocean, delegate(int tid)
						{
							oceanChunk.Add(tid);
							touched[tid] = true;
						}, 2147483647, null);
						if (oceanChunk.Count <= 15)
						{
							for (int j = 0; j < oceanChunk.Count; j++)
							{
								grid[oceanChunk[j]].biome = BiomeDefOf.Lake;
							}
						}
						oceanChunk.Clear();
					}
				}
			}
		}
	}
}
