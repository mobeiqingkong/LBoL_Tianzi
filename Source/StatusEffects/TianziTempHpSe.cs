using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.StatusEffects
{
    public sealed class TianziTempHpSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Add;
            config.IsStackable = true;
            config.HasDuration = false;
            config.HasCount = false;
            config.Order = 10;
            return config;
        }
    }

    /// <summary>
    /// 绝壁：独立于生命值上限的战斗内生命池，上限 15。
    /// 受击时【优先于格挡与护盾】被扣减（因此挂在 DamageReceiving —— 早于 MeasureDamage）。
    /// </summary>
    [EntityLogic(typeof(TianziTempHpSeDef))]
    public sealed class TianziTempHpSe : StatusEffect
    {
        public const int MaxLevel = 15;

        // 本次结算要扣掉的池量（DamageTaking 改伤害 → DamageReceived 才真正扣 Level）
        private int _pendingCost;

        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageEventArgs>(
                base.Owner.DamageReceiving,
                new GameEventHandler<DamageEventArgs>(this.OnOwnerDamageReceiving)
            );
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Owner.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnOwnerDamageReceived)
            );
            if (unit == base.Battle.Player)
            {
                base.ReactOwnerEvent<UnitEventArgs>(
                    base.Battle.Player.TurnStarting,
                    new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting)
                );
            }
            int cap = TianziTempHp.MaxOf(base.Owner);
            if (base.Level > cap)
                base.Level = cap;
        }

        /// <summary>叠加时夹到上限（基础 15，可被「仙桃长久」提高）。</summary>
        public override bool Stack(StatusEffect other)
        {
            bool handled = base.Stack(other);
            int cap = TianziTempHp.MaxOf(base.Owner);
            if (base.Level > cap)
                base.Level = cap;
            return handled;
        }

        private void OnOwnerDamageReceiving(DamageEventArgs args)
        {
            // 预览计算（BattleController.CalculateDamage）也会走这条链路，必须排除，否则预览会白扣池子。
            if (args.Cause == ActionCause.OnlyCalculate)
                return;
            if (base.Level <= 0)
                return;

            int incoming = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (incoming <= 0)
                return;

            int cost = Math.Min(incoming, base.Level);
            base.NotifyActivating();
            this._pendingCost += cost;
            args.DamageInfo = args.DamageInfo.ReduceBy(cost);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
        {
            TianziTempHp.RotateTurnLoss();
            yield break;
        }

        private IEnumerable<BattleAction> OnOwnerDamageReceived(DamageEventArgs args)
        {
            if (this._pendingCost <= 0)
                yield break;

            int used = this._pendingCost;
            this._pendingCost = 0;
            base.Level -= used;
            TianziTempHp.LostThisTurn += used;
            TianziTempHp.RaiseLost(base.Owner, used);
            TianziTempHp.RaiseChanged(base.Owner);

            if (base.Level <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    /// <summary>临时生命值的统一入口：读取 / 获得 / 修改获得量。</summary>
    public static class TianziTempHp
    {
        public static int Max => TianziTempHpSe.MaxLevel;

        /// <summary>
        /// 该单位的临时生命值上限 = 基础 15 + 「仙桃长久」等状态提供的加成。
        /// </summary>
        public static int MaxOf(Unit unit)
        {
            if (unit == null)
                return TianziTempHpSe.MaxLevel;
            TianziPeachEternitySe bonus = unit.GetStatusEffect<TianziPeachEternitySe>();
            int extra = bonus == null ? 0 : bonus.Level;
            return TianziTempHpSe.MaxLevel + extra;
        }

        public static int Get(Unit unit)
        {
            if (unit == null)
                return 0;
            TianziTempHpSe se = unit.GetStatusEffect<TianziTempHpSe>();
            return se == null ? 0 : se.Level;
        }

        /// <summary>“获得临时生命值时额外获得 N 点”这类效果的挂载点。</summary>
        public static event Func<Unit, int, int> GainModifier;

        /// <summary>“临时生命值减少时”的挂载点，参数为 (单位, 实际减少量)。</summary>
        public static event Action<Unit, int> Lost;

        /// <summary>层数变化时刷新黄色血条。</summary>
        public static event Action<Unit> Changed;

        internal static void RaiseLost(Unit unit, int amount)
        {
            Action<Unit, int> handler = Lost;
            if (handler != null && amount > 0)
                handler(unit, amount);
            RaiseChanged(unit);
        }

        internal static void RaiseChanged(Unit unit)
        {
            Action<Unit> handler = Changed;
            if (handler != null)
                handler(unit);
        }

        public static int ModifyGain(Unit unit, int amount)
        {
            Func<Unit, int, int> handler = GainModifier;
            if (handler == null)
                return amount;
            foreach (Func<Unit, int, int> f in handler.GetInvocationList())
                amount = f(unit, amount);
            return amount;
        }

        /// <summary>返回应该【追加】多少层（已含加成），已考虑上限。</summary>
        public static int ExtraAmount(Unit unit, int amount)
        {
            int cur = Get(unit);
            int target = Math.Min(cur + ModifyGain(unit, amount), MaxOf(unit));
            return Math.Max(target - cur, 0);
        }

        /// <summary>获得临时生命值。返回 null 表示已满，无需执行。</summary>
        public static BattleAction GainAction(Unit unit, int amount, float wait = 0.2f)
        {
            int delta = ExtraAmount(unit, amount);
            if (delta <= 0)
                return null;
            return new ApplyStatusEffectAction<TianziTempHpSe>(unit, delta, null, null, null, wait);
        }

        public static int LostThisTurn;
        public static int LostLastTurn;

        public static void RotateTurnLoss()
        {
            LostLastTurn = LostThisTurn;
            LostThisTurn = 0;
        }

        public static int ConsumeAll(Unit unit)
        {
            return Consume(unit, int.MaxValue);
        }

        public static int Consume(Unit unit, int amount)
        {
            int cur = Get(unit);
            if (cur <= 0 || amount <= 0)
                return 0;
            int used = amount < cur ? amount : cur;
            TianziTempHpSe se = unit.GetStatusEffect<TianziTempHpSe>();
            if (se == null)
                return 0;
            se.Level -= used;
            LostThisTurn += used;
            RaiseLost(unit, used);
            return used;
        }
    }
}
