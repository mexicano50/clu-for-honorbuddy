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

using System;
using System.Linq;
using Styx;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;
using Action = Styx.TreeSharp.Action;

namespace CLU.Classes.Monk
{
    internal class Brewmaster : RotationBase
    {
        public override string Name
        {
            get { return "Brewmaster Monk"; }
        }

        public override string Revision
        {
            get { return "$Rev$"; }
        }

        public override string KeySpell
        {
            get { return "Dizzying Haze"; }
        }

        public override int KeySpellId
        {
            get { return 115180; }
        }

        public override float CombatMaxDistance
        {
            get { return 3.2f; }
        }

        public override string Help
        {
            get
            {
                return "----------------------------------------------------------------------\n" +
                       "Credits: Dagradt" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static uint Chi
        {
            get
            {
                return Me.CurrentChi; // StyxWoW.Me.GetCurrentPower(WoWPowerType.LightForce);
            }
        }
        
        public static Composite HandleFlyingUnits
        {
            get
            {
                //Shoot flying targets
                return new Decorator(
                    ret => StyxWoW.Me.CurrentTarget != null && (StyxWoW.Me.CurrentTarget.IsFlying || StyxWoW.Me.CurrentTarget.Distance2DSqr < 5 * 5 && Math.Abs(StyxWoW.Me.Z - StyxWoW.Me.CurrentTarget.Z) >= 5) && CLUSettings.Instance.EnableMovement,
                    new PrioritySelector(
                        Spell.ChannelSpell("Crackling Jade Lightning", ret => true, "Crackling Jade Lightning")));
            }
        }

        // TODO: GUI OPTIONS FOR FUCKING EVERYTHIGN!!!
        // TODO: FIND AWAY TO RETURN ENERGY REGEN RATE
        // TODO: CHECK ALL SPELL NAMES FROM "SPELLS" DUMP
        // TODO: CHECK ALL AURAS

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),
                    new Decorator(
                        ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                            Item.UseEngineerGloves())),

                    HandleFlyingUnits,

                    // Interupt
                    Spell.CastInterupt("Spear Hand Strike", ret => true, "Spear Hand Strike"),
                    Spell.CastSpell("Leg Sweep", ret => TalentManager.HasTalent(12) && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 2 && Me.CurrentTarget.IsWithinMeleeRange, "Leg Sweep"),

                    //Not sure what to do with these
                    Spell.CastSpell("Invoke Xuen, the White Tiger", ret => TalentManager.HasTalent(17) && Buff.PlayerCountBuff("Tiger Power") == 3 && Me.CurrentEnergy <= 80, "Invoke Xuen"),
                    Spell.CastSpell("Touch of Karma", ret => Chi <= 2 && Me.HealthPercent <= 40, "Touch of Karma"),
                    Spell.CastSpell("Clash", ret => Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 50 * 50, "Clash"),
                    Spell.CastSpell("Disable", ret => Me.CurrentEnergy >= 15 && (Me.CurrentTarget.IsPlayer || Me.CurrentTarget.Fleeing) && Me.CurrentTarget.MovementInfo.RunSpeed > 3.5, "Disable"),

