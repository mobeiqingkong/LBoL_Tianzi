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
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    // ==================================================================================
    //  稀有 · 技能牌（3 张）
    // ==================================================================================

    // ------------------------------------------------------------------ 天界造物
    public sealed class TianziHeavenCreationDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, White = 1, Red = 1 };
            config.UpgradedCost = ManaGroup.Hybrids(2, ManaColor.White, ManaColor.Red);
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Skill;
            config.TargetType = TargetType.Self;

            config.Value1 = 3;
            config.UpgradedValue1 = 5;

            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Exile | Keyword.Ethereal;
            config.UpgradedRelativeKeyword = Keyword.Exile | Keyword.Ethereal;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 天界造物：从弃牌堆中选择一张牌，将它的 {Value1} 份复制置入手中。
    /// 若弃牌堆为空，则抽 {Value2} 张牌。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenCreationDef))]
    public sealed class TianziHeavenCreation : TianziCard
    {
        public override Interaction Precondition()
        {
            List<Card> options = new List<Card>();
            List<CardConfig> pool = new List<CardConfig>();
            foreach (CardConfig cfg in CardConfig.AllConfig())
            {
                if (cfg == null || !cfg.IsPooled)
                    continue;
                if (cfg.Rarity != Rarity.Uncommon)
                    continue;
                if (cfg.Owner != BepinexPlugin.modUniqueID)
                    continue;
                if (cfg.Type == CardType.Unknown)
                    continue;
                pool.Add(cfg);
            }
            int take = base.Value1 > 0 ? base.Value1 : 3;
            if (take > pool.Count)
                take = pool.Count;
            for (int i = 0; i < take && pool.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                Card made = Library.CreateCard(pool[idx].Id);
                made.SetTurnCost(ManaGroup.Empty);
                made.IsExile = true;
                made.IsEthereal = true;
                options.Add(made);
                pool.RemoveAt(idx);
            }
            if (options.Count == 0)
                return null;
            return new SelectCardInteraction(1, 1, options);
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            SelectCardInteraction interaction = precondition as SelectCardInteraction;
            if (interaction == null || interaction.SelectedCards.Count == 0)
                yield break;
            Card pick = interaction.SelectedCards[0];
            pick.SetTurnCost(ManaGroup.Empty);
            pick.IsExile = true;
            pick.IsEthereal = true;
            yield return new AddCardsToHandAction(new Card[] { pick }, AddCardsType.Normal, false);
        }
    }
}
