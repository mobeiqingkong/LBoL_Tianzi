using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{

    // ------------------------------------------------------------------ 天人的直�?
    public sealed class TianziHeavenlyInstinctDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Mana = new ManaGroup() { White = 1, Red = 1 };
            config.Value1 = 2; // 偶数手牌抽牌�?

            config.RelativeEffects = new List<string>() { nameof(TianziParityKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Keywords = Keyword.Exile;
            config.UpgradedKeywords = Keyword.None;

            config.Illustrator = "Xiirus";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 天人的直觉：手牌张数为奇数时，获�?{Mana} 点费用；
    /// 为偶数时，抽 {Value1} 张牌。（放逐；升级后取消放逐）
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyInstinctDef))]
    public sealed class TianziHeavenlyInstinct : TianziCard
    {
        protected override bool HasParityKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            foreach (BattleAction action in TianziParityPlay.Resolve(this, this.OddBranch(), this.EvenBranch()))
                yield return action;
        }

        private IEnumerable<BattleAction> OddBranch()
        {
            yield return new GainManaAction(base.Mana);
        }

        private IEnumerable<BattleAction> EvenBranch()
        {
            yield return new DrawManyCardAction(base.Value1);
        }
    }
}
