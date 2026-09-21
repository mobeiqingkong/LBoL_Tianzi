using System.Collections.Generic;
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

namespace TianziMod.Cards
{
    public sealed class TianziSecondSlashDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;

            config.GunName = GunNameID.GetGunFromId(6162);
            config.GunNameBurst = GunNameID.GetGunFromId(6162);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 0 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 6;
            config.UpgradedDamage = 9;

            config.Keywords = Keyword.Exile | Keyword.Retain;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;
            config.RelativeKeyword = Keyword.Block | Keyword.Shield;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Shield;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 再斩（衍生牌）：造成 {Damage} 点伤害。
    /// 若目标拥有格挡或护盾，则伤害翻倍。
    /// </summary>
    [EntityLogic(typeof(TianziSecondSlashDef))]
    public sealed class TianziSecondSlash : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                bool guarded = enemy.Block > 0 || enemy.Shield > 0;
                DamageInfo info = guarded
                    ? DamageInfo.Attack(base.Damage.Damage * 2f, false)
                    : DamageInfo.Attack(base.Damage.Damage, false);

                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    info,
                    base.GunName,
                    GunType.Single
                );
            }
            yield break;
        }
    }
}
