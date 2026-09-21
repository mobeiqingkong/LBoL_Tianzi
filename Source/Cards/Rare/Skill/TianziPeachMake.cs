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

    public sealed class TianziPeachMakeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Green };
            config.Cost = new ManaGroup() { Any = 0 };
            config.Rarity = Rarity.Rare;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.UpgradedValue1 = 3;
            config.Keywords = Keyword.Exile | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Replenish;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziPeachMakeDef))]
    public sealed class TianziPeachMake : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            if (TianziTempHp.Get(base.Battle.Player) < 3)
                yield break;
            int used = TianziTempHp.Consume(base.Battle.Player, 3);
            if (used < 3)
                yield break;
            yield return new HealAction(base.Battle.Player, base.Battle.Player, 2, HealType.Normal, 0.2f);
            yield return new GainPowerAction(2);
            yield return new GainManaAction(new ManaGroup() { Philosophy = base.Value1 });
            yield return new DrawManyCardAction(base.Value1);
        }
    }
}
