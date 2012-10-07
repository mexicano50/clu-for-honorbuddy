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

using CLU.Helpers;

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
            CLULogger.TroubleshootLog("WoWStats: Connected to the Grid");
            this.spellCasts = 0;
            this.spellList = new Dictionary<string, int>();
            this.spellInterval = new Dictionary<string, List<DateTime>>();
            this.healingStats = new Dictionary<DateTime, string>();
            this.start = DateTime.Now;
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
        /// Handle spellcast succeeded
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="raw">raw Lua Events</param>
        public void UnitSpellcastSucceeded(object sender, LuaEventArgs raw)
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
                CLULogger.DiagnosticLog("Adding " + DateTime.Now + " for " + spell);
                this.spellInterval[spell].Add(DateTime.Now);
            }

            // initialize or increment the count for this item
            try {
                this.healingStats[DateTime.Now] = CLULogger.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth);
                CLULogger.DiagnosticLog("[CLU SUCCEED] " + CLU.Version + ": " + CLULogger.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth) + ", HealthPercent: " + Math.Round(Me.CurrentTarget.HealthPercent * 10.0) / 10.0);
            } catch {
                this.healingStats[DateTime.Now] = this.healingStats.ContainsKey(DateTime.Now) ? this.healingStats[DateTime.Now] = CLULogger.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth) : "blank";
                CLULogger.DiagnosticLog("[CLU SUCCEED] " + CLU.Version + ": " + CLULogger.SafeName(Me.CurrentTarget) + ", " + spell + ", MaxHealth: " + Me.CurrentTarget.MaxHealth + ", CurrentHealth: " + Me.CurrentTarget.CurrentHealth + ", Deficit: " + (Me.CurrentTarget.MaxHealth - Me.CurrentTarget.CurrentHealth) + ", HealthPercent: " + Math.Round(Me.CurrentTarget.HealthPercent * 10.0) / 10.0);
            }
        }

        public void PrintReport()
        {
            // var spells = spellList.OrderByDescending(x => x.Value);
            // var seconds = DateTime.Now.Subtract(start).TotalSeconds;
            var minutes = DateTime.Now.Subtract(this.start).TotalMinutes;

            var apm = (int)(this.spellCasts / minutes);

            CLULogger.Log("CLU stats report:");
            CLULogger.Log("Runtime: {0} minutes", Math.Round(minutes * 10.0) / 10.0);
            CLULogger.Log("Spells cast: {0}", this.spellCasts > 1000 ? ((Math.Round(this.spellCasts / 100.0) * 10) + "k") : this.spellCasts.ToString(CultureInfo.InvariantCulture));
            CLULogger.Log("Average APM: {0}", apm);
            CLULogger.Log("------------------------------------------");
            foreach (KeyValuePair<string, int> spell in this.spellList) {
                CLULogger.Log(spell.Key + " was cast " + spell.Value + " time(s).");
            }

            CLULogger.Log("------------------------------------------");

            foreach (KeyValuePair<string, List<DateTime>> spell in this.spellInterval) {
                var lastInterval = this.start;
                var output = "0 ";

                for (int x = 0; x < spell.Value.Count - 1; ++x) {
                    var interval = spell.Value[x];
                    var difference = interval - lastInterval;
                    output = output + string.Format(", {0} ", Math.Round(difference.TotalSeconds * 100.0) / 100.0);
                    lastInterval = interval;
                }

                CLULogger.Log(spell.Key + " intervals: ");
                Logging.Write(" " + output);
            }
        }

        public void PrintHealReport()
        {
            CLULogger.Log("CLU Healing report:");
            CLULogger.Log("------------------------------------------");
            CLULogger.Log("These are the spells from UNIT_SPELLCAST_SUCCEEDED");
            CLULogger.Log("------------------------------------------");
            foreach (KeyValuePair<DateTime, string> entry in this.healingStats) {
                CLULogger.Log("[" + entry.Key + "] " + entry.Value);
            }
            CLULogger.Log("------------------------------------------");
        }
    }
}

