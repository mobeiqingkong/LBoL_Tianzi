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

    public sealed class TianziFeastDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Green };
            config.Cost = new ManaGroup() { Green = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Initial | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Initial | Keyword.Replenish;
            config.Mana = new ManaGroup() { Philosophy = 1 };
            config.UpgradedMana = new ManaGroup() { Philosophy = 1 };
            config.RelativeEffects = new List<string>() { nameof(TianziFeastSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziFeastDef))]
    public sealed class TianziFeast : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziFeastSe>(1, 0, 0, 0, 0.2f);
        }
    }
}
