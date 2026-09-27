using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;

namespace TianziMod.Cards
{
    public sealed class TianziScarletSlashDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.IsPooled = false;
            config.Damage = 10;
            config.UpgradedDamage = 14;
            config.GunName = GunNameID.GetGunFromId(6162);
            config.GunNameBurst = GunNameID.GetGunFromId(6162);

            config.Keywords = Keyword.Basic;
            config.UpgradedKeywords = Keyword.Basic;

            config.Illustrator = "鶴亀";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>绯想剑斩击（初始牌 B 组）</summary>
    [EntityLogic(typeof(TianziScarletSlashDef))]
    public sealed class TianziScarletSlash : TianziCard { }
}
