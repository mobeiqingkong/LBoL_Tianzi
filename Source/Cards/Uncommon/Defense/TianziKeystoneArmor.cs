using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;

namespace TianziMod.Cards
{
    public sealed class TianziKeystoneArmorDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();

            config.Colors = new List<ManaColor>() { ManaColor.White };
            config.Cost = new ManaGroup() { Any = 1, White = 1 };
            config.Rarity = Rarity.Uncommon;

            config.Type = CardType.Defense;
            config.TargetType = TargetType.Self;

            config.Block = 10;
            config.UpgradedBlock = 12;

            config.Shield = 3;
            config.UpgradedShield = 5;

            config.Value1 = 1;
            config.UpgradedValue1 = 2;

            config.Keywords = Keyword.Shield;
            config.UpgradedKeywords = Keyword.Shield;

            config.Illustrator = "";
            config.RelativeKeyword = Keyword.Block | Keyword.Shield;
            config.UpgradedRelativeKeyword = Keyword.Block | Keyword.Shield;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 要石覆体：获得 {Block} 点格挡和 {Shield} 点护盾。
    /// 本场战斗每多打出一次，护盾额外 +{Value1}（体现在卡面数字上）。
    /// </summary>
    [EntityLogic(typeof(TianziKeystoneArmorDef))]
    public sealed class TianziKeystoneArmor : TianziCard
    {
        private int _plays;

        protected override void OnEnterBattle(BattleController battle)
        {
            base.OnEnterBattle(battle);
            this._plays = 0;
            this.NotifyChanged();
        }

        protected override int AdditionalShield
        {
            get { return this._plays * base.Value1; }
        }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.DefenseAction(true);
            this._plays++;
            this.NotifyChanged();
        }
    }
}
