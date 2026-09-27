using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziEarthStoneDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Green };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Value1 = 1;
            config.UpgradedValue1 = 2;
            config.Keywords = Keyword.Replenish;
            config.UpgradedKeywords = Keyword.Replenish;
            config.RelativeEffects = new List<string>() { nameof(TianziRegenSe), nameof(TianziEarthDelaySe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Exile;
            config.UpgradedRelativeKeyword = Keyword.Exile;
            config.Illustrator = "ボキ ★シモ一ル";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 进手即放逐并挂延迟自愈。参考药水 / 森林探险：OnDraw + OnMove(→Hand) + 开战已在手。
    /// </summary>
    [EntityLogic(typeof(TianziEarthStoneDef))]
    public sealed class TianziEarthStone : TianziCard
    {
        public override IEnumerable<BattleAction> OnDraw()
        {
            return this.EnterHandReactor();
        }

        public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
        {
            if (dstZone != CardZone.Hand)
                return null;
            return this.EnterHandReactor();
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            // 开战已在手：不能同步 React，延后到可解析动作时
            if (base.Zone == CardZone.Hand)
                this.React((LazySequencedReactor)this.EnterHandLazy);
        }

        private IEnumerable<BattleAction> EnterHandLazy()
        {
            return this.EnterHandReactor();
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            return this.EnterHandReactor(ensureInHand: false);
        }

        private IEnumerable<BattleAction> EnterHandReactor(bool ensureInHand = true)
        {
            if (base.Battle == null || base.Battle.BattleShouldEnd)
                yield break;
            if (ensureInHand && base.Zone != CardZone.Hand)
                yield break;
            base.NotifyActivating();
            yield return new ExileCardAction(this);
            // Level=自愈层数；Count=后续回合数
            yield return BuffAction<TianziEarthDelaySe>(2, 0, 0, base.Value1, 0.2f);
        }
    }
}
