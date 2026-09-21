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

    public sealed class TianziWeatherForecastSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.HasDuration = true;
            config.DurationDecreaseTiming = DurationDecreaseTiming.Custom;
            return config;
        }
    }


    [EntityLogic(typeof(TianziWeatherForecastSeDef))]
    public sealed class TianziWeatherForecastSe : StatusEffect
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
            int dur = base.Duration > 0 ? base.Duration : 2;
            foreach (BattleAction action in TianziWeather.ApplyRandom(base.Battle.Player, dur))
                yield return action;
            yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }
}
