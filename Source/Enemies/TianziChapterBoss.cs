using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;
using TianziMod.Cards;
using TianziMod.StatusEffects;

namespace TianziMod.Enemies
{
    [EntityLogic(typeof(TianziChapterBossDef))]
    public sealed class TianziChapterBoss : EnemyUnit
    {
        private enum MoveKind
        {
            /// <summary>天穹斩 → 要石浮游炮 → 因果之剑 → 状态防御</summary>
            SkySlash,
            Keystone,
            KarmaSword,
            StatusGuard,
            Rainbow,
        }

        private MoveKind _next = MoveKind.SkySlash;
        private MoveKind _last = MoveKind.StatusGuard;
        private bool _altCycle;
        private bool _rainbowUsed;

        // Move yaml 顺序：0 要石浮游炮 / 1 天穹斩 / 2 因果之剑 / 3 状态防御 / 4 彩符
        private string MoveKeystone { get { return base.GetMove(0); } }
        private string MoveSkySlash { get { return base.GetMove(1); } }
        private string MoveKarma { get { return base.GetMove(2); } }
        private string MoveGuard { get { return base.GetMove(3); } }
        private string MoveRainbow { get { return base.GetMove(4); } }

        /// <summary>第二章持有纪念品时的登场台词（来自 yaml）。</summary>
        public string KeepsakeDebutChat
        {
            get { return this.LocalizeProperty("KeepsakeDebutChat", false, true); }
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            this._next = MoveKind.SkySlash;
            this._last = MoveKind.StatusGuard;
            this._altCycle = false;
            this._rainbowUsed = false;

            base.ReactBattleEvent(battle.BattleStarted, this.OnBattleStarted);
        }

        private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
        {
            yield return new ApplyStatusEffectAction<EnemyEnergy>(this, 0);

            int heal;
            int bonusEnergy;
            switch (base.Difficulty)
            {
                case GameDifficulty.Lunatic:
                    heal = 2;
                    bonusEnergy = 3;
                    break;
                case GameDifficulty.Hard:
                    heal = 1;
                    bonusEnergy = 2;
                    break;
                default:
                    heal = 1;
                    bonusEnergy = 1;
                    break;
            }
            // Level=回复；Count=额外 P
            yield return new ApplyStatusEffectAction<TianziBossPeachSe>(
                this, heal, null, null, bonusEnergy, 0f);
            yield return new ApplyStatusEffectAction<TianziBossHeavenQiSe>(this, null, null, null, null, 0f);
            int threshold = TianziChapterBossPassive.KarmaThreshold(base.Difficulty);
            yield return new ApplyStatusEffectAction<TianziBossKarmaInfluenceSe>(
                this, threshold, null, null, null, 0f);
        }

        protected override IEnumerable<IEnemyMove> GetTurnMoves()
        {
            if (TianziChapterBossPassive.HasSpellEnergy(this))
            {
                MoveKind beforeUlt = this._next;
                yield return this.RainbowMove();
                this._last = MoveKind.Rainbow;
                // 释符前若下一手是天穹斩或状态防御 → 进入另类循环
                this._altCycle = beforeUlt == MoveKind.SkySlash || beforeUlt == MoveKind.StatusGuard;
                this._next = MoveKind.SkySlash;
                yield break;
            }

            switch (this._next)
            {
                case MoveKind.SkySlash:
                    yield return this.SkySlashMove();
                    this._last = MoveKind.SkySlash;
                    break;
                case MoveKind.Keystone:
                    // 要石浮游炮：5/5/6 × 3，精准
                    yield return base.AttackMove(
                        this.MoveKeystone, base.Gun1, base.Damage1, base.Count1, true);
                    this._last = MoveKind.Keystone;
                    break;
                case MoveKind.KarmaSword:
                    yield return this.KarmaMove();
                    this._last = MoveKind.KarmaSword;
                    break;
                case MoveKind.StatusGuard:
                    yield return this.GuardMove();
                    this._last = MoveKind.StatusGuard;
                    break;
            }
        }

