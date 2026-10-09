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

    public sealed class TianziGrowingBraverDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7040);
            config.GunNameBurst = GunNameID.GetGunFromId(7040);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 8;
            config.UpgradedDamage = 12;
            config.Value1 = 1;
            config.UpgradedValue1 = 2;
            config.RelativeEffects = new List<string>() { nameof(Firepower) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "春藤平四郎";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziGrowingBraverDef))]
    public sealed class TianziGrowingBraver : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            Dictionary<Unit, int> hp = new Dictionary<Unit, int>();
            foreach (Unit enemy in selector.GetUnits(base.Battle))
                hp[enemy] = enemy.Hp;
            yield return base.AttackAction(selector);
            foreach (KeyValuePair<Unit, int> pair in hp)
            {
                if (pair.Key != null && pair.Key.IsAlive && pair.Key.Hp < pair.Value)
                    yield return BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
            }
        }
    }
}
