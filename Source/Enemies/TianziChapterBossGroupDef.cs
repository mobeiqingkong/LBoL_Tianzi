using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader;
using TianziMod.Enemies.Template;
using static LBoLEntitySideloader.Entities.EnemyGroupTemplate;

namespace TianziMod.Enemies
{
    public sealed class TianziChapterBossGroupDef : TianziEnemyGroupTemplate
    {
        public override IdContainer GetId() => nameof(TianziChapterBoss);

        public override EnemyGroupConfig MakeConfig()
        {
            EnemyGroupConfig config = GetEnemyGroupDefaultConfig();
            config.Name = nameof(TianziChapterBoss);
            config.FormationName = VanillaFormations.Single;
            config.Enemies = new List<string>() { nameof(TianziChapterBoss) };
            config.EnemyType = EnemyType.Boss;
            config.RollBossExhibit = true;
            return config;
        }
    }
}