        protected override void UpdateMoveCounters()
        {
            if (this._last == MoveKind.Rainbow)
                return;

            if (this._altCycle)
            {
                // 另类：状态防御 → 天穹斩 → 要石浮游炮 → 因果之剑
                this._next = this._last switch
                {
                    MoveKind.StatusGuard => MoveKind.SkySlash,
                    MoveKind.SkySlash => MoveKind.Keystone,
                    MoveKind.Keystone => MoveKind.KarmaSword,
                    MoveKind.KarmaSword => MoveKind.StatusGuard,
                    _ => MoveKind.SkySlash,
                };
                return;
            }

            // 常态：天穹斩 → 要石浮游炮 → 因果之剑 → 状态防御
            this._next = this._last switch
            {
                MoveKind.SkySlash => MoveKind.Keystone,
                MoveKind.Keystone => MoveKind.KarmaSword,
                MoveKind.KarmaSword => MoveKind.StatusGuard,
                MoveKind.StatusGuard => MoveKind.SkySlash,
                _ => MoveKind.SkySlash,
            };
        }

        private IEnemyMove SkySlashMove()
        {
            // 天穹斩：7/7/8 × 2，非精准；附带易伤/虚弱
            return new SimpleEnemyMove(
                Intention.Attack(base.Damage2, base.Count2).WithMoveName(this.MoveSkySlash),
                this.SkySlashActions());
        }

        private IEnumerable<BattleAction> SkySlashActions()
        {
            yield return new EnemyMoveAction(this, this.MoveSkySlash);
            // N/H：易伤1 + 虚弱1；L：易伤1 + 虚弱2
            int weakDur = base.Difficulty == GameDifficulty.Lunatic ? 2 : 1;
            yield return new ApplyStatusEffectAction<Vulnerable>(
                base.Battle.Player, null, 1, null, null, 0.1f);
            yield return new ApplyStatusEffectAction<Weak>(
                base.Battle.Player, null, weakDur, null, null, 0.1f);
            foreach (BattleAction action in base.AttackActions(
                null, base.Gun2, base.Damage2, base.Count2, false))
                yield return action;
        }

        private IEnemyMove KarmaMove()
        {
            // 因果之剑：10/12/14 × 1，精准
            return new SimpleEnemyMove(
                Intention.Attack(base.Damage3, true).WithMoveName(this.MoveKarma),
                this.KarmaActions());
        }

        private IEnumerable<BattleAction> KarmaActions()
        {
            yield return new EnemyMoveAction(this, this.MoveKarma);
            yield return new AddCardsToDrawZoneAction(
                new[] { Library.CreateCard<TianziKarmaShackle>() }, DrawZoneTarget.Random);
            yield return new AddCardsToDiscardAction(
                new[] { Library.CreateCard<TianziKarmaShackle>() });
            foreach (BattleAction action in base.AttackActions(
                null, base.Gun3, base.Damage3, 1, true))
                yield return action;
        }

        private IEnemyMove GuardMove()
        {
            // 状态防御：格挡 6/8/10，护盾 4/4/5
            int shield = base.Difficulty == GameDifficulty.Lunatic ? 5 : 4;
            return new SimpleEnemyMove(
                Intention.Defend().WithMoveName(this.MoveGuard),
                this.GuardActions(shield));
        }

        private IEnumerable<BattleAction> GuardActions(int shield)
        {
            yield return new EnemyMoveAction(this, this.MoveGuard);
            yield return new CastBlockShieldAction(this, this, base.Defend, shield);
            yield return new AddCardsToDiscardAction(
                new[] { Library.CreateCard<TianziKarmaShackle>() });
        }

        private IEnemyMove RainbowMove()
        {
            int damage = this.RainbowDamage();
            return new SimpleEnemyMove(
                Intention.SpellCard(this.MoveRainbow, damage, true),
                this.RainbowActions(damage));
        }

        private IEnumerable<BattleAction> RainbowActions(int damage)
        {
            yield return new ApplyStatusEffectAction<EnemyEnergyNegative>(
                this, TianziChapterBossPassive.SpellEnergyCost);
            foreach (BattleAction action in base.AttackActions(
                this.MoveRainbow, base.Gun4, damage, 1, true))
                yield return action;
        }

        private int RainbowDamage()
        {
            // 首次 N/H 40、L 45；之后用 Damage4（30/30/35）
            if (!this._rainbowUsed)
            {
                this._rainbowUsed = true;
                return base.Difficulty == GameDifficulty.Lunatic ? 45 : 40;
            }
            return base.Damage4;
        }
    }
}
