using HarmonyLib;
using LBoL.Core.StatusEffects;
using TianziMod.StatusEffects;

namespace TianziMod.Patches
{
    [HarmonyPatch(typeof(Graze), nameof(Graze.LoseGraze))]
    internal static class TianziClearKeepGrazePatch
    {
        private static bool Prefix(Graze __instance)
        {
            if (__instance == null || __instance.Battle == null || __instance.Battle.Player == null)
                return true;
            if (__instance.Owner != __instance.Battle.Player)
                return true;
            TianziWeatherClear weather = __instance.Battle.Player.GetStatusEffect<TianziWeatherClear>();
            if (weather == null)
                return true;
            weather.KeepGraze();
            return false;
        }
    }
}
