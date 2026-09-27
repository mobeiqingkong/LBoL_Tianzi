using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziIkuFriendDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red, ManaColor.Blue };
            config.Cost = new ManaGroup() { Red = 1, Blue = 1 };
            config.UpgradedCost = ManaGroup.Hybrids(1, ManaColor.Red, ManaColor.Blue);
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Friend;
            config.TargetType = TargetType.Self;
            config.Loyalty = 3;
            config.UpgradedLoyalty = 3;
            config.PassiveCost = 1;
            config.UpgradedPassiveCost = 1;
            config.ActiveCost = -8;
            config.UpgradedActiveCost = -8;
            // 不可设正数 UltimateCost：MinActiveCost 取 Max 后会把主动门槛算成负数，3 点也能放主动
            config.Value1 = 4;
            config.Value2 = 3;
            config.Scry = 4;
            config.UpgradedScry = 4;
            config.RelativeEffects = new List<string>() { nameof(TempElectric), nameof(Invincible) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Scry;
            config.UpgradedRelativeKeyword = Keyword.Scry;
            config.Illustrator = "わんこソラ";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziIkuFriendDef))]
    public sealed class TianziIkuFriend : TianziCard
    {
        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            // TempElectric 会在 TurnStarted 移除；被动电击必须在那之后再挂
            base.ReactBattleEvent<UnitEventArgs>(
                battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted),
                GameEventPriority.Lowest);
        }

        public override IEnumerable<BattleAction> OnTurnStartedInHand()
        {
            return this.GetPassiveActions();
        }

        public override IEnumerable<BattleAction> GetPassiveActions()
        {
            if (!base.Summoned || base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            base.Loyalty += base.PassiveCost;
            yield return new ScryAction(base.Scry);
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            if (!base.Summoned || base.Zone != CardZone.Hand || base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return BuffAction<TempElectric>(base.Value2, 0, 0, 0, 0.2f);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            base.Loyalty += base.ActiveCost;
            yield return BuffAction<Invincible>(1, 1, 0, 0, 0.2f);
        }
    }
}
