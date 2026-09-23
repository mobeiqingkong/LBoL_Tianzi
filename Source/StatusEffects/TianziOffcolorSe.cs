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
using TianziMod.Keywords;

namespace TianziMod.StatusEffects
{
    public sealed class TianziShrugSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.IsStackable = false;
            config.LevelStackType = StackType.Max;
            return config;
        }
    }

    [EntityLogic(typeof(TianziShrugSeDef))]
    public sealed class TianziShrugSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnDmg));
        }

        private IEnumerable<BattleAction> OnDmg(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || !base.Battle.Player.IsInTurn)
                yield break;
            if (args.DamageInfo.Damage <= 0f && args.DamageInfo.Amount <= 0f)
                yield break;
            base.NotifyActivating();
            yield return new CastBlockShieldAction(
                base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, false);
        }
    }

    public sealed class TianziProbeSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziProbeSeDef))]
    public sealed class TianziProbeSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnUsed));
        }

        private IEnumerable<BattleAction> OnUsed(CardUsingEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || args.Card == null)
                yield break;
            if (!TianziKeywords.HasParity(args.Card))
                yield break;
            base.NotifyActivating();
            yield return new DrawManyCardAction(1);
        }
    }

    public sealed class TianziEarthDelaySeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            // 自愈层数取更高；多次进手只叠加回合数 Count
            config.LevelStackType = StackType.Max;
            config.HasCount = true;
            config.CountStackType = StackType.Add;
            config.HasDuration = false;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziEarthDelaySeDef))]
    public sealed class TianziEarthDelaySe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            if (base.Count <= 0)
                base.Count = 1;
            // 在自愈自身 TurnStarted 扣层之后再加，避免刚加上就被 -1
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurn),
                GameEventPriority.Lowest);
        }

        private IEnumerable<BattleAction> OnTurn(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Level <= 0 || base.Count <= 0)
                yield break;
            base.NotifyActivating();
            yield return new ApplyStatusEffectAction<TianziRegenSe>(
                base.Battle.Player, base.Level, null, null, null, 0.1f);
            base.Count -= 1;
            if (base.Count <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.05f);
        }
    }

    public sealed class TianziFeastSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziFeastSeDef))]
    public sealed class TianziFeastSe : StatusEffect
    {
        public ManaGroup Mana
        {
            get { return new ManaGroup() { Philosophy = 1 }; }
        }

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<DieEventArgs>(
                base.Battle.EnemyDied,
                new EventSequencedReactor<DieEventArgs>(this.OnDie));
        }

        private IEnumerable<BattleAction> OnDie(DieEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new HealAction(base.Battle.Player, base.Battle.Player, 2, HealType.Normal, 0.1f);
            yield return new GainManaAction(new ManaGroup() { Philosophy = 1 });
            yield return new DrawManyCardAction(1);
        }
    }

    public sealed class TianziReflectSeDef : TianziStatusEffectTemplate
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

    /// <summary>境界对决：参考龟甲地狱（YachieDefendSe）——在 DamageTaking 完全抵消并原额反击。</summary>
    [EntityLogic(typeof(TianziReflectSeDef))]
    public sealed class TianziReflectSe : StatusEffect
    {
        private readonly Queue<(Unit target, int damage)> _pending =
            new Queue<(Unit target, int damage)>();

        private int _activeTimes;

        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageTaking,
                new GameEventHandler<DamageEventArgs>(this.OnTaking));
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnReceived));
        }

        private void OnTaking(DamageEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            int amount = (int)Math.Round(args.DamageInfo.Damage);
            if (amount < 1 || this._activeTimes >= base.Level)
                return;
            base.NotifyActivating();
            this._activeTimes += 1;
            args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(amount);
            args.AddModifier(this);
            Unit source = args.Source;
            if (source is EnemyUnit && source.IsAlive)
                this._pending.Enqueue((source, amount));
        }

        private IEnumerable<BattleAction> OnReceived(DamageEventArgs args)
        {
            while (this._pending.Count > 0)
            {
                (Unit target, int damage) hit = this._pending.Dequeue();
                if (hit.target != null && hit.target.IsAlive && hit.damage > 0)
                {
                    yield return new DamageAction(
                        base.Battle.Player,
                        hit.target,
                        DamageInfo.Reaction(hit.damage));
                }
            }
            base.Level -= this._activeTimes;
            this._activeTimes = 0;
            if (base.Level <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.05f);
        }
    }

    public sealed class TianziGraceSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziGraceSeDef))]
    public sealed class TianziGraceSe : StatusEffect
    {
        public ManaGroup Mana
        {
            get { return new ManaGroup() { White = 1 }; }
        }

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnStart));
            // 先挂上「本回合开始不丢格挡」，再在 TurnStarting（LoseBlockGraze 之后）把格挡转成绝壁
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting));
        }

        private IEnumerable<BattleAction> OnStart(UnitEventArgs args)
        {
            yield return new GainManaAction(new ManaGroup() { White = 1 });
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            if (base.Battle.Player.Block <= 0)
                yield break;
            // 已有 DontLoseBlock（如耐性）则复用；否则挂一层，避免 LoseBlockGraze 先清掉格挡
            if (base.Battle.Player.HasStatusEffect<TurnStartDontLoseBlock>())
                yield break;
            yield return new ApplyStatusEffectAction<TurnStartDontLoseBlock>(
                base.Battle.Player, 1, null, null, null, 0.05f);
        }

        private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
        {
            // 敌人回合结束后、本回合即将失去剩余格挡时：格挡 → 绝壁
            Unit player = base.Battle.Player;
            int block = player.Block;
            if (block <= 0)
                yield break;
            base.NotifyActivating();
            BattleAction gain = TianziTempHp.GainAction(player, block, 0.1f);
            if (gain != null)
                yield return gain;
            if (player.Block > 0)
                yield return new LoseBlockShieldAction(player, player.Block, 0, true);
        }
    }

    public sealed class TianziLethalSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziLethalSeDef))]
    public sealed class TianziLethalSe : StatusEffect
    {
        public ManaGroup Mana
        {
            get { return new ManaGroup() { Colorless = 1 }; }
        }

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<ManaEventArgs>(
                base.Battle.ManaGaining,
                new EventSequencedReactor<ManaEventArgs>(this.OnMana));
        }

        private IEnumerable<BattleAction> OnMana(ManaEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || !base.Battle.Player.IsInTurn)
                yield break;
            if (args.Value.Colorless <= 0)
                yield break;
            base.NotifyActivating();
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                int dmg = enemy.MaxHp / 100;
                if (dmg < 1)
                    dmg = 1;
                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    DamageInfo.HpLose(dmg),
                    TianziMod.GunName.GunNameID.GetGunFromId(4540),
                    GunType.Single);
            }
        }
    }
}
