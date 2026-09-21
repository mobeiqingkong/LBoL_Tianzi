using TianziMod.Config;
using TianziMod.ImageLoader;
using TianziMod.Localization;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using UnityEngine;

namespace TianziMod.TianziUlt
{
    public class TianziUltTemplate : UltimateSkillTemplate
    {
        public override IdContainer GetId()
        {
            return TianziDefaultConfig.DefaultID(this);
        }

        public override LocalizationOption LoadLocalization()
        {
            return TianziLocalization.UltimateSkillsBatchLoc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            return TianziImageLoader.LoadUltLoader(ult: this);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            throw new System.NotImplementedException();
        }

        public UltimateSkillConfig GetDefaultUltConfig()
        {
            return TianziDefaultConfig.DefaultUltConfig();
        }
    }
}
