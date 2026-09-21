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
    // ==================================================================================
    //  罕见 · 防御牌（3 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 要石覆体
    public sealed class TianziKeystoneArmorDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 10;
            config.UpgradedBlock = 12;

            config.Shield = 3;
            config.UpgradedShield = 5;

            config.Value1 = 2; // 本回合已打出防御牌时的追加护盾
            config.UpgradedValue1 = 3;

            config.Keywords = Keyword.Shield;
            config.UpgradedKeywords = Keyword.Shield;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 要石覆体：获得 {Block} 点格挡和 {Shield} 点护盾。
    /// 若本回合已打出过其他防御牌，护盾额外 +{Value1}。
    /// </summary>
    [EntityLogic(typeof(TianziKeystoneArmorDef))]
    public sealed class TianziKeystoneArmor : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            int block = base.HasBlock ? base.Block.Block : 0;
            int shield = base.HasShield ? base.Shield.Shield : 0;
            if (this.CountTurnPlayed(CardType.Defense) > 0)
                shield += base.Value1;

            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                block,
                shield,
                BlockShieldType.Normal,
                true
            );
            yield break;
        }
    }

    // ------------------------------------------------------------------ 晴空万里
    public sealed class TianziClearSkyDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            // Defense 牌必须在 config 里带 Block 或 Shield，否则 Card.Verify() 会
            // Debug.LogError("<Id> must set block or shield in config")。
            // 这张卡是「每放逐一张牌获得护盾」，把【单张护盾值】直接放在 Shield 上，
            // 卡面 {Shield} 显示的数值就和实际每张给的护盾一致。
            config.Shield = 5;
            config.UpgradedShield = 6;
            config.Value1 = 2; // 最多放逐张数

            config.Keywords = Keyword.Exile | Keyword.Echo;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Echo;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 晴空万里：从手牌、抽牌堆和弃牌堆中，选择至多 {Value2} 张牌放逐。
    /// 每放逐一张牌，获得 {Value1} 点护盾。（放逐 / 回响）
    /// </summary>
    [EntityLogic(typeof(TianziClearSkyDef))]
    public sealed class TianziClearSky : TianziCard
    {
        // 混合 zone 的选牌池官方写法（见 Cirno 的 FreezeToIce）：
        //  · 池子在 Precondition() 里构建，而不是在 Actions() 里另开一个 InteractionAction；
        //  · 必须排除 this；
        //  · 抽牌堆要用 DrawZoneToShow（UI 能显示的那一份）；
        //  · 多张牌一起放逐用 ExileManyCardAction，而不是在 foreach 里逐张 ExileCardAction
        //    —— 后者会在 play-area / 手牌控件簿记上留下脏状态，进而让之后每次出牌都抛 NRE。
        public override Interaction Precondition()
        {
            List<Card> pool = new List<Card>();
            foreach (Card c in base.Battle.HandZone)
                if (c != null && c != this) pool.Add(c);
            foreach (Card c in base.Battle.DrawZoneToShow)
                if (c != null && c != this) pool.Add(c);
            foreach (Card c in base.Battle.DiscardZone)
                if (c != null) pool.Add(c);
            if (pool.Count == 0)
                return null;
            return new SelectCardInteraction(0, base.Value1, pool, SelectedCardHandling.DoNothing);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            SelectCardInteraction interaction = precondition as SelectCardInteraction;
            if (interaction == null || interaction.SelectedCards.Count == 0)
                yield break;

            List<Card> picks = new List<Card>(interaction.SelectedCards);
            yield return new ExileManyCardAction(picks);

            int shield = base.ConfigShield * picks.Count;
            if (shield > 0)
            {
                yield return new CastBlockShieldAction(
                    base.Battle.Player,
                    base.Battle.Player,
                    0,
                    shield,
                    BlockShieldType.Direct,
                    false
                );
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 有顶天穹
    public sealed class TianziHeavenCanopyDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 9;
            config.UpgradedBlock = 12;

            config.Value1 = 1; // 抽牌数
            config.UpgradedValue1 = 2;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>有顶天穹：获得 {Block} 点格挡。抽 {Value1} 张牌。</summary>
    [EntityLogic(typeof(TianziHeavenCanopyDef))]
    public sealed class TianziHeavenCanopy : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);
            yield return new DrawManyCardAction(base.Value1);
            yield break;
        }
    }

    // ==================================================================================
    //  罕见 · 技能牌（5 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 天人的飞翔
    public sealed class TianziHeavenlyFlightDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 0, Red = 0 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Mana = new ManaGroup() { Any = 3 };
            config.UpgradedMana = new ManaGroup() { Any = 4 };

            config.Value1 = 1; // 抽牌数
            config.UpgradedValue1 = 2;

            config.Keywords = Keyword.Exile | Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Replenish;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天人的飞翔：获得 {Mana} 点彩色费用。抽 {Value1} 张牌。（放逐 / 填充）</summary>
    [EntityLogic(typeof(TianziHeavenlyFlightDef))]
    public sealed class TianziHeavenlyFlight : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new GainManaAction(base.Mana);
            yield return new DrawManyCardAction(base.Value1);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天人的直觉
    public sealed class TianziHeavenlyInstinctDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 0 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Mana = new ManaGroup() { White = 1, Red = 1 };
            config.Value1 = 2; // 偶数手牌抽牌数

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.None;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天人的直觉：手牌张数为奇数时，获得 {Mana} 点费用；
    /// 为偶数时，抽 {Value1} 张牌。（放逐；升级后取消放逐）
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyInstinctDef))]
    public sealed class TianziHeavenlyInstinct : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            if (base.Battle.HandZone.Count % 2 == 1)
                yield return new GainManaAction(base.Mana);
            else
                yield return new DrawManyCardAction(base.Value1);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天人的流仪
    public sealed class TianziHeavenlyRitualDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { White = 0 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Scry = 5;
            config.UpgradedScry = 5;

            config.Value1 = 2; // 按类型追加的抽牌数

            config.RelativeEffects = new List<string>() { nameof(Firepower), nameof(Spirit) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天人的流仪：占卜 {Scry}。抽 1 张牌。
    /// 若抽到攻击牌获得 1 点火力；防御牌获得 1 点灵力；技能牌额外抽 {Value1} 张。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyRitualDef))]
    public sealed class TianziHeavenlyRitual : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new ScryAction(base.Scry);

            HashSet<Card> before = new HashSet<Card>();
            foreach (Card c in base.Battle.HandZone)
                before.Add(c);

            yield return new DrawManyCardAction(1);

            Card drawn = null;
            foreach (Card c in base.Battle.HandZone)
            {
                if (!before.Contains(c))
                {
                    drawn = c;
                    break;
                }
            }
            if (drawn == null)
                yield break;

            switch (drawn.CardType)
            {
                case CardType.Attack:
                    yield return BuffAction<Firepower>(1, 0, 0, 0, 0.2f);
                    break;
                case CardType.Defense:
                    yield return BuffAction<Spirit>(1, 0, 0, 0, 0.2f);
                    break;
                default:
                    yield return new DrawManyCardAction(base.Value1);
                    break;
            }
            yield break;
        }
    }

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
                nameof(Fragil),
                nameof(AmuletForCard),
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
                SelectHandInteraction interaction = new SelectHandInteraction(1, 1, hand)
                {
                    Source = this,
                };
                yield return new InteractionAction(interaction, false);
                if (interaction.SelectedCards.Count > 0)
                    exiled = interaction.SelectedCards[0];
            }

            if (exiled != null)
                yield return new ExileCardAction(exiled);

            yield return new DrawManyCardAction(base.Value1);

            if (exiled == null)
                yield break;

            switch (exiled.CardType)
            {
                case CardType.Attack:
                    // ⚠ 原来 break 写在 foreach 内部 -> 只对「第一个存活敌人」生效。改为对全体生效。
                    foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
                    {
                        if (base.Battle.BattleShouldEnd)
                            yield break;
                        yield return base.DebuffAction<Vulnerable>(enemy, 1, 1, 0, 0, true, 0.2f);
                    }
                    break;

                case CardType.Defense:
                    foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
                    {
                        if (base.Battle.BattleShouldEnd)
                            yield break;
                        yield return base.DebuffAction<Fragil>(enemy, 1, 1, 0, 0, true, 0.2f);
                    }
                    break;

                case CardType.Ability:
                    yield return BuffAction<AmuletForCard>(base.Value2, 0, 0, 0, 0.2f);
                    break;

                case CardType.Status:
                case CardType.Misfortune:
                    yield return new AddCardsToHandAction(
                        new Card[] { Library.CreateCard<LBoL.EntityLib.Cards.Neutral.NoColor.WManaCard>() }
                    );
                    break;

                default:
                    yield return new DrawManyCardAction(1);
                    break;
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天候掌握
    public sealed class TianziWeatherMasteryDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 2; // 天气持续回合
            config.UpgradedValue1 = 3;
            config.Value2 = 5; // 格挡
            config.UpgradedValue2 = 7;

            config.RelativeEffects = new List<string>()
            {
                nameof(TianziWeatherClear),
                nameof(TianziWeatherMist),
                nameof(TianziWeatherCloud),
                nameof(TianziWeatherAzure),
                nameof(TianziWeatherHail),
                nameof(TianziWeatherFog),
                nameof(TianziWeatherTyphoon),
                nameof(TianziWeatherCalm),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天候掌握：随机释放一种天气（持续 {Value1} 回合），获得 {Value2} 点格挡。</summary>
    [EntityLogic(typeof(TianziWeatherMasteryDef))]
    public sealed class TianziWeatherMastery : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            foreach (BattleAction action in TianziWeather.ApplyRandom(base.Battle.Player, base.Value1))
                yield return action;

            yield return new CastBlockShieldAction(
                base.Battle.Player,
                base.Battle.Player,
                base.Value2,
                0,
                BlockShieldType.Direct,
                false
            );
            yield break;
        }
    }
}
