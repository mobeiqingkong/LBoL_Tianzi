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
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    // ==================================================================================
    //  罕见 · 防御牌（3 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 要石覆体
    public sealed class TianziKeystoneArmorDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 10;
            config.UpgradedBlock = 12;

            config.Shield = 3;
            config.UpgradedShield = 5;

            config.Value1 = 1;
            config.UpgradedValue1 = 2;

            config.Keywords = Keyword.Shield;
            config.UpgradedKeywords = Keyword.Shield;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 要石覆体：获得 {Block} 点格挡和 {Shield} 点护盾。
    /// 若本回合已打出过其他防御牌，护盾额外 +{Value1}。
    /// </summary>
    [EntityLogic(typeof(TianziKeystoneArmorDef))]
    public sealed class TianziKeystoneArmor : TianziCard
    {
        private int _plays;

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            int block = base.HasBlock ? base.Block.Block : 0;
            int shield = (base.HasShield ? base.Shield.Shield : 0) + this._plays * base.Value1;
            this._plays++;

            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                block,
                shield,
                BlockShieldType.Normal,
                true
            );
            yield break;
        }
    }
}
