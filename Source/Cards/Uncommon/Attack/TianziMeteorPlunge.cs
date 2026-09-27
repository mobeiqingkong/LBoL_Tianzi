using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziMeteorPlungeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(23072);
            config.GunNameBurst = GunNameID.GetGunFromId(23072);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 2, Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;
            config.Damage = 17;
            config.UpgradedDamage = 21;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "蓬莱雾理";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziMeteorPlungeDef))]
    public sealed class TianziMeteorPlunge : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            int bonus = TianziTempHp.Get(base.Battle.Player) * 2;
            yield return base.AttackAllAliveEnemyAction();
            if (bonus <= 0 || base.Battle.BattleShouldEnd)
                yield break;

            // 第二段用固定 Attack 伤害；不走本卡 Damage，避免吃两次火力/加成
            yield return new DamageAction(
                base.Battle.Player,
                base.Battle.AllAliveEnemies,
                DamageInfo.Attack(bonus, base.IsAccuracy),
                base.GunName,
                GunType.Single);
        }
    }
}
