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
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    // ==================================================================================
    //  稀有 · 技能牌（3 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 天界造物
    public sealed class TianziHeavenCreationDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 2; // 复制份数
            config.Value2 = 2; // 弃牌堆为空时的抽牌数

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天界造物：从弃牌堆中选择一张牌，将它的 {Value1} 份复制置入手中。
    /// 若弃牌堆为空，则抽 {Value2} 张牌。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenCreationDef))]
    public sealed class TianziHeavenCreation : TianziCard
    {
        public override Interaction Precondition()
        {
            List<Card> pool = new List<Card>(base.Battle.DiscardZone);
            if (pool.Count == 0)
                return null;
            return new SelectCardInteraction(1, 1, pool);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            SelectCardInteraction interaction = precondition as SelectCardInteraction;
            if (interaction == null || interaction.SelectedCards.Count == 0)
            {
                // 弃牌堆为空（或玩家未选）时的兜底：抽牌。
                yield return new DrawManyCardAction(base.Value1 > 0 ? base.Value1 : 1);
                yield break;
            }

            Card origin = interaction.SelectedCards[0];
            List<Card> made = new List<Card>();
            for (int i = 0; i < base.Value1; i++)
            {
                made.Add(origin.CloneBattleCard());
            }
            yield return new AddCardsToHandAction(made, AddCardsType.Normal, false);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天界漫游
    public sealed class TianziHeavenRoamDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 3; // 每打出多少张牌取回一次
            config.UpgradedValue1 = 2;

            config.RelativeEffects = new List<string>() { nameof(TianziHeavenRoamSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天界漫游：选择一张手牌。本次战斗中，{PlayerName}每打出 {Value1} 张牌，
    /// 那张牌就会从弃牌堆回到手中。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenRoamDef))]
    public sealed class TianziHeavenRoam : TianziCard
    {
        public override Interaction Precondition()
        {
            List<Card> pool = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
            {
                if (c != this)
                    pool.Add(c);
            }
            if (pool.Count == 0)
                return null;
            return new SelectHandInteraction(1, 1, pool);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            Card chosen = null;
            SelectHandInteraction interaction = precondition as SelectHandInteraction;
            if (interaction != null && interaction.SelectedCards.Count > 0)
                chosen = interaction.SelectedCards[0];

            int need = base.Value1 > 0 ? base.Value1 : 1;
            yield return BuffAction<TianziHeavenRoamSe>(need, 0, 0, 0, 0.2f);

            if (chosen != null)
            {
                TianziHeavenRoamSe se = base.Battle.Player.GetStatusEffect<TianziHeavenRoamSe>();
                if (se != null)
                    se.Target = chosen;
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天界之镜
    public sealed class TianziHeavenMirrorDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { White = 0 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 1; // 本回合尚未打出过其他牌时的抽牌数
            config.UpgradedValue1 = 2;

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天界之镜：将本回合上一张打出的牌的一张复制置入手中，该复制费用为 0 且放逐。
    /// 若本回合尚未打出过其他牌，则抽 {Value1} 张牌。（放逐）
    /// </summary>
    [EntityLogic(typeof(TianziHeavenMirrorDef))]
    public sealed class TianziHeavenMirror : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            Card previous = this.PreviousPlayedCard;
            if (previous == null)
            {
                yield return new DrawManyCardAction(base.Value1 > 0 ? base.Value1 : 1);
                yield break;
            }

            Card copy = previous.CloneBattleCard();
            copy.SetTurnCost(ManaGroup.Empty);
            copy.IsExile = true;
            yield return new AddCardsToHandAction(new Card[] { copy }, AddCardsType.Normal, false);
            yield break;
        }
    }

    // ==================================================================================
    //  稀有 · 能力牌（5 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 桃符「固若金汤的仙桃」
    public sealed class TianziPeachTalismanDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 2, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 6; // 触发时获得的临时生命值
            config.UpgradedValue1 = 9;

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachTalismanSe),
                nameof(TianziTempHpSe),
                nameof(Invincible),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 桃符「固若金汤的仙桃」：每 4 个回合获得 1 回合天衣无缝，
    /// 并立即获得 {Value1} 点临时生命值。
    /// </summary>
    [EntityLogic(typeof(TianziPeachTalismanDef))]
    public sealed class TianziPeachTalisman : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziPeachTalismanSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 凡间之游
    public sealed class TianziMortalJourneyDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 4; // 每打出多少张牌触发一次
            config.UpgradedValue1 = 3;

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
            int need = base.Value1 > 0 ? base.Value1 : 1;
            yield return BuffAction<TianziMortalJourneySe>(need, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天界玉座
    public sealed class TianziHeavenThroneDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 3; // 每回合开始获得的格挡
            config.UpgradedValue1 = 5;

            config.RelativeEffects = new List<string>() { nameof(TianziHeavenThroneSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天界玉座：立即获得 {Value1} 点格挡；此后每回合开始获得 {Value1} 点格挡。</summary>
    [EntityLogic(typeof(TianziHeavenThroneDef))]
    public sealed class TianziHeavenThrone : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Value1,
                0,
                BlockShieldType.Direct,
                false
            );
            yield return BuffAction<TianziHeavenThroneSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 仙桃长久
    public sealed class TianziPeachEternityDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 8; // 临时生命值上限提升量
            config.UpgradedValue1 = 12;

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachEternitySe),
                nameof(TianziTempHpSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 仙桃长久：{PlayerName}的临时生命值上限提高 {Value1}；
    /// 每回合开始获得 {TianziPeachEternitySe:PerTurnTempHp} 点临时生命值。
    /// </summary>
    [EntityLogic(typeof(TianziPeachEternityDef))]
    public sealed class TianziPeachEternity : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziPeachEternitySe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 要石奇点
    public sealed class TianziKeystoneSingularityDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { White = 0 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;

            config.Value1 = 8; // 回合结束时若未受伤获得的格挡
            config.UpgradedValue1 = 12;

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziKeystoneSingularitySe),
                nameof(TianziNextTurnManaSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 要石奇点：每回合结束时，若{PlayerName}本回合未受到过伤害，
    /// 获得 {Value1} 点格挡，并在下回合获得 1 点白色法力。
    /// </summary>
    [EntityLogic(typeof(TianziKeystoneSingularityDef))]
    public sealed class TianziKeystoneSingularity : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziKeystoneSingularitySe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
