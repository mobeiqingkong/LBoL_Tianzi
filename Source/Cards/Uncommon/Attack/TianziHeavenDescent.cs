using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziHeavenDescentDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(11231);
            config.GunNameBurst = GunNameID.GetGunFromId(11231);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 2 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 14;
            config.UpgradedDamage = 16;
            config.Value1 = 5;
            config.UpgradedValue1 = 7;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe), nameof(TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Shield;
            config.UpgradedRelativeKeyword = Keyword.Shield;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziHeavenDescentDef))]
    public sealed class TianziHeavenDescent : TianziCard
    {
        protected override bool HasParityKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.AttackAction(selector);

            int consumed = TianziTempHp.ConsumeAll(base.Battle.Player);
            TianziTempHpSe leftover = base.Battle.Player.GetStatusEffect<TianziTempHpSe>();
            if (leftover != null)
                yield return new RemoveStatusEffectAction(leftover, true, 0.05f);

            if (consumed <= 0)
                yield break;

            foreach (BattleAction action in TianziParityPlay.Resolve(
                this, this.OddExtra(selector, consumed), this.EvenExtra(selector, consumed)))
                yield return action;
        }

        private IEnumerable<BattleAction> OddExtra(UnitSelector selector, int consumed)
        {
            // 消耗值×2，奇数时额外 +Value1
            int dmg = consumed * 2 + base.Value1;
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    DamageInfo.Attack(dmg, base.IsAccuracy),
                    base.GunName,
                    GunType.Single);
            }
        }

        private IEnumerable<BattleAction> EvenExtra(UnitSelector selector, int consumed)
        {
            int dmg = consumed * 2;
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    DamageInfo.Attack(dmg, base.IsAccuracy),
                    base.GunName,
                    GunType.Single);
            }
            yield return new CastBlockShieldAction(
                base.Battle.Player, base.Battle.Player, 0, consumed, BlockShieldType.Direct, false);
        }
    }
}
