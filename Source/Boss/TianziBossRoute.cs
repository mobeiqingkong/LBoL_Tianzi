using LBoL.Core;
using LBoL.Core.Units;
using TianziMod.Exhibits;

namespace TianziMod.Boss
{
    /// <summary>
    /// 第一幕自订天子 Boss 掉落中选了仙桃/绯想之剑后，
    /// 第二幕强制为原版 Boss 组「Tianzi」；否则第二幕池排除原版天子。
    /// </summary>
    public static class TianziBossRoute
    {
        /// <summary>第一幕自订 Boss（掉落展品用）。</summary>
        public const string ChapterBossGroupId = "TianziChapterBoss";

        /// <summary>第二幕原版天子 Boss 组。</summary>
        public const string VanillaAct2BossGroupId = "Tianzi";

        public static bool AwaitingBossExhibitPick { get; set; }

        public static bool ShouldForceAct2Boss(GameRunController run)
        {
            PlayerUnit player = run == null ? null : run.Player;
            if (player == null)
                return false;
            if (player.Id == BepinexPlugin.modUniqueID)
                return false;
            return player.HasExhibit<TianziExhibitA>() || player.HasExhibit<TianziExhibitB>();
        }

        public static void ApplyAct2BossOverride(GameRunController run)
        {
            if (run == null || run.Stages == null)
                return;
            bool force = ShouldForceAct2Boss(run);
            foreach (Stage stage in run.Stages)
            {
                if (stage == null || stage.Level != 2)
                    continue;
                if (force)
                    ForceBoss(stage, VanillaAct2BossGroupId);
            }
        }

        public static void ForceBoss(Stage stage, string groupId)
        {
            if (stage == null || string.IsNullOrEmpty(groupId))
                return;
            EnemyGroupEntry entry = Library.GetEnemyGroupEntry(groupId);
            typeof(Stage).GetProperty(nameof(Stage.Boss))?.SetValue(stage, entry);
            typeof(Stage).GetProperty("SelectedBoss")?.SetValue(stage, groupId);
        }
    }
}
