using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;

namespace TianziMod.Cards
{
    public sealed class TianziScarletSparkDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Light;
            config.GunNameBurst = GunNameID.Light;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 4;
            config.UpgradedDamage = 5;

            config.Value1 = 4; // 目标有易伤时的追加伤害
            config.UpgradedValue1 = 5;

            config.RelativeEffects = new List<string>() { nameof(Vulnerable) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Vulnerable) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 绯色余烬：造成 {Damage} 点伤害。
    /// 若目标处于易伤状态，额外造成 {Value1} 点伤害。
    /// </summary>
    [EntityLogic(typeof(TianziScarletSparkDef))]
    public sealed class TianziScarletSpark : TianziCard
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
                if (enemy.GetStatusEffect<Vulnerable>() != null)
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
