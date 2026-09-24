using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziWeatherWatchDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;
            config.Block = 8;
            config.UpgradedBlock = 12;
            config.Value1 = 2;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziWeatherClear),
                nameof(TianziNextTurnManaSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天气观测：获得格挡。下回合释放一种天气，持续 {Value1} 回合。</summary>
    [EntityLogic(typeof(TianziWeatherWatchDef))]
    public sealed class TianziWeatherWatch : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.DefenseAction(true);
            yield return BuffAction<TianziWeatherForecastSe>(0, base.Value1, 0, 0, 0.2f);
        }
    }
}
