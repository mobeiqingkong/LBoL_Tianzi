using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader.CustomKeywords;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards.Template
{
    /// <summary>
    /// 天子所有卡牌的公共基类。
    /// 提供 Value3 / Value4 / Value5 / overFlowMana 等扩展数值，避免为每张牌单独声明；
    /// 同时集中几条本卡池高频用到的查询（上一张打出的牌、本回合出牌计数）。
    /// </summary>
    public class TianziCard : Card
    {
        // ---- 扩展数值 3 ----
        protected virtual int BaseValue3 { get; set; } = 0;
        protected virtual int BaseUpgradedValue3 { get; set; } = 0;
        public int Value3
        {
            get
            {
                if (this.IsUpgraded)
                {
                    return BaseUpgradedValue3;
                }
                return BaseValue3;
            }
        }

        // ---- 扩展数值 4 ----
        protected virtual int BaseValue4 { get; set; } = 0;
        protected virtual int BaseUpgradedValue4 { get; set; } = 0;
        public int Value4
        {
            get
            {
                if (this.IsUpgraded)
                {
                    return BaseUpgradedValue4;
                }
                return BaseValue4;
            }
        }

        // ---- 扩展数值 5（少数牌需要第 5 个数字）----
        protected virtual int BaseValue5 { get; set; } = 0;
        protected virtual int BaseUpgradedValue5 { get; set; } = 0;
        public int Value5
        {
            get
            {
                if (this.IsUpgraded)
                {
                    return BaseUpgradedValue5;
                }
                return BaseValue5;
            }
        }

        // ---- 追加获得法力（用于描述里展示）----
        protected virtual ManaGroup baseOverFlowMana { get; set; } = new ManaGroup() { };
        protected virtual ManaGroup baseUpgradedOverFlowMana { get; set; } = new ManaGroup() { };
        public ManaGroup overFlowMana
        {
            get
            {
                if (this.IsUpgraded)
                {
                    return baseUpgradedOverFlowMana;
                }
                return baseOverFlowMana;
            }
        }

        // ---- 临时附带的状态（部分牌在描述里引用）----
        protected virtual StatusEffect tempCustomStatusEffect { get; set; } = null;
        protected virtual StatusEffect tempUpgradedCustomStatusEffect { get; set; } = null;
        public StatusEffect customStatusEffect
        {
            get
            {
                if (this.IsUpgraded)
                {
                    return tempUpgradedCustomStatusEffect;
                }
                return tempCustomStatusEffect;
            }
        }

        // ------------------------------------------------------------------
        //  常用查询
        // ------------------------------------------------------------------

        /// <summary>
        /// 本回合「上一张打出的牌」（不含自己）。
        /// 手牌打出记在 TurnCardUsageHistory；强制打出记在 TurnCardPlayHistory。
        /// </summary>
        public Card PreviousPlayedCard
        {
            get
            {
                if (base.Battle == null)
                    return null;
                Card fromUsage = this.FindPreviousIn(base.Battle.TurnCardUsageHistory);
                if (fromUsage != null)
                    return fromUsage;
                return this.FindPreviousIn(base.Battle.TurnCardPlayHistory);
            }
        }

        /// <summary>本回合已打出的牌张数（不含自己）。</summary>
        public int TurnPlayedCountExceptSelf
        {
            get { return this.CountTurnPlayed(null); }
        }

        /// <summary>本回合已打出的、指定类型的牌张数（不含自己）。type 为 null 时统计全部。</summary>
        public int CountTurnPlayed(CardType? type)
        {
            if (base.Battle == null)
                return 0;
            int n = 0;
            n += this.CountInHistory(base.Battle.TurnCardUsageHistory, type);
            n += this.CountInHistory(base.Battle.TurnCardPlayHistory, type);
            return n;
        }

        /// <summary>本回合已打出的、指定类型的牌张数（不含自己）。</summary>
        public int CountTurnPlayed(CardType type)
        {
            return this.CountTurnPlayed((CardType?)type);
        }

        private Card FindPreviousIn(IReadOnlyList<Card> history)
        {
            if (history == null)
                return null;
            for (int i = history.Count - 1; i >= 0; i--)
            {
                Card c = history[i];
                if (c != null && c != this)
                    return c;
            }
            return null;
        }

        private int CountInHistory(IReadOnlyList<Card> history, CardType? type)
        {
            if (history == null)
                return 0;
            int n = 0;
            foreach (Card c in history)
            {
                if (c == null || c == this)
                    continue;
                if (type.HasValue && c.CardType != type.Value)
                    continue;
                n++;
            }
            return n;
        }

        /// <summary>手牌张数。</summary>
        public int HandCount
        {
            get { return base.Battle == null ? 0 : base.Battle.HandZone.Count; }
        }

        /// <summary>手牌张数为奇数（含自己结算完后的手牌）。</summary>
        public bool HandIsOdd
        {
            get { return this.HandCount % 2 == 1; }
        }

        protected virtual bool HasParityKeyword
        {
            get { return false; }
        }

        protected virtual bool HasKarmaKeyword
        {
            get { return false; }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (this.HasParityKeyword && !this.HasCustomKeyword(TianziKeywords.ParityId))
                this.AddCustomKeyword(TianziKeywords.Parity());
            if (this.HasKarmaKeyword && !this.HasCustomKeyword(TianziKeywords.KarmaId))
                this.AddCustomKeyword(TianziKeywords.Karma());
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            if (!this.HasParityKeyword)
                return;
            this.HandleBattleEvent<CardsEventArgs>(
                battle.CardsAddedToHand,
                new GameEventHandler<CardsEventArgs>(this.OnParityHandChanged));
            this.HandleBattleEvent<CardMovingEventArgs>(
                battle.CardMoved,
                new GameEventHandler<CardMovingEventArgs>(this.OnParityCardMoved));
            this.HandleBattleEvent<StatusEffectApplyEventArgs>(
                battle.Player.StatusEffectAdded,
                new GameEventHandler<StatusEffectApplyEventArgs>(this.OnParitySeChanged));
            this.HandleBattleEvent<StatusEffectEventArgs>(
                battle.Player.StatusEffectRemoved,
                new GameEventHandler<StatusEffectEventArgs>(this.OnParitySeRemoved));
        }

        private void OnParityHandChanged(CardsEventArgs args)
        {
            this.NotifyChanged();
        }

        private void OnParityCardMoved(CardMovingEventArgs args)
        {
            this.NotifyChanged();
        }

        private void OnParitySeChanged(StatusEffectApplyEventArgs args)
        {
            if (args.Effect is TianziOddEvenSe)
                this.NotifyChanged();
        }

        private void OnParitySeRemoved(StatusEffectEventArgs args)
        {
            if (args.Effect is TianziOddEvenSe)
                this.NotifyChanged();
        }

        protected override string GetBaseDescription()
        {
            if (!this.HasParityKeyword || this.Battle == null || TianziParity.Forced)
                return base.GetBaseDescription();
            if (TianziParity.IsOdd(this.Battle))
            {
                if (this.HasExtraDescription1)
                    return this.GetExtraDescription1;
            }
            else if (this.HasExtraDescription2)
            {
                return this.GetExtraDescription2;
            }
            return base.GetBaseDescription();
        }
    }
}
