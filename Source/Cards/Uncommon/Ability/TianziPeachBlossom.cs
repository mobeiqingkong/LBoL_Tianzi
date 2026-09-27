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

    // ------------------------------------------------------------------ 桃华
    public sealed class TianziPeachBlossomDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 2 };
            config.UpgradedCost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.RelativeEffects = new List<string>() { nameof(TianziKarmaKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "here /ヘレ";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 桃华：每回合结束时，若{PlayerName}的临时生命值不少于 {Value1}，
    /// 抽 1 张牌并在下回合获得 1 点白色法力。
    /// </summary>
    [EntityLogic(typeof(TianziPeachBlossomDef))]
    public sealed class TianziPeachBlossom : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziKarmaSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
