using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.CustomKeywords;
using TianziMod.StatusEffects;

namespace TianziMod.Keywords
{
    public static class TianziKeywords
    {
        public const string ParityId = nameof(TianziParityKwSe);
        public const string KarmaId = nameof(TianziKarmaKwSe);

        public static CardKeyword Parity()
        {
            return new CardKeyword(ParityId) { descPos = KwDescPos.Last };
        }

        public static CardKeyword Karma()
        {
            return new CardKeyword(KarmaId) { descPos = KwDescPos.Last };
        }

        public static bool HasParity(Card card)
        {
            return card != null && card.HasCustomKeyword(ParityId);
        }

        public static bool HasKarma(Card card)
        {
            return card != null && card.HasCustomKeyword(KarmaId);
        }

        public static bool HandHasParity(BattleController battle)
        {
            if (battle == null)
                return false;
            foreach (Card card in battle.HandZone)
            {
                if (HasParity(card))
                    return true;
            }
            return false;
        }
    }

    public static class TianziParityPlay
    {
        public static IEnumerable<BattleAction> Resolve(
            Card source,
            IEnumerable<BattleAction> oddActions,
            IEnumerable<BattleAction> evenActions)
        {
            bool odd;
            if (TianziParity.Forced)
            {
                MiniSelectCardInteraction pick = new MiniSelectCardInteraction(
                    new Card[]
                    {
                        Library.CreateCard<TianziParityOddChoice>(),
                        Library.CreateCard<TianziParityEvenChoice>(),
                    },
                    false,
                    false,
                    false)
                {
                    Source = source,
                };
                yield return new InteractionAction(pick, false);
                odd = pick.SelectedCard is TianziParityOddChoice;
            }
            else
            {
                odd = TianziParity.IsOdd(source == null ? null : source.Battle);
            }

            IEnumerable<BattleAction> chosen = odd ? oddActions : evenActions;
            if (chosen == null)
                yield break;
            foreach (BattleAction action in chosen)
                yield return action;
        }
    }

    public sealed class TianziParityOddChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Rarity = Rarity.Common;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 0 };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziParityOddChoiceDef))]
    public sealed class TianziParityOddChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziParityEvenChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Rarity = Rarity.Common;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 0 };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziParityEvenChoiceDef))]
    public sealed class TianziParityEvenChoice : TianziMod.Cards.Template.TianziCard { }

    public static class TianziKarmaPlay
    {
        public static IEnumerable<BattleAction> Resolve(
            Card source,
            CardType actual,
            IEnumerable<BattleAction> attack,
            IEnumerable<BattleAction> defense,
            IEnumerable<BattleAction> skill,
            IEnumerable<BattleAction> ability,
            IEnumerable<BattleAction> curse)
        {
            CardType chosen = actual;
            if (TianziMod.StatusEffects.TianziKarma.Forced)
            {
                List<Card> options = new List<Card>
                {
                    Library.CreateCard<TianziKarmaAttackChoice>(),
                    Library.CreateCard<TianziKarmaDefenseChoice>(),
                    Library.CreateCard<TianziKarmaSkillChoice>(),
                    Library.CreateCard<TianziKarmaAbilityChoice>(),
                    Library.CreateCard<TianziKarmaCurseChoice>(),
                };
                MiniSelectCardInteraction pick = new MiniSelectCardInteraction(options, false, false, false)
                {
                    Source = source,
                };
                yield return new InteractionAction(pick, false);
                if (pick.SelectedCard is TianziKarmaDefenseChoice)
                    chosen = CardType.Defense;
                else if (pick.SelectedCard is TianziKarmaSkillChoice)
                    chosen = CardType.Skill;
                else if (pick.SelectedCard is TianziKarmaAbilityChoice)
                    chosen = CardType.Ability;
                else if (pick.SelectedCard is TianziKarmaCurseChoice)
                    chosen = CardType.Misfortune;
                else
                    chosen = CardType.Attack;
            }

            IEnumerable<BattleAction> branch = attack;
            switch (chosen)
            {
                case CardType.Defense:
                    branch = defense;
                    break;
                case CardType.Skill:
                    branch = skill;
                    break;
                case CardType.Ability:
                    branch = ability;
                    break;
                case CardType.Status:
                case CardType.Misfortune:
                    branch = curse;
                    break;
            }
            if (branch == null)
                yield break;
            foreach (BattleAction action in branch)
                yield return action;
        }
    }

    public sealed class TianziKarmaAttackChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziKarmaAttackChoiceDef))]
    public sealed class TianziKarmaAttackChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziKarmaDefenseChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziKarmaDefenseChoiceDef))]
    public sealed class TianziKarmaDefenseChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziKarmaSkillChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.Blue };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziKarmaSkillChoiceDef))]
    public sealed class TianziKarmaSkillChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziKarmaAbilityChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.Colorless };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziKarmaAbilityChoiceDef))]
    public sealed class TianziKarmaAbilityChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziKarmaCurseChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Misfortune;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.Black };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziKarmaCurseChoiceDef))]
    public sealed class TianziKarmaCurseChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziPlayChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziPlayChoiceDef))]
    public sealed class TianziPlayChoice : TianziMod.Cards.Template.TianziCard { }

    public sealed class TianziExileChoiceDef : TianziMod.Cards.Template.TianziCardTemplate
    {
        public override LBoL.ConfigData.CardConfig MakeConfig()
        {
            LBoL.ConfigData.CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.HideMesuem = true;
            config.FindInBattle = false;
            config.IsUpgradable = false;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Nobody;
            config.Colors = new List<ManaColor>() { ManaColor.Black };
            config.Index = TianziMod.Cards.Template.CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [LBoLEntitySideloader.Attributes.EntityLogic(typeof(TianziExileChoiceDef))]
    public sealed class TianziExileChoice : TianziMod.Cards.Template.TianziCard { }
}