                    //Single Target
                    new Decorator(ret => Me.CurrentChi < 4,
                        new PrioritySelector(
                            Spell.CastSpell("Keg Smash", ret => Me.CurrentEnergy >= 40 && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 8 * 8 && Me.CurrentChi <= 2, "Keg Smash"),
                            Spell.CastSelfSpell("Expel Harm", ret => Me.CurrentEnergy >= 40 && Me.HealthPercent < 100, "Expel Harm"),
                            Spell.CastSpell("Jab", ret => Me.CurrentEnergy >= 40 && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Me.HealthPercent > 35, "Jab")
                            )),
                    new Decorator(ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Unit.CountEnnemiesInRange(Me.Location, 8) < 3,
                        new PrioritySelector(
                            Spell.CastSpell("Touch of Death", ret => Me.CurrentChi >= 3 && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Buff.PlayerHasBuff("Death Note"), "Touch of Death"),//~> GUI option for use on boss only
                            //Spell.CastSpell("Rushing Jade Wind", ret => TalentManager.HasTalent(16) && Buff.PlayerCountBuff("Tiger Power") == 3 && Me.CurrentEnergy <= 80, "Rushing Jade Wind"),
                            Spell.CastSpell("Blackout Kick", ret => Me.CurrentChi >= 2 && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && (!Buff.PlayerHasActiveBuff("Shuffle") || Buff.PlayerActiveBuffTimeLeft("Shuffle").Seconds < 18), "Blackout Kick"),//~> GUI option for Shuffle min time
                            Spell.CastSpell("Tiger Palm", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Buff.PlayerCountBuff("Tiger Power") < 3 || Buff.PlayerCountBuff("Power Guard") < 3, "Tiger Palm"),
                            Spell.CastSpell("Tiger Palm", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentEnergy < 40, "Tiger Palm")
                            )),
                    new Decorator(ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Unit.CountEnnemiesInRange(Me.Location, 8) >= 3,
                        new PrioritySelector(
                            Spell.CastOnUnitLocation("Dizzying Haze", ret => Me.CurrentTarget, ret => Me.CurrentEnergy >= 20 && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 40 * 40 && Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 8, CLU.LocationContext == GroupLogic.Battleground).Any(x => !x.HasAura("Dizzying Haze")), "Dizzying Haze"),//~> GUI option for mob count
                            Spell.CastSpell("Breath of Fire", ret => Me.CurrentChi >= 2 && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 8 * 8 && Unit.NearbyNonControlledUnits(Me.Location, 8, CLU.LocationContext == GroupLogic.Battleground).Any(x => !x.HasAura("Breath of Fire")), "Breath of Fire"),//~> GUI option for mob count
                            Spell.CastAreaSpell("Spinning Crane Kick", 8, false, 5, 0.0, 0.0, ret => Me.CurrentEnergy >= 40, "Spinning Crane Kick"),//~> GUI option for mob count
                            //Spell.CastSpell("Rushing Jade Wind", ret => TalentManager.HasTalent(16) && Buff.PlayerCountBuff("Tiger Power") == 3 && Me.CurrentEnergy <= 80, "Rushing Jade Wind"),
                            Spell.CastSpell("Blackout Kick", ret => Me.CurrentChi >= 2 && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && (!Buff.PlayerHasActiveBuff("Shuffle") || Buff.PlayerActiveBuffTimeLeft("Shuffle").Seconds < 18), "Blackout Kick"),//~> GUI option for Shuffle min time
                            Spell.CastSpell("Tiger Palm", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Buff.PlayerCountBuff("Tiger Power") < 3 || Buff.PlayerCountBuff("Power Guard") < 3, "Tiger Palm"),
                            Spell.CastSpell("Tiger Palm", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentEnergy < 40, "Tiger Palm")
                            )),
                    new Decorator(ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 8 * 8 && Unit.CountEnnemiesInRange(Me.Location, 8) >= 3,
                        new PrioritySelector(
                            Spell.CastOnUnitLocation("Dizzying Haze", ret => Me.CurrentTarget, ret => Me.CurrentEnergy >= 20 && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 40 * 40 && Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 8, CLU.LocationContext == GroupLogic.Battleground).Any(x => !x.HasAura("Dizzying Haze")), "Dizzying Haze"),//~> GUI option for mob count
                            Spell.CastSpell("Breath of Fire", ret => Me.CurrentChi >= 2 && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 8 * 8 && Unit.NearbyNonControlledUnits(Me.Location, 8, CLU.LocationContext == GroupLogic.Battleground).Any(x => !x.HasAura("Breath of Fire")), "Breath of Fire"),//~> GUI option for mob count
                            Spell.CastAreaSpell("Spinning Crane Kick", 8, false, 5, 0.0, 0.0, ret => Me.CurrentEnergy >= 40, "Spinning Crane Kick")//~> GUI option for mob count
                            )),
                    new Decorator(ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 8 * 8 && Me.CurrentTarget.DistanceSqr <= 40 * 40 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 3,
                        new PrioritySelector(
                            Spell.CastOnUnitLocation("Dizzying Haze", ret => Me.CurrentTarget, ret => Me.CurrentEnergy >= 20 && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 40 * 40 && Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 8, CLU.LocationContext == GroupLogic.Battleground).Any(x => !x.HasAura("Dizzying Haze")), "Dizzying Haze")//~> GUI option for mob count
                            )));    
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    Movement.CreateMoveToLosBehavior(),
                    Movement.CreateFaceTargetBehavior(),
                    Spell.CastSpell("Crackling Jade Lightning", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Spell.SpellOnCooldown("Roll"), "Crackling Jade Lightning"),
                    Spell.CastSpell("Provoke", ret => !CLU.IsMounted && (Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8), "Provoke"),
                    Spell.CastSpell("Roll", ret => !CLU.IsMounted && (Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 10 * 10), "Roll"),
                    this.SingleRotation,
                    Movement.CreateMoveToMeleeBehavior(true));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Buff.CastBuff("Fortifying Brew", ret => Me.HealthPercent < 50, "Fortifying Brew"),//~> GUI option for HP%
                        //avert_harm
                        Spell.CastSelfSpell("Purifying Brew", ret => Me.CurrentChi >= 1 && Buff.PlayerHasActiveBuff("Moderate Stagger") || Buff.PlayerHasActiveBuff("Heavy Stagger"), "Purifying Brew"),
                        Spell.CastSelfSpell("Elusive Brew", ret => Buff.PlayerCountBuff("Elusive Brew") >= 9, "Elusive Brew"),//~> GUI option for stacks
                        Spell.CastSelfSpell("Guard", ret => Me.CurrentChi >= 2 && Buff.PlayerCountBuff("Power Guard") == 3, "Guard"),
                        //Spell.CastSpell("Rushing Jade Wind", ret => TalentManager.HasTalent(16) && Buff.PlayerCountBuff("Tiger Power") == 3 && Me.CurrentEnergy <= 80, "Rushing Jade Wind"),
                        Spell.CastSpell("Blackout Kick", ret => Me.CurrentChi >= 2 && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && (!Buff.PlayerHasActiveBuff("Shuffle") || Buff.PlayerActiveBuffTimeLeft("Shuffle").Seconds < 18), "Blackout Kick"),//~> GUI option for Shuffle min time
                        Spell.CastOnUnitLocation("Summon Black Ox Statue", ret => Me.CurrentTarget, ret => !Buff.PlayerHasBuff("Sanctuary of the Ox"), "Summon Black Ox Statue"),
                        Spell.CastOnUnitLocation("Healing Sphere", ret => Me, ret => Me.CurrentEnergy >= 60 && Me.HealthPercent <= 50, "Healing Sphere"),//~> GUI option for HP%
                        Spell.CastSelfSpell("Zen Sphere", ret => Me.CurrentChi >= 2 && Me.HealthPercent <= 75 && !Buff.PlayerHasActiveBuff("Zen Sphere") || Me.HealthPercent <= 50 && Buff.PlayerHasActiveBuff("Zen Sphere"), "Zen Sphere"),//~> GUI option for HP%
                        Spell.CastSpell("Chi Wave", ret => TalentManager.HasTalent(4) && Chi >= 2 && Me.HealthPercent <= 40, "Chi Wave"),//~> This is usless
                        Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                    ret =>
                    !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") &&
                    !Me.HasAura("Drink"),
                    new PrioritySelector(
                        Buff.CastBuff("Stance of the Sturdy Ox", ret => !Me.HasMyAura("Stance of the Sturdy Ox"), "Stance of the Sturdy Ox We need it!"),
                        Buff.CastRaidBuff("Legacy of the Emperor", ret => true, "Legacy of the Emperor")));
            }
        }

        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}