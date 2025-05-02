
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
    }
}