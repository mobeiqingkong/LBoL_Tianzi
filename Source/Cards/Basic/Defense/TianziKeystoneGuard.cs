using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    public sealed class TianziKeystoneGuardDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;
            config.IsPooled = false;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 10;
            config.UpgradedBlock = 13;

            config.Keywords = Keyword.Basic;
            config.UpgradedKeywords = Keyword.Basic;

            config.Illustrator = "あめにん";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>要石护盾（初始牌）</summary>
    [EntityLogic(typeof(TianziKeystoneGuardDef))]
    public sealed class TianziKeystoneGuard : TianziCard { }
}
