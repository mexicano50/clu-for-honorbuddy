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
    using Styx.WoWInternals;

    /// <summary>
    /// Common Druid Functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Used to store confirmation of previous natures swiftness cast.
        /// </summary>
        public static bool PrevNaturesSwiftness;

        /// <summary>
        /// A check how high the multiplier were when Rip/ Rake were Apllied / refreshed 
        /// </summary>
        public static double RakeMultiplier;
        public static double RipMultiplier;
        public static double ExtendedRip;

        /// <summary>
        /// UnitMana - Used in Druid Energy calculation // using (StyxWoW.Memory.AcquireFrame()) {}
        /// </summary>
        /// <returns></returns>
        public static double PlayerEnergy
         {
            get
             {
                 try
                 {
                        return Lua.GetReturnVal<int>("return UnitMana(\"player\");", 0);
                 }
                 catch
                 {
                     CLU.DiagnosticLog(" Lua Failed in PlayerEnergy");
                     return 0;
                 }
             }
         }

        /// <summary>
        /// GetPowerRegen - Used in Druid Energy calculation // using (StyxWoW.Memory.AcquireFrame()) {}
        /// </summary>
        /// <returns></returns>
        public static double EnergyRegen
         {
            get
             {
                 try
                 {
                        return Lua.GetReturnVal<float>("return GetPowerRegen()", 1);
                 }
                 catch
                 {
                     CLU.DiagnosticLog(" Lua Failed in EnergyRegen");
                     return 0;
                 }
             }
         }

        /// <summary>
        /// Calculate time to energy cap.
        /// </summary>
         public static double TimetoEnergyCap
         {
             get
             {
                 try
                 {
                        return (100 - PlayerEnergy)*(1.0/EnergyRegen);
                 }
                 catch
                 {
                     CLU.DiagnosticLog(" Calculation Failed in TimetoEnergyCap");
                     return 999999;
                 }
             }
         }

        /// <summary>
        ///  we got certain buffs (savage roar  | tf | incarnation) or dots tick higher
        /// </summary>
        public static double tick_multiplier
         {
             get
             {
                 double tick_multiplier = 1;
                 //Tigers Fury
                 if (StyxWoW.Me.HasAura(5217))
                     tick_multiplier = tick_multiplier * 1.15;
                 //Savage Roar
                 if (StyxWoW.Me.HasAura(127538))
                     tick_multiplier = tick_multiplier * 1.3;
                 //Doc
                 if (StyxWoW.Me.HasAura(108373))
                     tick_multiplier = tick_multiplier * 1.25;
                 return tick_multiplier;
             }
         }

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
