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
    public sealed class TianziSkywardStrikeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(39073);
            config.GunNameBurst = GunNameID.GetGunFromId(39073);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 12;
            config.UpgradedDamage = 12;

            config.Value1 = 2; // 每张防御牌提供的追加伤害
            config.UpgradedValue1 = 3;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 冲天一击：造成 {Damage} 点伤害。
    /// 本回合每打出过一张防御牌，此牌伤害提高 {Value1} 点。
    /// </summary>
    [EntityLogic(typeof(TianziSkywardStrikeDef))]
    public sealed class TianziSkywardStrike : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            int defenseCount = this.CountTurnPlayed(CardType.Defense);
            float damage = base.Damage.Damage + defenseCount * base.Value1;

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
