using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using UnityEngine;

namespace TianziMod.Cards
{
    public sealed class TianziScarletFlameWheelDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.RedAura;
            config.GunNameBurst = GunNameID.RedAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 6;
            config.UpgradedDamage = 9;

            config.Value1 = 2; // 攻击段数

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 绯想焰轮：造成 {Damage} 点伤害 {Value1} 次。
    /// 结算后再随机对一名敌人造成一次等量伤害。
    /// </summary>
    [EntityLogic(typeof(TianziScarletFlameWheelDef))]
    public sealed class TianziScarletFlameWheel : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, base.Value1, true);
            foreach (GunPair gunPair in base.CardGuns.GunPairs)
            {
                yield return base.AttackAction(selector, gunPair);
            }

            // 随机追加一击
            List<EnemyUnit> alive = new List<EnemyUnit>(base.Battle.AllAliveEnemies);
            if (alive.Count > 0)
            {
                // EnemyUnit extra = alive[Random.Range(0, alive.Count)];
                // yield return new DamageAction(
                //     base.Battle.Player,
                //     extra,
                //     base.Damage,
                //     base.GunName,
                //     GunType.Single
                // );
            }
            yield break;
        }
    }
}
