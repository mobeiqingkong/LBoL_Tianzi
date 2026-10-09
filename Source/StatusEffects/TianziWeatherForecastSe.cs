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
            // 不可堆叠：重复获得不再把持续时间相加（原 DurationStackType 默认是 Add）
            config.IsStackable = false;
            return config;
        }
    }


    [EntityLogic(typeof(TianziWeatherForecastSeDef))]
    public sealed class TianziWeatherForecastSe : StatusEffect
    {
        /// <summary>
        /// 不可堆叠 ⇒ 重复获得会走「新增实例」路径，这里必须自己把旧的摘掉，
        /// 否则场上会同时挂两个观测、下回合各放一次随机天气。
        /// </summary>
        protected override void OnAdding(Unit unit)
        {
            foreach (StatusEffect se in unit.StatusEffects)
            {
                if (se is TianziWeatherForecastSe)
                {
                    this.React(new RemoveStatusEffectAction(se, true, 0f));
                    break;
                }
            }
        }

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
