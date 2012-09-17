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
                resetMacro("TrapMT");
                resetMacro("TrapFT");
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

        static int TrapMT = 0;
        static int TrapFT = 0;
        static string whatTrap;
        static WoWSpell _Trap;
        static bool shouldUseScatter = true;//~> Replace with GUI option
        static bool _usedScatter = false;
        
        /// <summary>
        /// Main Trap call: checks if Trap MT or FT is active, also checks for LoS and if we can cast the specified spell
        /// If zero(0) is present: return to primary or previous rotation
        /// If one(1) is present: run MT or FT then return to primary or previous rotation
        /// </summary>
        public static void isTrapMacroInUse()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                TrapMT = Lua.GetReturnVal<int>("return TrapMT", 0);
                TrapFT = Lua.GetReturnVal<int>("return TrapFT", 0);
                whatTrap = Lua.GetReturnVal<String>("return TrapType", 0);
            }
            if (TrapMT > 0 || TrapFT > 0)
            {
                if (whatTrap == null)
                {
                    CLU.Log("Invalid Trap");
                    resetAllMacros();
                }
                else
                {
                    SpellManager.Spells.TryGetValue(whatTrap, out _Trap);
                    if (!_Trap.IsValid || _Trap.CooldownTimeLeft > SpellManager.GlobalCooldownLeft)
                    {
                        CLU.Log("Can't cast trap");
                        resetAllMacros();
                    }
                    else if (Me.CurrentFocus <= 20 && !Buff.PlayerHasActiveBuff("Trap Launcher"))
                    {
                        CLU.Log("Can't cast trap launcher");
                        resetAllMacros();
                    }
                }
            }
            if (TrapMT > 0)
            {
                if (SpellManager.GlobalCooldown || Me.IsCasting)
                    return;
                if (Me.CurrentTarget.InLineOfSight)
                {
                    if (_usedScatter == false && shouldUseScatter == true && SpellManager.CanCast("Scatter Shot"))//cancast on current target
                    {
                        _usedScatter = true;
                        Spell.CastSpell("Scatter Shot", ret => Me.CurrentTarget, ret => true, "Scatter Shot");
                        return;
                    }
                    if (SpellManager.CanCast("Trap Launcher"))
                    {
                        Buff.CastBuff("Trap Launcher", ret => true, "Trap Launcher");
                        return;
                    }
                    if (SpellManager.CanCast(whatTrap) && Buff.PlayerHasBuff("Trap Launcher"))
                    {
                        Lua.DoString("RunMacroText(\"/cast " + whatTrap + "\")");
                        SpellManager.ClickRemoteLocation(Me.CurrentTarget.Location);
                        resetMacro("TrapMT");
                        _usedScatter = false;
                    }
                }
                else
                {
                    resetMacro("TrapMT");
                    _usedScatter = false;
                }
            }
            else if (TrapFT > 0)
            {
                if (SpellManager.GlobalCooldown || Me.IsCasting)
                    return;
                if (Me.FocusedUnit.InLineOfSight)
                {
                    if (_usedScatter == false && shouldUseScatter == true && SpellManager.CanCast("Scatter Shot"))//cancast on focused target
                    {
                        _usedScatter = true;
                        Spell.CastSpell("Scatter Shot", ret => Me.FocusedUnit, ret => true, "Scatter Shot");
                        return;
                    }
                    if (SpellManager.CanCast("Trap Launcher"))
                    {
                        Buff.CastBuff("Trap Launcher", ret => true, "Trap Launcher");
                        return;
                    }
                    if (SpellManager.CanCast(whatTrap) && Buff.PlayerHasBuff("Trap Launcher"))
                    {
                        Lua.DoString("RunMacroText(\"/cast [@focus] " + whatTrap + "\")");
                        SpellManager.ClickRemoteLocation(Me.FocusedUnit.Location);
                        resetMacro("TrapFT");
                        _usedScatter = false;
                    }
                }
                else
                {
                    resetMacro("TrapFT");
                    _usedScatter = false;
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
    }
}
