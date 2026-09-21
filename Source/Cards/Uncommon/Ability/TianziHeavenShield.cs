using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 天界之庇护
    public sealed class TianziHeavenShieldDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 2 };
            config.UpgradedCost = new ManaGroup() { White = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Mana = new ManaGroup() { White = 1 };
            config.UpgradedMana = new ManaGroup() { White = 1 };
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziHeavenShieldSe),
                nameof(AmuletForCard),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>天界之庇护：每回合结束时获得 1 层庇护，下回合获得 1 点白色法力。</summary>
    [EntityLogic(typeof(TianziHeavenShieldDef))]
    public sealed class TianziHeavenShield : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziHeavenShieldSe>(1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
