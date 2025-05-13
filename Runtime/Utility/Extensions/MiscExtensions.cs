
using System;
using DG.Tweening;
using R3;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class MiscExtensions
    {
        public static float RoundedSeconds(this TimeSpan dateTime)
        {
            return Mathf.Round((float)dateTime.TotalSeconds);
        }

        public static bool IsTodayUTC(this DateTime timestamp)
        {
            DateTime nowUtc = DateTime.UtcNow;
            return timestamp.Date == nowUtc.Date;
        }

        public static TimeSpan GetTimeTillMidnight()
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime nextMidnightUtc = nowUtc.Date.AddDays(1);
            return nextMidnightUtc - nowUtc;
        }

        public static TimeSpan GetTimeFromMidnight()
        {
            DateTime nowUtc = DateTime.UtcNow;
            return nowUtc - nowUtc.Date;
        }
    }
}