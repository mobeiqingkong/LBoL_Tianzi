using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;

namespace TianziMod.Cards
{
    public sealed class TianziKeystoneShotDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;
            config.IsPooled = false;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 10;
            config.UpgradedDamage = 14;
            config.GunName = GunNameID.GetGunFromId(1100);
            config.GunNameBurst = GunNameID.GetGunFromId(1100);

            config.Keywords = Keyword.Basic;
            config.UpgradedKeywords = Keyword.Basic;

            config.Illustrator = "画渣桃乐";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>要石射击（初始牌）</summary>
    [EntityLogic(typeof(TianziKeystoneShotDef))]
    public sealed class TianziKeystoneShot : TianziCard { }
}
