using System;

namespace Shared
{
    public static class TimeConverter
    {
        public static double CurrentTimeToEpoch()
        {
            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return Math.Round(span.TotalMilliseconds);
        }

        public static bool CheckForEpochTimer(double toCompare, double extraValue)
        {
            if (CurrentTimeToEpoch() > toCompare + extraValue) return true;
            else return false;
        }
    }
}