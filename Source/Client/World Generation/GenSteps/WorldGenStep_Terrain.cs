using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Noise;
using Verse;

namespace GameClient
{
  internal class WorldGenStep_Terrain : WorldGenStep
  {
    [Unsaved(false)]
    private ModuleBase noiseElevation;

    [Unsaved(false)]
    private ModuleBase noiseTemperatureOffset;

    [Unsaved(false)]
    private ModuleBase noiseRainfall;

    [Unsaved(false)]
    private ModuleBase noiseSwampiness;

    [Unsaved(false)]
    private ModuleBase noiseMountainLines;

    [Unsaved(false)]
    private ModuleBase noiseHillsPatchesMicro;

    [Unsaved(false)]
    private ModuleBase noiseHillsPatchesMacro;

    private const float ElevationFrequencyMicro = 0.035f;

    private const float ElevationFrequencyMacro = 0.012f;

    private const float ElevationMacroFactorFrequency = 0.12f;

    private const float ElevationContinentsFrequency = 0.01f;

    private const float MountainLinesFrequency = 0.025f;

    private const float MountainLinesHolesFrequency = 0.06f;

    private const float HillsPatchesFrequencyMicro = 0.19f;

    private const float HillsPatchesFrequencyMacro = 0.032f;

    private const float SwampinessFrequencyMacro = 0.025f;

    private const float SwampinessFrequencyMicro = 0.09f;

    private static readonly FloatRange SwampinessMaxElevation = new FloatRange(650f, 750f);

    private static readonly FloatRange SwampinessMinRainfall = new FloatRange(725f, 900f);

    private static readonly FloatRange ElevationRange = new FloatRange(-500f, 5000f);

    private const float TemperatureOffsetFrequency = 0.018f;

    private const float TemperatureOffsetFactor = 4f;

    private static readonly SimpleCurve AvgTempByLatitudeCurve = new SimpleCurve
  {
    new CurvePoint(0f, 30f),
    new CurvePoint(0.1f, 29f),
    new CurvePoint(0.5f, 7f),
    new CurvePoint(1f, -37f)
  };

    private const float ElevationTempReductionStartAlt = 250f;

    private const float ElevationTempReductionEndAlt = 5000f;

    private const float MaxElevationTempReduction = 40f;

    private const float RainfallOffsetFrequency = 0.013f;

    private const float RainfallPower = 1.5f;

    private const float RainfallFactor = 4000f;

    private const float RainfallStartFallAltitude = 500f;

    private const float RainfallFinishFallAltitude = 5000f;

    private const float FertilityTempMinimum = -15f;

    private const float FertilityTempOptimal = 30f;

    private const float FertilityTempMaximum = 50f;

    public override int SeedPart => 83469557;

    private static float FreqMultiplier => 1f;

    public override void GenerateFresh(string seed)
    {
      GenerateGridIntoWorld();
    }

    public override void GenerateFromScribe(string seed)
    {
      Find.World.pathGrid = new WorldPathGrid();
      NoiseDebugUI.ClearPlanetNoises();
    }

    private void GenerateGridIntoWorld()
    {
      Find.World.grid = new WorldGrid();
      Find.World.pathGrid = new WorldPathGrid();
      NoiseDebugUI.ClearPlanetNoises();
      SetupElevationNoise();
      SetupTemperatureOffsetNoise();
      SetupRainfallNoise();
      SetupHillinessNoise();
      SetupSwampinessNoise();
      List<Tile> tiles = Find.WorldGrid.tiles;
      tiles.Clear();
      int tilesCount = Find.WorldGrid.TilesCount;
      for (int i = 0; i < tilesCount; i++)
      {
        Tile item = GenerateTileFor(i);
        tiles.Add(item);
      }
    }

