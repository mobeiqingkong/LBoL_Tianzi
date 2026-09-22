using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoLEntitySideloader.Attributes;

namespace TianziMod.Exhibits
{
    public sealed class TianziBossKeepsakeDef : TianziExhibitTemplate
    {
        public override ExhibitConfig MakeConfig()
        {
            ExhibitConfig config = GetDefaultExhibitConfig();
            config.IsPooled = true;
            config.Appearance = AppearanceType.Nowhere;
            config.Rarity = Rarity.Shining;
            config.BaseManaColor = ManaColor.White;
            config.BaseManaAmount = 0;
            return config;
        }
    }

    /// <summary>第一章 Boss 掉落纪念品；拾取后第二章 Boss 必定为天子。</summary>
    [EntityLogic(typeof(TianziBossKeepsakeDef))]
    public sealed class TianziBossKeepsake : Exhibit
    {
    }
}
