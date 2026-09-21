using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    public sealed class TianziKeystonePillarDef : TianziCardTemplate
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

            config.Block = 9;
            config.UpgradedBlock = 12;

            config.Value1 = 4;
            config.UpgradedValue1 = 6;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 要石磐座：获得 {Block} 点格挡。
    /// 若本回合尚未打出过其他防御牌，额外获得 {Value1} 点格挡。
    /// </summary>
    [EntityLogic(typeof(TianziKeystonePillarDef))]
    public sealed class TianziKeystonePillar : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);

            if (this.CountTurnPlayed(CardType.Defense) == 0)
            {
                yield return new CastBlockShieldAction(
                    base.Battle.Player,
                    base.Battle.Player,
                    base.Value1,
                    0,
                    BlockShieldType.Direct,
                    false
                );
            }
            yield break;
        }
    }
}
