using System.Collections.Generic;
using TianziMod.Cards;
using TianziMod.Cards.Template;
using TianziMod.Exhibits;
using TianziMod.TianziUlt;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace TianziMod
{
    public class TianziLoadouts
    {
        public static string UltimateSkillA = nameof(TianziUltA);
        public static string UltimateSkillB = nameof(TianziUltB);

        public static string ExhibitA = nameof(TianziExhibitA);
        public static string ExhibitB = nameof(TianziExhibitB);

        // A 模式的初始卡组：射击x2 结界x2 要石射击x2 要石护盾x2 要石浮游炮x1 = 9 张
        public static List<string> DeckA = new List<string>
        {
            nameof(Shoot),
            nameof(Shoot),
            nameof(Boundary),
            nameof(Boundary),
            nameof(TianziKeystoneShot),
            nameof(TianziKeystoneShot),
            nameof(TianziKeystoneGuard),
            nameof(TianziKeystoneGuard),
            nameof(TianziKeystoneTurret),
        };

        // B 模式的初始卡组：射击x2 结界x2 绯想剑斩击x2 绯想剑格挡x2 天穹斩x1 = 9 张
        public static List<string> DeckB = new List<string>
        {
            nameof(Shoot),
            nameof(Shoot),
            nameof(Boundary),
            nameof(Boundary),
            nameof(TianziScarletSlash),
            nameof(TianziScarletSlash),
            nameof(TianziScarletGuard),
            nameof(TianziScarletGuard),
            nameof(TianziSkyCleave),
        };

        public static PlayerUnitConfig playerUnitConfig = new PlayerUnitConfig(
            Id: BepinexPlugin.modUniqueID,
            HasHomeName: true,
            ShowOrder: 9,
            Order: 0,
            UnlockLevel: 0,
            ModleName: "",
            NarrativeColor: "#d7567b",
            IsSelectable: true,
            MaxHp: 65,
            InitialMana: new ManaGroup()
            {
                White = 2,
                Blue = 0,
                Black = 0,
                Red = 2,
                Green = 0,
                Colorless = 0,
                Philosophy = 0,
            },
            InitialMoney: 80,
            InitialPower: 0,
            BasicRingOrder: null,
            LeftColor: ManaColor.White,
            RightColor: ManaColor.Red,
            UltimateSkillA: TianziLoadouts.UltimateSkillA,
            UltimateSkillB: TianziLoadouts.UltimateSkillB,
            ExhibitA: TianziLoadouts.ExhibitA,
            ExhibitB: TianziLoadouts.ExhibitB,
            DeckA: TianziLoadouts.DeckA,
            DeckB: TianziLoadouts.DeckB,
            DifficultyA: 2,
            DifficultyB: 1
        );
    }
}
