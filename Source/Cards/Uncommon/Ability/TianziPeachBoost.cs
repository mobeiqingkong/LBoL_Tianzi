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

    // ------------------------------------------------------------------ 仙桃增幅
    public sealed class TianziPeachBoostDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.UpgradedValue1 = 4;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "伊吹のつ";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>仙桃增幅：获得临时生命值时额外获得 1 点；立即获得 {Value1} 点临时生命值。</summary>
    [EntityLogic(typeof(TianziPeachBoostDef))]
    public sealed class TianziPeachBoost : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPeachBoostSe>(1, 0, 0, 0, 0.2f);
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, base.Value1);
            if (gain != null)
                yield return gain;
            yield break;
        }
    }
}
