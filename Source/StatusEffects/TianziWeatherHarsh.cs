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
    //  无风：
    //  · 刚出现的那个「主角回合周期」内：最后一次造成未被格挡生命伤害的单位
    //    （含敌人）在下一次主角 TurnStarting 时获得 2 层自愈并立刻结算；
    //  · 之后每次未被格挡伤害按转移逻辑（换手 +1）。
    // ================================================================
    public sealed class TianziWeatherCalmDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    /// <summary>战斗内「未被格挡生命伤害」最后一击记录（供无风首回合判定）。</summary>
    public static class TianziUnblockedHit
    {
        private static BattleController _battle;
        private static Unit _dealer;
        private static int _turn;

        public static void Ensure(BattleController battle)
        {
            if (battle == null || battle.Player == null)
                return;
            if (_battle == battle)
                return;
            _battle = battle;
            _dealer = null;
            _turn = -1;
            battle.Player.DamageReceived.AddHandler(
                new GameEventHandler<DamageEventArgs>(OnDamageReceived),
                GameEventPriority.Lowest);
            battle.EnemySpawned.AddHandler(
                new GameEventHandler<UnitEventArgs>(OnEnemySpawned),
                GameEventPriority.Lowest);
            foreach (EnemyUnit enemy in battle.AllAliveEnemies)
                HookEnemy(enemy);
            battle.Player.TurnStarting.AddHandler(
                new GameEventHandler<UnitEventArgs>(OnTurnStarting),
                GameEventPriority.Lowest);
        }

        private static void OnEnemySpawned(UnitEventArgs args)
        {
            HookEnemy(args.Unit as EnemyUnit);
        }

        private static void HookEnemy(EnemyUnit enemy)
        {
            if (enemy == null)
                return;
            enemy.DamageReceived.AddHandler(
                new GameEventHandler<DamageEventArgs>(OnDamageReceived),
                GameEventPriority.Lowest);
        }

        private static void OnTurnStarting(UnitEventArgs args)
        {
            // 跨回合后旧记录失效（首回合判定用「施加当回合周期」内的命中）
        }

        private static void OnDamageReceived(DamageEventArgs args)
        {
            if (_battle == null || args == null)
                return;
            int hpHit = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (hpHit <= 0)
                return;
            Unit dealer = args.Source;
            if (dealer == null || !dealer.IsAlive)
                return;
            _dealer = dealer;
            _turn = _battle.Player.TurnCounter;
        }

        /// <summary>记录一次命中（无风自身监听时同步写入）。</summary>
        public static void Note(BattleController battle, Unit dealer)
        {
            if (battle == null || dealer == null || !dealer.IsAlive)
                return;
            Ensure(battle);
            _dealer = dealer;
            _turn = battle.Player.TurnCounter;
        }

        public static Unit Peek(BattleController battle)
        {
            if (battle == null || _dealer == null || !_dealer.IsAlive)
                return null;
            return _dealer;
        }

        public static void Clear()
        {
            _dealer = null;
        }
    }

    [EntityLogic(typeof(TianziWeatherCalmDef))]
    public sealed class TianziWeatherCalm : TianziWeatherSeBase
    {
        private readonly List<Unit> _hooked = new List<Unit>();
        private Unit _holder;
        /// <summary>刚施加后、尚未做「首回合最后一击→2 层」结算。</summary>
        private bool _awaitInitialGrant = true;
        private Unit _lastDealer;

        private void HookUnit(Unit unit)
        {
            if (unit == null || this._hooked.Contains(unit))
                return;
            base.ReactOwnerEvent<DamageEventArgs>(
                unit.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnAnyDamageReceived)
            );
            this._hooked.Add(unit);
        }

        private void HookAll()
        {
            this.HookUnit(base.Battle.Player);
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
                this.HookUnit(enemy);
        }

        protected override void RegisterHooks()
        {
            TianziUnblockedHit.Ensure(base.Battle);
            this.HookAll();
            // 施加前同回合已造成的未被格挡伤害也算进「最后一次」
            this._lastDealer = TianziUnblockedHit.Peek(base.Battle);
            this._awaitInitialGrant = true;
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            this.HookAll();
            yield break;
        }

        private IEnumerable<BattleAction> OnAnyDamageReceived(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            int hpHit = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (hpHit <= 0)
                yield break;
            Unit dealer = args.Source;
            if (dealer == null || !dealer.IsAlive)
                yield break;

            TianziUnblockedHit.Note(base.Battle, dealer);
            this._lastDealer = dealer;

            // 首回合周期内只记「最后一击」，等 TurnStarting 统一发 2 层
            if (this._awaitInitialGrant)
                yield break;

            foreach (BattleAction action in this.ClaimRegen(dealer))
                yield return action;
        }

        private IEnumerable<BattleAction> GrantInitial()
        {
            Unit gainer = this._lastDealer;
            if (gainer == null || !gainer.IsAlive)
                yield break;

            foreach (Unit u in base.Battle.AllAliveUnits)
            {
                TianziRegenSe other = u.GetStatusEffect<TianziRegenSe>();
                if (other != null)
                    yield return new RemoveStatusEffectAction(other, true, 0.1f);
            }

            this._holder = gainer;
            base.NotifyActivating();
            yield return new ApplyStatusEffectAction<TianziRegenSe>(gainer, 2, null, null, null, 0.1f);
            yield return new HealAction(gainer, gainer, 2, HealType.Normal, 0.1f);
        }

        private IEnumerable<BattleAction> ClaimRegen(Unit gainer)
        {
            if (gainer == null)
                yield break;

            int carried = 0;
            if (this._holder != null && this._holder.IsAlive)
                carried = TianziRegen.Get(this._holder);

            foreach (Unit u in base.Battle.AllAliveUnits)
            {
                TianziRegenSe other = u.GetStatusEffect<TianziRegenSe>();
                if (other != null)
                    yield return new RemoveStatusEffectAction(other, true, 0.1f);
            }

            int amount;
            if (this._holder == gainer)
                amount = carried > 0 ? carried : 2;
            else
                amount = carried > 0 ? carried + 1 : 2;

            this._holder = gainer;
            base.NotifyActivating();
            yield return new ApplyStatusEffectAction<TianziRegenSe>(gainer, amount, null, null, null, 0.1f);
            yield return new HealAction(gainer, gainer, amount, HealType.Normal, 0.1f);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            // 首个主角回合周期结束时：把 2 层自愈给「最后一击」单位（可含敌人）
            if (this._awaitInitialGrant)
            {
                this._awaitInitialGrant = false;
                foreach (BattleAction action in this.GrantInitial())
                    yield return action;
            }

            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }
}
