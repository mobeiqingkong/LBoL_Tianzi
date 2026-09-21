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
            config.HasDuration = true;
            config.DurationDecreaseTiming = DurationDecreaseTiming.TurnEnd;
            config.IsStackable = true;
            config.LevelStackType = StackType.Add;
            return config;
        }
    }

    [EntityLogic(typeof(TianziEarthDelaySeDef))]
    public sealed class TianziEarthDelaySe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurn));
        }

        private IEnumerable<BattleAction> OnTurn(UnitEventArgs args)
        {
            yield return new ApplyStatusEffectAction<TianziRegenSe>(
                base.Battle.Player, base.Level, null, null, null, 0.1f);
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
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziReflectSeDef))]
    public sealed class TianziReflectSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageEventArgs>(
                base.Owner.DamageReceiving,
                new GameEventHandler<DamageEventArgs>(this.OnRecv));
            base.ReactOwnerEvent<DamageEventArgs>(
                base.Owner.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnGot));
        }

        private float _stored;

        private void OnRecv(DamageEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            this._stored = args.DamageInfo.Damage;
            args.DamageInfo = args.DamageInfo.ReduceBy((int)this._stored + 1);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnGot(DamageEventArgs args)
        {
            Unit src = args.Source;
            float dmg = this._stored;
            this._stored = 0f;
            yield return new RemoveStatusEffectAction(this, true, 0.05f);
            if (src == null || !src.IsAlive || dmg <= 0f)
                yield break;
            base.NotifyActivating();
            yield return new DamageAction(
                base.Owner, src, DamageInfo.Attack(dmg, true), "Instant", GunType.Single);
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

        private int _blockAtStart;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnStart));
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnEnd));
        }

        private IEnumerable<BattleAction> OnStart(UnitEventArgs args)
        {
            this._blockAtStart = base.Battle.Player.Block;
            yield return new GainManaAction(new ManaGroup() { White = 1 });
        }

        private IEnumerable<BattleAction> OnEnd(UnitEventArgs args)
        {
            int lost = this._blockAtStart - base.Battle.Player.Block;
            if (lost <= 0)
                yield break;
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, lost, 0.1f);
            if (gain == null)
                yield break;
            base.NotifyActivating();
            yield return gain;
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

        public ManaGroup Mana2
        {
            get { return new ManaGroup() { Philosophy = 1 }; }
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
            if (args.Value.Colorless <= 0 && args.Value.Philosophy <= 0)
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
