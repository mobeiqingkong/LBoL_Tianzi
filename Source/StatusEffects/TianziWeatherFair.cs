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
    //  快晴：随机一张卡牌任意费用 -1；自身的闪避不会消失
    // ================================================================
    public sealed class TianziWeatherClearDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherClearDef))]
    public sealed class TianziWeatherClear : TianziWeatherSeBase
    {
        private int _savedGraze;

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            // 回合结算前先记下闪避，回合开始后再补回来（快晴：闪避不会消失）
            Graze graze = base.Owner.GetStatusEffect<Graze>();
            this._savedGraze = graze?.Level ?? 0;
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;

            base.NotifyActivating();

            // 闪避补给
            if (this._savedGraze > 0)
            {
                Graze cur = base.Owner.GetStatusEffect<Graze>();
                int have = cur == null ? 0 : cur.Level;
                if (have < this._savedGraze)
                    yield return new ApplyStatusEffectAction<Graze>(
                        base.Owner, this._savedGraze - have, null, null, null, 0.1f);
            }

            // 随机一张手牌费用 -1（仅本回合）
            IReadOnlyList<Card> hand = base.Battle.HandZone;
            if (hand.Count > 0)
            {
                Card card = hand[Random.Range(0, hand.Count)];
                if (card != null && card.CostToMana(false).Total > 0)
                    card.DecreaseTurnCost(ManaGroup.Anys(1));
            }
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
                base.Battle.Player.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing)
            );
        }

        private void OnPlayerDamageDealing(DamageDealingEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            if (args.Cause != ActionCause.Us && args.Cause != ActionCause.UsUse)
                return;
            base.NotifyActivating();
            args.DamageInfo = args.DamageInfo.MultiplyBy(1.25f);
            args.AddModifier(this);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  云天：所有卡牌任意费用 -1
    // ================================================================
    public sealed class TianziWeatherCloudDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherCloudDef))]
    public sealed class TianziWeatherCloud : TianziWeatherSeBase
    {
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
                    card.DecreaseTurnCost(ManaGroup.Anys(1));
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

        protected override void RegisterHooks()
        {
            this.React(new GainPowerAction(PowerGain));
        }
    }
}
