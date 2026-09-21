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

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachEternitySe),
                nameof(TianziTempHpSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 仙桃长久：{PlayerName}的临时生命值上限提高 {Value1}；
    /// 每回合开始获得 {TianziPeachEternitySe:PerTurnTempHp} 点临时生命值。
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
            yield return BuffAction<TianziPeachEternitySe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
