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
    public sealed class TianziHeavenStepGuardDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1 };
            // 注意：不能写 UpgradedCost = { White = 1 }。
            // Card.Verify() 要求升级后【每一个颜色分量的值都不能上升】，
            // 原费 {Any=1} 的 White 分量为 0，升级到 {White=1} 会让 White 变成 1 -> throw。
            // 费用在升级时保持 1 任意不变，升级收益全部放在格挡 5 -> 7。
            config.Rarity = Rarity.Common;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 5;
            config.UpgradedBlock = 7;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天界步法：获得 {Block} 点格挡。抽 1 张牌。</summary>
    [EntityLogic(typeof(TianziHeavenStepGuardDef))]
    public sealed class TianziHeavenStepGuard : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);
            yield return new DrawManyCardAction(1);
            yield break;
        }
    }
}
