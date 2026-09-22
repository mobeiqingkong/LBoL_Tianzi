namespace TianziMod.Boss
{
    /// <summary>
    /// 第一章击败天子并拾取 A/B 展品后，第二章 Boss 固定为天子；
    /// 未拾取则从 Boss 池排除。
    /// </summary>
    public static class TianziBossRoute
    {
        public const string BossGroupId = "TianziChapterBossGroup";
        public const string BossUnitId = "TianziChapterBoss";

        public static bool KeepsakeTaken { get; set; }

        /// <summary>刚生成 Boss 奖励、等待玩家挑选展品。</summary>
        public static bool AwaitingBossExhibitPick { get; set; }

        public static void ResetKeepsake()
        {
            KeepsakeTaken = false;
            AwaitingBossExhibitPick = false;
        }
    }
}
