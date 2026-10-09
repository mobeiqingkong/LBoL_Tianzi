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
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;
            config.Block = 6;
            config.UpgradedBlock = 9;
            config.Scry = 2;
            config.UpgradedScry = 3;
            config.RelativeCards = new List<string>() { nameof(TianziPlayChoice), nameof(TianziExileChoice) };
            config.UpgradedRelativeCards = config.RelativeCards;
            config.Illustrator = "竜崎いち";
            config.RelativeKeyword = Keyword.Block | Keyword.Scry | Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Scry | Keyword.Exile;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 无念无想：占卜 {Scry}，获得格挡，抽取抽牌堆顶一张牌后可选打出或放逐。
    /// </summary>
    [EntityLogic(typeof(TianziMushinDef))]
    public sealed class TianziMushin : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return new ScryAction(base.Scry);
            yield return base.DefenseAction(true);
            if (base.Battle.BattleShouldEnd || base.Battle.DrawZone.Count == 0)
                yield break;

            DrawManyCardAction draw = new DrawManyCardAction(1);
            yield return draw;
            if (draw.DrawnCards.Count == 0)
                yield break;

            Card drawn = draw.DrawnCards[0];
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
                yield return new ExileCardAction(drawn);
            else
                yield return new PlayCardAction(drawn);
        }
    }
}
