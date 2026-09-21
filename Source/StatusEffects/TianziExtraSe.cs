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

        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Battle.Player.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing));
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardPlayed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardPlayed));
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
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
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
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
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            int turn = base.Battle.Player.TurnCounter;
            Firepower fpSe = base.Battle.Player.GetStatusEffect<Firepower>();
            int fp = fpSe == null ? 0 : fpSe.Level;
            if (turn % 2 == 1)
            {
                yield return new ApplyStatusEffectAction<Firepower>(
                    base.Battle.Player, 1, null, null, null, 0.2f);
            }
            else
            {
                int block = (int)Math.Round(fp * base.Level / 10.0, MidpointRounding.AwayFromZero);
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
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
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
        private bool _armed;

        protected override void OnAdded(Unit unit)
        {
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Battle.Player.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing));
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardPlayed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardPlayed));
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
        }

        private void OnPlayerDamageDealing(DamageDealingEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            base.NotifyActivating();
            this._armed = true;
            args.DamageInfo = args.DamageInfo.MultiplyBy(2f);
            args.AddModifier(this);
        }

        private IEnumerable<BattleAction> OnCardPlayed(CardUsingEventArgs args)
        {
            if (this._armed && args.Card != null && args.Card.CardType == CardType.Attack)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }
}
