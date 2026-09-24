using System.Collections.Generic;
using System.Reflection;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using TianziMod.Exhibits;

namespace TianziMod.Boss
{
    /// <summary>
    /// 第一幕自订天子 Boss 掉落中选了仙桃/绯想之剑后，
    /// 第二幕强制原版「Tianzi」，并在登场时说话；
    /// 否则第二幕池排除原版天子。
    /// 主角为模组天子时，第二幕一定不会遇到原版天子。
    /// </summary>
    public static class TianziBossRoute
    {
        public const string ChapterBossGroupId = "TianziChapterBoss";
        public const string VanillaAct2BossGroupId = "Tianzi";

        public static bool KeepsakeTaken { get; set; }
        public static bool AwaitingBossExhibitPick { get; set; }

        private static readonly PropertyInfo BossProp =
            typeof(Stage).GetProperty(nameof(Stage.Boss));
        private static readonly PropertyInfo SelectedBossProp =
            typeof(Stage).GetProperty("SelectedBoss");

        public static void ResetKeepsake()
        {
            KeepsakeTaken = false;
            AwaitingBossExhibitPick = false;
        }

        public static bool IsModTianziPlayer(GameRunController run)
        {
            PlayerUnit player = run == null ? null : run.Player;
            return player != null && player.Id == BepinexPlugin.modUniqueID;
        }

        public static bool ShouldForceAct2Boss(GameRunController run)
        {
            // 模组天子永不强制原版自己
            if (IsModTianziPlayer(run))
                return false;

            if (KeepsakeTaken)
                return true;

            PlayerUnit player = run == null ? null : run.Player;
            if (player == null)
                return false;
            return player.HasExhibit<TianziExhibitA>() || player.HasExhibit<TianziExhibitB>();
        }

        public static void ApplyAct2BossOverride(GameRunController run)
        {
            if (run == null || run.Stages == null)
                return;

            if (IsModTianziPlayer(run))
            {
                EnsureNoVanillaTianziForModPlayer(run);
                return;
            }

            if (!ShouldForceAct2Boss(run))
                return;

            foreach (Stage stage in run.Stages)
            {
                if (stage != null && stage.Level == 2)
                    ForceBoss(stage, VanillaAct2BossGroupId);
            }
        }

        private static readonly MethodInfo EnumerateOpponentIdsMethod =
            typeof(Library).GetMethod(
                "EnumerateOpponentIds",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// 一面选 Boss 走 GetOpponentCandidates：列出所有序章对手再抽 3 个，
        /// 只排除与主角 Id 相同的单位。本模组 Boss 叫 TianziChapterBoss，对不上，要在这里拿掉并补一位。
        /// </summary>
        public static EnemyUnit[] FilterAct1Choices(GameRunController run, EnemyUnit[] opponents)
        {
            if (opponents == null || !IsModTianziPlayer(run))
                return opponents;

            bool hit = false;
            for (int i = 0; i < opponents.Length; i++)
            {
                if (IsChapterBoss(opponents[i]))
                {
                    hit = true;
                    break;
                }
            }
            if (!hit)
                return opponents;

            List<EnemyUnit> kept = new List<EnemyUnit>();
            HashSet<string> used = new HashSet<string> { ChapterBossGroupId };
            string playerId = run.Player.Id;
            for (int i = 0; i < opponents.Length; i++)
            {
                EnemyUnit enemy = opponents[i];
                if (IsChapterBoss(enemy))
                    continue;
                kept.Add(enemy);
                if (enemy != null)
                    used.Add(enemy.Id);
            }

            IEnumerable<string> ids = EnumerateOpponentIdsMethod == null
                ? null
                : EnumerateOpponentIdsMethod.Invoke(null, null) as IEnumerable<string>;
            if (ids != null)
            {
                foreach (string id in ids)
                {
                    if (kept.Count >= opponents.Length)
                        break;
                    if (id == ChapterBossGroupId || id == playerId || !used.Add(id))
                        continue;
                    EnemyUnit extra = Library.CreateEnemyUnit(id);
                    if (extra != null)
                        kept.Add(extra);
                }
            }

            return kept.ToArray();
        }

        private static bool IsChapterBoss(EnemyUnit enemy)
        {
            return enemy != null && (enemy.Id == ChapterBossGroupId
                || (enemy.Config != null && enemy.Config.Id == ChapterBossGroupId));
        }

        /// <summary>模组天子选一面 Boss 时，池中不出现本模组 Boss。</summary>
        public static void EnsureNoModBossForModPlayer(Stage stage)
        {
            if (stage == null || stage.Level != 1 || !IsModTianziPlayer(stage.GameRun))
                return;

            RemoveBossFromPool(stage.BossPool, ChapterBossGroupId);
            bool picked = (stage.Boss != null && stage.Boss.Id == ChapterBossGroupId)
                || stage.SelectedBoss == ChapterBossGroupId;
            if (picked)
                RerollBoss(stage, stage.GameRun, ChapterBossGroupId);
        }

        /// <summary>模组天子：二幕若抽到原版天子则重抽。</summary>
        public static void EnsureNoVanillaTianziForModPlayer(GameRunController run)
        {
            if (run == null || run.Stages == null || !IsModTianziPlayer(run))
                return;

            foreach (Stage stage in run.Stages)
            {
                if (stage == null || stage.Level != 2)
                    continue;

                RemoveBossFromPool(stage.BossPool, VanillaAct2BossGroupId);
                if (stage.Boss != null && stage.Boss.Id == VanillaAct2BossGroupId)
                    RerollBoss(stage, run, VanillaAct2BossGroupId);
            }
        }

        public static void ForceBoss(Stage stage, string groupId)
        {
            if (stage == null || string.IsNullOrEmpty(groupId))
                return;
            EnemyGroupEntry entry = Library.GetEnemyGroupEntry(groupId);
            BossProp?.SetValue(stage, entry);
            SelectedBossProp?.SetValue(stage, groupId);
        }

        public static void RemoveBossFromPool(object pool, string groupId)
        {
            if (pool == null)
                return;
            MethodInfo remove = pool.GetType().GetMethod(
                "Remove",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);
            remove?.Invoke(pool, new object[] { groupId });
        }

        private static void RerollBoss(Stage stage, GameRunController run, string excludeId)
        {
            RepeatableRandomPool<string> pool = stage.BossPool as RepeatableRandomPool<string>;
            if (pool == null)
                return;
            RemoveBossFromPool(pool, excludeId);
            RandomGen rng = run == null ? null : (run.RootRng ?? run.StationRng);
            if (rng == null)
                return;
            string id = pool.SampleOrDefault(rng);
            if (!string.IsNullOrEmpty(id) && id != excludeId)
                ForceBoss(stage, id);
        }
    }
}
