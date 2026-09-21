using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ==================================================================================
    //  罕见 · 技能牌�? 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 天人的飞�?
    public sealed class TianziHeavenlyFlightDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Mana = new ManaGroup() { Philosophy = 3 };
            config.UpgradedMana = new ManaGroup() { Philosophy = 4 };

            config.Value1 = 1; // 抽牌�?
            config.UpgradedValue1 = 2;

            config.Keywords = Keyword.Exile | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Replenish;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天人的飞翔：获得 {Mana} 点彩色费用。抽 {Value1} 张牌。（放�?/ 填充�?/summary>
    [EntityLogic(typeof(TianziHeavenlyFlightDef))]
    public sealed class TianziHeavenlyFlight : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new GainManaAction(base.Mana);
            yield return new DrawManyCardAction(base.Value1);
            yield break;
        }
    }
}
