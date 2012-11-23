#region Revision info
/*
 * $Author: clutwopointzero@gmail.com $
 * $Date: 2012-09-24 07:30:05 +0200 (Mon, 24 Sep 2012) $
 * $ID$
 * $Revision: 466 $
 * $URL: https://clu-for-honorbuddy.googlecode.com/svn/trunk/CLU/CLU/Classes/Shaman/Elemental.cs $
 * $LastChangedBy: clutwopointzero@gmail.com $
 * $ChangesMade$
 */
#endregion

using CLU.Helpers;
using Styx.TreeSharp;
using System.Linq;
using CommonBehaviors.Actions;
using CLU.Lists;
using Styx.WoWInternals.WoWObjects;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;
using Styx.WoWInternals;
using Styx.CommonBot;
using global::CLU.Managers;

namespace CLU.Classes.Shaman
{

    class Elemental : RotationBase
    {
        public override string Name
        {
            get
            {
                return "Elemental Shaman";
            }
        }

        public override string Revision
        {
            get
            {
                return "$Rev: 466 $";
            }
        }

        public override string KeySpell
        {
            get
            {
                return "Thunderstorm";
            }
        }

        public override int KeySpellId
        {
            get { return 51490; }
        }

        public override float CombatMaxDistance
        {
            get
            {
                return 34f;
            }
        }

        public override float CombatMinDistance
        {
            get
            {
                return 28f;
            }
        }

