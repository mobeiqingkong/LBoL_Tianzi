using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziPeachNectarDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 3;
            config.UpgradedValue1 = 5;
            config.Value2 = 2; // 临时生命值
            config.UpgradedValue2 = 3;

            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(TianziTempHpSe) };

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile;

            config.Illustrator = "绫缪";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>桃之甘露：恢复 {Value1} 点生命值，并获得 {Value2} 点临时生命值。</summary>
    [EntityLogic(typeof(TianziPeachNectarDef))]
    public sealed class TianziPeachNectar : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new HealAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Value1,
                HealType.Normal,
                0.2f
            );

            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, base.Value2);
            if (gain != null)
                yield return gain;
            yield break;
        }
    }
}
