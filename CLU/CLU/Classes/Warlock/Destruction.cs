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

using CLU.Helpers;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using Styx.WoWInternals;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Warlock
{
    class Destruction : RotationBase
    {
        public override string Name
        {
            get {
                return "Destruction Warlock";
            }
        }

        public override string Revision
        {
            get
            {
                return "$Rev$";
            }
        }

        public override string KeySpell
        {
            get {
                return "Conflagrate";
            }
        }
        public override int KeySpellId
        {
            get { return 17962; }
        }
        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "to be done\n" +
                       "----------------------------------------------------------------------\n";
            }
        }


        //Storm..putting this here for your sanity
        /*[SpellManager] Dark Soul: Instability (113858) overrides Dark Soul (77801)
[SpellManager] Immolate (348) overrides Corruption (172)
[SpellManager] Incinerate (29722) overrides Shadow Bolt (686)
[SpellManager] Chaotic Energy (111546) overrides Life Tap (1454)*/
        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // Performance Timer (True = Enabled) returns Runstatus.Failure
                           // Spell.TreePerformance(true),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                            PetManager.CastPetSummonSpell(691, ret => !Me.GotAlivePet && (Buff.PlayerHasBuff("Demonic Rebirth") || Buff.PlayerHasBuff("Soulburn") && !WoWSpell.FromId(111897).Cooldown && TalentManager.HasTalent(14)), "Summon Pet"),
                            new Decorator(ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Soulburn")) && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503),
                                new Sequence(
                                    Buff.CastBuff("Soulburn", ret => !Me.ActiveAuras.ContainsKey("Soulburn"), "Soulburn for Pet"),
                                    new WaitContinue(new System.TimeSpan(0, 0, 0, 0, 50), ret => false, new ActionAlwaysSucceed()),
                                    Spell.CreateWaitForLagDuration(),
                                    PetManager.CastPetSummonSpell(691, ret => true, "Summon Pet"))),
                            Common.WarlockGrimoire,
                                   // Threat
                           Buff.CastBuff("Soulshatter",                    ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Soulshatter"),
                           // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50%. //Can be disabled within the GUI
                           //Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50, "Immolate"),
                           //Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50, "Corruption"),
                    //Cooldowns
                    new Decorator(ret => CLUSettings.Instance.UseCooldowns,
                        new PrioritySelector(
                            Buff.CastBuff("Dark Soul", ret => !WoWSpell.FromId(113858).Cooldown && Me.CurrentTarget != null && Unit.UseCooldowns() && !Me.IsMoving && !Me.HasAnyAura(Common.DarkSoul), "Dark Soul"),
                            Spell.CastSpell("Summon Doomguard", ret => !WoWSpell.FromId(18540).Cooldown && Me.CurrentTarget != null && Unit.UseCooldowns(), "Summon Doomguard"),
                            Spell.CastSelfSpell("Unending Resolve", ret => Me.CurrentTarget != null && Unit.UseCooldowns() && Me.HealthPercent < 40, "Unending Resolve (Save my life)"),
                            Spell.CastSelfSpell("Twilight Warden", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Unit.UseCooldowns() && Me.HealthPercent < 80, "Twilight Warden (Protect me from magical damage)"))),
                    Buff.CastDebuff("Curse of the Elements",       ret => Me.CurrentTarget != null && Unit.UseCooldowns() && Me.CurrentTarget.HealthPercent > 70 && !Buff.UnitHasMagicVulnerabilityDeBuffs(Me.CurrentTarget), "Curse of the Elements"),
                    Buff.CastDebuff("Corruption", ret => Me.CurrentTarget != null && !Me.CurrentTarget.HasMyAura("Immolate"), "Immolate"),
                    //Spell.CastSpell("Havoc", u => Unit.BestBaneOfHavocTarget, ret => true, "Havoc on "), // + Unit.BestBaneOfHavocTarget.Name
                    Spell.CastSpell("Conflagrate", ret => Me.CurrentTarget != null && Me.CurrentTarget.HasMyAura("Immolate"), "Conflagrate"),
                    new Decorator(ret => CLUSettings.Instance.EnableSelfHealing && Me.CurrentHealth <= 70, Common.WarlockTierOneTalents),
                    Spell.CastSpell("Chaos Bolt", ret => Buff.GetAuraStack(Me, "Backdraft", true) < 3 && Me.GetCurrentPower(Styx.WoWPowerType.BurningEmbers)>0 && Me.CurrentTarget.HealthPercent >= 20, "Chaos Bolt"),
                    Spell.CastSpell("Shadowburn", ret => Buff.GetAuraStack(Me, "Backdraft", true) < 3 && Me.GetCurrentPower(Styx.WoWPowerType.BurningEmbers) > 0 && Me.CurrentTarget.HealthPercent < 20, "Chaos Bolt"),
                    Spell.CastSpell("Shadow Bolt", ret => true, "Incinerate"),
                    Spell.CastSelfSpell("Life Tap", ret => Me.IsMoving && Me.HealthPercent > Me.ManaPercent && Me.ManaPercent < 80, "Life tap while moving"),
                    Spell.CastSpell("Fel Flame", ret => Me.IsMoving, "Fel flame while moving"),
                    Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 100 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40, "Life tap while mana < 100%"));
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    Unit.EnsureTarget(),
                    Movement.CreateMoveToLosBehavior(),
                    Movement.CreateFaceTargetBehavior(),
                    Spell.WaitForCast(true),
                    this.SingleRotation,
                    Movement.CreateMoveToTargetBehavior(true, 39f)
                    );
            }
        }

        public override Composite Medic
        {
            get {return Common.WarlockMedic;}
        }

        public override Composite PreCombat
        {
            get {return Common.WarlockPreCombat;}
        }

        public override Composite Resting
        {
            get {return Rest.CreateDefaultRestBehaviour();}
        }

        public override Composite PVPRotation
        {
            get {return this.SingleRotation;}
        }

        public override Composite PVERotation
        {
            get {return this.SingleRotation;}
        }
    }
}
