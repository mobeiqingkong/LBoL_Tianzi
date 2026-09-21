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
    // ==================================================================================
    //  罕见 · 能力牌（13 张）
    //  说明：能力牌的共同形态是「打出后获得一个持续状态」，这里统一用 BuffAction 施加。
    // ==================================================================================

    // ------------------------------------------------------------------ 绯想的威光
    public sealed class TianziScarletRadianceDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 2, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.Keywords = Keyword.Initial | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Initial | Keyword.Replenish;
            config.RelativeEffects = new List<string>() { nameof(TianziScarletRadianceSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>绯想的威光：每当一张牌被放逐，对所有敌人造成 {Value1} 点攻击伤害。</summary>
    [EntityLogic(typeof(TianziScarletRadianceDef))]
    public sealed class TianziScarletRadiance : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziScarletRadianceSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
