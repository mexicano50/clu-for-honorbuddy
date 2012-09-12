using System.Linq;
using CLU.Helpers;
using Styx;
using Styx.WoWInternals.WoWObjects;
using CLU.Base;
using CLU.Managers;

namespace CLU.Classes
{
    using Styx.WoWInternals;

    public abstract class HealerRotationBase : RotationBase
    {
        protected static readonly TargetBase Healer = TargetBase.Instance;

        protected static double CurrentTargetHealthPercent
        {
            get {
                return Me.CurrentTarget != null ? Me.CurrentTarget.HealthPercent : 9999;
            }
        }
        protected static bool IsTank
        {
            get {
                return Me.CurrentTarget != null && Unit.Tanks.Any(u => u.Guid == Me.CurrentTarget.Guid);
            }
        }

        protected static bool IsHealer
        {
            get {
                return Me.CurrentTarget != null && Unit.Healers.Any(u => u.Guid == Me.CurrentTarget.Guid);
            }
        }

        protected static bool IsMe
        {
            get {
                return Me.CurrentTarget != null && Me.CurrentTarget.IsMe;
            }
        }

        // Restoration Druid ------------------------------------------------------------------------------------------------------------

        protected static bool CanNourish
        {
            get {
                return Buff.TargetHasBuff("Wild Growth") || Buff.TargetHasBuff("Rejuvenation") || !Buff.PlayerHasBuff("Harmony") || Buff.TargetHasBuff("Regrowth") || Buff.TargetHasBuff("Lifebloom");
            }
        }

        // Discipline/Holy Priest Shared ------------------------------------------------------------------------------------------------
        protected static bool CanShield
        {
            get {
                return !Buff.TargetHasDebuff("Weakened Soul") && StyxWoW.Me.Combat;
            }
        }

        protected static WoWUnit ShadowfiendTarget
        {
            get {
                return Battlegrounds.IsInsideBattleground ? Unit.RangedPvPUnits.FirstOrDefault(u => u.Distance < 35 && !Unit.UnitIsControlled(u, true)) : Unit.RangedPvEUnits.FirstOrDefault(u => !Unit.UnitIsControlled(u, true));
            }
        }

        protected static bool NotMoving
        {
            get {
                return !Me.IsMoving;
            }
        }

        // Discipline Priest ------------------------------------------------------------------------------------------------------------
        protected static uint GraceStacks
        {
            get {
                return Buff.TargetCountBuff("Grace");
            }
        }

        protected static bool IsAtonementSpec
        {
            get {
                return TalentManager.HasTalent(5) && TalentManager.HasGlyph("Divine Accuracy") && TalentManager.HasGlyph("Smite");
            }
        }

        // Holy Priest -------------------------------------------------------------------------------------------------------------------
        protected static bool HasSerendipity
        {
            get {
                return Buff.PlayerCountBuff("Serendipity") == 2;    // activated with Binding Heal or Flash Heal Greater Heal or Prayer of Healing
            }
        }

        protected static bool IsSpiritofRedemption
        {
            get {
                return !Buff.PlayerHasBuff("Spirit of Redemption");
            }
        }

        // Restoration Shaman ------------------------------------------------------------------------------------------------------------
        protected static bool CanEarthshield
        {
            get {
                return Me.CurrentTarget != null && HealableUnit.ListofHealableUnits.All(t => !t.ToUnit().GetAllAuras().Any(b => b.Name == "Earth Shield" && b.CreatorGuid == Me.Guid) && t.EarthShield);
            }
        }

        // Holy Paladin ------------------------------------------------------------------------------------------------------------------

        protected static uint HolyPower
        {
            get {
                return Me.CurrentHolyPower;
            }
        }

        protected static bool PaladinCooldownsOk
        {
            get {
                return Me.CurrentTarget != null && Spell.SpellCooldown("Divine Favor").TotalSeconds < 0.5 || Spell.SpellCooldown("Guardian of Ancient Kings").TotalSeconds < 0.5 || Spell.SpellCooldown("Avenging Wrath").TotalSeconds < 0.5;
            }
        }
    }
}
