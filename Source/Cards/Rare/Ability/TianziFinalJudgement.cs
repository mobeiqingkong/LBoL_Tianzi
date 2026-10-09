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
using LBoL.EntityLib.Cards.Neutral.NoColor;
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
            config.Colors = new List<ManaColor>() { ManaColor.White, ManaColor.Green, ManaColor.Red };
            config.Cost = new ManaGroup() { Any = 1, White = 1, Green = 1, Red = 1};
            config.UpgradedCost = new ManaGroup() { White = 1, Green = 1, Red = 1 };
            config.Rarity = Rarity.Rare;

            config.Type = CardType.Ability;
            config.TargetType = TargetType.Self;
            config.Mana = new ManaGroup() { Philosophy = 1 };
            config.UpgradedMana = new ManaGroup() { Philosophy = 1 };
            config.RelativeCards = new List<string>() { nameof(GManaCard) };
            config.UpgradedRelativeCards = new List<string>() { nameof(PManaCard) };
            config.Illustrator = "核燃黑猫";
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

            // 打出时将 1 张「生机」加入手中；本牌已升级则改为「幻虹」。
            // 用「换卡」而不是「把生机升级」：原生从不升级法力牌，需要更强的一张时一律换卡
            // （参见 EntityLib 的同款写法 —— 升级场合发 PManaCard 幻虹，否则发 GManaCard 生机）。
            Card mana;
            if (base.IsUpgraded)
                mana = Library.CreateCard<PManaCard>();
            else
                mana = Library.CreateCard<GManaCard>();
            yield return new AddCardsToHandAction(new Card[] { mana });
        }
    }
}
