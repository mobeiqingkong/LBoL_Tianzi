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

    // ------------------------------------------------------------------ 绯想剑斩波
    public sealed class TianziScarletWaveDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Red = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Value1 = 1;
            config.UpgradedValue1 = 2;
            config.RelativeEffects = new List<string>() { nameof(TianziScarletWaveSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>绯想剑斩波：每回合首次造成攻击伤害时（含符卡），获得等量的格挡。</summary>
    [EntityLogic(typeof(TianziScarletWaveDef))]
    public sealed class TianziScarletWave : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return BuffAction<TianziScarletWaveSe>(base.Value1, 0, 0, 0, 0.2f);
            yield break;
        }
    }
}
