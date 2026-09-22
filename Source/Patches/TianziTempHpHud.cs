using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
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
    /// 绝壁血条：完全复用原版 HealthBar.TweenHp 的护盾/格挡外延算法，
    /// 把绝壁当作格挡之后的下一段（health → shield → block → temp）。
    /// </summary>
    public static class TianziTempHpHud
    {
        private const string BadgeName = "TianziTempHp";
        private const string FillName = "TianziTempHpFill";
        private static readonly Color Yellow = new Color(1f, 0.82f, 0.18f, 1f);
        private static readonly ConditionalWeakTable<HealthBar, Unit> BoundUnit = new ConditionalWeakTable<HealthBar, Unit>();
        private static readonly ConditionalWeakTable<Unit, HealthBar> BoundBar = new ConditionalWeakTable<Unit, HealthBar>();
        private static bool _listening;

        public static void Bind(HealthBar bar, Unit unit)
        {
            if (bar == null || unit == null)
                return;
            BoundUnit.Remove(bar);
            BoundUnit.Add(bar, unit);
            BoundBar.Remove(unit);
            BoundBar.Add(unit, bar);
            EnsureChangedListener();
        }

        private static void EnsureChangedListener()
        {
            if (_listening)
                return;
            _listening = true;
            TianziTempHp.Changed += OnTempChanged;
        }

        private static void OnTempChanged(Unit unit)
        {
            if (unit == null)
                return;
            if (!BoundBar.TryGetValue(unit, out HealthBar bar) || bar == null)
                return;
            Sync(bar, unit);
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
            UpdateBadge(bar, TianziTempHp.Get(unit));
            // 走我们接管后的 TweenHp（Prefix 会计入绝壁）
            bar.TweenHp(unit.Hp, unit.MaxHp, unit.Shield, unit.Block, instant: true);
        }

        public static void UpdateBadge(HealthBar bar, int temp)
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
        /// 与原版 TweenHp 相同公式，仅把总量从 (shield+block) 扩成 (shield+block+temp)，
        /// 并多画一段黄条：healthEnd → shieldEnd → blockEnd → tempEnd。
        /// </summary>
        public static bool TryTweenWithTemp(
            HealthBar bar, int hp, int maxHp, int shield, int block, bool instant)
        {
            int temp = TempOf(bar);
            UpdateBadge(bar, temp);
            if (temp <= 0 || maxHp <= 0)
            {
                Image idle = FindFill(bar);
                if (idle != null)
                {
                    idle.DOKill(complete: true);
                    idle.fillAmount = 0f;
                    idle.gameObject.SetActive(false);
                }
                return false; // 走原版
            }

            Traverse tr = Traverse.Create(bar);
            Image healthImage = tr.Field("healthImage").GetValue<Image>();
            Image shieldImage = tr.Field("shieldImage").GetValue<Image>();
            Image blockImage = tr.Field("blockImage").GetValue<Image>();
            Image tempImage = EnsureFill(bar, healthImage, shieldImage, blockImage);
            if (healthImage == null || shieldImage == null || blockImage == null || tempImage == null)
                return false;

            // 叠层：最底（先画/最外）→ 最上（后画/最内）= temp, block, shield, health
            // 与原版「格挡在护盾外延」同一视觉：外圈不被内圈盖住，内圈盖住外圈中心
            EnsureDrawOrder(tempImage, blockImage, shieldImage, healthImage);

            int curHp = tr.Field("_hp").GetValue<int>();

            // ---- 以下逐行对应 HealthBar.TweenHp，只把 total 扩了 temp ----
            float hpRatio = (float)hp / (float)maxHp;
            float budget = 0.3f;
            if (1f - hpRatio > 0.3f)
                budget = 1f - hpRatio;
            if (1f - hpRatio > 0.6f)
                budget = 0.6f;

            float healthEnd = hpRatio;
            float shieldEnd = 0f;
            float blockEnd = 0f;
            float tempEnd = 0f;
            int total = shield + block + temp;
            if (total != 0)
            {
                float span = budget * (float)total / ((float)total + 20f);
                float shieldPart = span * (float)shield / (float)total;
                float blockPart = span * (float)block / (float)total;
                float tempPart = span * (float)temp / (float)total;
                if (span > 1f - hpRatio)
                    healthEnd = 1f - span;
                shieldEnd = healthEnd + shieldPart;
                blockEnd = shieldEnd + blockPart;
                tempEnd = blockEnd + tempPart;
            }

            bar.DOKill(complete: true);
            tempImage.gameObject.SetActive(true);

            if (instant)
            {
                healthImage.fillAmount = healthEnd;
                shieldImage.fillAmount = shieldEnd;
                blockImage.fillAmount = blockEnd;
                tempImage.fillAmount = tempEnd;
                bar.SetHp(hp, maxHp);
                bar.SetShield(shield, block);
                return true;
            }

            bar.SetShield(shield, block);
            float lerp = 0f;
            DOTween.Sequence()
                .Insert(0f, tempImage.DOFillAmount(tempEnd, 0.2f))
                .Insert(0f, blockImage.DOFillAmount(blockEnd, 0.2f))
                .Insert(0.05f, shieldImage.DOFillAmount(shieldEnd, 0.2f))
                .Insert(0.1f, healthImage.DOFillAmount(healthEnd, 0.2f))
                .Insert(0f, DOTween.To(
                    () => lerp,
                    delegate (float v)
                    {
                        lerp = v;
                        bar.SetHp(v.Lerp(curHp, hp).RoundToInt(), maxHp);
                    },
                    1f,
                    0.5f))
                .SetUpdate(isIndependentUpdate: true)
                .SetTarget(bar);
            return true;
        }

        /// <summary>
        /// Unity UI：siblingIndex 越大越靠上。
        /// 要让外延段可见，fill 越大的必须越靠下：temp &lt; block &lt; shield &lt; health。
        /// </summary>
        private static void EnsureDrawOrder(
            Image tempImage, Image blockImage, Image shieldImage, Image healthImage)
        {
            Transform parent = blockImage.transform.parent;
            // 先收集当前顺序，再按目标重排到 parent 下连续四层
            Transform[] ordered = new Transform[]
            {
                tempImage.transform,
                blockImage.transform,
                shieldImage.transform,
                healthImage.transform,
            };
            int baseIndex = parent.childCount;
            for (int i = 0; i < ordered.Length; i++)
            {
                int idx = ordered[i].GetSiblingIndex();
                if (idx < baseIndex)
                    baseIndex = idx;
            }
            // 从下到上设置，避免互相顶开
            for (int i = 0; i < ordered.Length; i++)
                ordered[i].SetSiblingIndex(baseIndex + i);
        }

        private static Image EnsureFill(
            HealthBar bar, Image healthImage, Image shieldImage, Image blockImage)
        {
            Image existing = FindFill(bar);
            if (existing != null)
                return existing;

            GameObject clone = Object.Instantiate(blockImage.gameObject, blockImage.transform.parent);
            clone.name = FillName;
            // 先插到格挡位置（之下），随后 EnsureDrawOrder 会再排一次
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

    /// <summary>有绝壁时完全接管 TweenHp；否则放行原版。</summary>
    [HarmonyPatch(typeof(HealthBar), nameof(HealthBar.TweenHp))]
    internal static class HealthBarTweenHpPatch
    {
        private static bool Prefix(HealthBar __instance, int hp, int maxHp, int shield, int block, bool instant)
        {
            return !TianziTempHpHud.TryTweenWithTemp(__instance, hp, maxHp, shield, block, instant);
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
        private static bool Prefix(UnitStatusWidget __instance, StatusEffect effect, StatusEffectAddResult addResult)
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
