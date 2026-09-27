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

    // ------------------------------------------------------------------ 先忧后乐之剑
    public sealed class TianziHardshipFirstDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(510);
            config.GunNameBurst = GunNameID.GetGunFromId(510);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 12;
            config.UpgradedDamage = 14;
            config.RelativeEffects = new List<string>() { nameof(TianziParityKwSe), nameof(TianziHardshipRetainSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 先忧后乐之剑：造成 {Damage} 点伤害。
    /// 手牌张数为奇数时，打出后置于抽牌堆顶；
    /// 为偶数时，本回合结束前再让当时的手牌暂留。
    /// </summary>
    [EntityLogic(typeof(TianziHardshipFirstDef))]
    public sealed class TianziHardshipFirst : TianziCard
    {
        protected override bool HasParityKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);
            foreach (BattleAction action in TianziParityPlay.Resolve(this, this.OddBranch(), this.EvenBranch()))
                yield return action;
        }

        private IEnumerable<BattleAction> OddBranch()
        {
            yield return new MoveCardToDrawZoneAction(this, DrawZoneTarget.Top);
        }

        private IEnumerable<BattleAction> EvenBranch()
        {
            // 先挂标记，等本回合 TurnEnding（弃牌之前）再给当时的手牌上暂留。
            yield return BuffAction<TianziHardshipRetainSe>(0, 0, 0, 0, 0.1f);
        }
    }
}
