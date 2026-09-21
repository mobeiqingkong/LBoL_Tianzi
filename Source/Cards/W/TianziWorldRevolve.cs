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
    public sealed class TianziWorldRevolveDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 3; // 张数

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天地回转：手牌张数为奇数时，抽 {Value1} 张牌；
    /// 为偶数时，将弃牌堆的 {Value1} 张随机牌置入手中。
    /// </summary>
    [EntityLogic(typeof(TianziWorldRevolveDef))]
    public sealed class TianziWorldRevolve : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            if (base.Battle.HandZone.Count % 2 == 1)
            {
                yield return new DrawManyCardAction(base.Value1);
                yield break;
            }

            List<Card> pool = new List<Card>(base.Battle.DiscardZone);
            if (pool.Count == 0)
                yield break;

            // 洗牌后取前 Value1 张
            // for (int i = pool.Count - 1; i > 0; i--)
            // {
            //     int j = Random.Range(0, i + 1);
            //     Card tmp = pool[i];
            //     pool[i] = pool[j];
            //     pool[j] = tmp;
            // }
            int take = base.Value1 < pool.Count ? base.Value1 : pool.Count;

            // ⚠ 弃牌堆 -> 手牌 必须用 MoveCardAction。
            //   BattleController.AddCardToHand 开头就写死：
            //       if (card.Zone != CardZone.None) throw new InvalidOperationException(...)
            //   即 AddCardsToHandAction 只接受「刚创建、还没进任何牌区」的牌。
            //   传弃牌堆的牌会抛异常，而异常发生在 Battle.Resolve / Phase.Flow 的动作队列里，
            //   该动作会被截断、弃牌区的牌与 play-area 状态就此错乱，
            //   之后每一次出牌都在 CardUi.ConfirmUseCard 抛 NullReferenceException
            //   -> 从玩家角度看就是「一用就卡死」，而且是用过一次后所有牌都卡。
            //   官方同类卡（CirnoEcho / KoishiDive / PeaceEndTurn 等 12 处）全都用 MoveCardAction。
            foreach (Card card in pool.GetRange(0, take))
            {
                // 再按牌区守一次：MoveCardAction 的构造函数里直接调 MoveCardCheck，
                // dstZone == card.Zone 会当场 throw「Cannot move card ... to same zone」。
                if (card != null && card.Zone == CardZone.Discard)
                {
                    yield return new MoveCardAction(card, CardZone.Hand);
                }
            }
            yield break;
        }
    }
}
