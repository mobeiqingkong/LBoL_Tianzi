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

    // ==================================================================================
    //  稀有 · 能力牌（5 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 桃符「固若金汤的仙桃」
    public sealed class TianziPeachTalismanDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 2, White = 3 };
            config.UpgradedCost = new ManaGroup() { Any = 4, White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 3;
            config.UpgradedValue1 = 5;

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziTempHpSe),
                nameof(Invincible),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "久蒼穹";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 桃符「固若金汤的仙桃」：每 4 个回合获得 1 回合天衣无缝，
    /// 并立即获得 {Value1} 点临时生命值。
    /// </summary>
    [EntityLogic(typeof(TianziPeachTalismanDef))]
    public sealed class TianziPeachTalisman : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<Invincible>(1, 1, 0, 0, 0.2f);
            BattleAction temp = TianziTempHp.GainAction(base.Battle.Player, Value1);
            if (temp != null)
                yield return temp;
            yield return BuffAction<TianziPeachTalismanSe>(Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
