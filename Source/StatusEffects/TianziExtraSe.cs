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
using LBoLEntitySideloader.Attributes;

namespace TianziMod.StatusEffects
{
    public sealed class TianziScarletChargeSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Add;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziScarletChargeSeDef))]
    public sealed class TianziScarletChargeSe : StatusEffect
    {
        private bool _armed;
        /// <summary>挂上时这张充能牌还在结算，它自己的伤害和 CardUsed 不能把状态清掉。</summary>
        private bool _ignoreThisPlay = true;

        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Battle.Player.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing));
            // 手牌打出走 UseCardAction → CardUsed（不是 CardPlayed）
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
        }

        private void OnPlayerDamageDealing(DamageDealingEventArgs args)
        {
            if (this._ignoreThisPlay || base.Level <= 0 || args.DamageInfo.DamageType != DamageType.Attack)
                return;
            base.NotifyActivating();
            this._armed = true;
            args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (this._ignoreThisPlay)
            {
                this._ignoreThisPlay = false;
                yield break;
            }
            if (this._armed && args.Card != null && args.Card.CardType == CardType.Attack)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    public sealed class TianziHeavenlyTempoSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            // Level = 偶数回合格挡倍数（取高，固定 2）；Count = 奇数回合火力（叠加）
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.IsStackable = true;
            config.HasCount = true;
            config.CountStackType = StackType.Add;
            return config;
        }
    }

    [EntityLogic(typeof(TianziHeavenlyTempoSeDef))]
    public sealed class TianziHeavenlyTempoSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            if (base.Count <= 0)
                base.Count = 1;
            if (base.Level <= 0)
                base.Level = 1;
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
        }

        public override bool Stack(StatusEffect other)
        {
            bool handled = base.Stack(other);
            if (base.Count < 1)
                base.Count = 1;
            return handled;
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            int turn = base.Battle.Player.TurnCounter;
            if (turn % 2 == 1)
            {
                int gain = base.Count > 0 ? base.Count : 1;
                yield return new ApplyStatusEffectAction<Firepower>(
                    base.Battle.Player, gain, null, null, null, 0.2f);
            }
            else
            {
                Firepower fpSe = base.Battle.Player.GetStatusEffect<Firepower>();
                int fp = fpSe == null ? 0 : fpSe.Level;
                int mult = base.Level > 0 ? base.Level : 1;
                int block = fp * mult;
                if (block > 25)
                    block = 25;
                if (block <= 0)
                    yield break;
                yield return new CastBlockShieldAction(
                    base.Battle.Player, base.Battle.Player, block, 0, BlockShieldType.Direct, false);
            }
        }
    }

    public sealed class TianziPeachEternitySeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Add;
            config.HasCount = true;
            config.CountStackType = StackType.Add;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziPeachEternitySeDef))]
    public sealed class TianziPeachEternitySe : StatusEffect
    {
        /// <summary>单次打出默认回合开始绝壁；叠层时用 Count（可叠加）。</summary>
        public const int PerTurnTempHp = 2;

        protected override void OnAdded(Unit unit)
        {
            if (base.Count <= 0)
                base.Count = PerTurnTempHp;
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            int amount = base.Count > 0 ? base.Count : PerTurnTempHp;
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, amount, 0.1f);
            if (gain == null)
                yield break;
            base.NotifyActivating();
            yield return gain;
        }
    }

    public sealed class TianziDoubleAttackSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziDoubleAttackSeDef))]
    public sealed class TianziDoubleAttackSe : StatusEffect
    {
        /// <summary>施加本状态的那张牌的 CardUsed 已过，开始对「下一张」生效。</summary>
        private bool _started;

        protected override void OnAdded(Unit unit)
        {
            this._started = false;
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Battle.Player.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing));
            // 手牌打出走 UseCardAction → CardUsed
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
        }

        private void OnPlayerDamageDealing(DamageDealingEventArgs args)
        {
            // 未过施加牌的 CardUsed 前不生效（避免误伤本张）
            if (!this._started || args.DamageInfo.DamageType != DamageType.Attack)
                return;
            base.NotifyActivating();
            args.DamageInfo = args.DamageInfo.MultiplyBy(2f);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (!this._started)
            {
                // 第一次：绯色狂想自己的 CardUsed，只武装，不移除
                this._started = true;
                yield break;
            }
            // 之后打出的下一张攻击牌结束时移除
            if (args.Card != null && args.Card.CardType == CardType.Attack)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }
}
