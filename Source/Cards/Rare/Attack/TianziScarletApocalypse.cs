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
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    public sealed class TianziScarletApocalypseDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7160);
            config.GunNameBurst = GunNameID.GetGunFromId(7160);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 2 };
            config.UpgradedCost = new ManaGroup() { Red = 1, Any = 1};
            config.Rarity = Rarity.Rare;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 16;
            config.UpgradedDamage = 24;
            config.Value1 = 1;
            config.Keywords = Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Accuracy;
            config.RelativeEffects = TianziWeather.RelativeIds(includeForecast: true);
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziScarletApocalypseDef))]
    public sealed class TianziScarletApocalypse : TianziCard
    {
        private bool HasWeather
        {
            get
            {
                if (base.Battle == null || base.Battle.Player == null)
                    return false;
                foreach (StatusEffect se in base.Battle.Player.StatusEffects)
                {
                    if (se is TianziWeatherSeBase)
                        return true;
                }
                return false;
            }
        }

        /// <summary>有天气时伤害 ×1.5，加成反映在卡面 {Damage} 上。</summary>
        protected override int AdditionalDamage
        {
            get
            {
                if (!this.HasWeather)
                    return 0;
                return (int)Math.Round((base.ConfigDamage + base.DeltaDamage) * 0.5f, MidpointRounding.AwayFromZero);
            }
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            this.HandleBattleEvent<StatusEffectApplyEventArgs>(
                battle.Player.StatusEffectAdded,
                new GameEventHandler<StatusEffectApplyEventArgs>(this.OnSeChanged));
            this.HandleBattleEvent<StatusEffectEventArgs>(
                battle.Player.StatusEffectRemoved,
                new GameEventHandler<StatusEffectEventArgs>(this.OnSeRemoved));
        }

        private void OnSeChanged(StatusEffectApplyEventArgs args)
        {
            if (args.Effect is TianziWeatherSeBase)
                this.NotifyChanged();
        }

        private void OnSeRemoved(StatusEffectEventArgs args)
        {
            if (args.Effect is TianziWeatherSeBase)
                this.NotifyChanged();
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.AttackAction(selector);
            yield return BuffAction<TianziWeatherForecastSe>(0, base.Value1, 0, 0, 0.2f);
        }
    }
}
