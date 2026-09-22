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

    // ------------------------------------------------------------------ 天人之气
    public sealed class TianziOddEvenQiDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 1, Red = 1 };
            // Card.Verify()：升级费用不得比原费用贵。
            // 原写 {Any=1, White=1, Red=1} 是 3 点 > 原费 2 点 -> 启动时 throw，卡死主菜单。
            // 升级语义改为「费用颜色解绑」：双色 1+1 -> 任意 2，总额不变、各色分量不上升。
            config.UpgradedCost = new ManaGroup() { Hybrid = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.RelativeEffects = new List<string>() { nameof(TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天人之气：手牌数量的奇偶效果变为选择触发。</summary>
    [EntityLogic(typeof(TianziOddEvenQiDef))]
    public sealed class TianziOddEvenQi : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziOddEvenSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
