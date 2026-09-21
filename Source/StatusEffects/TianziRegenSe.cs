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
    public sealed class TianziRegenSeDef : TianziStatusEffectTemplate
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
            return config;
        }
    }

    /// <summary>
    /// 自愈：每回合开始恢复 {Level} 点生命值，回合结束时层数 -1。
    /// </summary>
    [EntityLogic(typeof(TianziRegenSeDef))]
    public sealed class TianziRegenSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Owner.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Owner.TurnEnded,
                new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded)
            );
        }

        private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Level <= 0)
                yield break;
            base.NotifyActivating();
            yield return new HealAction(base.Owner, base.Owner, base.Level, HealType.Normal, 0.2f);
        }

        private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.Level -= 1;
            if (base.Level <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    /// <summary>自愈的统一入口。</summary>
    public static class TianziRegen
    {
        public static int Get(Unit unit)
        {
            if (unit == null)
                return 0;
            TianziRegenSe se = unit.GetStatusEffect<TianziRegenSe>();
            return se == null ? 0 : se.Level;
        }

        public static BattleAction GainAction(Unit unit, int amount, float wait = 0.2f)
        {
            if (amount <= 0)
                return null;
            return new ApplyStatusEffectAction<TianziRegenSe>(unit, amount, null, null, null, wait);
        }
    }
}
