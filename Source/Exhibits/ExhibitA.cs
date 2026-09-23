using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Exhibits;
using LBoLEntitySideloader.Attributes;
using TianziMod.Enemies;
using TianziMod.StatusEffects;

namespace TianziMod.Exhibits
{
    public sealed class TianziExhibitADef : TianziExhibitTemplate
    {
        public override ExhibitConfig MakeConfig()
        {
            ExhibitConfig config = GetDefaultExhibitConfig();
            config.Mana = new ManaGroup() { White = 1 };
            config.BaseManaColor = ManaColor.White;
            config.BaseManaAmount = 1;
            config.Value1 = 2;
            config.Value2 = 2;
            config.HasCounter = true;
            config.InitialCounter = 7;
            config.Keywords = Keyword.None;
            // Boss 专属展品：Owner 必须与 EnemyGroup/Unit Id 一致才能进 Boss 展品槽
            config.Owner = nameof(TianziChapterBoss);
            config.RelativeEffects = new List<string>() { nameof(TianziTempHpSe) };
            return config;
        }
    }

    /// <summary>
    /// 仙桃（光耀）【A】
    /// 提供 1 点白色费用。每场战斗第一回合开始时多抽 1 张牌，并额外获得 2 点白色法力。
    /// 每次受到影响生命值的任意伤害时恢复 2 点生命值并获得 2 点 p点，一场战斗最多 7 次。
    /// </summary>
    [EntityLogic(typeof(TianziExhibitADef))]
    public sealed class TianziExhibitA : ShiningExhibit
    {
        private const int MaxTriggers = 7;

        public ManaGroup Mana2
        {
            get { return new ManaGroup() { White = 2 }; }
        }

        protected override void OnEnterBattle()
        {
            base.Counter = MaxTriggers;
            base.ReactBattleEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
            base.ReactBattleEvent<DamageEventArgs>(
                base.Battle.Player.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived)
            );
            base.HandleBattleEvent<GameEventArgs>(
                base.Battle.BattleEnded,
                new GameEventHandler<GameEventArgs>(this.OnBattleEnded)
            );
        }

        private void OnBattleEnded(GameEventArgs args)
        {
            base.Counter = MaxTriggers;
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.Player.TurnCounter != 1)
                yield break;
            base.NotifyActivating();
            yield return new DrawManyCardAction(1);
            yield return new GainManaAction(this.Mana2);
        }

        private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || base.Counter <= 0)
                yield break;

            int damage = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (damage <= 0)
                yield break;

            base.Counter -= 1;
            base.NotifyActivating();
            yield return new HealAction(base.Owner, base.Owner, base.Value1, HealType.Normal, 0.1f);
            yield return new GainPowerAction(base.Value2);
        }
    }
}
