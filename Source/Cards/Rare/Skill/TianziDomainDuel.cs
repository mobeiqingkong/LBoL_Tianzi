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

    public sealed class TianziDomainDuelDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7510);
            config.GunNameBurst = GunNameID.GetGunFromId(7510);
            config.Colors = new List<ManaColor>() { ManaColor.Black };
            config.Cost = new ManaGroup() { Any = 1, Black = 1 };
            config.UpgradedCost = new ManaGroup() { Black = 1 };
            config.Rarity = Rarity.Rare;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Exile | Keyword.Retain;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;
            config.RelativeEffects = new List<string>() { nameof(TianziReflectSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziDomainDuelDef))]
    public sealed class TianziDomainDuel : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziReflectSe>(1, 0, 0, 0, 0.2f);
        }
    }
}
