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

using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using Styx.CommonBot;
using Rest = CLU.Base.Rest;
using Styx;
using Styx.Pathing;
using Styx.WoWInternals;
using global::CLU.Managers;

namespace CLU.Classes.Warrior
{
    class Arms : RotationBase
    {
        private const int ItemSetId = 1073; // Tier set ID Colossal Dragonplate (Normal)

        public override string Name
        {
            get { return "Arms Warrior"; }
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
            get { return "Mortal Strike"; }
        }

        public override int KeySpellId
        {
            get { return 12294; }
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
                       "2pc Tier set Bonus?: " + Item.Has2PcTeirBonus(ItemSetId) + "\n" +
                       "4pc Tier set Bonus?: " + Item.Has4PcTeirBonus(ItemSetId) + "\n" +
                       "This Rotation will:\n" +
                       "1. Heal using Victory Rush, Enraged Regeneration\n" +
                       "==> Rallying Cry, Healthstone \n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "3. Stance Dance\n" +
                       "4. Best Suited for end game raiding\n" +
                       "NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'. \n" +
                       "Credits to gniegsch, lathrodectus and Obliv, alxaw \n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static bool IsMortalStrikeOnCooldown { get { return Spell.SpellCooldown("Mortal Strike").TotalSeconds > 1.5; } }
        private static bool HasColossusSmash { get { return Buff.PlayerBuffTimeLeft("Colossus Smash") > 5; } }
        private static bool HasColossusSmash2 { get { return Buff.PlayerBuffTimeLeft("Colossus Smash") > 2; } }
        private static bool IsColossusSmashOnCooldown { get { return Spell.SpellCooldown("Colossus Smash").TotalSeconds > 4.0; } }
        private static bool IsTasteForBloodOnCooldown { get { return Buff.PlayerBuffTimeLeft("Taste For Blood") <= 3; } }
        private static bool UseDeadlyCalm { get { return Buff.GetAuraStack(Me, "Taste for Blood", true) > 2; } }
        private static bool TasteForBloodStacks { get { return Buff.GetAuraStack(Me, "Taste for Blood", true) > 3; } }
        private static bool IsColossusSmashOnCoolDownHeroicStrike { get { return Spell.SpellCooldown("Colossus Smash").TotalSeconds > 0.3; } }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                                new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                                // For DS Encounters.
                                EncounterSpecific.ExtraActionButton(),

                                // Kill flying units.
                                Common.HandleFlyingUnits,

                                new Decorator(
                                    ret => CLUSettings.Instance.UseCooldowns && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Unit.UseCooldowns(),
                                        new PrioritySelector(
                                        Item.UseTrinkets(),
                                        Racials.UseRacials(),
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                        Item.UseEngineerGloves(),
                                        Spell.CastSelfSpell("Recklessness", ret => (HasColossusSmash || IsColossusSmashOnCooldown) && CLUSettings.Instance.Warrior.UseRecklessness, "Recklessness"),
                                        Spell.CastSelfSpell("Avatar", ret => SpellManager.HasSpell("Avatar") && (SpellManager.HasSpell("Avatar") && (((Spell.SpellCooldown("Recklessness").TotalSeconds >= 180 || Buff.PlayerHasBuff("Recklessness")) || (Me.CurrentTarget.HealthPercent >= 20 && Unit.TimeToDeath(Me.CurrentTarget) > 195) || (Me.CurrentTarget.HealthPercent < 20 && Item.Has4PcTeirBonus(ItemSetId))) || Unit.TimeToDeath(Me.CurrentTarget) <= 20)), "Avatar"),
                                        Spell.CastSelfSpell("Skull Banner", ret => !Me.HasAura(114206), "Skull Banner"))),
                    // Interupts
                                    Spell.CastInterupt("Pummel", ret => CLUSettings.Instance.Warrior.UsePummel, "Pummel"),
                                    Spell.CastInterupt("Spell Reflection", ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTarget == Me, "Spell Reflection"),
                    //Aoe 4+ Adds
                                    new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.CountEnnemiesInRange(Me.Location, 8f) > 3,
                                        new PrioritySelector(
                                    Spell.CastSelfSpell("Berserker Rage", ret => !Me.Auras.ContainsKey("Enrage") && Spell.SpellOnCooldown("Colossus Smash"), "Berserker Rage"),
                                    Spell.CastSpell("Thunder Clap", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Thunder Clap"),
                                    Spell.CastSpell("Dragon Roar", ret => SpellManager.HasSpell("Dragon Roar"), "Dragon Roar"),
                                    Spell.CastSpell("Bladestorm", ret => SpellManager.HasSpell("Bladestorm"), "Bladestorm"),
                                    Spell.CastSpell("Shockwave", ret => SpellManager.HasSpell("Shockwave"), "Shockwave"),
                                    Spell.CastSpell("Mortal Strike", ret => true, "Mortal Strike"),
                                    Spell.CastSpell("Colossus Smash", ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds <= 1.5, "Colossus Smash"),
                                    Spell.CastSpell("Overpower", ret => true, "Overpower"),
                                    Spell.CastSpell("Storm Bolt", ret => TalentManager.HasTalent(18), "Storm Bolt"),
                                    Spell.CastSpell("Whirlwind", ret => true, "Whirlwind"),
                                    Spell.CastSpell("Commanding Shout", ret => CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding && !WoWSpell.FromId(469).Cooldown && (Me.RagePercent < 60), "Commanding Shout for Rage"),
                                    Spell.CastSpell("Battle Shout", ret => CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle && !WoWSpell.FromId(6673).Cooldown && (Me.RagePercent < 60), "Battle Shout for Rage"))),
                    //Aoe 3 Adds
                                    new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.CountEnnemiesInRange(Me.Location, 8f) == 3,
                                        new PrioritySelector(
                                    Spell.CastSelfSpell("Berserker Rage", ret => !Me.Auras.ContainsKey("Enrage") && Spell.SpellOnCooldown("Colossus Smash"), "Berserker Rage"),
                                    Spell.CastSelfSpell("Deadly Calm", ret => Me.CurrentRage > 40, "Deadly Calm"),
                                    Spell.CastSpell("Thunder Clap", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Thunder Clap"),
                                    Buff.CastBuff("Sweeping Strikes", ret => true, "Sweeping Strikes"),
                                    Spell.CastSpell("Dragon Roar", ret => SpellManager.HasSpell("Dragon Roar"), "Dragon Roar"),
                                    Spell.CastSpell("Bladestorm", ret => SpellManager.HasSpell("Bladestorm"), "Bladestorm"),
                                    Spell.CastSpell("Shockwave", ret => SpellManager.HasSpell("Shockwave"), "Shockwave"),
                                    Spell.CastSpell("Mortal Strike", ret => true, "Mortal Strike"),
                                    Spell.CastSpell("Colossus Smash", ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds <= 1.5, "Colossus Smash"),
                                    Spell.CastSpell("Overpower", ret => true, "Overpower"),
                                    Spell.CastSpell("Storm Bolt", ret => TalentManager.HasTalent(18), "Storm Bolt"),
                                    Spell.CastSpell("Cleave", ret => true, "Cleave"),
                                    Spell.CastSpell("Commanding Shout", ret => CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding && !WoWSpell.FromId(469).Cooldown && (Me.RagePercent < 60), "Commanding Shout for Rage"),
                                    Spell.CastSpell("Battle Shout", ret => CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle && !WoWSpell.FromId(6673).Cooldown && (Me.RagePercent < 60), "Battle Shout for Rage"))),
                    //Single Target
                                    new Decorator(ret => !CLUSettings.Instance.UseAoEAbilities || Unit.CountEnnemiesInRange(Me.Location, 8f) < 3,
                                        new PrioritySelector(
                                    Spell.CastSelfSpell("Berserker Rage", ret => !Me.Auras.ContainsKey("Enrage") && Spell.SpellOnCooldown("Colossus Smash"), "Berserker Rage"),
                                    Spell.CastSelfSpell("Deadly Calm", ret => UseDeadlyCalm, "Deadly Calm"),
                                    Spell.CastSelfSpell("Bloodbath", ret => Me.CurrentTarget != null && (SpellManager.HasSpell("Bloodbath") && (((Spell.SpellCooldown("Recklessness").TotalSeconds >= 10 || Buff.PlayerHasBuff("Recklessness")) || (Me.CurrentTarget.HealthPercent >= 20 && (Unit.TimeToDeath(Me.CurrentTarget) <= 165 || (Unit.TimeToDeath(Me.CurrentTarget) <= 315 & !Item.Has4PcTeirBonus(ItemSetId))) && Unit.TimeToDeath(Me.CurrentTarget) > 75)) || Unit.TimeToDeath(Me.CurrentTarget) <= 19)), "Bloodbath"),
                                    Spell.CastSelfSpell("Sweeping Strikes", ret => Unit.CountEnnemiesInRange(Me.Location, 8f) == 2 && CLUSettings.Instance.UseAoEAbilities, "Sweeping Strikes"),
                                    Spell.CastSpell("Heroic Strike", ret => UseDeadlyCalm && Buff.TargetHasDebuff("Colossus Smash"), "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike", ret => Buff.GetAuraStack(Me, "Taste for Blood", true) > 1 && IsTasteForBloodOnCooldown && Me.CurrentTarget.HealthPercent >= 20, "Heroic Strike"),
                                    Spell.CastSpell("Slam", ret => Me.CurrentTarget != null && Me.CurrentRage > 110 && Me.CurrentTarget.HealthPercent >= 20, "Slam"),
                                    Spell.CastSpell("Mortal Strike", ret => true, "Mortal Strike"),
                                    Spell.CastSpell("Colossus Smash", ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds <= 1.5, "Colossus Smash"),
                                    Spell.CastSpell("Execute", ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 20, "Execute"),
                                    Spell.CastSpell("Heroic Strike", ret => TasteForBloodStacks && Spell.CanCast("Overpower") && Me.CurrentTarget.HealthPercent >= 20, "Heroic Strike"),
                                    Spell.CastSpell("Overpower", ret => true, "Overpower"),
                                    Spell.CastSpell("Dragon Roar", ret => CLUSettings.Instance.Warrior.UseDragonRoar && Me.CurrentTarget.IsWithinMeleeRange && SpellManager.HasSpell("Dragon Roar"), "Dragon Roar"),
                                    Spell.CastSpell("Storm Bolt", ret => SpellManager.HasSpell("Storm Bolt"), "Storm Bolt"),
                                    Spell.CastSpell("Slam", ret => Me.CurrentTarget != null && Me.CurrentRage > 60 && Buff.TargetHasDebuff("Colossus Smash") && Me.CurrentTarget.HealthPercent > 20, "Slam"),
                                    Spell.CastSpell("Slam", ret => !Spell.CanCast("Overpower") && Me.CurrentTarget.HealthPercent >= 20, "Slam"),
                                    Spell.CastSpell("Heroic Throw", ret => Me.CurrentTarget.HealthPercent >= 20, "Heroic Throw"),
                                    Spell.CastSpell("Commanding Shout", ret => CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding && !WoWSpell.FromId(469).Cooldown && (Me.RagePercent < 60), "Commanding Shout for Rage"),
                                    Spell.CastSpell("Battle Shout", ret => CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle && !WoWSpell.FromId(6673).Cooldown && (Me.RagePercent < 60), "Battle Shout for Rage"))));
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
                        Buff.CastBuff("Battle Stance", ret => !Macro.weaponSwap && Me.Shapeshift != ShapeshiftForm.BattleStance, "Battle Stance"),
                        Buff.CastBuff("Defensive Stance", ret => Macro.weaponSwap && Me.Shapeshift != ShapeshiftForm.DefensiveStance, "Defensive Stance"),
                        Spell.CastSpell("Charge", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 25 * 25 && Navigator.CanNavigateFully(Me.Location, Me.CurrentTarget.Location), "Charge"),
                    //Spell.CastOnUnitLocation("Heroic Leap",     ret => Me.CurrentTarget != null && && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 40 * 40 &&  SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                        Spell.CastSpell("Hamstring", ret => !Buff.TargetHasDebuff("Hamstring"), "Hamstring"),

