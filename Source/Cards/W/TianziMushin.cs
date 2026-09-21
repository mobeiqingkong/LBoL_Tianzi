using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using UnityEngine;

namespace TianziMod.Cards
{
    public sealed class TianziMushinDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 8;
            config.UpgradedBlock = 12;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 无念无想：获得 {Block} 点格挡。
    /// 从抽牌堆随机打出一张防御牌。
    /// </summary>
    [EntityLogic(typeof(TianziMushinDef))]
    public sealed class TianziMushin : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);
            // 官方 PlayDrawTop / TheFool 的写法：产生动作后先检查战斗是否已结束，
            // 否则可能在战斗收尾阶段继续往动作队列里塞「打牌」动作 -> 队列错乱。
            if (base.Battle.BattleShouldEnd)
                yield break;

            List<Card> defenses = new List<Card>();
            foreach (Card c in base.Battle.DrawZone)
            {
                if (c != null && c.CardType == CardType.Defense)
                    defenses.Add(c);
            }
            if (defenses.Count == 0)
                yield break;

            Card pick = defenses[Random.Range(0, defenses.Count)];
            yield return new PlayCardAction(pick);
            yield break;
        }
    }
}
