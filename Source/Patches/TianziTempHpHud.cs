using HarmonyLib;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using TianziMod.StatusEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TianziMod.Patches
{
    /// <summary>
    /// 把临时生命值画成格挡同款数字/条，颜色黄色，排在格挡前面。
    /// </summary>
    public static class TianziTempHpHud
    {
        private static readonly Color Yellow = new Color(1f, 0.82f, 0.18f, 1f);

        public static void Sync(HealthBar bar, Unit unit)
        {
            if (bar == null)
                return;
            Transform clone = EnsureClone(bar);
            if (clone == null)
                return;

            int amount = TianziTempHp.Get(unit);
            TextMeshProUGUI tmp = clone.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
                tmp.text = amount.ToString();
            clone.gameObject.SetActive(amount > 0);
        }

        private static Transform EnsureClone(HealthBar bar)
        {
            Transform existing = bar.transform.Find("TianziTempHp");
            if (existing != null)
                return existing;

            Traverse tr = Traverse.Create(bar);
            Transform blockParent = tr.Field("blockParent").GetValue<Transform>();
            if (blockParent == null)
                return null;

            GameObject clone = Object.Instantiate(blockParent.gameObject, blockParent.parent);
            clone.name = "TianziTempHp";
            clone.transform.SetSiblingIndex(blockParent.GetSiblingIndex());
            Tint(clone.transform);
            return clone.transform;
        }

        private static void Tint(Transform root)
        {
            foreach (Graphic g in root.GetComponentsInChildren<Graphic>(true))
                g.color = Yellow;
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "TweenHpBar")]
    internal static class UnitStatusTweenHpBarPatch
    {
        private static void Postfix(UnitStatusWidget __instance)
        {
            HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Sync(bar, __instance.Unit);
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "SetHpBar")]
    internal static class UnitStatusSetHpBarPatch
    {
        private static void Postfix(UnitStatusWidget __instance)
        {
            HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Sync(bar, __instance.Unit);
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "OnAddStatusEffect")]
    internal static class UnitStatusAddSePatch
    {
        private static void Postfix(UnitStatusWidget __instance)
        {
            HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Sync(bar, __instance.Unit);
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "OnRemoveStatusEffect")]
    internal static class UnitStatusRemoveSePatch
    {
        private static void Postfix(UnitStatusWidget __instance)
        {
            HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Sync(bar, __instance.Unit);
        }
    }
}
