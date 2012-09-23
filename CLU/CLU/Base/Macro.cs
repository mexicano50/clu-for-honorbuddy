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

using System;
using Styx.WoWInternals;
using Styx;
using Styx.CommonBot;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using CLU.Settings;
using Action = Styx.TreeSharp.Action;
using CommonBehaviors.Actions;

namespace CLU.Base
{
    internal static class Macro
    {
        /* Putting all the Macro logic here */

        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }

        /// <summary>
        /// Toggle macro for combat rotation On/Off
        /// </summary>
        public static bool Manual
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return Manual and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Toggle macro for burst rotation On/Off
        /// </summary>
        public static bool Burst
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return Burst and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Toggle macro for switching between two(2) rotations
        /// </summary>
        public static bool rotationSwap
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return rotationSwap and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Toggle macro for switching between Offensive and Defensive weapon sets
        /// </summary>
        public static bool weaponSwap
        {
            get
            {
                return Convert.ToBoolean(Lua.GetReturnVal<int>("return weaponSwap and 0 or 1", 0));
            }
        }

        /// <summary>
        /// Resets individual MultiCastMacro int's from one(1) to zero(0)
        /// </summary>
        /// <param name="Which">Spell name</param>
        public static void resetMacro(string Which)
        {
            Lua.DoString(Which + " = 0;");
        }

        /// <summary>
        /// Resets all MultiCastMacro int's within from one(1) to zero(0)
        /// </summary>
        public static void resetAllMacros()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                resetMacro("MultiCastFT");
                resetMacro("MultiCastMT");
            }
        }

        static int MultiCastMacroMT = 0;
        static int MultiCastMacroFT = 0;
        static string whatSpell;
        static WoWSpell _Spell;

        /// <summary>
        /// Main MultiCastMacro call: checks if MultiCastMacro MT or FT is active, also checks for LoS and if we can cast the specified spell
        /// If zero(0) is present: return to primary or previous rotation
        /// If one(1) is present: run MT or FT then return to primary or previous rotation
        /// </summary>
        public static void isMultiCastMacroInUse()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                MultiCastMacroMT = Lua.GetReturnVal<int>("return MultiCastMT", 0);
                MultiCastMacroFT = Lua.GetReturnVal<int>("return MultiCastFT", 0);
                whatSpell = Lua.GetReturnVal<String>("return spellName", 0);
            }
            if (MultiCastMacroMT > 0 || MultiCastMacroFT > 0)
            {
                if (whatSpell == null)
                {
                    CLU.Log("Please enter a spell!");
                    resetAllMacros();
                }
                else
                {
                    SpellManager.Spells.TryGetValue(whatSpell, out _Spell);
                    if (!_Spell.IsValid)
                    {
                        CLU.Log("Can't Cast Spell, invalid!");
                        resetAllMacros();
                    }
                    if (_Spell.CooldownTimeLeft > SpellManager.GlobalCooldownLeft)
                    {
                        CLU.Log("Can't Cast Spell, on CD!");
                        resetAllMacros();
                    }
                }
            }
            if (MultiCastMacroMT > 0)
            {
                if (SpellManager.GlobalCooldown || Me.IsCasting)
                    return;
                if (Me.CurrentTarget.InLineOfSight)
                {
                    if (SpellManager.CanCast(whatSpell))
                    {
                        Lua.DoString("RunMacroText(\"/cast " + whatSpell + "\")");
                        SpellManager.ClickRemoteLocation(Me.CurrentTarget.Location);
                        CLU.Log("Casting " + whatSpell);
                        resetMacro("MultiCastMT");
                    }
                }
                else
                {
                    resetMacro("MultiCastMT");
                }
            }
            else if (MultiCastMacroFT > 0)
            {
                if (SpellManager.GlobalCooldown || Me.IsCasting)
                    return;
                if (Me.FocusedUnit.InLineOfSight)
                {
                    if (SpellManager.CanCast(whatSpell))
                    {
                        Lua.DoString("RunMacroText(\"/cast [@focus] " + whatSpell + "\")");
                        SpellManager.ClickRemoteLocation(Me.FocusedUnit.Location);
                        CLU.Log("Casting " + whatSpell);
                        resetMacro("MultiCastFT");
                    }
                }
                else
                {
                    resetMacro("MultiCastFT");
                }
            }
        }

        /// <summary>
        /// Checks to see if MultiCast MT or FT are active
        /// </summary>
        /// <returns>Returns false if active</returns>
        public static bool canIContinue()
        {
            if (MultiCastMacroMT > 0 || MultiCastMacroFT > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets a trap at the specified targets location
        /// </summary>
        /// <param name="trapName">The name of the trap to use</param>
        /// <param name="onUnit">The unit to place the trap on</param>
        /// <param name="cond">Check conditions supplied are true</param>
        /// <returns></returns>
        public static Composite pvpTrapBehavior(string trapName, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond)
        {
            return (
                new PrioritySelector(
                    new Decorator(delegate(object a)
                        {
                            if (!cond(a))
                                return false;
                            return onUnit != null && onUnit(a) != null && onUnit(a).DistanceSqr <= 20 * 20 && SpellManager.HasSpell(trapName) && !SpellManager.Spells[trapName].Cooldown;
                        },
                new PrioritySelector(
                    Buff.CastBuff("Trap Launcher", ret => !Buff.PlayerHasBuff("Trap Launcher"), "Trap Launcher"),
                    Spell.CastSpell("Scatter Shot", ret => true, "Scatter Shot"),
                    new Decorator(ret => Buff.PlayerHasBuff("Trap Launcher"),
                        new Sequence(
                            new Action(ret => Lua.DoString(string.Format("CastSpellByName(\"{0}\")", trapName))),
                            new WaitContinue(TimeSpan.FromMilliseconds(200), ret => false, new ActionAlwaysSucceed()),
                            new Action(ret => SpellManager.ClickRemoteLocation(onUnit(ret).Location)))))
            )));
        }
    }
}
