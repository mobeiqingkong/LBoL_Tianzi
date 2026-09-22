using LBoL.ConfigData;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using TianziMod.Config;
namespace TianziMod.Enemies.Template
{
    public abstract class TianziEnemyGroupTemplate : EnemyGroupTemplate
    {
        public override IdContainer GetId()
        {
            return TianziDefaultConfig.DefaultID(this);
        }

        public override EnemyGroupConfig MakeConfig()
        {
            return TianziDefaultConfig.EnemyGroupDefaultConfig();
        }

        protected EnemyGroupConfig GetEnemyGroupDefaultConfig()
        {
            return TianziDefaultConfig.EnemyGroupDefaultConfig();
        }
    }
}
