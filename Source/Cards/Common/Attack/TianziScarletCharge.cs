using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziScarletChargeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(4121);
            config.GunNameBurst = GunNameID.GetGunFromId(4121);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 4;
            config.UpgradedDamage = 6;
            config.Value1 = 3;
            config.UpgradedValue1 = 4;

            config.RelativeEffects = new List<string>() { nameof(TianziScarletChargeSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>绯想之剑充能：造成 {Damage} 点伤害。下一张攻击牌伤害 +{Value1}。</summary>
    [EntityLogic(typeof(TianziScarletChargeDef))]
    public sealed class TianziScarletCharge : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.AttackAction(selector);
            yield return BuffAction<TianziScarletChargeSe>(base.Value1, 0, 0, 0, 0.2f);
        }
    }
}
