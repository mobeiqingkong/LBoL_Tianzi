using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.Presentation.UI.Panels;
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
        /// <summary>
        /// 开局就会给所有幕抽样 Boss。二幕默认去掉原版天子；
        /// 模组天子再保险：抽完若仍是原版天子则重抽。
        /// </summary>
        static void Prefix(Stage __instance)
        {
            if (__instance.Level == 1)
                TianziBossRoute.ResetKeepsake();

            if (__instance.Level != 2)
                return;
            TianziBossRoute.RemoveBossFromPool(
                __instance.BossPool, TianziBossRoute.VanillaAct2BossGroupId);
        }

        static void Postfix(Stage __instance)
        {
            if (__instance.Level == 1)
                TianziBossRoute.EnsureNoModBossForModPlayer(__instance);
            if (__instance.Level != 2)
                return;
            TianziBossRoute.EnsureNoVanillaTianziForModPlayer(__instance.GameRun);
        }
    }

    [HarmonyPatch(typeof(Stage), nameof(Stage.GetBossExhibits))]
    internal static class TianziBossExhibitPatch
    {
        static void Postfix(Stage __instance, ref Exhibit[] __result)
        {
            if (__result == null || __result.Length == 0)
                return;
            // 仅第一幕自订天子 Boss 掉落塞入专属展品
            if (__instance.Boss == null || __instance.Boss.Id != TianziBossRoute.ChapterBossGroupId)
                return;

            Exhibit forced = null;
            int found = -1;
            for (int i = 0; i < __result.Length; i++)
            {
                if (__result[i] is TianziExhibitA || __result[i] is TianziExhibitB)
                {
                    if (found < 0)
                    {
                        found = i;
                        forced = __result[i];
                    }
                }
            }
            if (forced == null)
            {
                if (Random.Range(0, 2) == 0)
                    forced = Library.CreateExhibit<TianziExhibitA>();
                else
                    forced = Library.CreateExhibit<TianziExhibitB>();
                // 暂放中间（原版 Boss 槽）；最终左右由 Panel 按坐标重排
                __result[__result.Length >= 3 ? 1 : __result.Length - 1] = forced;
            }

            TianziBossRoute.AwaitingBossExhibitPick = true;
        }
    }

    /// <summary>
    /// contentList 下标与屏幕左右不一定一致；按世界坐标 X 最大的槽位放天子专属。
    /// </summary>
    [HarmonyPatch(typeof(BossExhibitPanel), "OnShowing", typeof(Exhibit[]))]
    internal static class TianziBossExhibitPanelPatch
    {
        private static readonly FieldInfo ContentListField =
            AccessTools.Field(typeof(BossExhibitPanel), "contentList");

        static void Prefix(BossExhibitPanel __instance, Exhibit[] exhibits)
        {
            if (exhibits == null || exhibits.Length == 0)
                return;

            int tianziIdx = -1;
            for (int i = 0; i < exhibits.Length; i++)
            {
                if (exhibits[i] is TianziExhibitA || exhibits[i] is TianziExhibitB)
                {
                    tianziIdx = i;
                    break;
                }
            }
            if (tianziIdx < 0)
                return;

            List<Transform> contentList = ContentListField?.GetValue(__instance) as List<Transform>;
            if (contentList == null || contentList.Count == 0)
                return;

            int rightmost = 0;
            float maxX = float.NegativeInfinity;
            int limit = exhibits.Length < contentList.Count ? exhibits.Length : contentList.Count;
            for (int i = 0; i < limit; i++)
            {
                if (contentList[i] == null)
                    continue;
                float x = contentList[i].position.x;
                if (x > maxX)
                {
                    maxX = x;
                    rightmost = i;
                }
            }

            if (tianziIdx == rightmost)
                return;

            Exhibit swap = exhibits[rightmost];
            exhibits[rightmost] = exhibits[tianziIdx];
            exhibits[tianziIdx] = swap;
        }
    }

    [HarmonyPatch(typeof(Exhibit), nameof(Exhibit.OnAdded))]
    internal static class TianziBossExhibitAddedPatch
    {
        static void Postfix(Exhibit __instance)
        {
            // 必须是第一幕自订天子 Boss 掉落这次选择
            if (!TianziBossRoute.AwaitingBossExhibitPick)
                return;

            TianziBossRoute.AwaitingBossExhibitPick = false;
            if (!(__instance is TianziExhibitA) && !(__instance is TianziExhibitB))
                return;

            TianziBossRoute.KeepsakeTaken = true;
            TianziBossRoute.ApplyAct2BossOverride(__instance.GameRun);
        }
    }

    /// <summary>进入二幕建图前覆盖/规避 Boss（CreateMap 会读 Boss.Id）。</summary>
    [HarmonyPatch(typeof(GameRunController), "EnterStage", typeof(int))]
    internal static class TianziBossRouteEnterStagePatch
    {
        static void Prefix(GameRunController __instance, int index)
        {
            if (__instance.Stages == null || index < 0 || index >= __instance.Stages.Count)
                return;
            if (__instance.Stages[index].Level != 2)
                return;
            TianziBossRoute.ApplyAct2BossOverride(__instance);
        }
    }

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.DebutAnimation))]
    internal static class TianziBossDebutChatPatch
    {
        static void Postfix(UnitView __instance)
        {
            Unit unit = __instance.Unit;
            if (!(unit is EnemyUnit enemy))
                return;
            if (enemy.GameRun == null || enemy.GameRun.CurrentStage == null)
                return;
            if (enemy.GameRun.CurrentStage.Level != 2)
                return;
            // 只有第一幕自订天子掉落选了专属展品才说话
            if (!TianziBossRoute.KeepsakeTaken)
                return;

            // 二幕原版天子 Id 为 "Tianzi"
            bool isVanillaTianzi = enemy.Id == TianziBossRoute.VanillaAct2BossGroupId || enemy is Tianzi;
            bool isChapterTianzi = enemy is TianziChapterBoss;
            if (!isVanillaTianzi && !isChapterTianzi)
                return;

            string chat = isChapterTianzi
                ? ((TianziChapterBoss)enemy).KeepsakeDebutChat
                : "把我的道具还给我！";
            if (string.IsNullOrEmpty(chat))
                chat = "把我的道具还给我！";

            // 等登场动画再出字，避免被 debut 清掉
            __instance.Chat(chat, 3.5f, ChatWidget.CloudType.RightTalk, 1.2f);
        }
    }
}
