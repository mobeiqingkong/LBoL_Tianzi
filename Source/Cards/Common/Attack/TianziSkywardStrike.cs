using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;

namespace TianziMod.Cards
{
    public sealed class TianziSkywardStrikeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(39073);
            config.GunNameBurst = GunNameID.GetGunFromId(39073);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 12;
            config.UpgradedDamage = 12;

            config.Value1 = 2;
            config.UpgradedValue1 = 3;

            config.Illustrator = "racer";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    /// <summary>
    /// 天地之压：造成 {Damage} 点伤害。
    /// 本回合每打出一张防御牌，伤害提高 {Value1} 点（加成会反映在卡面伤害上）。
    /// </summary>
    [EntityLogic(typeof(TianziSkywardStrikeDef))]
    public sealed class TianziSkywardStrike : TianziCard
    {
        protected override int AdditionalDamage
        {
            get { return this.CountTurnPlayed(CardType.Defense) * base.Value1; }
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            this.HandleBattleEvent<CardUsingEventArgs>(
                battle.CardUsed,
                new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
            this.HandleBattleEvent<CardUsingEventArgs>(
                battle.CardPlayed,
                new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
            this.HandleBattleEvent<UnitEventArgs>(
                battle.Player.TurnStarting,
                new GameEventHandler<UnitEventArgs>(this.OnTurnStarted));
            this.HandleBattleEvent<UnitEventArgs>(
                battle.Player.TurnStarted,
                new GameEventHandler<UnitEventArgs>(this.OnTurnStarted));
        }

        private void OnCardUsed(CardUsingEventArgs args)
        {
            if (args.Card != null && args.Card.CardType == CardType.Defense)
                this.NotifyChanged();
        }

        private void OnTurnStarted(UnitEventArgs args)
        {
            this.NotifyChanged();
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                yield return new DamageAction(
                    base.Battle.Player,
                    enemy,
                    base.Damage,
                    base.GunName,
                    GunType.Single
                );
            }
        }
    }
}
