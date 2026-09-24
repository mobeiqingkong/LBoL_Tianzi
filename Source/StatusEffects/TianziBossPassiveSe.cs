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
            return config;
        }
    }

    /// <summary>
    /// 本章天子 Boss 专属仙桃：受击回血并额外积攒 P（经 EnemyEnergy）。
    /// </summary>
    [EntityLogic(typeof(TianziBossPeachSeDef))]
    public sealed class TianziBossPeachSe : StatusEffect
    {
        public override string OverrideIconName
        {
            get { return nameof(FlatPeach); }
        }

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

    /// <summary>
    /// 因果影响：打出第 Level 张牌时按「打出前手牌数」奇偶惩罚。
    /// Count 为已打出张数；Count == Level-1 时 Highlight 发光表示即将触发。
    /// </summary>
    [EntityLogic(typeof(TianziBossKarmaInfluenceSeDef))]
    public sealed class TianziBossKarmaInfluenceSe : StatusEffect
    {
        /// <summary>CardUsing 时快照的打出前手牌数（含正打出的那张）。</summary>
        private int _handBeforePlay = -1;

        protected override void OnAdded(Unit unit)
        {
            base.Count = 0;
            base.Highlight = false;
            this._handBeforePlay = -1;
            base.HandleOwnerEvent(
                base.Battle.CardUsing,
                new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
            base.ReactOwnerEvent(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
        }

        private void OnCardUsing(CardUsingEventArgs args)
        {
            if (args.Card == null || args.Card.CardType == CardType.Status)
            {
                this._handBeforePlay = -1;
                return;
            }
            // 此时牌还在手里：手牌数 = 打出前张数
            this._handBeforePlay = base.Battle.HandZone.Count;
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (args.Card == null || args.Card.CardType == CardType.Status)
                yield break;

            int handBefore = this._handBeforePlay;
            this._handBeforePlay = -1;
            if (handBefore < 0)
                handBefore = base.Battle.HandZone.Count + 1;

            base.Count++;
            int threshold = Math.Max(base.Level, 1);

            // 差一张触发：发光提示激活
            if (base.Count == threshold - 1)
            {
                base.Highlight = true;
                base.NotifyActivating();
                yield break;
            }

            if (base.Count < threshold)
                yield break;

            // 第 Level 张：触发惩罚并复位
            base.Highlight = false;
            base.Count = 0;
            base.NotifyActivating();

            bool oddHand = handBefore % 2 == 1;
            if (oddHand)
            {
                foreach (BattleAction action in this.LoseRandomMana())
                    yield return action;
            }
            else
            {
                List<Card> hand = new List<Card>();
                foreach (Card c in base.Battle.HandZone)
                {
                    if (c != null)
                        hand.Add(c);
                }
                if (hand.Count > 0)
                {
                    Card discard = hand[base.GameRun.BattleRng.NextInt(0, hand.Count)];
                    yield return new DiscardAction(discard);
                }
            }
        }

        private IEnumerable<BattleAction> LoseRandomMana()
        {
            // 按当前每一滴费用等概率抽一点，而不是在“有的颜色”里等概率抽一种
            ManaGroup pool = base.Battle.BattleMana;
            if (pool.Amount <= 0)
                yield break;

            int roll = base.GameRun.BattleRng.NextInt(0, pool.Amount);
            ManaColor[] order = new ManaColor[]
            {
                ManaColor.White, ManaColor.Blue, ManaColor.Black, ManaColor.Red,
                ManaColor.Green, ManaColor.Colorless, ManaColor.Philosophy,
            };
            ManaColor pick = ManaColor.White;
            bool found = false;
            foreach (ManaColor color in order)
            {
                int amount = pool.GetValue(color);
                if (amount <= 0)
                    continue;
                if (roll < amount)
                {
                    pick = color;
                    found = true;
                    break;
                }
                roll -= amount;
            }
            if (!found)
                yield break;
            yield return new LoseManaAction(ManaGroup.FromColor(pick, 1));
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
            // 原 9/8/7，改为层数+1：第 10/9/8 张触发
            switch (difficulty)
            {
                case GameDifficulty.Lunatic: return 8;
                case GameDifficulty.Hard: return 9;
                default: return 10;
            }
        }
    }
}
