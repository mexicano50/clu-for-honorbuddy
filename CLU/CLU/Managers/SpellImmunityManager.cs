// Credit to Singular Devs for this class.

namespace CLU.Managers
{
    using System.Collections.Generic;
    //using Styx.Logic.Combat;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;

    static class SpellImmunityManager
    {
        // This dictionary uses Unit.Entry as key and WoWSpellSchool as value.
        static readonly Dictionary<uint, WoWSpellSchool> ImmuneNpcs = new Dictionary<uint, WoWSpellSchool>();

        public static void Add(uint mobId, WoWSpellSchool school)
        {
            if (!ImmuneNpcs.ContainsKey(mobId)) {
                ImmuneNpcs.Add(mobId, school);
            }
        }

        public static bool IsImmune(this WoWUnit unit, WoWSpellSchool school)
        {
            return unit != null && ImmuneNpcs.ContainsKey(unit.Entry) && (ImmuneNpcs[unit.Entry] & school) > 0;
        }
    }
}
