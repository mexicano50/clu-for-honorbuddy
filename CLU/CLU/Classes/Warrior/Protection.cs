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
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;
using CLU.Managers;
using Styx.WoWInternals;


namespace CLU.Classes.Warrior
{
    using Styx;
    using Styx.CommonBot;

    class Protection : RotationBase
    {
        private const int ItemSetId = -466; // Tier set ID Colossal Dragonplate Armor

        public override string Name
        {
            get { return "Protection Warrior"; }
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
            get { return "Shield Slam"; }
        }

        public override int KeySpellId
        {
            get { return 23922; }
        }

        public override float CombatMaxDistance
        {
            get { return 3.2f; }
        }

        public override string Help
        {
            get
            {
                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return
                    @"
----------------------------------------------------------------------
Fury MoP:
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Settings)
This Rotation will:
1. Heal using Last Stand, Shield Block, Rallying Cry, Shield Barrier
	==> Healthstone, Impending Victory
2. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Avatar, Bloodbath, Death Wish
NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'.
----------------------------------------------------------------------" + twopceinfo + "\n" + fourpceinfo + "\n";
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

                     // Kill flying units.
                     Common.HandleFlyingUnits,

                     // Trinkets & Cooldowns
                         new Decorator(
                             ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                 new PrioritySelector(
                                         Item.UseTrinkets(),
                                         Racials.UseRacials(),
                                         Item.UseEngineerGloves(),
                                         Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                         Spell.CastSpell("Shattering Throw", ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Buff.UnitHasHasteBuff(Me)) && CLUSettings.Instance.Warrior.UseShatteringThrow, "Shattering Throw"),
                                         Spell.CastSelfSpell("Recklessness", ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Buff.UnitHasHasteBuff(Me)) && CLUSettings.Instance.Warrior.UseRecklessness && CLUSettings.Instance.UseCooldowns, "Recklessness"))),
                    // Interupts
                     Spell.CastInterupt("Pummel",               ret => CLUSettings.Instance.Warrior.UsePummel, "Pummel"),
                     Spell.CastInterupt("Shockwave",            ret => CLUSettings.Instance.Warrior.UseShockwave, "Shockwave"),
                     Spell.CastInterupt("Spell Reflection",     ret => Me.CurrentTarget != null && Me.CurrentTarget.CurrentTarget == Me && CLUSettings.Instance.Warrior.UseSpellReflection, "Spell Reflection"),
                     Spell.CastAreaSpell("Intimidating Shout", 8, false, 1, 0.0, 0.0, ret => CLUSettings.Instance.Warrior.UseIntimidatingShout, "Intimidating Shout"),
                     Buff.CastDebuff("Demoralizing Shout",       ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedBlows(Me.CurrentTarget) && CLUSettings.Instance.Warrior.UseDemoralizingShout, "Demoralizing Shout"),
                     // START AoE
                     new Decorator(
                         ret => Unit.CountEnnemiesInRange(Me.Location, 12) >= CLUSettings.Instance.Warrior.ProtAoECount,
                         new PrioritySelector(
                             Spell.CastConicSpell("Shockwave", 11f, 33f, ret => true, "Shockwave"),
                             Spell.CastSpell("Cleave",          ret => Buff.PlayerHasActiveBuff("Ultimatum") || Me.RagePercent >= CLUSettings.Instance.Warrior.ProtAoECleaveRagePercent, "Cleave"),
                             Spell.CastAreaSpell("Thunder Clap", 10, false, CLUSettings.Instance.Warrior.ProtAoECount, 0.0, 0.0, ret => !WoWSpell.FromId(6343).Cooldown && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Thunder Clap"),
                             Spell.CastSpell("Intimidating Shout", ret => TalentManager.HasGlyph("Intimidating Shout"), "Intimidating Shout") //only use if glyphed
                             )),
                    // START Main Rotation
                     Spell.CastSpell("Heroic Strike",       ret => Buff.PlayerHasActiveBuff("Ultimatum") || Me.RagePercent >= CLUSettings.Instance.Warrior.ProtHeroicStrikeRagePercent, "Heroic Strike"),
                     Spell.CastSpell("Shield Slam",         ret => true, "Shield Slam on CD"),
                     Spell.CastSpell("Revenge",             ret => true, "Revenge on CD"),
                     Spell.CastSelfSpell("Deadly Calm",     ret => CLUSettings.Instance.Warrior.UseDeadlyCalm && CLUSettings.Instance.UseCooldowns, "Deadly Calm"),
                     Spell.CastSelfSpell("Berserker Rage", ret => Me.CurrentTarget != null && (CLUSettings.Instance.Warrior.UseBerserkerRage && CLUSettings.Instance.UseCooldowns && Me.CurrentTarget.IsWithinMeleeRange), "Berserker Rage"),
                     Spell.CastAreaSpell("Thunder Clap", 8, false, 1, 0.0, 0.0, ret => !Buff.UnitHasWeakenedBlows(Me.CurrentTarget) && Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Thunder Clap for Weakened Blows"),
                     Buff.CastDebuff("Demoralizing Shout",  ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedBlows(Me.CurrentTarget) && CLUSettings.Instance.Warrior.UseDemoralizingShout, "Demoralizing Shout"),
                     Spell.CastSpell("Commanding Shout",    ret => Me.RagePercent < 40 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding, "Commanding Shout for Rage"),
                     Spell.CastSpell("Battle Shout",        ret => Me.RagePercent < 40 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle, "Battle Shout for Rage"),
                     Spell.CastSpell("Sunder Armor", ret => true, "Devastate if SS and Rev on CD")

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
                        Spell.CastSpell("Charge",                   ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 25 * 25, "Charge"),
                        Spell.CastOnUnitLocation("Heroic Leap",     ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8  && Me.CurrentTarget.DistanceSqr <= 40 * 40 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                        Spell.CastSpell("Hamstring",                ret => !Buff.TargetHasDebuff("Hamstring"), "Hamstring"),

                        //Rotation
                        //earthen_potion,if=health_pct<35&buff.earthen_potion.down
                        Racials.UseRacials(),
                        Spell.CastSelfSpell("Last Stand",           ret => Me.CurrentHealth < 30000, "Last Stand"),
                        Spell.CastSpell("Heroic Strike",            ret => Buff.PlayerHasActiveBuff("Ultimatum"), "Heroic Strike"),
                        Spell.CastSelfSpell("Berserker Rage",       ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Berserker Rage"),
                        Spell.CastSpell("Shield Slam",              ret => Me.CurrentRage < 75, "Shield Slam"),
                        Spell.CastSpell("Revenge",                  ret => Me.CurrentRage < 75, "revenge"),
                        Spell.CastSpell("Shield Block",             ret => true, "Shield Block"),
                        Spell.CastSpell("Thunder Clap",             ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Thunder Clap"),
                        Buff.CastBuff("Battle Shout",               ret => Me.CurrentRage < 80, "Battle Shout"),
                        Spell.CastSpell("Sunder Armor",             ret => true, "Devastate")
                ));
            }
        }

        public override Composite Pull
        {
             get { return this.SingleRotation; }
        }

        public override Composite Medic
        {
            get
            {
                return (
                    new Decorator(ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                        new PrioritySelector(
                            Spell.CastSelfSpell("Last Stand",       ret => Me.HealthPercent < CLUSettings.Instance.Warrior.LastStandPercent && !Buff.PlayerHasBuff("Shield Wall") && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Last Stand"),
                            Spell.CastSelfSpell("Shield Block",     ret => Me.HealthPercent < CLUSettings.Instance.Warrior.ShieldBlockPercent && Me.RagePercent >= 60 && !Buff.PlayerHasBuff("Shield Block"), "Shield Block"),
                            Spell.CastSelfSpell("Shield Barrier",   ret => Me.HealthPercent < CLUSettings.Instance.Warrior.ShieldBarrierPercent && Me.RagePercent >= 60 && !Buff.PlayerHasBuff("Shield Barrier"), "Shield Block"),
                            Spell.CastSpell("Victory Rush",    ret => Me.CurrentTarget != null && Me.HealthPercent < CLUSettings.Instance.Warrior.ImpendingVictoryPercent && Me.RagePercent > 10, "Victory Rush or Impending Victory"),
                            Spell.CastSelfSpell("Shield Wall",      ret => Me.HealthPercent < CLUSettings.Instance.Warrior.ShieldWallPercent && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Rallying Cry"), "Shield Wall"),
                            Spell.CastSelfSpell("Rallying Cry",     ret => Me.HealthPercent > CLUSettings.Instance.Warrior.RallyingCryPercent && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Shield Wall") && Unit.WarriorRallyingCryPlayers, "Rallying Cry - Somebody needs me!"),
                            Item.UseBagItem("Healthstone",          ret => Me.HealthPercent < CLUSettings.Instance.Warrior.HealthstonePercent, "Healthstone")
                )));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new PrioritySelector(
                        new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                            new PrioritySelector(
                                //flask,type=earth
                                //food,type=great_pandaren_banquet
                                Buff.CastBuff("Defensive Stance",           ret => StyxWoW.Me.Shapeshift != ShapeshiftForm.DefensiveStance, "Defensive Stance"),
                                //earthen_potion
                                Buff.CastRaidBuff("Commanding Shout",       ret => true, "Commanding Shout"),
                                Buff.CastRaidBuff("Battle Shout",           ret => true, "Battle Shout"),
                                Spell.CastSpell("Charge",                   ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 25 * 25, "Charge"),
                                Spell.CastOnUnitLocation("Heroic Leap",     ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Me.CurrentTarget.DistanceSqr <= 40 * 40 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds > 1 && SpellManager.Spells["Charge"].CooldownTimeLeft.Seconds < 18, "Heroic Leap"),
                                new Action(delegate
                                {
                                    Macro.isMultiCastMacroInUse();
                                    return RunStatus.Failure;
                                })
                ))));
            }
        }

        public override Composite Resting
        {
            get { return Base.Rest.CreateDefaultRestBehaviour(); }
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
                            new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
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