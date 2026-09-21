using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    // ==================================================================================
    //  稀有 · 攻击牌（7 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 终绝的一剑
    public sealed class TianziFinalBladeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.VioletAura;
            config.GunNameBurst = GunNameID.VioletAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 9;
            config.UpgradedDamage = 12;

            config.Value1 = 2; // 每失去 5% 生命值提高的伤害百分比
            config.UpgradedValue1 = 3;

            config.Keywords = Keyword.Exile | Keyword.Retain;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 终绝的一剑：造成 {Damage} 点伤害。
    /// 目标每失去 5% 生命值，此牌伤害提高 {Value1}%。（放逐 / 保留）
    /// </summary>
    [EntityLogic(typeof(TianziFinalBladeDef))]
    public sealed class TianziFinalBlade : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            float base6 = base.Damage.Damage;
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                int steps = 0;
                if (enemy.MaxHp > 0)
                {
                    float lost = 1f - (float)enemy.Hp / enemy.MaxHp;
                    steps = (int)Math.Floor(lost * 20f);
                }
                float damage = base6 * (1f + steps * base.Value1 / 100f);

                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    DamageInfo.Attack(damage, false),
                    base.GunName,
                    GunType.Single
                );
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 先忧后乐之剑
    public sealed class TianziHardshipFirstDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Sword;
            config.GunNameBurst = GunNameID.Sword;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 12;
            config.UpgradedDamage = 14;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 先忧后乐之剑：造成 {Damage} 点伤害。
    /// 手牌张数为奇数时，打出后置于抽牌堆顶；
    /// 为偶数时，保留所有手牌一回合。
    /// </summary>
    [EntityLogic(typeof(TianziHardshipFirstDef))]
    public sealed class TianziHardshipFirst : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            if (base.Battle.HandZone.Count % 2 == 1)
            {
                yield return new MoveCardToDrawZoneAction(this, DrawZoneTarget.Top);
            }
            else
            {
                List<Card> hand = new List<Card>(base.Battle.HandZone);
                foreach (Card c in hand)
                {
                    yield return new RetainAction(c);
                }
            }
            yield break;
        }
    }

    // -------------------------------------------------- 非想「非想非非想之剑」
    public sealed class TianziUnthinkableBladeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.VioletAura;
            config.GunNameBurst = GunNameID.VioletAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 3, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2, Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 18;
            config.UpgradedDamage = 24;

            config.Value1 = 2; // 天气持续回合
            config.UpgradedValue1 = 3;

            config.RelativeEffects = new List<string>()
            {
                nameof(ExtraTurn),
                nameof(TimeIsLimited),
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

    /// <summary>
    /// 非想「非想非非想之剑」：造成 {Damage} 点伤害。
    /// 随机释放一种天气并持续 {Value1} 回合。
    /// 结束{PlayerName}的回合，进行一个额外的回合，并获得 1 点|时间有限|。
    /// </summary>
    [EntityLogic(typeof(TianziUnthinkableBladeDef))]
    public sealed class TianziUnthinkableBlade : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            foreach (BattleAction action in TianziWeather.ApplyRandom(
                base.Battle.Player, base.Value1))
            {
                yield return action;
            }

            yield return BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
            yield return base.DebuffAction<TimeIsLimited>(
                base.Battle.Player,
                1,
                0,
                0,
                0,
                true,
                0.2f
            );
            yield return new RequestEndPlayerTurnAction();
            yield break;
        }
    }

    // ------------------------------------------------------------------ 恒净之黎
    public sealed class TianziPureDawnDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GoldAura;
            config.GunNameBurst = GunNameID.GoldAura;

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 1, Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 10;
            config.UpgradedDamage = 15;

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile;

            config.RelativeEffects = new List<string>() { nameof(TianziRegenSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 恒净之黎：造成 {Damage} 点伤害，移除{PlayerName}身上的所有负面效果，
    /// 并获得造成伤害减半的自愈。（放逐）
    /// </summary>
    [EntityLogic(typeof(TianziPureDawnDef))]
    public sealed class TianziPureDawn : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            yield return new RemoveAllNegativeStatusEffectAction(base.Battle.Player, 0.2f);

            int regen = Math.Max(1, (int)(base.Damage.Damage / 2f));
            yield return new ApplyStatusEffectAction<TianziRegenSe>(
                base.Battle.Player,
                regen,
                null,
                null,
                null,
                0.2f
            );
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天地开辟之剑
    public sealed class TianziWorldCleaverDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Heaven;
            config.GunNameBurst = GunNameID.Heaven;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 3 };
            config.UpgradedCost = new ManaGroup() { Red = 2 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;

            config.Damage = 22;
            config.UpgradedDamage = 28;

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.Exile;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天地开辟之剑：对所有敌人造成 {Damage} 点伤害。（放逐）</summary>
    [EntityLogic(typeof(TianziWorldCleaverDef))]
    public sealed class TianziWorldCleaver : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, 1, false);
            yield return base.AttackAllAliveEnemyAction();
            yield break;
        }
    }

    // ------------------------------------------------------------------ 绯想天变
    public sealed class TianziScarletApocalypseDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.RedAura;
            config.GunNameBurst = GunNameID.RedAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 2, Red = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 2, Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 20;
            config.UpgradedDamage = 26;

            config.Value1 = 3; // 天气持续回合

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

    /// <summary>绯想天变：造成 {Damage} 点伤害，随机释放一种天气并持续 {Value1} 回合。</summary>
    [EntityLogic(typeof(TianziScarletApocalypseDef))]
    public sealed class TianziScarletApocalypse : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            foreach (BattleAction action in TianziWeather.ApplyRandom(
                base.Battle.Player, base.Value1))
            {
                yield return action;
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 终焉审判
    public sealed class TianziFinalJudgementDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Heaven;
            config.GunNameBurst = GunNameID.Heaven;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 3, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2, White = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 15;
            config.UpgradedDamage = 20;

            config.Value1 = 3; // 每张被放逐过的牌提供的追加伤害
            config.UpgradedValue1 = 4;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 终焉审判：造成 {Damage} 点伤害。
    /// 本场战斗中每有一张牌被放逐，此牌伤害提高 {Value1} 点。
    /// </summary>
    [EntityLogic(typeof(TianziFinalJudgementDef))]
    public sealed class TianziFinalJudgement : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            int exiled = base.Battle.ExileZone.Count;
            float damage = base.Damage.Damage + exiled * base.Value1;

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    DamageInfo.Attack(damage, false),
                    base.GunName,
                    GunType.Single
                );
            }
            yield break;
        }
    }
}
