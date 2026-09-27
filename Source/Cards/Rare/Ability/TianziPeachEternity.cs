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

    // ------------------------------------------------------------------ 仙桃长久
    public sealed class TianziPeachEternityDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 3;
            config.UpgradedValue1 = 5;
            // 回合开始获得的绝壁；多次打出时 SE.Count 按 Add 叠加
            config.Value2 = 2;
            config.UpgradedValue2 = 2;

            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe), nameof(TianziPeachEternitySe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "7saki";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 野生桃奶：绝壁上限 +{Value1}；每回合开始获得 {Value2} 点绝壁（叠层均可叠加）。
    /// </summary>
    [EntityLogic(typeof(TianziPeachEternityDef))]
    public sealed class TianziPeachEternity : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziPeachEternitySe>(base.Value1, 0, 0, base.Value2, 0.2f);
            yield break;
        }
    }
}
