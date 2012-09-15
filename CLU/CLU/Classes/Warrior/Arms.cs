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
        private static bool IsTasteForBloodOnCooldown { get { return Buff.PlayerBuffTimeLeft("Taste For Blood") > 2; } }
        private static bool TasteForBloodStacks { get { return Buff.PlayerCountBuff("Taste For Blood") > 5; } }
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
                                    ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                        new PrioritySelector(
                                        Item.UseTrinkets(),
                                        Spell.UseRacials(),
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                        Item.UseEngineerGloves())),
                                    // Interupts
                                    Spell.CastInterupt("Pummel", ret => CLUSettings.Instance.Warrior.UsePummel, "Pummel"),
                                    Spell.CastInterupt("Spell Reflection", ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTarget == Me, "Spell Reflection"),

                                    // Start of Actions - SimCraft as of 3/31/2012
                                    // TODO: GCD Check and Tier pce check needs attention. Spell.GCD = 0
                                    Spell.CastSelfSpell("Berserker Rage",   ret => Me.CurrentRage > 30 || !Me.Auras.ContainsKey("Enrage") || Buff.PlayerHasBuff("Recklessness"), "Berserker Rage"), //Thanks to swordfish for !Me.Auras.ContainsKey tip.
                                    Spell.CastSelfSpell("Deadly Calm",      ret => Me.CurrentRage > 40 || Buff.PlayerCountBuff("Taste For Blood") > 3, "Deadly Calm"),
                                    Spell.CastSelfSpell("Recklessness",     ret => Me.CurrentTarget != null && ((HasColossusSmash || IsColossusSmashOnCooldown) && Unit.IsTargetWorthy(Me.CurrentTarget)), "Recklessness"),
                                    Spell.CastAreaSpell("Sweeping Strikes", 5, false, 2, 0.0, 0.0, a => true, "Sweeping Strikes"),
                                    Spell.CastAreaSpell("Thunder Clap", 5, false, 3, 0.0, 0.0, a => true, "Thunder Clap"),
                                    Spell.CastAreaSpell("Cleave", 5, false, 3, 0.0, 0.0, a => Me.CurrentRage > 40, "Cleave"),
                                    Spell.CastSpell("Dragon Roar",          ret => true, "Dragon Roar"),
                                    Spell.CastAreaSpell("Whirlwind", 5, false, 3, 0.0, 0.0, a => Me.CurrentRage > 50, "Whirlwind"),
                                    Spell.CastAreaSpell("Bladestorm", 5, false, 4, 0.0, 0.0, a => true, "Bladestorm"),
                                    Spell.CastAreaSpell("Shockwave", 5, false, 1, 0.0, 0.0, a => true, "Shockwave"),
                                    Spell.CastSpell("Heroic Strike",        ret => Buff.PlayerHasBuff("Deadly Calm") || (Me.CurrentRage > 95 && Me.CurrentTarget.HealthPercent > 20 && Buff.TargetHasDebuff("Colosuss Smash")), "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike",        ret => Buff.PlayerHasBuff("Taste For Blood") && IsColossusSmashOnCoolDownHeroicStrike && HasColossusSmash2, "Heroic Strike"),
                                    Spell.CastSpell("Mortal Strike",        ret => true, "Mortal Strike"),
                                    Spell.CastSpell("Colossus Smash",       ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds <= 1.5 || IsMortalStrikeOnCooldown, "Colossus Smash"),
                                    Spell.CastSpell("Execute",              ret => Me.CurrentTarget.HealthPercent > 20, "Execute"),
                                    Spell.CastSpell("Heroic Strike",        ret => TasteForBloodStacks && SpellManager.CanCast("Overpower"), "Heroic Strike"),
                                    Spell.CastSpell("Heroic Throw",         ret => true, "Heroic Throw"),
                                    Spell.CastSpell("Heroic Strike",        ret => Buff.PlayerHasBuff("Taste for Blood") && IsTasteForBloodOnCooldown, "Heroic Strike"),
                                    Spell.CastSpell("Overpower",            ret => true, "Overpower"),
                                    Spell.CastSpell("Slam",                 ret => Me.CurrentRage > 70 && Buff.TargetHasDebuff("Colossus Smash") && Me.CurrentTarget.HealthPercent > 20, "Slam"),
                                    Spell.CastSpell("Slam",                 ret => Me.CurrentTarget.HealthPercent > 20, "Slam"),
                                    Spell.CastSpell("Commanding Shout",     ret => Me.RagePercent < 60 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding, "Commanding Shout for Rage"),
                                    Spell.CastSpell("Battle Shout",         ret => Me.RagePercent < 60 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle, "Battle Shout for Rage"));
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
                        //charge,if=!currenttarget.iswithinmeleerenage
                        Spell.CastSpell("Charge", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2, "Charge"),
                        //heroic_leap,if=!currenttarget.iswithinmeleerange&spell.charge.down
                        Spell.CastSpellAtLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 &&
                            SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                        //mogu_power_potion,if=(target.health.pct<20&buff.recklessness.up)|buff.bloodlust.react|target.time_to_die<=25
                        //recklessness,use_off_gcd=1,if=((debuff.colossus_smash.remains>=5|cooldown.colossus_smash.remains<=4)&((!talent.avatar.enabled|!set_bonus.tier14_4pc_melee)&((target.health.pct<20|target.time_to_die>315|(target.time_to_die>165&set_bonus.tier14_4pc_melee)))|(talent.avatar.enabled&set_bonus.tier14_4pc_melee&buff.avatar.up)))|target.time_to_die<=18
                        //avatar,use_off_gcd=1,if=talent.avatar.enabled&(((cooldown.recklessness.remains>=180|buff.recklessness.up)|(target.health.pct>=20&target.time_to_die>195)|(target.health.pct<20&set_bonus.tier14_4pc_melee))|target.time_to_die<=20)
                        //bloodbath,use_off_gcd=1,if=talent.bloodbath.enabled&(((cooldown.recklessness.remains>=10|buff.recklessness.up)|(target.health.pct>=20&(target.time_to_die<=165|(target.time_to_die<=315&!set_bonus.tier14_4pc_melee))&target.time_to_die>75))|target.time_to_die<=19)
                        //berserker_rage,use_off_gcd=1,if=!buff.enrage.up
                        Spell.CastSelfSpell("Berserker Rage", ret => Me.CurrentTarget.IsWithinMeleeRange && !Buff.PlayerHasActiveBuff("Enrage"), "Berserker Rage"),
                        //heroic_leap,use_off_gcd=1,if=debuff.colossus_smash.up
                        Spell.CastSpellAtLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Buff.TargetHasDebuff("Colossus Smash"), "Heroic Leap"),
                        //deadly_calm,use_off_gcd=1,if=rage>=40
                        Spell.CastSelfSpell("Deadly Calm", ret => Me.CurrentRage >= 40, "Deadly Calm"),
                        //heroic_strike,use_off_gcd=1,if=((buff.taste_for_blood.up&buff.taste_for_blood.remains<=2)|(buff.taste_for_blood.stack=5&buff.overpower.up)|(buff.taste_for_blood.up&debuff.colossus_smash.remains<=2&!cooldown.colossus_smash.remains=0)|buff.deadly_calm.up|rage>110)&target.health.pct>=20&debuff.colossus_smash.up
                        Spell.CastSpell("Heroic Strike", ret => ((Buff.PlayerHasActiveBuff("Taste for Blood") && Buff.PlayerActiveBuffTimeLeft("Taste for Blood").Seconds <= 2) ||
                            (Buff.PlayerCountBuff("Taste for Blood") == 5 && SpellManager.CanCast("Overpower")) || (Buff.PlayerHasActiveBuff("Taste for Blood") &&
                            Buff.TargetDebuffTimeLeft("Colossus Smash").Seconds <= 2 && SpellManager.Spells["Colossus Smash"].CooldownTimeLeft.Seconds != 0) || Buff.PlayerHasActiveBuff("Deadly Calm") ||
                            Me.CurrentRage > 110) && Me.CurrentTarget.HealthPercent >= 20 && Buff.TargetHasDebuff("Colossus Smash"), "Heroic Strike"),
                        //mortal_strike
                        Spell.CastSpell("Mortal Strike", ret => true, "Mortal Strike"),
                        //colossus_smash,if=debuff.colossus_smash.remains<=1.5
                        Spell.CastSpell("Colossus Smash", ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds <= 1.5, "Colossus Smash"),
                        //execute
                        Spell.CastSpell("Execute", ret => true, "Execute"),
                        //storm_bolt,if=talent.storm_bolt.enabled
                        Spell.CastSpell("Storm Bolt", ret => SpellManager.HasSpell("Storm Bolt"), "Storm Bolt"),
                        //overpower,if=buff.overpower.up
                        Spell.CastSpell("Overpower", ret => true, "Overpwoer"),
                        //shockwave,if=talent.shockwave.enabled
                        Spell.CastSpell("Shockwave", ret => SpellManager.HasSpell("Shockwave"), "Shockwave"),
                        //dragon_roar,if=talent.dragon_roar.enabled
                        Spell.CastSpell("Dragon Roar", ret => SpellManager.HasSpell("Dragon Roar"), "Dragon Roar"),
                        //slam,if=(rage>=70|debuff.colossus_smash.up)&target.health.pct>=20
                        Spell.CastSpell("Slam", ret => (Me.CurrentRage >= 70 || Buff.TargetHasDebuff("Colossus Smash")) && Me.CurrentTarget.HealthPercent >= 20, "Slam"),
                        //heroic_throw
                        Spell.CastSpell("Heroic Throw", ret => true, "Heroic Throw"),
                        //battle_shout,if=rage<70&!debuff.colossus_smash.up
                        Buff.CastBuff("Battle Shout", ret => Me.CurrentRage < 70 && !Buff.TargetHasDebuff("Colossus Smash"), "Battle Shout"),
                        //bladestorm,if=talent.bladestorm.enabled&cooldown.colossus_smash.remains>=5&!debuff.colossus_smash.up&cooldown.bloodthirst.remains>=2&target.health.pct>=20
                        Spell.CastSpell("Bladestorm", ret => SpellManager.HasSpell("Bladestorm") && SpellManager.Spells["Colossus Smash"].CooldownTimeLeft.Seconds >= 5 && !Buff.TargetHasDebuff("Colossus Smash")
                            && SpellManager.Spells["Bloodthirst"].CooldownTimeLeft.Seconds >= 2 && Me.CurrentTarget.HealthPercent >= 20, "Bladestorm"),
                        //slam,if=target.health.pct>=20
                        Spell.CastSpell("Slam", ret => Me.CurrentTarget.HealthPercent >= 20, "Slam"),
                        //impending_victory,if=talent.impending_victory.enabled&target.health.pct>=20
                        Spell.CastSpell("Impending Victory", ret => SpellManager.HasSpell("Impending Victory") && Me.CurrentTarget.HealthPercent >= 20, "Impending Victory"),
                        //battle_shout,if=rage<70
                        Buff.CastBuff("Battle Shout", ret => Me.CurrentRage < 70, "Battle Shout")
                ));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Spell.CastSpell("Victory Rush",                 ret => Me.HealthPercent < 80 && Buff.PlayerHasBuff("Victorious"), "Victory Rush"),
                        Spell.CastSelfSpell("Enraged Regeneration",    ret => Me.HealthPercent < 45 && !Buff.PlayerHasBuff("Rallying Cry"), "Enraged Regeneration"),
                        Spell.CastSelfSpell("Rallying Cry",            ret => Me.HealthPercent < 45 && !Buff.PlayerHasBuff("Enraged Regeneration"), "Rallying Cry"),
                        Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40 && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            //flask,type=winters_bite
                            //food,type=black_pepper_ribs_and_shrimp
                            //stance,choose=battle
                            Buff.CastBuff("Berserker Stance", ret => StyxWoW.Me.Shapeshift != CLUSettings.Instance.Warrior.StanceSelection && CLUSettings.Instance.Warrior.StanceSelection == ShapeshiftForm.BerserkerStance, "Stance is Berserker"),
                            Buff.CastBuff("Battle Stance", ret => StyxWoW.Me.Shapeshift != CLUSettings.Instance.Warrior.StanceSelection && CLUSettings.Instance.Warrior.StanceSelection == ShapeshiftForm.BattleStance, "Stance is Battle"),
                            //mogu_power_potion
                            //battle_shout,if=!buff_exists
                            Buff.CastRaidBuff("Battle Shout", ret => true, "Battle Shout"),
                            //commanding_shout,if=!buff_exists
                            Buff.CastRaidBuff("Commanding Shout", ret => true, "Commanding Shout"),
                            //charge,if=!currenttarget.iswithinmeleerenage
                            Spell.CastSpell("Charge", ret => Me.CurrentTarget != null && (CLU.LocationContext == GroupLogic.Battleground && Macro.Manual || Unit.IsTrainingDummy(Me.CurrentTarget)) &&
                                Me.CurrentTarget.DistanceSqr > 3.2 * 3.2, "Charge"),
                            //heroic_leap,if=!currenttarget.iswithinmeleerange&spell.charge.down
                            Spell.CastSpellAtLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && (CLU.LocationContext == GroupLogic.Battleground && Macro.Manual ||
                                Unit.IsTrainingDummy(Me.CurrentTarget)) && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 &&
                                SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"))
                ));
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
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Spell.UseRacials(),
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
    }
}