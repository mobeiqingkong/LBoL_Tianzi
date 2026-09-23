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
            config.Owner = null;
            config.Type = CardType.Status;
            config.TargetType = TargetType.Self;
            config.Colors = new List<ManaColor>() { ManaColor.Colorless };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Common;
            config.Keywords = Keyword.Ethereal | Keyword.Forbidden;
            config.UpgradedKeywords = config.Keywords;
            config.RelativeKeyword = Keyword.Forbidden;
            config.UpgradedRelativeKeyword = Keyword.Forbidden;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 因果枷锁：仅影响左右相邻两张牌——打出一侧后，另一侧本回合获得|禁止|。
    /// </summary>
    [EntityLogic(typeof(TianziKarmaShackleDef))]
    public sealed class TianziKarmaShackle : TianziCard
    {
        private Card _marked;
        private bool _markedAddedForbidden;

        public override IEnumerable<BattleAction> OnTurnStartedInHand()
        {
            this.ClearShackleForbidden();
            yield break;
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            base.HandleBattleEvent<CardUsingEventArgs>(
                battle.CardUsing,
                new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
            base.HandleBattleEvent<UnitEventArgs>(
                battle.Player.TurnEnded,
                new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnded));
        }

        private void OnCardUsing(CardUsingEventArgs args)
        {
            if (base.Zone != CardZone.Hand || args.Card == null)
                return;
            if (this._marked != null)
                return;
            if (!this.TryGetIndices(args.Card, out int selfIndex, out int cardIndex))
                return;

            // 只在打出左/右相邻牌时，给另一侧加禁止
            Card other = null;
            if (cardIndex == selfIndex - 1)
                other = this.CardAt(selfIndex + 1);
            else if (cardIndex == selfIndex + 1)
                other = this.CardAt(selfIndex - 1);
            else
                return;

            if (other == null)
                return;

            this._marked = other;
            if (!other.IsForbidden)
            {
                other.IsForbidden = true;
                this._markedAddedForbidden = true;
            }
        }

        private void OnPlayerTurnEnded(UnitEventArgs args)
        {
            this.ClearShackleForbidden();
        }

        private void ClearShackleForbidden()
        {
            if (this._marked != null && this._markedAddedForbidden && this._marked.IsForbidden)
                this._marked.IsForbidden = false;
            this._marked = null;
            this._markedAddedForbidden = false;
        }

        private Card CardAt(int index)
        {
            IReadOnlyList<Card> hand = base.Battle.HandZone;
            if (index < 0 || index >= hand.Count)
                return null;
            return hand[index];
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
