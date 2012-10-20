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

namespace CLU.Lists
{
    using System.Collections.Generic;

    public static class MiscLists
    {

        internal static HashSet<string> Racials
        {
            get
            {
                return _racials;
            }
        }

        #region _racials
        private static readonly HashSet<string> _racials = new HashSet<string> {
            "Stoneform",
            // Activate to remove poison, disease, and bleed effects; +10% Armor; Lasts 8 seconds. 2 minute cooldown.
            "Escape Artist",
            // Escape the effects of any immobilization or movement speed reduction effect. Instant cast. 1.45 min cooldown
            "Every Man for Himself",
            // Removes all movement impairing effects and all effects which cause loss of control of your character. This effect
            "Shadowmeld",
            // Activate to slip into the shadows, reducing the chance for enemies to detect your presence. Lasts until cancelled or upon
            "Gift of the Naaru",
            // Heals the target for 20% of their total health over 15 sec. 3 minute cooldown.
            "Darkflight",
            // Activates your true form, increasing movement speed by 40% for 10 sec. 3 minute cooldown.
            "Blood Fury",
            // Activate to increase attack power and spell damage by an amount based on level/class for 15 seconds. 2 minute cooldown.
            "War Stomp",
            // Activate to stun opponents - Stuns up to 5 enemies within 8 yards for 2 seconds. 2 minute cooldown.
            "Berserking",
            // Activate to increase attack and casting speed by 20% for 10 seconds. 3 minute cooldown.
            "Will of the Forsaken",
            // Removes any Charm, Fear and Sleep effect. 2 minute cooldown.
            "Cannibalize",
            // When activated, regenerates 7% of total health and mana every 2 seconds for 10 seconds. Only works on Humanoid or Undead corpses within 5 yards. Any movement, action, or damage taken while Cannibalizing will cancel the effect.
            "Arcane Torrent",
            // Activate to silence all enemies within 8 yards for 2 seconds. In addition, you gain 15 Energy, 15 Runic Power or 6% Mana. 2 min. cooldown.
            "Rocket Barrage",
            // Launches your belt rockets at an enemy, dealing X-Y fire damage. (24-30 at level 1; 1654-2020 at level 80). 2 min. cooldown.
        };
        #endregion

        public static HashSet<int> GCDFreeAbilities
        {
            get {
                return _gcdFreeAbilities;
            }
        }

