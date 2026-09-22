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

    // ------------------------------------------------------------------ 漫漫桃园
    public sealed class TianziPeachGardenDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Initial;
            config.UpgradedKeywords = Keyword.Initial;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>漫漫桃园：临时生命值减少时，获得 1 点临时生命值。（固有）</summary>
    [EntityLogic(typeof(TianziPeachGardenDef))]
    public sealed class TianziPeachGarden : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPeachGardenSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
