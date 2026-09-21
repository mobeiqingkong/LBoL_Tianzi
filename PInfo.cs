using HarmonyLib;

namespace TianziMod
{
    public static class PInfo
    {
        //Rename the variable below to prevent conflicts between mod.
        public const string GUID = "author.game.typeofmod.TianziCharacter";
        public const string Name = "TianziMod";
        public const string version = "0.1.0";
        public static readonly Harmony harmony = new Harmony(GUID);
    }
}
