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
    //  雹：下回合获得与当前基础法力等量的法力（含光耀展品提供的基础法力）
    // ================================================================
    public sealed class TianziWeatherHailDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherHailDef))]
    public sealed class TianziWeatherHail : TianziWeatherSeBase
    {
        protected override void RegisterHooks()
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding)
            );
        }

        private bool _grantNext;

        private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
        {
            this._grantNext = true;
            yield break;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || !this._grantNext)
                yield break;
            this._grantNext = false;
            // GameRun.BaseMana 已包含角色初始法力与光耀展品 OnGain 的基础法力
            ManaGroup baseMana = base.Battle.BaseTurnMana;
            if (baseMana.Amount <= 0)
                yield break;
            base.NotifyActivating();
            yield return new GainManaAction(baseMana);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  浓雾：造成的未被阻挡的伤害会恢复自身 5% 生命值
    // ================================================================
    public sealed class TianziWeatherFogDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherFogDef))]
    public sealed class TianziWeatherFog : TianziWeatherSeBase
    {
        public const float HealRatio = 0.05f;


        protected override void RegisterHooks()
        {
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageDealt,
                new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt)
            );
        }

        private IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            int dealt = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (dealt <= 0)
                yield break;
            int heal = Math.Max(1, (int)Math.Ceiling(dealt * HealRatio));
            base.NotifyActivating();
            yield return new HealAction(base.Battle.Player, base.Battle.Player, heal, HealType.Normal, 0.1f);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  台风：对手无法获得格挡或护盾
    //  说明：敌方单位是动态生成的，所以每个主角回合开始都补挂一次；
    //        所有监听都由 StatusEffect 自带的 handler holder 在移除时统一清理。
    // ================================================================
    public sealed class TianziWeatherTyphoonDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherTyphoonDef))]
    public sealed class TianziWeatherTyphoon : TianziWeatherSeBase
    {
        private readonly List<EnemyUnit> _hooked = new List<EnemyUnit>();

        private void OnEnemyBlockShieldGaining(BlockShieldEventArgs args)
        {
            if (args.Block <= 0f && args.Shield <= 0f)
                return;
            args.Block = 0f;
            args.Shield = 0f;
            args.AddModifier(this);
        }

        private void HookAllEnemies()
        {
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || this._hooked.Contains(enemy))
                    continue;
                base.HandleOwnerEvent<BlockShieldEventArgs>(
                    enemy.BlockShieldGaining,
                    new GameEventHandler<BlockShieldEventArgs>(this.OnEnemyBlockShieldGaining)
                );
                this._hooked.Add(enemy);
            }
        }

        protected override void RegisterHooks()
        {
            this.HookAllEnemies();
            foreach (BattleAction action in this.ClearEnemyBlockShield())
                this.React(action);
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.EnemySpawned,
                new EventSequencedReactor<UnitEventArgs>(this.OnEnemySpawned)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnEnemySpawned(UnitEventArgs args)
        {
            this.HookAllEnemies();
            foreach (BattleAction action in this.ClearEnemyBlockShield())
                yield return action;
        }

        private IEnumerable<BattleAction> ClearEnemyBlockShield()
        {
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;
                if (enemy.Block <= 0 && enemy.Shield <= 0)
                    continue;
                yield return new LoseBlockShieldAction(enemy, enemy.Block, enemy.Shield, false);
            }
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            this.HookAllEnemies();
            yield break;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  烈日：每个敌人回合结束时，扣除其 1% 最大生命值
    // ================================================================
    public sealed class TianziWeatherSunDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherSunDef))]
    public sealed class TianziWeatherSun : TianziWeatherSeBase
    {
        private readonly List<EnemyUnit> _hooked = new List<EnemyUnit>();

        private void HookEnemy(EnemyUnit enemy)
        {
            if (enemy == null || this._hooked.Contains(enemy))
                return;
            base.ReactOwnerEvent<UnitEventArgs>(
                enemy.TurnEnded,
                new EventSequencedReactor<UnitEventArgs>(this.OnEnemyTurnEnded)
            );
            this._hooked.Add(enemy);
        }

        protected override void RegisterHooks()
        {
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
                this.HookEnemy(enemy);
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.EnemySpawned,
                new EventSequencedReactor<UnitEventArgs>(this.OnEnemySpawned)
            );
        }

        private IEnumerable<BattleAction> OnEnemySpawned(UnitEventArgs args)
        {
            this.HookEnemy(args.Unit as EnemyUnit);
            yield break;
        }

        private IEnumerable<BattleAction> OnEnemyTurnEnded(UnitEventArgs args)
        {
            EnemyUnit enemy = args.Unit as EnemyUnit;
            if (base.Battle.BattleShouldEnd || enemy == null || !enemy.IsAlive || enemy.MaxHp <= 0)
                yield break;
            int dmg = enemy.MaxHp / 100;
            if (dmg <= 0)
                yield break;
            base.NotifyActivating();
            yield return new DamageAction(
                base.Battle.Player,
                enemy,
                DamageInfo.HpLose(dmg),
                GunNameID.GetGunFromId(4540),
                GunType.Single);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }
}
