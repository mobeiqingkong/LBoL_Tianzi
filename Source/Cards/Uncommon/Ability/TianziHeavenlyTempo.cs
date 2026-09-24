using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 天人永不落俗
    public sealed class TianziHeavenlyTempoDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 1, Red = 1 };
            config.UpgradedCost = ManaGroup.Hybrids(2, ManaColor.White, ManaColor.Red);
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 1;
            config.UpgradedValue1 = 2;
            config.Value2 = 1;
            config.UpgradedValue2 = 2;
            config.RelativeEffects = new List<string>() { nameof(Firepower) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 奇数回合获得 {Value1} 点火力（升级 2）；偶数回合获得当前火力 2 倍格挡，至多 25。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyTempoDef))]
    public sealed class TianziHeavenlyTempo : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziHeavenlyTempoSe>(base.Value2, 0, 0, base.Value1, 0.2f);
            yield break;
        }
    }
}
