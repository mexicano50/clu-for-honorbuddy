#region Revision info
/*
 * $Author: clutwopointzero@gmail.com $
 * $Date$
 * $ID$
 * $Revision$
 * $URL $
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion

namespace CLU.Base
{
    using System.Collections.Generic;
    using System.Linq;

    using Styx;
    using Styx.Combat.CombatRoutine;
    using Styx.CommonBot;
    using Styx.TreeSharp;
    using Styx.WoWInternals;

    using global::CLU.Lists;

    internal static class Racials
    {
        /// <summary>
        ///  Blows your wad all over the floor
        /// </summary>
        /// <returns>Nothing but win</returns>
        public static Composite UseRacials()
        {
            return new PrioritySelector(delegate
            {
                foreach (WoWSpell r in CurrentRacials.Where(racial => Spell.CanCast(racial.Name, StyxWoW.Me) && RacialUsageSatisfied(racial)))
                {
                    CLU.Log(" [Racial Abilitie] {0} ", r.Name);
                    SpellManager.Cast(r.Name);
                }

                return RunStatus.Success;
            });
        }

        /// <summary>
        /// Returns true if the racials conditions are met
        /// </summary>
        /// <param name="racial">the racial to check for</param>
        /// <returns>true if we can use the racial</returns>
        private static bool RacialUsageSatisfied(WoWSpell racial)
        {
            if (racial != null)
            {
                switch (racial.Name)
                {
                    case "Stoneform":
                        return StyxWoW.Me.GetAllAuras().Any(a => a.Spell.Mechanic == WoWSpellMechanic.Bleeding || a.Spell.DispelType == WoWDispelType.Disease || a.Spell.DispelType == WoWDispelType.Poison);
                    case "Escape Artist":
                        return StyxWoW.Me.Rooted;
                    case "Every Man for Himself":
                        return Unit.IsCrowdControlled(StyxWoW.Me);
                    case "Shadowmeld":
                        return false;
                    case "Gift of the Naaru":
                        return StyxWoW.Me.HealthPercent <= 80;
                    case "Darkflight":
                        return false;
                    case "Blood Fury":
                        return true;
                    case "War Stomp":
                        return false;
                    case "Berserking":
                        return true;
                    case "Will of the Forsaken":
                        return Unit.IsCrowdControlled(StyxWoW.Me);
                    case "Cannibalize":
                        return false;
                    case "Arcane Torrent":
                        return StyxWoW.Me.ManaPercent < 91 && StyxWoW.Me.Class != WoWClass.DeathKnight;
                    case "Rocket Barrage":
                        return true;

                    default:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a list of current racials
        /// </summary>
        public static IEnumerable<WoWSpell> CurrentRacials
        {
            get
            {
                //lil bit hackish ... but HB is broken ... maybe -- edit by wulf.
                var listPairs = SpellManager.Spells.Where(racial => MiscLists.Racials.Contains(racial.Value.Name)).ToList();
                return listPairs.Select(s => s.Value).ToList();
            }
        }

    }
}
