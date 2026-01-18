using GameClient;
using GameClient.Core.Configs;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GameClient.Misc
{
    public static class DiagnosticsHandler
    {
        public static int PlayerNetworkRateBits { get; private set; } = 0;

        public static int TotalPlayerNetworkRateBits { get; private set; } = 0;

        private static int PlayerNetworkRateBuffer { get; set; } = 0;

        public static int PlayerNetworkReadRateMs { get; private set; } = 0;

        public static int PlayerNetworkWriteRateMs { get; private set; } = 0;

        private static float UpdateCurrentTime { get; set; } = 0;

        private static float UpdateTargetTime { get; set; } = 1.0f;

        private static Stopwatch ReadingStopwatch { get; set; } = new Stopwatch();

        private static Stopwatch WritingStopwatch { get; set; } = new Stopwatch();

        [OnSessionStart]
        private static void Initialize()
        {
            PlayerNetworkRateBits = 0;
            PlayerNetworkRateBuffer = 0;
            PlayerNetworkReadRateMs = 0;
            PlayerNetworkWriteRateMs = 0;
            TotalPlayerNetworkRateBits = 0;
            UpdateCurrentTime = 0;

            ReadingStopwatch = new Stopwatch();
            WritingStopwatch = new Stopwatch();
        }

        [OnUpdate]
        private static void CalculateNetworkRate()
        {
            UpdateCurrentTime += Time.deltaTime;

            if (UpdateCurrentTime > UpdateTargetTime)
            {
                PlayerNetworkRateBits = PlayerNetworkRateBuffer;
                TotalPlayerNetworkRateBits += PlayerNetworkRateBits;
                PlayerNetworkRateBuffer = 0;
                UpdateCurrentTime = 0;
            }
        }

        public static void IncreaseNetworkRate(int toAdd) { PlayerNetworkRateBuffer += toAdd * 8; }

        public static void ToggleReadStopwatch(bool mode)
        {
            if (mode) ReadingStopwatch.Start();
            else
            {
                ReadingStopwatch.Stop();
                PlayerNetworkReadRateMs = (int)ReadingStopwatch.ElapsedMilliseconds;
                ReadingStopwatch.Restart();
            }
        }

        public static void ToggleWriteStopwatch(bool mode)
        {
            if (mode) WritingStopwatch.Start();
            else
            {
                WritingStopwatch.Stop();
                PlayerNetworkWriteRateMs = (int)WritingStopwatch.ElapsedMilliseconds;
                WritingStopwatch.Restart();
            }
        }

        public static void DrawDiagnosticsUI()
        {
            float defaultMargin = 8.0f;
            float lineHeight = defaultMargin;

            string toDisplay = $"Total network usage rate: {(int)(TotalPlayerNetworkRateBits / 1000f)} kbps";
            Vector2 size = Text.CalcSize(toDisplay);
            Vector2 position = new Vector2(UI.screenWidth - size.x - defaultMargin, lineHeight);
            Widgets.Label(new Rect(position, size), toDisplay);

            toDisplay = $"Network usage rate: {(int)(PlayerNetworkRateBits / 1000f)} kbps";
            size = Text.CalcSize(toDisplay);
            lineHeight += size.y;
            position = new Vector2(UI.screenWidth - size.x - defaultMargin, lineHeight);
            Widgets.Label(new Rect(position, size), toDisplay);

            toDisplay = $"Network read rate: {PlayerNetworkReadRateMs} ms";
            size = Text.CalcSize(toDisplay);
            lineHeight += size.y;
            position = new Vector2(UI.screenWidth - size.x - defaultMargin, lineHeight);
            Widgets.Label(new Rect(position, size), toDisplay);

            toDisplay = $"Network write rate: {PlayerNetworkWriteRateMs} ms";
            size = Text.CalcSize(toDisplay);
            lineHeight += size.y;
            position = new Vector2(UI.screenWidth - size.x - defaultMargin, lineHeight);
            Widgets.Label(new Rect(position, size), toDisplay);

            toDisplay = $"Random seed value: {ReflectionHandler.GetPrivateField(typeof(Rand), null, "seed", true)}";
            size = Text.CalcSize(toDisplay);
            lineHeight += size.y;
            position = new Vector2(UI.screenWidth - size.x - defaultMargin, lineHeight);
            Widgets.Label(new Rect(position, size), toDisplay);

            toDisplay = $"Tick count: {Find.TickManager.TicksSinceSettle}";
            size = Text.CalcSize(toDisplay);
            lineHeight += size.y;
            position = new Vector2(UI.screenWidth - size.x - defaultMargin, lineHeight);
            Widgets.Label(new Rect(position, size), toDisplay);
        }
    }
}
