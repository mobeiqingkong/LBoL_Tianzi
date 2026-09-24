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
    /// <summary>
    /// 天气的公共基类。
    /// 规则：同一时刻只有一种天气（与游戏「心境」互斥方式相同）；
    /// 天气持续 {Duration} 个「主角回合」；新天气生效时旧天气立刻失效。
    /// </summary>
    public abstract class TianziWeatherSeBase : StatusEffect
    {
        /// <summary>Localized display name from DirResources yaml (`Name`).</summary>
        public string WeatherName { get { return this.Name; } }

        /// <summary>
        /// 参考 <see cref="Mood"/>：在 OnAdding 时若已有其它天气，立刻移除，保证互斥。
        /// </summary>
        protected override void OnAdding(Unit unit)
        {
            StatusEffect existing = null;
            foreach (StatusEffect se in unit.StatusEffects)
            {
                if (se is TianziWeatherSeBase)
                {
                    existing = se;
                    break;
                }
            }
            if (existing != null)
                this.React(new RemoveStatusEffectAction(existing, true, 0f));
        }

        protected override void OnAdded(Unit unit)
        {
            this.RegisterHooks();

            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnWeatherTurnStarting)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnWeatherTurnStarted)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnWeatherTurnEnding)
            );
        }

        protected override void OnRemoving(Unit unit)
        {
            this.UnregisterHooks();
        }

        /// <summary>子类在这里挂自己的事件监听。</summary>
        protected virtual void RegisterHooks() { }

        /// <summary>子类在这里摘掉自己的事件监听。</summary>
        protected virtual void UnregisterHooks() { }

        /// <summary>主角回合开始（天气倒计时）。</summary>
        protected virtual IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            yield break;
        }

        /// <summary>主角回合开始后（天气实际效果）。</summary>
        protected virtual IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            yield break;
        }

        /// <summary>主角回合即将结束。</summary>
        protected virtual IEnumerable<BattleAction> OnWeatherTurnEnding(UnitEventArgs args)
        {
            yield break;
        }

        /// <summary>倒计时（在 TurnStarting 里调用，避免施加当回合立刻 -1）。</summary>
        protected IEnumerable<BattleAction> TickDuration()
        {
            if (!base.HasDuration)
                yield break;
            base.Duration -= 1;
            if (base.Duration <= 0)
                yield return new RemoveStatusEffectAction(this, true, 0.1f);
        }
    }

    /// <summary>天气的统一入口。</summary>
    public static class TianziWeather
    {
        public const int WeatherKindCount = 8;

        /// <summary>卡面关联状态：全部天气种类。预报是「下回合才释放」时再带上。</summary>
        public static List<string> RelativeIds(bool includeForecast)
        {
            List<string> ids = new List<string>();
            if (includeForecast)
                ids.Add(nameof(TianziWeatherForecastSe));
            ids.Add(nameof(TianziWeatherClear));
            ids.Add(nameof(TianziWeatherMist));
            ids.Add(nameof(TianziWeatherCloud));
            ids.Add(nameof(TianziWeatherAzure));
            ids.Add(nameof(TianziWeatherHail));
            ids.Add(nameof(TianziWeatherFog));
            ids.Add(nameof(TianziWeatherTyphoon));
            ids.Add(nameof(TianziWeatherCalm));
            return ids;
        }

        public enum Kind
        {
            Clear = 0,
            Mist = 1,
            Cloud = 2,
            Azure = 3,
            Hail = 4,
            Fog = 5,
            Typhoon = 6,
            Calm = 7,
        }

        /// <summary>移除当前所有天气，并随机施加一种新天气。</summary>
        public static IEnumerable<BattleAction> ApplyRandom(Unit owner, int duration)
        {
            // 先让旧天气失效
            List<StatusEffect> olds = new List<StatusEffect>();
            foreach (StatusEffect se in owner.StatusEffects)
            {
                if (se is TianziWeatherSeBase)
                    olds.Add(se);
            }
            foreach (StatusEffect se in olds)
                yield return new RemoveStatusEffectAction(se, true, 0f);

            int idx = UnityEngine.Random.Range(0, WeatherKindCount);
            yield return MakeAction((Kind)idx, owner, duration);
        }

        /// <summary>施加指定天气（会先清掉旧天气）。</summary>
        public static IEnumerable<BattleAction> Apply(Kind kind, Unit owner, int duration)
        {
            List<StatusEffect> olds = new List<StatusEffect>();
            foreach (StatusEffect se in owner.StatusEffects)
            {
                if (se is TianziWeatherSeBase)
                    olds.Add(se);
            }
            foreach (StatusEffect se in olds)
                yield return new RemoveStatusEffectAction(se, true, 0f);

            yield return MakeAction(kind, owner, duration);
        }

        private static BattleAction MakeAction(Kind kind, Unit owner, int duration)
        {
            switch (kind)
            {
                case Kind.Clear:
                    return new ApplyStatusEffectAction<TianziWeatherClear>(owner, 0, duration, null, null, 0.2f);
                case Kind.Mist:
                    return new ApplyStatusEffectAction<TianziWeatherMist>(owner, 0, duration, null, null, 0.2f);
                case Kind.Cloud:
                    return new ApplyStatusEffectAction<TianziWeatherCloud>(owner, 0, duration, null, null, 0.2f);
                case Kind.Azure:
                    return new ApplyStatusEffectAction<TianziWeatherAzure>(owner, 0, duration, null, null, 0.2f);
                case Kind.Hail:
                    return new ApplyStatusEffectAction<TianziWeatherHail>(owner, 0, duration, null, null, 0.2f);
                case Kind.Fog:
                    return new ApplyStatusEffectAction<TianziWeatherFog>(owner, 0, duration, null, null, 0.2f);
                case Kind.Typhoon:
                    return new ApplyStatusEffectAction<TianziWeatherTyphoon>(owner, 0, duration, null, null, 0.2f);
                default:
                    return new ApplyStatusEffectAction<TianziWeatherCalm>(owner, 0, duration, null, null, 0.2f);
            }
        }

        /// <summary>天气状态的统一配置。</summary>
        public static StatusEffectConfig BaseConfig()
        {
            StatusEffectConfig config = TianziStatusEffectTemplate.GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.HasDuration = true;
            config.DurationStackType = StackType.Max;
            config.DurationDecreaseTiming = DurationDecreaseTiming.Custom;
            config.HasCount = false;
            config.IsStackable = true;
            return config;
        }
    }
}
