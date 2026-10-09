using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 无垢之土
    public sealed class TianziWeatherMasteryDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Common;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;
            config.Block = 8;
            config.UpgradedBlock = 12;
            config.Value1 = 4;
            config.Mana = new ManaGroup() { White = 1 };
            config.UpgradedMana = new ManaGroup() { White = 1 };
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.RelativeEffects = new List<string>();
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "紅月夜";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天候掌握：随机释放一种天气（持续 {Value1} 回合），获得 {Value2} 点格挡。</summary>
    [EntityLogic(typeof(TianziWeatherMasteryDef))]
    public sealed class TianziWeatherMastery : TianziCard
    {
        public override Interaction Precondition()
        {
            List<Card> hand = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
            {
                if (c != null && c != this)
                    hand.Add(c);
            }
            if (hand.Count == 0)
                return null;
            return new SelectHandInteraction(1, 1, hand);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            SelectHandInteraction pick = precondition as SelectHandInteraction;
            if (pick != null && pick.SelectedCards.Count > 0)
                yield return new DiscardAction(pick.SelectedCards[0]);

            yield return base.DefenseAction(true);

            List<Card> discard = new List<Card>(base.Battle.DiscardZone);
            if (discard.Count > 0)
            {
                SelectCardInteraction top = new SelectCardInteraction(0, 1, discard, SelectedCardHandling.DoNothing)
                {
                    Source = this,
                };
                yield return new InteractionAction(top, false);
                if (top.SelectedCards.Count > 0 && top.SelectedCards[0].Zone == CardZone.Discard)
                    yield return new MoveCardToDrawZoneAction(top.SelectedCards[0], DrawZoneTarget.Top);
            }

            if (base.Battle.HandZone.Count > base.Value1)
                yield return new GainManaAction(new ManaGroup() { White = 1 });
        }
    }
}
