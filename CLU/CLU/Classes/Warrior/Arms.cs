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
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that. \n" +
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
                                    Spell.CastSpell("Rend",                        ret => !Buff.TargetHasDebuff("Rend"), "Rend"),
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
                            Spell.CastSpell("Battle Stance", ret => !StyxWoW.Me.HasMyAura("Battle Stance"), "Battle Stance"),
                            //mogu_power_potion
                            Buff.CastRaidBuff("Commanding Shout",   ret => true, "Commanding Shout"),
                            Buff.CastRaidBuff("Battle Shout",       ret => true, "Battle Shout")));
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
                        new Decorator(ret => Macro.Manual,
                            new Decorator(ret => StyxWoW.Me.CurrentTarget != null && Unit.IsTargetWorthy(StyxWoW.Me.CurrentTarget),
                                new PrioritySelector(
                                    Spell.CastSpell("Charge", ret => !StyxWoW.Me.CurrentTarget.IsWithinMeleeRange && !Unit.IsCrowdControlled(StyxWoW.Me.CurrentTarget), "Charge"),
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
                                    new Decorator(ret => !Macro.Burst, baseRotation)))
                )));
            }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}