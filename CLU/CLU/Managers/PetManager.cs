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

#endregion Revision info

using CLU.Helpers;

namespace CLU.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommonBehaviors.Actions;
    using global::CLU.Base;
    using global::CLU.Settings;
    using Styx;
    using Styx.Common.Helpers;
    using Styx.CommonBot;
    using Styx.TreeSharp;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using Action = Styx.TreeSharp.Action;

    internal class PetManager
    {
        /* putting all the Pets Spell/Buff/Debuff logic here */

        private static readonly WaitTimer CallPetTimer = WaitTimer.OneSecond;

        private static readonly List<WoWPetSpell> PetSpells = new List<WoWPetSpell>();

        private static readonly PetManager PetsInstance = new PetManager();

        private static ulong petGuid;

        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }

        public static readonly WaitTimer PetTimer = new WaitTimer(TimeSpan.FromSeconds(2));

        /// <summary>
        /// An instance of the Pets Class
        /// </summary>
        public static PetManager Instance
        {
            get
            {
                return PetsInstance;
            }
        }

        internal static void Pulse()
        {
            if (!StyxWoW.Me.GotAlivePet)
            {
                PetSpells.Clear();
                return;
            }

            if (StyxWoW.Me.Pet != null && petGuid != StyxWoW.Me.Pet.Guid)
            {
                petGuid = StyxWoW.Me.Pet.Guid;
                PetSpells.Clear();
                PetSpells.AddRange(StyxWoW.Me.PetSpells);
            }
        }

        /*--------------------------------------------------------
         * Spells
         *--------------------------------------------------------
         */

        /// <summary>Returns the Pet spell cooldown using Timespan (00:00:00.0000000)
        /// gtfo if the Pet dosn't have the spell.</summary>
        /// <param name="name">the name of the spell to check for</param>
        /// <returns>The spell cooldown.</returns>
        public static TimeSpan PetSpellCooldown(string name)
        {
            WoWPetSpell petAction = PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (petAction == null || petAction.Spell == null)
            {
                return TimeSpan.Zero;
            }

            CLULogger.DiagnosticLog(" [PetSpellCooldown] {0} : {1}", name, petAction.Spell.CooldownTimeLeft);
            return petAction.Spell.CooldownTimeLeft;
        }

        /// <summary>Returns a summoned pet</summary>
        /// <param name="name">the name of the spell</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast pet summon spell.</returns>
        public static Composite CastPetSummonSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            try
            {
                var isWarlock = Me.Class == WoWClass.Warlock && SpellManager.Spells[name].Name.StartsWith("Summon ");
                if (isWarlock)
                {
                    Spell.KnownChanneledSpells.Add(name);
                }

                return new Decorator(
                    delegate(object a)
                    {
                        if (!cond(a))
                            return false;

                        if (!Spell.CanCast(name, Me))
                            return false;

                        return true;
                    },
                new Decorator(ret => !Me.GotAlivePet && PetTimer.IsFinished,
                               new Sequence(
                                   new Action(a => CLULogger.Log(" {0}", label)),
                                   new Action(a => SpellManager.Cast(name)),
                                   Spell.CreateWaitForLagDuration(),
                                   new Wait(5, a => Me.GotAlivePet || !Me.IsCasting,
                                            new PrioritySelector(
                                                new Decorator(a => StyxWoW.Me.IsCasting, new Action(ret => SpellManager.StopCasting())),
                                                new ActionAlwaysSucceed())))));
            }
            catch
            {
                CLULogger.DiagnosticLog("Summon Spell {0} not supported for your Class / Specc ({1}/{2})", name, Me.Class, Me.Specialization);
                return null;
            }
        }

        /// <summary>Returns a summoned pet</summary>
        /// <param name="nameID">the id of the spell</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast pet summon spell.</returns>
        public static Composite CastPetSummonSpell(int nameID, CanRunDecoratorDelegate cond, string label)
        {
            WoWSpell summonPet = WoWSpell.FromId(nameID);
            if (summonPet.CastTime > 0)
            {
                Spell.KnownChanneledSpells.Add(summonPet.Name);
            }

            return new Decorator(
                delegate(object a)
                {
                    if (!cond(a))
                        return false;

                    if (!Spell.CanCast(summonPet.ToString()))
                        return false;

                    return true;
                },
            new Decorator(ret => !Me.GotAlivePet && PetTimer.IsFinished,
                           new Sequence(
                               new Action(a => CLULogger.Log(" {0}", label)),
                               new Action(a => SpellManager.Cast(summonPet)),
                               Spell.CreateWaitForLagDuration(),
                               new Wait(5, a => Me.GotAlivePet || !Me.IsCasting,
                                        new PrioritySelector(
                                            new Decorator(a => StyxWoW.Me.IsCasting, new Action(ret => SpellManager.StopCasting())),
                                            new ActionAlwaysSucceed())))));
        }

        /// <summary>
        ///   Calls a pet by name, if applicable.
        /// </summary>
        /// <remarks>
        ///   Created 2/7/2011.
        /// </remarks>
        /// <param name = "petName">Name of the pet. This parameter is ignored for mages. Warlocks should pass only the name of the pet. Hunters should pass which pet (1, 2, etc)</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public static bool CallPet(string petName)
        {
            if (!CallPetTimer.IsFinished)
            {
                return false;
            }

            switch (StyxWoW.Me.Class)
            {
                case WoWClass.Warlock:
                    if (Spell.CanCast("Summon " + petName))
                    {
                        CLULogger.DiagnosticLog(string.Format("[Pet] Calling out my {0}", petName));
                        bool result = SpellManager.Cast("Summon " + petName);

                        //if (result)
                        //    StyxWoW.SleepForLagDuration();
                        return result;
                    }
                    break;

                case WoWClass.Mage:
                    if (Spell.CanCast("Summon Water Elemental"))
                    {
                        CLULogger.DiagnosticLog("[Pet] Calling out Water Elemental");
                        bool result = SpellManager.Cast("Summon Water Elemental");

                        //if (result)   - All calls to this method are now placed in a sequence that uses WaitContinue
                        //    StyxWoW.SleepForLagDuration();
                        return result;
                    }
                    break;

                case WoWClass.Hunter:
                    if (Spell.CanCast("Call Pet " + petName))
                    {
                        if (!StyxWoW.Me.GotAlivePet)
                        {
                            CLULogger.DiagnosticLog(string.Format("[Pet] Calling out pet #{0}", petName));
                            bool result = SpellManager.Cast("Call Pet " + petName);

                            //if (result)
                            //    StyxWoW.SleepForLagDuration();
                            return result;
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>Cast's a pet spell</summary>
        /// <param name="name">the pet spell to cast</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast pet spell.</returns>
        public static Composite CastPetSpell(string name, CanRunDecoratorDelegate cond, string label)
        {
            return new Decorator(
                       a => cond(a) && CanCastPetSpell(name),
                       new Sequence(
                           new Action(a => CLULogger.Log(" [Pet Spell] {0} ", label)), new Action(a => CastMyPetSpell(name))));
        }

        /// <summary>Returns true if the pet spell can be cast</summary>
        /// <param name="name">the name of the spell to check</param>
        /// <returns>The can cast pet spell.</returns>
        public static bool CanCastPetSpell(string name)
        {
            WoWPetSpell petAction = Me.PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (petAction == null || petAction.Spell == null)
            {
                return false;
            }
            return !petAction.Spell.Cooldown;
        }

        /// <summary>
        /// Check to see if our pet has a spell
        /// </summary>
        /// <param name="name">the spell name in english to check for</param>
        /// <returns>true if the pet has the spell</returns>
        public static bool HasSpellPet(string name)
        {
            WoWPetSpell petAction = Me.PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (petAction == null || petAction.Spell == null)
            {
                CLULogger.TroubleshootLog(String.Format("[PetManager] Pet does not have the spell {0}", name));
                return false;
            }
            return true;
        }

        /// <summary>
        /// the main cast pet spell method (uses Lua)
        /// </summary>
        /// <param name="name">the name of the spell to cast</param>
        private static void CastMyPetSpell(string name)
        {
            var spell = Me.PetSpells.FirstOrDefault(p => p.ToString() == name);
            if (spell == null)
                return;
            try
            {
                Lua.DoString("CastPetAction({0})", spell.ActionBarIndex + 1);
            }
            catch
            {
                CLULogger.DiagnosticLog("Lua failed in CastMyPetSpell");
            }
        }

        /// <summary>Casts a Pet spell at the units location</summary>
        /// <param name="name">the name of the spell to cast</param>
        /// <param name="onUnit">The on Unit.</param>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>The cast spell at location.</returns>
        public static Composite CastPetSpellAtLocation(string name, CLU.UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            // CLU.DebugLog(" [CastSpellAtLocation] name = {0} location = {1} and can we cast it? {2}", name, SysLog.SafeName(unit), CanCast(name));
            return new Decorator(
                delegate(object a)
                {
                    if (!CLUSettings.Instance.UseAoEAbilities)
                        return false;

                    if (Me.CurrentTarget == null)
                        return false;

                    WoWPoint currTargetLocation = Me.CurrentTarget.Location;
                    if (Unit.NearbyControlledUnits(currTargetLocation, 20, false).Any())
                        return false;

                    if (!cond(a))
                        return false;

                    if (!CanCastPetSpell(name))
                        return false;

                    return onUnit != null;
                },
            new Sequence(
                new Action(a => CLULogger.Log(" [Pet Casting at location] {0} ", label)),
                new Action(a => CastMyPetSpell(name)),

                // new WaitContinue(
                //    0,
                //    ret => StyxWoW.Me.CurrentPendingCursorSpell != null &&
                //           StyxWoW.Me.CurrentPendingCursorSpell.Name == name,
                //    new ActionAlwaysSucceed()),
                new Action(a => SpellManager.ClickRemoteLocation(onUnit(a).Location))));
        }

        /*--------------------------------------------------------
         * Buffs
         *--------------------------------------------------------
         */

        /// <summary>Returns true if the Pet has the buff</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The pet has buff.</returns>
        public static bool PetHasBuff(string name)
        {
            // Me.Pet.ActiveAuras.ContainsKey(name);
            return Me.GotAlivePet && Me.Pet.HasAura(name);
        }

        /// <summary>Returns true if the Pet has the buff</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The pet has buff.</returns>
        public static bool PetHasActiveBuff(string name)
        {
            return Me.GotAlivePet && Me.Pet.ActiveAuras.ContainsKey(name);
        }

        /// <summary>Returns the buff count on the Pet</summary>
        /// <param name="name">the name of the buff to check</param>
        /// <returns>The pet count buff.</returns>
        public static uint PetCountBuff(string name)
        {
            return !Me.GotAlivePet ? 0 : Buff.GetAuraStack(Me.Pet, name, false);
        }

        /// <summary>Returns the buff time left on the Pet</summary>
        /// <param name="name">the name of the buff to check for</param>
        /// <returns>The pet buff time left.</returns>
        public static TimeSpan PetBuffTimeLeft(string name)
        {
            return !Me.GotAlivePet ? TimeSpan.Zero : Buff.GetAuraTimeLeft(Me.Pet, name, false);
        }

        /// <summary>Returns true if the Pet has the buff</summary>
        /// <param name="id">The id.</param>
        /// <returns>The pet has buff.</returns>
        public static bool PetAuraByIdActive(int id)
        {
            // Me.Pet.ActiveAuras.ContainsKey(name);
            return Me.Pet != null && StyxWoW.Me.Pet.GetAuraById(id).IsActive; // !Me.Pet.Stunned ||  PetAuraByIdActive(32752) //Summoning Disorientation
        }
    }
}