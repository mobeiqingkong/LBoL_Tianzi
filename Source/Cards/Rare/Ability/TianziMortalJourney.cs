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

    // ------------------------------------------------------------------ 凡间之游
    public sealed class TianziMortalJourneyDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 2, White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 5;
            config.Keywords = Keyword.Initial | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Initial | Keyword.Replenish;
            config.Mana = new ManaGroup() { White = 1 };
            config.UpgradedMana = new ManaGroup() { White = 1 };
            config.RelativeEffects = new List<string>() { nameof(TianziMortalJourneySe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 凡间之游：本次战斗中，{PlayerName}每打出 {Value1} 张牌，
    /// 就获得 1 点白色法力并抽 1 张牌。
    /// </summary>
    [EntityLogic(typeof(TianziMortalJourneyDef))]
    public sealed class TianziMortalJourney : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziMortalJourneySe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
