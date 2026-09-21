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
    public sealed class TianziPeachReserveDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 0 };
            config.Rarity = Rarity.Common;
            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;
            config.Value1 = 3;
            config.UpgradedValue1 = 2;
            config.Value2 = 2;
            config.UpgradedValue2 = 3;
            config.Keywords = Keyword.Exile | Keyword.Retain;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Retain;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziPeachReserveDef))]
    public sealed class TianziPeachReserve : TianziCard
    {
        private static bool _handledStart;
        private static bool _handledEnd;

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            _handledStart = false;
            _handledEnd = false;
            base.HandleBattleEvent<UnitEventArgs>(
                battle.Player.TurnStarting,
                new GameEventHandler<UnitEventArgs>(this.OnTurnStarting));
        }

        private void OnTurnStarting(UnitEventArgs args)
        {
            _handledStart = false;
            _handledEnd = false;
        }

        public override IEnumerable<BattleAction> OnTurnStartedInHand()
        {
            if (_handledStart)
                yield break;
            _handledStart = true;
            int lost = TianziTempHp.LostLastTurn;
            int threshold = base.Value1 > 0 ? base.Value1 : 1;
            int times = lost / threshold;
            if (times > 2)
                times = 2;
            for (int i = 0; i < times; i++)
            {
                yield return new CastBlockShieldAction(
                    base.Battle.Player, base.Battle.Player, base.Value2, 0, BlockShieldType.Direct, false);
                yield return new DrawManyCardAction(1);
            }
        }

        public override IEnumerable<BattleAction> OnTurnEndingInHand()
        {
            if (_handledEnd)
                yield break;
            _handledEnd = true;
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, 2, 0.1f);
            if (gain != null)
                yield return gain;
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield break;
        }
    }
}
