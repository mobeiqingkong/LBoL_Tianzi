using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziHeavenDoorDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Value1 = 3;
            config.UpgradedValue1 = 5;
            config.Value2 = 6;
            config.UpgradedValue2 = 8;
            config.Keywords = Keyword.Retain | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Retain | Keyword.Replenish;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziTempHpSe),
                nameof(TianziKarmaKwSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Block | Keyword.Exile | Keyword.Ethereal | Keyword.Upgrade;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Exile | Keyword.Ethereal | Keyword.Upgrade;
            config.Illustrator = "朱シオ";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziHeavenDoorDef))]
    public sealed class TianziHeavenDoor : TianziCard
    {
        protected override bool HasKarmaKeyword { get { return true; } }

        public override Interaction Precondition()
        {
            List<Card> pool = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
            {
                if (c != null && c != this && c.CanUpgrade)
                    pool.Add(c);
            }
            if (pool.Count == 0)
                return null;
            return new SelectHandInteraction(1, 1, pool);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            Card chosen = null;
            SelectHandInteraction pick = precondition as SelectHandInteraction;
            if (pick != null && pick.SelectedCards.Count > 0)
                chosen = pick.SelectedCards[0];

            if (chosen != null && chosen.CanUpgrade)
                yield return new UpgradeCardAction(chosen);

            BattleAction temp = TianziTempHp.GainAction(base.Battle.Player, base.Value1);
            if (temp != null)
                yield return temp;

            CardType kind = chosen == null ? CardType.Unknown : chosen.CardType;
            foreach (BattleAction action in TianziKarmaPlay.Resolve(
                this,
                kind,
                this.DoorAttack(chosen),
                this.DoorDefense(),
                this.DoorSkill(),
                null,
                null))
                yield return action;
        }

        private IEnumerable<BattleAction> DoorAttack(Card origin)
        {
            if (origin == null)
                yield break;
            Card copy = origin.CloneBattleCard();
            copy.SetTurnCost(ManaGroup.Empty);
            copy.IsExile = true;
            copy.IsEthereal = true;
            yield return new AddCardsToHandAction(new Card[] { copy }, AddCardsType.Normal, false);
        }

        private IEnumerable<BattleAction> DoorDefense()
        {
            yield return new CastBlockShieldAction(
                base.Battle.Player, base.Battle.Player, base.Value2, 0, BlockShieldType.Direct, false);
        }

        private IEnumerable<BattleAction> DoorSkill()
        {
            yield return new DrawManyCardAction(2);
        }
    }
}
