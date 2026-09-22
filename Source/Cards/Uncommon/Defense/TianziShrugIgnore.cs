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
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziShrugIgnoreDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Blue };
            config.Cost = new ManaGroup() { White = 1, Blue = 1 };
            config.UpgradedCost = new ManaGroup() { Hybrid = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;
            config.Block = 8;
            config.UpgradedBlock = 12;
            config.RelativeEffects = new List<string>() { nameof(TianziShrugSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziShrugIgnoreDef))]
    public sealed class TianziShrugIgnore : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.DefenseAction(true);
            yield return BuffAction<TianziShrugSe>(1, 0, 0, 0, 0.2f);
        }
    }
}
