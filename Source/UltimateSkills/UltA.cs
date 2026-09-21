using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.TianziUlt
{
    public sealed class TianziUltADef : TianziUltTemplate
    {
        public override UltimateSkillConfig MakeConfig()
        {
            UltimateSkillConfig config = GetDefaultUltConfig();
            // 设计稿：p点最大 180，每次使用消耗 60。
            // 游戏用 MaxPowerLevel × PowerPerLevel = p点上限，所以这里 3 × 60 = 180。
            config.PowerCost = 60;
            config.PowerPerLevel = 60;
            config.MaxPowerLevel = 3;
            config.Damage = 1;
            config.Value1 = 1; // 天衣无缝持续回合
            config.Value2 = 5; // 生命值
            config.Keywords = Keyword.None;
            config.RelativeEffects = new List<string>()
            {
                nameof(Invincible),
            };
            return config;
        }
    }

    [EntityLogic(typeof(TianziUltADef))]
    public sealed class TianziUltA : UltimateSkill
    {
        public TianziUltA()
        {
            base.TargetType = TargetType.Self;
            base.GunName = TianziMod.GunName.GunNameID.Heaven;
        }

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {
            // 气符「无念无想的境界」
            // 获得 1 回合天衣无缝 + 5 点生命值，并移除主角身上所有负面状态。
            yield return new ApplyStatusEffectAction<Invincible>(
                base.Battle.Player,
                1,
                base.Value1,
                null,
                null,
                0.2f
            );

            yield return new HealAction(base.Battle.Player, base.Battle.Player, base.Value2, HealType.Normal, 0.2f);
            yield return new RemoveAllNegativeStatusEffectAction(base.Battle.Player, 0.2f);
            yield break;
        }
    }
}
