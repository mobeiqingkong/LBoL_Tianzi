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
            config.Value1 = 10; // 每场战斗首次额外一段伤害
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
            // 彩符「天穹虹华之剑」
            // 随机释放一种天气（持续 3 回合），并对目标造成 45 点伤害（每场首次 55 点）。
            foreach (BattleAction action in TianziWeather.ApplyRandom(base.Battle.Player, base.Value2))
                yield return action;

            int extra = this._usedInThisBattle ? 0 : base.Value1;
            this._usedInThisBattle = true;

            foreach (Unit enemy in selector.GetUnits(base.Battle))
            {
                yield return new DamageAction(
                    base.Owner,
                    enemy,
                    DamageInfo.Attack(base.Config.Damage, true),
                    base.GunName,
                    GunType.Single
                );
                if (extra > 0)
                {
                    yield return new DamageAction(
                        base.Owner,
                        enemy,
                        DamageInfo.Attack(extra, true),
                        base.GunName,
                        GunType.Single
                    );
                }
            }
            yield break;
        }
    }
}
