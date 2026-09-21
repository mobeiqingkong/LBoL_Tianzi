using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.Exhibits
{
    public sealed class TianziExhibitBDef : TianziExhibitTemplate
    {
        public override ExhibitConfig MakeConfig()
        {
            ExhibitConfig config = GetDefaultExhibitConfig();
            config.Mana = new ManaGroup() { Red = 1 };
            config.BaseManaColor = ManaColor.Red;
            config.BaseManaAmount = 1;
            config.Value1 = 1; // 额外施加的层数
            config.RelativeEffects = new List<string>()
            {
                nameof(Weak),
                nameof(Vulnerable),
            };
            return config;
        }
    }

    /// <summary>
    /// 绯想之剑（光耀）【B】
    /// 提供 1 点红色费用。
    /// 主角每回合首次对敌人施加负面状态时，额外施加 1 层「虚弱」；
    /// 若目标身上已存在「虚弱」，则改为施加 1 层「易伤」。
    /// </summary>
    [EntityLogic(typeof(TianziExhibitBDef))]
    public sealed class TianziExhibitB : ShiningExhibit
    {
        private bool _usedThisTurn;
        private readonly List<EnemyUnit> _hooked = new List<EnemyUnit>();

        protected override void OnEnterBattle()
        {
            base.ReactBattleEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
            // 敌人是分批登场的，新敌人出现时要补挂监听。
            base.ReactBattleEvent<UnitEventArgs>(
                base.Battle.EnemySpawned,
                new EventSequencedReactor<UnitEventArgs>(this.OnEnemySpawned)
            );
            this.HookAllEnemies();
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            this._usedThisTurn = false;
            this.HookAllEnemies();
            yield break;
        }

        private IEnumerable<BattleAction> OnEnemySpawned(UnitEventArgs args)
        {
            this.HookAllEnemies();
            yield break;
        }

        private void HookAllEnemies()
        {
            foreach (EnemyUnit enemy in base.Battle.AllAliveEnemies)
            {
                if (enemy == null || this._hooked.Contains(enemy))
                    continue;
                base.ReactBattleEvent<StatusEffectApplyEventArgs>(
                    enemy.StatusEffectAdded,
                    new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemyStatusEffectAdded)
                );
                this._hooked.Add(enemy);
            }
        }

        private IEnumerable<BattleAction> OnEnemyStatusEffectAdded(StatusEffectApplyEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || this._usedThisTurn)
                yield break;
            if (args.Unit == null || args.Effect == null)
                yield break;
            if (args.Effect.Type != StatusEffectType.Negative)
                yield break;
            // 只响应「主角施加的」，敌方自己给自己上的负面状态不触发。
            if (
                args.Cause != ActionCause.Card
                && args.Cause != ActionCause.Us
                && args.Cause != ActionCause.UsUse
            )
                yield break;

            // 先上锁，避免我们追加的这一层再次触发自己（递归）。
            this._usedThisTurn = true;
            base.NotifyActivating();

            // ⚠ 参数顺序是 (target, level, duration, count, limit, occupationTime)。
            //   「虚弱 / 易伤」是【持续回合】类状态：HasLevel=false、HasDuration=true
            //   （已用 _seconf.py 从游戏 StatusEffectConfig.bin 核对过）。
            //   而 ApplyStatusEffectAction 的构造函数里有：
            //       if (statusEffect.HasDuration) { if (duration == null) throw new ArgumentException(...); }
            //   之前把 base.Value1 填在 level 位、duration 传 null ->
            //   这个 throw 发生在动作队列的协程里，异常被 ActionResolver 吞掉、动作队列被截断，
            //   play-area / 手牌控件簿记就此错乱 -> 之后每次出牌都抛 NullReferenceException
            //   -> 玩家看到的就是「一用就卡死」。
            //   所以这里必须把 Value1 填到 duration 位。
            if (args.Unit.GetStatusEffect<Weak>() != null)
            {
                yield return new ApplyStatusEffectAction<Vulnerable>(
                    args.Unit,
                    null,
                    base.Value1,
                    null,
                    null,
                    0.2f
                );
            }
            else
            {
                yield return new ApplyStatusEffectAction<Weak>(
                    args.Unit,
                    null,
                    base.Value1,
                    null,
                    null,
                    0.2f
                );
            }
        }
    }
}
