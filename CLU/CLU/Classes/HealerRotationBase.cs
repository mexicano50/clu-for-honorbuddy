#region Revision info
/*
 * $Author: clutwopointzero@gmail.com $
 * $Date: 2012-10-02 06:11:22 -0400 (Tue, 02 Oct 2012) $
 * $ID$
 * $Revision: 583 $
 * $URL: https://clu-for-honorbuddy.googlecode.com/svn/trunk/CLU/CLU/Classes/HealerRotationBase.cs $
 * $LastChangedBy: clutwopointzero@gmail.com $
 * $ChangesMade$
 */
#endregion

using System.Linq;
using System;
using CLU.Helpers;
using Styx;
using Styx.WoWInternals.WoWObjects;
using CLU.Base;
using CLU.Managers;
using Styx.TreeSharp;
using System.Text;

namespace CLU.Classes
{
    using Styx.WoWInternals;

    public abstract class HealerRotationBase : RotationBase
    {
        protected static readonly TargetBase Healer = TargetBase.Instance;


        protected static WoWUnit HealTarget
        {
            get
            {
                return HealableUnit.HealTarget.ToUnit();
            }
        }

        protected static double CurrentTargetHealthPercent
        {
            get
            {
                return HealTarget != null ? HealTarget.HealthPercent : 9999;
            }
        }
        protected static bool IsTank
        {
            get
            {
                return HealTarget != null && Unit.Tanks.Any(u => u.Guid == HealTarget.Guid);
            }
        }

        protected static bool IsHealer
        {
            get
            {
                return HealTarget != null && Unit.Healers.Any(u => u.Guid == HealTarget.Guid);
            }
        }

        protected static bool IsMe
        {
            get
            {
                return HealTarget != null && HealTarget.IsMe;
            }
        }

        protected static bool HealthCheck(int percent)
        {
            return HealTarget != null && HealTarget.HealthPercent < percent;
        }

        // Restoration Druid ------------------------------------------------------------------------------------------------------------

        protected static bool CanNourish
        {
            get
            {
                return HealTarget.HasAura("Wild Growth") || HealTarget.HasAura("Rejuvenation") || !HealTarget.HasAura("Harmony") || HealTarget.HasAura("Regrowth") || HealTarget.HasAura("Lifebloom");
            }
        }

        //Monk ------------ ------------------------------------------------------------------------------------------------------------
        protected static bool ChannelingSoothingMist
        {
            get
            {
                return Spell.PlayerIsChanneling && StyxWoW.Me.ChanneledCastingSpellId == 115175;
            }
        }        
        protected static bool GotTea
        {
            get
            {
                return Buff.PlayerCountBuff("Mana Tea") > 2;
            }
        }
        protected static uint Chi
        {
            get
            {
                return Me.CurrentChi;// StyxWoW.Me.GetCurrentPower(WoWPowerType.LightForce);
            }
        }
        protected static bool MaxSpheres
        {
            get
            {
                //Getting much more spheres than we want.
                //HB is puting out more than the max allowed and WoW is letting it.
                //We need a condition to stop sphere spam, idk how to see how many are out.
                //casting only on odd seconds should cut down the sphere spam a bit.
                return DateTime.Now.Second % 2 == 0;
                //return ObjectManager.ObjectList.Where(obj => obj.Name == "Healing Sphere").Count() >= 3;
            }
        }

        // Discipline/Holy Priest Shared ------------------------------------------------------------------------------------------------
        protected static bool CanShield
        {
            get
            {
                return HealTarget.HasAura("Weakened Soul") && StyxWoW.Me.Combat;//!Buff.TargetHasDebuff("Weakened Soul") && StyxWoW.Me.Combat;
            }
        }

        protected static WoWUnit ShadowfiendTarget
        {
            get
            {
                return Battlegrounds.IsInsideBattleground ? Unit.RangedPvPUnits.FirstOrDefault(u => u.DistanceSqr < 35 * 35 && !Unit.UnitIsControlled(u, true)) : Unit.RangedPvEUnits.FirstOrDefault(u => !Unit.UnitIsControlled(u, true));
            }
        }

        protected static bool NotMoving
        {
            get
            {
                return !Me.IsMoving;
            }
        }

        // Discipline Priest ------------------------------------------------------------------------------------------------------------
        protected static uint GraceStacks
        {
            get
            {
                return HealTarget != null ? Buff.GetAuraStack(HealTarget, "Grace", true) : 0;
            }
        }

        protected static bool IsAtonementSpec
        {
            get
            {
                return TalentManager.HasTalent(5) && TalentManager.HasGlyph("Divine Accuracy") && TalentManager.HasGlyph("Smite");
            }
        }

        // Holy Priest -------------------------------------------------------------------------------------------------------------------
        protected static bool HasSerendipity
        {
            get
            {
                return Buff.PlayerCountBuff("Serendipity") == 2;    // activated with Binding Heal or Flash Heal Greater Heal or Prayer of Healing
            }
        }

        protected static bool IsSpiritofRedemption
        {
            get
            {
                return !Buff.PlayerHasBuff("Spirit of Redemption");
            }
        }

        // Restoration Shaman ------------------------------------------------------------------------------------------------------------
        protected static bool CanEarthshield
        {
            get
            {
                return HealTarget != null && HealableUnit.ListofHealableUnits.All(t => !t.ToUnit().GetAllAuras().Any(b => b.Name == "Earth Shield" && b.CreatorGuid == Me.Guid) && t.EarthShield);
            }
        }

        // Holy Paladin ------------------------------------------------------------------------------------------------------------------

        protected static uint HolyPower
        {
            get
            {
                return Me.CurrentHolyPower;
            }
        }

        protected static bool PaladinCooldownsOk
        {
            get
            {
                return HealTarget != null && Spell.SpellCooldown("Divine Favor").TotalSeconds < 0.5 || Spell.SpellCooldown("Guardian of Ancient Kings").TotalSeconds < 0.5 || Spell.SpellCooldown("Avenging Wrath").TotalSeconds < 0.5;
            }
        }
    }
}
