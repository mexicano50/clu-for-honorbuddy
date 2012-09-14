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

using CLU.Settings;
using Styx;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using CLU.Base;

namespace CLU.Classes.Druid
{
    /// <summary>
    /// Common Druid Functions.
    /// </summary>
    public static class Common
    {

        /// <summary>
        /// erm..me
        /// </summary>
        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }

        public static bool IsBehind
        {
            get
            {
                return Me.CurrentTarget != null && StyxWoW.Me.IsBehind(Me.CurrentTarget);
            }
        }

        /// <summary>
        /// Handles Shapeshift form.
        /// </summary>
        public static Composite HandleShapeshiftForm
        {
            get
            {
                return new Decorator(
                         ret => CLUSettings.Instance.EnableMovement,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Bear Form", ret => Me.HealthPercent <= 20 && !Buff.PlayerHasBuff("Bear Form"), "Bear Form"),
                                   Spell.CastSelfSpell("Cat Form", ret => Me.HealthPercent > 20 && !Buff.PlayerHasBuff("Cat Form"), "Cat Form")));
            }
        }

        /// <summary>
        /// Returns true if we have Incarnation up or we are stealthed.
        /// </summary>
        public static bool CanRavage 
        { 
            get
            {
                return Buff.PlayerHasBuff("Incarnation: King of the Jungle") || Buff.PlayerHasBuff("Prowl");
            }
        }
    }
}
