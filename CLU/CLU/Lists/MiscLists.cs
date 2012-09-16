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

        public static HashSet<string> GCDFreeAbilities
        {
            get {
                return _gcdFreeAbilities;
            }
        }

        #region _gcdFreeAbilities
        private static readonly HashSet<string> _gcdFreeAbilities = new HashSet<string> {
            "Alter Time",
            "Ancient Guardian",
            "Annihilate",
            "Anti-Magic Shell",
            "Anti-Magic Shield",
            "Anti-Magic Zone",
            "Arcane Power",
            "Avenging Wrath",
            "Barkskin",
            "Blessing of Protection",
            "Blood Tap",
            "Blood and Thunder",
            "Bloodlust",
            "Bloodrage",
            "Charge",
            "Cleave",
            "Cold Blood",
            "Cold Snap",
            "Combustion",
            "Counterspell",
            "Create Soulwell",
            "Dark Command",
            "Dark Soul",
            "Death Grip",
            "Demon Soul",
            "Demonic Sacrifice",
            "Deterrence",
            "Devour Magic",
            "Disengage",
            "Divine Favor",
            "Divine Sacrifice",
            "Elemental Mastery",
            "Empower Rune Weapon",
            "Evasion",
            "Feign Death",
            "Fel Domination",
            "Feral Charge",
            "Frenzied Regeneration",
            "Guardian Spirit",
            "Hammer of Wrath",
            "Hand of Reckoning",
            "Heroic Strike",
            "Hysteria",
            "Icebound Fortitude",
            "Icy Veins",
            "Inner Focus",
            "Intercept",
            "Kick",
            "Kill Command",
            "Leap of Faith",
            "Lichborne",
            "Masters Call",
            "Maul",
            "Metamorphosis",
            "Mind Freeze",
            "Nature's Grasp",
            "Nature's Swiftness",
            "Pillar of Frost",
            "Power Infusion",
            "Premeditation",
            "Preparation",
            "Presence of Mind",
            "Pummel",
            "Rapid Fire",
            "Rune Tap",
            "Seduction",
            "Shadow Infusion",
            "Shadowfury",
            "Shadowstep",
            "Shield Bash",
            "Shield Block",
            "Shield of the Righteous",
            "Shock Blast",
            "Silence",
            "Silencing Shot",
            "Solar Beam",
            "Soulburn",
            "Spell Lock",
            "Spell Reflection",
            "Spirit Mend",
            "Spiritwalker's Grace",
            "Sprint",
            "Starfall",
            "Survival Instincts",
            "Sweeping Strikes",
            "Time Warp",
            "Unending Resolve",
            "Vampiric Blood",
            "Vanish",
            "Whiplash",
            "Wild Polymornph",
            "Wind Shear",
            "Zealotry",
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
        private static readonly HashSet<string> _spellsThatBreakCrowdControl = new HashSet<string>
        {
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
            "",
        };
        #endregion
    }
}