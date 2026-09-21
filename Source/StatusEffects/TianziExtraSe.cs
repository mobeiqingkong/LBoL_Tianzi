using System;
using System.Collections.Generic;
using System.Linq;
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
    // ================================================================
    //  绯想之剑充能：下一张攻击牌造成的伤害 +{Level}
    //  实现要点：加成对「下一张攻击牌的每一段伤害」都生效，
    //            该牌整张结算完（CardPlayed）后自动移除；未被消耗则回合结束移除。
    // ================================================================
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

        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Battle.Player.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing)
            );
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardPlayed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardPlayed)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
        }

        private void OnPlayerDamageDealing(DamageDealingEventArgs args)
        {
            if (base.Level <= 0 || args.DamageInfo.DamageType != DamageType.Attack)
                return;
            base.NotifyActivating();
            this._armed = true;
            args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnCardPlayed(CardUsingEventArgs args)
        {
            if (this._armed && args.Card != null && args.Card.CardType == CardType.Attack)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
            yield break;
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    // ================================================================
    //  绯色领域：每当一张牌被放逐，{PlayerName}获得 {Level} 点格挡
    // ================================================================
    public sealed class TianziScarletDomainSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziScarletDomainSeDef))]
    public sealed class TianziScarletDomainSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<CardEventArgs>(
                base.Battle.CardExiled,
                new EventSequencedReactor<CardEventArgs>(this.OnCardExiled)
            );
        }

        private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || args.Cause == ActionCause.AutoExile)
                yield break;
            base.NotifyActivating();
            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Level,
                0,
                BlockShieldType.Direct,
                false
            );
        }
    }

    // ================================================================
    //  桃华：回合结束时若临时生命值 ≥ {Level}，抽 1 张牌并在下回合获得 1 点白色法力
    // ================================================================
    public sealed class TianziPeachBlossomSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziPeachBlossomSeDef))]
    public sealed class TianziPeachBlossomSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            if (TianziTempHp.Get(base.Battle.Player) < base.Level)
                yield break;
            base.NotifyActivating();
            yield return new DrawManyCardAction(1);
            yield return new ApplyStatusEffectAction<TianziNextTurnManaSe>(
                base.Battle.Player,
                null,
                null,
                1,
                null,
                0.1f
            );
        }
    }

    // ================================================================
    //  要石镇守：每回合开始获得 {Level} 点格挡；受到的攻击伤害 -1
    // ================================================================
    public sealed class TianziKeystoneWardSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziKeystoneWardSeDef))]
    public sealed class TianziKeystoneWardSe : StatusEffect
    {
        public const int DamageReduce = 1;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
            base.HandleOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageReceiving,
                new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageReceiving)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Level,
                0,
                BlockShieldType.Direct,
                false
            );
        }

        private void OnPlayerDamageReceiving(DamageEventArgs args)
        {
            if (args.Cause == ActionCause.OnlyCalculate)
                return;
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            if (args.DamageInfo.Damage <= DamageReduce)
                return;
            base.NotifyActivating();
            args.DamageInfo = args.DamageInfo.ReduceBy(DamageReduce);
            args.AddModifier(this);
        }
    }

    // ================================================================
    //  天人合一：回合开始时，手牌为奇数 → 获得 {Level} 点火力；
    //            手牌为偶数 → 获得 {Level} * 2 点格挡
    // ================================================================
    public sealed class TianziHeavenlyTempoSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziHeavenlyTempoSeDef))]
    public sealed class TianziHeavenlyTempoSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            if (base.Battle.HandZone.Count % 2 == 1)
            {
                yield return new ApplyStatusEffectAction<Firepower>(
                    base.Battle.Player,
                    base.Level,
                    null,
                    null,
                    null,
                    0.2f
                );
            }
            else
            {
                yield return new CastBlockShieldAction(
                    base.Battle.Player,
                    base.Battle.Player,
                    base.Level * 2,
                    0,
                    BlockShieldType.Direct,
                    false
                );
            }
        }
    }

    // ================================================================
    //  仙桃长久：临时生命值上限 +{Level}；每回合开始获得 2 点临时生命值
    // ================================================================
    public sealed class TianziPeachEternitySeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziPeachEternitySeDef))]
    public sealed class TianziPeachEternitySe : StatusEffect
    {
        public const int PerTurnTempHp = 2;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, PerTurnTempHp, 0.1f);
            if (gain == null)
                yield break;
            base.NotifyActivating();
            yield return gain;
        }
    }

    // ================================================================
    //  要石奇点：回合结束时，若本回合未受到伤害，获得 {Level} 点格挡，
    //            并在下回合获得 1 点白色法力
    // ================================================================
    public sealed class TianziKeystoneSingularitySeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziKeystoneSingularitySeDef))]
    public sealed class TianziKeystoneSingularitySe : StatusEffect
    {
        private bool _hurtThisTurn;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
        }

        private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
        {
            if (args.DamageInfo.Damage > 0f)
                this._hurtThisTurn = true;
            yield break;
        }

        private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
        {
            this._hurtThisTurn = false;
            yield break;
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || this._hurtThisTurn)
                yield break;
            base.NotifyActivating();
            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Level,
                0,
                BlockShieldType.Direct,
                false
            );
            yield return new ApplyStatusEffectAction<TianziNextTurnManaSe>(
                base.Battle.Player,
                null,
                null,
                1,
                null,
                0.1f
            );
        }
    }

    // ================================================================
    //  天界玉座：每回合开始获得 {Level} 点格挡
    // ================================================================
    public sealed class TianziHeavenThroneSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziHeavenThroneSeDef))]
    public sealed class TianziHeavenThroneSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Level,
                0,
                BlockShieldType.Direct,
                false
            );
        }
    }

    // ================================================================
    //  天界漫游：{PlayerName}每打出 {Level} 张牌，{Target} 就从弃牌堆回到手中
    // ================================================================
    public sealed class TianziHeavenRoamSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziHeavenRoamSeDef))]
    public sealed class TianziHeavenRoamSe : StatusEffect
    {
        /// <summary>要被反复取回的那张牌。</summary>
        public Card Target;

        private int _roamCount;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed)
            );
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || this.Target == null)
                yield break;
            this._roamCount++;
            if (this._roamCount < base.Level)
                yield break;
            this._roamCount = 0;
            // ⚠ MoveCardAction 的构造函数里就调用 MoveCardCheck：
            //   dstZone == card.Zone（牌已经在手上）会当场 throw「Cannot move card ... to same zone」，
            //   而这个 throw 在协程里会被 ActionResolver 吞掉 -> 动作队列被截断 -> UI 簿记错乱
            //   -> 之后每次出牌都 NRE。所以先按牌区守卫，而不是只按「是否在弃牌堆」判断。
            if (this.Target.Zone != CardZone.Discard)
                yield break;
            base.NotifyActivating();
            yield return new MoveCardAction(this.Target, CardZone.Hand);
        }
    }

    // ================================================================
    //  仙桃储备：
    //   · 每回合结束时获得 2 点临时生命值；
    //   · 每当累计减少 {Level} 点临时生命值，就获得 {Count} 点格挡并抽 1 张牌；
    //     每回合最多触发 2 次。
    // ================================================================
    public sealed class TianziPeachReserveSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.HasCount = true;
            config.CountStackType = StackType.Max;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziPeachReserveSeDef))]
    public sealed class TianziPeachReserveSe : StatusEffect
    {
        public const int PerTurnTempHp = 2;
        public const int MaxTriggerPerTurn = 2;

        private int _accumulated;
        private int _triggered;

        protected override void OnAdded(Unit unit)
        {
            TianziTempHp.Lost += this.OnTempHpLost;
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
        }

        protected override void OnRemoving(Unit unit)
        {
            TianziTempHp.Lost -= this.OnTempHpLost;
        }

        private void OnTempHpLost(Unit target, int amount)
        {
            if (target != base.Owner || amount <= 0)
                return;
            this._accumulated += amount;
        }

        private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
        {
            this._accumulated = 0;
            this._triggered = 0;
            yield break;
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;

            int threshold = base.Level > 0 ? base.Level : 1;

            while (this._accumulated >= threshold && this._triggered < MaxTriggerPerTurn)
            {
                this._accumulated -= threshold;
                this._triggered++;
                base.NotifyActivating();
                yield return new CastBlockShieldAction(
                    base.Battle.Player,
                    base.Battle.Player,
                    base.Count,
                    0,
                    BlockShieldType.Direct,
                    false
                );
                yield return new DrawManyCardAction(1);
            }

            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, PerTurnTempHp, 0.1f);
            if (gain != null)
                yield return gain;
        }
    }
}
