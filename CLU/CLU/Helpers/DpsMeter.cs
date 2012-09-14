#region Revision info
/*
 * $Author: Apoc $
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy: clutwopointzero@gmail.com $
 * $ChangesMade$
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace CLU.Helpers
{
    /// <summary>
    /// Calculates DPS, and combat time left on nearby mobs in combat.
    /// DPS is not calculated as YOUR DPS. But instead; global DPS on the mob. (You and your buddies combined DPS on a single mob)
    /// </summary>
    internal class DpsMeter
    {
        private static readonly Dictionary<ulong, DpsInfo> DpsInfos = new Dictionary<ulong, DpsInfo>();
        private static bool _initialized;

        /// <summary>
        /// Starts the DpsMeter.
        /// </summary>
        public static void Initialize()
        {
            if (!_initialized)
            {
                DpsInfos.Clear();
                _initialized = true;
            }
        }

        public static void Shutdown()
        {
            if (_initialized)
            {
                DpsInfos.Clear();
                _initialized = false;
            }
        }

        public static void Update()
        {
            List<WoWUnit> availableUnits = (from u in ObjectManager.GetObjectsOfType<WoWUnit>(true, false)
                                            where !u.IsDead && u.Attackable && u.IsMe && !u.IsFriendly && u.Combat
                                            orderby u.Distance ascending
                                            select u).ToList();

            foreach (WoWUnit unit in availableUnits)
            {
                if (!DpsInfos.ContainsKey(unit.Guid))
                {
                    var di = new DpsInfo { Unit = unit, CombatStart = DateTime.Now, StartHealth = unit.CurrentHealth };

                    DpsInfos.Add(unit.Guid, di);
                }
                else
                {
                    DpsInfo di = DpsInfos[unit.Guid];

                    di.CurrentDps = (di.StartHealth - unit.CurrentHealth) / (DateTime.Now - di.CombatStart).TotalSeconds;
                    di.CombatTimeLeft = new TimeSpan(0, 0, (int)(unit.CurrentHealth / di.CurrentDps));

                    // .NET makes a copy of the struct when we grab it out of the collection.
                    // Make sure we put the updated version back in!
                    DpsInfos[unit.Guid] = di;
                }
            }

            // Kill off any 'bad' units in our list.
            KeyValuePair<ulong, DpsInfo>[] removeUnits = DpsInfos.Where(kv => !kv.Value.Unit.IsValid).ToArray();
            for (int i = 0; i < removeUnits.Length; i++)
            {
                DpsInfos.Remove(removeUnits[i].Key);
            }
        }

        /// <summary>
        /// Returns the current DPS on a specific unit, or -1 if the unit is not currently being tracked, or doesn't exist.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static double GetDps(WoWUnit u)
        {
            if (DpsInfos.ContainsKey(u.Guid))
            {
                return DpsInfos[u.Guid].CurrentDps;
            }
            // -1 is a fail case.
            return -1;
        }

        /// <summary>
        /// Returns the estimated combat time left for this unit. (Time until death)
        /// If the unit is invalid; TimeSpan.MinValue is returned.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static TimeSpan GetCombatTimeLeft(WoWUnit u)
        {
            if (DpsInfos.ContainsKey(u.Guid))
            {
                return DpsInfos[u.Guid].CombatTimeLeft;
            }
            return TimeSpan.MinValue;
        }

        #region Nested type: DpsInfo

        private struct DpsInfo
        {
            public DateTime CombatStart;
            public TimeSpan CombatTimeLeft;
            public double CurrentDps;
            public uint StartHealth;
            public WoWUnit Unit;
        }

        #endregion
    }
}