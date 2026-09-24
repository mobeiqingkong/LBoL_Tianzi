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

    // -------------------------------------------------- 非想「非想非非想之剑」
    public sealed class TianziUnthinkableBladeDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7160);
            config.GunNameBurst = GunNameID.GetGunFromId(7160);

            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 3, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2, Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 18;
            config.UpgradedDamage = 24;

            config.Value1 = 2;
            config.UpgradedValue1 = 3;
            config.Mana = new ManaGroup() { Any = 1 };

            config.Keywords = Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Accuracy;

            List<string> effects = new List<string>()
            {
                nameof(ExtraTurn),
                nameof(TimeIsLimited),
            };
            effects.AddRange(TianziWeather.RelativeIds(includeForecast: true));
            config.RelativeEffects = effects;
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 非想「非想非非想之剑」：造成 {Damage} 点伤害。
    /// 下回合随机释放一种天气并持续 {Value1} 回合。
    /// 结束{PlayerName}的回合，进行一个额外的回合，并获得 1 点|时间有限|。
    /// </summary>
    [EntityLogic(typeof(TianziUnthinkableBladeDef))]
    public sealed class TianziUnthinkableBlade : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            yield return BuffAction<TianziWeatherForecastSe>(0, base.Value1, 0, 0, 0.2f);

            yield return BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
            yield return base.DebuffAction<TimeIsLimited>(
                base.Battle.Player,
                1,
                0,
                0,
                0,
                true,
                0.2f
            );
            yield return new RequestEndPlayerTurnAction();
            yield break;
        }
    }
}
