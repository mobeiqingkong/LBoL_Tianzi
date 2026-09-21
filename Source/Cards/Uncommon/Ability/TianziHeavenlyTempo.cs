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
            config.UpgradedCost = new ManaGroup() { Hybrid = 2 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 10;
            config.UpgradedValue1 = 15;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziHeavenlyTempoSe),
                nameof(Firepower),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 天人合一：每回合开始时，手牌张数为奇数则获得 {Value1} 点火力；
    /// 为偶数则获得 {Value1} ×2 点格挡。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyTempoDef))]
    public sealed class TianziHeavenlyTempo : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziHeavenlyTempoSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
