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

    public sealed class TianziLandLiquefyDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(4713);
            config.GunNameBurst = GunNameID.GetGunFromId(4713);
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 2 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;
            config.Damage = 16;
            config.UpgradedDamage = 20;
            config.Value1 = 5;
            config.UpgradedValue1 = 7;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block | Keyword.Shield;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Shield;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziLandLiquefyDef))]
    public sealed class TianziLandLiquefy : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            base.CardGuns = new Guns(base.GunName, 1, false);
            yield return base.AttackAllAliveEnemyAction();
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || (enemy.Block <= 0 && enemy.Shield <= 0))
                    continue;
                yield return new LoseBlockShieldAction(enemy, base.Value1, base.Value1, false);
            }
        }
    }
}
