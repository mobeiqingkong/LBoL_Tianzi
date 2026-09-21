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

    // ------------------------------------------------------------------ 终焉审判
    public sealed class TianziFinalJudgementDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(4540);
            config.GunNameBurst = GunNameID.GetGunFromId(4540);

            config.Colors = new List<ManaColor>() { ManaColor.Colorless };
            config.Cost = new ManaGroup() { Any = 3, Colorless = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2, Colorless = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Mana = new ManaGroup() { Colorless = 1 };
            config.UpgradedMana = new ManaGroup() { Colorless = 1 };
            config.RelativeEffects = new List<string>() { nameof(TianziLethalSe) };

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 终焉审判：造成 {Damage} 点伤害。
    /// 本场战斗中每有一张牌被放逐，此牌伤害提高 {Value1} 点。
    /// </summary>
    [EntityLogic(typeof(TianziFinalJudgementDef))]
    public sealed class TianziFinalJudgement : TianziCard
    {
        public ManaGroup Mana2
        {
            get { return new ManaGroup() { Philosophy = 1 }; }
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return BuffAction<TianziLethalSe>(1, 0, 0, 0, 0.2f);
        }
    }
}
