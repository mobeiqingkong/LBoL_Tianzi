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
    public sealed class TianziCloudPillarDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 7;
            config.UpgradedBlock = 10;

            config.Value1 = 3; // 手牌门槛
            config.Value2 = 3; // 追加格挡

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 云柱：获得 {Block} 点格挡。
    /// 若手牌张数不少于 {Value1}，额外获得 {Value2} 点格挡。
    /// </summary>
    [EntityLogic(typeof(TianziCloudPillarDef))]
    public sealed class TianziCloudPillar : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);

            if (base.Battle.HandZone.Count >= base.Value1)
            {
                yield return new CastBlockShieldAction(
                    base.Battle.Player,
                    base.Battle.Player,
                    base.Value2,
                    0,
                    BlockShieldType.Direct,
                    false
                );
            }
            yield break;
        }
    }
}
