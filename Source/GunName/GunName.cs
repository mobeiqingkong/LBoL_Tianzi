using System.Collections.Generic;
using System.Linq;
using LBoL.ConfigData;

namespace TianziMod.GunName
{
    public static class GunNameID
    {
        private static IReadOnlyList<GunConfig> gunConfig = GunConfig.AllConfig();

        //*****************************************
        //To get a list of all the in-game gun IDs:
        //***************************************** 
        // 1) Install debug mode
        // 2) Start an enemy encounter
        // 3) Click F2
        // 4) Select "gun test"
        // 5) This will open a menu with all the gun options in the game. The IDs are located on the left of the gun names.

        // 离线核对办法（不必开游戏）：
        //   用 UnityPy 解开 LBoL_Data/resources.assets，取出名为 UnitModelConfig / GunConfig
        //   的 TextAsset，按 BinaryReader 格式解析（string = 1字节标记 + 7bit长度 + UTF8）。
        //   本项目工作区脚本：_gamedata/*.bin + _cfgparse.py。
        // ⚠ 旧模板里的 GetGunFromId(800) 在 1.8.0 里 **不存在**（会打 "id: 800 doesn't exist" 日志）。
        public static string GetGunFromId(int id)
        {
            string gun_name = "";
            try
            {
                gun_name = (from config in gunConfig
                            where config.Id == id
                            select config.Name).ToList<string>()[0];
            }
            catch
            {
                UnityEngine.Debug.Log("[TianziMod] GunName id: " + id + " doesn't exist, fallback to Instant.");
                gun_name = "Instant";
            }
            return gun_name;
        }

        // ---------------------------------------------------------------
        //  枪械表：ID 全部经 resources.assets/ConfigData/GunConfig 核对存在
        // ---------------------------------------------------------------
        /// <summary>通用 / 兜底</summary>
        public static string Instant => "Instant";
        /// <summary>普通弹幕（札弹射击 11050）</summary>
        public static string Basic { get { return GetGunFromId(11050); } }
        /// <summary>要石 / 岩石类冲击（冰封球 950）</summary>
        public static string Rock { get { return GetGunFromId(950); } }
        /// <summary>斩击（离剑之见 13010）</summary>
        public static string Slash { get { return GetGunFromId(13010); } }
        /// <summary>剑光（离剑之见B 13011）</summary>
        public static string Sword { get { return GetGunFromId(13011); } }
        /// <summary>重斩（结界猛击2 39072）</summary>
        public static string HeavySlash { get { return GetGunFromId(39072); } }
        /// <summary>光弹（光辉宝枪 23061）</summary>
        public static string Light { get { return GetGunFromId(23061); } }
        /// <summary>天候 / 气象（星光台风 12240）</summary>
        public static string Weather { get { return GetGunFromId(12240); } }
        /// <summary>地震 / 天崩（龙神电光 25000）</summary>
        public static string Quake { get { return GetGunFromId(25000); } }
        /// <summary>大型弹（低音的雷鼓B 23052）</summary>
        public static string BigShot { get { return GetGunFromId(23052); } }
        /// <summary>散弹（退魔符乱舞 39080）</summary>
        public static string Spread { get { return GetGunFromId(39080); } }
        /// <summary>元素弹（Sacrifice 862）</summary>
        public static string Element { get { return GetGunFromId(862); } }
        /// <summary>红色剑气（天子专属射击1 4121）</summary>
        public static string RedAura { get { return GetGunFromId(4121); } }
        /// <summary>紫色剑气（天子专属射击2 4122）</summary>
        public static string VioletAura { get { return GetGunFromId(4122); } }
        /// <summary>金色剑气（光辉宝枪B 23062）</summary>
        public static string GoldAura { get { return GetGunFromId(23062); } }
        /// <summary>高天原之光 / 符卡（天子专属 TenshiSpell1 511）</summary>
        public static string Heaven { get { return GetGunFromId(511); } }
    }
}
