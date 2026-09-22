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

    public sealed class TianziScarletApocalypseDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7160);
            config.GunNameBurst = GunNameID.GetGunFromId(7160);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Red = 3 };
            config.UpgradedCost = new ManaGroup() { Red = 2 };
            config.Rarity = Rarity.Rare;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 15;
            config.UpgradedDamage = 20;
            config.Value1 = 1;
            config.Keywords = Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Accuracy;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziWeatherForecastSe),
                nameof(TianziWeatherClear),
                nameof(TianziWeatherMist),
                nameof(TianziWeatherCloud),
                nameof(TianziWeatherAzure),
                nameof(TianziWeatherHail),
                nameof(TianziWeatherFog),
                nameof(TianziWeatherTyphoon),
                nameof(TianziWeatherCalm),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziScarletApocalypseDef))]
    public sealed class TianziScarletApocalypse : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            bool hasWeather = false;
            foreach (StatusEffect se in base.Battle.Player.StatusEffects)
            {
                if (se is TianziWeatherSeBase)
                {
                    hasWeather = true;
                    break;
                }
            }

            float dmg = base.Damage.Damage;
            if (hasWeather)
                dmg *= 1.5f;
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                yield return new DamageAction(
                    base.Battle.Player, enemy, DamageInfo.Attack(dmg, true), base.GunName, GunType.Single);
            }
            yield return BuffAction<TianziWeatherForecastSe>(0, base.Value1, 0, 0, 0.2f);
        }
    }
}
