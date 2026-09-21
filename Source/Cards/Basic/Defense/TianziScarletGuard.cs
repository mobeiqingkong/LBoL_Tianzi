using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    public sealed class TianziScarletGuardDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 10;
            config.UpgradedBlock = 13;
            config.IsPooled = false;
            config.Keywords = Keyword.Basic;
            config.UpgradedKeywords = Keyword.Basic;

            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>绯想剑格挡（初始牌 B 组）</summary>
    [EntityLogic(typeof(TianziScarletGuardDef))]
    public sealed class TianziScarletGuard : TianziCard { }
}
