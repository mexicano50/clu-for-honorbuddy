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

using System.Linq;
using Styx;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

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
                       "Credits: alxaw , Kbrebel04" +
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
                    ret => StyxWoW.Me.CurrentTarget.IsFlying && CLUSettings.Instance.EnableMovement,
                    new PrioritySelector(
                        Spell.ChannelSpell("Crackling Jade Lightning", ret => true, "Crackling Jade Lightning")));
            }
        }

        // TODO: CHECK COMBO BREAKER NAMES.
        // TODO: CHECK CHI
        // TODO: FIND AWAY TO RETURN ENERGY REGEN RATE
        // TODO: CHECK ALL SPELL NAMES FROM "SPELLS" DUMP
        // TODO: CHECK ALL AURAS
        // TODO: CHECK JAB IS NOT AFFECTED BY THE WEAPON YOU ARE CARRYING AND WE ONLY NEED TO USE JAB AND THE SPELLID AND ICON WILL CHANGE.

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
                        ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                            Item.UseEngineerGloves())),

                    HandleFlyingUnits,

                    // Interupt
                    Spell.CastInterupt("Spear Hand Strike", ret => true, "Spear Hand Strike"),
                    Spell.CastSpell("Leg Sweep", ret => TalentManager.HasTalent(12) && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 2 && Me.CurrentTarget.IsWithinMeleeRange, "Leg Sweep"),

                    //Single Target
                    Spell.CastSpell("Clash", ret => Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 50 * 50, "Clash"),
                    Spell.CastSpell("Touch of Death", ret => Buff.PlayerHasBuff("Death Note"), "Touch of Death"),
                    Spell.CastSpell("Elusive Brew", ret => !Buff.PlayerHasBuff("Elusive Brew Use") && Buff.PlayerCountBuff("Elusive Brew") >= 6 && Me.HealthPercent <= 80, "Elusive Brew"),
                    Spell.CastSpell("Purifying Brew", ret => Chi >= 1 && Me.HasAura("Moderate Stagger") && Me.HealthPercent <= 60 || Chi >= 1 && Me.HasAura("Heavy Stagger") && Me.HealthPercent <= 60, "Purifying Brew"),
                    Spell.CastOnUnitLocation("Summon Black Ox Statue", u => Me.CurrentTarget, ret => Me.HealthPercent <= 70, "Summon Black Ox Statue"),
                    Spell.CastSpell("Disable", ret => Me.CurrentEnergy >= 15 && (Me.CurrentTarget.IsPlayer || Me.CurrentTarget.Fleeing) && Me.CurrentTarget.MovementInfo.RunSpeed > 3.5, "Disable"),
                    Spell.CastSelfSpell("Chi Wave", ret => TalentManager.HasTalent(4) && Chi >= 2 && Me.HealthPercent <= 40, "Chi Wave"),
                    Spell.CastSelfSpell("Guard", ret => Chi >= 2 && Buff.PlayerHasBuff("Power Guard"), "Guard"),
                    Spell.CastSpell("Tiger Palm", ret => Buff.PlayerCountBuff("Tiger Power") < 3 || Buff.PlayerBuffTimeLeft("Tiger Power") <= 3, "Tiger Palm"),
                    Spell.CastSpell("Blackout Kick", ret => Chi >= 2 && !Buff.PlayerHasActiveBuff("Shuffle") || Chi >= 2 && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Buff.PlayerHasActiveBuff("Guard"), "Blackout Kick"),
                    Spell.CastSelfSpell("Invoke Xuen, the White Tiger", ret => TalentManager.HasTalent(17) && Buff.PlayerCountBuff("Tiger Power") == 3 && Me.CurrentEnergy <= 80, "Invoke Xuen"),
                    Spell.CastSpell("Rushing Jade Wind", ret => TalentManager.HasTalent(16) && Buff.PlayerCountBuff("Tiger Power") == 3 && Me.CurrentEnergy <= 80, "Rushing Jade Wind"),
                    Spell.CastSpell("Keg Smash", ret => Chi <= 2, "Keg Smash"),
                    Spell.CastSpell("Expel Harm", ret => Chi <= 2 && Me.HealthPercent <= 80 && Spell.SpellOnCooldown("Keg Smash"), "Keg Smash"),
                    Spell.CastSpell("Touch of Karma", ret => Chi <= 2 && Me.HealthPercent <= 40, "Touch of Karma"),
                    Spell.CastSpell("Jab", ret => Chi <= 2 && Me.HealthPercent > 80 && Spell.SpellOnCooldown("Keg Smash") || Chi <= 2 && Spell.SpellOnCooldown("Expel Harm") && Spell.SpellOnCooldown("Keg Smash") && Me.HealthPercent <= 80, "Jab"),
                    Spell.CastSpell("Tiger Palm", ret => Chi < 2 && Me.CurrentEnergy < 40, "Tiger Palm"),

                    
                    // AoE
                    Spell.CastAreaSpell("Dizzying Haze", 10, false, 3, 0.0, 0.0, ret => (from enemy in Unit.EnemyUnits where !enemy.HasAura("Dizzying Haze") select enemy).Any(), "Dizzying Haze"),
                    //Spell.CastOnGround("Dizzying Haze", u => Me.CurrentTarget.Location, ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 3 && !Buff.TargetHasDebuff("Dizzying Haze")),
                    Spell.CastSpell("Breath of Fire", ret => Chi >= 2 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 3 && Buff.TargetHasDebuff("Dizzying Haze") && !Buff.TargetHasDebuff("Breath of Fire"), "Breath of Fire"),
                    Spell.CastAreaSpell("Spinning Crane Kick", 8, false, 7, 0.0, 0.0, ret => true, "Spinning Crane Kick"));
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => CLUSettings.Instance.EnableMovement,
                        new PrioritySelector(
                            Spell.CastSpell("Provoke", ret => !CLU.IsMounted && (Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8), "Provoke"),
                            Spell.CastSpell("Roll", ret => !CLU.IsMounted && (Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 10 * 10), "Roll"),
                            this.SingleRotation)),
                    this.SingleRotation
                    );
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Buff.CastBuff("Fortifying Brew", ret => Me.HealthPercent < 50, "Fortifying Brew"),
                    // Turns your skin to stone, increasing your health by 20%, and reducing damage taken by 20%. Lasts 20 sec.
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