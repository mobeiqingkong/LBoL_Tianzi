using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using TianziMod.Boss;
using TianziMod.Enemies;
using TianziMod.Exhibits;
using UnityEngine;

namespace TianziMod.Patches
{
    [HarmonyPatch(typeof(Stage), nameof(Stage.InitBoss))]
    internal static class TianziBossRouteInitBossPatch
    {
        static void Postfix(Stage __instance)
        {
            if (__instance.Level == 1)
                TianziBossRoute.ResetKeepsake();

            if (TianziBossRoute.KeepsakeTaken)
                return;

            RemoveBossFromPool(__instance.BossPool, TianziBossRoute.BossGroupId);
        }

        private static void RemoveBossFromPool(object pool, string groupId)
        {
            if (pool == null)
                return;
            System.Reflection.MethodInfo remove = pool.GetType().GetMethod(
                "Remove",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);
            remove?.Invoke(pool, new object[] { groupId });
        }
    }

    [HarmonyPatch(typeof(Stage), nameof(Stage.GetBoss))]
    internal static class TianziBossRouteGetBossPatch
    {
        static void Prefix(Stage __instance)
        {
            if (__instance.Level != 2)
                return;
            if (!TianziBossRoute.KeepsakeTaken)
                return;
            __instance.SetBoss(TianziBossRoute.BossGroupId);
        }
    }

    [HarmonyPatch(typeof(Stage), nameof(Stage.GetBossExhibits))]
    internal static class TianziBossExhibitPatch
    {
        static void Postfix(Stage __instance, ref Exhibit[] __result)
        {
            if (__result == null || __result.Length == 0)
                return;
            if (__instance.Boss == null || __instance.Boss.Id != TianziBossRoute.BossGroupId)
                return;

            Exhibit forced;
            if (Random.Range(0, 2) == 0)
                forced = Library.CreateExhibit<TianziExhibitA>();
            else
                forced = Library.CreateExhibit<TianziExhibitB>();
            __result[__result.Length - 1] = forced;
            TianziBossRoute.AwaitingBossExhibitPick = true;
        }
    }

    [HarmonyPatch(typeof(Exhibit), nameof(Exhibit.OnAdded))]
    internal static class TianziBossExhibitAddedPatch
    {
        static void Postfix(Exhibit __instance)
        {
            if (!TianziBossRoute.AwaitingBossExhibitPick)
                return;

            TianziBossRoute.AwaitingBossExhibitPick = false;
            if (__instance is TianziExhibitA || __instance is TianziExhibitB)
                TianziBossRoute.KeepsakeTaken = true;
        }
    }

    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.EnemyDebutAnimation), typeof(EnemyUnit))]
    internal static class TianziBossDebutChatPatch
    {
        static void Postfix(EnemyUnit enemy)
        {
            if (!(enemy is TianziChapterBoss boss))
                return;
            if (!TianziBossRoute.KeepsakeTaken)
                return;
            if (enemy.GameRun == null || enemy.GameRun.CurrentStage == null)
                return;
            if (enemy.GameRun.CurrentStage.Level != 2)
                return;

            UnitView view = enemy.GetView<UnitView>();
            if (view != null)
                view.Chat(boss.KeepsakeDebutChat, 3.2f, ChatWidget.CloudType.RightTalk, 0.35f);
        }
    }
}
