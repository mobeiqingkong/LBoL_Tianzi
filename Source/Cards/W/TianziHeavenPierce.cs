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

namespace TianziMod.Cards
{
    public sealed class TianziHeavenPierceDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Heaven;
            config.GunNameBurst = GunNameID.Heaven;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 9;
            config.UpgradedDamage = 13;

            config.Value1 = 5;
            config.UpgradedValue1 = 7;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天穿：造成 {Damage} 点伤害。
    /// 若目标拥有格挡或护盾，伤害提高 {Value1} 点。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenPierceDef))]
    public sealed class TianziHeavenPierce : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                float damage = base.Damage.Damage;
                if (enemy.Block > 0 || enemy.Shield > 0)
                    damage += base.Value1;

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
