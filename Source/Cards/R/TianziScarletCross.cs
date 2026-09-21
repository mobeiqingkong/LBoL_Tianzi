using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;

namespace TianziMod.Cards
{
    public sealed class TianziScarletCrossDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.VioletAura;
            config.GunNameBurst = GunNameID.VioletAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 6;
            config.UpgradedDamage = 8;

            config.RelativeEffects = new List<string>() { nameof(Vulnerable) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Vulnerable) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>绯色十字：造成 {Damage} 点伤害。若目标处于易伤状态，抽 1 张牌。</summary>
    [EntityLogic(typeof(TianziScarletCrossDef))]
    public sealed class TianziScarletCross : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            bool vulnerable = false;
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (enemy.GetStatusEffect<Vulnerable>() != null)
                    vulnerable = true;
            }

            yield return base.AttackAction(selector);

            if (vulnerable)
                yield return new DrawManyCardAction(1);
            yield break;
        }
    }
}
