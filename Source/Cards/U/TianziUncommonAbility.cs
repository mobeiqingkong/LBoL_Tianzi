using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    // ==================================================================================
    //  罕见 · 能力牌（13 张）
    //  说明：能力牌的共同形态是「打出后获得一个持续状态」，这里统一用 BuffAction 施加。
    // ==================================================================================

    // ------------------------------------------------------------------ 绯想的威光
    public sealed class TianziScarletRadianceDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 2, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.RelativeEffects = new List<string>() { nameof(TianziScarletRadianceSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>绯想的威光：每当一张牌被放逐，对所有敌人造成 {Value1} 点攻击伤害。</summary>
    [EntityLogic(typeof(TianziScarletRadianceDef))]
    public sealed class TianziScarletRadiance : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziScarletRadianceSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 绯想剑斩波
    public sealed class TianziScarletWaveDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.RelativeEffects = new List<string>() { nameof(TianziScarletWaveSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>绯想剑斩波：每回合首次造成攻击伤害时（含符卡），获得等量的格挡。</summary>
    [EntityLogic(typeof(TianziScarletWaveDef))]
    public sealed class TianziScarletWave : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziScarletWaveSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天人之气
    public sealed class TianziOddEvenQiDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 1, Red = 1 };
            // Card.Verify()：升级费用不得比原费用贵。
            // 原写 {Any=1, White=1, Red=1} 是 3 点 > 原费 2 点 -> 启动时 throw，卡死主菜单。
            // 升级语义改为「费用颜色解绑」：双色 1+1 -> 任意 2，总额不变、各色分量不上升。
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.RelativeEffects = new List<string>() { nameof(TianziOddEvenSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天人之气：手牌数量的奇偶效果变为选择触发。</summary>
    [EntityLogic(typeof(TianziOddEvenQiDef))]
    public sealed class TianziOddEvenQi : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziOddEvenSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 仙桃增幅
    public sealed class TianziPeachBoostDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.UpgradedValue1 = 4;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachBoostSe),
                nameof(TianziTempHpSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>仙桃增幅：获得临时生命值时额外获得 1 点；立即获得 {Value1} 点临时生命值。</summary>
    [EntityLogic(typeof(TianziPeachBoostDef))]
    public sealed class TianziPeachBoost : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPeachBoostSe>(1, 0, 0, 0, 0.2f);
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, base.Value1);
            if (gain != null)
                yield return gain;
            yield break;
        }
    }

    // ------------------------------------------------------------------ 漫漫桃园
    public sealed class TianziPeachGardenDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Initial;
            config.UpgradedKeywords = Keyword.Initial;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachGardenSe),
                nameof(TianziTempHpSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>漫漫桃园：临时生命值减少时，获得 1 点临时生命值。（固有）</summary>
    [EntityLogic(typeof(TianziPeachGardenDef))]
    public sealed class TianziPeachGarden : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPeachGardenSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 清霖之愿
    public sealed class TianziPureWishDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 2, White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.UpgradedValue1 = 3;
            config.RelativeEffects = new List<string>() { nameof(TianziPureWishSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>清霖之愿：每回合前 {Value1} 次抽到诅咒或状态牌时，将其放逐并抽 1 张牌。</summary>
    [EntityLogic(typeof(TianziPureWishDef))]
    public sealed class TianziPureWish : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPureWishSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天界之庇护
    public sealed class TianziHeavenShieldDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziHeavenShieldSe),
                nameof(AmuletForCard),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天界之庇护：每回合结束时获得 1 层庇护，下回合获得 1 点白色法力。</summary>
    [EntityLogic(typeof(TianziHeavenShieldDef))]
    public sealed class TianziHeavenShield : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziHeavenShieldSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 冥想
    public sealed class TianziMeditationDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Keywords = Keyword.Initial;
            config.UpgradedKeywords = Keyword.Initial;
            config.RelativeEffects = new List<string>() { nameof(TianziMeditationSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>冥想：每回合多抽一张牌；回合开始时，选择一张手牌与弃牌堆内的一张牌互换。（固有）</summary>
    [EntityLogic(typeof(TianziMeditationDef))]
    public sealed class TianziMeditation : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziMeditationSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天人的耐性
    public sealed class TianziEnduranceDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 3 };
            config.UpgradedCost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.RelativeEffects = new List<string>() { nameof(TianziEnduranceSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天人的耐性：回合开始时只失去一半格挡；
    /// 受到不高于 5 点的未被格挡攻击伤害时，将伤害降低为 1。
    /// </summary>
    [EntityLogic(typeof(TianziEnduranceDef))]
    public sealed class TianziEndurance : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziEnduranceSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 桃华
    public sealed class TianziPeachBlossomDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 8; // 触发所需的临时生命值
            config.UpgradedValue1 = 6;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziPeachBlossomSe),
                nameof(TianziTempHpSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 桃华：每回合结束时，若{PlayerName}的临时生命值不少于 {Value1}，
    /// 抽 1 张牌并在下回合获得 1 点白色法力。
    /// </summary>
    [EntityLogic(typeof(TianziPeachBlossomDef))]
    public sealed class TianziPeachBlossom : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziPeachBlossomSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 绯色领域
    public sealed class TianziScarletDomainDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 2, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.RelativeEffects = new List<string>() { nameof(TianziScarletDomainSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>绯色领域：每当一张牌被放逐，{PlayerName}获得 {Value1} 点格挡。</summary>
    [EntityLogic(typeof(TianziScarletDomainDef))]
    public sealed class TianziScarletDomain : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziScarletDomainSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 要石镇守
    public sealed class TianziKeystoneWardDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 2;
            config.UpgradedValue1 = 3;
            config.RelativeEffects = new List<string>() { nameof(TianziKeystoneWardSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>要石镇守：每回合开始获得 {Value1} 点格挡；受到的攻击伤害减少 1 点。</summary>
    [EntityLogic(typeof(TianziKeystoneWardDef))]
    public sealed class TianziKeystoneWard : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziKeystoneWardSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天人合一
    public sealed class TianziHeavenlyTempoDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 1;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziHeavenlyTempoSe),
                nameof(Firepower),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天人合一：每回合开始时，手牌张数为奇数则获得 {Value1} 点火力；
    /// 为偶数则获得 {Value1} ×2 点格挡。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyTempoDef))]
    public sealed class TianziHeavenlyTempo : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziHeavenlyTempoSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
