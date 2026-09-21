using System.Collections.Generic;
using System.Linq;
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
using TianziMod.Keywords;
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
            config.Illustrator = "";
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
                    GunType.Single);
            }
        }
    }
}
