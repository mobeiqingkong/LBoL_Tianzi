using TianziMod.Config;
using TianziMod.ImageLoader;
using TianziMod.Localization;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;

namespace TianziMod.Cards.Template
{
    public abstract class TianziCardTemplate : CardTemplate
    {
        public override IdContainer GetId()
        {
            return TianziDefaultConfig.DefaultID(this);
        }

        public override CardImages LoadCardImages()
        {
            return TianziImageLoader.LoadCardImages(this);
        }

        public override LocalizationOption LoadLocalization()
        {
            return TianziLocalization.CardsBatchLoc.AddEntity(this);
        }

        public CardConfig GetDefaultCardConfig()
        {
            return TianziDefaultConfig.DefaultCardConfig();
        }
    }
}
