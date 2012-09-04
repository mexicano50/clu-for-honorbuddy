﻿namespace CLU.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Clu;
    using Clu.Helpers;

    using Styx;
    using Styx.Combat.CombatRoutine;
    using Styx.Logic;
    using Styx.Logic.Combat;
    using Styx.Logic.Pathing;
    using Styx.Logic.POI;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;

    using System.Drawing;

    using TreeSharp;

    using Clu.Lists;
    using Clu.Settings;

    using Action = TreeSharp.Action;

    internal static class Unit
    {
        /* putting all the Unit logic here */

        public static uint CurrentTargetEntry
        {
            get {
                return Me.CurrentTarget != null ? Me.CurrentTarget.Entry : 0;
            }
        }

        private static IEnumerable<WoWPartyMember> GroupMemberInfos
        {
            get {
                return !Me.IsInRaid ? Me.PartyMemberInfos : Me.RaidMemberInfos;
            }
        }

        private static readonly string[] ControlDebuffs = new[] {
            "Bind Elemental", "Hex", "Polymorph", "Hibernate", "Entangling Roots", "Freezing Trap", "Wyvern Sting",
            "Repentance", "Psychic Scream", "Sap", "Blind", "Fear", "Seduction", "Howl of Terror"
        };

        private static readonly string[] ControlUnbreakableDebuffs = new[] { "Cyclone", "Mind Control", "Banish" };

        private static readonly string[] HealerSpells = new[] {
            // -- Priests
            "Penance",                  // [47540] = "PRIEST", -- Penance
            "Holy Word: Chastise",      // [88625] = "PRIEST", -- Holy Word: Chastise
            "Holy Word: Serenity",      // [88684] = "PRIEST", -- Holy Word: Serenity
            "Holy Word: Sanctuary",     // [88685] = "PRIEST", -- Holy Word: Sanctuary
            "Inner Focus",              // [89485] = "PRIEST", -- Inner Focus
            "Power Infusion",           // [10060] = "PRIEST", -- Power Infusion
            "Pain Suppression",         // [33206] = "PRIEST", -- Pain Suppression
            "Power Word: Barrier",      // [62618] = "PRIEST", -- Power Word: Barrier
            "Lightwell",                // [724]   = "PRIEST",   -- Lightwell
            "Chakra",                   // [14751] = "PRIEST", -- Chakra
            "Circle of Healing",        // [34861] = "PRIEST", -- Circle of Healing
            "Guardian Spirit",          // [47788] = "PRIEST", -- Guardian Spirit
            // -- Druids
            "Swiftmend",                // [18562] = "DRUID", -- Swiftmend
            "Nourish",                  // [50464] = "DRUID", -- Nourish
            "Nature's Swiftness",       // [17116] = "DRUID", -- Nature's Swiftness
            "Wild Growth",              // [48438] = "DRUID", -- Wild Growth
            "Tree of Life",             // [33891] = "DRUID", -- Tree of Life
            "Regrowth",                 // [8936] = "DRUID", -- Regrowth
            "Healing Touch",            // [5185] = "DRUID", -- Healing Touch
            // -- Shamans
            "Earth Shield",             // [974]   = "SHAMAN", -- Earth Shield
            "Healing Wave",             // [331]   = "SHAMAN", -- Healing Wave
            "Healing Surge",            // [8004]   = "SHAMAN", -- Healing Surge
            "Greater Healing Wave",     // [77472]   = "SHAMAN", -- Greater Healing Wave
            "Lesser Healing Wave",      // [68115]   = "SHAMAN", -- Lesser Healing Wave
            "Nature's Swiftness",       // [17116] = "SHAMAN", -- Nature's Swiftness
            "Mana Tide Totem",          // [16190] = "SHAMAN", -- Mana Tide Totem
            "Riptide",                  // [61295] = "SHAMAN", -- Riptide
            "Chain Heal",               // [1064]   = "SHAMAN", -- Chain Heal
            // -- Paladins
            "Holy Shock",               // [20473] = "PALADIN", -- Holy Shock
            "Divine Favor",             // [31842] = "PALADIN", -- Divine Favor
            "Beacon of Light",          // [53563] = "PALADIN", -- Beacon of Light
            "Aura Mastery",             // [31821] = "PALADIN", -- Aura Mastery
            "Light of Dawn",            // [85222] = "PALADIN", -- Light of Dawn
            "Flash Heal",               // [2061] = "PALADIN", -- Flash Heal
            "Flash of Light",           // [19750] = "PALADIN", -- Light of Dawn
            "Holy Light"                // [635] = "PALADIN", -- Holy Light
        };

        private static readonly HashSet<uint> IgnoreMobs = new HashSet<uint> {
            52288, // Venomous Effusion (NPC near the snake boss in ZG. Its the green lines on the ground. We want to ignore them.)
            52302, // Venomous Effusion Stalker (Same as above. A dummy unit)
            52320, // Pool of Acid
            52525, // Bloodvenom
            52387, // Cave in stalker - Kilnara
        };

        private static List<FocusedUnit> mostFocusedUnits;

        private static DateTime mostFocusedUnitsTimer = DateTime.MinValue;


        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }

        /// <summary>
        ///     List of nearby units to heal that pass certain criteria.
        /// </summary>
        public static IEnumerable<WoWUnit> HealList
        {
            get {
                try {
                    var ret = from o in ObjectManager.ObjectList
                              where o is WoWPlayer && o.Location.DistanceSqr(Me.Location) < 40 * 40
                              let p = o.ToUnit().ToPlayer()
                                      where p.IsAlive && !p.IsGhost && p.IsPlayer && p.ToPlayer() != null && !p.IsFlying && !p.OnTaxi
                                      orderby p.CurrentHealth
                                      select p.ToUnit();

                    return ret;
                } catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        ///     List of nearby Party units to heal that pass certain criteria.
        /// </summary>
        public static IEnumerable<WoWPartyMember> PartyHealList
        {
            get {
                try {
                    return from o in GroupMemberInfos
                           where o.Location3D.Distance2DSqr(Me.Location) < 40 * 40
                           && o.IsOnline
                           && o.ToPlayer().IsAlive
                           && !o.ToPlayer().IsGhost
                           && o.ToPlayer().IsPlayer
                           && o.ToPlayer() != null
                           && !o.ToPlayer().IsFlying
                           && !o.ToPlayer().OnTaxi
                           orderby o.Health
                           select o;
                } catch (NullReferenceException) {
                    return new List<WoWPartyMember>();
                }
            }
        }

        /// <summary>
        ///     List of nearby Ranged enemy units that pass certain criteria, this list should only return units
        ///     in active combat with the player, the player's party, or the player's raid.
        /// </summary>
        public static IEnumerable<WoWUnit> RangedPvPUnits
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false)
                              .Where(unit =>
                                     !unit.IsFriendly
                                     && (unit.IsTargetingMeOrPet
                                         || unit.IsTargetingMyPartyMember
                                         || unit.IsTargetingMyRaidMember
                                         || unit.IsPlayer
                                         || unit.MaxHealth == 1)
                                     && !unit.IsNonCombatPet
                                     && !unit.IsCritter
                                     && unit.Distance2D
                                     <= 40).OrderBy(u => u.DistanceSqr);

                    return ret;
                }
                catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        ///     List of nearby enemy units that pass certain criteria, this list should only return units
        ///     in active combat with the player, the player's party, or the player's raid.
        /// </summary>
        public static IEnumerable<WoWUnit> EnemyUnits
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false)
                              .Where(unit =>
                                     !unit.IsFriendly
                                     && (unit.IsTargetingMeOrPet
                                         || unit.IsTargetingMyPartyMember
                                         || unit.IsTargetingMyRaidMember
                                         || unit.IsPlayer
                                         || unit.MaxHealth == 1)
                                     && !unit.IsNonCombatPet
                                     && !unit.IsCritter
                                     && unit.Distance2D
                                     <= 12);

                    return ret;
                } catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        ///     List of nearby Ranged enemy units that pass certain criteria, this list should only return units
        ///     in active combat with the player, the player's party, or the player's raid.
        /// </summary>
        public static IEnumerable<WoWUnit> RangedPvEUnits
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false)
                              .Where(unit =>
                                     !unit.IsFriendly
                                     && (unit.IsTargetingMeOrPet
                                         || unit.IsTargetingMyPartyMember
                                         || unit.IsTargetingMyRaidMember
                                         || unit.MaxHealth == 1
                                         || BossList.BossIds.Contains(unit.Entry))
                                     && !unit.IsNonCombatPet
                                     && !unit.IsCritter
                                     && unit.Distance2D
                                     <= 40).OrderBy(u => u.DistanceSqr);

                    return ret.ToList();
                }
                catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        ///     Check for players to resurrect
        /// </summary>
        public static List<WoWPlayer> ResurrectablePlayers
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWPlayer>().Where(
                                  p => !p.IsMe &&
                                  p.Dead &&
                                  p.IsFriendly &&
                                  p.IsInMyPartyOrRaid &&
                                  p.DistanceSqr < 30 * 30);

                    return ret.ToList();
                } catch (NullReferenceException) {
                    return new List<WoWPlayer>();
                }
            }
        }

        /// <summary>
        ///     Check for players to use Chain Heal (Hop)
        /// </summary>
        public static bool ChainHealWillHop (WoWUnit target)
        {
            {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(false).Find(
                                  p => p != null
                                  && p.IsPlayer && !p.IsPet
                                  && p != target
                                  && p.IsAlive
                                  && p.HealthPercent < 95
                                  && target.Location.Distance(p.Location) <= 12);

                    return ret != null;
                } catch (NullReferenceException) {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Check for players to use Rallying Cry on (Warrior Heal)
        /// </summary>
        public static bool WarriorRallyingCryPlayers
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Any(
                                  u => u.HealthPercent < 20 &&
                                  !u.ActiveAuras.ContainsKey("Rallying Cry") &&
                                  !u.Dead &&
                                  u.IsFriendly &&
                                  u.DistanceSqr < 30 * 30);

                    return ret;
                } catch (NullReferenceException) {
                    return false;
                }
            }
        }

        /// <summary>
        /// Enemy Healers for PvP
        /// </summary>
        /// <returns>Closest Enemy Healer</returns>
        private static IEnumerable<WoWUnit> EnemyHealer
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(
                                  u =>
                                  !UnitIsControlled(u, true) &&
                                  IsAttackable(u) &&
                                  u.IsAlive &&
                                  !u.IsMe &&
                                  u.IsPlayer &&
                                  u.Distance < 30 &&
                                  !u.IsFriendly &&
                                  !u.IsPet &&
                                  u.InLineOfSpellSight &&
                                  u.IsCasting &&
                                  HealerSpells.Contains(u.CastingSpell.Name)).OrderBy(u => u.DistanceSqr);

                    return ret.ToList();
                } catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        /// Enemy players bashing on us for PvP
        /// </summary>
        /// <returns>Closest Enemy Pounding on us</returns>
        private static IEnumerable<WoWUnit> EnemysAttackingUs
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(
                                  u =>
                                  !UnitIsControlled(u, true) &&
                                  IsAttackable(u) &&
                                  u.IsAlive &&
                                  !u.IsMe &&
                                  u.IsPlayer &&
                                  u.DistanceSqr < 15 &&
                                  !u.IsFriendly &&
                                  !u.IsPet &&
                                  u.InLineOfSpellSight &&
                                  u.IsTargetingMeOrPet).OrderBy(u => u.DistanceSqr);

                    return ret.ToList();
                } catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        /// Weak low health players for PvP
        /// </summary>
        /// <returns>Closest Weak low health players</returns>
        private static IEnumerable<WoWUnit> EnemyLowHealth
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(
                                  u =>
                                  !UnitIsControlled(u, true) &&
                                  IsAttackable(u) &&
                                  u.IsAlive &&
                                  u.IsPlayer &&
                                  u.DistanceSqr < 30 &&
                                  !u.IsMe &&
                                  !u.IsFriendly &&
                                  !u.IsPet &&
                                  u.InLineOfSpellSight &&
                                  u.HealthPercent < 20).OrderBy(u => u.DistanceSqr);

                    return ret.ToList();
                }
                catch (NullReferenceException) {
                    return new List<WoWUnit>();
                }
            }
        }

        /// <summary>
        /// Flag Carriers for PvP
        /// </summary>
        /// <returns>Closest Flag Carrier</returns>
        private static WoWUnit EnemyFlagCarrier
        {
            get {
                try {
                    var ret = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).FirstOrDefault(
                                  u => !UnitIsControlled(u, true) &&
                                  IsAttackable(u) &&
                                  u.IsAlive &&
                                  !u.IsMe &&
                                  u.IsPlayer &&
                                  u.DistanceSqr < 30 &&
                                  !u.IsFriendly &&
                                  !u.IsPet &&
                                  u.InLineOfSpellSight &&
                                  (Me.IsHorde ? u.HasAura("Alliance Flag") : u.HasAura("Horde Flag")));

                    return ret;
                } catch { }
                return null;
            }
        }

        /// <summary>
        /// Returns true if the unit is a boss
        /// </summary>
        /// <param name="unit">the unit to query</param>
        /// <returns>true if a baws</returns>
        private static bool IsBoss(WoWUnit unit)
        {
            return unit != null && BossList.BossIds.Contains(unit.Entry);
        }

        /// <summary>
        /// Returns true if the unit is a Training Dummy
        /// </summary>
        /// <param name="unit">the unit to check for</param>
        /// <returns>returns true if the unit is a training dummy</returns>
        private static bool IsTrainingDummy(WoWUnit unit)
        {
            return unit != null && BossList.TrainingDummies.Contains(unit.Entry);
        }

        /// <summary>
        /// Returns a list of tanks
        /// </summary>
        public static IEnumerable<WoWPlayer> Tanks
        {
            get {
                var result = new List<WoWPlayer>();

                if (!StyxWoW.Me.IsInParty)
                    return result;

                if ((StyxWoW.Me.Role & WoWPartyMember.GroupRole.Tank) != 0)
                    result.Add(StyxWoW.Me);

                var members = StyxWoW.Me.IsInRaid ? StyxWoW.Me.RaidMemberInfos : StyxWoW.Me.PartyMemberInfos;

                var tanks = members.Where(p => (p.Role & WoWPartyMember.GroupRole.Tank) != 0);

                result.AddRange(tanks.Where(t => t.ToPlayer() != null).Select(t => t.ToPlayer()));

                return result;
            }
        }

        /// <summary>
        /// Retrieve Role via Lua
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns>true if the wowplayer is a tank</returns>
        public static bool IsTank(WoWPlayer player)
        {
            using (new FrameLock()) {
                try {
                    var retValue = player != null && Lua.GetReturnValues("return UnitGroupRolesAssigned('" + Spell.RealLuaEscape(player.Name) + "')").First() == "TANK";
                    return retValue;
                } catch {
                    CLU.TroubleshootDebugLog(Color.Red, "Lua failed in IsTank");
                    return false;
                }
            }
        }

        /// <summary>
        /// Retrieve Maintank via Lua
        /// </summary>
        /// <param name="unit">the unit to check </param>
        /// <returns>true if the wowplayer is Maintank</returns>
        public static bool IsMaintank(WoWUnit unit)
        {
            using (new FrameLock()) {
                try {
                    var retValue = unit != null && Lua.GetReturnValues("return GetPartyAssignment('MAINTANK','" + Spell.RealLuaEscape(unit.Name) + "')").First() == "1";
                    return retValue;
                } catch {
                    CLU.TroubleshootDebugLog(Color.Red, "Lua failed in IsMaintank");
                    return false;
                }
            }
        }

        /// <summary>
        /// Retrieve Offtank (MainAssist) via Lua
        /// </summary>
        /// <param name="unit">the unit to check </param>
        /// <returns>true if the wowplayer is Offtank</returns>
        public static bool IsOfftank(WoWUnit unit)
        {
            using (new FrameLock()) {
                try {
                    var retValue = unit != null && Lua.GetReturnValues("return GetPartyAssignment('MAINASSIST','" + Spell.RealLuaEscape(unit.Name) + "')").First() == "1";
                    return retValue;
                } catch {
                    CLU.TroubleshootDebugLog(Color.Red, "Lua failed in IsOfftank");
                    return false;
                }
            }
        }

        /// <summary>
        /// returns a list of healers.
        /// </summary>
        public static List<WoWPlayer> Healers
        {
            get {
                var result = new List<WoWPlayer>();

                if (!StyxWoW.Me.IsInParty)
                    return result;

                if ((StyxWoW.Me.Role & WoWPartyMember.GroupRole.Healer) != 0)
                    result.Add(StyxWoW.Me);

                var members = StyxWoW.Me.IsInRaid ? StyxWoW.Me.RaidMemberInfos : StyxWoW.Me.PartyMemberInfos;

                var tanks = members.Where(p => (p.Role & WoWPartyMember.GroupRole.Healer) != 0);

                result.AddRange(tanks.Where(t => t.ToPlayer() != null).Select(t => t.ToPlayer()));

                return result;
            }
        }

        /// <summary>Returns true if the unit is attackable</summary>
        /// <param name="unit">unit to check for</param>
        /// <returns>The is attackable.</returns>
        public static bool IsAttackable(WoWUnit unit)
        {
            // erm..yea
            if (unit == null)
                return false;

            // Blacklisted...bad mob
            if (Blacklist.Contains(unit))
                return false;

            // ignore these
            if (IgnoreMobs.Contains(unit.Entry))
                return false;

            // Ignore shit we can't select/attack
            if (!unit.CanSelect || !unit.Attackable)
                return false;

            // Ignore friendlies!
            if (unit.IsFriendly)
                return false;

            // Duh
            if (unit.Dead)
                return false;

            // on a transport
            if (unit.IsOnTransport)
                return false;

            // Mounted...whats the point?
            if (unit.Mounted)
                return false;

            // Dummies/bosses are valid by default. Period.
            if (IsBoss(unit) || IsTrainingDummy(unit))
                return true;

            // If its a pet, lets ignore it please.
            if (unit.IsPet || unit.OwnedByRoot != null)
                return false;

            // And ignore critters/non-combat pets
            if (unit.IsNonCombatPet || unit.IsCritter)
                return false;

            // no totems!
            if (unit.IsTotem)
                return false;

            return true;
        }

        /// <summary>
        /// checks if target is worth blowing a cooldown on
        /// </summary>
        /// <param name="target">the target to check</param>
        /// <returns>true or false</returns>
        public static bool IsTargetWorthy(WoWUnit target)
        {
            if (!CLUSettings.Instance.UseCooldowns)
                return false;

            if (target == null)
                return false;

            // PvP Player
            var pvpTarget = target.IsPlayer && Battlegrounds.IsInsideBattleground;

            // Miniboss not a big boss =)
            var miniBoss = (target.Level >= Me.Level + 2) && target.Elite;


            var targetIsWorthy = ((IsBoss(target) || miniBoss || IsTrainingDummy(target) || pvpTarget) && CLUSettings.Instance.BurstOn == Burst.onBoss) || (CLUSettings.Instance.BurstOn == Burst.onMob && Unit.EnemyUnits.Count() >= CLUSettings.Instance.BurstOnMobCount);
            if (targetIsWorthy) {
                CLU.DebugLog(Color.ForestGreen, String.Format("[IsTargetWorthy] {0} is a boss? {1} or miniBoss? {2} or Training Dummy? {4}. {0} current Health = {3}",
                             CLU.SafeName(target),
                             IsBoss(target),
                             miniBoss,
                             target.CurrentHealth,
                             IsTrainingDummy(target)));
            }

            return targetIsWorthy;
        }

        /// <summary>returns true if the unit is crowd controlled.</summary>
        /// <param name="unit">unit to check</param>
        /// <param name="breakOnDamageOnly">true for break on damage</param>
        /// <returns>The unit is controlled.</returns>
        public static bool UnitIsControlled(WoWUnit unit, bool breakOnDamageOnly)
        {
            return unit != null && unit.ActiveAuras.Any(x => x.Value.IsHarmful && (ControlDebuffs.Contains(x.Value.Name) || (!breakOnDamageOnly && ControlUnbreakableDebuffs.Contains(x.Value.Name))));
        }

        /// <summary>
        /// Crowd controlled
        /// </summary>
        /// <param name="unit">unit to check for</param>
        /// <returns>true if controlled</returns>
        public static bool IsCrowdControlled(WoWUnit unit)
        {
            if (unit != null) {
                Dictionary<string, WoWAura>.ValueCollection auras = unit.Auras.Values;

                return auras.Any(
                           a => a.Spell.Mechanic == WoWSpellMechanic.Banished ||
                           a.Spell.Mechanic == WoWSpellMechanic.Disoriented ||
                           a.Spell.Mechanic == WoWSpellMechanic.Charmed ||
                           a.Spell.Mechanic == WoWSpellMechanic.Horrified ||
                           a.Spell.Mechanic == WoWSpellMechanic.Incapacitated ||
                           a.Spell.Mechanic == WoWSpellMechanic.Polymorphed ||
                           a.Spell.Mechanic == WoWSpellMechanic.Sapped ||
                           a.Spell.Mechanic == WoWSpellMechanic.Shackled ||
                           a.Spell.Mechanic == WoWSpellMechanic.Asleep ||
                           a.Spell.Mechanic == WoWSpellMechanic.Frozen ||
                           a.Spell.Mechanic == WoWSpellMechanic.Invulnerable ||
                           a.Spell.Mechanic == WoWSpellMechanic.Invulnerable2 ||
                           a.Spell.Mechanic == WoWSpellMechanic.Turned ||
                           a.Spell.Mechanic == WoWSpellMechanic.Fleeing ||

                           // Really want to ignore hexed mobs.
                           a.Spell.Name == "Hex");
            }
            return false;
        }

        /// <summary>
        /// Checks to see if we are silenced or stunned (used for cancast)
        /// </summary>
        /// <param name="unit">the unit to check</param>
        /// <returns>returns is incapacitated status</returns>
        public static bool IsIncapacitated(WoWUnit unit)
        {
            return unit != null && (unit.Stunned || unit.Silenced);
        }

        /// <summary>
        /// distance to the targets bounding box
        /// </summary>
        /// <returns>Returns the distance to the targets bounding box</returns>
        public static float DistanceToTargetBoundingBox()
        {
            return (float)(Me.CurrentTarget == null ? 999999f : Math.Round(DistanceToTargetBoundingBox(Me.CurrentTarget), 0));
        }

        /// <summary>get the distance of this point to our point (taking a stab at this description)</summary>
        /// <param name="target">unit to use as the distance check</param>
        /// <returns>The distance to target bounding box.</returns>
        public static float DistanceToTargetBoundingBox(WoWUnit target)
        {
            if (target != null) {
                return (float)Math.Max(0f, target.Distance - target.BoundingRadius);
            }
            return 99999;
        }

        /// <summary>Returns the angle we are facing towards given our point to the targets point  (taking a stab at this description)</summary>
        /// <param name="me">the player</param>
        /// <param name="target">the target</param>
        /// <returns>The facing towards unit radians.</returns>
        private static float FacingTowardsUnitRadians(WoWPoint me, WoWPoint target)
        {
            try {
                WoWPoint direction = me.GetDirectionTo(target);
                direction.Normalize();
                float myFacing = ObjectManager.Me.Rotation;

                // real and safe tan reverse function
                double ret = Math.Atan2(direction.Y, direction.X);

                double alpha = Math.Abs(myFacing - ret);
                if (alpha > Math.PI) {
                    alpha = Math.Abs(2 * Math.PI - alpha);
                }

                if (Double.IsNaN(alpha)) return 0f;
                return (float)alpha;
            } catch {
                return 0f;
            }
        }

        /// <summary>Determines how we are facing the target in degrees  (taking a stab at this description)</summary>
        /// <param name="me">the player</param>
        /// <param name="target">the target</param>
        /// <returns>The facing towards unit degrees.</returns>
        public static float FacingTowardsUnitDegrees(WoWPoint me, WoWPoint target)
        {
            return (float)(FacingTowardsUnitRadians(me, target) * 180.0 / Math.PI);
        }

        private static GroupType Group
        {
            get {
                if (Me.IsInParty)
                    return GroupType.Party;
                if (Me.IsInRaid)
                    return GroupType.Raid;
                return GroupType.Single;
            }
        }

        private static GroupLogic Logic
        {
            get {
                if (Battlegrounds.IsInsideBattleground) {
                    return GroupLogic.Battleground;
                }

                return StyxWoW.Me.CurrentMap.IsArena ? GroupLogic.Arena : GroupLogic.PVE;
            }
        }

        // returns list of most focused mobs by players
        public struct FocusedUnit {
            public int PlayerCount;
            public WoWUnit Unit;
        }

        /// <summary>
        /// Refreshes the most focused unit depending on the context
        /// </summary>
        private static void RefreshMostFocusedUnits()
        {
            var hostile = ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(
                              x => IsAttackable(x) &&
                              // check for controlled units, like sheep etc
                              !UnitIsControlled(x, true));

            if (Group == GroupType.Single) {
                hostile = hostile.Where(x => x.IsHostile &&  x.DistanceSqr <= 70 * 70);
                var ret = hostile.Select(h => new FocusedUnit { Unit = h }).ToList();
                mostFocusedUnits = ret.OrderBy(x => x.Unit.DistanceSqr).ToList();
            } else {
                // raid or party
                var friends = Me.IsInRaid ? Me.RaidMembers : Me.PartyMembers;
                var ret = hostile.Select(h => new FocusedUnit { Unit = h, PlayerCount = friends.Count(x => x.CurrentTargetGuid == h.Guid) }).ToList();
                mostFocusedUnits = ret.OrderByDescending(x => x.PlayerCount).ToList();
            }
        }

        /// <summary>
        /// Refreshes units every 3 seonds.
        /// </summary>
        private static IEnumerable<FocusedUnit> MostFocusedUnits
        {
            get {
                if (DateTime.Now.Subtract(mostFocusedUnitsTimer).TotalSeconds > 3) {
                    if (Me.IsValid && StyxWoW.IsInGame) {
                        RefreshMostFocusedUnits();
                    }
                    mostFocusedUnitsTimer = DateTime.Now;
                }

                return mostFocusedUnits;
            }
        }

        /// <summary>
        /// Returns a valid target
        /// </summary>
        public static WoWUnit EnsureUnitTargeted
        {
            get {
                // If we have a RaF leader, then use its target.
                var rafLeader = RaFHelper.Leader;
                if (rafLeader != null && rafLeader.IsValid && !rafLeader.IsMe && rafLeader.Combat &&
                        rafLeader.CurrentTarget != null && rafLeader.CurrentTarget.IsAlive && !Blacklist.Contains(rafLeader.CurrentTarget)) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: RaFHelper*", CLU.SafeName(rafLeader));
                    return rafLeader.CurrentTarget;
                }

                // Healers first
                if (EnemyHealer.OrderBy(u => u.CurrentHealth).FirstOrDefault() != null && Logic == GroupLogic.Battleground) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: Healer*", CLU.SafeName(EnemyHealer.OrderBy(u => u.CurrentHealth).FirstOrDefault()));
                    return EnemyHealer.OrderBy(u => u.CurrentHealth).FirstOrDefault();
                }

                // Enemys Attacking Us
                if (EnemysAttackingUs.OrderBy(u => u.CurrentHealth).FirstOrDefault(u => u.DistanceSqr < 10) != null && Logic == GroupLogic.Battleground) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: Enemy Attacking Us*", CLU.SafeName(EnemysAttackingUs.OrderBy(u => u.CurrentHealth).FirstOrDefault(u => u.DistanceSqr < 10)));
                    return EnemysAttackingUs.OrderBy(u => u.CurrentHealth).FirstOrDefault(u => u.DistanceSqr < 10);
                }

                // Flag Carrier units
                if (EnemyFlagCarrier != null && Logic == GroupLogic.Battleground) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: Flag Carrier*", EnemyFlagCarrier);
                    return EnemyFlagCarrier;
                }

                // Low Health units
                if (EnemyLowHealth.OrderBy(u => u.CurrentHealth).FirstOrDefault() != null && Logic == GroupLogic.Battleground) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: Low Health*", CLU.SafeName(EnemyLowHealth.OrderBy(u => u.CurrentHealth).FirstOrDefault()));
                    return EnemyLowHealth.OrderBy(u => u.CurrentHealth).FirstOrDefault();
                }

                // Check bot poi.
                if (BotPoi.Current.Type == PoiType.Kill) {
                    var unit = BotPoi.Current.AsObject as WoWUnit;

                    if (unit != null && unit.IsAlive && !unit.IsMe && !Blacklist.Contains(unit)) {
                        CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: BotPoi*", CLU.SafeName(unit));
                        return unit;
                    }
                }

                // Does the target list have anything in it? And is the unit in combat?
                // Make sure we only check target combat, if we're NOT in a BG. (Inside BGs, all targets are valid!!)
                var firstUnit = Targeting.Instance.FirstUnit;
                if (firstUnit != null && firstUnit.IsAlive && !firstUnit.IsMe &&
                        (Logic != GroupLogic.Battleground ? firstUnit.Combat : firstUnit != null) && !Blacklist.Contains(firstUnit)) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: Target list*", CLU.SafeName(firstUnit));
                    return firstUnit;
                }

                // Check for Instancebuddy and Disable targeting
                if (BotChecker.BotBaseInUse("Instancebuddy")) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, " [BotChecker] Instancebuddy Detected. *TARGETING DISABLED*");
                    return null;
                }

                // Target the unit everyone else is belting on.
                if (MostFocusedUnit.Unit != null) {
                    CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging  [{0}] Reason: Most Focused*", CLU.SafeName(MostFocusedUnit.Unit));
                    return MostFocusedUnit.Unit;
                }

                //// Healing units
                // if (HealList.OrderBy(u => u.CurrentHealth).FirstOrDefault() != null && this.Logic == GroupLogic.PVE)
                // {
                // CLU.DebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting activated. *Engaging [{0}] Reason: Healing Target*", HealList.OrderBy(u => u.CurrentHealth).FirstOrDefault());
                // return HealList.OrderBy(u => u.CurrentHealth).FirstOrDefault();
                // }
                CLU.TroubleshootDebugLog(Color.Goldenrod, "[CLU] " + CLU.Version + ": CLU targeting FAILED. *Reason: I cannot find a good target.*");
                return null;
            }
        }

        /// <summary>
        /// Returns the most focused unit
        /// </summary>
        public static FocusedUnit MostFocusedUnit
        {
            get {
                return MostFocusedUnits.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns the best player to cast tricks of the trade on
        /// </summary>
        public static WoWUnit BestTricksTarget
        {
            get {
                if (!CLUSettings.Instance.Rogue.UseTricksOfTheTrade) return null;

                if (!StyxWoW.Me.IsInParty && !StyxWoW.Me.IsInRaid)
                    return null;

                // If the player has a focus target set, use it instead.
                if (StyxWoW.Me.FocusedUnitGuid != 0 && StyxWoW.Me.FocusedUnit.IsAlive)
                    return StyxWoW.Me.FocusedUnit;

                if (StyxWoW.Me.IsInInstance) {
                    if (RaFHelper.Leader != null && !RaFHelper.Leader.IsMe && RaFHelper.Leader.IsAlive) {
                        // Leader first, always. Otherwise, pick a rogue/DK/War pref. Fall back to others just in case.
                        return RaFHelper.Leader;
                    }

                    if (StyxWoW.Me.IsInParty) {
                        var bestTank = Tanks.OrderBy(t => t.DistanceSqr).FirstOrDefault(t => t.IsAlive);

                        if (bestTank != null)
                            return bestTank;
                    }

                    var bestPlayer = GetPlayerByClassPrio(
                                         100f,
                                         false,
                                         WoWClass.Rogue,
                                         WoWClass.DeathKnight,
                                         WoWClass.Warrior,
                                         WoWClass.Hunter,
                                         WoWClass.Mage,
                                         WoWClass.Warlock,
                                         WoWClass.Shaman,
                                         WoWClass.Druid,
                                         WoWClass.Paladin,
                                         WoWClass.Priest
                                         //TODO: WoWClass.Monk
                                     );
                    return bestPlayer;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the best player to cast Unholy Frenzy on
        /// </summary>
        public static WoWUnit BestUnholyFrenzyTarget
        {
            get {
                // If the player has a focus target set, use it instead.
                if (StyxWoW.Me.FocusedUnitGuid != 0 && StyxWoW.Me.FocusedUnit.IsAlive)
                    return StyxWoW.Me.FocusedUnit;

                return Me;
            }
        }

        /// <summary>
        /// Returns the best player to cast Misdirection on
        /// </summary>
        public static WoWUnit BestMisdirectTarget
        {
            get {
                // If the player has a focus target set, use it instead.
                if (StyxWoW.Me.FocusedUnitGuid != 0 && StyxWoW.Me.FocusedUnit.IsAlive)
                    return StyxWoW.Me.FocusedUnit;

                if (!StyxWoW.Me.IsInParty && !StyxWoW.Me.IsInRaid && Me.GotAlivePet)
                    return Me.Pet;

                if (StyxWoW.Me.IsInInstance) {
                    if (RaFHelper.Leader != null && !RaFHelper.Leader.IsMe && RaFHelper.Leader.IsAlive) {
                        // Leader first, always.
                        return RaFHelper.Leader;
                    }

                    if (StyxWoW.Me.IsInParty) {
                        var bestTank = Tanks.OrderBy(t => t.DistanceSqr).FirstOrDefault(t => t.IsAlive);

                        if (bestTank != null)
                            return bestTank;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the best player to cast Bane of Havoc on
        /// </summary>
        public static WoWUnit BestBaneOfHavocTarget
        {
            get {
                if (!CLUSettings.Instance.Warlock.ApplyBaneOfHavoc)
                    return null;

                // if (!StyxWoW.Me.IsInParty && !StyxWoW.Me.IsInRaid)
                //    return null;

                // If the player has a focus target set, use it instead.
                if (Me.CurrentTarget != null && (Me.CurrentTarget != StyxWoW.Me.FocusedUnit) && StyxWoW.Me.FocusedUnitGuid != 0 && Me.FocusedUnit.InLineOfSpellSight && Me.FocusedUnit.IsAlive && !Me.FocusedUnit.GetAllAuras().Any(a => a.Name == "Bane of Havoc"))
                    return StyxWoW.Me.FocusedUnit;

                var bestHostileEnemy =
                    RangedPvEUnits.Where(
                        t => t != Me.CurrentTarget).OrderBy(
                        t => t.DistanceSqr).FirstOrDefault(t => t.IsAlive && !UnitIsControlled(t, true));

                if (bestHostileEnemy != null && !bestHostileEnemy.GetAllAuras().Any(a => a.Name == "Bane of Havoc") && StyxWoW.Me.FocusedUnit == null)
                    return bestHostileEnemy;

                return null;
            }
        }

        /// <summary>Gets a player by class priority. The order of which classes are passed in, is the priority to find them.</summary>
        /// <remarks>Created 9/9/2011.</remarks>
        /// <param name="range">distance to player</param>
        /// <param name="includeDead">true or false</param>
        /// <param name="classes">A variable-length parameters list containing classes.</param>
        /// <returns>The player by class prio.</returns>
        private static WoWUnit GetPlayerByClassPrio(float range, bool includeDead, params WoWClass[] classes)
        {
            return (from woWClass in classes select StyxWoW.Me.PartyMemberInfos.FirstOrDefault(p => p.ToPlayer() != null && p.ToPlayer().Distance < range && p.ToPlayer().Class == woWClass) into unit where unit != null where !includeDead && unit.Dead || unit.Ghost select unit.ToPlayer()).FirstOrDefault();
        }

        /// <summary>
        /// Finds a target that does not have the specified spell and applys it.
        /// </summary>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="spell">The spell to be cast</param>
        /// <returns>success we have aquired a target and failure if not.</returns>
        public static Composite FindMultiDotTarget (CanRunDecoratorDelegate cond, string spell)
        {
            return new Decorator(
                       cond,
                       new Sequence(
                           // get a target
                           new Action(
                           	delegate {
                           		if (!CLUSettings.Instance.EnableMultiDotting) {
                           			return RunStatus.Failure;
                           		}

                           		WoWUnit target = RangedPvEUnits.FirstOrDefault(u => !u.HasAura(spell) && u.Distance2DSqr < 40 * 40);
                           		if (target != null) {
                           			CLU.DebugLog(Color.ForestGreen,target.Name);
                           			target.Target();
                           			return RunStatus.Success;
                           		}

                           		return RunStatus.Failure;
                           	}),
                           new Action(a => StyxWoW.SleepForLagDuration()),
                           // if success, keep going. Else quit
                           new PrioritySelector(Buff.CastDebuff(spell, cond, spell))));
        }

        /// <summary>Locates nearby units from location</summary>
        /// <param name="fromLocation">units location</param>
        /// <param name="radius">radius</param>
        /// <param name="playersOnly">true for players only</param>
        /// <returns>The nearby units.</returns>
        private static List<WoWUnit> NearbyUnits(WoWPoint fromLocation, double radius, bool playersOnly)
        {
            List<WoWUnit> hostile = ObjectManager.GetObjectsOfType<WoWUnit>(true, false);
            var maxDistance2 = radius * radius;

            if (playersOnly) {
                hostile = hostile.Where(x =>
                                        x.IsPlayer && IsAttackable(x)
                                        && x.Location.Distance2DSqr(fromLocation) < maxDistance2).ToList();
            } else {
                hostile = hostile.Where(x =>
                                        !x.IsPlayer && IsAttackable(x)
                                        && x.Location.Distance2DSqr(fromLocation) < maxDistance2).ToList();
            }

            CLU.DebugLog(Color.ForestGreen, "CountEnnemiesInRange");
            foreach (var u in hostile)
                CLU.DebugLog(Color.ForestGreen, " -> " + CLU.SafeName(u) + " " + u.Level);
            CLU.DebugLog(Color.ForestGreen, "---------------------");
            return hostile;
        }

        /// <summary>Locates nearby units from location that are crowd controlled</summary>
        /// <param name="fromLocation">units location</param>
        /// <param name="radius">radius</param>
        /// <param name="playersOnly">true for players only</param>
        /// <returns>The nearby units.</returns>
        public static IEnumerable<WoWUnit> NearbyControlledUnits(WoWPoint fromLocation, double radius, bool playersOnly)
        {
            var hostile = ObjectManager.GetObjectsOfType<WoWUnit>(true, false);
            var maxDistance2 = radius * radius;

            if (playersOnly) {
                hostile = hostile.Where(x =>
                                        x.IsPlayer && IsAttackable(x) && IsCrowdControlled(x)
                                        && x.Location.Distance2D(fromLocation) < maxDistance2).ToList();
            } else {
                hostile = hostile.Where(x =>
                                        !x.IsPlayer && IsAttackable(x) && IsCrowdControlled(x)
                                        && x.Location.Distance2D(fromLocation) < maxDistance2).ToList();
            }

            if (CLUSettings.Instance.EnableDebugLogging) {
                CLU.DebugLog(Color.ForestGreen, "CountControlledEnemiesInRange");
                foreach (var u in hostile)
                    CLU.DebugLog(Color.ForestGreen, " -> " + CLU.SafeName(u) + " " + u.Level);
                CLU.DebugLog(Color.ForestGreen, "---------------------");
            }

            return hostile;
        }

        /// <summary>Locates nearby units from location that are not crowd controlled</summary>
        /// <param name="fromLocation">units location</param>
        /// <param name="radius">radius</param>
        /// <param name="playersOnly">true for players only</param>
        /// <returns>The nearby units.</returns>
        public static IEnumerable<WoWUnit> NearbyNonControlledUnits(WoWPoint fromLocation, double radius, bool playersOnly)
        {
            var hostile = ObjectManager.GetObjectsOfType<WoWUnit>(true, false);
            var maxDistance2 = radius * radius;

            if (playersOnly) {
                hostile = hostile.Where(x =>
                                        x.IsPlayer && IsAttackable(x) && !IsCrowdControlled(x)
                                        && x.Location.Distance2D(fromLocation) < maxDistance2).ToList();
            } else {
                hostile = hostile.Where(x =>
                                        !x.IsPlayer && IsAttackable(x) && !IsCrowdControlled(x)
                                        && x.Location.Distance2D(fromLocation) < maxDistance2).ToList();
            }

            if (CLUSettings.Instance.EnableDebugLogging) {
                CLU.DebugLog(Color.ForestGreen, "CountNonControlledEnemiesInRange");
                foreach (var u in hostile)
                    CLU.DebugLog(Color.ForestGreen, " -> " + CLU.SafeName(u) + " " + u.Level);
                CLU.DebugLog(Color.ForestGreen, "---------------------");
            }

            return hostile;
        }

        /// <summary>returns the amount of targets from the units location</summary>
        /// <param name="fromLocation">units location</param>
        /// <param name="maxRange">maximum range</param>
        /// <returns>The count ennemies in range.</returns>
        public static int CountEnnemiesInRange(WoWPoint fromLocation, double maxRange)
        {
            return CLUSettings.Instance.UseAoEAbilities ? NearbyUnits(fromLocation, maxRange, Battlegrounds.IsInsideBattleground).Count : 0;
        }

        /// <summary>Finds clustered targets</summary>
        /// <param name="radius">radius</param>
        /// <param name="minDistance">minimum distance</param>
        /// <param name="maxDistance">maximum distance</param>
        /// <param name="minTargets">minimum targets to qualify</param>
        /// <param name="playersOnly">true for players only</param>
        /// <returns>The find cluster targets.</returns>
        public static WoWPoint FindClusterTargets(double radius, double minDistance, double maxDistance, int minTargets, bool playersOnly)
        {
            List<WoWUnit> hostile = ObjectManager.GetObjectsOfType<WoWUnit>(true, false);
            var avoid = new List<WoWUnit>();
            var maxDistance2 = (maxDistance + radius) * (maxDistance + radius);

            if (playersOnly) {
                hostile = hostile.Where(x =>
                                        x.IsPlayer &&
                                        IsAttackable(x) && x.Distance2DSqr < maxDistance2).ToList();
            } else {
                hostile = hostile.Where(x =>
                                        !x.IsPlayer &&
                                        IsAttackable(x) && x.Distance2DSqr < maxDistance2).ToList();
                avoid = hostile.Where(
                            x => // check for controlled units, like sheep etc
                            UnitIsControlled(x, true)).ToList();
            }

            if (hostile.Count < minTargets) {
                return WoWPoint.Empty;
            }

            var score = minTargets - 1;
            var best = WoWPoint.Empty;

            for (var x = Me.Location.X - maxDistance; x <= Me.Location.X + maxDistance; x++) {
                for (var y = Me.Location.Y - maxDistance; y <= Me.Location.Y + maxDistance; y++) {
                    var spot = new WoWPoint(x, y, Me.Location.Z);
                    var dSquare = spot.Distance2DSqr(Me.Location);
                    if (dSquare > maxDistance * maxDistance || dSquare < minDistance * minDistance) {
                        continue;
                    }

                    if (avoid.Any(t => t.Location.Distance2DSqr(spot) <= radius * radius)) {
                        continue;
                    }

                    var hits = hostile.Count(t => t.Location.DistanceSqr(spot) < radius * radius);
                    if (hits > score) {
                        best = spot;
                        score = hits;
                        CLU.DebugLog(Color.ForestGreen, "ClusteredTargets(range=" + minDistance + "-" + maxDistance + ", radius=" + radius + ") => SCORE=" + score + " at " + spot);
                        foreach (var u in hostile.Where(t => t.Location.DistanceSqr(spot) < radius * radius))
                            CLU.DebugLog(Color.ForestGreen, " -> " + CLU.SafeName(u) + " " + u.Level);
                        CLU.DebugLog(Color.ForestGreen, "---------------------");
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// Predicts target time to death
        /// credits to Stormchasing for providing this code
        /// </summary>
        private static uint first_life;
        private static uint first_life_max;
        private static int first_time;
        private static uint current_life;
        private static int current_time;

        private static ulong guid;

        private static int conv_Date2Timestam(DateTime _time)
        {
            var date1 = new DateTime(1970, 1, 1); // Reference (Set)
            DateTime date2 = _time; // date / time
            var ts = new TimeSpan(date2.Ticks - date1.Ticks); // Calculate the delta
            // The Delta as a whole number of seconds, the timestamp
            return (Convert.ToInt32(ts.TotalSeconds));
        }

        public static long TimeToDeath(WoWUnit target)
        {
            if (target == null) return 0;
            if (IsTrainingDummy(target)) return 9999; // added for DoT's and Black arrow and shit so users wont post.."But its not using XXX abilitie" when their fucking around on the training dummy.
            if (target.CurrentHealth == 0 || target.Dead || !target.IsValid || !target.IsAlive) {
                return 0;
            }
            // Fill variables on new target or on target switch, this will loose all calculations from last target
            if (guid != target.Guid) {
                guid = target.Guid;
                first_life = target.CurrentHealth;
                first_life_max = target.MaxHealth;
                first_time = conv_Date2Timestam(DateTime.Now); // Lets do a little trick and calculate with seconds / u know Timestamp from unix? we'll do so too
            }

            current_life = target.CurrentHealth;
            current_time = conv_Date2Timestam(DateTime.Now);
            var time_diff = current_time - first_time;
            var hp_diff = first_life - current_life;
            if (hp_diff > 0) {
                /*
                 * Rule of three (Dreisatz):
                 * If in a given timespan a certain value of damage is done, what timespan is needed to do 100% damage?
                 * The longer the timespan the more precise the prediction
                 * time_diff/hp_diff = x/first_life_max
                 * x = time_diff*first_life_max/hp_diff
                 */
                var full_time = time_diff * first_life_max / hp_diff;
                var past_first_time = (first_life_max - first_life) * time_diff / hp_diff;
                var calc_time = first_time - past_first_time + full_time - current_time;
                if (calc_time < 1) calc_time = 99;
                // commented out - caused exceptions when time_diff is 0 and does take no effect while calculating!
                // var dps = hp_diff / time_diff;
                var time_to_die = calc_time;
                var fight_length = full_time;
                return time_to_die;
            }

            if (hp_diff < 0) {
                // unit was healed,resetting the initial values
                guid = target.Guid;
                first_life = target.CurrentHealth;
                first_life_max = target.MaxHealth;
                first_time = conv_Date2Timestam(DateTime.Now); // Lets do a little trick and calculate with seconds / u know Timestamp from unix? we'll do so too
                return -1;
            }

            if (current_life == first_life_max) {
                return 9999;
            }

            return -1;
        }
    }
}