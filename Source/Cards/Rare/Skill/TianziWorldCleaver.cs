using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 天地开辟之剑
    public sealed class TianziWorldCleaverDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { Hybrid = 1 };
            config.IsXCost = true;
            config.Rarity = Rarity.Rare;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Echo;
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
            for (int i = 0; i < n; i++)
                dumped.Add(hand[i]);

            foreach (Card c in dumped)
                yield return new DiscardAction(c);

            if (n > 0)
                yield return new DrawManyCardAction(n);
            if (x > n)
                yield return new GainManaAction(ManaGroup.Anys(x - n));

            HashSet<CardType> kinds = new HashSet<CardType>();
            foreach (Card c in dumped)
                kinds.Add(c.CardType);

            if (kinds.Contains(CardType.Attack))
                yield return BuffAction<Firepower>(1, 0, 0, 0, 0.2f);
            if (kinds.Contains(CardType.Defense))
                yield return BuffAction<Spirit>(1, 0, 0, 0, 0.2f);
            if (kinds.Contains(CardType.Skill))
                yield return new GainManaAction(new ManaGroup() { Philosophy = 2 });
            if (kinds.Contains(CardType.Ability) || kinds.Contains(CardType.Status) || kinds.Contains(CardType.Misfortune))
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
