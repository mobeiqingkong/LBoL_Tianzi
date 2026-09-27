using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziKeystoneStormDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(39050);
            config.GunNameBurst = GunNameID.GetGunFromId(39050);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 2, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;
            config.Damage = 7;
            config.UpgradedDamage = 10;
            config.Value1 = 7;
            config.Value2 = 3;
            config.UpgradedValue2 = 5;
            config.RelativeEffects = new List<string>() { nameof(Weak) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "kaden";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziKeystoneStormDef))]
    public sealed class TianziKeystoneStorm : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            base.CardGuns = new Guns(base.GunName, 1, false);
            yield return base.AttackAllAliveEnemyAction();
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;
                yield return base.DebuffAction<Weak>(enemy, 1, 1, 0, 0, true, 0.2f);
            }
            int block = base.Value1;
            if (base.Battle.HandZone.Count >= 3)
                block += base.Value2;
            yield return new CastBlockShieldAction(
                base.Battle.Player, base.Battle.Player, block, 0, BlockShieldType.Direct, false);
        }
    }
}
