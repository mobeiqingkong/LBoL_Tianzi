using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    // ------------------------------------------------------------------ 虹彩气息
    public sealed class TianziRainbowAuraDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red, ManaColor.Blue };
            config.Cost = new ManaGroup() { White = 1, Red = 1, Blue = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Keywords =  Keyword.Exile;
            config.UpgradedKeywords = Keyword.Initial | Keyword.Replenish | Keyword.Exile;
            config.RelativeKeyword = Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Exile;

            config.Illustrator = "ｼﾞｮﾝﾃﾞｨｰ";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 虹彩气息：根据主角当前基础法力的颜色种类，每种获得 2 点。
    /// 升级后先放逐手中所有状态与厄运牌，再获得同样的法力。
    /// </summary>
    [EntityLogic(typeof(TianziRainbowAuraDef))]
    public sealed class TianziRainbowAura : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            ManaGroup pool = base.Battle.BaseTurnMana;
            ManaGroup gain = new ManaGroup()
            {
                White = pool.White > 0 ? 2 : 0,
                Blue = pool.Blue > 0 ? 2 : 0,
                Black = pool.Black > 0 ? 2 : 0,
                Red = pool.Red > 0 ? 2 : 0,
                Green = pool.Green > 0 ? 2 : 0,
                Colorless = pool.Colorless > 0 ? 2 : 0,
                Philosophy = pool.Philosophy > 0 ? 2 : 0,
            };
            if (gain.Amount > 0)
                yield return new GainManaAction(gain);
        }
    }
}
