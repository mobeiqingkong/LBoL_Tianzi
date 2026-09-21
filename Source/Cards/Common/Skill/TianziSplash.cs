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
    public sealed class TianziSplashDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = false;
            config.FindInBattle = true;
            config.Colors = new List<ManaColor>() { ManaColor.Blue };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Common;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Exile | Keyword.Retain;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziSplashDef))]
    public sealed class TianziSplash : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return new DrawManyCardAction(1);
        }
    }
}
