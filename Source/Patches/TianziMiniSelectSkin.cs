using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;

namespace TianziMod.Patches
{
    /// <summary>
    /// MiniSelect 选项卡沿用触发牌（Source）的立绘。
    /// </summary>
    public static class TianziMiniSelectSkin
    {
        private static readonly ConditionalWeakTable<Card, Card> SourceByChoice = new ConditionalWeakTable<Card, Card>();

        public static Card Bind(Card choice, Card source)
        {
            if (choice == null || source == null)
                return choice;
            SourceByChoice.Remove(choice);
            SourceByChoice.Add(choice, source);
            return choice;
        }

        public static Card[] BindAll(Card source, params Card[] choices)
        {
            if (choices == null)
                return choices;
            for (int i = 0; i < choices.Length; i++)
                Bind(choices[i], source);
            return choices;
        }

        public static List<Card> BindAll(Card source, List<Card> choices)
        {
            if (choices == null)
                return choices;
            for (int i = 0; i < choices.Count; i++)
                Bind(choices[i], source);
            return choices;
        }

        public static bool TryGetSource(Card choice, out Card source)
        {
            return SourceByChoice.TryGetValue(choice, out source);
        }

        public static string ImageKeyOf(Card card)
        {
            if (card == null)
                return null;
            if (!card.Config.UpgradeImageId.IsNullOrEmpty() && card.IsUpgraded)
                return card.Config.UpgradeImageId;
            if (!card.Config.ImageId.IsNullOrEmpty())
                return card.Config.ImageId;
            return card.Id;
        }
    }

    [HarmonyPatch(typeof(CardWidget), "RefreshCardImage")]
    internal static class TianziMiniSelectSkinPatch
    {
        private static void Postfix(CardWidget __instance)
        {
            Card card = __instance.Card;
            if (card == null || !TianziMiniSelectSkin.TryGetSource(card, out Card source) || source == null)
                return;

            string key = TianziMiniSelectSkin.ImageKeyOf(source);
            if (key.IsNullOrEmpty())
                return;

            string illustrator = LBoL.Presentation.GameMaster.GetPreferredCardIllustrator(source);
            if (illustrator == null)
                illustrator = "";
            Traverse.Create(__instance).Method("RefreshCardImageTexture", key + illustrator).GetValue();
        }
    }
}
