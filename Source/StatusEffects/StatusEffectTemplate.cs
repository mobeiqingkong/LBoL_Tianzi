using TianziMod.Config;
using TianziMod.ImageLoader;
using TianziMod.Localization;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using UnityEngine;

namespace TianziMod.StatusEffects
{
    public class TianziStatusEffectTemplate : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return TianziDefaultConfig.DefaultID(this);
        }

        public override LocalizationOption LoadLocalization()
        {
            return TianziLocalization.StatusEffectsBatchLoc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return TianziImageLoader.LoadStatusEffectLoader(status: this);
        }

        public override StatusEffectConfig MakeConfig()
        {
            return GetDefaultStatusEffectConfig();
        }

        public static StatusEffectConfig GetDefaultStatusEffectConfig()
        {
            return TianziDefaultConfig.DefaultStatusEffectConfig();
        }
    }
}
