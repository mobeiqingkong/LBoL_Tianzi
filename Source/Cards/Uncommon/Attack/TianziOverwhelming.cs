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

    public sealed class TianziOverwhelmingDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(4534);
            config.GunNameBurst = GunNameID.GetGunFromId(4534);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 8;
            config.UpgradedDamage = 11;
            config.Value1 = 2;
            config.RelativeEffects = new List<string>() { nameof(Vulnerable) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block | Keyword.Shield;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Shield;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziOverwhelmingDef))]
    public sealed class TianziOverwhelming : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            List<Unit> targets = new List<Unit>(selector.GetUnits(base.Battle));
            bool[] wasGuarded = new bool[targets.Count];
            for (int i = 0; i < targets.Count; i++)
                wasGuarded[i] = targets[i].Block > 0 || targets[i].Shield > 0;
            yield return base.AttackAction(selector);
            for (int i = 0; i < targets.Count; i++)
            {
                Unit enemy = targets[i];
                if (!enemy.IsAlive || !wasGuarded[i])
                    continue;
                if (enemy.Block <= 0 && enemy.Shield <= 0)
                    yield return base.DebuffAction<Vulnerable>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
            }
        }
    }
}
