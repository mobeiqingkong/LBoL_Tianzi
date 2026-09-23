using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using TianziMod.Enemies.Template;
using TianziMod.GunName;

namespace TianziMod.Enemies
{
    public sealed class TianziChapterBossDef : TianziEnemyUnitTemplate
    {
        static TianziChapterBossDef()
        {
            // 一幕 Boss 节点图标：复用原版二幕天子（Tianzi）的 BossIcon
            EnemyUnitTemplate.AddBossNodeIcon(
                nameof(TianziChapterBoss),
                () => ResourcesHelper.TryGetBossIcon("Tianzi"));
        }

        public override IdContainer GetId() => nameof(TianziChapterBoss);

        public override EnemyUnitConfig MakeConfig()
        {
            EnemyUnitConfig config = GetEnemyUnitDefaultConfig();
            config.IsPreludeOpponent = BepinexPlugin.enableAct1Boss.Value;
            config.BaseManaColor = new List<ManaColor>() { ManaColor.White, ManaColor.Red };
            config.Type = EnemyType.Boss;
            config.ModleName = "Tianzi";
            config.MaxHp = 240;
            config.MaxHpHard = 250;
            config.MaxHpLunatic = 260;
            config.Damage1 = 5;
            config.Damage1Hard = 5;
            config.Damage1Lunatic = 6;
            config.Damage2 = 7;
            config.Damage2Hard = 7;
            config.Damage2Lunatic = 8;
            config.Damage3 = 10;
            config.Damage3Hard = 12;
            config.Damage3Lunatic = 14;
            config.Damage4 = 30;
            config.Damage4Hard = 30;
            config.Damage4Lunatic = 35;
            config.Defend = 6;
            config.DefendHard = 8;
            config.DefendLunatic = 10;
            config.Count1 = 3;
            config.Count1Hard = 3;
            config.Count1Lunatic = 3;
            config.Count2 = 2;
            config.Count2Hard = 2;
            config.Count2Lunatic = 2;
            config.PowerLoot = new MinMax(100, 100);
            config.Gun1 = new List<string> { GunNameID.GetGunFromId(4122) };
            config.Gun2 = new List<string> { GunNameID.GetGunFromId(4121) };
            config.Gun3 = new List<string> { GunNameID.GetGunFromId(7300) };
            config.Gun4 = new List<string> { GunNameID.GetGunFromId(511) };
            return config;
        }
    }
}
