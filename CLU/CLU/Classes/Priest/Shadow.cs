using System.Linq;
using CLU.Helpers;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Lists;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Priest
{
    using global::CLU.Managers;
    using Styx.CommonBot;

    class Shadow : RotationBase
    {
        private const int ItemSetId = 1067; // Tier set ID Regalia of Dying light (Normal)

        public override string Name
        {
            get {
                return "Shadow Priest";
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
                return "Mind Blast"; // Mind Flay
            }
        }

        public override int KeySpellId
        {
            get { return 8092; }
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


                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return
                    @"
----------------------------------------------------------------------
Shadow MoP:
[*] Default Rotation is MoP Compliant: Leveling Rotaion is for just that..fast kills.
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Settings)
[*] Talents should be: From Darkness Comes Light, Divine Insight
This Rotation will:
1. Fade on threat, Shadowform during combat, Dispersion, Power Word: Shield,
	==> Healthstone, Flash Heal if movement enabled.
2. Buffs: Power Word: Fortitude, Inner Fire
3. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Shadowfiend & Dispersion
3. AoE with Mind Sear and Divine Star
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
----------------------------------------------------------------------" + twopceinfo + "\n" + fourpceinfo + "\n";
            }
        }

        private static bool CanMindFlay { get { return Buff.TargetHasBuff("Vampiric Touch") && Spell.SpellCooldown("Mind Blast").TotalSeconds > 4 && Buff.PlayerCountBuff("Shadow Orb") < 3; } }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Item.UseEngineerGloves())),
                           // Threat
                           Buff.CastBuff("Fade", ret => (CLUSettings.Instance.UseCooldowns || CLUSettings.Instance.Priest.UseFade) && Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Fade (Threat)"),

                           Item.RunMacroText("/cast Shadowform", ret => !Buff.PlayerHasBuff("Shadowform"), "Shadowform"),

                           // Grinding/Leveling/Farmig/Questing,etc Rotation (designed for High Kill Rate with the least amount of mana)
                           new Decorator(
                               ret => CLUSettings.Instance.Priest.SpriestRotationSelection == ShadowPriestRotation.Leveling,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Dispersion",        ret => (Me.HealthPercent < 10 || Me.ManaPercent < 10),"Dispersion"),
                                   Spell.CastSelfSpell("Mindbender",        ret => CLUSettings.Instance.UseCooldowns && Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet) >= 2, "Mindbender"),
                                   // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50% mana and we have Empowered Shadow. //Can be disabled within the GUI
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent <= 25, "Shadow Word: Death"),
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow") && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Shadow Word: Pain"),
                                   Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 4 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 6 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow") && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Vampiric Touch"),
                                   // End Multi-Dotting
                                   Spell.CastSpell("Shadow Word: Death",    ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 25, "Shadow Word: Death"),
                                   Spell.CastSpell("Mind Spike",            ret => Buff.PlayerHasBuff("Surge of Darkness"), "Mind Spike"), // Free Mindspike
                                   Spell.CastSpell("Mind Blast",            ret => !Me.IsMoving, "Mind Blast"),
                                   Buff.CastDebuff("Vampiric Touch",        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 10, "Vampiric Touch"),
                                   Buff.CastDebuff("Shadow Word: Pain",     ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 15, "Shadow Word: Pain"),
                                   Spell.CastSpell("Divine Star",           ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 4 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Divine Star"), // New patch 5.0.4 - could be a mana drainer --wulf
                                   Spell.CastSpell("Mind Sear",             ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 4 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Mind Sear"),
                                   Buff.CastDebuff("Devouring Plague",      ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 20, "Devouring Plague"),
                                   Spell.CastSpell("Shadowfiend",           ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Shadowfiend"),
                                   Spell.CastSpell("Mind Spike",            ret => true, "Mind Spike"),
                                   Spell.CastSpecialSpell("Mind Flay",      ret => CanMindFlay && Buff.TargetDebuffTimeLeft("Mind Flay").TotalSeconds <= Spell.ClippingDuration(), "Mind Flay")
                               )),


                           // Default Rotation
                           new Decorator(
                               ret => CLUSettings.Instance.Priest.SpriestRotationSelection == ShadowPriestRotation.Default,
                               new PrioritySelector(
                                   
                                   // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50% mana and we have Empowered Shadow. //Can be disabled within the GUI
                                   //Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent <= 25, "Shadow Word: Death"),
                                   //Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Shadow Word: Pain"),
                                   //Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 4 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 6 && Me.ManaPercent > 50 && Me.CurrentTarget.HealthPercent > 25 && Buff.PlayerHasActiveBuff("Empowered Shadow"), "Vampiric Touch"),
                                   // End Multi-Dotting
                                   Spell.CastSpell("Mind Blast",              ret => Buff.PlayerHasActiveBuff("Divine Insight"), "Mind Blast"),
                                   Buff.CastDebuff("Vampiric Touch",          ret => !Me.IsMoving, "Vampiric Touch"), // Vampiric Touch <DND> ??
                                   Buff.CastDebuff("Shadow Word: Pain",       ret => true, "Shadow Word: Pain"),
                                   Spell.CastSpell("Mind Blast",              ret => true, "Mind Blast"),
                                   Buff.CastDebuff("Devouring Plague",        ret => Buff.PlayerCountBuff("Shadow Orb") > 2, "Devouring Plague"),
                                   Spell.CastSpell("Shadow Word: Death",      ret => Me.CurrentTarget != null && (TalentManager.HasGlyph("Shadow Word: Death") ? Me.CurrentTarget.HealthPercent <= 100 : Me.CurrentTarget.HealthPercent <= 25), "Shadow Word: Death"),
                                   Spell.CastSpell("Mind Spike", ret => Buff.PlayerHasActiveBuff("Surge of Darkness"), "Mind Spike"),
                                   Spell.CastSpell("Mindbender",              ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Mindbender"),
                                   Spell.CastSpell("Mind Sear",               ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) >= 3 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Mind Sear"),
                                   Spell.CastSpell("Shadow Word: Death",      ret => Me.ManaPercent < 10, "Shadow Word: Death - Low Mana"),
                                   Spell.CastSpell("Shadow Word: Death",      ret => Me.IsMoving, "Shadow Word: Death - Moving"),
                                   Spell.CastSpell("Devouring Plague",        ret => Me.IsMoving && Me.ManaPercent > 10, "Devouring Plague"),
                                   Spell.CastSpell("Mind Blast",              ret => Buff.PlayerHasActiveBuff("Divine Insight"), "Mind Blast"),
                                   Spell.CastSelfSpell("Dispersion",          ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && (Me.HealthPercent < 10 || Me.ManaPercent < 10), "Dispersion"),
                                   Spell.CastSpecialSpell("Mind Flay",        ret => CanMindFlay && Buff.TargetDebuffTimeLeft("Mind Flay").TotalSeconds <= Spell.ClippingDuration(), "Mind Flay")
                               )) 
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
                        //new Action(a => { CLU.Log("I am the start of public Composite baseRotation"); return RunStatus.Failure; }),
                        //PvP Utilities

                        //Rotation
                        //shadowform
                        Spell.CastSelfSpell("Shadowform", ret => !Buff.PlayerHasBuff("Shadowform"), "Shadowform"),
                        //use_item,name=guardian_serpent_gloves
                        Item.UseEngineerGloves(),
                        //jade_serpent_potion,if=buff.bloodlust.react|target.time_to_die<=40
                        new Decorator(ret => !Me.IsMoving,
                            new PrioritySelector(
                                //devouring_plague,if=shadow_orb=3&(cooldown.mind_blast.remains<2|target.health.pct<20)
                                Spell.CastSpell("Devouring Plague", ret => Buff.PlayerCountBuff("Shadow Orb") == 3 && (SpellManager.Spells["Mind Blast"].CooldownTimeLeft.Seconds < 2 || Me.CurrentTarget.HealthPercent < 20), "Devouring Plague"),
                                //berserking
                                Racials.UseRacials(),
                                //mind_blast,if=num_targets<=6&cooldown_react
                                Spell.CastSpell("Mind Blast", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) <= 6, "Mind Blast"),
                                //shadow_word_pain,cycle_targets=1,max_cycle_targets=8,if=(!ticking|remains<tick_time)&miss_react
                                Spell.CastSpell("Shadow Word: Pain", ret => !Buff.TargetHasDebuff("Shadow Word: Pain") || Buff.TargetDebuffTimeLeft("Shadow Word: Pain").TotalSeconds < 2.52, "Shadow Word: Pain"),//~> GUI option for tick and cast time
                                //shadow_word_death,if=num_targets<=5
                                Spell.CastSpell("Shadow Word: Death", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) <= 5, "Shadow Word: Death"),
                                //vampiric_touch,cycle_targets=1,max_cycle_targets=8,if=(!ticking|remains<cast_time+tick_time)&miss_react
                                Spell.CastSpell("Vampiric Touch", ret => !Buff.TargetHasDebuff("Vampiric Touch") || Buff.TargetDebuffTimeLeft("Vampiric Touch").TotalSeconds < 1.26 + 2.52, "Vampiric Touch"),//~> GUI option for tick and cast time
                                //devouring_plague,if=shadow_orb=3
                                Spell.CastSpell("Devouring Plague", ret => Buff.PlayerCountBuff("Shadow Orb") == 3, "Devouring Plague"),
                                //H	11.10	halo_damage
                                //mind_spike,if=num_targets<=6&buff.surge_of_darkness.react
                                Spell.CastSpell("Mind Spike", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) <= 6 && Buff.PlayerHasActiveBuff("Surge of Darkness"), "Mind Spike"),
                                //shadowfiend,if=cooldown_react
                                Spell.CastSpell("Shadowfiend", ret => true, "Shadowfiend"),
                                //mind_sear,chain=1,interrupt=1,if=num_targets>=3
                                Spell.CastSpell("Mind Sear", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) >= 3, "Mind Sear"),
                                //mind_flay,chain=1,interrupt=1
                                Spell.ChannelSpell("Mind Flay", ret => true, "Mind Flay"),
                                //dispersion
                                Spell.CastSelfSpell("Dispersion", ret => true, "Dispersion"))),
                        new Decorator(ret => Me.IsMoving,
                            new PrioritySelector(
                                //berserking
                                Racials.UseRacials(),
                                //shadow_word_death,moving=1
                                Spell.CastSpell("Shadow Word: Death", ret => Me.IsMoving, "Shadow Word: Death"),
                                //mind_blast,moving=1,if=buff.divine_insight_shadow.react&cooldown_react
                                Spell.CastSpell("Mind Blast", ret => Me.IsMoving && Buff.PlayerHasActiveBuff("Divine Insight"), "Mind Blast"),
                                //shadow_word_pain,moving=1
                                Spell.CastSpell("Shadow Word: Pain", ret => Me.IsMoving, "Shadow Word: Pain"),
                                //dispersion
                                Spell.CastSelfSpell("Dispersion", ret => true, "Dispersion")))
                ));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Power Word: Shield",  ret => !Buff.PlayerHasBuff("Weakened Soul") && Me.HealthPercent < CLUSettings.Instance.Priest.ShieldHealthPercent, "Power Word: Shield"),
                               Item.UseBagItem("Healthstone",             ret => Me.HealthPercent < CLUSettings.Instance.Priest.UseHealthstone, "Healthstone"),
                               Spell.CastSpell("Flash Heal",              ret => Me.HealthPercent < CLUSettings.Instance.Priest.ShadowFlashHealHealth, "Emergency flash heal")));
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
                                //flask,type=warm_sun
                                //food,type=mogu_fish_stew
                                //power_word_fortitude,if=!aura.stamina.up
                                Buff.CastRaidBuff("Power Word: Fortitude", ret => CLUSettings.Instance.Priest.UsePowerWordFortitude, "Power Word: Fortitude"),
                                //inner_fire
                                Buff.CastBuff("Inner Fire", ret => CLUSettings.Instance.Priest.UseInnerFire, "Inner Fire"),
                                //shadowform
                                Spell.CastSelfSpell("Shadowform", ret => !Buff.PlayerHasBuff("Shadowform"), "Shadowform"),
                                //jade_serpent_potion
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
            get {
                return this.SingleRotation;
            }
        }
    }
}
