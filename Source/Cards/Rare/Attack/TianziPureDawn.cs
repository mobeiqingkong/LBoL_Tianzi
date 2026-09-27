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

    // ------------------------------------------------------------------ 恒净之黎
    public sealed class TianziPureDawnDef : TianziCardTemplate
    {
        public override CardConfig MakeConfig()
        {
            CardConfig config = GetDefaultCardConfig();
            config.GunName = GunNameID.GetGunFromId(520);
            config.GunNameBurst = GunNameID.GetGunFromId(520);

            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Cost = new ManaGroup() { White = 1, Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Attack;
            config.TargetType = TargetType.SingleEnemy;

            config.Damage = 10;
            config.UpgradedDamage = 15;

            config.Keywords = Keyword.Exile | Keyword.Accuracy;
            config.UpgradedKeywords = Keyword.Exile | Keyword.Accuracy;

            config.RelativeEffects = new List<string>() { nameof(TianziRegenSe) };
            config.UpgradedRelativeEffects = config.RelativeEffects;

            config.Illustrator = "ryosios";
            config.Index = CardIndexGenerator.GetUniqueIndex(config);
            return config;
        }
    }


    /// <summary>
    /// 恒净之黎：造成 {Damage} 点伤害，移除{PlayerName}身上的所有负面效果，
    /// 并获得造成伤害减半的自愈。（放逐）
    /// </summary>
    [EntityLogic(typeof(TianziPureDawnDef))]
    public sealed class TianziPureDawn : TianziCard
    {
        protected override IEnumerable<BattleAction> Actions(
            UnitSelector selector,
            ManaGroup consumingMana,
            Interaction precondition
        )
        {
            yield return base.AttackAction(selector);

            yield return new RemoveAllNegativeStatusEffectAction(base.Battle.Player, 0.2f);

            int regen = Math.Max(1, (int)(base.Damage.Damage / 2f));
            yield return new ApplyStatusEffectAction<TianziRegenSe>(
                base.Battle.Player,
                regen,
                null,
                null,
                null,
                0.2f
            );
            yield break;
        }
    }
}
