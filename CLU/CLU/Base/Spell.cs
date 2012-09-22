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

using System.Reflection;

namespace CLU.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CommonBehaviors.Actions;
    using Styx;
    using Styx.CommonBot;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using Styx.TreeSharp;
    using System.Diagnostics;

    using global::CLU.Helpers;
    using global::CLU.Lists;
    using global::CLU.Settings;
    using Action = Styx.TreeSharp.Action;

    internal static class Spell
    {
        /* putting all the spell logic here */

        /// <summary>
        /// known channeled spells. used as part of the isChannelled spell check method
        /// </summary>
        public static readonly HashSet<string> KnownChanneledSpells = new HashSet<string>();

        // Specify the override ID of an original spell to make the spell manager CanCast(originalId) - recommended by Apoc.
        public static HashSet<int> CanCastUseOriginals = new HashSet<int>
        {
            102355, // Faerie Swarm
        };


        /// <summary>
        /// Me! or is it you?
        /// </summary>
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        /// <summary>
        /// Clip time to account for lag
        /// </summary>
        /// <returns>Time to begin next channel</returns>
        public static double ClippingDuration()
        {
            return CLUSettings.Instance.Priest.MindFlayClippingDuration;
        }

        /// <summary>
        /// a shortcut too GlobalCooldownLeft
        /// </summary>
        public static double GCD
        {
            get
            {
                return 1; //SpellManager.GlobalCooldownLeft.TotalSeconds;
            }
        }

        /// <summary>
        /// Returns true if the player is currently channeling a spell
        /// </summary>
        public static bool PlayerIsChanneling
        {
            get
            {
                return ChanneledTimeLeft > TimeSpan.FromSeconds(0);//StyxWoW.Me.ChanneledCastingSpellId != 0;
            }
        }

        /// <summary>Returns the current casttime of the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>The cast time.</returns>
        public static double CastTime(string name)
        {
            try
            {
                if (!SpellManager.HasSpell(name))
                    return 999999.9;

                WoWSpell s = SpellManager.Spells[name];
                return s.CastTime / 1000.0;
            }
            catch
            {
                CLU.DiagnosticLog("[ERROR] in CastTime: {0} ", name);
                return 999999.9;
            }
        }

        /// <summary>Returns the spellcooldown using Timespan (00:00:00.0000000)
        /// gtfo if the player dosn't have the spell.</summary>
        /// <returns>The spell cooldown.</returns>
        public static TimeSpan SpellCooldown(string spell)
        {
            return SpellManager.HasSpell(spell) ? SpellManager.Spells[spell].CooldownTimeLeft : TimeSpan.MaxValue;
        }


        /// <summary>Returns the true if the spell is on cooldown (ie: its been used)
        /// gtfo if the player dosn't have the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>true if the spell is currently on cooldown</returns>
        public static bool SpellOnCooldown(string name)
        {
            // Fishing for KeyNotFoundException's yay!
            
            if (!SpellManager.HasSpell(name))
                return false;

            var spellToCheck = GetSpellByName(name); // Convert the string name to a WoWspell

            return spellToCheck.Cooldown;
        }

        /// <summary>
        ///  Creates a composite that will return a success, so long as you are currently casting. (Use this to prevent the CC from
        ///  going down to lower branches in the tree, while casting.)
        /// </summary>
        /// <param name="allowLagTollerance">Whether or not to allow lag tollerance for spell queueing</param>
        /// <returns>success, so long as you are currently casting.</returns>
        public static Composite WaitForCast(bool allowLagTollerance = true)
        {
            return new Action(ret =>
            {
                if (!StyxWoW.Me.IsCasting)
                    return RunStatus.Failure;

                if (StyxWoW.Me.ChannelObjectGuid > 0)
                    return RunStatus.Failure;

                uint latency = StyxWoW.WoWClient.Latency * 2;
                TimeSpan castTimeLeft = StyxWoW.Me.CurrentCastTimeLeft;
                if (allowLagTollerance && castTimeLeft != TimeSpan.Zero &&
                    StyxWoW.Me.CurrentCastTimeLeft.TotalMilliseconds < latency)
                    return RunStatus.Failure;

                // return RunStatus.Running;
                return RunStatus.Success;
            });
        }

        /// <summary>
        /// This is meant to replace the 'SleepForLagDuration()' method. Should only be used in a Sequence
        /// </summary>
        public static Composite CreateWaitForLagDuration()
        {
            return new WaitContinue(TimeSpan.FromMilliseconds((StyxWoW.WoWClient.Latency * 2) + 150), ret => false, new ActionAlwaysSucceed());
        }


        public static float ActualMaxRange(this WoWSpell spell, WoWUnit unit)
        {
            if (spell.MaxRange == 0)
                return 0;
            return unit != null ? spell.MaxRange + unit.CombatReach + 1f : spell.MaxRange;
        }

        public static float ActualMinRange(this WoWSpell spell, WoWUnit unit)
        {
            if (spell.MinRange == 0)
                return 0;
            return unit != null ? spell.MinRange + unit.CombatReach + 1.6666667f : spell.MinRange;
        }

        /// <summary>
        ///  Returns the current Melee range for the player Unit.DistanceToTargetBoundingBox(target)
        /// </summary>
        public static float MeleeRange
        {
            get
            {
                // If we have no target... then give nothing.
                // if (StyxWoW.Me.CurrentTargetGuid == 0)  // chg to GotTarget due to non-zero vals with no target in Guid
                if (!StyxWoW.Me.GotTarget)
                    return 0f;

                if (StyxWoW.Me.CurrentTarget.IsPlayer)
                    return 3.5f;

                return Math.Max(5f, StyxWoW.Me.CombatReach + 1.3333334f + StyxWoW.Me.CurrentTarget.CombatReach);
            }
        }

        /// <summary>This is CLU's cancast method. It checks ALOT! Returns true if the player can cast the spell.</summary>
        /// <param name="name">name of the spell to check.</param>
        /// <param name="target">The target.</param>
        /// <returns>The can cast.</returns>
        public static bool CanCast(string name, WoWUnit target)
        {

            var canCast = false;
            var inRange = false;
            var minReqs = target != null;
            if (minReqs)
            {

                canCast = SpellManager.CanCast(name, target, false, false);

                if (canCast)
                {
                    // We're always in range of ourselves. So just ignore this bit if we're casting it on us
                    if (target.IsMe)
                    {
                        inRange = true;
                    }
                    else
                    {
                        WoWSpell spell;
                        if (SpellManager.Spells.TryGetValue(name, out spell))
                        {
                            float minRange = spell.ActualMinRange(target);
                            float maxRange = spell.ActualMaxRange(target);
                            var targetDistance = Unit.DistanceToTargetBoundingBox(target);

                            // RangeId 1 is "Self Only". This should make life easier for people to use self-buffs, or stuff like Starfall where you cast it as a pseudo-buff.
                            if (spell.IsSelfOnlySpell)
                                inRange = true;
                            // RangeId 2 is melee range. Huzzah :)
                            else if (spell.IsMeleeSpell)
                                inRange = targetDistance < MeleeRange;
                            else
                                inRange = targetDistance < maxRange &&
                                          targetDistance > (Math.Abs(minRange - 0) < 0.01 ? minRange : minRange + 3);
                        }
                    }
                }
            }

            return minReqs && canCast && inRange;
        }

        private static bool CanCast(WoWSpell spell, WoWUnit target)
        {
            if (target == null)
            {
                CLU.DiagnosticLog("{0}({1},{2}): Target is null.", MethodBase.GetCurrentMethod().Name, spell.Name, target.Name);
                return false;
            }

            if (!spell.CanCast)
            {
                CLU.DiagnosticLog("{0}({1},{2}): CanCast failed.", MethodBase.GetCurrentMethod().Name, spell.Name, target.Name);
                return false;
            }

            if (target.IsMe)
                return true;

            float minRange = spell.ActualMinRange(target);
            float maxRange = spell.ActualMaxRange(target);
            var targetDistance = Unit.DistanceToTargetBoundingBox(target);

            // RangeId 1 is "Self Only". This should make life easier for people to use self-buffs, or stuff like Starfall where you cast it as a pseudo-buff.
            if (spell.IsSelfOnlySpell)
                return true;
            // RangeId 2 is melee range. Huzzah :)
            if (spell.IsMeleeSpell)
                return targetDistance < MeleeRange;

            bool inRange = targetDistance < maxRange &&
                           targetDistance > (Math.Abs(minRange - 0) < 0.01 ? minRange : minRange + 3);

            if (!inRange)
            {
                CLU.DiagnosticLog("{0}({1},{2}): Not in range.", MethodBase.GetCurrentMethod().Name, spell.Name, target.Name);
                return false;
            }

            return true;
        }

        #region CastSpell - by ID

        /// <summary>Casts self spells by ID</summary>
        /// <param name="spellid">the id of the spell to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSelfSpell(int spellid, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(spellid, x => StyxWoW.Me, cond, label);
        }

        /// <summary>Casts a spell by ID on a target</summary>
        /// <param name="spellid">the id of the spell to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpell(int spellid, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(spellid, ret => Me.CurrentTarget, cond, label);
        }

        public static Composite CastSpell(int spellid, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            var spell = WoWSpell.FromId(spellid);
            return new Decorator
                (context =>
                {
                    if (!cond(context))
                    {
                        return false;
                    }
                    if (spell == null)
                    {
                        CLU.TroubleshootLog("SpellID not found: {0}", spellid);
                        return false;
                    }

                    var target = onUnit(context);

                    if (target == null)
                    {
                        CLU.TroubleshootLog("Target not found.");
                        return false;
                    }

                    var canCast = CanCast(spell, target);

                    if (!canCast)
                    {
                        return false;
                    }

                    return true;
                },
                 new Sequence
                     (new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                      new Action(a => SpellManager.Cast(spell, onUnit(a)))));
        }

        #endregion

        #region CastSpell - by name
        /// <summary>Casts a spell by name on a target</summary>
        /// <param name="name">the name of the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(name, ret => Me.CurrentTarget, cond, label);
        }

        /// <summary>Casts a spell by name on a target</summary>
        /// <param name="spell">the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpell(WoWSpell spell, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(spell, ret => Me.CurrentTarget, cond, label);
        }

        /// <summary>Casts a spell on a specified unit</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell on the unit</returns>
        public static Composite CastSpell(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            /*WoWSpell spell;
            bool spellIsOnCoolDown = false;
            if (SpellManager.Spells.TryGetValue(name, out spell))
            {
                if (spell != null) spellIsOnCoolDown = !spell.Cooldown;
            }*/
            return new Decorator(
                delegate(object a)
                {
                    if (!cond(a))
                        return false;
                    //CLU.TroubleshootLog("Cancast: {0} = {1}", name, CanCast(name, onUnit(a)));
                    if (!CanCast(name, onUnit(a)))
                        return false;

                    return onUnit(a) != null;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Action(a => SpellManager.Cast(name, onUnit(a)))));
        }

        /// <summary>Casts a spell on a specified unit</summary>
        /// <param name="spell">the name of the spell to cast</param>
        /// <param name="onUnit">The Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell on the unit</returns>
        public static Composite CastSpell(WoWSpell spell, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!cond(a))
                        return false;
                    //CLU.TroubleshootLog("Cancast: {0} = {1}", name, CanCast(name, onUnit(a)));
                    if (!CanCast(spell, onUnit(a)))
                        return false;

                    return onUnit(a) != null;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Action(a => SpellManager.Cast(spell, onUnit(a)))));
        }

        /// <summary>Casts self spells eg: 'Fient', 'Shield Wall', 'Blood Tap', 'Rune Tap' </summary>
        /// <param name="name">the name of the spell in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast self spell.</returns>
        public static Composite CastSelfSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return CastSpell(name, ret => StyxWoW.Me, cond, label);
        }
        #endregion

        #region CastSpell - by Specific Requirements and Functionality

        /// <summary>Casts a spell by name on a target without checking the Cancast Method</summary>
        /// <param name="name">the name of the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastSpecialSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!cond(a))
                        return false;

                    return true;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} ", label)), new Action(a => SpellManager.Cast(name))));
        }

        /// <summary>Casts a spell on a specified unit (used primarily for healing)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell on the unit</returns>
        public static Composite CastHeal(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Sequence(
                       CastSpell(name, onUnit, cond, label),
                       new WaitContinue(
                           1,
                           ret =>
                           {
                               WoWSpell spell;
                               if (SpellManager.Spells.TryGetValue(name, out spell))
                               {
                                   if (spell.CastTime == 0)
                                   {
                                       return true;
                                   }

                                   return StyxWoW.Me.IsCasting;
                               }

                               return true;
                           },
            new ActionAlwaysSucceed()),
                       new WaitContinue(
                           10,
                           ret =>
                           {
                               // Dont interupt chanelled spells
                               if (StyxWoW.Me.ChanneledCastingSpellId != 0)
                               {
                                   return false;
                               }

                               // Interrupted or finished casting. Continue
                               if (!StyxWoW.Me.IsCasting)
                               {
                                   return true;
                               }

                               // If conditions are no longer valid, stop casting and continue
                               if (!cond(ret))
                               {
                                   SpellManager.StopCasting();
                                   return true;
                               }

                               return false;
                           },
            new ActionAlwaysSucceed()));
        }

        public static Composite CastHeal(string name, CanRunDecoratorDelegate cond, string label)
        {
            return CastHeal(name, ret => HealableUnit.HealTarget.ToUnit(), cond, label);
        }
        /// <summary>Casts a spell on the MostFocused Target (used for smite healing with disc priest mainly)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <param name="faceTarget">true if you want to auto face target</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastSpellOnTargetFacing(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label, bool faceTarget)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!cond(a))
                        return false;

                    if (!CanCast(name, onUnit(a)))
                        return false;

                    return onUnit(a) != null;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Decorator(x => faceTarget && !StyxWoW.Me.IsSafelyFacing(onUnit(x), 45f), new Action(a => WoWMovement.Face(onUnit(a).Guid))),
                new Action(a => SpellManager.Cast(name, onUnit(a)))));
        }

        /// <summary>Casts a spell on the CurrentTargets Target (used for smite healing with disc priest mainly)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <param name="faceTarget">true if you want to auto face target</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastSpellOnCurrentTargetsTarget(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label, bool faceTarget)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (onUnit == null || onUnit(a) == null)
                        return false;

                    if (onUnit(a).CurrentTarget == null)
                        return false;

                    if (!cond(a))
                        return false;

                    if (onUnit(a).Guid == Me.Guid)
                        return false;

                    if (!CanCast(name, onUnit(a).CurrentTarget))
                        return false;

                    // if (Unit.TimeToDeath(onUnit(a).CurrentTarget) < 5)
                    // return false;

                    return (Unit.NearbyNonControlledUnits(onUnit(a).Location, 15, false).Any() || BossList.IgnoreRangeCheck.Contains(onUnit(a).CurrentTarget.Entry));
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a).CurrentTarget))),
                new DecoratorContinue(x => faceTarget, new Action(a => WoWMovement.Face(onUnit(a).CurrentTarget.Guid))),
                new Action(a => SpellManager.Cast(name, onUnit(a).CurrentTarget))));
        }

        /// <summary>Casts a spell on the MostFocused Target (used for smite healing with disc priest mainly)</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <param name="faceTarget">true if you want to auto face target</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastSpellOnMostFocusedTarget(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label, bool faceTarget)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (onUnit == null || onUnit(a) == null)
                        return false;

                    if (!cond(a))
                        return false;

                    if (!CanCast(name, onUnit(a)))
                        return false;

                    // if (Unit.TimeToDeath(onUnit(a).CurrentTarget) < 5)
                    // return false;

                    return (Unit.NearbyNonControlledUnits(onUnit(a).Location, 15, false).Any() || BossList.IgnoreRangeCheck.Contains(onUnit(a).CurrentTarget.Entry));
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new DecoratorContinue(x => faceTarget, new Action(a => WoWMovement.Face(onUnit(a).Guid))),
                new Action(a => SpellManager.Cast(name, onUnit(a)))));
        }

        /// <summary>Casts a Totem by name</summary>
        /// <param name="name">the name of the spell to cast in engrish</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell.</returns>
        public static Composite CastTotem(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.Shaman.HandleTotems)
                        return false;

                    if (!cond(a))
                        return false;

                    if (!CanCast(name, Me))
                        return false;

                    return true;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Totem] {0} ", label)),
                new Action(a => SpellManager.Cast(name))));
        }

        /// <summary>Casts a spell provided we are inrange and facing the target </summary>
        /// <param name="name">name of the spell to cast</param>
        /// <param name="maxDistance">maximum distance</param>
        /// <param name="maxAngleDeltaDegrees">maximum angle in degrees</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast conic spell.</returns>
        public static Composite CastConicSpell(
            string name, float maxDistance, float maxAngleDeltaDegrees, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       a => Me.CurrentTarget != null && cond(a) && CanCast(name, Me.CurrentTarget) &&
                       Unit.DistanceToTargetBoundingBox() <= maxDistance &&
                       Unit.FacingTowardsUnitDegrees(Me.Location, Me.CurrentTarget.Location) <= maxAngleDeltaDegrees,
                       new Sequence(
                           new Action(a => CLU.Log(" [Casting Conic] {0} ", label)),
                           new Action(a => SpellManager.Cast(name))));
        }

        #endregion

        #region CastInterupt - by Name
        /// <summary>Casts the interupt by name on the provided target. Checks CanInterruptCurrentSpellCast.</summary>
        /// <param name="name">the name of the spell in english</param>
        /// <param name="onUnit">the unit to cas the interupt on. </param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast interupt.</returns>
        public static Composite CastInterupt(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.EnableInterupts)
                        return false;

                    if (!cond(a))
                        return false;

                    if (onUnit != null && !(onUnit(a).IsCasting && onUnit(a).CanInterruptCurrentSpellCast))
                        return false;

                    if (onUnit != null && CanCast(name, onUnit(a)))
                        return false;

                    return true;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Interupt] {0} on {1}", label, CLU.SafeName(onUnit(a)))),
                new Action(a => SpellManager.Cast(name, onUnit(a)))));
        }

        /// <summary>Casts the interupt by name on your current target. Checks CanInterruptCurrentSpellCast.</summary>
        /// <param name="name">the name of the spell in english</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast interupt.</returns>
        public static Composite CastInterupt(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.EnableInterupts)
                        return false;

                    if (!cond(a))
                        return false;

                    if (Me.CurrentTarget != null && !(Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast))
                        return false;

                    if (Me.CurrentTarget != null && !CanCast(name, Me.CurrentTarget))
                        return false;

                    return true;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Interupt] {0} on {1}", label, CLU.SafeName(Me.CurrentTarget))),
                new Action(a => SpellManager.Cast(name))));
        }

        #endregion

        #region ChanneledSpell - by name
        /// <summary>Returns true if the spell is a known channeled spell</summary>
        /// <param name="name">the name of the spell to check</param>
        /// <returns>The is channeled spell.</returns>
        private static bool IsChanneledSpell(string name)
        {
            return KnownChanneledSpells != null && KnownChanneledSpells.Contains(name);
        }

        /// <summary>Channel spell on target. Will not break channel and adds the name of spell to knownChanneledSpells</summary>
        /// <param name="name">the name of the spell to channel</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel spell.</returns>
        public static Composite ChannelSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            KnownChanneledSpells.Add(name);
            WoWSpell spell;
            SpellManager.Spells.TryGetValue(name, out spell);
            return
                new PrioritySelector(
                    new Decorator(x => KnownChanneledSpells.Contains(name) && ChanneledTimeLeft > TimeSpan.FromSeconds(0), //TODO: HB fix PlayerIsChanneling && Me.ChanneledCastingSpellId == spell.Id
                            new Action(a => CLU.Log(" [Channeling] {0} : {1} seconds remaining", name, ChanneledTimeLeft))),
                    CastSpell(name, cond, label));
        }

        /// <summary>Channel spell on target. Will not break channel and adds the name of spell to knownChanneledSpells</summary>
        /// <param name="spellId">the name of the spell to channel</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel spell.</returns>
        public static Composite ChannelSpell(int spellId, CanRunDecoratorDelegate cond, string label)
        {
            var spell = WoWSpell.FromId(spellId);
            KnownChanneledSpells.Add(spell.Name);
            return
                new PrioritySelector(
                    new Decorator(
                        x => KnownChanneledSpells.Contains(spell.Name) && ChanneledTimeLeft > TimeSpan.FromSeconds(0), //TODO: HB fix PlayerIsChanneling && Me.ChanneledCastingSpellId == spell.Id
                        new Action(a => CLU.Log(" [Channeling] {0} : {1} seconds remaining", spell.Name, ChanneledTimeLeft))),
                    CastSpell(spell, cond, label));
        }

        /// <summary>Channel spell on player. Will not break channel and adds the name of spell to _knownChanneledSpells</summary>
        /// <param name="name">the name of the spell to channel</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel self spell.</returns>
        public static Composite ChannelSelfSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            KnownChanneledSpells.Add(name);
            return
                new PrioritySelector(
                    new Decorator(
                        x => PlayerIsChanneling && KnownChanneledSpells.Contains(name),
                        new Action(a => CLU.Log(" [Channeling] {0} ", name))),
                    CastSelfSpell(name, cond, label));
        }
        #endregion

        #region CastOnGround - placeable spell casting
        public delegate WoWPoint LocationRetriever(object context);

        /// <summary>
        ///   Creates a behavior to cast a spell by name, on the ground at the specified location. Returns
        ///   RunStatus.Success if successful, RunStatus.Failure otherwise.
        /// </summary>
        /// <remarks>
        ///   Created 5/2/2011.
        /// </remarks>
        /// <param name = "spell">The spell.</param>
        /// <param name = "onLocation">The on location.</param>
        /// <returns>.</returns>
        public static Composite CastOnGround(string spell, LocationRetriever onLocation)
        {
            return CastOnGround(spell, onLocation, ret => true);
        }

        /// <summary>
        ///   Creates a behavior to cast a spell by name, on the ground at the specified location. Returns RunStatus.Success if successful, RunStatus.Failure otherwise.
        /// </summary>
        /// <remarks>
        ///   Created 5/2/2011.
        /// </remarks>
        /// <param name = "spell">The spell.</param>
        /// <param name = "onLocation">The on location.</param>
        /// <param name = "requirements">The requirements.</param>
        /// <returns>.</returns>
        public static Composite CastOnGround(string spell, LocationRetriever onLocation,
            CanRunDecoratorDelegate requirements)
        {
            return CastOnGround(spell, onLocation, requirements, true);
        }

        /// <summary>
        ///   Creates a behavior to cast a spell by Id, on the ground at the specified location. Returns RunStatus.Success if successful, RunStatus.Failure otherwise.
        /// </summary>
        /// <remarks>
        ///   Created 5/2/2011.
        /// </remarks>
        /// <param name = "spellid">The spell Id</param>
        /// <param name = "onLocation">The on location.</param>
        /// <param name = "requirements">The requirements.</param>
        /// <returns>.</returns>
        public static Composite CastOnGround(int spellid, LocationRetriever onLocation,
            CanRunDecoratorDelegate requirements)
        {
            return CastOnGround(spellid, onLocation, requirements, true);
        }


        /// <summary>
        ///   Creates a behavior to cast a spell by name, on the ground at the specified location. Returns RunStatus.Success if successful, RunStatus.Failure otherwise.
        /// </summary>
        /// <remarks>
        ///   Created 5/2/2011.
        /// </remarks>
        /// <param name = "spell">The spell.</param>
        /// <param name = "onLocation">The on location.</param>
        /// <param name = "requirements">The requirements.</param>
        /// <param name="waitForSpell">Waits for spell to become active on cursor if true. </param>
        /// <returns>.</returns>
        public static Composite CastOnGround(string spell, LocationRetriever onLocation,
            CanRunDecoratorDelegate requirements, bool waitForSpell)
        {
            return
                new Decorator(
                    ret =>
                    requirements(ret) && onLocation != null && SpellManager.CanCast(spell) && CLUSettings.Instance.UseAoEAbilities &&
                    (StyxWoW.Me.Location.Distance(onLocation(ret)) <= SpellManager.Spells[spell].MaxRange ||
                     SpellManager.Spells[spell].MaxRange == 0),
                    new Sequence(
                        new Action(ret => CLU.Log("Casting {0} at location {1}", spell, onLocation(ret))),
                        new Action(ret => SpellManager.Cast(spell)),

                        new DecoratorContinue(ctx => waitForSpell,
                            new WaitContinue(1,
                                ret =>
                                StyxWoW.Me.CurrentPendingCursorSpell != null &&
                                StyxWoW.Me.CurrentPendingCursorSpell.Name == spell, new ActionAlwaysSucceed())),

                        new Action(ret => SpellManager.ClickRemoteLocation(onLocation(ret)))));
        }

        /// <summary>
        ///   Creates a behavior to cast a spell by Id, on the ground at the specified location. Returns RunStatus.Success if successful, RunStatus.Failure otherwise.
        /// </summary>
        /// <remarks>
        ///   Created 5/2/2011.
        /// </remarks>
        /// <param name = "spellid">The spell Id</param>
        /// <param name = "onLocation">The on location.</param>
        /// <param name = "requirements">The requirements.</param>
        /// <param name="waitForSpell">Waits for spell to become active on cursor if true. </param>
        /// <returns>.</returns>
        public static Composite CastOnGround(int spellid, LocationRetriever onLocation,
            CanRunDecoratorDelegate requirements, bool waitForSpell)
        {
            return
                new Decorator(
                    ret =>
                    requirements(ret) && onLocation != null && CLUSettings.Instance.UseAoEAbilities,
                    new Sequence(
                        new Action(ret => CLU.Log("Casting {0} at location {1}", spellid, onLocation(ret))),
                        new Action(ret => SpellManager.Cast(spellid)),

                        new DecoratorContinue(ctx => waitForSpell,
                            new WaitContinue(1,
                                ret =>
                                StyxWoW.Me.CurrentPendingCursorSpell != null &&
                                StyxWoW.Me.CurrentPendingCursorSpell.Id == spellid, new ActionAlwaysSucceed())),

                        new Action(ret => SpellManager.ClickRemoteLocation(onLocation(ret)))));
        }

        /// <summary>Casts a spell at the units location</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastOnUnitLocation(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.UseAoEAbilities)
                        return false;

                    if (!cond(a))
                        return false;

                    return onUnit != null && CanCast(name, onUnit(a));
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting at Location] {0} ", label)),
                new Action(a => SpellManager.Cast(name)),
                //new WaitContinue(
                //   1,
                //   ret => StyxWoW.Me.CurrentPendingCursorSpell != null &&
                //          StyxWoW.Me.CurrentPendingCursorSpell.Name == name,
                //   new ActionAlwaysSucceed()),
                //new WaitContinue(TimeSpan.FromMilliseconds(200), ret => false, new ActionAlwaysSucceed()),
                                new Action(ret => SpellManager.ClickRemoteLocation(onUnit(ret).Location))));
        }

        /// <summary>Casts Sanctuary at the units location</summary>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>Sanctuary at location.</returns>
        public static Composite CastSanctuaryAtLocation(CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                delegate(object a)
                {

                    if (!cond(a))
                        return false;

                    return onUnit(a) != null && !WoWSpell.FromId(88685).Cooldown;
                },
            new Sequence(
                new Action(a => CLU.Log(" [Casting at Location] {0} ", label)),
                Item.RunMacroText("/cast Holy Word: Sanctuary", cond, label),
                new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location))));
        }

        /// <summary>
        /// Sets a trap at the current targets location.
        /// </summary>
        /// <param name="trapName">the name of the trap to use</param>
        /// <param name="onUnit">the unit to place the trap on.</param>
        /// <param name="cond">check conditions supplied are true </param>
        /// <returns>nothing</returns>
        public static Composite HunterTrapBehavior(string trapName, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond)
        {
            return new PrioritySelector(
                new Decorator(
                    delegate(object a)
                    {
                        if (!CLUSettings.Instance.UseAoEAbilities)
                            return false;

                        if (!cond(a)) return false;

                        return onUnit != null && onUnit(a) != null && !onUnit(a).IsMoving && onUnit(a).DistanceSqr < 40 * 40
                            && SpellManager.HasSpell(trapName) && !SpellManager.Spells[trapName].Cooldown;
                    },
            new PrioritySelector(
                Buff.CastBuff("Trap Launcher", ret => true, "Trap Launcher"),
                new Decorator(
                    ret => Me.HasAura("Trap Launcher"),
                    new Sequence(
                //new Switch<string>(ctx => trapName,
                //                   new SwitchArgument<string>("Immolation Trap",
                //                           new Action(ret => SpellManager.CastSpellById(82945))),
                //                   new SwitchArgument<string>("Freezing Trap",
                //                           new Action(ret => SpellManager.CastSpellById(60192))),
                //                   new SwitchArgument<string>("Explosive Trap",
                //                           new Action(ret => SpellManager.CastSpellById(82939))),
                //                   new SwitchArgument<string>("Ice Trap",
                //                           new Action(ret => SpellManager.CastSpellById(82941))),
                //                   new SwitchArgument<string>("Snake Trap",
                //                           new Action(ret => SpellManager.CastSpellById(82948)))
                //                  ),
                //// new ActionSleep(200),
                //new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location))))

                        new Action(ret => Lua.DoString(string.Format("CastSpellByName(\"{0}\")", trapName))),
                                new WaitContinue(TimeSpan.FromMilliseconds(200), ret => false, new ActionAlwaysSucceed()),
                                new Action(ret => SpellManager.ClickRemoteLocation(onUnit(ret).Location))
                        )))));
        }

        /// <summary>Casts an area spell such as DnD or Hellfire</summary>
        /// <param name="name">name of the area spell</param>
        /// <param name="radius">radius</param>
        /// <param name="requiresTerrainClick">true if the area spell requires a terrain click</param>
        /// <param name="minAffectedTargets">the minimum affected targets in the cluster</param>
        /// <param name="minRange">minimum range</param>
        /// <param name="maxRange">maximum range</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast area spell.</returns>
        public static Composite CastAreaSpell(
            string name,
            double radius,
            bool requiresTerrainClick,
            int minAffectedTargets,
            double minRange,
            double maxRange,
            CanRunDecoratorDelegate cond,
            string label)
        {
            WoWPoint bestLocation = WoWPoint.Empty;
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.UseAoEAbilities)
                        return false;

                    if (!cond(a))
                        return false;

                    bestLocation = Unit.FindClusterTargets(
                        radius, minRange, maxRange, minAffectedTargets, Battlegrounds.IsInsideBattleground);
                    if (bestLocation == WoWPoint.Empty)
                        return false;

                    if (!CanCast(name, Me.CurrentTarget))
                        return false;

                    return true;
                },
            new Sequence(
                new Action(a => CLU.Log(" [AoE] {0} ", label)),
                new Action(a => SpellManager.Cast(name)),
                new DecoratorContinue(x => requiresTerrainClick, new Action(a => SpellManager.ClickRemoteLocation(bestLocation)))));
        }


        /// <summary>Channels an area spell such as Rain of Fire</summary>
        /// <param name="name">name of the area spell</param>
        /// <param name="radius">radius</param>
        /// <param name="requiresTerrainClick">true if the area spell requires a terrain click</param>
        /// <param name="minAffectedTargets">the minimum affected targets in the cluster</param>
        /// <param name="minRange">minimum range</param>
        /// <param name="maxRange">maximum range</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The channel area spell.</returns>
        public static Composite ChannelAreaSpell(
            string name,
            double radius,
            bool requiresTerrainClick,
            int minAffectedTargets,
            double minRange,
            double maxRange,
            CanRunDecoratorDelegate cond,
            string label)
        {
            WoWPoint bestLocation = WoWPoint.Empty;
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.UseAoEAbilities)
                        return false;

                    if (!cond(a))
                        return false;

                    if (!CanCast(name, Me))
                        return false;

                    bestLocation = Unit.FindClusterTargets(
                        radius, minRange, maxRange, minAffectedTargets, Battlegrounds.IsInsideBattleground);
                    return bestLocation != WoWPoint.Empty;
                },
            new PrioritySelector(
                // dont break it if already casting it
                new Decorator(
                    x => PlayerIsChanneling && Me.ChanneledCastingSpellId == SpellManager.Spells[name].Id,
                    new Action(a => CLU.TroubleshootLog(name))),
                // casting logic
                new Sequence(
                    new Action(a => CLU.Log(" [AoE Channel] {0} ", label)),
                    new Action(a => SpellManager.Cast(name)),
                    new DecoratorContinue(
                        x => requiresTerrainClick,
                        new Action(a => SpellManager.ClickRemoteLocation(bestLocation))))));
        }

        #endregion


        /// <summary>Stop casting, plain and simple.</summary>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The stop cast.</returns>
        public static Composite StopCast(CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       x => (Me.IsCasting || Me.ChanneledCastingSpellId > 0 || PlayerIsChanneling) && cond(x),
                       new Sequence(
                           new Action(a => CLU.Log(" [Stop Casting] {0} ", label)),
                           new Action(a => SpellManager.StopCasting())));
        }

        /// <summary>
        /// Gets the spell by name (string)
        /// </summary>
        /// <param name="spellName">the spell name to get</param>
        /// <returns> Returns the spellname</returns>
        private static WoWSpell GetSpellByName(string spellName)
        {
            WoWSpell spell;
            if (!SpellManager.Spells.TryGetValue(spellName, out spell))
                spell = SpellManager.Spells.FirstOrDefault(s => s.Key == spellName).Value;

            return spell;
        }

        /// <summary>
        /// this will localise the spell name to the local client.
        /// </summary>
        private static readonly Dictionary<string, string> LocalizedSpellNames = new Dictionary<string, string>();
        public static string LocalizeSpellName(string name)
        {
            if (LocalizedSpellNames.ContainsKey(name))
                return LocalizedSpellNames[name];

            string loc;

            int id = 0;
            try
            {
                id = SpellManager.Spells[name].Id;
            }
            catch
            {
                return name;
            }

            loc = Lua.GetReturnValues("return select(1, GetSpellInfo(" + id + "))")[0];
            LocalizedSpellNames[name] = loc;
            CLU.TroubleshootLog("Localized spell: '" + name + "' is '" + loc + "'.");
            return loc;
        }


        /// <summary>Escape Lua names using UTF8 encoding.
        /// Heres a url we can use to decode this. http://software.hixie.ch/utilities/cgi/unicode-decoder/utf8-decoder
        /// Usefull when a Lua command fails.</summary>
        /// <param name="luastring">the string to encode</param>
        /// <returns>The real lua escape.</returns>
        public static string RealLuaEscape(string luastring)
        {
            var bytes = Encoding.UTF8.GetBytes(luastring);
            return bytes.Aggregate(String.Empty, (current, b) => current + ("\\" + b));
        }


        /// <summary>
        ///  CLU's Performance timer for the BT
        /// </summary>
        /// <returns>Failure to enable it to traverse the tree.</returns>
        private static readonly Stopwatch TreePerformanceTimer = new Stopwatch(); // lets see if we can get some performance on this one.

        public static Composite TreePerformance(bool enable)
        {
            return
            new Action(ret =>
            {
                if (!enable)
                {
                    return RunStatus.Failure;
                }

                if (TreePerformanceTimer.ElapsedMilliseconds > 0)
                {
                    // NOTE: This dosnt account for Spell casts (meaning the total time is not the time to traverse the tree plus the current cast time of the spell)..this is actual time to traverse the tree.
                    CLU.TroubleshootLog("[CLU] " + CLU.Version + ": " + " [CLU TreePerformance] Elapsed Time to traverse the tree: {0} ms", TreePerformanceTimer.ElapsedMilliseconds);
                    TreePerformanceTimer.Stop();
                    TreePerformanceTimer.Reset();
                }

                TreePerformanceTimer.Start();

                return RunStatus.Failure;
            });
        }

        /// <summary>
        /// Use this to print all known spells
        /// </summary>
        public static void DumpSpells()
        {
            CLU.TroubleshootLog("Dumping List of Known Spell Information");
            foreach (var sp in SpellManager.Spells)
            {
                WoWSpell spell;
                if (SpellManager.Spells.TryGetValue(sp.Value.Name, out spell))
                {
                    CLU.TroubleshootLog("Spell ID:" + sp.Value.Id + " MaxRange:" + sp.Value.MaxRange + " MinRange:" + sp.Value.MinRange + " PowerCost:" + sp.Value.PowerCost + " HasRange:" + sp.Value.HasRange + " IsMeleeSpell:" + sp.Value.IsMeleeSpell + " IsSelfOnlySpell:" + sp.Value.IsSelfOnlySpell + " Cooldown:" + spell.Cooldown + " CooldownTimeLeft.TotalMilliseconds:" + spell.CooldownTimeLeft.TotalMilliseconds + " " + spell);
                }
                else
                {
                    CLU.TroubleshootLog(sp.Value.Name);
                }

            }
            CLU.TroubleshootLog("End Spell Information");
        }

        // ===================================== Lua ==================================================================

        /// <summary>
        /// Returns the cooldown of a rune in seconds, Rune count is backwards (eg:4,3,2,1,0 Zero is READY)
        /// </summary>
        /// first_blood = 1
        /// second_blood = 2
        /// first_Unholy = 3
        /// second_Unholy = 4
        /// first_Frost = 5
        /// second_Frost = 6
        /// <param name="rune">number of the run to check (see above)</param>
        /// <returns>The cooldown of the rune specified.</returns>
        public static double RuneCooldown(int rune)
        {
            string runename = String.Empty;
            if (rune == 1)
                runename = "Blood_1";
            else if (rune == 2)
                runename = "Blood_2";
            else if (rune == 3)
                runename = "Unholy_1";
            else if (rune == 4)
                runename = "Unholy_2";
            else if (rune == 5)
                runename = "Frost_1";
            else if (rune == 6)
                runename = "Frost_2";

            // Lets track some rune cooldowns!
            var lua = String.Format("local r_start, r_duration, r_ready = GetRuneCooldown({0}) if r_start > 0 then return math.ceil((r_start + r_duration) - GetTime()) else return 0 end", rune);
            try
            {
                var retValue = Double.Parse(Lua.GetReturnValues(lua)[0]);
                return retValue;
            }
            catch
            {
                CLU.DiagnosticLog("Lua failed in RuneCooldown: " + lua);
                return 9999;
            }
        }

        public static bool IsRuneCooldown(int rune)
        {
            //using (StyxWoW.Memory.AcquireFrame())
            //{
            string runename = String.Empty;
            if (rune == 1) runename = "Blood_1";
            else if (rune == 2) runename = "Blood_2";
            else if (rune == 3) runename = "Unholy_1";
            else if (rune == 4) runename = "Unholy_2";
            else if (rune == 5) runename = "Frost_1";
            else if (rune == 6) runename = "Frost_2";

            // Lets track some rune cooldowns!

            //var raw = Lua.GetReturnValues("if " + key.ToString("g") + "() then return 1 else return 0 end");
            var lua =
                String.Format(
                    "local r_start, r_duration, r_ready = GetRuneCooldown({0}) if r_ready then return 1 else return 0 end",
                    rune);
            try
            {
                return Lua.GetReturnValues(lua)[0] == "1";
                //bool retValue = Convert.ToBoolean(Lua.GetReturnValues(lua)[0]);
                //return retValue;
            }
            catch
            {
                CLU.DiagnosticLog("Lua failed in IsRuneCooldown: " + lua);
                return false;
            }
            //}
        }

        /// <summary>Return true of the target has a Dispelable HELPFUL buff</summary>
        /// <returns>The target has Dispelable buff.</returns>
        public static bool TargetHasDispelableBuffLua()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                // should count how many buffs the target has but meh
                for (int i = 1; i <= 40; i++)
                {
                    try
                    {
                        List<string> luaRet = Lua.GetReturnValues(String.Format("local buffName, _, _, _, debuffType = UnitAura(\"target\", {0}, \"CANCELABLE\") return debuffType,buffName", i));

                        if (luaRet != null)
                        {
                            var purgableSpell = luaRet[0] == "Magic";
                            if (purgableSpell)
                            {
                                CLU.DiagnosticLog("Buff Name: {0} is Dispelable!", luaRet[1]);
                            }

                            return purgableSpell;
                        }
                    }
                    catch
                    {
                        CLU.DiagnosticLog("Lua failed in TargetHasDispelableBuff");
                        return false;
                    }
                }

                return false;
            }
        }

        /// <summary>Return true of the target has a stealable HELPFUL buff</summary>
        /// <returns>The target has stealable buff.</returns>
        public static bool TargetHasStealableBuff()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                // should count how many buffs the target has but meh
                for (int i = 1; i <= 40; i++)
                {
                    try
                    {
                        List<string> luaRet =
                            Lua.GetReturnValues(
                                String.Format(
                                    "local buffName, _, _, _, _, _, _, _, isStealable = UnitAura(\"target\", {0}, \"HELPFUL\") return isStealable,buffName",
                                    i));

                        if (luaRet != null && luaRet[0] == "1")
                        {
                            var stealableSpell = !Buff.PlayerHasActiveBuff(luaRet[1]) && (luaRet[1] != "Arcane Brilliance" && luaRet[1] != "Dalaran Brilliance");
                            if (stealableSpell)
                            {
                                CLU.DiagnosticLog("Buff Name: {0} isStealable", luaRet[1]);
                            }

                            return stealableSpell;
                        }
                    }
                    catch
                    {
                        CLU.DiagnosticLog("Lua failed in TargetHasStealableBuff");
                        return false;
                    }
                }

                return false;
            }
        }

        // fixed this for ya storm..would never work lol, you forgot RunMacroText :P -- wulf
        public static Composite CancelMyAura(string name, CanRunDecoratorDelegate cond, string label)
        {
            name = LocalizeSpellName(name);
            var macro = string.Format("/cancelaura {0}", name);
            return new Decorator(
                delegate(object a)
                {
                    //CLU.TroubleshootLog("LocalizeSpellName: {0} .... macro: {1}", name, macro);
                    if (name.Length == 0)
                        return false;
                    if (!cond(a))
                        return false;
                    return true;
                },
            new Sequence(
                new Action(a => CLU.Log(" [CancelAura] {0}", name)),
                new Action(a => Lua.DoString("RunMacroText(\"" + RealLuaEscape(macro) + "\")"))));
        }



        //========================================= TO REMOVE OR CONSOLADATE ===================================================

        public static int CurrentDemonicFury
        {
            get { return Lua.GetReturnVal<int>("return UnitPower(\"player\", SPELL_POWER_DEMONIC_FURY)", 0); }
        }
        public static int CurrentBurningEmber
        {
            get { return Lua.GetReturnVal<int>("return UnitPower(\"player\", SPELL_POWER_BURNING_EMBERS, true)", 0); }
        }

        // TODO: Remove this.
        public static void CastSpellByName(string name)
        {
            name = LocalizeSpellName(name);
            Lua.DoString(string.Format("CastSpellByName(\"{0}\")", RealLuaEscape(name)));
        }

        // //TODO: REMOVE THIS SHIT..WAS TESTING -- WULF.
        public static TimeSpan CooldownTimeLeft
        {
            get
            {
                var Id = 53301;
                var luaTime = Lua.GetReturnVal<double>(string.Format("local x,y=GetSpellCooldown({0}); return x+y-GetTime()", Id), 0);
                if (luaTime <= 0)
                    return TimeSpan.Zero;
                return TimeSpan.FromSeconds(luaTime);
            }
        }

         public static TimeSpan ChanneledTimeLeft
        {
            get
            {
                try
                {
                    var luaTime = Lua.GetReturnVal<double>(string.Format("local spell, _, _, _, _, endTime=UnitChannelInfo(\"player\"); return endTime/1000 - GetTime()"), 0);
                    if (luaTime <= 0)
                        return TimeSpan.Zero;
                    return TimeSpan.FromSeconds(luaTime);
                }
                catch
                {
                    CLU.DiagnosticLog("Lua failed in ChanneledTimeLeft");
                    return TimeSpan.Zero;
                }
               
            }
        }


        // =======================================================================================================================


    }
}
