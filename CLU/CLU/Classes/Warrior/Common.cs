#region Revision info
/*
 * $Author: clutwopointzero@gmail.com $
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 * $LastChangedBy$
 * $ChangesMade$
 */
#endregion

using System;

namespace CLU.Classes.Warrior
{
    using Styx;
    using Styx.TreeSharp;

    using global::CLU.Base;

    /// <summary>
    /// Common Warrior Functions.
    /// </summary>
    public static class Common
    {
        public static Composite HandleFlyingUnits
        {
            get
            {
                //Shoot flying targets
                return new Decorator(
                      ret => StyxWoW.Me.CurrentTarget.IsFlying || StyxWoW.Me.CurrentTarget.Distance2DSqr < 5 * 5 && Math.Abs(StyxWoW.Me.Z - StyxWoW.Me.CurrentTarget.Z) >= 5,
                      new PrioritySelector(
                          Spell.CastSpell("Heroic Throw", ret => true, "Heroic Throw"),
                          Spell.CastSpell("Throw", ret => true, "Throw")));
            }
        }
    }
}
