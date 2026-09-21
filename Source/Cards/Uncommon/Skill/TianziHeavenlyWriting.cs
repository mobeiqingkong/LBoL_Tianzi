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
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 天人落书
    public sealed class TianziHeavenlyWritingDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 2; // 抽牌数
            config.Value2 = 2; // 庇护层数

            config.RelativeEffects = new List<string>()
            {
                nameof(Vulnerable),
                nameof(AmuletForCard),
                nameof(TianziKarmaKwSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 天人落书：选择一张手牌放逐，抽 {Value1} 张牌。
    /// 被放逐的牌是 攻击 / 防御 / 技能 / 能力 / 诅咒或状态 时，
    /// 分别对应 易伤 / 脆弱 / 多抽 1 张 / {Value2} 层庇护 / 获得一张光芒。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyWritingDef))]
    public sealed class TianziHeavenlyWriting : TianziCard
    {
        protected override bool HasKarmaKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            Card exiled = null;
            List<Card> hand = new List<Card>(base.Battle.HandZone);
            if (hand.Count > 0)
            {
                SelectHandInteraction interaction = new SelectHandInteraction(1, 1, hand) { Source = this };
                yield return new InteractionAction(interaction, false);
                if (interaction.SelectedCards.Count > 0)
                    exiled = interaction.SelectedCards[0];
            }
            if (exiled != null)
                yield return new ExileCardAction(exiled);
            yield return new DrawManyCardAction(base.Value1);
            if (exiled == null)
                yield break;
            foreach (BattleAction action in TianziKarmaPlay.Resolve(
                this,
                exiled.CardType,
                this.WritingVuln(),
                this.WritingVuln(),
                this.WritingDraw(),
                this.WritingAmulet(),
                this.WritingLight()))
                yield return action;
        }

        private IEnumerable<BattleAction> WritingVuln()
        {
            List<EnemyUnit> enemies = new List<EnemyUnit>(base.Battle.AllAliveEnemies);
            if (enemies.Count == 0)
                yield break;
            EnemyUnit pick = enemies[UnityEngine.Random.Range(0, enemies.Count)];
            yield return base.DebuffAction<Vulnerable>(pick, 1, 1, 0, 0, true, 0.2f);
        }

        private IEnumerable<BattleAction> WritingDraw()
        {
            yield return new DrawManyCardAction(1);
        }

        private IEnumerable<BattleAction> WritingAmulet()
        {
            yield return BuffAction<AmuletForCard>(base.Value2, 0, 0, 0, 0.2f);
        }

        private IEnumerable<BattleAction> WritingLight()
        {
            yield return new AddCardsToHandAction(
                new Card[] { Library.CreateCard<LBoL.EntityLib.Cards.Neutral.NoColor.WManaCard>() });
        }
    }
}
