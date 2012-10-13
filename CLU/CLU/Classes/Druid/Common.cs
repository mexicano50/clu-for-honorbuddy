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

using System.Collections.Generic;
using System.Linq;
using CLU.Helpers;
using CLU.Managers;
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
                     CLULogger.DiagnosticLog(" Lua Failed in PlayerEnergy");
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
                     CLULogger.DiagnosticLog(" Lua Failed in EnergyRegen");
                     return 0;
                 }
             }
         }
        

        public static int Units()
        {
            _nearbyUnfriendlyUnits = UnfriendlyUnitsNearTarget(10f);
            return _nearbyUnfriendlyUnits.Count();
        }

        /*Get targets around us*/
        public static IEnumerable<WoWUnit> _nearbyUnfriendlyUnits;
        public static IEnumerable<WoWUnit> UnfriendlyUnitsNearTarget(float distance)
        {
            var dist = distance * distance;
            var curTarLocation = StyxWoW.Me.CurrentTarget.Location;
            return ObjectManager.GetObjectsOfType<WoWUnit>(false, false).Where(
                        p => ValidUnit(p) && p.Location.DistanceSqr(curTarLocation) <= dist).ToList();
        }

        static bool ValidUnit(WoWUnit p)
        {
            // Ignore shit we can't select/attack
            if (!p.CanSelect || !p.Attackable)
                return false;

            // Ignore friendlies!
            if (p.IsFriendly)
                return false;

            // Duh
            if (p.IsDead)
                return false;

            // Dummies/bosses are valid by default. Period.
            if (Unit.UseCooldowns())
                return true;

            // If its a pet, lets ignore it please.
            if (p.IsPet || p.OwnedByRoot != null)
                return false;

            // And ignore critters/non-combat pets
            if (p.IsNonCombatPet || p.IsCritter)
                return false;

            return true;
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
                     CLULogger.DiagnosticLog(" Calculation Failed in TimetoEnergyCap");
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
                                   Spell.CastSelfSpell("Bear Form", ret => TalentManager.CurrentSpec == WoWSpec.DruidGuardian && !Buff.PlayerHasBuff("Bear Form"), "Bear Form"),
                                   Spell.CastSelfSpell("Cat Form", ret => TalentManager.CurrentSpec == WoWSpec.DruidFeral && !Buff.PlayerHasBuff("Cat Form") && !Buff.PlayerHasBuff("Might of Ursoc"), "Cat Form")));
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

        public static string lastEclipse;

        /// <summary>
        /// Checks which Eclipse we have if any, and returns which direction we are going
        /// </summary>
        /// <returns>A numeric value for Right/Left or Centered && our last gained Eclipse</returns>
        public static int goingDir()
        {
            //We have the Lunar buff and are casting at Starfire increments(Me.CurrentEclipse == -100 || Me.CurrentEclipse == -80 || Me.CurrentEclipse == -60 || Me.CurrentEclipse == -40 || Me.CurrentEclipse == -20))
            if (Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse >= -100 && Me.CurrentEclipse <= -1)
            {
                lastEclipse = "Lunar";
                return 1;
            }
            //We have no buff but are still casting at Starfire increments(Me.CurrentEclipse == 0 || Me.CurrentEclipse == +40 || Me.CurrentEclipse == +80))
            if (!Buff.PlayerHasBuff("Eclipse (Lunar)") && !Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse >= 0 && lastEclipse == "Lunar")//and lastEclipse == "Lunar"
            {
                return 1;
            }
            //We have the solar buff and are casting at Wrath increments(Me.CurrentEclipse == +100 || Me.CurrentEclipse == +85 || Me.CurrentEclipse == +70 || Me.CurrentEclipse == +55 || Me.CurrentEclipse == +40 || Me.CurrentEclipse == +25 || Me.CurrentEclipse == +10))
            if (Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse <= 100 && Me.CurrentEclipse >= 1)
            {
                lastEclipse = "Solar";
                return -1;
            }
            //We have no buff and are still casting at Wrath increments(Me.CurrentEclipse == -5 || Me.CurrentEclipse == -35 || Me.CurrentEclipse == -65 || Me.CurrentEclipse == -95))
            if (!Buff.PlayerHasBuff("Eclipse (Solar)") && !Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse <= 0 && lastEclipse == "Solar")//and lastEclipse == "Solar"
            {
                return -1;
            }
            //Assume we have yet to start combat and we can go either direction
            return 0;
        }

        /// <summary>
        /// Returns the Eclipse direction we are going
        /// </summary>
        /// <returns>Eclipse direction</returns>
        public static int eclipseDir()
        {
            return goingDir();
        }
    }
}
