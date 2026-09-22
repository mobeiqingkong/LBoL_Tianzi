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

        private string MoveSkySlash { get { return base.GetMove(0); } }
        private string MoveKeystone { get { return base.GetMove(1); } }
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

            // 文档仙桃 = 回血 + 额外 p；能量 = 伤害等量转 p。均仅本章 Boss。
            this.React(new ApplyStatusEffectAction<EnemyEnergy>(this, 0));
            this.React(new ApplyStatusEffectAction<TianziBossPeachSe>(this, null, null, null, null, 0f));
            this.React(new ApplyStatusEffectAction<TianziBossHeavenQiSe>(this, null, null, null, null, 0f));
            int threshold = TianziChapterBossPassive.KarmaThreshold(base.Difficulty);
            this.React(new ApplyStatusEffectAction<TianziBossKarmaInfluenceSe>(
                this, threshold, null, null, null, 0f));
        }

        protected override IEnumerable<IEnemyMove> GetTurnMoves()
        {
            if (TianziChapterBossPassive.HasSpellEnergy(this))
            {
                MoveKind beforeUlt = this._next;
                yield return this.RainbowMove();
                this._last = MoveKind.Rainbow;
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
            return new SimpleEnemyMove(
                Intention.Attack(base.Damage2, base.Count2).WithMoveName(this.MoveSkySlash),
                this.SkySlashActions());
        }

        private IEnumerable<BattleAction> SkySlashActions()
        {
            yield return new EnemyMoveAction(this, this.MoveSkySlash);
            // 文档：N/H 易伤1+虚弱1；L 易伤1+虚弱2（duration 必须走第 3 参）
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
            if (!this._rainbowUsed)
            {
                this._rainbowUsed = true;
                return base.Difficulty == GameDifficulty.Lunatic ? 45 : 40;
            }
            return base.Damage4;
        }
    }
}
