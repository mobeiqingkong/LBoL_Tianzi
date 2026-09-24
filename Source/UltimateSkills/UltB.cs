using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using TianziMod.StatusEffects;

namespace TianziMod.TianziUlt
{
    public sealed class TianziUltBDef : TianziUltTemplate
    {
        public override UltimateSkillConfig MakeConfig()
        {
            UltimateSkillConfig config = GetDefaultUltConfig();
            // 设计稿：p点最大 200，每次使用消耗 100 → 2 × 100 = 200。
            config.PowerCost = 100;
            config.PowerPerLevel = 100;
            config.MaxPowerLevel = 2;
            config.Damage = 45;
            config.Value1 = 10; // 每场战斗首次额外伤害（合并进同一段）
            config.Value2 = 3; // 天气持续回合
            config.Keywords = Keyword.Accuracy;
            config.RelativeEffects = new List<string>()
            {
                nameof(TianziWeatherClear),
                nameof(TianziWeatherMist),
                nameof(TianziWeatherCloud),
                nameof(TianziWeatherAzure),
                nameof(TianziWeatherHail),
                nameof(TianziWeatherFog),
                nameof(TianziWeatherTyphoon),
                nameof(TianziWeatherCalm),
            };
            return config;
        }
    }

    [EntityLogic(typeof(TianziUltBDef))]
    public sealed class TianziUltB : UltimateSkill
    {
        // 每场战斗首次释放时伤害更高。
        // 符卡实例随 PlayerUnit 每场战斗重建，因此一个普通字段即可表达「本场首次」。
        private bool _usedInThisBattle;

        public TianziUltB()
        {
            base.TargetType = TargetType.SingleEnemy;
            base.GunName = TianziMod.GunName.GunNameID.GetGunFromId(511);
        }

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {
            // 不要再 PerformAction.Spell：Gun 511（TenshiSpell1）开火时
            // UnitView.PerformShootRunner 会 SpellDeclare(gun.Spell)，再手写 Spell 会播两次。
            // 彩符「天穹虹华之剑」
            // 随机释放一种天气（持续 3 回合），并对目标造成一段伤害（本场首次 +Value1）。
            // 面板上的 {Damage} 只显示基础伤害，首次加成不写进 Damage 属性。
            foreach (BattleAction action in TianziWeather.ApplyRandom(base.Battle.Player, base.Value2))
                yield return action;

            int amount = base.Config.Damage;
            if (!this._usedInThisBattle)
                amount += base.Value1;
            DamageInfo damage = DamageInfo.Attack(amount, true);
            this._usedInThisBattle = true;

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                yield return new DamageAction(
                    base.Owner,
                    enemy,
                    damage,
                    base.GunName,
                    GunType.Single
                );
            }
            yield break;
        }
    }
}
