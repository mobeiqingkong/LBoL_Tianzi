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
using TianziMod.GunName;

namespace TianziMod.StatusEffects
{
    // ================================================================
    //  仙桃增幅：获得临时生命值时额外获得 1 点；打出时获得 2(4) 点临时生命值
    // ================================================================
    public sealed class TianziPeachBoostSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziPeachBoostSeDef))]
    public sealed class TianziPeachBoostSe : StatusEffect
    {
        public const int Bonus = 1;

        protected override void OnAdded(Unit unit)
        {
            TianziTempHp.GainModifier += this.OnTempHpGain;
        }

        protected override void OnRemoving(Unit unit)
        {
            TianziTempHp.GainModifier -= this.OnTempHpGain;
        }

        private int OnTempHpGain(Unit target, int amount)
        {
            if (target != base.Owner)
                return amount;
            base.NotifyActivating();
            return amount + Bonus;
        }
    }

    // ================================================================
    //  漫漫桃园：临时生命值减少时获得 1 点临时生命值
    // ================================================================
    public sealed class TianziPeachGardenSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziPeachGardenSeDef))]
    public sealed class TianziPeachGardenSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            TianziTempHp.Lost += this.OnTempHpLost;
        }

        protected override void OnRemoving(Unit unit)
        {
            TianziTempHp.Lost -= this.OnTempHpLost;
        }

        private void OnTempHpLost(Unit target, int amount)
        {
            // 这里只做排队，真正的获得交给卡片/状态动作序列之外的一个延迟动作。
            base.NotifyActivating();
            BattleAction action = TianziTempHp.GainAction(base.Owner, 1, 0.05f);
            if (action != null)
                this.React(action);
        }
    }

    // ================================================================
    //  绯想的威光：每当一张牌被放逐时，对所有敌人造成 2 点攻击伤害
    // ================================================================
    public sealed class TianziScarletRadianceSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziScarletRadianceSeDef))]
    public sealed class TianziScarletRadianceSe : StatusEffect
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
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                yield return new DamageAction(
                    base.Owner,
                    enemy,
                    DamageInfo.Attack(base.Level, false),
                    GunNameID.GetGunFromId(7070),
                    GunType.Single
                );
            }
        }
    }

    // ================================================================
    //  绯想剑斩波：每回合首次造成攻击伤害时（含符卡）获得等量的格挡
    // ================================================================
    public sealed class TianziScarletWaveSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziScarletWaveSeDef))]
    public sealed class TianziScarletWaveSe : StatusEffect
    {
        private int _usedThisTurn;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageDealt,
                new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            this._usedThisTurn = 0;
            yield break;
        }

        private IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
        {
            int cap = base.Level > 0 ? base.Level : 1;
            if (base.Battle.BattleShouldEnd || this._usedThisTurn >= cap)
                yield break;
            if (args.DamageInfo.DamageType != DamageType.Attack)
                yield break;
            int dealt = (int)Math.Round(args.DamageInfo.Amount, MidpointRounding.AwayFromZero);
            if (dealt <= 0)
                yield break;
            this._usedThisTurn += 1;
            base.NotifyActivating();
            yield return new CastBlockShieldAction(
                base.Battle.Player, base.Battle.Player, dealt, 0, BlockShieldType.Direct, false);
        }
    }

    // ================================================================
    //  要石矩阵：接下来 {Duration} 回合，回合开始时获得 {Level} 点格挡
    // ================================================================
    public sealed class TianziKeystoneMatrixSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.HasDuration = true;
            config.DurationStackType = StackType.Max;
            config.DurationDecreaseTiming = DurationDecreaseTiming.Custom;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziKeystoneMatrixSeDef))]
    public sealed class TianziKeystoneMatrixSe : StatusEffect
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
                base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, false);
            base.Duration -= 1;
            if (base.Duration <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    // ================================================================
    //  下回合获得法力（环舞之剑 / 天界之庇护 等）
    // ================================================================
    public sealed class TianziNextTurnManaSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.HasCount = true;
            config.CountStackType = StackType.Add;
            return config;
        }
    }

    [EntityLogic(typeof(TianziNextTurnManaSeDef))]
    public sealed class TianziNextTurnManaSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            // 用 Count 表示「本回合要补的法力总量」，颜色由外部约定为白色
            if (base.Count > 0)
                yield return new GainTurnManaAction(new ManaGroup() { White = base.Count });
            base.Count = 0;
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    public sealed class TianziNextTurnPhilSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.HasCount = true;
            config.CountStackType = StackType.Add;
            return config;
        }
    }

    [EntityLogic(typeof(TianziNextTurnPhilSeDef))]
    public sealed class TianziNextTurnPhilSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            if (base.Count > 0)
                yield return new GainTurnManaAction(new ManaGroup() { Philosophy = base.Count });
            base.Count = 0;
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }
}