        public override string Help
        {
            get
            {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Enchant Weapon: Flametongue Weapon(MainHand)\n" +
                       "2. Totem Bar: Stoneskin Totem, Wrath of Air, Healing Stream Totem, Searing (with range check) \n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Elemental Mastery \n" +
                       "==> Fire Elemental Totem \n" +
                       "==> Earth Elemental Totem \n" +
                       "4. AoE with Magma Totem, Chain Lightning, Earthquake\n" +
                       "4. Heal using: Lightning Shield, Shamanistic Rage, Healing Surge\n" +
                       "6. Best Suited for end game raiding\n" +
                       "NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'. \n" +
                       "Credits to Digitalmocking for his Initial Version and Stormchasing\n" +
                       "Updated for MoP by alxaw\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

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
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                    // PvP TRICK.
                    // Spell.CastSpell("Purge",              ret => Spell.TargetHasPurgableBuff(), "Purge"), // this is working but it needs a hashset to prioritise what to purge as we dont want to purge blessings and shit, its on my to-do
                    // Interupts
                    // Spell.CastInterupt("Wind Shear",                    ret => true, "Wind Shear"),
                    // Threat
                    //  Buff.CastBuff("Wind Shear",                         ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Wind Shear (Threat)"),
                    // Totem management
                           Totems.CreateTotemsBehavior(),
                    // AoE
                           Spell.CastTotem("Magma Totem", ret => Unit.EnemyRangedUnits.Count(u => u.DistanceSqr <= 12 * 12) >= 3 && !Totems.Exist(WoWTotem.FireElemental), "Magma Totem"),
                           Spell.CastAreaSpell("Chain Lightning", 15, false, 2, 0.0, 12.0, a => true, "Chain Lightning"),
                           Spell.CastAreaSpell("Thunderstorm", 8, false, 6, 0.0, 0.0, a => true, "Thunderstorm"),
                           Spell.CastAreaSpell("Earthquake", 8, false, 6, 0.0, 0.0, a => true, "Earthquake"),
                           Item.RunMacroText("/Cast Lava Beam", ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 3 && Buff.PlayerHasActiveBuff("Ascendance"), "Pew Pew LaserBEEEAM"),

                  //Default Rotation
                           Spell.WaitForCast(),
                           Spell.CastSpell("Unleash Elements", ret => TalentManager.HasTalent(16) && !Buff.PlayerHasActiveBuff("Ascendance"), "Unleash Elements"),
                           Spell.CastSpell("Flame Shock", ret => !Buff.TargetHasDebuff("Flame Shock") || Buff.TargetDebuffTimeLeft("Flame Shock").Seconds < 3, "Flame Shock"),
                           Spell.CastSpell("Lava Burst", ret => Buff.TargetDebuffTimeLeft("Flame Shock").TotalSeconds > 1.25 && (Buff.PlayerHasActiveBuff("Ascendance") || Spell.CanCast("Lava Burst")), "Lava Burst"),
                           Spell.CastSpell("Elemental Blast",ret => TalentManager.HasTalent(19) && !Buff.PlayerHasActiveBuff("Ascendance"),"Elemental Blast"),
                           Spell.CastSpell("Earth Shock", ret => Buff.PlayerCountBuff("Lightning Shield") == 7, "Earth Shock"),
                           Spell.CastSpell("Earth Shock", ret => Buff.PlayerCountBuff("Lightning Shield") > 3 && Buff.TargetDebuffTimeLeft("Flame Shock").Seconds > 5 && Buff.TargetDebuffTimeLeft("Flame Shock").TotalSeconds < 5 + 2.50, "Earth Shock"),
                           Spell.CastSpell("Lightning Bolt", ret => true, "Lightning Bolt"),
                           Spell.CastSpell("Fire Elemental Totem", ret => Unit.UseCooldowns(), "Fire Elemental Totem"),


                  //Cooldowns
                           Buff.CastBuff("Ascendance", ret => CLUSettings.Instance.Shaman.AscendanceSelection == Ascendance.OnBoss && Unit.UseCooldowns(), "Ascendance"),
                           Buff.CastBuff("Ascendance", ret => CLUSettings.Instance.Shaman.AscendanceSelection == Ascendance.OnCooldown, "Ascendance"),
                           Spell.CastSelfSpell("Elemental Mastery", ret => CLUSettings.Instance.Shaman.ElementalMasterySelection == ElementalMastery.OnBoss && Unit.UseCooldowns() && TalentManager.HasTalent(10), "Elemental Mastery"),
                           Spell.CastSelfSpell("Elemental Mastery", ret => CLUSettings.Instance.Shaman.ElementalMasterySelection == ElementalMastery.OnCooldown && TalentManager.HasTalent(10), "Elemental Mastery"),
                           Spell.CastSpell("Stormlash Totem", ret => CLUSettings.Instance.Shaman.UseStormlashTotem != StormlashTotem.Never
                                        && ((CLUSettings.Instance.Shaman.UseStormlashTotem == StormlashTotem.OnHaste && Me.HasAnyAura(Me.IsHorde ? "Bloodlust" : "Heroism", "Timewarp", "Ancient Hysteria")
                                        || CLUSettings.Instance.Shaman.UseStormlashTotem == StormlashTotem.OnCooldown)
                                        && !Totems.Exist(WoWTotemType.Air)), "Stormlash Totem")
                       );
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
                    //new Action(a => { SysLog.Log("I am the start of public Composite baseRotation"); return RunStatus.Failure; }),
                    //PvP Utilities

                        //Rotation
                    //6	0.00	wind_shear
                    //7	0.01	bloodlust,if=target.health.pct<25|time>5
                    //jade_serpent_potion,if=time>60&(pet.primal_fire_elemental.active|pet.greater_fire_elemental.active|target.time_to_die<=60)
                    //use_item,name=firebirds_gloves,if=((cooldown.ascendance.remains>10|level<87)&cooldown.fire_elemental_totem.remains>10)|buff.ascendance.up|buff.bloodlust.up|totem.fire_elemental_totem.active
                        Item.UseEngineerGloves(),
                    //blood_fury,if=buff.bloodlust.up|buff.ascendance.up|((cooldown.ascendance.remains>10|level<87)&cooldown.fire_elemental_totem.remains>10)
                        Racials.UseRacials(),
                    //elemental_mastery,if=talent.elemental_mastery.enabled&time>15&((!buff.bloodlust.up&time<120)|(!buff.berserking.up&!buff.bloodlust.up&buff.ascendance.up)|(time>=200&(cooldown.ascendance.remains>30|level<87)))
                        Spell.CastSelfSpell("Elemental Mastery", ret => TalentManager.HasTalent(10) && (!Buff.UnitHasHasteBuff(Me) || (!Buff.PlayerHasActiveBuff("Berserking") && !Buff.UnitHasHasteBuff(Me) && Buff.PlayerHasActiveBuff("Ascendance")) || (SpellManager.Spells["Ascendance"].CooldownTimeLeft.Seconds > 30 || Me.CurrentTarget.Level < 87)), "Elemental Mastery"),
                    //fire_elemental_totem,if=!active
                        Spell.CastSpell("Fire Elemental Totem", ret => true, "Fire Elemental Totem"),
                    //ascendance,if=dot.flame_shock.remains>buff.ascendance.duration&(target.time_to_die<20|buff.bloodlust.up|time>=180)
                        Spell.CastSelfSpell("Ascendance", ret => Buff.TargetDebuffTimeLeft("Flame shock").Seconds > 15 && Unit.TimeToDeath(Me.CurrentTarget) < 20, "Ascendance"),
                    //ancestral_swiftness,if=talent.ancestral_swiftness.enabled&!buff.ascendance.up
                        Spell.CastSelfSpell("Ancestral Swiftness", ret => TalentManager.HasTalent(11) && !Buff.PlayerHasActiveBuff("Ascendance"), "Ancestral Swiftness"),
                    //unleash_elements,if=talent.unleashed_fury.enabled&!buff.ascendance.up
                        Spell.CastSpell("Unleash Elements", ret => TalentManager.HasTalent(16) && !Buff.PlayerHasActiveBuff("Ascendance"), "Unleash Elements"),
                    //flame_shock,if=!buff.ascendance.up&(!ticking|ticks_remain<2|((buff.bloodlust.up|buff.elemental_mastery.up)&ticks_remain<3))
                        Spell.CastSpell("Flame Shock", ret => !Buff.PlayerHasActiveBuff("Ascendance") && (!Buff.TargetHasDebuff("Flame Shock") || Buff.TargetDebuffTimeLeft("Flame Shock").Seconds < 5 || ((Buff.UnitHasHasteBuff(Me) || Buff.PlayerHasActiveBuff("Elemental Mastery")) && Buff.TargetDebuffTimeLeft("Flame Shock").TotalSeconds < 7.50)), "Flame Shock"),
                    //lava_burst,if=dot.flame_shock.remains>cast_time&(buff.ascendance.up|cooldown_react)
                        Spell.CastSpell("Lava Burst", ret => Buff.TargetDebuffTimeLeft("Flame Shock").TotalSeconds > 1.25 && (Buff.PlayerHasActiveBuff("Ascendance") || Spell.CanCast("Lava Burst")), "Lava Burst"),
                    //elemental_blast,if=talent.elemental_blast.enabled&!buff.ascendance.up
                        Spell.CastSpell("Elemental Blast", ret => TalentManager.HasTalent(18) && !Buff.PlayerHasActiveBuff("Ascendance"), "Elemental Blast"),
                    //earth_shock,if=buff.lightning_shield.react=buff.lightning_shield.max_stack
                        Spell.CastSpell("Earth Shock", ret => Buff.PlayerCountBuff("Lightning Shield") == 7, "Earth Shock"),
                    //earth_shock,if=buff.lightning_shield.react>3&dot.flame_shock.remains>cooldown&dot.flame_shock.remains<cooldown+action.flame_shock.tick_time
                        Spell.CastSpell("Earth Shock", ret => Buff.PlayerCountBuff("Lightning Shield") > 3 && Buff.TargetDebuffTimeLeft("Flame Shock").Seconds > 5 && Buff.TargetDebuffTimeLeft("Flame Shock").TotalSeconds < 5 + 2.50, "Earth Shock"),
                    //earth_elemental_totem,if=!active&cooldown.fire_elemental_totem.remains>=50
                        Spell.CastSpell("Earth Elemental Totem", ret => SpellManager.Spells["Fire Elemental Totem"].CooldownTimeLeft.Seconds >= 50, "Earth Elemental Totem"),
                    //searing_totem,if=!totem.fire.active
                        Spell.CastSpell("Searing Totem", ret => !Totems.Exist(WoWTotemType.Fire), "Searing Totem"),
                    //spiritwalkers_grace,moving=1
                        Spell.CastSelfSpell("Spiritwalker's Grace", ret => Me.IsMoving, "Spiritwalker's Grace"),
                    //unleash_elements,moving=1
                        Spell.CastSpell("Unleash Elements", ret => Me.IsMoving, "Unleash Elements"),
                    //lightning_bolt
                        Spell.CastSpell("Lightning Bolt", ret => true, "Lightning Bolt")
                ));
            }
        }

        public override Composite Pull
        {
             get { return new PrioritySelector(
                Movement.CreateFaceTargetBehavior(),
                this.SingleRotation);   }
        }

        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                    Common.HandleCompulsoryShamanBuffs(),
                    Common.HandleTotemRecall(),
                    // Healing shit.
                    new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Shamanistic Rage", ret => Me.CurrentTarget != null && (Me.HealthPercent < 60 || (Me.ManaPercent < 65 && Me.CurrentTarget.HealthPercent >= 75)), "Shamanistic Rage"),
                               Spell.CastSpell("Healing Surge", ret => Me, ret => Me.HealthPercent < 75 && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "Healing Surge"))));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                    //flask,type=warm_sun
                    //food,type=mogu_fish_stew
                            Common.HandleCompulsoryShamanBuffs(),
                    //jade_serpent_potion
                            Common.HandleTotemRecall()
                )));
            }
        }

        public override Composite Resting
        {
            get
            {
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                    //new Action(a => { SysLog.Log("I am the start of public override Composite PVPRotation"); return RunStatus.Failure; }),
                        CrowdControl.freeMe(),
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    new Action(delegate
                                    {
                                        Macro.isMultiCastMacroInUse();
                                        return RunStatus.Failure;
                                    }),
                                    new Decorator(ret => Macro.Burst, burstRotation),
                                    new Decorator(ret => !Macro.Burst || BotChecker.BotBaseInUse("BGBuddy"), baseRotation)))
                )));
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return this.SingleRotation;
            }
        }
    }
}
