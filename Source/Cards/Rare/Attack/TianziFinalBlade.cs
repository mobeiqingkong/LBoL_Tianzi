using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziFinalBladeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(4610);
            config.GunNameBurst = GunNameID.GetGunFromId(4610);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 9;
            config.UpgradedDamage = 12;

            config.Value1 = 2;
            config.UpgradedValue1 = 3;

            config.Keywords = Keyword.Exile | Keyword.Retain | Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain | Keyword.Accuracy;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 终绝的一剑：造成 {Damage} 点伤害。
    /// 目标每失去 4% 生命值，此牌伤害额外提高目标最大生命值 {Value1}% 的伤害。（放逐 / 保留）
    /// </summary>
    [EntityLogic(typeof(TianziFinalBladeDef))]
    public sealed class TianziFinalBlade : TianziCard
    {
        protected override int AdditionalDamage
        {
            get
            {
                Unit target = base.PendingTarget;
                if (target == null || !target.IsAlive || target.MaxHp <= 0)
                    return 0;
                float lostRatio = 1f - (float)target.Hp / target.MaxHp;
                int steps = (int)Math.Floor(lostRatio * 100f / 4f);
                if (steps <= 0)
                    return 0;
                return (int)Math.Round(steps * target.MaxHp * base.Value1 / 100f, MidpointRounding.AwayFromZero);
            }
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);
        }
    }
}
