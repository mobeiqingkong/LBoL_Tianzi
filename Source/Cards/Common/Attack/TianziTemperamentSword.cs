using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards.Template;
using TianziMod.GunName;
using TianziMod.Keywords;
using TianziMod.StatusEffects;

namespace TianziMod.Cards
{
    public sealed class TianziTemperamentSwordDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(7300);
            config.GunNameBurst = GunNameID.GetGunFromId(7300);
            config.Colors = new List<ManaColor>() { ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, Red = 1 };
            config.UpgradedCost = new ManaGroup() { Any = 2 };
            config.Rarity = Rarity.Common;
            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;
            config.Damage = 9;
            config.UpgradedDamage = 12;
            config.Value1 = 1;
            config.UpgradedValue1 = 2;
            config.RelativeEffects = new List<string>()
            {
                nameof(Vulnerable),
                nameof(Weak),
                nameof(TianziKarmaKwSe),
            };
            config.UpgradedRelativeEffects = config.RelativeEffects;
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    [EntityLogic(typeof(TianziTemperamentSwordDef))]
    public sealed class TianziTemperamentSword : TianziCard
    {
        protected override bool HasKarmaKeyword { get { return true; } }

        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            

            Card previous = this.PreviousPlayedCard;
            CardType kind = previous == null ? CardType.Unknown : previous.CardType;
            // 无上一张牌时不触发因果；有因果之剑时可自行选择分支
            if (previous == null && !TianziKarma.Forced)
                yield break;

            // 气质之剑只有「攻击→易伤 / 防御&技能→虚弱」两支；技能并入防御分支
            if (kind == CardType.Skill)
                kind = CardType.Defense;

            foreach (BattleAction action in TianziKarmaPlay.Resolve(
                this,
                kind,
                this.DebuffAll(selector, true),
                this.DebuffAll(selector, false),
                null,
                null,
                null))
                yield return action;
            yield return base.AttackAction(selector);
        }

        private IEnumerable<BattleAction> DebuffAll(UnitSelector selector, bool vuln)
        {
            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                if (!enemy.IsAlive)
                    continue;
                if (vuln)
                    yield return base.DebuffAction<Vulnerable>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
                else
                    yield return base.DebuffAction<Weak>(enemy, 1, base.Value1, 0, 0, true, 0.2f);
            }
        }
    }
}
