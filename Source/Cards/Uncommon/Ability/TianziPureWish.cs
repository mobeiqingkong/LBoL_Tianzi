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

    // ------------------------------------------------------------------ 清霖之愿
    public sealed class TianziPureWishDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 2, White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.UpgradedValue1 = 3;
            config.RelativeEffects = new List<string>() { nameof(TianziPureWishSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>清霖之愿：每回合前 {Value1} 次抽到诅咒或状态牌时，将其放逐并抽 1 张牌。</summary>
    [EntityLogic(typeof(TianziPureWishDef))]
    public sealed class TianziPureWish : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPureWishSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
