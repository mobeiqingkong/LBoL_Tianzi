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
    // ================================================================
    //  雹：每回合基础法力获取数量翻倍
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
            base.NotifyActivating();
            yield return new GainManaAction(new ManaGroup() { White = 2, Red = 2 });
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
            this.StripEnemyBlockShield();
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
            this.StripEnemyBlockShield();
            yield break;
        }

        private void StripEnemyBlockShield()
        {
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null)
                    continue;
                if (enemy.Block > 0 || enemy.Shield > 0)
                    enemy.LoseBlockShield(enemy.Block, enemy.Shield);
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
    //  无风：最后造成未被格挡伤害的一方获得 2 层自愈；
    //        主角受到未被格挡伤害时，自愈转移给对手并提高 1 层。
    // ================================================================
    public sealed class TianziWeatherCalmDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherCalmDef))]
    public sealed class TianziWeatherCalm : TianziWeatherSeBase
    {
        private readonly List<EnemyUnit> _hooked = new List<EnemyUnit>();
        private Unit _holder;

        private void HookAllEnemies()
        {
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || this._hooked.Contains(enemy))
                    continue;
                base.ReactOwnerEvent<DamageEventArgs>(
                    enemy.DamageDealt,
                    new EventSequencedReactor<DamageEventArgs>(this.OnEnemyDamageDealt)
                );
                this._hooked.Add(enemy);
            }
        }

        protected override void RegisterHooks()
        {
            this.HookAllEnemies();
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageDealt,
                new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            this.HookAllEnemies();
            yield break;
        }

        private IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || args.DamageInfo.Damage <= 0f)
                yield break;
            foreach (BattleAction action in this.ClaimRegen(base.Battle.Player))
                yield return action;
        }

        private IEnumerable<BattleAction> OnEnemyDamageDealt(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || args.DamageInfo.Damage <= 0f)
                yield break;
            Unit src = args.Source;
            if (src == null || src == base.Battle.Player)
                yield break;
            foreach (BattleAction action in this.ClaimRegen(src))
                yield return action;
        }

        /// <summary>把自愈（唯一一份）转移给 gainer，层数为「原层数 +1」或初值 2。</summary>
        private IEnumerable<BattleAction> ClaimRegen(Unit gainer)
        {
            if (gainer == null || this._holder == gainer)
                yield break;

            int carried = this._holder != null ? TianziRegen.Get(this._holder) : 0;

            foreach (Unit u in base.Battle.AllAliveUnits)
            {
                if (u == gainer)
                    continue;
                TianziRegenSe other = u.GetStatusEffect<TianziRegenSe>();
                if (other != null)
                    yield return new RemoveStatusEffectAction(other, true, 0.1f);
            }

            this._holder = gainer;
            int amount = carried > 0 ? carried + 1 : 2;
            base.NotifyActivating();
            yield return new ApplyStatusEffectAction<TianziRegenSe>(gainer, amount, null, null, null, 0.1f);
            yield return new HealAction(gainer, gainer, amount, HealType.Normal, 0.1f);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }
}
