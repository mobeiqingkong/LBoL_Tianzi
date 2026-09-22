using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    public sealed class TianziKarmaShackleDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.FindInBattle = false;
            config.HideMesuem = true;
            config.IsUpgradable = false;
            config.Type = CardType.Status;
            config.TargetType = TargetType.Self;
            config.Colors = new List<ManaColor>() { ManaColor.Colorless };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Common;
            config.Keywords = Keyword.Ethereal | Keyword.Forbidden;
            config.UpgradedKeywords = config.Keywords;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 因果枷锁：只能打出与其相邻的牌；打出一侧后，另一侧本回合无法打出。
    /// </summary>
    [EntityLogic(typeof(TianziKarmaShackleDef))]
    public sealed class TianziKarmaShackle : TianziCard
    {
        private enum LockedSide
        {
            None,
            Left,
            Right,
        }

        private LockedSide _locked = LockedSide.None;

        public override IEnumerable<BattleAction> OnTurnStartedInHand()
        {
            this._locked = LockedSide.None;
            yield break;
        }

        public override bool ShouldPreventOtherCardUsage(Card card)
        {
            if (base.Zone != CardZone.Hand || card == null || card.Zone != CardZone.Hand)
                return false;
            if (!this.TryGetIndices(card, out int selfIndex, out int cardIndex))
                return false;

            bool isLeft = cardIndex == selfIndex - 1;
            bool isRight = cardIndex == selfIndex + 1;
            if (!isLeft && !isRight)
                return true;
            if (this._locked == LockedSide.Left && isLeft)
                return true;
            if (this._locked == LockedSide.Right && isRight)
                return true;
            return false;
        }

        public override string PreventCardUsageMessage
        {
            get { return this.LocalizeProperty("PreventUsageMessage", false, true); }
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            base.HandleBattleEvent<CardUsingEventArgs>(
                battle.CardUsing,
                new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
        }

        private void OnCardUsing(CardUsingEventArgs args)
        {
            if (base.Zone != CardZone.Hand || args.Card == null)
                return;
            if (!this.TryGetIndices(args.Card, out int selfIndex, out int cardIndex))
                return;

            if (cardIndex == selfIndex - 1)
                this._locked = LockedSide.Right;
            else if (cardIndex == selfIndex + 1)
                this._locked = LockedSide.Left;
        }

        private bool TryGetIndices(Card card, out int selfIndex, out int cardIndex)
        {
            selfIndex = -1;
            cardIndex = -1;
            IReadOnlyList<Card> hand = base.Battle.HandZone;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] == this)
                    selfIndex = i;
                if (hand[i] == card)
                    cardIndex = i;
            }
            return selfIndex >= 0 && cardIndex >= 0;
        }
    }
}
