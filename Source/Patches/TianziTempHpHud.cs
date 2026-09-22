using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using HarmonyLib;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Widgets;
using TianziMod.StatusEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TianziMod.Patches
{
    /// <summary>
    /// 绝壁：格挡同款数字 + 在原版血/盾/格挡之外再往外延伸一段黄条。
    /// 绝不改写 health/shield/block 的 fillAmount。
    /// </summary>
    public static class TianziTempHpHud
    {
        private const string BadgeName = "TianziTempHp";
        private const string FillName = "TianziTempHpFill";
        private static readonly Color Yellow = new Color(1f, 0.82f, 0.18f, 1f);
        private static readonly ConditionalWeakTable<HealthBar, Unit> BoundUnit = new ConditionalWeakTable<HealthBar, Unit>();

        public static void Bind(HealthBar bar, Unit unit)
        {
            if (bar == null || unit == null)
                return;
            BoundUnit.Remove(bar);
            BoundUnit.Add(bar, unit);
        }

        public static int TempOf(HealthBar bar)
        {
            if (bar != null && BoundUnit.TryGetValue(bar, out Unit unit))
                return TianziTempHp.Get(unit);
            return 0;
        }

        public static void Sync(HealthBar bar, Unit unit)
        {
            if (bar == null || unit == null)
                return;
            Bind(bar, unit);

            int temp = TianziTempHp.Get(unit);
            UpdateBadge(bar, temp);

            // 先让原版画出生命/护盾/格挡，再只叠绝壁外圈
            bar.TweenHp(unit.Hp, unit.MaxHp, unit.Shield, unit.Block, instant: true);
        }

        public static void AfterVanillaTween(
            HealthBar bar, int hp, int maxHp, int shield, int block, bool instant)
        {
            int temp = TempOf(bar);
            UpdateBadge(bar, temp);
            ExtendTempOnly(bar, hp, maxHp, shield, block, temp, instant);
        }

        private static void UpdateBadge(HealthBar bar, int temp)
        {
            Transform badge = EnsureBadge(bar);
            if (badge == null)
                return;
            TextMeshProUGUI tmp = badge.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
                tmp.text = temp.ToString();
            badge.gameObject.SetActive(temp > 0);
        }

        /// <summary>
        /// 只设置黄条：从原版最外缘（格挡/护盾/生命）再往外延伸。
        /// 延伸长度用与格挡相同的 量/(量+20) 规则。
        /// </summary>
        private static void ExtendTempOnly(
            HealthBar bar, int hp, int maxHp, int shield, int block, int temp, bool instant)
        {
            Image tempImage = EnsureFill(bar);
            if (tempImage == null)
                return;

            if (temp <= 0 || maxHp <= 0)
            {
                tempImage.fillAmount = 0f;
                tempImage.gameObject.SetActive(false);
                return;
            }

            Traverse tr = Traverse.Create(bar);
            Image healthImage = tr.Field("healthImage").GetValue<Image>();
            Image shieldImage = tr.Field("shieldImage").GetValue<Image>();
            Image blockImage = tr.Field("blockImage").GetValue<Image>();
            if (healthImage == null || shieldImage == null || blockImage == null)
                return;

            float outer = healthImage.fillAmount;
            if (shieldImage.fillAmount > outer)
                outer = shieldImage.fillAmount;
            if (blockImage.fillAmount > outer)
                outer = blockImage.fillAmount;

            float hpRatio = (float)hp / (float)maxHp;
            float budget = 0.3f;
            if (1f - hpRatio > 0.3f)
                budget = 1f - hpRatio;
            if (1f - hpRatio > 0.6f)
                budget = 0.6f;

            int sb = shield + block;
            float spanWithout = sb <= 0 ? 0f : budget * (float)sb / ((float)sb + 20f);
            float spanWith = budget * (float)(sb + temp) / ((float)(sb + temp) + 20f);
            float tempLen = spanWith - spanWithout;
            if (tempLen < 0f)
                tempLen = 0f;

            float tempFill = outer + tempLen;
            if (tempFill > 1f)
                tempFill = 1f;

            tempImage.gameObject.SetActive(true);
            if (instant)
            {
                tempImage.fillAmount = tempFill;
                return;
            }

            tempImage.DOKill(complete: true);
            tempImage.DOFillAmount(tempFill, 0.2f).SetUpdate(isIndependentUpdate: true);
        }

        private static Transform EnsureBadge(HealthBar bar)
        {
            Transform keep = FindNamed(bar.transform, BadgeName);
            if (keep != null)
                return keep;

            Traverse tr = Traverse.Create(bar);
            Transform blockParent = tr.Field("blockParent").GetValue<Transform>();
            if (blockParent == null)
                return null;

            Transform junk = null;
            CollectAndDedup(bar.transform, BadgeName, ref junk);

            GameObject clone = Object.Instantiate(blockParent.gameObject, blockParent.parent);
            clone.name = BadgeName;
            clone.transform.SetSiblingIndex(blockParent.GetSiblingIndex() + 1);
            foreach (Graphic g in clone.GetComponentsInChildren<Graphic>(true))
                g.color = Yellow;
            clone.SetActive(false);
            return clone.transform;
        }

        private static Image EnsureFill(HealthBar bar)
        {
            Image existing = FindFill(bar);
            if (existing != null)
                return existing;

            Traverse tr = Traverse.Create(bar);
            Image blockImage = tr.Field("blockImage").GetValue<Image>();
            if (blockImage == null)
                return null;

            GameObject clone = Object.Instantiate(blockImage.gameObject, blockImage.transform.parent);
            clone.name = FillName;
            // 画在格挡填充之下，作为更外一圈
            clone.transform.SetSiblingIndex(blockImage.transform.GetSiblingIndex());
            Image img = clone.GetComponent<Image>();
            if (img != null)
            {
                img.color = Yellow;
                img.fillAmount = 0f;
            }
            clone.SetActive(false);
            return img;
        }

        private static Image FindFill(HealthBar bar)
        {
            Transform t = FindNamed(bar.transform, FillName);
            return t == null ? null : t.GetComponent<Image>();
        }

        private static Transform FindNamed(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindNamed(root.GetChild(i), name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void CollectAndDedup(Transform root, string name, ref Transform keep)
        {
            if (root == null)
                return;
            if (root.name == name)
            {
                if (keep == null)
                    keep = root;
                else
                    Object.Destroy(root.gameObject);
                return;
            }
            for (int i = root.childCount - 1; i >= 0; i--)
                CollectAndDedup(root.GetChild(i), name, ref keep);
        }
    }

    [HarmonyPatch(typeof(HealthBar), nameof(HealthBar.TweenHp))]
    internal static class HealthBarTweenHpPatch
    {
        private static void Postfix(HealthBar __instance, int hp, int maxHp, int shield, int block, bool instant)
        {
            TianziTempHpHud.AfterVanillaTween(__instance, hp, maxHp, shield, block, instant);
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "TweenHpBar")]
    internal static class UnitStatusTweenHpBarPatch
    {
        private static void Prefix(UnitStatusWidget __instance)
        {
            HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Bind(bar, __instance.Unit);
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "SetHpBar")]
    internal static class UnitStatusSetHpBarPatch
    {
        private static void Prefix(UnitStatusWidget __instance)
        {
            HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Bind(bar, __instance.Unit);
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "OnAddStatusEffect")]
    internal static class UnitStatusAddSePatch
    {
        private static bool Prefix(UnitStatusWidget __instance, StatusEffect effect)
        {
            if (effect is TianziTempHpSe)
            {
                HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
                TianziTempHpHud.Sync(bar, __instance.Unit);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "SetStatusEffects")]
    internal static class UnitStatusSetSePatch
    {
        private static bool Prefix(UnitStatusWidget __instance)
        {
            Traverse tr = Traverse.Create(__instance);
            tr.Method("ClearStatusEffects").GetValue();
            Unit unit = tr.Field("_unit").GetValue<Unit>();
            if (unit != null)
            {
                List<StatusEffectWidget> widgets =
                    tr.Field("_statusEffectWidgets").GetValue<List<StatusEffectWidget>>();
                foreach (StatusEffect statusEffect in unit.StatusEffects)
                {
                    if (statusEffect is TianziTempHpSe)
                        continue;
                    widgets.Add(tr.Method("CreateStatusEffectWidget", statusEffect, false).GetValue<StatusEffectWidget>());
                }
            }
            HealthBar bar = tr.Field("hpBar").GetValue<HealthBar>();
            TianziTempHpHud.Sync(bar, unit);
            return false;
        }
    }

    [HarmonyPatch(typeof(UnitStatusWidget), "OnRemoveStatusEffect")]
    internal static class UnitStatusRemoveSePatch
    {
        private static void Postfix(UnitStatusWidget __instance, StatusEffect effect)
        {
            if (effect is TianziTempHpSe)
            {
                HealthBar bar = Traverse.Create(__instance).Field("hpBar").GetValue<HealthBar>();
                TianziTempHpHud.Sync(bar, __instance.Unit);
            }
        }
    }
}
