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
    /// 结算顺序：绝壁 → 格挡 → 护盾 → 生命值（在 MeasureDamage 之前的 DamageReceiving 扣减）。
    /// </summary>
    [EntityLogic(typeof(TianziTempHpSeDef))]
    public sealed class TianziTempHpSe : StatusEffect
    {
        public const int MaxLevel = 15;

        private int _pendingCost;

        protected override void OnAdded(Unit unit)
        {
            // DamageReceiving 在 MeasureDamage(格挡/护盾) 之前，绝壁最优先
            base.HandleOwnerEvent<DamageEventArgs>(
                base.Owner.DamageReceiving,
                new GameEventHandler<DamageEventArgs>(this.OnOwnerDamageReceiving)
            );
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Owner.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnOwnerDamageReceived)
            );
            int cap = TianziTempHp.MaxOf(base.Owner);
            if (base.Level > cap)
                base.Level = cap;
            TianziTempHp.RaiseChanged(base.Owner);
            TianziTempHp.EnsureBattleHooks(base.Battle);
        }

        public override bool Stack(StatusEffect other)
        {
            bool handled = base.Stack(other);
            int cap = TianziTempHp.MaxOf(base.Owner);
            if (base.Level > cap)
                base.Level = cap;
            TianziTempHp.RaiseChanged(base.Owner);
            return handled;
        }

        private void OnOwnerDamageReceiving(DamageEventArgs args)
        {
            if (base.Level <= 0)
                return;
            // 意图/预览（OnlyCalculate）不能 ReduceBy，否则敌人攻击会显示成 0×N
            if (args.Cause == ActionCause.OnlyCalculate)
                return;
            TianziTempHp.DamageAbsorbing = 0;
            int incoming = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (incoming <= 0)
                return;

            int cost = Math.Min(incoming, base.Level);
            base.NotifyActivating();
            this._pendingCost += cost;
            TianziTempHp.DamageAbsorbing = this._pendingCost;
            // Measure 之前必须用 ReduceBy（尚未 Blocked/Shielded）
            args.DamageInfo = args.DamageInfo.ReduceBy(cost);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnOwnerDamageReceived(DamageEventArgs args)
        {
            if (this._pendingCost <= 0)
                yield break;

            int used = this._pendingCost;
            this._pendingCost = 0;
            base.Level -= used;
            TianziTempHp.LostThisTurn += used;

            foreach (BattleAction action in TianziTempHp.RaiseLost(base.Owner, used))
                yield return action;

            if (base.Level <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    /// <summary>临时生命值的统一入口：读取 / 获得 / 修改获得量。</summary>
    public static class TianziTempHp
    {
        public static int Max => TianziTempHpSe.MaxLevel;

        private static BattleController _hookedBattle;
        private static bool _rotatedThisTurn;

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

        public static event Func<Unit, int, int> GainModifier;

        /// <summary>绝壁减少时的动作挂载点（须在战斗动作链内 yield，不可自行 React）。</summary>
        public static event Func<Unit, int, IEnumerable<BattleAction>> LostActions;

        public static event Action<Unit> Changed;

        internal static IEnumerable<BattleAction> RaiseLost(Unit unit, int amount)
        {
            RaiseChanged(unit);
            Func<Unit, int, IEnumerable<BattleAction>> handler = LostActions;
            if (handler == null || amount <= 0)
                yield break;
            foreach (Func<Unit, int, IEnumerable<BattleAction>> f in handler.GetInvocationList())
            {
                IEnumerable<BattleAction> seq = f(unit, amount);
                if (seq == null)
                    continue;
                foreach (BattleAction action in seq)
                {
                    if (action != null)
                        yield return action;
                }
            }
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

        public static int ExtraAmount(Unit unit, int amount)
        {
            int cur = Get(unit);
            int target = Math.Min(cur + ModifyGain(unit, amount), MaxOf(unit));
            return Math.Max(target - cur, 0);
        }

        public static BattleAction GainAction(Unit unit, int amount, float wait = 0.2f)
        {
            int delta = ExtraAmount(unit, amount);
            if (delta <= 0)
                return null;
            EnsureBattleHooks(unit == null ? null : unit.Battle);
            return new ApplyStatusEffectAction<TianziTempHpSe>(unit, delta, null, null, null, wait);
        }

        /// <summary>本次伤害结算里，绝壁已经吸收、尚未清掉的点数。预览不算。</summary>
        public static int DamageAbsorbing;

        public static int LostThisTurn;
        public static int LostLastTurn;

        public static void RotateTurnLoss()
        {
            LostLastTurn = LostThisTurn;
            LostThisTurn = 0;
        }

        public static void EnsureBattleHooks(BattleController battle)
        {
            if (battle == null || battle.Player == null)
                return;
            if (_hookedBattle == battle)
                return;
            _hookedBattle = battle;
            _rotatedThisTurn = false;
            LostThisTurn = 0;
            LostLastTurn = 0;
            battle.Player.TurnStarting.AddHandler(
                new GameEventHandler<UnitEventArgs>(OnPlayerTurnStarting),
                GameEventPriority.Highest);
            battle.Player.TurnEnded.AddHandler(
                new GameEventHandler<UnitEventArgs>(OnPlayerTurnEnded),
                GameEventPriority.Lowest);
        }

        private static void OnPlayerTurnStarting(UnitEventArgs args)
        {
            if (_rotatedThisTurn)
                return;
            _rotatedThisTurn = true;
            RotateTurnLoss();
        }

        private static void OnPlayerTurnEnded(UnitEventArgs args)
        {
            _rotatedThisTurn = false;
        }

        public static int ConsumeAll(Unit unit)
        {
            return Consume(unit, int.MaxValue);
        }

        /// <summary>静默消耗层数并刷新 UI；不触发 LostActions。</summary>
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
            RaiseChanged(unit);
            return used;
        }

        /// <summary>消耗并 yield 漫漫桃园等后续动作。</summary>
        public static IEnumerable<BattleAction> ConsumeActions(Unit unit, int amount, out int used)
        {
            used = Consume(unit, amount);
            if (used <= 0)
                return EmptyActions();
            return RaiseLost(unit, used);
        }

        private static IEnumerable<BattleAction> EmptyActions()
        {
            yield break;
        }
    }
}
