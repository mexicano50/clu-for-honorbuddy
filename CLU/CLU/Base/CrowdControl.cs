using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLU.Helpers;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx;
using Styx.CommonBot;
using global::CLU.Lists;

namespace CLU.Base
{
    internal static class CrowdControl
    {
        /// <summary>
        /// Frees you from movement imparing effects if a usable spell exists
        /// </summary>
        /// <returns>More win then Wulf's UseRacial call</returns>
        public static Composite freeMe()
        {
            return(
                new PrioritySelector(delegate
                    {
                        foreach (WoWSpell s in freeMeSpellList.Where(spell => Spell.CanCast(spell.Name, StyxWoW.Me) && freeMeSpellUsage(spell)))
                        {
                            CLULogger.Log(" [Freeing you via] {0} ", s.Name);
                            SpellManager.Cast(s.Name);
                        }
                        return RunStatus.Success;
                    })
            );
        }

        /// <summary>
        /// Returns true if the movement imparing effects are breakable via a usable spell
        /// </summary>
        /// <param name="spell">The spell to check for</param>
        /// <returns>True if we should use the spell</returns>
        private static bool freeMeSpellUsage(WoWSpell spell)
        {
            if (spell != null)
            {
                switch (spell.Name)
                {
                    case "Berserker Rage":
                        return StyxWoW.Me.GetAllAuras().Any(a => a.Spell.Mechanic == WoWSpellMechanic.Fleeing || a.Spell.Mechanic == WoWSpellMechanic.Sapped || a.Spell.Mechanic == WoWSpellMechanic.Incapacitated);
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of spells that break movement imparing effects
        /// </summary>
        public static IEnumerable<WoWSpell> freeMeSpellList
        {
            get
            {
                var listPairs = SpellManager.Spells.Where(spell => MiscLists.spellsThatBreakCrowdControl.Contains(spell.Value.Name)).ToList();
                return listPairs.Select(s => s.Value).ToList();
            }
        }
    }
}
