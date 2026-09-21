using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    public sealed class TianziHeavenlyShowerDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 2;
            config.Value2 = 2;
            config.UpgradedValue2 = 4;

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天界之洗礼：抽 {Value1} 张牌。恢复 {Value2} 点生命值。</summary>
    [EntityLogic(typeof(TianziHeavenlyShowerDef))]
    public sealed class TianziHeavenlyShower : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new DrawManyCardAction(base.Value1);
            yield return new HealAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Value2,
                HealType.Normal,
                0.2f
            );
            yield break;
        }
    }
}
