using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Utils;
using UnityEngine;
using TianziMod.Localization;

namespace TianziMod.model
{
    public sealed class TianziModel : UnitModelTemplate
    {
        // 是否使用游戏内置模型（比那名居天子）。
        // InGame: 加载游戏内 EnemyUnits.Character.Tianzi 的 Spine 骨骼。
        // Custom: 加载 DirResources/TianziModel.png（静态图，无动画）。
        public static bool useInGameModel = BepinexPlugin.useInGameModel;
        public static string model_name = useInGameModel ? BepinexPlugin.modelName : "TianziModel.png";
        // 使用自定模型时，符卡立绘用这张图。
        public static string spellsprite_name = "TianziStand.png";

        public override IdContainer GetId()
        {
            return BepinexPlugin.modUniqueID;
        }

        public override LocalizationOption LoadLocalization()
        {
            // UnitModel 没有 TypeFactory，不能走 BatchLocalization（factoryType=null 会刷
            // FillLocalizationTables was given a null facType）。直接用 LocalizationFiles 写入 UnitNameTable。
            LocalizationFiles files = new LocalizationFiles(BepinexPlugin.directorySource);
            files.AddLocaleFile(Locale.ZhHans, TianziLocalization.UnitModel + "ZhHans");
            files.fallbackLoc = Locale.ZhHans;
            return files;
        }

        public override ModelOption LoadModelOptions()
        {
            if (useInGameModel)
            {
                //Load the character's spine.
                return new ModelOption(ResourcesHelper.LoadSpineUnitAsync(model_name));
            }
            else
            {
                //Load the custom character's sprite.
                return new ModelOption(
                    ResourceLoader.LoadSpriteAsync(model_name, BepinexPlugin.directorySource, ppu: 565)
                );
            }
        }

        public override UniTask<Sprite> LoadSpellSprite()
        {
            if (useInGameModel)
            {
                //Load the ingame character's portrait for the Ultimate.
                return ResourcesHelper.LoadSpellPortraitAsync(model_name);
            }
            else
            {
                //Load the custom character's portrait.
                return ResourceLoader.LoadSpriteAsync(spellsprite_name, BepinexPlugin.directorySource);
            }
        }

        public override UnitModelConfig MakeConfig()
        {
            if (useInGameModel)
            {
                UnitModelConfig config = UnitModelConfig.FromName(model_name).Copy();
                //Flipping the model is only necessary for enemy portraits. 
                config.Flip = BepinexPlugin.modelIsFlipped;
                return config;
            }
            else
            {
                UnitModelConfig config = DefaultConfig().Copy();
                config.Flip = false;
                config.Type = 0;
                config.Offset = new Vector2(0, -0.10f);
                config.HasSpellPortrait = true;
                return config;
            }
        }
    }
}
