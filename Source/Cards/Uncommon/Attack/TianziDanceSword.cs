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

    public sealed class TianziDanceSwordDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7301);
            config.GunNameBurst = GunNameID.GetGunFromId(7301);
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 0 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 6;
            config.UpgradedDamage = 9;
            config.Value1 = 1;
            config.Value2 = 1;
            config.UpgradedValue2 = 2;
            config.RelativeEffects = new List<string>() { nameof(Weak), nameof(TianziNextTurnPhilSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziDanceSwordDef))]
    public sealed class TianziDanceSword : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            bool paid = base.Battle.BattleMana.White >= 1;
            if (paid)
                yield return new LoseManaAction(new ManaGroup() { White = 1 });
            yield return base.AttackAction(selector);
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return base.DebuffAction<Weak>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
            }
            if (paid)
            {
                yield return new ApplyStatusEffectAction<TianziNextTurnPhilSe>(
                    base.Battle.Player, null, null, base.Value2, null, 0.1f);
            }
            yield return new DrawManyCardAction(1);
        }
    }
}
