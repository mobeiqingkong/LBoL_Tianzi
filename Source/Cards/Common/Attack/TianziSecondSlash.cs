using System.Collections.Generic;
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
            config.Cost = ManaGroup.Empty;
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
    /// ????? {Damage} ????
    /// ???????????????????????????
    /// </summary>
    [EntityLogic(typeof(TianziSecondSlashDef))]
    public sealed class TianziSecondSlash : TianziCard
    {
        protected override int AdditionalDamage
        {
            get
            {
                Unit target = base.PendingTarget;
                if (target == null || !target.IsAlive)
                    return 0;
                if (target.Block <= 0 && target.Shield <= 0)
                    return 0;
                return base.ConfigDamage + base.DeltaDamage;
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
