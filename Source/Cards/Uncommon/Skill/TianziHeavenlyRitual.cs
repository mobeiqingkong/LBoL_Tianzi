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

    // ------------------------------------------------------------------ 天人的流�?
    public sealed class TianziHeavenlyRitualDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { White = 1 };
            config.UpgradedCost = ManaGroup.Empty;
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Scry = 5;
            config.UpgradedScry = 5;

            config.Value1 = 2; // 按类型追加的抽牌�?

            config.RelativeEffects = new List<string>() { nameof(Firepower), nameof(Spirit), nameof(TianziKarmaKwSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.RelativeKeyword = Keyword.Scry;
            config.UpgradedRelativeKeyword = Keyword.Scry;

            config.Illustrator = "";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 天人的流仪：占卜 {Scry}。抽 1 张牌�?
    /// 若抽到攻击牌获得 1 点火力；防御牌获�?1 点灵力；技能牌额外�?{Value1} 张�?
    /// </summary>
    [EntityLogic(typeof(TianziHeavenlyRitualDef))]
    public sealed class TianziHeavenlyRitual : TianziCard
    {
        protected override bool HasKarmaKeyword { get { return true; } }
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return new ScryAction(base.Scry);

            HashSet<Card> before = new HashSet<Card>();
            foreach (Card c in base.Battle.HandZone)
                before.Add(c);

            yield return new DrawManyCardAction(1);

            Card drawn = null;
            foreach (Card c in base.Battle.HandZone)
            {
                if (!before.Contains(c))
                {
                    drawn = c;
                    break;
                }
            }
            if (drawn == null)
                yield break;

            foreach (BattleAction action in TianziKarmaPlay.Resolve(
                this,
                drawn.CardType,
                this.RitualAttack(),
                this.RitualDefense(),
                this.RitualSkill(),
                null,
                null))
                yield return action;
        }

        private IEnumerable<BattleAction> RitualAttack()
        {
            yield return BuffAction<Firepower>(1, 0, 0, 0, 0.2f);
        }

        private IEnumerable<BattleAction> RitualDefense()
        {
            yield return BuffAction<Spirit>(1, 0, 0, 0, 0.2f);
        }

        private IEnumerable<BattleAction> RitualSkill()
        {
            yield return new DrawManyCardAction(base.Value1);
        }
    }
}
