using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.StatusEffects
{
    // ================================================================
    //  天人之气：手牌数量「奇数 / 偶数」的分支效果改为由玩家选择触发
    // ================================================================
    public sealed class TianziOddEvenSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziOddEvenSeDef))]
    public sealed class TianziOddEvenSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            TianziParity.ActiveCount += 1;
        }

        protected override void OnRemoving(Unit unit)
        {
            TianziParity.ActiveCount -= 1;
        }
    }

    /// <summary>奇偶分支的统一判定入口。</summary>
    public static class TianziParity
    {
        internal static int ActiveCount;

        /// <summary>天人之气生效中：奇偶分支交给玩家选。</summary>
        public static bool Forced { get { return ActiveCount > 0; } }

        /// <summary>只按手牌奇偶判定。</summary>
        public static bool IsOdd(BattleController battle)
        {
            return battle != null && battle.HandZone.Count % 2 == 1;
        }
    }

    // ================================================================
    //  冥想：每回合多抽一张牌；回合开始时把手牌与弃牌堆各选一张互换
    // ================================================================
    public sealed class TianziMeditationSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziMeditationSeDef))]
    public sealed class TianziMeditationSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;

            base.NotifyActivating();
            // 每回合多抽一张牌
            yield return new DrawManyCardAction(1);
            if (base.Battle.BattleShouldEnd)
                yield break;

            List<Card> hand = base.Battle.HandZone.Where(c => c != null).ToList();
            List<Card> discard = base.Battle.DiscardZone.ToList();
            if (hand.Count == 0 || discard.Count == 0)
                yield break;

            SelectHandInteraction handPick = new SelectHandInteraction(1, 1, hand) { Source = this };
            yield return new InteractionAction(handPick, true);
            if (handPick.SelectedCards == null || handPick.SelectedCards.Count == 0)
                yield break;
            Card fromHand = handPick.SelectedCards[0];

            SelectCardInteraction discardPick =
                new SelectCardInteraction(1, 1, discard, SelectedCardHandling.DoNothing) { Source = this };
            yield return new InteractionAction(discardPick, true);
            if (discardPick.SelectedCards == null || discardPick.SelectedCards.Count == 0)
                yield break;
            Card fromDiscard = discardPick.SelectedCards[0];

            // 互换：手牌 → 弃牌堆，弃牌堆 → 手牌。
            // ⚠ MoveCardAction 的构造函数里就直接调用 BattleController.MoveCardCheck：
            //     dstZone == None / Draw、或 dstZone == card.Zone 都会当场 throw；
            //     另外 MoveCard 在 card.Zone == None 时还会 throw「Cannot move new card」。
            //   这个 throw 发生在动作队列的协程里，异常被 ActionResolver 吞掉、队列被截断，
            //   play-area 与手牌控件簿记就此错乱 -> 之后每一次出牌都抛 NullReferenceException
            //   -> 玩家看到的就是「一用就卡死」。
            //   两个交互之间隔了一次玩家操作，牌区可能被别的效果抢先改变，所以要自己守卫来源牌区。
            if (fromHand != null && fromDiscard != null
                && fromHand.Zone == CardZone.Hand && fromDiscard.Zone == CardZone.Discard)
            {
                yield return new MoveCardAction(fromHand, CardZone.Discard);
                yield return new MoveCardAction(fromDiscard, CardZone.Hand);
            }
        }
    }

    // ================================================================
    //  天人的耐性：回合开始只失去一半格挡；≤5 点未被格挡攻击伤害降为 1
    // ================================================================
    public sealed class TianziEnduranceSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziEnduranceSeDef))]
    public sealed class TianziEnduranceSe : StatusEffect
    {
        public const int Threshold = 5;

        private int _blockBeforeTurn;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
            base.HandleOwnerEvent<DamageEventArgs>(
                base.Battle.Player.DamageTaking,
                new GameEventHandler<DamageEventArgs>(this.OnDamageTaking)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
        {
            this._blockBeforeTurn = base.Battle.Player.Block;
            yield break;
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            int keep = this._blockBeforeTurn / 2;
            int now = base.Battle.Player.Block;
            if (keep > 0 && now < keep)
            {
                base.NotifyActivating();
                yield return new CastBlockShieldAction(
                    base.Battle.Player, base.Battle.Player, keep - now, 0, BlockShieldType.Direct, false);
            }
        }

        private void OnDamageTaking(DamageEventArgs args)
        {
            if (args.DamageInfo.DamageType != DamageType.Attack)
                return;
            int dmg = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (dmg < 2 || dmg > Threshold)
                return;
            base.NotifyActivating();
            args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(dmg - 1);
            args.AddModifier(this);
        }
    }

    // ================================================================
    //  清霖之愿：每回合前 {Level} 次抽到状态/厄运牌时，将其放逐并抽 1 张牌
    // ================================================================
    public sealed class TianziPureWishSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziPureWishSeDef))]
    public sealed class TianziPureWishSe : StatusEffect
    {
        private int _usedThisTurn;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
            base.ReactOwnerEvent<CardEventArgs>(
                base.Battle.CardDrawn,
                new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            this._usedThisTurn = 0;
            yield break;
        }

        private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || this._usedThisTurn >= base.Level)
                yield break;
            Card card = args.Card;
            if (card == null)
                yield break;
            if (card.CardType != CardType.Status && card.CardType != CardType.Misfortune)
                yield break;
            this._usedThisTurn += 1;
            base.NotifyActivating();
            yield return new ExileCardAction(card);
            yield return new DrawManyCardAction(1);
        }
    }

    // ================================================================
    //  天界之庇护：每回合结束时获得 1 层庇护，下回合获得 1 点白色法力
    // ================================================================
    public sealed class TianziHeavenShieldSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Add;
            config.IsStackable = true;
            config.RelativeEffects = new List<string>() { nameof(AmuletForCard) };
            return config;
        }
    }

    [EntityLogic(typeof(TianziHeavenShieldSeDef))]
    public sealed class TianziHeavenShieldSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new ApplyStatusEffectAction<AmuletForCard>(
                base.Battle.Player, base.Level, null, null, null, 0.1f);
            yield return new ApplyStatusEffectAction<TianziNextTurnManaSe>(
                base.Battle.Player, null, null, base.Level, null, 0.1f);
        }
    }

    // ================================================================
    //  凡间之游：每打出 {Level} 张牌，获得 1 点白色法力并抽 1 张牌
    // ================================================================
    public sealed class TianziMortalJourneySeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Min;
            config.IsStackable = true;
            return config;
        }
    }

    [EntityLogic(typeof(TianziMortalJourneySeDef))]
    public sealed class TianziMortalJourneySe : StatusEffect
    {
        private int _counter;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed)
            );
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            this._counter += 1;
            int need = Math.Max(base.Level, 1);
            if (this._counter < need)
                yield break;
            this._counter = 0;
            base.NotifyActivating();
            yield return new GainManaAction(new ManaGroup() { White = 1 });
            yield return new DrawManyCardAction(1);
        }
    }

    // ================================================================
    //  桃符「固若金汤的仙桃」：每 4 个回合触发一次 天衣无缝 + 临时生命值
    // ================================================================
    public sealed class TianziPeachTalismanSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = true;
            config.LevelStackType = StackType.Max;
            config.HasCount = true;
            config.CountStackType = StackType.Keep;
            config.IsStackable = true;
            config.RelativeEffects = new List<string>() { nameof(Invincible), nameof(TianziTempHpSe) };
            return config;
        }
    }

    [EntityLogic(typeof(TianziPeachTalismanSeDef))]
    public sealed class TianziPeachTalismanSe : StatusEffect
    {
        public const int Interval = 4;

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.Count += 1;
            if (base.Count < Interval)
                yield break;
            base.Count = 0;
            base.NotifyActivating();

            yield return new ApplyStatusEffectAction<Invincible>(
                base.Battle.Player, null, 1, null, null, 0.1f);

            int amount = Math.Max(base.Level, 1);
            BattleAction tempHp = TianziTempHp.GainAction(base.Battle.Player, amount, 0.1f);
            if (tempHp != null)
                yield return tempHp;
        }
    }
}
