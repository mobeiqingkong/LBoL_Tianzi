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
    /// 天界造物：从随机罕见牌中选一张加入手中。选牌栏不可取消。
    /// </summary>
    [EntityLogic(typeof(TianziHeavenCreationDef))]
    public sealed class TianziHeavenCreation : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            List<Card> options = this.RollOptions();
            if (options.Count == 0)
                yield break;

            // 出牌前置选牌会被 UseCardAction 强制成可取消，所以改到这里打开，并关掉取消。
            SelectCardInteraction interaction = new SelectCardInteraction(1, 1, options)
            {
                Source = this,
                CanCancel = false
            };
            yield return new InteractionAction(interaction, false);
            if (interaction.IsCanceled || interaction.SelectedCards == null || interaction.SelectedCards.Count == 0)
                yield break;

            Card pick = interaction.SelectedCards[0];
            pick.SetTurnCost(ManaGroup.Empty);
            pick.IsExile = true;
            pick.IsEthereal = true;
            yield return new AddCardsToHandAction(new Card[] { pick }, AddCardsType.Normal, false);
        }

        private List<Card> RollOptions()
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
            return options;
        }
    }
}
