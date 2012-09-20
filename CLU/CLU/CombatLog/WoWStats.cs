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
    using Styx.Common;
    using Styx.WoWInternals;

    using System.Globalization;
    using Styx;
    using Styx.WoWInternals.WoWObjects;

    public class WoWStats
    {

        private ulong spellCasts;
        private Dictionary<string, int> spellList;
        private Dictionary<string, List<DateTime>> spellInterval;
        private Dictionary<DateTime, string> healingStats;
        private DateTime start;
        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }
        
        private WoWStats()
        {

        }

		private static WoWStats instance;
        public static WoWStats Instance
        {
            get {
        		return instance ?? (instance = new WoWStats());
            }
        }

        public void ClearStats()
        {
            this.spellInterval.Clear();
            this.healingStats.Clear();
            this.spellList.Clear();
            this.spellCasts = 0;
            this.start = DateTime.Now;
        }

        /// <summary>
        /// Attatch and watch for spell cast succeed
        /// </summary>
        /// <param name="o">object</param>
        public void WoWStatsOnStarted(object o)
        {
            Lua.Events.AttachEvent("UNIT_SPELLCAST_SUCCEEDED", this.UNIT_SPELLCAST_SUCCEEDED);
            CLU.TroubleshootLog( "WoWStats: Connected to the Grid");
            this.spellCasts = 0;
            this.spellList = new Dictionary<string, int>();
            this.spellInterval = new Dictionary<string, List<DateTime>>();
            this.healingStats = new Dictionary<DateTime, string>();
            this.start = DateTime.Now;
        }

        /// <summary>
        /// Destroy
        /// </summary>
        /// <param name="o">object</param>
        public void WoWStatsOnStopped(object o)
        {
            Lua.Events.DetachEvent("UNIT_SPELLCAST_SUCCEEDED", this.UNIT_SPELLCAST_SUCCEEDED);
        }

        /// <summary>
        /// Handle spellcast succeeded
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="raw">raw Lua Events</param>
        private void UNIT_SPELLCAST_SUCCEEDED(object sender, LuaEventArgs raw)
        {
            var args = raw.Args;
            var player = Convert.ToString(args[0]);

            // Not me ... Im out!
            if (player != "player") {
                return;
            }

            // get the english spell name, not the localized one!
            var spellID = Convert.ToInt32(args[4]);
            var spell = WoWSpell.FromId(spellID).Name;
            var sourceGuid = ulong.Parse(args[3].ToString().Replace("0x", string.Empty), NumberStyles.HexNumber);

            // We need to 'sleep' for these spells. Otherwise, we'll end up double-casting them. Which will cause issues
            switch (spell) {
            case "Rejuvenation":
            case "Lifebloom":
            case "Regrowth":
            case "Nourish":
            case "Healing Touch":
            case "Remove Corruption":
            case "Holy Light":
            case "Holy Radiance":
            case "Divine Light":
            case "Holy Shock":
                CLU.DiagnosticLog("Sleeping for heal success. ({0})", spell);
                StyxWoW.SleepForLagDuration();
                break;
            case "Nature's Swiftness":
                CLU.DiagnosticLog("PrevNaturesSwiftness. ({0})", spell);
                if (sourceGuid == StyxWoW.Me.Guid)
                {
                    Classes.Druid.Common.PrevNaturesSwiftness = spellID == 132158;
                }
                break;
            }

            // increments or decrements 
            int value;
            if (spellList.TryGetValue(spell, out value))
            {
               spellList[spell] = value + 1;
            }

            this.spellCasts++;

            // Add the spell to our spell list with a timestamp
            if (!this.spellInterval.ContainsKey(spell))
                this.spellInterval[spell] = new List<DateTime>();

            if (!this.spellInterval[spell].Contains(DateTime.Now)) {
                CLU.DiagnosticLog("Adding " + DateTime.Now + " for " + spell);
                this.spellInterval[spell].Add(DateTime.Now);
            }

            // initialize or increment the count for this item
            try {
                this.healingStats[DateTime.Now] = CLU.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth);
                CLU.DiagnosticLog("[CLU SUCCEED] " + CLU.Version + ": " + CLU.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth) + ", HealthPercent: " + Math.Round(Me.CurrentTarget.HealthPercent * 10.0) / 10.0);
            } catch {
                this.healingStats[DateTime.Now] = this.healingStats.ContainsKey(DateTime.Now) ? this.healingStats[DateTime.Now] = CLU.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth) : "blank";
                CLU.DiagnosticLog("[CLU SUCCEED] " + CLU.Version + ": " + CLU.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth) + ", HealthPercent: " + Math.Round(Me.CurrentTarget.HealthPercent * 10.0) / 10.0);
            }
        }

        public void PrintReport()
        {
            // var spells = spellList.OrderByDescending(x => x.Value);
            // var seconds = DateTime.Now.Subtract(start).TotalSeconds;
            var minutes = DateTime.Now.Subtract(this.start).TotalMinutes;

            var apm = (int)(this.spellCasts / minutes);

            CLU.Log("CLU stats report:");
            CLU.Log("Runtime: {0} minutes", Math.Round(minutes * 10.0) / 10.0);
            CLU.Log("Spells cast: {0}", this.spellCasts > 1000 ? ((Math.Round(this.spellCasts / 100.0) * 10) + "k") : this.spellCasts.ToString(CultureInfo.InvariantCulture));
            CLU.Log("Average APM: {0}", apm);
            CLU.Log("------------------------------------------");
            foreach (KeyValuePair<string, int> spell in this.spellList) {
                CLU.Log(spell.Key + " was cast " + spell.Value + " time(s).");
            }

            CLU.Log("------------------------------------------");

            foreach (KeyValuePair<string, List<DateTime>> spell in this.spellInterval) {
                var lastInterval = this.start;
                var output = "0 ";

                for (int x = 0; x < spell.Value.Count - 1; ++x) {
                    var interval = spell.Value[x];
                    var difference = interval - lastInterval;
                    output = output + string.Format(", {0} ", Math.Round(difference.TotalSeconds * 100.0) / 100.0);
                    lastInterval = interval;
                }

                CLU.Log(spell.Key + " intervals: ");
                Logging.Write(" " + output);
            }
        }

        public void PrintHealReport()
        {
            CLU.Log("CLU Healing report:");
            CLU.Log("------------------------------------------");
            CLU.Log("These are the spells from UNIT_SPELLCAST_SUCCEEDED");
            CLU.Log("------------------------------------------");
            foreach (KeyValuePair<DateTime, string> entry in this.healingStats) {
                CLU.Log("[" + entry.Key + "] " + entry.Value);
            }
            CLU.Log("------------------------------------------");
        }
    }
}

