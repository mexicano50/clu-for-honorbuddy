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

        // adding some help
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
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
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

                     // Trinkets & Cooldowns
                         new Decorator(
                             ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                 new PrioritySelector(
                                         Item.UseTrinkets(),
                                         Spell.UseRacials(),
                                         Item.UseEngineerGloves(),
                                         Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                         Spell.CastSpell("Shattering Throw", ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Buff.UnitHasHasteBuff(Me)) && CLUSettings.Instance.Warrior.UseShatteringThrow, "Shattering Throw"),
                                         Spell.CastSelfSpell("Recklessness", ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Buff.UnitHasHasteBuff(Me)) && CLUSettings.Instance.Warrior.UseRecklessness, "Recklessness"))),
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
                             Spell.CastAreaSpell("Thunder Clap", 10, false, CLUSettings.Instance.Warrior.ProtAoECount, 0.0, 0.0, ret => !WoWSpell.FromId(6343).Cooldown, "Thunder Clap"),
                             Spell.CastSpell("Intimidating Shout", ret => TalentManager.HasGlyph("Intimidating Shout"), "Intimidating Shout") //only use if glyphed
                             )),
                    // START Main Rotation
                     Spell.CastSpell("Heroic Strike",       ret => Buff.PlayerHasActiveBuff("Ultimatum") || Me.RagePercent >= CLUSettings.Instance.Warrior.ProtHeroicStrikeRagePercent, "Heroic Strike"),
                     Spell.CastSpell("Shield Slam",         ret => true, "Shield Slam on CD"),
                     Spell.CastSpell("Revenge",             ret => true, "Revenge on CD"),
                     Spell.CastSelfSpell("Deadly Calm",     ret => CLUSettings.Instance.Warrior.UseDeadlyCalm && CLUSettings.Instance.UseCooldowns, "Deadly Calm"),
                     Spell.CastSelfSpell("Berserker Rage",  ret => CLUSettings.Instance.Warrior.UseBerserkerRage && CLUSettings.Instance.UseCooldowns, "Berserker Rage"),
                     Spell.CastAreaSpell("Thunder Clap", 8, false, 1, 0.0, 0.0, ret => !Buff.UnitHasWeakenedBlows(Me.CurrentTarget), "Thunder Clap for Weakened Blows"),
                     Buff.CastDebuff("Demoralizing Shout",  ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedBlows(Me.CurrentTarget) && CLUSettings.Instance.Warrior.UseDemoralizingShout, "Demoralizing Shout"),
                     Spell.CastSpell("Commanding Shout",    ret => Me.RagePercent < 40 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Commanding, "Commanding Shout for Rage"),
                     Spell.CastSpell("Battle Shout",        ret => Me.RagePercent < 40 && CLUSettings.Instance.Warrior.ShoutSelection == WarriorShout.Battle, "Battle Shout for Rage"),
                     Spell.CastSpell("Devastate",           ret => true, "Devastate if SS and Rev on CD")

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
                        Spell.CastSelfSpell("Last Stand",       ret => Me.HealthPercent < CLUSettings.Instance.Warrior.LastStandPercent && !Buff.PlayerHasBuff("Shield Wall") && !Buff.PlayerHasBuff("Rallying Cry") && !Buff.PlayerHasBuff("Enraged Regeneration"), "Last Stand"),
                        Spell.CastSelfSpell("Shield Block",     ret => Me.HealthPercent < CLUSettings.Instance.Warrior.ShieldBlockPercent && Me.RagePercent >= 60 && !Buff.PlayerHasBuff("Shield Block"), "Shield Block"),
                        Spell.CastSelfSpell("Shield Barrier",   ret => Me.HealthPercent < CLUSettings.Instance.Warrior.ShieldBarrierPercent && Me.RagePercent >= 60 && !Buff.PlayerHasBuff("Shield Barrier"), "Shield Block"),
                        Spell.CastSpell("Impending Victory",    ret => Me.CurrentTarget != null && Me.HealthPercent < CLUSettings.Instance.Warrior.ImpendingVictoryPercent && Me.RagePercent > 10, "Impending Victory"),
                        Spell.CastSelfSpell("Shield Wall",      ret => Me.HealthPercent < CLUSettings.Instance.Warrior.ShieldWallPercent && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Rallying Cry"), "Shield Wall"),
                        Spell.CastSelfSpell("Rallying Cry",     ret => Me.HealthPercent > CLUSettings.Instance.Warrior.RallyingCryPercent && !Buff.PlayerHasBuff("Last Stand") && !Buff.PlayerHasBuff("Shield Wall") && Unit.WarriorRallyingCryPlayers, "Rallying Cry - Somebody needs me!"),
                        Item.UseBagItem("Healthstone",          ret => Me.HealthPercent < CLUSettings.Instance.Warrior.HealthstonePercent, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                        new Decorator(
                            ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                            new PrioritySelector(
                                Buff.CastRaidBuff("Commanding Shout", ret => true, "Commanding Shout"),
                                Buff.CastRaidBuff("Battle Shout", ret => true, "Battle Shout"),
                                Buff.CastBuff("Defensive Stance", ret => StyxWoW.Me.Shapeshift != ShapeshiftForm.DefensiveStance, "Defensive Stance We need it!"))));
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