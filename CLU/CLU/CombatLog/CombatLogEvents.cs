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

namespace CLU.CombatLog
{
    using System;
    using System.Collections.Generic;
    using Styx;
    using Styx.CommonBot;
    using Styx.CommonBot.POI;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using Base;
    using Helpers;
    using Managers;
    using Settings;

    public class CombatLogEvents
    {

		private static CombatLogEvents instance;
        public static CombatLogEvents Instance
        {
            get {
        		return instance ?? (instance = new CombatLogEvents());
            }
        }

       
        public static readonly Dictionary<string, DateTime> Locks = new Dictionary<string, DateTime>();

        public static readonly double ClientLag = CLUSettings.Instance.EnableClientLagDetection ? StyxWoW.WoWClient.Latency * 2 / 1000.0 : 1;

        public void CombatLogEventsOnStarted(object o)
        {
            try
            {
                CLU.TroubleshootLog("CombatLogEvents: Connected to the Grid");
                // means spell was cast (did not hit target yet)
                CLU.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_SUCCEEDED");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_SUCCEEDED", this.OnSpellFired_ACK);
                // user got stunned, silenced, kicked...
                CLU.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_INTERRUPTED");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_INTERRUPTED", this.OnSpellFired_NACK);
                // misc fails, due to stopcast, spell spam, etc.
                CLU.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_FAILED");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_FAILED", this.OnSpellFired_FAIL);
                CLU.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_FAILED_QUIET");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_FAILED_QUIET", this.OnSpellFired_FAIL);
                CLU.TroubleshootLog("CombatLogEvents: Connect UNIT_SPELLCAST_STOP");
                Lua.Events.AttachEvent("UNIT_SPELLCAST_STOP", this.OnSpellFired_FAIL);
                // Handle Spell Missed events
                CLU.TroubleshootLog("CombatLogEvents: Connect SPELL_MISSED");
                Lua.Events.AttachEvent("SPELL_MISSED", this.HandleSpellMissed);
                CLU.TroubleshootLog("CombatLogEvents: Connect RANGE_MISSED");
                Lua.Events.AttachEvent("RANGE_MISSED", this.HandleSpellMissed);
                CLU.TroubleshootLog("CombatLogEvents: Connect SWING_MISSED");
                Lua.Events.AttachEvent("SWING_MISSED", this.HandleSpellMissed);
                CLU.TroubleshootLog("CombatLogEvents: Connect PARTY_MEMBERS_CHANGED");
                Lua.Events.AttachEvent("PARTY_MEMBERS_CHANGED", this.HandlePartyMembersChanged);
            }
            catch (Exception ex)
            { 
                CLU.DiagnosticLog("HandlePartyMembersChanged : {0}", ex);
            }
        }

        public void CombatLogEventsOnStopped(object o)
        {
            // Lua.Events.DetachEvent("CHARACTER_POINTS_CHANGED", UpdateActiveRotation);
            // Lua.Events.DetachEvent("ACTIVE_TALENT_GROUP_CHANGED", UpdateActiveRotation)

            Lua.Events.DetachEvent("UNIT_SPELLCAST_SUCCEEDED", this.OnSpellFired_ACK);

            Lua.Events.DetachEvent("UNIT_SPELLCAST_INTERRUPTED", this.OnSpellFired_NACK);

            Lua.Events.DetachEvent("UNIT_SPELLCAST_FAILED", this.OnSpellFired_FAIL);
            Lua.Events.DetachEvent("UNIT_SPELLCAST_FAILED_QUIET", this.OnSpellFired_FAIL);
            Lua.Events.DetachEvent("UNIT_SPELLCAST_STOP", this.OnSpellFired_FAIL);

            // Handle Spell Missed events
            Lua.Events.DetachEvent("SPELL_MISSED", this.HandleSpellMissed);
            Lua.Events.DetachEvent("RANGE_MISSED", this.HandleSpellMissed);
            Lua.Events.DetachEvent("SWING_MISSED", this.HandleSpellMissed);
            Lua.Events.DetachEvent("PARTY_MEMBERS_CHANGED", this.HandlePartyMembersChanged);
        }

        private void UpdateActiveRotation(object sender, LuaEventArgs args)
        {
            // CLU.rotationBase = null;
        }

        private void HandlePartyMembersChanged(object sender, LuaEventArgs args)
        {
            try {
                if (CLU.IsHealerRotationActive && StyxWoW.IsInGame) {
                    CLU.TroubleshootLog( "CombatLogEvents: Party Members Changed - Re-Initialize list Of HealableUnits");
                    switch (CLUSettings.Instance.SelectedHealingAquisition) {
                    case HealingAquisitionMethod.Proximity:
                        HealableUnit.HealableUnitsByProximity();
                        break;
                    case HealingAquisitionMethod.RaidParty:
                        HealableUnit.HealableUnitsByPartyorRaid();
                        break;
                    }
                }
            } catch (Exception ex) {
                CLU.DiagnosticLog("HandlePartyMembersChanged : {0}", ex);
            }

        }

        public void Player_OnMapChanged(BotEvents.Player.MapChangedEventArgs args)
        {
            try {

                //Why would we create same behaviors all over ?
                if (CLU.LastLocationContext == CLU.LocationContext)
                {
                    return;
                }

                CLU.TroubleshootLog("Context changed. New context: " + CLU.LocationContext + ". Rebuilding behaviors.");
                CLU.Instance.CreateBehaviors();

                if (CLU.IsHealerRotationActive && StyxWoW.IsInGame) {
                    CLU.TroubleshootLog( "CombatLogEvents: Party Members Changed - Re-Initialize list Of HealableUnits");
                    switch (CLUSettings.Instance.SelectedHealingAquisition) {
                    case HealingAquisitionMethod.Proximity:
                        HealableUnit.HealableUnitsByProximity();
                        break;
                    case HealingAquisitionMethod.RaidParty:
                        HealableUnit.HealableUnitsByPartyorRaid();
                        break;
                    }
                }
            } catch (Exception ex) {
                CLU.DiagnosticLog("Player_OnMapChanged : {0}", ex);
            }
        }




        private void OnSpellFired_ACK(object sender, LuaEventArgs raw)
        {
            this.OnSpellFired(true, true, raw);
        }

        private void OnSpellFired_NACK(object sender, LuaEventArgs raw)
        {
            this.OnSpellFired(false, true, raw);
        }

        private void OnSpellFired_FAIL(object sender, LuaEventArgs raw)
        {
            this.OnSpellFired(false, false, raw);
        }

        private void OnSpellFired (bool success, bool spellCast, LuaEventArgs raw)
        {
            var args = raw.Args;
            var player = Convert.ToString(args[0]);

            if (player != "player") {
                return;
            }

            // get the english spell name, not the localized one!
            var spellID = Convert.ToInt32(args[4]);
            var spellName = WoWSpell.FromId(spellID).Name;

            if (!success && spellCast) {
                CLU.DiagnosticLog("Woops, '{0}' cast failed: {1}", spellName, raw.EventName);
            }

            // if the spell is locked, let's extend it (spell travel time + client lag) / or reset it...
            if (Locks.ContainsKey(spellName))
            {
                if (success)
                {
                    // yay!
                    Locks[spellName] = DateTime.Now.AddSeconds(ClientLag + 4.0);
                }
                else
                {
                    if (spellCast)
                    {
                        // interrupted while casting
                        Locks[spellName] = DateTime.Now;
                    }
                    else
                    {
                        // failed to cast it. moar spam!
                        Locks[spellName] = DateTime.Now;
                    }
                }
            }
        }

        // Thanks to Singular Devs for the CombatLogEventArgs class and SpellImmunityManager.
        private void HandleSpellMissed(object sender, LuaEventArgs args)
        {
            var e = new CombatLogEventArgs(args.EventName, args.FireTimeStamp, args.Args);
            var missType = Convert.ToString(e.Args[14]);

            switch (missType) {
            case "EVADE":
                CLU.TroubleshootLog( "Mob is evading. Blacklisting it!");
                Blacklist.Add(e.DestGuid, TimeSpan.FromMinutes(30));
                if (StyxWoW.Me.CurrentTargetGuid == e.DestGuid) {
                    StyxWoW.Me.ClearTarget();
                }

                BotPoi.Clear("Blacklisting evading mob");
                StyxWoW.SleepForLagDuration();
                break;
            case "IMMUNE":
                WoWUnit unit = e.DestUnit;
                if (unit != null && !unit.IsPlayer) {
                    CLU.TroubleshootLog( "{0} is immune to {1} spell school", unit.Name, e.SpellSchool);
                    SpellImmunityManager.Add(unit.Entry, e.SpellSchool);
                }
                break;
            case "SPELL_AURA_REFRESH":
            if (e.SourceGuid == StyxWoW.Me.Guid)
                {
                    if (e.SpellId == 1822)
                    {
                        Classes.Druid.Common.RakeMultiplier = 1;
                        //TF
                        if (StyxWoW.Me.HasAura(5217))
                            Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.15;
                        //Savage Roar
                        if (StyxWoW.Me.HasAura(127538))
                            Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.3;
                        //Doc
                        if (StyxWoW.Me.HasAura(108373))
                            Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.25;
                    }
                    if (e.SpellId == 1079)
                    {
                        Classes.Druid.Common.ExtendedRip = 0;
                        Classes.Druid.Common.RipMultiplier = 1;
                        //TF
                        if (StyxWoW.Me.HasAura(5217))
                            Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.15;
                        //Savage Roar
                        if (StyxWoW.Me.HasAura(127538))
                            Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.3;
                        //Doc
                        if (StyxWoW.Me.HasAura(108373))
                            Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.25;
                    }
                }
                break;
            case "SPELL_AURA_APPLIED":
                if (e.SourceGuid == StyxWoW.Me.Guid)
                {
                    if (e.SpellId == 1822)
                    {
                        Classes.Druid.Common.RakeMultiplier = 1;
                        //TF
                        if (StyxWoW.Me.HasAura(5217))
                            Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.15;
                        //Savage Roar
                        if (StyxWoW.Me.HasAura(127538))
                            Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.3;
                        //Doc
                        if (StyxWoW.Me.HasAura(108373))
                            Classes.Druid.Common.RakeMultiplier = Classes.Druid.Common.RakeMultiplier * 1.25;
                    }
                    if (e.SpellId == 1079)
                    {
                        Classes.Druid.Common.ExtendedRip = 0;
                        Classes.Druid.Common.RipMultiplier = 1;
                        //TF
                        if (StyxWoW.Me.HasAura(5217))
                            Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.15;
                        //Savage Roar
                        if (StyxWoW.Me.HasAura(127538))
                            Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.3;
                        //Doc
                        if (StyxWoW.Me.HasAura(108373))
                            Classes.Druid.Common.RipMultiplier = Classes.Druid.Common.RipMultiplier * 1.25;
                    }
                }
                break;
            case "SPELL_AURA_REMOVED":
                if (e.SourceGuid == StyxWoW.Me.Guid)
                {
                    if (e.SpellId == 1822)
                    {
                        Classes.Druid.Common.RakeMultiplier = 0;
                    }
                    if (e.SpellId == 1079)
                    {
                        Classes.Druid.Common.ExtendedRip = 0;
                        Classes.Druid.Common.RipMultiplier = 0;
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Dumps the spells that are locked to the spelllockwatcher GUI
        /// </summary>
        /// <returns> a list of spells within the spelllock dictionary</returns>
        public Dictionary<string, double> DumpSpellLocks()
        {
            var ret = new Dictionary<string, double>();
            var now = DateTime.Now;

            foreach (var x in Locks) {
                var s = x.Value.Subtract(now).TotalSeconds;
                if (s < 0) s = 0;
                s = Math.Round(s, 3);
                ret[x.Key] = s;
            }

            return ret;
        }
    }
}
