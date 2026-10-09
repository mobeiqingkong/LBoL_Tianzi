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
using UnityEngine;

namespace TianziMod.StatusEffects
{
    // ================================================================
    //  快晴：获得该效果时给 1 层闪避 + 1 点主角基础法力颜色的费用；自身闪避不会消失
    //  「闪避不会消失」由 TianziClearKeepGrazePatch 拦 Graze.LoseGraze 实现
    // ================================================================
    public sealed class TianziWeatherClearDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = TianziWeather.BaseConfig();
            // 不可堆叠：重复获得时走「新增实例」路径，由基类 OnAdding 移除旧天气，
            // 净效果 = 用新的持续时间替换、并重新触发一次「获得时」效果（1 层闪避 + 1 点基础色法力）。
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziWeatherClearDef))]
    public sealed class TianziWeatherClear : TianziWeatherSeBase
    {
        /// <summary>获得该天气时：1 层闪避 + 主角基础法力颜色的 1 点费用。</summary>
        protected override void OnAdded(Unit unit)
        {
            base.OnAdded(unit);
            if (base.Battle.BattleShouldEnd)
                return;
            base.NotifyActivating();
            // startAutoDecreasing:false ⇒ 施加后第一次回合开始不扣层（见 Graze.LoseGraze）
            this.React(new ApplyStatusEffectAction<Graze>(base.Owner, 1, null, null, null, 0.2f, false));
            this.React(new GainManaAction(this.RandomBasicMana()));
        }

        internal void KeepGraze()
        {
            base.NotifyActivating();
        }

        /// <summary>主角的基础法力颜色（白 / 红）随机取一种，给 1 点。</summary>
        private ManaGroup RandomBasicMana()
        {
            PlayerUnit player = base.Battle.Player;
            if (player == null || player.Config == null)
                return ManaGroup.Whites(1);
            ManaColor pick = Random.Range(0, 2) == 0 ? player.Config.LeftColor : player.Config.RightColor;
            return ManaGroup.FromColor(pick, 1);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  雾雨：自身符卡的每次伤害提升 1.25 倍
    // ================================================================
    public sealed class TianziWeatherMistDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherMistDef))]
    public sealed class TianziWeatherMist : TianziWeatherSeBase
    {
        protected override void RegisterHooks()
        {
            base.HandleOwnerEvent<DamageDealingEventArgs>(
                base.Owner.DamageDealing,
                new GameEventHandler<DamageDealingEventArgs>(this.OnOwnerDamageDealing)
            );
        }

        private void OnOwnerDamageDealing(DamageDealingEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            // 符卡伤害：ActionSource 为 UltimateSkill（Cause 一般为 Us）
            if (!(args.ActionSource is UltimateSkill) && args.Cause != ActionCause.Us)
                return;
            args.DamageInfo = args.DamageInfo.MultiplyBy(1.25f);
            args.AddModifier(this);
            if (args.Cause != ActionCause.OnlyCalculate)
                base.NotifyActivating();
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  云天：该效果存在时，每打出一张牌获得 1 点无色法力
    // ================================================================
    public sealed class TianziWeatherCloudDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherCloudDef))]
    public sealed class TianziWeatherCloud : TianziWeatherSeBase
    {
        private static ManaGroup ColorlessMana
        {
            get { return ManaGroup.Colorlesses(1); }
        }

        protected override void RegisterHooks()
        {
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed)
            );
        }

        /// <summary>每打出一张牌，获得 1 点无色法力。</summary>
        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || args.Card == null)
                yield break;
            base.NotifyActivating();
            yield return new GainManaAction(ColorlessMana);
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }
    }

    // ================================================================
    //  苍天：获得 25 p点
    // ================================================================
    public sealed class TianziWeatherAzureDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig() { return TianziWeather.BaseConfig(); }
    }

    [EntityLogic(typeof(TianziWeatherAzureDef))]
    public sealed class TianziWeatherAzure : TianziWeatherSeBase
    {
        public const int PowerGain = 7;

        protected override IEnumerable<BattleAction> OnWeatherTurnStarting(UnitEventArgs args)
        {
            foreach (BattleAction action in this.TickDuration())
                yield return action;
        }

        protected override IEnumerable<BattleAction> OnWeatherTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new GainPowerAction(PowerGain);
        }
    }
}
