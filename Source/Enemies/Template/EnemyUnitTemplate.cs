using System;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using TianziMod.Config;
using TianziMod.Localization;

namespace TianziMod.Enemies.Template
{
    public abstract class TianziEnemyUnitTemplate : EnemyUnitTemplate
    {
        public override IdContainer GetId()
        {
            return TianziDefaultConfig.DefaultID(this);
        }

        public override EnemyUnitConfig MakeConfig()
        {
            return TianziDefaultConfig.EnemyUnitDefaultConfig();
        }

        public override LocalizationOption LoadLocalization()
        {
            return TianziLocalization.EnemiesUnitBatchLoc.AddEntity(this);
        }

        public override Type TemplateType()
        {
            return typeof(EnemyUnitTemplate);
        }

        protected EnemyUnitConfig GetEnemyUnitDefaultConfig()
        {
            return TianziDefaultConfig.EnemyUnitDefaultConfig();
        }
    }
}
