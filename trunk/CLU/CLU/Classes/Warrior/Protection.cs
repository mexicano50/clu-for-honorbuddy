﻿using CLU.Helpers;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;
using CLU.Managers;

namespace CLU.Classes.Warrior
{

    class Protection : RotationBase
    {

        public override string Name
        {
            get { return "Protection Warrior 5.0.4 V1"; }
        }

        public override string KeySpell
        {
            get { return "Shield Slam"; }
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
                return "----------------------------------------------------------------------\n" +
                      "uses impending victory and shockwave talents" +
                       "intimidating shout glyphed" +
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
                             Spell.CastSpell("Intimidating Shout", ret => TalentManager.HasGlyph("Intimidating Shout"), "Intimidating Shout") //only use if glyphed
                             )),
                    //Spell.CastAreaSpell("Thunder Clap", 10, false, CLUSettings.Instance.Warrior.ProtAoECount, 0.0, 0.0, ret => true, "Thunder Clap"), TODO: This - for some stupid reason is halting the rotation.
                    // START Main Rotation

                     Spell.CastSpell("Heroic Strike",       ret => Buff.PlayerHasActiveBuff("Ultimatum") || Me.RagePercent >= CLUSettings.Instance.Warrior.ProtHeroicStrikeRagePercent, "Heroic Strike"),
                     Spell.CastSpell("Shield Slam",         ret => true, "Shield Slam on CD"),
                     Spell.CastSpell("Revenge",             ret => true, "Revenge on CD"),
                     Spell.CastSelfSpell("Deadly Calm",     ret => CLUSettings.Instance.Warrior.UseDeadlyCalm, "Deadly Calm"),
                     Spell.CastSelfSpell("Berserker Rage",  ret => CLUSettings.Instance.Warrior.UseBerserkerRage, "Berserker Rage"),
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
                                Buff.CastBuff("Defensive Stance", ret => !Me.HasMyAura("Defensive Stance"), "Defensive Stance We need it!"))));
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