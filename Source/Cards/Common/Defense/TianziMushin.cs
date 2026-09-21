using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.Patches;
using UnityEngine;

namespace TianziMod.Cards
{
    public sealed class TianziMushinDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;
            config.Block = 8;
            config.UpgradedBlock = 12;
            config.RelativeCards = new List<string>() { nameof(TianziPlayChoice), nameof(TianziExileChoice) };
            config.UpgradedRelativeCards = config.RelativeCards;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block | Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Exile;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziMushinDef))]
    public sealed class TianziMushin : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.DefenseAction(true);
            if (base.Battle.BattleShouldEnd)
                yield break;

            List<Card> defenses = new List<Card>();
            foreach (Card c in base.Battle.DrawZone)
            {
                if (c != null && c.CardType == CardType.Defense)
                    defenses.Add(c);
            }
            if (defenses.Count == 0)
                yield break;

            Card pick = defenses[Random.Range(0, defenses.Count)];
            MiniSelectCardInteraction choice = new MiniSelectCardInteraction(
                TianziMiniSelectSkin.BindAll(
                    this,
                    Library.CreateCard<TianziPlayChoice>(),
                    Library.CreateCard<TianziExileChoice>()),
                false, false, false)
            {
                Source = this,
            };
            yield return new InteractionAction(choice, false);
            if (choice.SelectedCard is TianziExileChoice)
                yield return new ExileCardAction(pick);
            else
                yield return new PlayCardAction(pick);
        }
    }
}
