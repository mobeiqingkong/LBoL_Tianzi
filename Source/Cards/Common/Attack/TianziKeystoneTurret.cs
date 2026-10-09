using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using System.Collections.Generic;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziKeystoneTurretDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(4122);
            config.GunNameBurst = GunNameID.GetGunFromId(4122);

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.AllEnemies;

            config.Damage = 3;
            config.UpgradedDamage = 4;

            config.Value1 = 5; // 绝壁
            config.Value2 = 3; // 攻击段数

            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = new List<string>() { nameof(TianziTempHpSe) };

            // 精准 = 无视闪避
            config.Keywords = Keyword.None;
            config.UpgradedKeywords = Keyword.Accuracy;

            config.Illustrator = "タツ";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>要石浮游炮：造成 {Damage} 点伤害 {Value2} 次。（精准）</summary>
    [EntityLogic(typeof(TianziKeystoneTurretDef))]
    public sealed class TianziKeystoneTurret : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, base.Value1);
            if (gain != null) yield return gain;
            base.CardGuns = new Guns(base.GunName, base.Value2, true);
            foreach (GunPair gunPair in base.CardGuns.GunPairs)
            {
                yield return base.AttackAction(selector, gunPair);
            }
            yield break;
        }
    }
}
