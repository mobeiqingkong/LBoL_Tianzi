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

    public sealed class TianziEarthStoneDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Green };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Value1 = 1;
            config.UpgradedValue1 = 2;
            config.Keywords = Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Replenish;
            config.RelativeEffects = new List<string>() { nameof(TianziRegenSe), nameof(TianziEarthDelaySe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Exile;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziEarthStoneDef))]
    public sealed class TianziEarthStone : TianziCard
    {
        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            base.ReactBattleEvent<CardsEventArgs>(
                battle.CardsAddedToHand,
                new EventSequencedReactor<CardsEventArgs>(this.OnAddedToHand));
        }

        private IEnumerable<BattleAction> OnAddedToHand(CardsEventArgs args)
        {
            if (args.Cards == null || base.Zone != CardZone.Hand)
                yield break;
            bool mine = false;
            foreach (Card c in args.Cards)
            {
                if (c == this)
                {
                    mine = true;
                    break;
                }
            }
            if (!mine)
                yield break;
            yield return new ExileCardAction(this);
            // Level=自愈层数；Count=后续回合数
            yield return BuffAction<TianziEarthDelaySe>(2, 0, 0, base.Value1, 0.2f);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziEarthDelaySe>(2, 0, 0, base.Value1, 0.2f);
        }
    }
}
