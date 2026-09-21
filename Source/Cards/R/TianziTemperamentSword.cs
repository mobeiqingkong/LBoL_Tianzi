using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;

namespace TianziMod.Cards
{
    public sealed class TianziTemperamentSwordDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.VioletAura;
            config.GunNameBurst = GunNameID.VioletAura;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 9;
            config.UpgradedDamage = 12;

            config.Value1 = 1; // 负面状态回合数

            config.RelativeEffects = new List<string>()
            {
                nameof(Vulnerable),
                nameof(Fragil),
                nameof(Weak),
            };
            config.UpgradedRelativeEffects = new List<string>()
            {
                nameof(Vulnerable),
                nameof(Fragil),
                nameof(Weak),
            };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 气质之剑：造成 {Damage} 点伤害。
    /// 根据上一张打出的牌的类型，施加 {Value1} 回合 易伤 / 脆弱 / 虚弱。
    /// </summary>
    [EntityLogic(typeof(TianziTemperamentSwordDef))]
    public sealed class TianziTemperamentSword : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            Card previous = this.PreviousPlayedCard;

            yield return base.AttackAction(selector);

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;

                if (previous == null)
                    continue;

                switch (previous.CardType)
                {
                    case CardType.Attack:
                        yield return base.DebuffAction<Vulnerable>(
                            enemy, 1, base.Value1, 0, 0, true, 0.2f);
                        break;
                    case CardType.Defense:
                        yield return base.DebuffAction<Fragil>(
                            enemy, 1, base.Value1, 0, 0, true, 0.2f);
                        break;
                    default:
                        yield return base.DebuffAction<Weak>(
                            enemy, 1, base.Value1, 0, 0, true, 0.2f);
                        break;
                }
            }
            yield break;
        }
    }
}
