using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Intentions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    // ==================================================================================
    //  罕见 · 攻击牌（10 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 气焰万丈之剑
    public sealed class TianziBlazingSwordDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.RedAura;
            config.GunNameBurst = GunNameID.RedAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.IsXCost = true;
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 5;
            config.UpgradedDamage = 6;

            config.Value1 = 1; // 易伤回合
            config.UpgradedValue1 = 2;

            config.Keywords = Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Accuracy;

            config.RelativeEffects = new List<string>() { nameof(Vulnerable) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Vulnerable) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 气焰万丈之剑（X 费）：施加 {Value1} 回合易伤，造成 {Damage} 点伤害。
    /// 每额外支付 1 点任意费用，攻击段数 +1。（精准）
    /// </summary>
    [EntityLogic(typeof(TianziBlazingSwordDef))]
    public sealed class TianziBlazingSword : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            int extra = base.SynergyAmount(consumingMana, ManaColor.Any, 1);
            int hits = 1 + extra;

            base.CardGuns = new Guns(base.GunName, hits, true);
            foreach (GunPair gunPair in base.CardGuns.GunPairs)
            {
                yield return base.AttackAction(selector, gunPair);
            }

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return base.DebuffAction<Vulnerable>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
            }
            yield break;
        }
    }

    // -------------------------------------------------------------------- 越战越勇
    public sealed class TianziGrowingBraverDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Sword;
            config.GunNameBurst = GunNameID.Sword;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 12;
            config.UpgradedDamage = 15;

            config.Value1 = 1; // 火力

            config.RelativeEffects = new List<string>() { nameof(Firepower) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Firepower) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>越战越勇：若敌人意图为攻击，获得 {Value1} 点火力。造成 {Damage} 点伤害。</summary>
    [EntityLogic(typeof(TianziGrowingBraverDef))]
    public sealed class TianziGrowingBraver : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            bool incomingAttack = false;
            foreach (EnemyUnit enemy in selector.GetEnemies(base.Battle))
            {
                if (enemy == null || enemy.Intentions == null)
                    continue;
                if (enemy.Intentions.Any(i => i is AttackIntention))
                {
                    incomingAttack = true;
                    break;
                }
            }

            if (incomingAttack)
                yield return BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);

            yield return base.AttackAction(selector);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 坠入大气圈
    public sealed class TianziMeteorPlungeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Quake;
            config.GunNameBurst = GunNameID.Quake;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 2, Red = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;

            config.Damage = 17;
            config.UpgradedDamage = 21;

            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(TianziTempHpSe) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 坠入大气圈：对所有敌人造成 {Damage} 点伤害。
    /// 再对所有敌人造成一次{PlayerName}临时生命值 ×2 的伤害。
    /// </summary>
    [EntityLogic(typeof(TianziMeteorPlungeDef))]
    public sealed class TianziMeteorPlunge : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, 1, false);
            yield return base.AttackAllAliveEnemyAction();

            int temp = TianziTempHp.Get(base.Battle.Player);
            if (temp > 0)
            {
                yield return new DamageAction(
                    base.Battle.Player,
                    base.Battle.AllAliveEnemies,
                    DamageInfo.Attack(temp * 2f, false),
                    base.GunName,
                    GunType.Single
                );
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 一击震乾坤
    public sealed class TianziQuakeWorldDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Quake;
            config.GunNameBurst = GunNameID.Quake;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 13;
            config.UpgradedDamage = 17;

            config.Mana = new ManaGroup() { Red = 1 };

            config.Value1 = 2; // 每张攻击牌的追加伤害
            config.UpgradedValue1 = 3;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 一击震乾坤：造成 {Damage} 点伤害，获得 1 点红色法力。
    /// 本回合每打出过一张其他攻击牌，此牌伤害提高 {Value1} 点。
    /// </summary>
    [EntityLogic(typeof(TianziQuakeWorldDef))]
    public sealed class TianziQuakeWorld : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            int attackCount = this.CountTurnPlayed(CardType.Attack);
            float damage = base.Damage.Damage + attackCount * base.Value1;

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
            yield return new GainManaAction(new ManaGroup() { Red = 1 });
            yield break;
        }
    }

    // ------------------------------------------------------------------ 摧枯拉朽
    public sealed class TianziOverwhelmingDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.HeavySlash;
            config.GunNameBurst = GunNameID.HeavySlash;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = new ManaGroup() { Red = 0 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 8;
            config.UpgradedDamage = 10;

            config.Value1 = 2; // 易伤回合

            config.RelativeEffects = new List<string>() { nameof(Vulnerable) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Vulnerable) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 摧枯拉朽：造成 {Damage} 点伤害。
    /// 若此伤害突破了目标的格挡或护盾，施加 {Value1} 回合易伤。
    /// </summary>
    [EntityLogic(typeof(TianziOverwhelmingDef))]
    public sealed class TianziOverwhelming : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            List<Unit> targets = new List<Unit>(selector.GetUnits(base.Battle));
            bool[] wasGuarded = new bool[targets.Count];
            for (int i = 0; i < targets.Count; i++)
                wasGuarded[i] = targets[i].Block > 0 || targets[i].Shield > 0;

            yield return base.AttackAction(selector);

            for (int i = 0; i < targets.Count; i++)
            {
                Unit enemy = targets[i];
                if (!enemy.IsAlive || !wasGuarded[i])
                    continue;
                if (enemy.Block <= 0 && enemy.Shield <= 0)
                {
                    yield return base.DebuffAction<Vulnerable>(
                        enemy, 1, base.Value1, 0, 0, true, 0.2f);
                }
            }
            yield break;
        }
    }

    // -------------------------------------------------------------------- 环舞之剑
    public sealed class TianziDanceSwordDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Slash;
            config.GunNameBurst = GunNameID.Slash;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            // 设计稿的「0 费白色牌，扣除 1 点白色法力」用 1 点白色费用表达，
            // 由引擎保证「付不起就不能打出」。
            config.Cost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 6;
            config.UpgradedDamage = 9;

            config.Value1 = 1; // 虚弱回合
            config.Value2 = 1; // 下回合获得的白色法力
            config.UpgradedValue2 = 2;

            config.RelativeEffects = new List<string>()
            {
                nameof(Weak),
                nameof(TianziNextTurnManaSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 环舞之剑：造成 {Damage} 点伤害，施加 {Value1} 回合虚弱。
    /// 下回合获得 {Value2} 点白色法力，抽 1 张牌。
    /// </summary>
    [EntityLogic(typeof(TianziDanceSwordDef))]
    public sealed class TianziDanceSword : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return base.DebuffAction<Weak>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
            }

            yield return new ApplyStatusEffectAction<TianziNextTurnManaSe>(
                base.Battle.Player,
                null,
                null,
                base.Value2,
                null,
                0.1f
            );
            yield return new DrawManyCardAction(1);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 大地液化
    public sealed class TianziLandLiquefyDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Quake;
            config.GunNameBurst = GunNameID.Quake;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;

            config.Damage = 16;
            config.UpgradedDamage = 20;

            config.Value1 = 5; // 扣除的格挡 / 护盾
            config.UpgradedValue1 = 7;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 大地液化：对所有敌人造成 {Damage} 点伤害，并扣除其格挡和护盾各 {Value1} 点。
    /// </summary>
    [EntityLogic(typeof(TianziLandLiquefyDef))]
    public sealed class TianziLandLiquefy : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, 1, false);
            yield return base.AttackAllAliveEnemyAction();

            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || (enemy.Block <= 0 && enemy.Shield <= 0))
                    continue;
                yield return new LoseBlockShieldAction(
                    enemy,
                    base.Value1,
                    base.Value1,
                    false
                );
            }
            yield break;
        }
    }

    // ------------------------------------------------------------------ 要石风暴
    public sealed class TianziKeystoneStormDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Rock;
            config.GunNameBurst = GunNameID.Rock;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 2, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;

            config.Damage = 8;
            config.UpgradedDamage = 11;

            config.Value1 = 5; // 临时生命值门槛

            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(TianziTempHpSe) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 要石风暴：对所有敌人造成 {Damage} 点伤害。
    /// 若{PlayerName}的临时生命值不少于 {Value1}，再造成一次。
    /// </summary>
    [EntityLogic(typeof(TianziKeystoneStormDef))]
    public sealed class TianziKeystoneStorm : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, 1, false);
            yield return base.AttackAllAliveEnemyAction();

            if (TianziTempHp.Get(base.Battle.Player) >= base.Value1)
                yield return base.AttackAllAliveEnemyAction();
            yield break;
        }
    }

    // ------------------------------------------------------------------ 绯想断罪
    public sealed class TianziScarletExecutionDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.RedAura;
            config.GunNameBurst = GunNameID.RedAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 14;
            config.UpgradedDamage = 18;

            config.Value1 = 3; // 击杀获得的 p点

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>绯想断罪：造成 {Damage} 点伤害。若此伤害击杀了敌人，获得 {Value1} 点 p点。</summary>
    [EntityLogic(typeof(TianziScarletExecutionDef))]
    public sealed class TianziScarletExecution : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            List<Unit> targets = new List<Unit>(selector.GetUnits(base.Battle));
            yield return base.AttackAction(selector);

            bool killed = false;
            foreach (Unit enemy in targets)
            {
                if (enemy != null && enemy.IsNotAlive)
                    killed = true;
            }
            if (killed)
                yield return new GainPowerAction(base.Value1);
            yield break;
        }
    }

    // ------------------------------------------------------------------ 天人降临
    public sealed class TianziHeavenDescentDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Heaven;
            config.GunNameBurst = GunNameID.Heaven;

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, White = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 10;
            config.UpgradedDamage = 13;

            config.Value1 = 1; // 天衣无缝回合

            config.RelativeEffects = new List<string>() { nameof(Invincible) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Invincible) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>天人降临：获得 {Value1} 回合天衣无缝，造成 {Damage} 点伤害。</summary>
    [EntityLogic(typeof(TianziHeavenDescentDef))]
    public sealed class TianziHeavenDescent : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<Invincible>(1, base.Value1, 0, 0, 0.2f);
            yield return base.AttackAction(selector);
            yield break;
        }
    }
}
