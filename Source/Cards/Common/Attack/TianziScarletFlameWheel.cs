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
            config.GunName = GunNameID.GetGunFromId(7500);
            config.GunNameBurst = GunNameID.GetGunFromId(7500);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 8;
            config.UpgradedDamage = 12;

            config.Illustrator = "おもいか";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>绯焰射击：造成伤害后再对随机敌人造成同样伤害。</summary>
    [EntityLogic(typeof(TianziScarletFlameWheelDef))]
    public sealed class TianziScarletFlameWheel : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.AttackAction(selector);
            List<EnemyUnit> alive = new List<EnemyUnit>(base.Battle.AllAliveEnemies);
            if (alive.Count == 0)
                yield break;
            EnemyUnit extra = alive[Random.Range(0, alive.Count)];
            yield return new DamageAction(
                base.Battle.Player,
                extra,
                DamageInfo.Attack(base.Damage.Damage, false),
                base.GunName,
                GunType.Single);
        }
    }
}
