using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 乾坤一掷
    public sealed class TianziWorldCleaverDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.IsXCost = true;
            config.Rarity = Rarity.Rare;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Echo;
            config.Mana = new ManaGroup() { Philosophy = 2 };
            config.UpgradedMana = new ManaGroup() { Philosophy = 2 };
            config.RelativeKeyword = Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Exile | Keyword.Echo;
            config.RelativeEffects = new List<string>() { nameof(Firepower), nameof(Spirit) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziWorldCleaverDef))]
    public sealed class TianziWorldCleaver : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            int x = base.SynergyAmount(consumingMana, ManaColor.Any, 1);
            List<Card> hand = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
            {
                if (c != null && c != this)
                    hand.Add(c);
            }
            int n = x < hand.Count ? x : hand.Count;
            List<Card> dumped = new List<Card>();
            if (n > 0)
            {
                SelectHandInteraction pick = new SelectHandInteraction(n, n, hand) { Source = this };
                yield return new InteractionAction(pick, false);
                foreach (Card c in pick.SelectedCards)
                    dumped.Add(c);
                n = dumped.Count;
            }

            foreach (Card c in dumped)
                yield return new DiscardAction(c);

            if (n > 0)
                yield return new DrawManyCardAction(n);
            // 返还超出可弃数量的费用：不能用 Any（会写入战斗法力池并触发 CanAfford 报错）
            if (x > n)
                yield return new GainManaAction(new ManaGroup() { Philosophy = x - n });

            int attacks = 0;
            int defenses = 0;
            int skills = 0;
            int junkKinds = 0;
            foreach (Card c in dumped)
            {
                if (c.CardType == CardType.Attack)
                    attacks++;
                else if (c.CardType == CardType.Defense)
                    defenses++;
                else if (c.CardType == CardType.Skill)
                    skills++;
                else if (c.CardType == CardType.Ability || c.CardType == CardType.Status || c.CardType == CardType.Misfortune)
                    junkKinds++;
            }

            if (attacks > 0)
                yield return BuffAction<Firepower>(attacks, 0, 0, 0, 0.2f);
            if (defenses > 0)
                yield return BuffAction<Spirit>(defenses, 0, 0, 0, 0.2f);
            if (skills > 0)
                yield return new GainManaAction(new ManaGroup() { Philosophy = 2 * skills });
            if (junkKinds > 0)
            {
                List<Card> junk = new List<Card>();
                foreach (Card c in base.Battle.HandZone)
                {
                    if (c.CardType == CardType.Status || c.CardType == CardType.Misfortune)
                        junk.Add(c);
                }
                foreach (Card c in base.Battle.DiscardZone)
                {
                    if (c.CardType == CardType.Status || c.CardType == CardType.Misfortune)
                        junk.Add(c);
                }
                if (junk.Count > 0)
                    yield return new ExileManyCardAction(junk);
            }
        }
    }
}
