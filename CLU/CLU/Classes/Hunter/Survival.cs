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

    using global::CLU.Managers;
    using Styx.CommonBot;

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
NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'.
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
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),

                           // Main Rotation
                           new Decorator(
                               ret => !Buff.PlayerHasBuff("Feign Death"),
                               new PrioritySelector(
                                   // HandleMovement? Lets Misdirect to Focus, Pet, RafLeader or Tank
                                   // TODO: Add Binding shot logic..need to see it working well.
                                   Common.HandleMisdirection(),
                                   Buff.CastDebuff("Hunter's Mark", ret => !TalentManager.HasGlyph("Marked for Death"), "Hunter's Mark"),
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

        public Composite burstRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                ));
            }
        }

        public Composite baseRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                        //new Action(a => { CLU.Log("I am the start of public Composite baseRotation"); return RunStatus.Failure; }),
                        //PvP Utilities
                        Spell.CastSpell("Concussive Shot", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 25 * 25, "Concussive Shot"),
                        Spell.CastSpell("Widow Venom", ret => !Buff.TargetHasDebuff("Widow Venom"), "Widow Venom"),
                        Spell.CastSpell("Tranquilizing Shot", ret => Buff.TargetHasBuff("Enrage"), "Tranquilizing Shot"),
                        Buff.CastBuff("Mend Pet", ret => Me.Pet.HealthPercent <= 90 && !Me.Pet.HasAura("Mend Pet"), "Mend Pet"),

                        //Rotation
                        //irmens_bite_potion,if=buff.bloodlust.react|target.time_to_die<=60
                        //blood_fury
                        Racials.UseRacials(),
                        //aspect_of_the_hawk,moving=0
                        Buff.CastBuff("Aspect of the Hawk", ret => !Me.IsMoving && !Buff.PlayerHasBuff("Aspect of the Hawk") && SpellManager.HasSpell("Aspect of the Hawk"), "Aspect of the Hawk"),
                        Buff.CastBuff("Aspect of the Iron Hawk", ret => !Me.IsMoving && !Buff.PlayerHasBuff("Aspect of the Iron Hawk") && SpellManager.HasSpell("Aspect of the Iron Hawk"), "Aspect of the Iron Hawk"),
                        //aspect_of_the_fox,moving=1
                        Buff.CastBuff("Aspect of the Fox", ret => Me.IsMoving && !Buff.PlayerHasBuff("Aspect of the Fox"), "Aspect of the Fox"),
                        //explosive_trap,if=target.adds>0
                        Spell.HunterTrapBehavior("Explosive Trap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 40 * 40 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 0),
                        //a_murder_of_crows,if=enabled&!ticking
                        Spell.CastSpell("A Murder of Crows", ret => SpellManager.HasSpell("A Murder of Crows") && !Buff.TargetHasDebuff("A Murder of Crows"), "A Murder of Crows"),
                        //blink_strike,if=enabled
                        Spell.CastSpell("Blink Strike", ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) <= 40 * 40 && Me.GotAlivePet && SpellManager.HasSpell("Blink Strike"), "Blink Strike"),
                        //lynx_rush,if=enabled&!ticking
                        Spell.CastSpell("Lynx Rush", ret => Me.CurrentTarget != null && Me.Pet.Location.DistanceSqr(Me.CurrentTarget.Location) <= 10 * 10 && Me.GotAlivePet && SpellManager.HasSpell("Lynx Rush") && !SpellManager.Spells["Lynx Rush"].Cooldown, "Lynx Rush"),
                        //explosive_shot,if=buff.lock_and_load.react
                        Spell.CastSpell("Explosive Shot", ret => Buff.PlayerHasActiveBuff("Lock and Load"), "Explosive Shot"),
                        //glaive_toss,if=enabled
                        Spell.CastSpell("Glaive Toss", ret => SpellManager.HasSpell("Glaive Toss"), "Glaive Toss"),
                        //powershot,if=enabled
                        Spell.CastSpell("Powershot", ret => SpellManager.HasSpell("Powershot"), "Powershot"),
                        //barrage,if=enabled
                        Spell.CastSpell("Barrage", ret => SpellManager.HasSpell("Barrage"), "Barage"),
                        //multi_shot,if=target.adds>2
                        //cobra_shot,if=target.adds>2
                        //serpent_sting,if=!ticking&target.time_to_die>=10
                        Spell.CastSpell("Serpent Sting", ret => !Buff.TargetHasDebuff("Serpent Sting") && Unit.TimeToDeath(Me.CurrentTarget) >= 10, "Serpent Sting"),
                        //explosive_shot,if=cooldown_react
                        Spell.CastSpell("Explosive Shot", ret => true, "Explosive Shot"),
                        //kill_shot
                        Spell.CastSpell("Kill Shot", ret => true, "Kill Shot"),
                        //black_arrow,if=!ticking&target.time_to_die>=8
                        Spell.CastSpell("Black Arrow", ret => !Buff.TargetHasDebuff("Black Arrow") && Unit.TimeToDeath(Me.CurrentTarget) >= 8, "Black Arrow"),
                        //multi_shot,if=buff.thrill_of_the_hunt.react
                        Spell.CastSpell("Multi Shot", ret => Buff.PlayerHasActiveBuff("Thrill of the Hunt"), "Multi Shot"),
                        //dire_beast,if=enabled
                        Spell.CastSpell("Dire Beast", ret => SpellManager.HasSpell("Dire Beast"), "Dire Beast"),
                        //rapid_fire,if=!buff.rapid_fire.up
                        Buff.CastBuff("Rapid Fire", ret => !Buff.PlayerHasActiveBuff("Rapid Fire"), "Rapid Fire"),
                        //stampede
                        Spell.CastSpell("Stampede", ret => true, "Stampede"),
                        //readiness,wait_for_rapid_fire=1
                        Spell.CastSelfSpell("Readiness", ret => Buff.PlayerHasActiveBuff("Rapid Fire"), "Readiness"),
                        //fervor,if=enabled&focus<=50
                        Buff.CastBuff("Fervor", ret => SpellManager.HasSpell("Feror") && Me.CurrentFocus <= 50, "Fervor"),
                        //cobra_shot,if=dot.serpent_sting.remains<6
                        Spell.CastSpell("Cobra Shot", ret => Buff.TargetDebuffTimeLeft("Serpent Sting").Seconds < 6, "Cobra Shot"),
                        //arcane_shot,if=focus>=67
                        Spell.CastSpell("Arcane Shot", ret => Me.CurrentFocus >= 67, "Arcane Shot"),
                        //cobra_shot
                        Spell.CastSpell("Cobra Shot", ret => true, "Cobra Shot")
                ));
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
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink") && !Buff.PlayerHasBuff("Feign Death"),
                        new PrioritySelector(
                            //flask,type=spring_blossoms
                            //food,type=sea_mist_rice_noodles
                            //hunters_mark,if=target.time_to_die>=21&!debuff.ranged_vulnerability.up
                            Buff.CastDebuff("Hunter's Mark", ret => Unit.TimeToDeath(Me.CurrentTarget) >= 21 && !TalentManager.HasGlyph("Marked for Death") || (CLU.LocationContext == GroupLogic.Battleground && Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 40 * 40), "Hunter's Mark"),
                            //summon_pet
                            Common.HunterCallPetBehavior(CLUSettings.Instance.Hunter.ReviveInCombat),
                            //trueshot_aura
                            //virmens_bite_potion
                            new Action(delegate
                            {
                                Macro.isTrapMacroInUse();
                                Macro.isMultiCastMacroInUse();
                                return RunStatus.Failure;
                            })
                )));
            }
        }

        public override Composite Resting
        {
            get {
                return Base.Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                    //new Action(a => { CLU.Log("I am the start of public override Composite PVPRotation"); return RunStatus.Failure; }),
                        CrowdControl.freeMe(),
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    Item.UseEngineerGloves(),
                                    new Action(delegate
                                    {
                                        Macro.isTrapMacroInUse();
                                        Macro.isMultiCastMacroInUse();
                                        return RunStatus.Failure;
                                    }),
                                    new Decorator(ret => Macro.Burst && !Buff.PlayerHasBuff("Feign Death"), burstRotation),
                                    new Decorator(ret => (!Macro.Burst || BotChecker.BotBaseInUse("BGBuddy")) && !Buff.PlayerHasBuff("Feign Death"), baseRotation)))
                )));
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