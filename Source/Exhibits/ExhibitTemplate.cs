using TianziMod.Config;
using TianziMod.ImageLoader;
using TianziMod.Localization;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;

namespace TianziMod.Exhibits
{
    public class TianziExhibitTemplate : ExhibitTemplate
    {
        public override IdContainer GetId()
        {
            return TianziDefaultConfig.DefaultID(this);
        }

        public override LocalizationOption LoadLocalization()
        {
            return TianziLocalization.ExhibitsBatchLoc.AddEntity(this);
        }

        public override ExhibitSprites LoadSprite()
        {
            return TianziImageLoader.LoadExhibitSprite(exhibit: this);
        }

        public override ExhibitConfig MakeConfig()
        {
            return GetDefaultExhibitConfig();
        }

        public ExhibitConfig GetDefaultExhibitConfig()
        {
            return TianziDefaultConfig.DefaultExhibitConfig();
        }
    }
}
