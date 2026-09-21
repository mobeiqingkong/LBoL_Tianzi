using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoL.Core.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using TianziMod.ImageLoader;
using TianziMod.Localization;
using UnityEngine;

namespace TianziMod
{
    public sealed class TianziPlayerDef : PlayerUnitTemplate
    {
        public UniTask<Sprite>? LoadSpellPortraitAsync { get; private set; }

        public override IdContainer GetId()
        {
            return BepinexPlugin.modUniqueID;
        }

        public override LocalizationOption LoadLocalization()
        {
            return TianziLocalization.PlayerUnitBatchLoc.AddEntity(this);
        }

        public override PlayerImages LoadPlayerImages()
        {
            return TianziImageLoader.LoadPlayerImages(BepinexPlugin.playerName);
        }

        public override PlayerUnitConfig MakeConfig()
        {
            return TianziLoadouts.playerUnitConfig;
        }

        [EntityLogic(typeof(TianziPlayerDef))]
        public sealed class TianziPlayer : PlayerUnit { }
    }
}
