using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.StatusEffects
{
    public sealed class TianziParityKwSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.IsVerbose = false;
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziParityKwSeDef))]
    public sealed class TianziParityKwSe : StatusEffect { }

    public sealed class TianziKarmaKwSeDef : TianziStatusEffectTemplate
    {
        public override StatusEffectConfig MakeConfig()
        {
            StatusEffectConfig config = GetDefaultStatusEffectConfig();
            config.Type = StatusEffectType.Positive;
            config.IsVerbose = false;
            config.HasLevel = false;
            config.IsStackable = false;
            return config;
        }
    }

    [EntityLogic(typeof(TianziKarmaKwSeDef))]
    public sealed class TianziKarmaKwSe : StatusEffect { }

    public sealed class TianziKarmaSeDef : TianziStatusEffectTemplate
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

    [EntityLogic(typeof(TianziKarmaSeDef))]
    public sealed class TianziKarmaSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            TianziKarma.ActiveCount += 1;
        }

        protected override void OnRemoving(Unit unit)
        {
            TianziKarma.ActiveCount -= 1;
        }
    }

    public static class TianziKarma
    {
        internal static int ActiveCount;

        public static bool Forced
        {
            get { return ActiveCount > 0; }
        }
    }
}
