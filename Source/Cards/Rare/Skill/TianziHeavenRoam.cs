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
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 天界漫游
    public sealed class TianziHeavenRoamDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 5;
            config.UpgradedValue1 = 4;
            config.Keywords = Keyword.Initial | Keyword.Debut;
            config.UpgradedKeywords = Keyword.Initial | Keyword.Debut;

            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Exile | Keyword.Debut;
            config.UpgradedRelativeKeyword = Keyword.Exile | Keyword.Debut;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 天界漫游：选择一张手牌。本次战斗中，{PlayerName}每打出 {Value1} 张牌，
    /// 那张牌就会从弃牌堆回到手中。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenRoamDef))]
    public sealed class TianziHeavenRoam : TianziCard
    {
        private int _plays;

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            this._plays = 0;
            base.HandleBattleEvent<CardUsingEventArgs>(
                battle.CardUsed,
                new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
        }

        private void OnCardUsed(CardUsingEventArgs args)
        {
            if (this.Zone != CardZone.Discard)
                return;
            this._plays++;
            if (this._plays < (base.Value1 > 0 ? base.Value1 : 5))
                return;
            this._plays = 0;
            this.React(new MoveCardAction(this, CardZone.Hand));
        }

        public override Interaction Precondition()
        {
            List<Card> pool = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
            {
                if (c != this)
                    pool.Add(c);
            }
            if (pool.Count == 0)
                return null;
            return new SelectHandInteraction(1, 1, pool);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            SelectHandInteraction interaction = precondition as SelectHandInteraction;
            if (interaction != null && interaction.SelectedCards.Count > 0)
                yield return new ExileCardAction(interaction.SelectedCards[0]);
            yield return new DrawManyCardAction(1);
            if (this.IsDebut && this.DebutActive)
            {
                yield return new AddCardsToHandAction(
                    new Card[] { Library.CreateCard<LBoL.EntityLib.Cards.Neutral.NoColor.WManaCard>() });
            }
        }
    }
}
