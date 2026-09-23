using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using UnityEngine;

namespace TianziMod.StatusEffects
{
    // ================================================================
    //  快晴：回合开始时随机一张卡牌任意费用 -1；自身的闪避不会消失
    // ================================================================
    public sealed class TianziWeatherClearDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherClearDef))]
    public sealed class TianziWeatherClear : TianziWeatherSeBase
    {
        public ManaGroup Mana
        {
            get { return ManaGroup.Anys(1); }
        }

        protected override void RegisterHooks() { }

        internal void KeepGraze()
        {
            base.NotifyActivating();
        }

        private void CheatOneCardCost()
        {
            IReadOnlyList<Card> hand = base.Battle.HandZone;
            if (hand == null || hand.Count == 0)
                return;
            Card card = hand[Random.Range(0, hand.Count)];
            if (card != null && card.CostToMana(false).Total > 0)
            {
                base.NotifyActivating();
                card.DecreaseTurnCost(this.Mana);
            }
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            this.CheatOneCardCost();
            yield break;
        }
    }

    // ================================================================
    //  雾雨：自身符卡的每次伤害提升 1.25 倍
    // ================================================================
    public sealed class TianziWeatherMistDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherMistDef))]
    public sealed class TianziWeatherMist : TianziWeatherSeBase
    {
        protected override void RegisterHooks()
        {
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Owner.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnOwnerDamageDealing)
            );
        }

        private void OnOwnerDamageDealing(DamageDealingEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            // 符卡伤害：ActionSource 为 UltimateSkill（Cause 一般为 Us）
            if (!(args.ActionSource is UltimateSkill) && args.Cause != ActionCause.Us)
                return;
            args.DamageInfo = args.DamageInfo.MultiplyBy(1.25f);
            args.AddModifier(this);
            if (args.Cause != ActionCause.OnlyCalculate)
                base.NotifyActivating();
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  云天：回合开始时所有卡牌任意费用 -1（Mana Any×1）
    // ================================================================
    public sealed class TianziWeatherCloudDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherCloudDef))]
    public sealed class TianziWeatherCloud : TianziWeatherSeBase
    {
        public ManaGroup Mana
        {
            get { return ManaGroup.Anys(1); }
        }

        protected override void RegisterHooks()
        {
            base.ReactOwnerEvent<CardsEventArgs>(
                base.Battle.CardsAddedToHand,
                new EventSequencedReactor<CardsEventArgs>(this.OnCardsAddedToHand)
            );
        }

        private IEnumerable<BattleAction> OnCardsAddedToHand(CardsEventArgs args)
        {
            this.CheatCost(args.Cards);
            yield break;
        }

        private void CheatCost(IEnumerable<Card> cards)
        {
            foreach (Card card in cards)
            {
                if (card != null && card.CostToMana(false).Total > 0)
                    card.DecreaseTurnCost(this.Mana);
            }
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            this.CheatCost(base.Battle.HandZone);
            yield break;
        }
    }

    // ================================================================
    //  苍天：获得 25 p点
    // ================================================================
    public sealed class TianziWeatherAzureDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherAzureDef))]
    public sealed class TianziWeatherAzure : TianziWeatherSeBase
    {
        public const int PowerGain = 7;

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new GainPowerAction(PowerGain);
        }
    }
}
