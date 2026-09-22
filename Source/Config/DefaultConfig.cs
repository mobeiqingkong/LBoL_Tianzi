using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoLEntitySideloader.Entities;
using static LBoLEntitySideloader.Entities.EnemyGroupTemplate;
using TianziMod.GunName;
using UnityEngine;

namespace TianziMod.Config
{
    public sealed class TianziDefaultConfig
    {
        private static readonly string OwnerName = BepinexPlugin.modUniqueID;

        public static string DefaultID(EntityDefinition entity)
        {
            string IDdef = entity.GetType().Name;
            //Remove the Def at the end of the entity (class name) to get the ID. 
            string ID = IDdef.Remove(IDdef.Length - 3);
            return ID;
        }

        public static CardConfig DefaultCardConfig()
        {
            return new CardConfig(
               Index: 0,
               Id: "",
               Order: 10,
               AutoPerform: true,
               Perform: new string[0][],
               GunName: "",
               GunNameBurst: "",
               DebugLevel: 0,
               Revealable: false,

               IsPooled: true,
               FindInBattle: true,

               HideMesuem: false,
               IsUpgradable: true,
               Rarity: Rarity.Common,
               Type: CardType.Unknown,
               TargetType: null,
               Colors: new List<ManaColor>() { },
               IsXCost: false,
               Cost: new ManaGroup() { },
               UpgradedCost: null,
               MoneyCost: null,
               Damage: null,
               UpgradedDamage: null,
               Block: null,
               UpgradedBlock: null,
               Shield: null,
               UpgradedShield: null,
               Value1: null,
               UpgradedValue1: null,
               Value2: null,
               UpgradedValue2: null,
               Mana: null,
               UpgradedMana: null,
               Kicker: null,
               UpgradedKicker: null,
               Scry: null,
               UpgradedScry: null,

               ToolPlayableTimes: null,

               Loyalty: null,
               UpgradedLoyalty: null,
               PassiveCost: null,
               UpgradedPassiveCost: null,
               ActiveCost: null,
               UpgradedActiveCost: null,
               ActiveCost2: null,
               UpgradedActiveCost2: null,
               UltimateCost: null,
               UpgradedUltimateCost: null,

               Keywords: Keyword.None,
               UpgradedKeywords: Keyword.None,
               EmptyDescription: false,
               RelativeKeyword: Keyword.None,
               UpgradedRelativeKeyword: Keyword.None,

               RelativeEffects: new List<string>() { },
               UpgradedRelativeEffects: new List<string>() { },
               RelativeCards: new List<string>() { },
               UpgradedRelativeCards: new List<string>() { },

               Owner: OwnerName,
               Pack: null,
               ImageId: "",
               UpgradeImageId: "",

               Unfinished: false,
               Illustrator: null,
               SubIllustrator: new List<string>() { }
            );
        }

        public static ExhibitConfig DefaultExhibitConfig()
        {
            return new ExhibitConfig(
                Index: 0,
                Id: "",
                Order: 10,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Nowhere,
                Owner: OwnerName,
                LosableType: ExhibitLosableType.DebutLosable,
                Rarity: Rarity.Shining,
                Value1: null,
                Value2: null,
                Value3: null,
                Mana: new ManaGroup() { },
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.White,
                BaseManaAmount: 1,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
            );
        }

        public static StatusEffectConfig DefaultStatusEffectConfig()
        {
            return new StatusEffectConfig(
                Id: "",
                ImageId: null,
                Index: 0,
                Order: 10,
                Type: StatusEffectType.Positive,
                IsVerbose: false,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: StackType.Add,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default"
            );
        }

        public static EnemyUnitConfig EnemyUnitDefaultConfig()
        {
            return new EnemyUnitConfig(
                Id: "",
                RealName: false,
                OnlyLore: false,
                BaseManaColor: new List<ManaColor>() { ManaColor.White, ManaColor.Red },
                Order: 10,
                ModleName: BepinexPlugin.modelName,
                NarrativeColor: "#d7567b",
                Type: EnemyType.Boss,
                IsPreludeOpponent: false,
                HpLength: null,
                MaxHpAdd: null,
                MaxHp: 240,
                Damage1: 5,
                Damage2: 7,
                Damage3: 10,
                Damage4: 30,
                Power: 1,
                Defend: 6,
                Count1: 3,
                Count2: 2,
                MaxHpHard: 250,
                Damage1Hard: 5,
                Damage2Hard: 7,
                Damage3Hard: 12,
                Damage4Hard: 30,
                PowerHard: 1,
                DefendHard: 8,
                Count1Hard: 3,
                Count2Hard: 2,
                MaxHpLunatic: 260,
                Damage1Lunatic: 6,
                Damage2Lunatic: 8,
                Damage3Lunatic: 14,
                Damage4Lunatic: 35,
                PowerLunatic: 1,
                DefendLunatic: 10,
                Count1Lunatic: 3,
                Count2Lunatic: 2,
                PowerLoot: new MinMax(100, 100),
                BluePointLoot: new MinMax(0, 0),
                Gun1: new List<string> { GunNameID.GetGunFromId(4122) },
                Gun2: new List<string> { GunNameID.GetGunFromId(4121) },
                Gun3: new List<string> { GunNameID.GetGunFromId(7300) },
                Gun4: new List<string> { GunNameID.GetGunFromId(511) }
            );
        }

        public static EnemyGroupConfig EnemyGroupDefaultConfig()
        {
            return new EnemyGroupConfig(
                Id: "",
                Hidden: false,
                Environment: null,
                IsSub: false,
                Subs: new List<string>() { },
                Name: "",
                FormationName: VanillaFormations.Single,
                Enemies: new List<string>() { },
                EnemyType: EnemyType.Boss,
                DebutTime: 1f,
                RollBossExhibit: true,
                PlayerRoot: new Vector2(-4f, 0.5f),
                PreBattleDialogName: "",
                PostBattleDialogName: ""
            );
        }

        public static UltimateSkillConfig DefaultUltConfig()
        {
            return new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 100,
                PowerPerLevel: 100,
                MaxPowerLevel: 2,
                RepeatableType: UsRepeatableType.OncePerTurn,
                Damage: 1,
                Value1: 0,
                Value2: 0,
                Keywords: Keyword.Accuracy,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
            );
        }
    }
}
