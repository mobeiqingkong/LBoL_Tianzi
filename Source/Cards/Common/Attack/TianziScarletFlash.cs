using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziScarletFlashDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(12021);
            config.GunNameBurst = GunNameID.GetGunFromId(12021);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 8;
            config.UpgradedDamage = 10;
            config.Value1 = 12;
            config.UpgradedValue1 = 16;
            config.Mana = new ManaGroup() { Red = 1 };

            config.RelativeEffects = new List<string>() { nameof(TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "しろもる";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>灼热气焰：奇数手牌时伤害提升；偶数获得 1 点红费。</summary>
    [EntityLogic(typeof(TianziScarletFlashDef))]
    public sealed class TianziScarletFlash : TianziCard
    {
        protected override bool HasParityKeyword
        {
            get { return true; }
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            foreach (BattleAction action in TianziParityPlay.Resolve(this, this.OddBranch(selector), this.EvenBranch(selector)))
                yield return action;
        }

        private IEnumerable<BattleAction> OddBranch(UnitSelector selector)
        {
            yield return new DamageAction(
                base.Battle.Player,
                selector.GetUnits(base.Battle),
                DamageInfo.Attack((float)base.Value1, false),
                base.GunName,
                GunType.Single);
        }

        private IEnumerable<BattleAction> EvenBranch(UnitSelector selector)
        {
            yield return base.AttackAction(selector);
            yield return new GainManaAction(new ManaGroup() { Red = 1 });
        }
    }
}
