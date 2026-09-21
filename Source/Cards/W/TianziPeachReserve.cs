using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziPeachReserveDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            // 0 费白色牌
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 0 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 2; // 打出时获得的临时生命值（由状态每回合发放）
            config.Value2 = 2; // 触发时获得的格挡
            config.UpgradedValue2 = 3;

            config.Keywords = Keyword.Exile | Keyword.Retain;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachReserveSe),
                nameof(TianziTempHpSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 仙桃储备（放逐 / 保留）
    /// 打出后获得「仙桃储备」：每回合结束时获得 {TianziPeachReserveSe:PerTurnTempHp} 点临时生命值；
    /// 每当累计减少 {Value3} 点临时生命值，获得 {Value2} 点格挡并抽 1 张牌（每回合最多 2 次）。
    /// </summary>
    [EntityLogic(typeof(TianziPeachReserveDef))]
    public sealed class TianziPeachReserve : TianziCard
    {
        // CardConfig 只有 Value1 / Value2，第三个数值用 TianziCard 自带的 Value3。
        protected override int BaseValue3 { get; set; } = 3; // 触发阈值
        protected override int BaseUpgradedValue3 { get; set; } = 2;

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziPeachReserveSe>(
                base.Value3,
                0,
                0,
                base.Value2,
                0.2f
            );
            yield break;
        }
    }
}
