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
    public sealed class TianziSkyCleaveDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.Slash;
            config.GunNameBurst = GunNameID.Slash;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 7;
            config.UpgradedDamage = 8;

            config.Value1 = 1; // 虚弱回合数
            config.UpgradedValue1 = 2;
            config.Value2 = 0; // 脆弱回合数（仅升级后）
            config.UpgradedValue2 = 1;

            config.RelativeEffects = new List<string>() { nameof(Weak), nameof(Fragil) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(Weak), nameof(Fragil) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天穹斩：造成 {Damage} 点伤害 2 次。
    /// 施加 {Value1} 回合虚弱（升级后追加 {Value2} 回合脆弱）。
    /// </summary>
    [EntityLogic(typeof(TianziSkyCleaveDef))]
    public sealed class TianziSkyCleave : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, 2, true);
            foreach (GunPair gunPair in base.CardGuns.GunPairs)
            {
                yield return base.AttackAction(selector, gunPair);
            }

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return base.DebuffAction<Weak>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
                if (base.Value2 > 0)
                {
                    yield return base.DebuffAction<Fragil>(enemy, 1, base.Value2, 0, 0, true, 0.2f);
                }
            }
            yield break;
        }
    }
}
