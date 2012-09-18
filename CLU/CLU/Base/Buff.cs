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

    using Styx;
    using Styx.CommonBot;
    using Styx.TreeSharp;
    using Styx.WoWInternals.WoWObjects;
    using System.Collections.Generic;
    using System.Linq;
    using Styx.WoWInternals;
    using CombatLog;
    using Managers;
    using Settings;
    using Action = Styx.TreeSharp.Action;

    internal static class Buff
    {
        /* putting all the Buff/Debuff logic here */

        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }

        private static readonly HashSet<int> BurstHaste = new HashSet<int> {
            2825,                //Shaman (2825) Bloodlust
            32182,               //Shaman (32182) Heroism
            80353,               //Mage (80353) Time Warp
            90355,               //Hunter (Core Hound) (90355) Ancient Hysteria
            49016,               //Unholy DeathKnight (49016) Unholy Frenzy
            3045,                //Hunter (3045) Rapid Fire
        };

        // ====================  Buffs ====================
        private static readonly HashSet<int> Stats = new HashSet<int> {
            117667,     //Legacy of The Emperor
            1126,       //Mark of The Wild
            20217,      //Blessing Of Kings
            90363,      //Embrace of the Shale Spider
        };

        private static readonly HashSet<int> Stamina = new HashSet<int> {
            469,        //Commanding Shout
            6307,       //Imp. Blood Pact
            21562,      //Power Word: Fortitude
            90364,      //Qiraji Fortitude
        };

        private static readonly HashSet<int> AttackPower = new HashSet<int> {
            19506,      //Trueshot Aura
            6673,       //Battle Shout
            57330,      //Horn of Winter
        };

        private static readonly HashSet<int> SpellPower = new HashSet<int> {
            77747,      //Burning Wrath
            109773,     //Dark Intent
            61316,      //Dalaran Brilliance
            1459,       //Arcane Brilliance 
            126309,     //Still Water
        };

        private static readonly HashSet<int> AttackSpeed = new HashSet<int> {
            30809,      //Unleashed Rage
            113742,     //Swiftblade's Cunning
            55610,      //Improved Icy Talons
            128432,     //Cackling Howl
            50498,      //Tear Armor

        };

        private static readonly HashSet<int> SpellHaste = new HashSet<int> {
            24907,      //Moonkin Aura
            51470,      //Elemental Oath
            49868,      //Mind Quickening
        };

        private static readonly HashSet<int> CriticalStrike = new HashSet<int> {
            1459,      //Arcane Brilliance
            61316,     //Dalaran Brilliance
            24932,     //Leader of The Pact
            116781,    //Legacy of the White Tiger
            97229,     //Bellowing Roar
            24604,    //Furious Howl
            90309,    //Terrifying Roar
            126373,   //Fearless Roar
            126309,   //Still Water

        };

        private static readonly HashSet<int> Mastery = new HashSet<int> {
            116956,    //Grace of Air
            19740,     //Blessing of Might
            93435,    //Roar of Courage
            128997,  //Spirit Beast Blessing
        };

        // ==================== Debuffs ====================
        private static readonly HashSet<int> MagicVulnerability = new HashSet<int> {
            //58410,      //Master Poisoner
            1490,       //Curse of the Elements TODO: Probably the better of the lot as it affects all targets within 15yrds..Soulburn: Curse ??
            //34889,      //Fire Breath (pet) the only 8% debuff..TODO: Do we want to overwrite the pets 8% with the 5% ?? --wulf
            //24844,      //Lightning Breath
        };

        private static readonly HashSet<int> WeakenedBlows = new HashSet<int> {
            115798, // Weakened Blows

        };

        private static readonly HashSet<int> WeakenedArmor = new HashSet<int> {
            113746, // Weakened Armor
        };

        private static readonly HashSet<int> PhysicalVulnerability = new HashSet<int> {
            81326, // Physical Vulnerability
        };

        private static readonly HashSet<int> SlowCasting = new HashSet<int> {
            5760,  // Mind-numbing Poison
            73975, //Necrotic Strike TODO: Needs to be checked
            31589, //Slow TODO: Needs to be checked
            109466, //Curse of Enfeeblement TODO: Needs to be checked
        };

        private static readonly HashSet<int> MortalWounds = new HashSet<int> {
            115804,  // Mortal Wounds
        };

        // ===================== Interuptable Spells =====================================

        private static readonly HashSet<int> InteruptableSpells = new HashSet<int> {
            30451, // Arcane Blast, // 
            32546, // Binding Heal, // 
            1064, // Chain Heal, // 
            421, // Chain Lightning, // 
            50796, // Chaos Bolt, // 
            693, // Create Healthstone, // 
            6201, // Create Healthstone, // 
            33786, // Cyclone, // 
            82326, // Divine Light, // 
            61882, // Earthquake, // 
            339, // Entangling Roots, // 
            12051, // Evocation, // 
            879, // Exorcism, // 
            5782, // Fear, // 
            133, // Fireball, // 
            2120, // Flamestrike, // 
            2061, // Flash Heal, // 
            19750, // Flash of Light, // 
            116, // Frostbolt, // 
            44614, // Frostfire Bolt, // 
            2060, // Greater Heal, // 
            77472, // Greater Healing Wave, // 
            71521, // Hand of Gul'dan, // 
            48181, // Haunt, // 
            2050, // Heal, // 
            73920, // Healing Rain, // 
            8004, // Healing Surge, // 
            5185, // Healing Touch, // 
            331, // Healing Wave, // 
            51514, // Hex, // 
            2637, // Hibernate, // 
            14914, // Holy Fire, // 
            635, // Holy Light, // 
            82327, // Holy Radiance, // 
            5484, // Howl of Terror, // 
            49203, // Hungering Cold, // 
            348, // Immolate, // 
            29722, // Incinerate, // 
            51505, // Lava Burst, // 
            403, // Lightning Bolt, // 
            8129, // Mana Burn, // 
            32375, // Mass Dispel, // 
            8092, // Mind Blast, // 
            605, // Mind Control, // 
            73510, // Mind Spike, // 
            50464, // Nourish, // 
            118, // Polymorph, // 
            28272, // Polymorph, // 
            61721, // Polymorph, // 
            61305, // Polymorph, // 
            61780, // Polymorph, // 
            28271, // Polymorph, // 
            596, // Prayer of Healing, // 
            11366, // Pyroblast, // 
            20484, // Rebirth, // 
            8936, // Regrowth, // 
            1513, // Scare Beast, // 
            2948, // Scorch, // 
            5676, // Searing Pain, // 
            27243, // Seed of Corruption, // 
            9484, // Shackle Undead, // 
            686, // Shadow Bolt, // 
            585, // Smite, // 
            6353, // Soul Fire, // 
            2912, // Starfire, // 
            78674, // Starsurge, // 
            30146, // Summon Felguard, // 
            691, // Summon Felhunter, // 
            688, // Summon Imp, // 
            1122, // Summon Infernal, // 
            712, // Summon Succubus, // 
            697, // Summon Voidwalker, // 
            30108, // Unstable Affliction, // 
            34914, // Vampiric Touch, // 
            5176, // Wrath, //


        };

        private static readonly HashSet<int> ChanneledInteruptableSpells = new HashSet<int> {
           5143, // Arcane Missiles, // 
           42650, // Army of the Dead, // 
           10, // Blizzard, // 
           64843, // Divine Hymn, // 
           689, // Drain Life, // 
           89420, // Drain Life, // 
           1120, // Drain Soul, // 
           755, // Health Funnel, // 
           1949, // Hellfire, // 
           85403, // Hellfire, // 
           16914, // Hurricane, // 
           64901, // Hymn of Hope, // 
           50589, // Immolation Aura, // 
           15407, // Mind Flay, // 
           47540, // Penance, // 
           5740, // Rain of Fire, // 
           740, // Tranquility, // 
           103103, // Malefic Grasp //

        };

        // ============================ Dispel shit.============================
        private static readonly HashSet<string> ignoreDispel = new HashSet<string> {
            "Blackout",
            "Toxic Torment",
            "Frostburn Formula",
            "Burning Blood",
            "Unstable Affliction",
             "Flame Shock",              // Magic
        };

        private static readonly HashSet<string> urgentDispel = new HashSet<string> {
            "Disrupting Shadows",       // magic
            "Boulder Smash",            // ??
            "Chains of Ice",            // Magic
            "Freezing Trap",            // Magic
            "Tentacle Smash",           // Magic
            "Shackles of Ice",          // Magic
            "Righteous Shear",          // Magic
            "Twilight Shear",           // Magic
            "Molten Blast",             // Magic
            "Temporal Vortex",          // Magic
            "Earth and Moon",           // Magic
            "Arcane Bomb",              // Magic
            "Shriek of the Highborne",  // Magic
            "Frost Corruption",         // Magic
            "Static Cling",             // Magic           
            "Static Discharge",         // Magic
            "Consuming Darkness",       // ???
            "Lash of Anguish",          // Magic
            "Static Disruption",        // Magic
            "Accelerated Corruption"    // Magic
        };


        /// <summary>
        /// Use this to print all auras
        /// </summary>
        /// Auras = All auras, including buffs, debuffs and hidden skill mechanics (enchants, etc.)
        /// Passive auras = buffs without any duration
        /// Active auras = mostly buffs and debuffs
        public static void DumpAuras()
        {
            if (Me.CurrentTarget == null) return;
            CLU.TroubleshootLog("===============Me.CurrentTarget.Auras===================");
            foreach (KeyValuePair<string, WoWAura> au in Me.CurrentTarget.Auras)
            {
                WoWAura aura;
                if (Me.Auras.TryGetValue(au.Key, out aura))
                {
                    CLU.TroubleshootLog("Name: " + aura.Name + " ID: " + aura.SpellId);
                } else {
                    CLU.TroubleshootLog( au.Key);
                }
            }
            CLU.TroubleshootLog( "=======================================================");
        }

        /// <summary>Used to  provide the time at which to attempt to refresh our debuff</summary>
        /// <param name="name">The name.</param>
        /// <returns>Spell cast time + GCD + Client Lag</returns>
        public static double DotDelta(string name)
        {
            // TODO: Decide if we need to individually supply seconds left to refresh the spell (for instance we could supply 2.5 seconds to refresh Vampiric Touch).
            return (Spell.CastTime(name) == 0 ? 1 :  Spell.CastTime(name) + Spell.GCD) + CombatLogEvents.ClientLag;
        }

        /// <summary>Check the aura thats created by yourself by the name on specified unit</summary>
        /// <param name="unit">The unit to check auras for. </param>
        /// <param name="aura">The name of the aura in English. </param>
        /// <returns>true if the target has the aura</returns>
        public static bool HasMyAura(this WoWUnit unit, string aura)
        {
            return HasAura(unit, aura, Me);
        }
        /// <summary>Check the aura thats created by yourself by the name on specified unit</summary>
        /// <param name="unit">The unit to check auras for. </param>
        /// <param name="spellId">The name of the aura in English. </param>
        /// <returns>true if the target has the aura</returns>
        public static bool HasMyAura(this WoWUnit unit, int spellId)
        {
            return HasAura(unit, spellId, Me);
        }
        /// <summary>Checks for the auras on a specified unit. Returns true if the unit has any aura in the auraNames list.</summary>
        /// <param name="unit">The unit to check auras for.</param>
        /// <param name="aura">Aura names to be checked.</param>
        /// <param name="creator">Check for only self or all buffs</param>
        /// <returns>The has aura.</returns>
        public static bool HasAura(WoWUnit unit, string aura, WoWUnit creator)
        {
            return unit != null && unit.GetAllAuras().Any(a => a.Name == aura && (creator == null || a.CreatorGuid == creator.Guid));
        }
        /// <summary>Checks for the auras on a specified unit. Returns true if the unit has any aura in the auraNames list.</summary>
        /// <param name="unit">The unit to check auras for.</param>
        /// <param name="spellId">Aura names to be checked.</param>
        /// <param name="creator">Check for only self or all buffs</param>
        /// <returns>The has aura.</returns>
        public static bool HasAura(WoWUnit unit, int spellId, WoWUnit creator)
        {
            return unit != null && unit.GetAllAuras().Any(a => a.SpellId == spellId && (creator == null || a.CreatorGuid == creator.Guid));
        }
        /// <summary>
        ///  Checks for the auras on a specified unit. Returns true if the unit has any aura in the auraNames list.
        /// </summary>
        /// <param name="unit"> The unit to check auras for. </param>
        /// <param name="auraNames"> Aura names to be checked. </param>
        /// <returns></returns>
        public static bool HasAnyAura(this WoWUnit unit, params string[] auraNames)
        {
            var auras = unit.GetAllAuras();
            var hashes = new HashSet<string>(auraNames);
            return auras.Any(a => hashes.Contains(a.Name));
        }

        /// <summary>
        ///  Checks for the auras on a specified unit. Returns true if the unit has any aura in the auraNames list.
        /// </summary>
        /// <param name="unit"> The unit to check auras for. </param>
        /// <param name="auraIDs">Aura ID to be checked. </param>
        /// <returns></returns>
        public static bool HasAnyAura(this WoWUnit unit, HashSet<int> auraIDs)
        {
            var auras = unit.GetAllAuras();
            return auras.Any(a => auraIDs.Contains(a.SpellId));
        }

        /// <summary>Checks the aura to see if it can be dispelled by the name on specified unit</summary>
        /// <param name="unit">The unit to check auras for. </param>
        /// <param name="isUrgent">The is Urgent.</param>
        /// <returns>true if the target has an aura to dispel</returns>
        public static bool HasAuraToDispel  (this WoWUnit unit, bool isUrgent)
        {
            switch (Me.Class) {
            case WoWClass.Rogue:
                return false;
            case WoWClass.DeathKnight:
                return false;
            case WoWClass.Hunter:
                return false;
            case WoWClass.Mage:
                return HasAuraToDispel(unit, false, false, false, true, isUrgent);
            case WoWClass.Warlock:
                var felhunterWarlock = PetManager.CanCastPetSpell("Devour Magic");     			// Warlock has his felhunter out?
                var impWarlock = PetManager.CanCastPetSpell("Singe Magic");            			// Warlock has his imp out?
                return HasAuraToDispel(unit, false, felhunterWarlock || impWarlock, false, false, isUrgent);
            case WoWClass.Shaman:
                var restoShaman = TalentManager.HasTalent(12);                               // Are we a Restoration Shaman with Improved Cleanse Spirit?
                return HasAuraToDispel(unit, false, restoShaman, false, true, isUrgent);
            case WoWClass.Druid:
                var restoDruid = TalentManager.HasTalent(17);                                // Do we have Nature's Cure talented?
                return HasAuraToDispel(unit, false, restoDruid, true, true, isUrgent);
            case WoWClass.Paladin:
                var holyPaladin = TalentManager.HasTalent(14);                               // Are we a Holy Paladin with Sacred Cleansing?
                return HasAuraToDispel(unit, true, holyPaladin, true, false, isUrgent);
            case WoWClass.Priest:
                // var holyPriest = TalentManager.HasTalent(14);                             // Are we a Holy Priest with Body and Soul?
                return HasAuraToDispel(unit, true, true, false, false, isUrgent);
            default:
                return false;
            }
        }

        /// <summary>Checks the aura to see if it can be dispelled by the name on specified unit</summary>
        /// <param name="unit">The unit to check auras for. </param>
        /// <param name="disease">The disease.</param>
        /// <param name="magic">The magic.</param>
        /// <param name="poison">The poison.</param>
        /// <param name="curse">The curse.</param>
        /// <param name="urgent">The urgent.</param>
        /// <returns>true if the target has an aura to dispel</returns>
        private static bool HasAuraToDispel(this WoWUnit unit, bool disease, bool magic, bool poison, bool curse, bool urgent)
        {
            foreach (WoWAura aura in unit.Debuffs.Values) {
                if (!ignoreDispel.Contains(aura.Name) && (urgent && urgentDispel.Contains(aura.Name))) {
                    WoWDispelType type = aura.Spell.DispelType;
                    switch (type) {
                    case WoWDispelType.Curse:
                        return curse;
                    case WoWDispelType.Disease:
                        return disease;
                    case WoWDispelType.Magic:
                        return magic;
                    case WoWDispelType.Poison:
                        return poison;
                    default:
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>Creats a list of WoWDispelTypes currently on the specified unit</summary>
        /// <param name="unit">The unit to check WoWDispelTypes for. </param>
        /// <returns>returns a list of WoWDispelTypes on the unit</returns>
        public static List<WoWDispelType> GetDispelType(WoWUnit unit)
        {
            return (from aura in unit.Debuffs.Values where !ignoreDispel.Contains(aura.Name) select aura.Spell.DispelType).ToList();
        }

        /// <summary>Returns true if a raid or party member has my aura</summary>
        /// <param name="aura">The aura.</param>
        /// <returns>true or false</returns>
        public static bool AnyHasMyAura(string aura)
        {
            return
                ObjectManager.GetObjectsOfType<WoWPlayer>(true, false).Any(
                    p => p.IsAlive &&
                    p.Guid != Me.Guid &&
                    p.IsPlayer &&
                    !p.IsGhost &&
                    p.GetAllAuras().Any(a => a.Name == aura && a.CreatorGuid == Me.Guid));
        }

        /// <summary>Returns the timeleft of an aura by TimeSpan. Return TimeSpan.Zero if the aura doesn't exist.</summary>
        /// <param name="unit">The unit to check the aura for.</param>
        /// <param name="auraName">The name of the aura in English.</param>
        /// <param name="fromMyAura"> true if its from my aura</param>
        /// <returns>The get aura time left.</returns>
        public static TimeSpan GetAuraTimeLeft(WoWUnit unit, string auraName, bool fromMyAura)
        {
            if (unit != null) {
                WoWAura aura;
                if (unit.Auras.TryGetValue(auraName, out aura))
                {
                    //CLU.TroubleshootLog(" [GetAuraTimeLeft] auraName: {0} Aura: {1} AuraTimeLeft: {2}", auraName, aura, aura.TimeLeft);
                    var returnvalue = (!fromMyAura || aura.CreatorGuid == Me.Guid);
                    return returnvalue ? aura.TimeLeft : TimeSpan.Zero;
                }
                return  TimeSpan.Zero;
            }

            CLU.DiagnosticLog(" [GetAuraTimeLeft] Unit is null ");
            return TimeSpan.Zero;
        }

        /// <summary>Check the aura stack count thats created by the specified unit</summary>
        /// <param name="unit">The unit to check auras for.</param>
        /// <param name="auraName">The name of the aura in English</param>
        /// <param name="fromMyAura">True if you applied the aura</param>
        /// <returns>The get aura stack.</returns>
        public static uint GetAuraStack(WoWUnit unit, string auraName, bool fromMyAura)
        {
            if (unit != null)
            {
                WoWAura aura;
                if (unit.Auras.TryGetValue(auraName, out aura))
                {
                    var returnvalue = (!fromMyAura || aura.CreatorGuid == Me.Guid) && aura.StackCount > 0;
                    return returnvalue ? aura.StackCount : 0;
                }
                return 0;
            }

            CLU.DiagnosticLog(" [GetAuraStack] Unit is null ");
            return 0;
        }

        /// <summary>
        /// Casts a specified debuff given specific conditions
        /// </summary>
        /// <param name="name">The name of the debuff to cast in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>true or false</returns>
        public static Composite CastDebuff(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
        		delegate(object a) {
        			if (!cond(a)) {
        				return false;
        			}

        			if (TargetDebuffTimeLeft(name).TotalSeconds > DotDelta(name)) {
        				return false;
        			}

        			var lockstatus = true;
        			try {
        				lockstatus = DateTime.Now.Subtract(CombatLogEvents.Locks[name]).TotalSeconds > 0;
        			} catch { }

        			if (!Spell.CanCast(name, Me.CurrentTarget)) {
        				return false;
        			}

        			return lockstatus;
            },
            new Sequence(
                new Action(a => CLU.Log(" [Casting Debuff] {0} : (RefreshTime={1}) had {2} second(s) left", label, DotDelta(name), TargetDebuffTimeLeft(name).TotalSeconds)),
                new Action(a => SpellManager.Cast(name)),
                new Action(a => CombatLogEvents.Locks[name] = DateTime.Now.AddSeconds(Spell.CastTime(name) * 1.5 + CombatLogEvents.ClientLag))));
        }

        /// <summary>
        /// Casts a specified buff given specific conditions
        /// </summary>
        /// <param name="name">The name of the buff to cast in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>true or false</returns>
        public static Composite CastTargetBuff(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
        		delegate(object a) {
        			if (!cond(a)) {
        				return false;
        			}

        			if (TargetBuffTimeLeft(name).TotalSeconds > DotDelta(name)) {
        				return false;
        			}

        			var lockstatus = true;
        			try {
        				lockstatus = DateTime.Now.Subtract(CombatLogEvents.Locks[name]).TotalSeconds > 0;
        			} catch { }

        			if (!Spell.CanCast(name, Me.CurrentTarget)) {
        				return false;
        			}

        			return lockstatus;
            },
            new Sequence(
                new Action(a => CLU.Log(" [Casting TargetBuff] {0} : (RefreshTime={1}) had {2} second(s) left", label, DotDelta(name), TargetDebuffTimeLeft(name).TotalSeconds)),
                new Action(a => SpellManager.Cast(name)),
                new Action(a => CombatLogEvents.Locks[name] = DateTime.Now.AddSeconds(Spell.CastTime(name) * 1.5 + CombatLogEvents.ClientLag))));
        }

        /// <summary>
        /// Used to refresh buffs such as Improved Soul Fire via other buffs to increase uptime.
        /// </summary>
        /// <param name="spell">Spell to cast that will re-apply the helpful buff</param>
        /// <param name="buff">buff Time left</param>
        /// <param name="maxTimeLeft">maximum time left before refresh</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>true of false</returns>
        public static Composite CastOffensiveBuff(string spell, string buff, double maxTimeLeft, string label)
        {
            return new Decorator(
        		delegate {

        			var lockstatus = true;
        			try {
        				lockstatus = DateTime.Now.Subtract(CombatLogEvents.Locks[spell]).TotalSeconds > 0;
        			} catch { }


        			if (PlayerBuffTimeLeft(buff) > maxTimeLeft) {
        				return false;
        			}

        			if (!Spell.CanCast(spell, Me.CurrentTarget)) {
        				return false;
        			}

        			return lockstatus;
            },
            new Sequence(
                new Action(
                    a => CLU.Log(
                        label == null ? null : " [Offensive Buff] {0} ({1} time left={2})",
                        label,
                        buff,
                        PlayerBuffTimeLeft(buff))),
                new Action(a => SpellManager.Cast(spell)),
                new Action(a => CombatLogEvents.Locks[spell] = DateTime.Now.AddSeconds(Spell.CastTime(spell) * 1.5 + CombatLogEvents.ClientLag)))); // cast time * Max Flight time (from 40yards) + Clientlag = (2.54 * 1.5 + 1 = 4.81s lock for soul fire)
        }

        /// <summary>
        ///  Casts a raid buff if the buff is not currently applied on the player or the raid
        /// </summary>
        /// <param name="name">name of the buff to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>true or false</returns>
        public static Composite CastRaidBuff(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
        		delegate(object a) {

        			if (!CLUSettings.Instance.EnableRaidPartyBuffing)
        				return false;

        			if (!cond(a))
        				return false;

                    if (!Spell.CanCast(name, Me))
                    {
                        return false;
                    }

        			// If we are solo then return true if the name of the requested buff matchs the users UI setting and the player does not have the buff..
        			if (!Unit.IsInGroup && !Me.IsDead && !Me.IsGhost && Me.IsAlive) {
                        if (name.Contains(CLUSettings.Instance.Warrior.ShoutSelection.ToString()) && !PlayerHasBuff(name)) return true;
                        if (name.Contains(CLUSettings.Instance.Monk.LegacySelection.ToString()) && !PlayerHasBuff(name)) return true;
                        if (name.Contains(CLUSettings.Instance.Paladin.BlessingSelection.ToString()) && !PlayerHasBuff(name)) return true;
                        //if (name.Contains(CLUSettings.Instance.Warrior.ShoutSelection.ToString()) && PlayerHasBuff(name)) return false;
                        //if (name.Contains(CLUSettings.Instance.Monk.LegacySelection.ToString()) && PlayerHasBuff(name)) return false;
                        //if (name.Contains(CLUSettings.Instance.Paladin.BlessingSelection.ToString()) && PlayerHasBuff(name)) return false;
        			}

        			// Continue on if we are in a raid group and check all raid members for the buffs we can provide and cast them if ok.
        			var players = new List<WoWPlayer> { Me };
        			if (Me.GroupInfo.IsInRaid)
        			{
                        players.Remove(Me);
                        players.AddRange(Me.RaidMembers);
        			}
                    else if (Me.IsInParty)
                    {
                        players.AddRange(Me.PartyMembers);
                    }

        			var ProvidablePlayerBuffs = new HashSet<int>();
        			switch (StyxWoW.Me.Class) {
        				case WoWClass.Warrior:
                            ProvidablePlayerBuffs.UnionWith(Stamina);
                            ProvidablePlayerBuffs.UnionWith(AttackPower);
        					
        					break;
        				case WoWClass.Paladin:
                            ProvidablePlayerBuffs.UnionWith(Stats);
                            ProvidablePlayerBuffs.UnionWith(Mastery);
        					
        					break;
        				case WoWClass.Hunter:
                            ProvidablePlayerBuffs.UnionWith(AttackPower);
                            ProvidablePlayerBuffs.UnionWith(CriticalStrike);
        					
        					break;
        				case WoWClass.Rogue:
                            ProvidablePlayerBuffs.UnionWith(AttackSpeed);
        					
        					break;
        				case WoWClass.Priest:
                            ProvidablePlayerBuffs.UnionWith(Stamina);
                            ProvidablePlayerBuffs.UnionWith(SpellHaste);
        					
        					break;
        				case WoWClass.DeathKnight:
                            ProvidablePlayerBuffs.UnionWith(AttackPower);
                            ProvidablePlayerBuffs.UnionWith(AttackSpeed);
        					
        					break;
        				case WoWClass.Shaman:
                            ProvidablePlayerBuffs.UnionWith(AttackSpeed);
                            ProvidablePlayerBuffs.UnionWith(SpellPower);
                            ProvidablePlayerBuffs.UnionWith(SpellHaste);
                            ProvidablePlayerBuffs.UnionWith(Mastery);
        					
        					break;
        				case WoWClass.Mage:
                            ProvidablePlayerBuffs.UnionWith(SpellPower);
                            ProvidablePlayerBuffs.UnionWith(CriticalStrike);
        					
        					break;
        				case WoWClass.Warlock:
                            ProvidablePlayerBuffs.UnionWith(Stamina);
                            ProvidablePlayerBuffs.UnionWith(SpellPower);
        					
        					break;
        				case WoWClass.Druid:
                            ProvidablePlayerBuffs.UnionWith(Stats);
                            ProvidablePlayerBuffs.UnionWith(CriticalStrike);
                            ProvidablePlayerBuffs.UnionWith(SpellHaste);
        			        break;

        			    case WoWClass.Monk:
                            ProvidablePlayerBuffs.UnionWith(Stats);
                            ProvidablePlayerBuffs.UnionWith(Mastery);          
        					break;
        			}

        			return  players.Any(x => x.Distance2DSqr < 40 * 40 && !x.HasAnyAura(ProvidablePlayerBuffs) && !x.IsDead && !x.IsGhost && x.IsAlive);
            },
            new Sequence(
                new Action(a => CLU.Log(" [Raid Buff] {0} ", label)),
                new Action(a => SpellManager.Cast(name))));
        }

        /// <summary>
        /// Casts a specified buff given specific conditions
        /// </summary>
        /// <param name="name">The name of the buff to cast in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>true or false</returns>
        public static Composite CastBuff(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
        		delegate(object a) {
        			if (PlayerHasBuff(name))
        				return false;

        			if (!cond(a))
        				return false;

        			if (!SpellManager.CanBuff(name, Me))
        				return false;

        			return true;
            },
            new Sequence(
                new Action(a => CLU.Log(" [Buff] {0} ", label)),
                new Action(a => SpellManager.Cast(name))));
        }

        /// <summary>Casts a Buff on a specified unit</summary>
        /// <param name="name">the name of the Buff to cast</param>
        /// <param name="onUnit">The Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>Buff on the unit</returns>
        public static Composite CastBuffonUnit(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
        		delegate(object a) {
        			if (TargetHasBuff(name))
        				return false;

        			if (!cond(a))
        				return false;

        			if (!SpellManager.CanBuff(name, Me))
        				return false;

        			return onUnit(a) != null;
            },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Action(a => SpellManager.Cast(name, onUnit(a)))));
        }


        /// <summary>
        ///     Returns an integer value of Units with MY debuff / Buff
        ///     in active combat with the player, the player's party, or the player's raid.
        /// </summary>
        public static bool UnitsHasMyBuff(string spell)
        {
            var mUnit = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(unit => unit.ActiveAuras.ContainsKey(spell)).ToList();
            return mUnit.Count(t => t.ActiveAuras[spell].CreatorGuid == Me.Guid) > 0;
        }

        /// <summary>Returns the buff time left on the player</summary>
        /// <param name="name">name of the buff to check</param>
        /// <returns>The player buff time left.</returns>
        public static double PlayerBuffTimeLeft(string name)
        {
            using (StyxWoW.Memory.AcquireFrame()) {
                try {
                    var lua = String.Format("local x=select(7, UnitBuff('player', \"{0}\", nil, 'PLAYER')); if x==nil then return 0 else return x-GetTime() end", Spell.RealLuaEscape(name));
                    var t = Double.Parse(Lua.GetReturnValues(lua)[0]);
                    return t;
                } catch {
                    CLU.DiagnosticLog("Lua failed in PlayerBuffTimeLeft");
                    return 999999;
                }
            }

            // return GetAuraTimeLeft(Me, name, true);
        }

        /// <summary>Returns the Active debuff time left on the player</summary>
        /// <param name="name">name of the debuff to check</param>
        /// <returns>The player debuff time left.</returns>
        public static TimeSpan PlayerDebuffTimeLeft(string name)
        {
            return GetAuraTimeLeft(Me, name, false);
        }

        /// <summary>Returns the buff time left on the player</summary>
        /// <param name="name">name of the buff to check</param>
        /// <returns>The player buff time left.</returns>
        public static TimeSpan PlayerActiveBuffTimeLeft(string name)
        {
            return GetAuraTimeLeft(Me, name, true);
        }

        /// <summary>Returns the buff count on the player</summary>
        /// <param name="name">name of the buff to check</param>
        /// <returns>The player count buff.</returns>
        public static uint PlayerCountBuff(string name)
        {
            return GetAuraStack(Me, name, true);
        }

        /// <summary>Returns true if the player has the buff</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The player has buff.</returns>
        public static bool PlayerHasBuff(string name)
        {
            return Me.HasAura(name);
        }

        //// todo: temporary fix.
        //public static bool PlayerHasBuff(string name)
        //{
        //    try
        //    {
        //        var lua = string.Format("local x=UnitBuff('player', \"{0}\"); if x==nil then return 0 else return 1 end", Spell.RealLuaEscape(name));
        //        return Lua.GetReturnValues(lua)[0] == "1";
        //    }
        //    catch
        //    {
        //        CLU.DiagnosticLog("Lua failed in PlayerHasBuff");
        //        return false;
        //    }
        //}

        /// <summary>Returns true if the player has the ACTIVE buff. Good for checking procs.</summary>
        /// <param name="name">the name of the active buff to check for</param>
        /// <returns>The player has active buff.</returns>
        public static bool PlayerHasActiveBuff(string name)
        {
            return Me.ActiveAuras.ContainsKey(name);
        }
        /// <summary>Returns true if the player has the ACTIVE buff. Good for checking procs.</summary>
        /// <param name="spellId">the spellId of the active buff to check for</param>
        /// <returns>The player has active buff.</returns>
        public static bool PlayerHasActiveBuff(int spellId)
        {

            return Me.ActiveAuras.Any(q => q.Value.SpellId == spellId);
        }

        /// <summary>Returns the debuff time left on the target</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The target debuff time left.</returns>
        public static TimeSpan TargetDebuffTimeLeft(string name)
        {
            return GetAuraTimeLeft(Me.CurrentTarget, name, true);
        }

        /// <summary>Returns the buff time left on the target</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The target buff time left.</returns>
        private static TimeSpan TargetBuffTimeLeft(string name)
        {
            return GetAuraTimeLeft(Me.CurrentTarget, name, true);
        }

        /// <summary>Returns the debuff count on the target</summary>
        /// <param name="name">the name of the debuff to check for</param>
        /// <returns>The target count debuff.</returns>
        public static uint TargetCountDebuff(string name)
        {
            return GetAuraStack(Me.CurrentTarget, name, true);
        }

        /// <summary>
        /// Returns the buff count on the target
        /// </summary>
        /// <param name="name">the name of the buff to check</param>
        /// <returns>the target count buff</returns>
        public static uint TargetCountBuff(string name)
        {
            return GetAuraStack(Me.CurrentTarget, name, false);
        }

        /// <summary>Returns true if the target has this debuff</summary>
        /// <param name="name">the name of the debuff to check for</param>
        /// <returns>The target has debuff.</returns>
        public static bool TargetHasDebuff(string name)
        {
            return HasAura(Me.CurrentTarget, name, Me);
        }

        /// <summary>Returns true if the target has this buff</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The target has the buff.</returns>
        public static bool TargetHasBuff(string name)
        {
            return HasAura(Me.CurrentTarget, name, Me);
        }

        /// <summary>Returns true if the target has this debuff</summary>
        /// <param name="name">the name of the debuff to check for</param>
        /// <param name="unit">the unit to check for</param>
        /// <returns>The target has debuff.</returns>
        public static bool TargetHasDebuff(string name, WoWUnit unit)
        {
            return unit != null && unit.ActiveAuras.Any(x => x.Value.Name == name);
        }

        /// <summary>Returns true if the Unit is under the affects of Haste.</summary>
        /// <param name="unit">The unit to check the for.</param>
        /// <returns>The unit has haste buff.</returns>
        public static bool UnitHasHasteBuff(WoWUnit unit)
        {
            return unit != null && unit.ActiveAuras.Any(x => BurstHaste.Contains(x.Value.SpellId));
        }

        /// <summary>Returns true if the Unit has Stamina Buffs</summary>
        /// <param name="unit">The unit to check the for.</param>
        /// <returns>The unit has bleed damage debuff.</returns>
        public static bool UnitHasMagicVulnerabilityDeBuffs(WoWUnit unit)
        {
            return unit != null && unit.ActiveAuras.Any(x => MagicVulnerability.Contains(x.Value.SpellId) && Me.Class == WoWClass.Warlock);
        }

        /// <summary>Returns true if the Unit has the 10% damage reduction debuff</summary>
        /// <param name="unit">The unit to check the for.</param>
        /// <returns>The unit has damage reduction debuff.</returns>
        public static bool UnitHasWeakenedBlows(WoWUnit unit)
        {
            return unit != null && unit.ActiveAuras.Any(x => WeakenedBlows.Contains(x.Value.SpellId));
        }

        /// <summary>Returns true if the Unit has the Armor Reduction debuff</summary>
        /// <param name="unit">The unit to check the for.</param>
        /// <returns>The unit has armor reduction debuff.</returns>
        public static bool UnitHasWeakenedArmor(WoWUnit unit)
        {
            return unit != null && unit.ActiveAuras.Any(x => WeakenedArmor.Contains(x.Value.SpellId));
        }

        /// <summary>Returns true if the Unit has the Strength and Agility Buff</summary>
        /// <param name="unit">The unit to check the for.</param>
        /// <returns>The unit has str agi buff.</returns>
        public static bool UnitHasStrAgiBuff(WoWUnit unit)
        {
            return unit != null && unit.ActiveAuras.Any(x => AttackPower.Contains(x.Value.SpellId));
        }

        /// <summary>Checks the unit for an interuptable spell being cast</summary>
        /// <param name="player">The unit to check the for.</param>
        /// <returns>The spell cast is interuptable</returns>
        public static bool CanInteruptCastSpell(this WoWUnit player)
        {
            return player != null && player.IsCasting && InteruptableSpells.Contains(player.CastingSpellId);
        }

        /// <summary>Checks the unit for an interuptable channeled spell being cast</summary>
        /// <param name="player">The unit to check the for.</param>
        /// <returns>the spell channel is interuptable</returns>
        public static bool CanInteruptChannelSpell(this WoWUnit player)
        {
            return player != null && player.IsCasting && ChanneledInteruptableSpells.Contains(player.CastingSpellId);
        }

        /// <summary>Returns true if the player has one of the supplied crowd control.</summary>
        /// <returns>The player is crowd controlled</returns>
        public static bool PlayerHasCrowdControl(WoWSpellMechanic cc)
        {
            return StyxWoW.Me.GetAllAuras().Any(a => a.Spell.Mechanic == cc);
        }

    }
}
