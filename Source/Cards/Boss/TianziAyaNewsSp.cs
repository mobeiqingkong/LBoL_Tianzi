using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    /// <summary>
    /// 《文文新闻》天子特刊。效果对齐 AyaNewsSakuyaSp：
    /// 牌在手牌里时，玩家回合结束失去 Value1 点灵力（挂 SpiritNegative）。
    /// </summary>
    public sealed class TianziAyaNewsSpDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.IsPooled = false;
            config.FindInBattle = false;
            config.HideMesuem = true;
            config.IsUpgradable = false;
            config.Owner = null;
            config.Type = CardType.Status;
            config.TargetType = TargetType.Self;
            config.Colors = new List<ManaColor>() { ManaColor.Colorless };
            config.Cost = ManaGroup.Empty;
            config.Rarity = Rarity.Rare;
            config.Value1 = 2;
            config.Keywords = Keyword.Exile | Keyword.Ethereal;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Ethereal;
            config.RelativeEffects = new List<string>() { nameof(SpiritNegative) };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }

    [EntityLogic(typeof(TianziAyaNewsSpDef))]
    public sealed class TianziAyaNewsSp : TianziCard
    {
        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            base.ReactBattleEvent<UnitEventArgs>(
                battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
        }

        private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Zone != CardZone.Hand)
                yield break;
            base.NotifyActivating();
            yield return base.DebuffAction<SpiritNegative>(
                base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
        }
    }
}
