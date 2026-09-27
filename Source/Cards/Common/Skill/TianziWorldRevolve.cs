using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;

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
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 3; // 张数

            config.RelativeEffects = new List<string>() { nameof(TianziMod.StatusEffects.TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

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
        protected override bool HasParityKeyword
        {
            get { return true; }
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            foreach (BattleAction action in TianziParityPlay.Resolve(this, this.OddBranch(), this.EvenBranch()))
                yield return action;
        }

        private IEnumerable<BattleAction> OddBranch()
        {
            yield return new DrawManyCardAction(base.Value1);
        }

        private IEnumerable<BattleAction> EvenBranch()
        {
            List<Card> pool = new List<Card>(base.Battle.DiscardZone);
            if (pool.Count == 0)
                yield break;
            int take = base.Value1 < pool.Count ? base.Value1 : pool.Count;
            foreach (Card card in pool.GetRange(0, take))
            {
                if (card != null && card.Zone == CardZone.Discard)
                    yield return new MoveCardAction(card, CardZone.Hand);
            }
        }
    }
}
