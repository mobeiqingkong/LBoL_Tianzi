using System.Collections.Generic;
using System.Linq;
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
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziBlazingSwordDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(510);
            config.GunNameBurst = GunNameID.GetGunFromId(510);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            // X 费的最低需求不能含 Any：BattleManaPanel 会对 XCostRequiredMana 调 CanAfford，
            // 而 CanAfford 禁止 receiver 带 Any（升级「任意1」会直接报错）。
            config.Cost = new ManaGroup() { Red = 1 };
            config.UpgradedCost = ManaGroup.Empty;
            config.IsXCost = true;
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 5;
            config.UpgradedDamage = 6;
            config.Value1 = 1;
            config.UpgradedValue1 = 1;
            config.UpgradedKeywords = Keyword.Accuracy;
            config.RelativeEffects = new List<string>() { nameof(Vulnerable), nameof(TianziWeatherClear) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "竜崎いち";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziBlazingSwordDef))]
    public sealed class TianziBlazingSword : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            bool hasWeather = base.Battle.Player.StatusEffects.Any(se => se is TianziWeatherSeBase);
            if (!hasWeather)
            {
                foreach (BattleAction action in
                    TianziWeather.Apply(TianziWeather.Kind.Clear, base.Battle.Player, 1))
                    yield return action;
            }

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                yield return base.DebuffAction<Vulnerable>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
            }
            // 额外支付的费用点数（不计颜色）：超出 X 费最低需求的每一费 +1 段
            int paid = consumingMana.Amount;
            int required = base.XCostRequiredMana.Amount;
            int extra = paid - required;
            if (extra < 0)
                extra = 0;
            int hits = 1 + extra;
            base.CardGuns = new Guns(base.GunName, hits, true);
            foreach (GunPair gunPair in base.CardGuns.GunPairs)
                yield return base.AttackAction(selector, gunPair);
            
        }
    }
}
