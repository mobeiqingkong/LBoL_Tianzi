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
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziBlueMeditationDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Blue };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Exile | Keyword.Echo;
            config.UpgradedKeywords = Keyword.None;
            config.RelativeCards = new List<string>() { nameof(TianziSplash) };
            config.UpgradedRelativeCards = config.RelativeCards;
            config.RelativeEffects = new List<string>() { nameof(TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Exile;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziBlueMeditationDef))]
    public sealed class TianziBlueMeditation : TianziCard
    {
        protected override bool HasParityKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<TianziSplash>() });
            foreach (BattleAction action in TianziParityPlay.Resolve(this, this.OddBranch(), this.EvenBranch()))
                yield return action;
        }

        private IEnumerable<BattleAction> OddBranch()
        {
            yield return new DrawManyCardAction(1);
        }

        private IEnumerable<BattleAction> EvenBranch()
        {
            List<Card> hand = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
            {
                if (c != null && c != this)
                    hand.Add(c);
            }
            if (hand.Count == 0)
                yield break;
            SelectHandInteraction pick = new SelectHandInteraction(1, 1, hand) { Source = this };
            yield return new InteractionAction(pick, false);
            if (pick.SelectedCards.Count > 0)
                yield return new ExileCardAction(pick.SelectedCards[0]);
        }
    }
}
