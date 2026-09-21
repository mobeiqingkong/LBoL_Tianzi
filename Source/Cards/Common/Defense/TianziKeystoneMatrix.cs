using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziKeystoneMatrixDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 6;
            config.UpgradedBlock = 7;

            config.Value1 = 2; // 持续回合
            config.UpgradedValue1 = 3;

            config.RelativeEffects = new List<string>() { nameof(TianziKeystoneMatrixSe) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(TianziKeystoneMatrixSe) };

            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 要石矩阵：获得 {Block} 点格挡。
    /// 接下来 {Value1} 回合内，回合开始时获得相同数量的格挡。
    /// </summary>
    [EntityLogic(typeof(TianziKeystoneMatrixDef))]
    public sealed class TianziKeystoneMatrix : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);

            int amount = base.HasBlock ? base.Block.Block : 0;
            if (amount > 0)
            {
                yield return BuffAction<TianziKeystoneMatrixSe>(amount, base.Value1, 0, 0, 0.2f);
            }
            yield break;
        }
    }
}
