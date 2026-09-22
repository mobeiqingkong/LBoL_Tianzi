using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader;

namespace TianziMod.Cards.Template
{
    public static class CardIndexGenerator
    {
        private static readonly List<ManaColor> offColors = BepinexPlugin.offColors;
        private static int? initial_offset = null;
        private static HashSet<int> uniqueIds = new HashSet<int>() { };

        public const int milx1 = (int)1E7;

        public static HashSet<int> UniqueIds
        {
            get
            {
                if (uniqueIds == null)
                    uniqueIds = new HashSet<int>();
                return uniqueIds;
            }
        }

        internal static void PromiseClearIndexSet() =>
            EntityManager.AddPostLoadAction(() => uniqueIds = null);

        public static int Initial_offset
        {
            get
            {
                if (initial_offset == null)
                {
                    int millions = 0;
                    if (
                        UniqueTracker.Instance.configIndexes.TryGetValue(
                            typeof(CardConfig),
                            out var indexSet
                        )
                    )
                    {
                        millions = indexSet.Where(i => i >= milx1).DefaultIfEmpty().Max() / milx1;
                    }
                    millions += 1;
                    initial_offset = millions * milx1;
                }
                return initial_offset.Value;
            }
        }

        public static int GetUniqueIndex(CardConfig config)
        {
            int id = Initial_offset;

            //Off-color check
            id += config.Colors.Any(x => offColors.Any(y => y == x)) ? 1000000 : 0;

            //Rarity
            id += config.Keywords.HasFlag(LBoL.Base.Keyword.Basic)
                ? 0
                : (int)(config.Rarity + 1) * 100000;

            //Color（无色/空 Colors 的状态牌等）
            int color =
                config.Colors == null || config.Colors.Count == 0
                    ? 0
                    : (config.Colors.Count > 1 ? 9 : (int)config.Colors[0]);
            id += color * 10000;

            //Cost
            int cost =
                (
                    config.IsXCost
                    || config.Keywords.HasFlag(LBoL.Base.Keyword.Forbidden)
                    || config.Cost.Total > 9
                )
                    ? 9
                    : config.Cost.Total;
            id += cost * 1000;

            //Type
            id += (int)config.Type * 100;

            //Cards with similar parameters
            if (
                UniqueTracker.Instance.configIndexes.TryGetValue(
                    typeof(CardConfig),
                    out var indexSet
                )
            )
                while (indexSet.Contains(id))
                    id++;

            return id;
        }
    }
}
