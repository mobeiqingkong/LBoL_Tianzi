using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziHeavenStepGuardDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 1 };
            config.Rarity = Rarity.Uncommon;
            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;
            config.Block = 8;
            config.UpgradedBlock = 12;
            config.Value1 = 2;
            config.UpgradedValue1 = 4;
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe), nameof(TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Illustrator = "苏予九u";
            config.RelativeKeyword = Keyword.Block;
            config.UpgradedRelativeKeyword = Keyword.Block;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziHeavenStepGuardDef))]
    public sealed class TianziHeavenStepGuard : TianziCard
    {
        protected override bool HasParityKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return base.DefenseAction(true);
            foreach (BattleAction action in TianziParityPlay.Resolve(this, this.OddBranch(), this.EvenBranch()))
                yield return action;
        }

        private IEnumerable<BattleAction> OddBranch()
        {
            BattleAction gain = TianziTempHp.GainAction(base.Battle.Player, base.Value1);
            if (gain != null)
                yield return gain;
        }

        private IEnumerable<BattleAction> EvenBranch()
        {
            yield return new DrawManyCardAction(1);
        }
    }
}