        #region _gcdFreeAbilities
        private static readonly HashSet<int> _gcdFreeAbilities = new HashSet<int> {
            108978, //= 0 GCD // ALTER TIME
            86659, // = 0 GCD // "Ancient Guardian",
            86669, //= 0 GCD  // "Ancient Guardian",
            86698, //= 0 GCD // "Ancient Guardian",
            71322, //= 0 GCD // "Annihilate",
            48707, //= 0 GCD // "Anti-Magic Shell",
            37538, //= 0 GCD // "Anti-Magic Shield",
            19645, //= 0 GCD // "Anti-Magic Shield",
            7121,  //= 0 GCD // "Anti-Magic Shield",
            51052, //= 0 GCD // "Anti-Magic Zone",
            12042, //= 0 GCD // "Arcane Power",
            31884, //= 0 GCD // "Avenging Wrath",
            22812, //= 0 GCD  // "Barkskin",
            41450, //= 0 GCD // "Blessing of Protection",
            45529, //= 0 GCD // "Blood Tap",
            84615, //= 0 GCD // "Blood and Thunder",
            2825,  //=  0 GCD // "Bloodlust",
            100,   //=  0 GCD // "Charge",
            845,   //=  0 GCD// "Cleave",
            11958, //=  0 GCD // "Cold Snap",
            11129, //=  0 GCD // "Combustion",
            2139,  //= 0 GCD // "Counterspell",
            //29893, //= 1.5 GCD  // "Create Soulwell",
            56222, //= 0 GCD // "Dark Command",
            77801, //= 0 GCD // "Dark Soul",
            49576, //= 0 GCD // "Death Grip",
            61595, //= 0 GCD // "Demonic Soul",
            19263, //= 0 GCD // "Deterrence",
            19505, //= 0 GCD // "Devour Magic",
            781, //= 0 GCD  // "Disengage",
            31842, //= 0 GCD // "Divine Favor",
            16166, //= 0 GCD // "Elemental Mastery",
            47568,  //= 0 GCD // "Empower Rune Weapon",
            5277, //= 0 GCD  // "Evasion",
            5384, //= 0 GCD // "Feign Death",
            87187, //= 0 GCD // "Feral Charge",
            79870, //= 0 GCD  // "Feral Charge",
            22842, //= 0 GCD // "Frenzied Regeneration",
            47788, //= 0 GCD // "Guardian Spirit",
            //24275, //= 1.5 GCD // "Hammer of Wrath",
            78, //= 0 GCD // "Heroic Strike",
            90255, //= 0 GCD  // "Hysteria",
            48792, //= 0 GCD // "Icebound Fortitude",
            12472, //= 0 GCD  // "Icy Veins",
            89485, //= 0 GCD  // "Inner Focus",
            50823, //= 0 GCD // "Intercept",
            78131, //= 0 GCD // "Intercept",
            27826, //= 0 GCD  // "Intercept",
            1766, //= 0 GCD // "Kick",
            34026, // "Kill Command", 1 second ???
            73325, // "Leap of Faith",
            49039, // "Lichborne",
            53271, // "Masters Call",
            6807, // "Maul",
            103958, // "Metamorphosis",
            47528, // "Mind Freeze",
            //16689, // "Nature's Grasp", GCD	1.5 seconds ??
            132158, // "Nature's Swiftness",
            51271, // "Pillar of Frost",
            10060, // "Power Infusion",
            14183, // "Premeditation",
            //14185, // "Preparation", GCD	1 second ???
            12043, // "Presence of Mind",
            6552, // "Pummel",
            3045, // "Rapid Fire",
            48982, // "Rune Tap",
            6358, // "Seduction",
            49572, // "Shadow Infusion",
            //30283, // "Shadowfury", GCD	500 milliseconds ??
            36554, // "Shadowstep",
            36988, // "Shield Bash",
            72194, // "Shield Bash",
            79732, // "Shield Bash",
            //101817, // "Shield Bash", GCD	1.5 seconds ??
            41197, // "Shield Bash",
            35178, // "Shield Bash",
            11972, // "Shield Bash",
            33871, // "Shield Bash",
            38233, // "Shield Bash",
            41180, // "Shield Bash",
            82800, // "Shield Bash",
            2565, // "Shield Block",
            53600, // "Shield of the Righteous",
            76008, // "Shock Blast",
            15487, // "Silence",
            34490, // "Silencing Shot",
            78675, // "Solar Beam",
            74434, // "Soulburn",
            19647, // "Spell Lock",
            23920, // "Spell Reflection",
            90361, // "Spirit Mend",
            79206, // "Spiritwalker's Grace",
            2983, // "Sprint",
            //48505, // "Starfall", GCD	1.5 seconds
            61336, // "Survival Instincts",
            12328, // Sweeping Strikes",
            80353, // "Time Warp",
            104773, // "Unending Resolve",
            55233, // "Vampiric Blood",
            1856, // "Vanish",
            6360, // "Whiplash",
            57994, // "Wind Shear",
            105809, // "Holy Avenger",
        };
        #endregion

        internal static HashSet<string> spellsThatBreakCrowdControl
        {
            get
            {
                return _spellsThatBreakCrowdControl;
            }
        }

        #region _spellsThatBreakCrowdControl
        /// <summary>
        /// Please add spells which can break movement imparing effects here
        /// </summary>
        private static readonly HashSet<string> _spellsThatBreakCrowdControl = new HashSet<string>
        {
            "Berserker Rage",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
        };
        #endregion
    }
}