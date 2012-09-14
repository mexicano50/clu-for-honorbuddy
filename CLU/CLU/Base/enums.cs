#region Revision info
/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion

namespace CLU.Base
{
    using System;

    public enum TrinketUsage {
        Never,
        OnCooldown,
        OnCooldownInCombat,
        LowPower,
        LowHealth,
        CrowdControlled,
        CrowdControlledSilenced
    }

    public enum PetSlot {
        FirstSlot = 1,
        SecondSlot,
        ThirdSlot,
        FourthSlot,
        FifthSlot
    }

    public enum Burst {
        onBoss,
        onMob
    }

    public enum DeathKnightTierOneTalent {
        PlagueLeech,
        UnholyBlight,
        RoilingBlood,
        None
    }

    public enum HealingAquisitionMethod {
        Proximity,
        RaidParty
    }


    public enum PaladinBlessing {
        Kings,
        Might
    }

    public enum MonkLegacy {
        Tiger,
        Emperor
    }

    public enum WarriorShout {
        Battle,
        Commanding
    }

    public enum DruidForm {
        None,
        Bear,
        Cat,
        Moonkin
    }


    public enum ShadowPriestRotation {
        Leveling,
        Default,
    }

    public enum SubtletyRogueRotation {
        Default,
        ImprovedTestVersion
    }

    public enum ChakraStance {
        None,
        Serenity,
        Sanctuary
    }

    public enum MHPoisonType {
        Wound,
        Deadly
    }

    public enum OHPoisonType
    {
        MindNumbing,
        Crippling,
        Leeching,
        Paralytic
       
    }

    public enum PetType {
        // These are CreatureFamily IDs. See 'CurrentPet' for usage.
        None = 0,
        Imp = 23,
        Felguard = 29,
        Voidwalker = 16,
        Felhunter = 15,
        Succubus = 17,
    }

    public enum OracleWatchMode {
        Healer,
        DPS,
        Tank
    }

    public enum GroupType {
        Solo,
        Party,
        Raid
    }

    public enum GroupLogic {
        PVE,
        Battleground,
        Solo
    }

    [Flags]
    public enum TargetFilter {
        None = 0,
        Tanks = 1,
        Healers = 2,
        Damage = 4,
        FlagCarrier = 5,
        EnemyHealers = 6,
        Threats = 7,
        LowHealth = 8,
        MostFocused = 9
    }

    public enum Keyboardfunctions {
        Nothing,                // - default
        IsAltKeyDown,           // - Returns whether an Alt key on the keyboard is held down.
        IsControlKeyDown,       // - Returns whether a Control key on the keyboard is held down
        IsLeftAltKeyDown,       // - Returns whether the left Alt key is currently held down
        IsLeftControlKeyDown,   // - Returns whether the left Control key is held down
        IsLeftShiftKeyDown,     // - Returns whether the left Shift key on the keyboard is held down
        IsModifierKeyDown,      // - Returns whether a modifier key is held down
        IsRightAltKeyDown,      // - Returns whether the right Alt key is currently held down
        IsRightControlKeyDown,  // - Returns whether the right Control key on the keyboard is held down
        IsRightShiftKeyDown,    // - Returns whether the right shift key on the keyboard is held down
        IsShiftKeyDown,         // - Returns whether a Shift key on the keyboard is held down
    }

    public enum FeralSymbiosis
    {
        None = 0,
        DeathCoil = 1,                       // DK -> Feral Druid
        PlayDead = 2,                        // Hunter -> Feral Duid
        FrostNova = 3,                       // Mage -> Feral Druid
        Clash = 4,                           // Monk -> Feral Druid
        DivineShield = 5,                    // Paladin -> Feral Druid
        Dispersion = 6,                      // Priest -> Feral Druid
        Redirect = 7,                        // Rogue -> Feral Druid
        FeralSpirit = 8,                     // Shaman -> Feral Druid
        SoulSwap = 9,                        // Warlock -> Feral Druid
        ShatteringBlow = 10,                 // Warrior -> Feral Druid
    }

    public enum GuardianSymbiosis
    {
        None = 0,
        BoneShield = 1,                          // DK -> Guardian Druid
        IceTrap = 2,                             // Hunter -> Guardian Duid
        MageWard = 3,                            // Mage -> Guardian Druid
        ElusiveBrew = 4,                         // Monk -> Guardian Druid
        Consecration = 5,                        // Paladin -> Guardian Druid
        FearWard = 6,                            // Priest -> Guardian Druid
        Feint = 7,                               // Rogue -> Guardian Druid
        LightningShield = 8,                     // Shaman -> Guardian Druid
        LifeTap = 9,                             // Warlock -> Guardian Druid
        SpellReflection = 10,                    // Warrior -> Feral Druid
    }
}
