using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;

namespace TianziMod.Localization
{
    public sealed class TianziLocalization
    {
        public static string Cards = "Cards";
        public static string Exhibits = "Exhibits";
        public static string PlayerUnit = "PlayerUnit";
        public static string UnitModel = "UnitModel";
        public static string UltimateSkills = "UltimateSkills";
        public static string StatusEffects = "StatusEffects";
        public static string EnemiesUnit = "EnemiesUnit";
        public static string EnemiesGroup = "EnemiesGroup";

        public static BatchLocalization CardsBatchLoc = new BatchLocalization(
            BepinexPlugin.directorySource,
            typeof(CardTemplate),
            Cards
        );
        public static BatchLocalization ExhibitsBatchLoc = new BatchLocalization(
            BepinexPlugin.directorySource,
            typeof(ExhibitTemplate),
            Exhibits
        );
        public static BatchLocalization PlayerUnitBatchLoc = new BatchLocalization(
            BepinexPlugin.directorySource,
            typeof(PlayerUnitTemplate),
            PlayerUnit
        );
        public static BatchLocalization UltimateSkillsBatchLoc = new BatchLocalization(
            BepinexPlugin.directorySource,
            typeof(UltimateSkillTemplate),
            UltimateSkills
        );
        public static BatchLocalization StatusEffectsBatchLoc = new BatchLocalization(
            BepinexPlugin.directorySource,
            typeof(StatusEffectTemplate),
            StatusEffects
        );
        public static BatchLocalization EnemiesUnitBatchLoc = new BatchLocalization(
            BepinexPlugin.directorySource,
            typeof(EnemyUnitTemplate),
            EnemiesUnit
        );

        public static void Init()
        {
            CardsBatchLoc.DiscoverAndLoadLocFiles(Cards);
            ExhibitsBatchLoc.DiscoverAndLoadLocFiles(Exhibits);
            PlayerUnitBatchLoc.DiscoverAndLoadLocFiles(PlayerUnit);
            UltimateSkillsBatchLoc.DiscoverAndLoadLocFiles(UltimateSkills);
            StatusEffectsBatchLoc.DiscoverAndLoadLocFiles(StatusEffects);
            EnemiesUnitBatchLoc.DiscoverAndLoadLocFiles(EnemiesUnit);
        }
    }
}