                        //Rotation
                    //mogu_power_potion,if=(target.health.pct<20&buff.recklessness.up)|buff.bloodlust.react|target.time_to_die<=25
                        Spell.CastSelfSpell("Recklessness", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && ((Buff.TargetDebuffTimeLeft("Colossus Smash").Seconds >= 5 || SpellManager.Spells["Colossus Smash"].CooldownTimeLeft.Seconds <= 4) && (!TalentManager.HasTalent(16) && ((Me.CurrentTarget.HealthPercent < 20 || Unit.TimeToDeath(Me.CurrentTarget) > 315 || Unit.TimeToDeath(Me.CurrentTarget) > 165)) || (TalentManager.HasTalent(16) && Buff.PlayerHasActiveBuff("Avatar")))) || Unit.TimeToDeath(Me.CurrentTarget) <= 18, "Recklessness"),
                        Spell.CastSelfSpell("Avatar", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && TalentManager.HasTalent(16) && (((SpellManager.Spells["Recklessness"].CooldownTimeLeft.Seconds >= 180 || Buff.PlayerHasActiveBuff("Recklessness")) || (Me.CurrentTarget.HealthPercent >= 20 && Unit.TimeToDeath(Me.CurrentTarget) > 195) || Me.CurrentTarget.HealthPercent < 20) || Unit.TimeToDeath(Me.CurrentTarget) <= 20), "Avatar"),
                        Spell.CastSelfSpell("Bloodbath", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && TalentManager.HasTalent(17) && (((SpellManager.Spells["Recklessness"].CooldownTimeLeft.Seconds >= 10 || Buff.PlayerHasActiveBuff("Recklessness")) || (Me.CurrentTarget.HealthPercent >= 20 && (Unit.TimeToDeath(Me.CurrentTarget) <= 165 || Unit.TimeToDeath(Me.CurrentTarget) <= 315) && Unit.TimeToDeath(Me.CurrentTarget) > 75)) || Unit.TimeToDeath(Me.CurrentTarget) <= 19), "Bloodbath"),
                        Spell.CastSelfSpell("Berserker Rage", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && !Buff.PlayerHasActiveBuff("Enrage"), "Berserker Rage"),
                        Spell.CastOnUnitLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Buff.TargetHasDebuff("Colossus Smash"), "Heroic Leap"),
                        Spell.CastSelfSpell("Deadly Calm", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentRage >= 40, "Deadly Calm"),
                        Spell.CastSpell("Heroic Strike", ret => Me.CurrentTarget != null && ((Buff.PlayerHasActiveBuff("Taste for Blood") && Buff.PlayerActiveBuffTimeLeft("Taste for Blood").Seconds <= 2) || (Buff.PlayerCountBuff("Taste for Blood") == 5 && Spell.CanCast("Overpower")) || (Buff.PlayerHasActiveBuff("Taste for Blood") && Buff.TargetDebuffTimeLeft("Colossus Smash").Seconds <= 2 && SpellManager.Spells["Colossus Smash"].CooldownTimeLeft.Seconds != 0) || Buff.PlayerHasActiveBuff("Deadly Calm") || Me.CurrentRage > 110) && Me.CurrentTarget.HealthPercent >= 20 && Buff.TargetHasDebuff("Colossus Smash"), "Heroic Strike"),
                        Spell.CastSpell("Mortal Strike", ret => true, "Mortal Strike"),
                        Spell.CastSpell("Colossus Smash", ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds <= 1.5, "Colossus Smash"),
                        Spell.CastSpell("Execute", ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 20, "Execute"),
                        Spell.CastSpell("Storm Bolt", ret => TalentManager.HasTalent(18), "Storm Bolt"),
                        Spell.CastSpell("Overpower", ret => true, "Overpower"),
                        Spell.CastSpell("Shockwave", ret => TalentManager.HasTalent(11), "Shockwave"),
                    //Needs fixing. Spell.CastSpell("Dragon Roar", ret => CLUSettings.Instance.Warrior.UseDragonRoar && Me.CurrentTarget.IsWithinMeleeRange && TalentManager.HasTalent(12), "Dragon Roar"),
                    //Spell.CastSpell("Slam",                     ret => Me.CurrentTarget != null && (Me.CurrentRage >= 70 || Buff.TargetHasDebuff("Colossus Smash")) && Me.CurrentTarget.HealthPercent >= 20, "Slam"),
                        Spell.CastSpell("Heroic Throw", ret => true, "Heroic Throw"),
                        Buff.CastBuff("Battle Shout", ret => Me.CurrentRage < 70 && !Buff.TargetHasDebuff("Colossus Smash"), "Battle Shout"),
                        Spell.CastSpell("Bladestorm", ret => Me.CurrentTarget != null && TalentManager.HasTalent(10) && SpellManager.Spells["Colossus Smash"].CooldownTimeLeft.Seconds >= 5 && !Buff.TargetHasDebuff("Colossus Smash") && SpellManager.Spells["Bloodthirst"].CooldownTimeLeft.Seconds >= 2 && Me.CurrentTarget.HealthPercent >= 20, "Bladestorm"),//<~ add GUI option for user descretion
                    //Spell.CastSpell("Slam",                     ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent >= 20, "Slam"),
                        Buff.CastBuff("Battle Shout", ret => Me.CurrentRage < 70, "Battle Shout")
                ));
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => CLUSettings.Instance.EnableMovement && CLU.LocationContext != GroupLogic.Battleground,
                        new PrioritySelector(
                            new Decorator(ret => !Me.IsSafelyFacing(Me.CurrentTarget.Location), new Action(a => Me.CurrentTarget.Face())),
                            Spell.CastSpell("Charge", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && Navigator.CanNavigateFully(Me.Location, Me.CurrentTarget.Location), "Charge"),
                           Spell.CastOnUnitLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null &&
                                Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 &&
                                SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 &&
                                SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                            this.SingleRotation)),
                    this.SingleRotation
                    );
            }
        }

        public override Composite Medic
        {
            get
            {
                return (
                    new Decorator(ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                        new PrioritySelector(
                            Spell.CastSpell("Victory Rush", ret => Buff.PlayerHasActiveBuff("Victorious") && Me.HealthPercent < CLUSettings.Instance.Warrior.ImpendingVictoryPercent, "Victory Rush or Impending Victory"),
                            Spell.CastSelfSpell("Enraged Regeneration", ret => Me.HealthPercent < 45 && !Buff.PlayerHasBuff("Rallying Cry"), "Enraged Regeneration"),
                            Spell.CastSelfSpell("Rallying Cry", ret => Me.HealthPercent < 45 && !Buff.PlayerHasBuff("Enraged Regeneration"), "Rallying Cry"),
                            Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Healthstone")
                )));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                    //lask,type=winters_bite
                    //food,type=black_pepper_ribs_and_shrimp
                            Buff.CastBuff("Berserker Stance", ret => StyxWoW.Me.Shapeshift != CLUSettings.Instance.Warrior.StanceSelection && CLUSettings.Instance.Warrior.StanceSelection == ShapeshiftForm.BerserkerStance, "Stance is Berserker"),
                            Buff.CastBuff("Battle Stance", ret => StyxWoW.Me.Shapeshift != CLUSettings.Instance.Warrior.StanceSelection && CLUSettings.Instance.Warrior.StanceSelection == ShapeshiftForm.BattleStance, "Stance is Battle"),
                    //mogu_power_potion
                            new Decorator(ret => Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapDefensive),
                            new Decorator(ret => !Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapOffensive),
                            Buff.CastRaidBuff("Battle Shout", ret => true, "Battle Shout"),
                            Buff.CastRaidBuff("Commanding Shout", ret => true, "Commanding Shout"),
                            Spell.CastSpell("Charge", ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 25 * 25, "Charge"),
                            Spell.CastOnUnitLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 40 * 40 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                            new Action(delegate
                            {
                                Macro.isMultiCastMacroInUse();
                                return RunStatus.Failure;
                            })
                )));
            }
        }

        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
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
                                    new Decorator(ret => Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapDefensive),
                                    new Decorator(ret => !Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapOffensive),
                                    Item.UseTrinkets(),
                                    Racials.UseRacials(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    Item.UseEngineerGloves(),
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
            get { return this.SingleRotation; }
        }

        #region Weapon swap stuff

        public Composite wepSwapDefensive
        {
            get
            {
                return (
                    new Decorator(ret => Me.Inventory.Equipped.OffHand == null && !string.IsNullOrEmpty(CLUSettings.Instance.Warrior.PvPMainHandItemName) && !string.IsNullOrEmpty(CLUSettings.Instance.Warrior.PvPOffHandItemName) && CLUSettings.Instance.Warrior.PvPMainHandItemName != "Input the name of your Main-Hand weapon here" && CLUSettings.Instance.Warrior.PvPOffHandItemName != "Input the name of your Off-Hand weapon here",
                        new Action(delegate
                        {
                            CLULogger.Log("Switching to Defensive Mode. Using MainHand: [{0}] Using OffHand: [{1}]", CLUSettings.Instance.Warrior.PvPMainHandItemName, CLUSettings.Instance.Warrior.PvPOffHandItemName);
                            Lua.DoString("RunMacroText(\"/equipslot 16 " + CLUSettings.Instance.Warrior.PvPMainHandItemName + "\")");
                            Lua.DoString("RunMacroText(\"/equipslot 17 " + CLUSettings.Instance.Warrior.PvPOffHandItemName + "\")");
                            return RunStatus.Failure;
                        })
                ));
            }
        }

        public Composite wepSwapOffensive
        {
            get
            {
                return (
                    new Decorator(ret => Me.Inventory.Equipped.OffHand != null && !string.IsNullOrEmpty(CLUSettings.Instance.Warrior.PvPTwoHandItemName) && CLUSettings.Instance.Warrior.PvPTwoHandItemName != "Input the name of your Two-Hand weapon here",
                        new Action(delegate
                        {
                            CLULogger.Log("Switching to Offensive Mode. Using TwoHand: [{0}] ", CLUSettings.Instance.Warrior.PvPTwoHandItemName);
                            Lua.DoString("RunMacroText(\"/equipslot 16 " + CLUSettings.Instance.Warrior.PvPTwoHandItemName + "\")");
                            return RunStatus.Failure;
                        })
                ));
            }
        }
        #endregion
    }
}