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
using TianziMod.Keywords;
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
            config.GunName = GunNameID.GetGunFromId(4610);
            config.GunNameBurst = GunNameID.GetGunFromId(4610);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 9;
            config.UpgradedDamage = 12;

            config.Value1 = 2; // 每失去 4% 生命，额外 + 目标最大生命 Value1%
            config.UpgradedValue1 = 3;

            config.Keywords = Keyword.Exile | Keyword.Retain | Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain | Keyword.Accuracy;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 终绝的一剑：造成 {Damage} 点伤害。
    /// 目标每失去 4% 生命值，此牌伤害额外提高目标最大生命值 {Value1}% 的伤害。（放逐 / 保留）
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
            float baseDamage = base.Damage.Damage;
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                int steps = 0;
                if (enemy.MaxHp > 0)
                {
                    float lostRatio = 1f - (float)enemy.Hp / enemy.MaxHp;
                    steps = (int)Math.Floor(lostRatio * 100f / 4f);
                }
                float extra = steps * enemy.MaxHp * base.Value1 / 100f;
                float damage = baseDamage + extra;

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
