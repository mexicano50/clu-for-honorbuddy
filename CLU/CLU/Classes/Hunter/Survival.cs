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


using CommonBehaviors.Actions;
using Styx.TreeSharp;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Hunter
{
    using Styx.WoWInternals;

    class Survival : RotationBase
    {
        public override string Name
        {
            get {
                return "Survival Hunter";
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
                return "Explosive Shot";
            }
        }
        public override int KeySpellId
        {
            get { return 53301; }
        }
        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        // adding some help
        public override string Help
        {
            get {
                return
                    @"
----------------------------------------------------------------------
Survival:
[*] Aspect of the Fox/Aspect of the Hawk/Iron Hawk Switching while moving.
[*] Misdirection on best target (tank or pet or focus) when movement enabled.
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)
[*] Tranquilizing Shot on Enrage.
[*] Level 90 talents supported. Glaive Toss, Barrage, Powershot
[*] Always Camouflage option (see UI setting)
This Rotation will:
1. Heal using Exhilaration, Healthstone, Deterrence.
2. Trap Launcher or Feign Death will halt the rotation
3. AoE with Multishot, Barrage, Explosive Trap, Powershot, Glaive Toss
4. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Rapid Fire, Dire Beast, Fervor, A Murder of Crows
    ==> Readiness, Blink Strike, Stampede, Lynx Rush
5. Will use Heart of the Phoenix, Mend Pet, Call Pet, Revive Pet.
Recommended Pets:
===================
Fox		        -haste
Cat		        +str/agil
Core Hound	    (+haste &-cast speed) exotic
Silithid		(+health) exotic
Wolf		    (+crit)
Shale Spider	(+5% stats) exotic
Raptor		    (-armor)
Carrion Bird	(-physical damage)
Sporebat	    (-cast speed)
Ravager		    (-Phys Armor)
Wind Serpent	(-Spell Armor)
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
----------------------------------------------------------------------";
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           // Camouflage
                           Buff.CastBuff("Camouflage", ret => CLUSettings.Instance.Hunter.EnableAlwaysCamouflage, "Camouflage"),

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && (Unit.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasBuff("Feign Death")),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),

                           // Main Rotation
                           new Decorator(
                               ret => !Buff.PlayerHasBuff("Feign Death"),
                               new PrioritySelector(
                                   // HandleMovement? Lets Misdirect to Focus, Pet, RafLeader or Tank
                                   // TODO: Add Binding shot logic..need to see it working well.
                                   Common.HandleMisdirection(),
                                   Buff.CastDebuff("Hunter's Mark",          ret => true, "Hunter's Mark"),
                                   Spell.CastSelfSpell("Feign Death",        ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && CLUSettings.Instance.Hunter.UseFeignDeath, "Feign Death Threat"),
                                   Spell.CastSpell("Concussive Shot",        ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && CLUSettings.Instance.Hunter.UseConcussiveShot, "Concussive Shot"),
                                   Spell.CastSpell("Tranquilizing Shot",     ret => Buff.TargetHasBuff("Enrage") && CLUSettings.Instance.Hunter.UseTranquilizingShot, "Tranquilizing Shot"),
                                   Common.HandleAspectSwitching(),
                                   Spell.CastSpell("Kill Shot",              ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 20, "Kill Shot"),
                                   // AoE
                                   Spell.HunterTrapBehavior("Explosive Trap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Lists.BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) >= CLUSettings.Instance.Hunter.ExplosiveTrapCount),
                                   Buff.CastBuff("A Murder of Crows",         ret => Unit.IsTargetWorthy(Me.CurrentTarget), "A Murder of Crows"), //reduced to 60sec cooldown if under 20%
                                    Spell.CastSpell("Blink Strike",           ret => Me.CurrentTarget != null && Me.Pet.Location.Distance(Me.CurrentTarget.Location) > 10 && Me.GotAlivePet, "Blink Strike"), // teleports behind target mad damage.
                                   Spell.CastSpell("Lynx Rush",               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 10, "Lynx Rush"),
                                   Spell.CastSpell("Multi-Shot",              ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) >= CLUSettings.Instance.Hunter.SurvMultiShotCount, "Multi-Shot"),
                                   //Spell.CastSpell("Cobra Shot",              ret => Unit.CountEnnemiesInRange(Me.Location, 30) >= CLUSettings.Instance.Hunter.SurvMultiShotCount, "Cobra Shot"),
                                   // Main rotation
                                   //Spell.CastSpellByID(53301, ret => !WoWSpell.FromId(53301).Cooldown, "Explosive Shot"),
                                   Spell.CastSpell("Explosive Shot",          ret => Buff.TargetDebuffTimeLeft("Explosive Shot").TotalSeconds <= 1 || Buff.PlayerHasActiveBuff("Lock and Load"), "Explosive Shot"),
                                   Buff.CastDebuff("Serpent Sting",           ret => Me.CurrentTarget != null && Buff.TargetDebuffTimeLeft("Serpent Sting").TotalSeconds <= 0.5 && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Serpent Sting"),
                                   Spell.CastSpell("Black Arrow",             ret => !Buff.TargetHasDebuff("Black Arrow") && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Black Arrow"),
                                   Spell.CastSpell("Arcane Shot",             ret => Buff.PlayerHasBuff("Thrill of the Hunt"), "Arcane Shot"),
                                   Spell.CastSpell("Dire Beast",              ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 10, "Dire Beast"),
                                   Buff.CastBuff("Stampede",                  ret => Unit.IsTargetWorthy(Me.CurrentTarget), "Stampede"),
                                   Buff.CastBuff("Rapid Fire",                ret => Me.CurrentTarget != null && !Buff.UnitHasHasteBuff(Me) && Unit.IsTargetWorthy(Me.CurrentTarget), "Rapid Fire"),
                                   Buff.CastBuff("Readiness",                 ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerHasActiveBuff("Rapid Fire"), "Readiness"),
                                   Spell.CastSpell("Arcane Shot",             ret => Me.FocusPercent >= CLUSettings.Instance.Hunter.SurArcaneShotFocusPercent && !Buff.PlayerHasActiveBuff("Lock and Load"), "Arcane Shot"),
                                   Buff.CastBuff("Fervor",                    ret => Me.CurrentTarget != null && Me.FocusPercent <= CLUSettings.Instance.Hunter.SurFevorFocusPercent && Unit.IsTargetWorthy(Me.CurrentTarget), "Fervor"),
                                   Spell.CastSpell("Cobra Shot",              ret => true, "Cobra Shot"))));
            }
        }

        public override Composite Medic
        {
            get {
                return new PrioritySelector(
                           // Make sure we go our pet.
                           Common.HunterCallPetBehavior(CLUSettings.Instance.Hunter.ReviveInCombat),
                           new Decorator(
                               ret => Me.HealthPercent < 100 && !Buff.PlayerHasBuff("Feign Death") && CLUSettings.Instance.EnableSelfHealing,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Exhilaration",      ret => Me.HealthPercent < CLUSettings.Instance.Hunter.ExhilarationPercent, "Exhilaration"),
                                   Item.UseBagItem("Healthstone",           ret => Me.HealthPercent < CLUSettings.Instance.Hunter.HealthstonePercent, "Healthstone"),
                                   Spell.CastSelfSpell("Deterrence",        ret => Me.HealthPercent < CLUSettings.Instance.Hunter.DeterrencePercent && Me.HealthPercent > 1, "Deterrence"))),
                           // Heart of the Phoenix, Mend Pet, etc
                           Common.HandlePetHelpers());
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink") && !Buff.PlayerHasBuff("Feign Death") && !Buff.PlayerHasBuff("Trap Launcher"),
                           new PrioritySelector(
                               Common.HunterCallPetBehavior(CLUSettings.Instance.Hunter.ReviveInCombat)));
            }
        }

        public override Composite Resting
        {
            get {
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get {
                return this.SingleRotation;
            }
        }

        public override Composite PVERotation
        {
            get {
                return this.SingleRotation;
            }
        }
    }
}