    private void SetupElevationNoise()
    {
      float freqMultiplier = FreqMultiplier;
      ModuleBase lhs = new Perlin(0.035f * freqMultiplier, 2.0, 0.4000000059604645, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      ModuleBase lhs2 = new RidgedMultifractal(0.012f * freqMultiplier, 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      ModuleBase input = new Perlin(0.12f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.High);
      ModuleBase moduleBase = new Perlin(0.01f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.High);
      float num;
      if (Find.World.PlanetCoverage < 0.55f)
      {
        ModuleBase input2 = new DistanceFromPlanetViewCenter(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, invert: true);
        input2 = new ScaleBias(2.0, -1.0, input2);
        moduleBase = new Blend(moduleBase, input2, new Const(0.4000000059604645));
        num = Rand.Range(-0.4f, -0.35f);
      }
      else
      {
        num = Rand.Range(0.15f, 0.25f);
      }
      NoiseDebugUI.StorePlanetNoise(moduleBase, "elevContinents");
      input = new ScaleBias(0.5, 0.5, input);
      lhs2 = new Multiply(lhs2, input);
      float num2 = Rand.Range(0.4f, 0.6f);
      noiseElevation = new Blend(lhs, lhs2, new Const(num2));
      noiseElevation = new Blend(noiseElevation, moduleBase, new Const(num));
      if (Find.World.PlanetCoverage < 0.9999f)
      {
        noiseElevation = new ConvertToIsland(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, noiseElevation);
      }
      noiseElevation = new ScaleBias(0.5, 0.5, noiseElevation);
      noiseElevation = new Power(noiseElevation, new Const(3.0));
      NoiseDebugUI.StorePlanetNoise(noiseElevation, "noiseElevation");
      noiseElevation = new ScaleBias(ElevationRange.Span, ElevationRange.min, noiseElevation);
    }

    private void SetupTemperatureOffsetNoise()
    {
      float freqMultiplier = FreqMultiplier;
      noiseTemperatureOffset = new Perlin(0.018f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      noiseTemperatureOffset = new Multiply(noiseTemperatureOffset, new Const(4.0));
    }

    private void SetupRainfallNoise()
    {
      float freqMultiplier = FreqMultiplier;
      ModuleBase input = new Perlin(0.015f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      input = new ScaleBias(0.5, 0.5, input);
      NoiseDebugUI.StorePlanetNoise(input, "basePerlin");
      ModuleBase moduleBase = new AbsLatitudeCurve(new SimpleCurve
    {
      { 0f, 1.12f },
      { 25f, 0.94f },
      { 45f, 0.7f },
      { 70f, 0.3f },
      { 80f, 0.05f },
      { 90f, 0.05f }
    }, 100f);
      NoiseDebugUI.StorePlanetNoise(moduleBase, "latCurve");
      noiseRainfall = new Multiply(input, moduleBase);
      float num = 0.00022222222f;
      float num2 = -500f * num;
      ModuleBase input2 = new ScaleBias(num, num2, noiseElevation);
      input2 = new ScaleBias(-1.0, 1.0, input2);
      input2 = new Clamp(0.0, 1.0, input2);
      NoiseDebugUI.StorePlanetNoise(input2, "elevationRainfallEffect");
      noiseRainfall = new Multiply(noiseRainfall, input2);
      Func<double, double> processor = delegate (double val)
      {
        if (val < 0.0)
        {
          val = 0.0;
        }
        if (val < 0.12)
        {
          val = (val + 0.12) / 2.0;
          if (val < 0.03)
          {
            val = (val + 0.03) / 2.0;
          }
        }
        return val;
      };
      noiseRainfall = new Arbitrary(noiseRainfall, processor);
      noiseRainfall = new Power(noiseRainfall, new Const(1.5));
      noiseRainfall = new Clamp(0.0, 999.0, noiseRainfall);
      NoiseDebugUI.StorePlanetNoise(noiseRainfall, "noiseRainfall before mm");
      noiseRainfall = new ScaleBias(4000.0, 0.0, noiseRainfall);
      SimpleCurve rainfallCurve = Find.World.info.overallRainfall.GetRainfallCurve();
      if (rainfallCurve != null)
      {
        noiseRainfall = new CurveSimple(noiseRainfall, rainfallCurve);
      }
    }

    private void SetupHillinessNoise()
    {
      float freqMultiplier = FreqMultiplier;
      noiseMountainLines = new Perlin(0.025f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      ModuleBase module = new Perlin(0.06f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      noiseMountainLines = new Abs(noiseMountainLines);
      noiseMountainLines = new OneMinus(noiseMountainLines);
      module = new Filter(module, -0.3f, 1f);
      noiseMountainLines = new Multiply(noiseMountainLines, module);
      noiseMountainLines = new OneMinus(noiseMountainLines);
      NoiseDebugUI.StorePlanetNoise(noiseMountainLines, "noiseMountainLines");
      noiseHillsPatchesMacro = new Perlin(0.032f * freqMultiplier, 2.0, 0.5, 5, Rand.Range(0, int.MaxValue), QualityMode.Medium);
      noiseHillsPatchesMicro = new Perlin(0.19f * freqMultiplier, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
    }

    private void SetupSwampinessNoise()
    {
      float freqMultiplier = FreqMultiplier;
      ModuleBase input = new Perlin(0.09f * freqMultiplier, 2.0, 0.4000000059604645, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      ModuleBase input2 = new RidgedMultifractal(0.025f * freqMultiplier, 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
      input = new ScaleBias(0.5, 0.5, input);
      input2 = new ScaleBias(0.5, 0.5, input2);
      noiseSwampiness = new Multiply(input, input2);
      InverseLerp rhs = new InverseLerp(noiseElevation, SwampinessMaxElevation.max, SwampinessMaxElevation.min);
      noiseSwampiness = new Multiply(noiseSwampiness, rhs);
      InverseLerp rhs2 = new InverseLerp(noiseRainfall, SwampinessMinRainfall.min, SwampinessMinRainfall.max);
      noiseSwampiness = new Multiply(noiseSwampiness, rhs2);
      NoiseDebugUI.StorePlanetNoise(noiseSwampiness, "noiseSwampiness");
    }

    private Tile GenerateTileFor(int tileID)
    {
      Tile tile = new Tile();
      Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
      tile.elevation = noiseElevation.GetValue(tileCenter);
      float value = noiseMountainLines.GetValue(tileCenter);
      if (value > 0.235f || tile.elevation <= 0f)
      {
        if (tile.elevation > 0f && noiseHillsPatchesMicro.GetValue(tileCenter) > 0.46f && noiseHillsPatchesMacro.GetValue(tileCenter) > -0.3f)
        {
          if (Rand.Bool)
          {
            tile.hilliness = Hilliness.SmallHills;
          }
          else
          {
            tile.hilliness = Hilliness.LargeHills;
          }
        }
        else
        {
          tile.hilliness = Hilliness.Flat;
        }
      }
      else if (value > 0.12f)
      {
        switch (Rand.Range(0, 4))
        {
          case 0:
            tile.hilliness = Hilliness.Flat;
            break;
          case 1:
            tile.hilliness = Hilliness.SmallHills;
            break;
          case 2:
            tile.hilliness = Hilliness.LargeHills;
            break;
          case 3:
            tile.hilliness = Hilliness.Mountainous;
            break;
        }
      }
      else if (value > 0.0363f)
      {
        tile.hilliness = Hilliness.Mountainous;
      }
      else
      {
        tile.hilliness = Hilliness.Impassable;
      }
      float num = BaseTemperatureAtLatitude(Find.WorldGrid.LongLatOf(tileID).y);
      num -= TemperatureReductionAtElevation(tile.elevation);
      num += noiseTemperatureOffset.GetValue(tileCenter);
      SimpleCurve temperatureCurve = Find.World.info.overallTemperature.GetTemperatureCurve();
      if (temperatureCurve != null)
      {
        num = temperatureCurve.Evaluate(num);
      }
      tile.temperature = num;
      tile.rainfall = noiseRainfall.GetValue(tileCenter);
      if (float.IsNaN(tile.rainfall))
      {
        Log.ErrorOnce(noiseRainfall.GetValue(tileCenter) + " rain bad at " + tileID, 694822);
      }
      if (tile.hilliness == Hilliness.Flat || tile.hilliness == Hilliness.SmallHills)
      {
        tile.swampiness = noiseSwampiness.GetValue(tileCenter);
      }
      tile.biome = BiomeFrom(tile, tileID);
      return tile;
    }

    private BiomeDef BiomeFrom(Tile ws, int tileID)
    {
      List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
      BiomeDef biomeDef = null;
      float num = 0f;
      for (int i = 0; i < allDefsListForReading.Count; i++)
      {
        BiomeDef biomeDef2 = allDefsListForReading[i];
        if (biomeDef2.implemented && biomeDef2.generatesNaturally)
        {
          float score = biomeDef2.Worker.GetScore(ws, tileID);
          if (score > num || biomeDef == null)
          {
            biomeDef = biomeDef2;
            num = score;
          }
        }
      }
      return biomeDef;
    }

    private static float FertilityFactorFromTemperature(float temp)
    {
      if (temp < -15f)
      {
        return 0f;
      }
      if (temp < 30f)
      {
        return Mathf.InverseLerp(-15f, 30f, temp);
      }
      if (temp < 50f)
      {
        return Mathf.InverseLerp(50f, 30f, temp);
      }
      return 0f;
    }

    private static float BaseTemperatureAtLatitude(float lat)
    {
      float x = Mathf.Abs(lat) / 90f;
      return AvgTempByLatitudeCurve.Evaluate(x);
    }

    private static float TemperatureReductionAtElevation(float elev)
    {
      if (elev < 250f)
      {
        return 0f;
      }
      float t = (elev - 250f) / 4750f;
      return Mathf.Lerp(0f, 40f, t);
    }
  }
}
