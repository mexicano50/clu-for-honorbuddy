using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx;
using Styx.CommonBot;
using global::CLU.Lists;

namespace CLU.Base
{
    internal static class CrowdControl
    {
        public static Composite freeMe()
        {
            return(
                new PrioritySelector(delegate
                    {
                        foreach (WoWSpell s in freeMeSpellList.Where(spell => Spell.CanCast(spell.Name, StyxWoW.Me) && freeMeSpellUsage(spell)))
                        {
                            CLU.Log(" [Freeing you via] {0} ", s.Name);
                            SpellManager.Cast(s.Name);
                        }
                        return RunStatus.Success;
                    })
            );
        }

        private static bool freeMeSpellUsage(WoWSpell spell)
        {
            if (spell != null)
            {
                switch (spell.Name)
                {
                    case "":
                        return false;
                    default:
                        return false;
                }
            }
            return false;
        }

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
