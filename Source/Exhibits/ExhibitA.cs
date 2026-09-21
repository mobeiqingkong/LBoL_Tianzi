using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Exhibits;
using LBoLEntitySideloader.Attributes;
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
            config.Keywords = Keyword.None;
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
        private int _triggers;
        private const int MaxTriggers = 7;

        protected override void OnEnterBattle()
        {
            this._triggers = 0;
            base.ReactBattleEvent<UnitEventArgs>(
                base.Battle.Player.TurnStarted,
                new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted)
            );
            base.ReactBattleEvent<DamageEventArgs>(
                base.Battle.Player.DamageReceived,
                new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived)
            );
        }

        private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
        {
            if (base.Battle.Player.TurnCounter != 1)
                yield break;
            base.NotifyActivating();
            yield return new DrawManyCardAction(1);
            yield return new GainTurnManaAction(new ManaGroup() { White = 2 });
        }

        private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
        {
            if (base.Battle.BattleShouldEnd || this._triggers >= MaxTriggers)
                yield break;

            int damage = (int)Math.Round(args.DamageInfo.Damage, MidpointRounding.AwayFromZero);
            if (damage <= 0)
                yield break;

            this._triggers += 1;
            base.NotifyActivating();
            yield return new HealAction(base.Owner, base.Owner, base.Value1, HealType.Normal, 0.1f);
            yield return new GainPowerAction(base.Value2);
        }
    }
}
