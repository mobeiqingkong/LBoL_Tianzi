using HarmonyLib;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using TianziMod.Keywords;
using UnityEngine;

namespace TianziMod.Patches
{
    /// <summary>
    /// 手中存在「奇偶数」牌时，没有该关键字的手牌变灰。
    /// </summary>
    [HarmonyPatch(typeof(CardWidget), "RefreshStatus")]
    internal static class TianziParityTintPatch
    {
        private static readonly Color Dim = new Color(0.55f, 0.55f, 0.62f, 1f);

        private static void Postfix(CardWidget __instance)
        {
            Card card = __instance.Card;
            if (card == null || card.Battle == null || card.Zone != CardZone.Hand)
            {
                Reset(__instance);
                return;
            }

            if (!TianziKeywords.HandHasParity(card.Battle) || TianziKeywords.HasParity(card))
            {
                Reset(__instance);
                return;
            }

            CanvasGroup cg = __instance.CanvasGroup;
            if (cg != null)
                cg.alpha = 0.72f;
            foreach (UnityEngine.UI.Graphic g in __instance.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            {
                if (g is TMPro.TextMeshProUGUI)
                    continue;
                g.color = Dim;
            }
        }

        private static void Reset(CardWidget widget)
        {
            CanvasGroup cg = widget.CanvasGroup;
            if (cg != null && cg.alpha < 0.95f && cg.alpha > 0.01f)
                cg.alpha = 1f;
        }
    }
}
