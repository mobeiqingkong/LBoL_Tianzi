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

namespace TianziMod.Cards
{
    public sealed class TianziDoubleSlashDef : TianziCardTemplate
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

            config.Damage = 4;
            config.UpgradedDamage = 6;

            config.Value1 = 2; // 攻击段数

            config.RelativeCards = new List<string>() { nameof(TianziSecondSlash) };
            config.UpgradedRelativeCards = new List<string>() { nameof(TianziSecondSlash) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>双斩：造成 {Damage} 点伤害 {Value1} 次。将「再斩」置入手中。</summary>
    [EntityLogic(typeof(TianziDoubleSlashDef))]
    public sealed class TianziDoubleSlash : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            base.CardGuns = new Guns(base.GunName, base.Value1, true);
            foreach (GunPair gunPair in base.CardGuns.GunPairs)
            {
                yield return base.AttackAction(selector, gunPair);
            }
            yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<TianziSecondSlash>() });
            yield break;
        }
    }
}
