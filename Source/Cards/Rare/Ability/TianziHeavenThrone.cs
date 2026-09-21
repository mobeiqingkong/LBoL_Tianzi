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
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziHeavenThroneDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Hybrid = 2 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Mana = new ManaGroup() { White = 1 };
            config.UpgradedMana = new ManaGroup() { White = 1 };
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.RelativeEffects = new List<string>() { nameof(TianziGraceSe), nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天界玉座：立即获得 {Value1} 点格挡；此后每回合开始获得 {Value1} 点格挡。</summary>
    [EntityLogic(typeof(TianziHeavenThroneDef))]
    public sealed class TianziHeavenThrone : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziGraceSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
