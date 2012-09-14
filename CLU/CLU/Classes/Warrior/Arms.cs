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
                       "Credits to gniegsch, lathrodectus and Obliv\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static bool IsMortalStrikeOnCooldown { get { return Spell.SpellCooldown("Mortal Strike").TotalSeconds > 1.5; } }

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
                                        Spell.UseRacials(),
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                        Item.UseEngineerGloves())),
                                    // Interupts
                                    Spell.CastInterupt("Pummel", ret => true, "Pummel"),
                                    Spell.CastInterupt("Spell Reflection", ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTarget == Me, "Spell Reflection"),

                                    // Start of Actions - SimCraft as of 3/31/2012
                                    // TODO: GCD Check and Tier pce check needs attention. Spell.GCD = 0
                                    Spell.CastSelfSpell("Berserker Rage",          ret => !Buff.PlayerHasActiveBuff("Deadly Calm") && Spell.SpellCooldown("Deadly Calm").TotalSeconds > 1.5 && Me.CurrentRage <= 95, "Berserker Rage"),
                                    Spell.CastSelfSpell("Deadly Calm",             ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Deadly Calm"),
                                    Spell.CastSelfSpell("Inner Rage",              ret => !Buff.PlayerHasActiveBuff("Deadly Calm") && Spell.SpellCooldown("Deadly Calm").TotalSeconds > 15, "Inner Rage"),
                                    Spell.CastSpell("Recklessness",                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && (Me.CurrentTarget.HealthPercent > 90 || Me.CurrentTarget.HealthPercent <= 20), "Recklessness"),
                                    Spell.CastSelfSpell("Berserker Stance",        ret => !Buff.PlayerHasBuff("Berserker Stance") && !Buff.PlayerHasActiveBuff("Taste for Blood") && Buff.TargetHasDebuff("Rend") && Me.CurrentRage <= 75 && CLUSettings.Instance.Warrior.EnableStanceDance, "**Berserker Stance**"),
                                    Spell.CastSelfSpell("Battle Stance",           ret => !Buff.PlayerHasBuff("Battle Stance") && !Buff.TargetHasDebuff("Rend") && CLUSettings.Instance.Warrior.EnableStanceDance, "**Battle Stance**"),
                                    Spell.CastSelfSpell("Battle Stance",           ret => !Buff.PlayerHasBuff("Battle Stance") && (Buff.PlayerHasActiveBuff("Taste for Blood") || Buff.PlayerHasActiveBuff("Overpower")) && Me.CurrentRage <= 75 && SpellManager.Spells["Mortal Strike"].Cooldown && CLUSettings.Instance.Warrior.EnableStanceDance, "**Battle Stance**"), // Spell.SpellCooldown("Mortal Strike").TotalSeconds > (1.5 + StyxWoW.WoWClient.Latency * 2 / 1000.0)
                                    Spell.CastSpell("Colossus Smash",              ret => Buff.PlayerHasBuff("Sudden Death") && IsMortalStrikeOnCooldown, "Colossus Smash"),
                                    Spell.CastAreaSpell("Sweeping Strikes", 5, false, 2, 0.0, 0.0, a => (Buff.PlayerHasBuff("Berserker Stance") || Buff.PlayerHasBuff("Battle Stance")), "Sweeping Strikes"),
                                    Spell.CastAreaSpell("Cleave", 5, false, 3, 0.0, 0.0, a => (Buff.PlayerHasBuff("Berserker Stance") || Buff.PlayerHasBuff("Battle Stance")) && Me.CurrentRage > 40, "Cleave"),
                                    // Disabled for now.  We need to only use if we have Blood and Thunder.
                                    // Spell.CastAreaSpell("Thunder Clap", 8, false, 2, 0.0, 0.0, a => Buff.PlayerHasBuff("Battle Stance") && Buff.TargetDebuffTimeLeft("Thunder Clap").TotalSeconds < 12.5 && Buff.TargetHasDebuff("Rend"), "TC Rend"),
                                    Spell.CastAreaSpell("Bladestorm", 5, false, 4, 0.0, 0.0, a => !Buff.PlayerHasActiveBuff("Deadly Calm") && !Buff.PlayerHasActiveBuff("Sweeping Strikes"), "Bladestorm"),
                                    Spell.CastSpell("Heroic Strike",               ret => Buff.PlayerHasActiveBuff("Deadly Calm"), "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike",               ret => Me.CurrentRage > 85, "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike",               ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Inner Rage") && Me.CurrentTarget.HealthPercent > 20 && (Item.Has2PcTeirBonus(ItemSetId) ? Me.CurrentRage > 50 : Me.CurrentRage > 60), "Heroic Strike"),
                                    Spell.CastSpell("Heroic Strike",               ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Inner Rage") && Me.CurrentTarget.HealthPercent <= 20 && ((Item.Has2PcTeirBonus(ItemSetId) ? Me.CurrentRage > 50 : Me.CurrentRage > 60) || Buff.PlayerHasActiveBuff("Battle Trance")), "Heroic Strike"),
                                    Spell.CastSpell("Mortal Strike",               ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent > 20, "Mortal Strike"),
                                    Spell.CastSpell("Colossus Smash",              ret => !Buff.TargetHasDebuff("Colossus Smash") && IsMortalStrikeOnCooldown, "Colossus Smash"),
                                    Spell.CastSpell("Execute",                     ret => Buff.PlayerActiveBuffTimeLeft("Executioner").TotalSeconds < 2.5, "Execute"),
                                    Spell.CastSpell("Mortal Strike",               ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 20 && (Buff.TargetDebuffTimeLeft("Rend").TotalSeconds < 3 || !Buff.PlayerHasActiveBuff("Wrecking Crew") || Me.CurrentRage <= 25 || Me.CurrentRage >= 35), "Mortal Strike"),
                                    Spell.CastSpell("Execute",                     ret => Me.CurrentRage > 90, "Execute"),
                                    Spell.CastSpell("Overpower",                   ret => Buff.PlayerHasActiveBuff("Taste for Blood") || Buff.PlayerHasActiveBuff("Overpower"), "Overpower"),
                                    Spell.CastSpell("Execute",                     ret => true, "Execute"),
                                    Spell.CastSpell("Colossus Smash",              ret => Buff.TargetDebuffTimeLeft("Colossus Smash").TotalSeconds < 1.5 && IsMortalStrikeOnCooldown, "Colossus Smash"),
                                    Spell.CastSpell("Slam",                        ret => (Me.CurrentRage >= 35 || Buff.PlayerHasActiveBuff("Battle Trance") || Buff.PlayerHasActiveBuff("Deadly Calm")) && IsMortalStrikeOnCooldown, "Slam"),
                                    Spell.CastSpell("Commanding Shout",            ret => Me.RagePercent < 60 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding, "Commanding Shout for Rage"),
                                    Spell.CastSpell("Battle Shout",                ret => Me.RagePercent < 60 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle, "Battle Shout for Rage"));
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
                        Spell.CastSpell("Charge", ret => Me.CurrentTarget.DistanceSqr > 5 * 5 && !Buff.TargetHasDebuff("Charge Stun"), "Charge"),
                        Spell.CastSpellAtLocation("Heroic Leap", ret => Me.CurrentTarget, ret => Me.CurrentTarget.DistanceSqr > 5 * 5 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 &&
                            SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                        //mogu_power_potion,if=(target.health.pct<20&buff.recklessness.up)|buff.bloodlust.react|target.time_to_die<=25
                        //7	3.46	recklessness,use_off_gcd=1,if=((debuff.colossus_smash.remains>=5|cooldown.colossus_smash.remains<=4)&((!talent.avatar.enabled|!set_bonus.tier14_4pc_melee)&((target.health.pct<20|target.time_to_die>315|(target.time_to_die>165&set_bonus.tier14_4pc_melee)))|(talent.avatar.enabled&set_bonus.tier14_4pc_melee&buff.avatar.up)))|target.time_to_die<=18
                        //8	0.00	avatar,use_off_gcd=1,if=talent.avatar.enabled&(((cooldown.recklessness.remains>=180|buff.recklessness.up)|(target.health.pct>=20&target.time_to_die>195)|(target.health.pct<20&set_bonus.tier14_4pc_melee))|target.time_to_die<=20)
                        //9	7.92	bloodbath,use_off_gcd=1,if=talent.bloodbath.enabled&(((cooldown.recklessness.remains>=10|buff.recklessness.up)|(target.health.pct>=20&(target.time_to_die<=165|(target.time_to_die<=315&!set_bonus.tier14_4pc_melee))&target.time_to_die>75))|target.time_to_die<=19)
                        //berserker_rage,use_off_gcd=1,if=!buff.enrage.up
                        Spell.CastSelfSpell("Berserker Rage", ret => !Buff.PlayerHasActiveBuff("Enrage"), "Berserker Rage"),
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
                        Spell.CastSpell("Impending Victory", ret => SpellManager.HasSpell("Impending Victory"), "Impending Victory"),
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
                            Buff.CastBuff("Battle Stance", ret => !StyxWoW.Me.HasMyAura("Battle Stance"), "Battle Stance"),
                            //mogu_power_potion
                            Buff.CastRaidBuff("Battle Shout", ret => true, "Battle Shout"),
                            Buff.CastRaidBuff("Commanding Shout", ret => true, "Commanding Shout"))
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