using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Utils;
using UnityEngine;
using TianziMod.Enemies;
using TianziMod.Localization;

namespace TianziMod.model
{
    /// <summary>
    /// 仅为 Boss 的 UnitNameTable 登记名字。战斗模型仍走 EnemyUnitConfig.ModleName = Tianzi。
    /// </summary>
    public sealed class TianziChapterBossModel : UnitModelTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(TianziChapterBoss);
        }

        public override LocalizationOption LoadLocalization()
        {
            // 与 TianziModel 同理：走批量发现，新增语言只丢 UnitModel<Locale>.yaml。
            return TianziLocalization.UnitModelBatchLoc.AddEntity(this);
        }

        public override ModelOption LoadModelOptions()
        {
            return new ModelOption(ResourcesHelper.LoadSpineUnitAsync("Tianzi"));
        }

        public override UniTask<Sprite> LoadSpellSprite()
        {
            return ResourcesHelper.LoadSpellPortraitAsync("Tianzi");
        }

        public override UnitModelConfig MakeConfig()
        {
            UnitModelConfig config = UnitModelConfig.FromName("Tianzi").Copy();
            config.Flip = true;
            return config;
        }
    }
}
