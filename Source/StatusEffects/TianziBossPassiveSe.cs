using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.StatusEffects
{
    public sealed class TianziBossPeachSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Special;
            config.IsStackable = false;
            // Level = 回复量；Count = 额外积攒 P
            config.HasLevel = true;
            config.HasCount = true;
            config.HasDuration = false;
            config.RelativeEffects = new List<string> { nameof(EnemyEnergy) };
            return config;
        }
    }

    /// <summary>
    /// 本章天子 Boss 专属仙桃：受击回血并额外积攒能量。
    /// 伤害等量转能量仍由 EnemyEnergy 负责。
    /// </summary>
    [EntityLogic(typeof(TianziBossPeachSeDef))]
    public sealed class TianziBossPeachSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            if (base.Level <= 0)
                base.Level = 1;
            if (base.Count <= 0)
                base.Count = 1;
            base.ReactOwnerEvent<DamageEventArgs>(
                unit.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
        }

        private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
        {
            if (args.DamageInfo.Damage <= 0f)
                yield break;

            base.NotifyActivating();
            yield return new HealAction(base.Owner, base.Owner, base.Level, HealType.Normal, 0.1f);
            if (base.Count > 0)
                yield return new ApplyStatusEffectAction<EnemyEnergy>(base.Owner, base.Count);
        }
    }

    public sealed class TianziBossHeavenQiSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Special;
            config.IsStackable = false;
            config.HasLevel = false;
            config.HasDuration = false;
            return config;
        }
    }

    /// <summary>天人之气：回合结束时负面状态额外再掉 1 层。</summary>
    [EntityLogic(typeof(TianziBossHeavenQiSeDef))]
    public sealed class TianziBossHeavenQiSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                unit.TurnEnded,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
        }

        private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
        {
            List<StatusEffect> snapshot = new List<StatusEffect>(base.Owner.StatusEffects);
            bool any = false;
            foreach (StatusEffect se in snapshot)
            {
                if (se == null || !se.HasDuration)
                    continue;
                if (se.Config.Type != StatusEffectType.Negative)
                    continue;
                if (se.Duration <= 0)
                    continue;

                any = true;
                int next = se.Duration - 1;
                se.Duration = next;
                if (next <= 0)
                    yield return new RemoveStatusEffectAction(se, true, 0.05f);
            }
            if (any)
                base.NotifyActivating();
            yield break;
        }
    }

    public sealed class TianziBossKarmaInfluenceSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Special;
            config.IsStackable = false;
            config.HasLevel = true;
            config.LevelStackType = StackType.Add;
            config.HasDuration = false;
            config.HasCount = true;
            config.CountStackType = StackType.Keep;
            return config;
        }
    }

    /// <summary>因果影响：每打出 Level 张牌后，下一张奇数牌丢随机费用，偶数牌随机弃牌。</summary>
    [EntityLogic(typeof(TianziBossKarmaInfluenceSeDef))]
    public sealed class TianziBossKarmaInfluenceSe : StatusEffect
    {
        private bool _punishNext;

        protected override void OnAdded(Unit unit)
        {
            base.Count = 0;
            this._punishNext = false;
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (args.Card == null || args.Card.CardType == CardType.Status)
                yield break;

            if (this._punishNext)
            {
                this._punishNext = false;
                base.NotifyActivating();
                bool odd = TianziParity.IsOddAtPlay(base.Battle);
                if (odd)
                {
                    foreach (BattleAction action in this.LoseRandomMana())
                        yield return action;
                }
                else
                {
                    List<Card> hand = new List<Card>();
                    foreach (Card c in base.Battle.HandZone)
                    {
                        if (c != null && c != args.Card)
                            hand.Add(c);
                    }
                    if (hand.Count > 0)
                    {
                        Card discard = hand[base.GameRun.BattleRng.NextInt(0, hand.Count)];
                        yield return new DiscardAction(discard);
                    }
                }
                yield break;
            }

            base.Count++;
            if (base.Count >= base.Level)
            {
                base.Count = 0;
                this._punishNext = true;
                base.NotifyActivating();
            }
        }

        private IEnumerable<BattleAction> LoseRandomMana()
        {
            ManaGroup pool = base.Battle.BattleMana;
            List<ManaColor> colors = new List<ManaColor>();
            if (pool.White > 0) colors.Add(ManaColor.White);
            if (pool.Blue > 0) colors.Add(ManaColor.Blue);
            if (pool.Black > 0) colors.Add(ManaColor.Black);
            if (pool.Red > 0) colors.Add(ManaColor.Red);
            if (pool.Green > 0) colors.Add(ManaColor.Green);
            if (pool.Colorless > 0) colors.Add(ManaColor.Colorless);
            if (pool.Philosophy > 0) colors.Add(ManaColor.Philosophy);
            if (colors.Count == 0)
                yield break;

            ManaColor pick = colors[base.GameRun.BattleRng.NextInt(0, colors.Count)];
            ManaGroup lose = default(ManaGroup);
            switch (pick)
            {
                case ManaColor.White: lose = new ManaGroup() { White = 1 }; break;
                case ManaColor.Blue: lose = new ManaGroup() { Blue = 1 }; break;
                case ManaColor.Black: lose = new ManaGroup() { Black = 1 }; break;
                case ManaColor.Red: lose = new ManaGroup() { Red = 1 }; break;
                case ManaColor.Green: lose = new ManaGroup() { Green = 1 }; break;
                case ManaColor.Colorless: lose = new ManaGroup() { Colorless = 1 }; break;
                case ManaColor.Philosophy: lose = new ManaGroup() { Philosophy = 1 }; break;
            }
            yield return new LoseManaAction(lose);
        }
    }

    public static class TianziChapterBossPassive
    {
        public const int SpellEnergyCost = 100;

        public static bool HasSpellEnergy(EnemyUnit boss, int cost = SpellEnergyCost)
        {
            if (boss == null)
                return false;
            EnemyEnergy energy = boss.GetStatusEffect<EnemyEnergy>();
            return energy != null && energy.Level >= cost;
        }

        public static int KarmaThreshold(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Lunatic: return 7;
                case GameDifficulty.Hard: return 8;
                default: return 9;
            }
        }
    }
}
