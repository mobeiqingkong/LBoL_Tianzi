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

        /// <summary>当前手牌奇偶（卡还在手里时，用于描述预览）。</summary>
        public static bool IsOdd(BattleController battle)
        {
            return battle != null && battle.HandZone.Count % 2 == 1;
        }

        /// <summary>
        /// 打出结算时的奇偶：卡已离手，按打出前张数判定（即当前手牌数 + 1）。
        /// </summary>
        public static bool IsOddAtPlay(BattleController battle)
        {
            return battle != null && (battle.HandZone.Count + 1) % 2 == 1;
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
    //  天人的耐性：参考「保留格挡」，回合开始保留至多 10 点格挡；并获得灵力（最多 3）
    // ================================================================
    public sealed class TianziEnduranceSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.HasLevel = false;
            config.IsStackable = false;
            config.HasCount = true;
            config.RelativeEffects = new List<string>() { nameof(TurnStartDontLoseBlock) };
            return config;
        }
    }

    [EntityLogic(typeof(TianziEnduranceSeDef))]
    public sealed class TianziEnduranceSe : StatusEffect
    {
        private const int MaxKeepBlock = 10;

        protected override void OnAdded(Unit unit)
        {
            // 回合结束时挂上「保留格挡」，跳过下回合开始的清空
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
            // LoseBlockGraze 之后立刻把超额格挡削到 10
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting),
                GameEventPriority.Highest
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            if (base.Battle.Player.Block <= 0)
                yield break;
            TurnStartDontLoseBlock existing = base.Battle.Player.GetStatusEffect<TurnStartDontLoseBlock>();
            if (existing != null)
                yield break;
            yield return new ApplyStatusEffectAction<TurnStartDontLoseBlock>(
                base.Battle.Player, 1, null, null, null, 0.05f);
        }

        private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            int block = base.Battle.Player.Block;
            int excess = block - MaxKeepBlock;
            if (excess <= 0)
                yield break;
            base.NotifyActivating();
            yield return new LoseBlockShieldAction(base.Battle.Player, excess, 0);
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Count < 3)
            {
                base.Count += 1;
                yield return new ApplyStatusEffectAction<Spirit>(base.Battle.Player, 1, null, null, null, 0.1f);
            }
        }
    }

    // ================================================================
    //  清霖之愿：每回合前 {Level} 次（剩余 {Count}）抽到状态/厄运牌时放逐并抽 1
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
            config.HasCount = true;
            config.CountStackType = StackType.Keep;
            return config;
        }
    }

    [EntityLogic(typeof(TianziPureWishSeDef))]
    public sealed class TianziPureWishSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            if (base.Count <= 0)
                base.Count = base.Level > 0 ? base.Level : 1;
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
            base.ReactOwnerEvent<CardEventArgs>(
                base.Battle.CardDrawn,
                new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn)
            );
            // 必须比 AmuletForCard 更早入队，否则庇护扣完层后这里会读到 Level<=0
            base.ReactOwnerEvent<CardsEventArgs>(
                base.Battle.CardsAddedToDiscard,
                new EventSequencedReactor<CardsEventArgs>(this.OnAmuletCards),
                GameEventPriority.Highest);
            base.ReactOwnerEvent<CardsEventArgs>(
                base.Battle.CardsAddedToHand,
                new EventSequencedReactor<CardsEventArgs>(this.OnAmuletCards),
                GameEventPriority.Highest);
            base.ReactOwnerEvent<CardsAddingToDrawZoneEventArgs>(
                base.Battle.CardsAddedToDrawZone,
                new EventSequencedReactor<CardsAddingToDrawZoneEventArgs>(this.OnAmuletDrawZone),
                GameEventPriority.Highest);
            // 必须比 Amulet 更早：庇护 Cancel 之后再检查 IsCanceled 会直接跳过
            base.ReactOwnerEvent<StatusEffectApplyEventArgs>(
                unit.StatusEffectAdding,
                new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusAdding),
                GameEventPriority.Highest);
        }

        public override bool Stack(StatusEffect other)
        {
            bool handled = base.Stack(other);
            if (base.Count < base.Level)
                base.Count = base.Level;
            return handled;
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            base.Count = base.Level > 0 ? base.Level : 1;
            yield break;
        }

        private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Count <= 0)
                yield break;
            Card card = args.Card;
            if (card == null)
                yield break;
            if (card.CardType != CardType.Status && card.CardType != CardType.Misfortune)
                yield break;
            base.Count -= 1;
            base.NotifyActivating();
            yield return new ExileCardAction(card);
            yield return new DrawManyCardAction(1);
        }

        private IEnumerable<BattleAction> OnAmuletCards(CardsEventArgs args)
        {
            return this.TryAmuletTriggers(args.Cards);
        }

        private IEnumerable<BattleAction> OnAmuletDrawZone(CardsAddingToDrawZoneEventArgs args)
        {
            return this.TryAmuletTriggers(args.Cards);
        }

        private IEnumerable<BattleAction> TryAmuletTriggers(IEnumerable<Card> cards)
        {
            if (base.Battle.BattleShouldEnd || base.Count <= 0)
                yield break;
            AmuletForCard amulet = base.Owner.GetStatusEffect<AmuletForCard>();
            if (amulet == null || amulet.Level <= 0)
                yield break;
            int statusCount = 0;
            foreach (Card card in cards)
            {
                if (card != null && card.CardType == CardType.Status)
                    statusCount++;
            }
            // 与庇护实际会放逐的次数对齐
            int triggers = statusCount;
            if (triggers > amulet.Level)
                triggers = amulet.Level;
            if (triggers <= 0)
                yield break;
            while (triggers > 0 && base.Count > 0)
            {
                base.Count -= 1;
                triggers -= 1;
                base.NotifyActivating();
                yield return new DrawManyCardAction(1);
            }
        }

        private IEnumerable<BattleAction> OnStatusAdding(StatusEffectApplyEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Count <= 0)
                yield break;
            if (args.Effect == null || args.Effect.Type != StatusEffectType.Negative)
                yield break;
            // Highest 下先于庇护执行；此时尚未 Cancel，层数也还在
            if (args.IsCanceled)
                yield break;
            Amulet amulet = base.Owner.GetStatusEffect<Amulet>();
            if (amulet == null || amulet.Level <= 0)
                yield break;
            base.Count -= 1;
            base.NotifyActivating();
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
        public ManaGroup Mana
        {
            get { return new ManaGroup() { White = 1 }; }
        }

        protected override void OnAdded(Unit unit)
        {
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding)
            );
        }

        private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            base.NotifyActivating();
            yield return new GainManaAction(new ManaGroup() { White = 1 });
        }

        private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Battle.Player.IsExtraTurn)
                yield break;
            base.NotifyActivating();
            yield return new ApplyStatusEffectAction<AmuletForCard>(
                base.Battle.Player, 1, null, null, null, 0.1f);
        }
    }

    // ================================================================
    //  凡间之游：每打出 {Level} 张牌触发；Count = 还需打出几张
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
            config.HasCount = true;
            config.CountStackType = StackType.Keep;
            return config;
        }
    }

    [EntityLogic(typeof(TianziMortalJourneySeDef))]
    public sealed class TianziMortalJourneySe : StatusEffect
    {
        public ManaGroup Mana
        {
            get { return new ManaGroup() { White = 1 }; }
        }

        protected override void OnAdded(Unit unit)
        {
            if (base.Count <= 0)
                base.Count = Math.Max(base.Level, 1);
            base.ReactOwnerEvent<CardUsingEventArgs>(
                base.Battle.CardUsed,
                new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed)
            );
        }

        public override bool Stack(StatusEffect other)
        {
            bool handled = base.Stack(other);
            int need = Math.Max(base.Level, 1);
            if (base.Count <= 0 || base.Count > need)
                base.Count = need;
            return handled;
        }

        private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
            int need = Math.Max(base.Level, 1);
            base.Count -= 1;
            if (base.Count > 0)
                yield break;
            base.Count = need;
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
                base.Battle.Player.TurnEnding,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding)
            );
            base.ReactOwnerEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarting,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
        {
            base.Count += 1;
            yield break;
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
        {
            if (base.Battle.BattleShouldEnd)
                yield break;
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
