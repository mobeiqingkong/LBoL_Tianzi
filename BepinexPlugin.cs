using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoL.Base;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using TianziMod.Cards.Template;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TianziMod
{
    [BepInPlugin(TianziMod.PInfo.GUID, TianziMod.PInfo.Name, TianziMod.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {
        //The Unique mod ID of the mod.
        //WARNING: It is mandatory to rename it to avoid issues.
        //注意：Sideloader 要求实体的 ID 必须与最终实体逻辑类的**类型名**完全一致。
        //游戏用 Library.CreatePlayerUnit(config.Id) -> TypeFactory<PlayerUnit>.CreateInstance(id) 查找，
        //查的是类型名。所以这里必须等于 Source/Player 里那个 PlayerUnit 子类的类名（TianziPlayer）。
        //（曾用 "TianziMod"，与类型名 TianziPlayer 不匹配 ->
        //  ArgumentException: Cannot create instance TianziMod of type PlayerUnit，角色不可选。）
        public static string modUniqueID = nameof(TianziPlayerDef.TianziPlayer);
        //Name of the character.
        //This is also the prefix that is used before every .png file in DirResources. 
        public static string playerName = "Tianzi";
        //Whether to use an ingame or custom model.
        //InGame: Will load the character model of the ingame character (Tenshi / 比那名居天子).
        //Custom: Will load DirResource/TianziModel.png 
        public static bool useInGameModel = true;
        //If InGame is selected, this is the model that will be loaded. 
        public static string modelName = nameof(LBoL.EntityLib.EnemyUnits.Character.Tianzi);
        //【必须为 false】模型是否左右镜像。
        // 经查游戏 resources.assets 的 ConfigData/UnitModelConfig 真值：
        //   Flip=false -> 玩家朝向（Reimu / Marisa / Sakuya / Cirno / Aya / Sanae / Junko ... 全部可玩角色）
        //   Flip=true  -> 敌人朝向（Alice / Youmu / Yuyuko / Eirin / Remilia ... 全部敌人）
        // 官方 "Tianzi" 这一行本身就是 Flip=false，且它是可玩角色取向。
        // 之前设成 true 会把它镜像成「敌人朝向」-> 人物动画左右反了。
        public static bool modelIsFlipped = false;
        //The character's off-color.
        //Used to separate cards in the card collection and put the off-color cards at the end.
        public static List<ManaColor> offColors = new List<ManaColor>()
        {
            ManaColor.Colorless,
            ManaColor.Blue,
            ManaColor.Green,
            ManaColor.Black,
        };

        private static readonly Harmony harmony = TianziMod.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        // add this for audio loading
        internal static DirectorySource directorySource = new DirectorySource(TianziMod.PInfo.GUID, "");

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            CardIndexGenerator.PromiseClearIndexSet();
            EntityManager.RegisterSelf();

            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }
    }
}
