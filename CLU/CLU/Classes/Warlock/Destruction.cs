using CLU.Helpers;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using CLU.Lists;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Warlock
{

    class Destruction : RotationBase
    {
        public override string Name
        {
            get {
                return "Destruction Warlock";
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
                return "Conflagrate";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Soulshatter, Soul Harvest < 2 shards out of combat\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Demon Soul while not moving & Summon Doomguard & Curse of the Elements & Lifeblood\n" +
                       "3. AoE with Rain of Fire, Shadowfury\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // Performance Timer (True = Enabled) returns Runstatus.Failure
                           // Spell.TreePerformance(true),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           PetManager.CastPetSummonSpell("Summon Imp", ret => !Me.GotAlivePet && (Buff.PlayerHasBuff("Demonic Rebirth") || Buff.PlayerHasBuff("Soulburn")), "Summoning Pet Imp"),
                           PetManager.CastPetSummonSpell("Summon Imp", ret => !Me.GotAlivePet, "Summoning Pet Imp"),
                           // Threat
                           Buff.CastBuff("Soulshatter",                    ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Soulshatter"),
                           // Multi-Dotting will occour if there are between 1 or more and less than 6 enemys within 15yrds of your current target and you have more than 50%. //Can be disabled within the GUI
                           Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50, "Immolate"),
                           Unit.FindMultiDotTarget(a => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 1 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) < 5 && Me.ManaPercent > 50, "Corruption"),
                           // Cooldown
                           Spell.CastSelfSpell("Demon Soul", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Me.IsMoving, "Demon Soul while not moving"),
                           // AoE here
                           Spell.ChannelAreaSpell("Rain of Fire", 10, true, 4, 0.0, 0.0, ret => !Me.IsMoving && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Rain of Fire"),
                           Spell.CastAreaSpell("Shadowfury", 10, true, 4, 0.0, 0.0, ret => Me.ManaPercent > 40, "Shadowfury"),
                           // End AoE
                           Buff.CastDebuff("Curse of the Elements",       ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 70 && !Buff.UnitHasMagicVulnerabilityDeBuffs(Me.CurrentTarget), "Curse of the Elements"),
                           //Spell.CastSelfSpell("Soulburn",                ret => !Buff.UnitHasHasteBuff(Me), "Soulburn"),
                           Spell.CastSpell("Soul Fire",                   ret => Buff.PlayerHasBuff("Soulburn"), "Soul Fire with soulburn"),
                           Buff.CastDebuff("Immolate",                    ret => true, "Immolate"),
                           Spell.CastSpell("Conflagrate",                 ret => true, "Conflagrate"),
                           Buff.CastDebuff("Immolate",                    ret => Buff.UnitHasHasteBuff(Me) && (Buff.PlayerBuffTimeLeft("Bloodlust") > 32 || Buff.PlayerBuffTimeLeft("Heroism") > 32 || Buff.PlayerBuffTimeLeft("Time Warp") > 32 || Buff.PlayerBuffTimeLeft("Ancient Hysteria") > 32) && (Spell.SpellCooldown("Conflagrate").TotalSeconds <= 3), "Immolate"),
                           Buff.CastDebuff("Bane of Doom",                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Unit.TimeToDeath(Me.CurrentTarget) > 60 && !Buff.UnitsHasMyBuff("Bane of Doom"), "Bane of Doom TTL=" + Unit.TimeToDeath(Me.CurrentTarget)),
                           Buff.CastDebuff("Agony",               ret => Me.CurrentTarget != null && !Unit.IsTargetWorthy(Me.CurrentTarget) || (Unit.IsTargetWorthy(Me.CurrentTarget) && (Unit.TimeToDeath(Me.CurrentTarget) < 60 || Unit.TimeToDeath(Me.CurrentTarget) == 9999) && (!Buff.UnitsHasMyBuff("Bane of Doom") || !Buff.TargetHasDebuff("Bane of Doom"))), "Agony TTL=" + Unit.TimeToDeath(Me.CurrentTarget)),
                           Spell.CastSpell("Bane of Havoc", u => Unit.BestBaneOfHavocTarget, ret => true, "Bane of Havoc on "), // + Unit.BestBaneOfHavocTarget.Name
                           //Buff.CastDebuff("Corruption",                  ret => true, "Corruption"),
                           Spell.CastConicSpell("Shadowflame", 11f, 33f,  ret => true, "ShadowFlame"),
                           Spell.CastSpell("Chaos Bolt",                  ret => (Spell.CastTime("Chaos Bolt") > 0.9), "Chaos Bolt"),
                           //Spell.CastSelfSpell("Summon Doomguard",        ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Doomguard"),
                           Spell.CastSpell("Soul Fire",                   ret => Buff.PlayerHasBuff("Empowered Imp"), "Soul Fire with Empowered Imp"),
                           // Buff.CastOffensiveBuff("Soul Fire", "Empowered Imp", Buff.PlayerBuffTimeLeft("Improved Soul Fire").TotalSeconds, "Soul Fire with Improved Soul Fire... (Empowered Imp=" + Buff.PlayerBuffTimeLeft("Empowered Imp").TotalSeconds + ")"),
                           Buff.CastOffensiveBuff("Soul Fire", "Improved Soul Fire", Spell.CastTime("Soul Fire") + 1.5 + Spell.CastTime("Incinerate") + Spell.GCD, "Soul Fire... (Soul Fire.cast_time+travel_time+incinerate.cast_time+gcd=" + (Spell.CastTime("Soul Fire") + 1.5 + Spell.CastTime("Incinerate") + Spell.GCD) + ")"),
                           Spell.CastSpell("Shadowburn",                  ret => true, "Shadowburn"),
                           Spell.CastSpell("Incinerate",                  ret => true, "Incinerate"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.IsMoving && Me.HealthPercent > Me.ManaPercent && Me.ManaPercent < 80, "Life tap while moving"),
                           Spell.CastSpell("Fel Flame",                   ret => Me.IsMoving, "Fel flame while moving"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.ManaPercent < 100 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40, "Life tap while mana < 100%"));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                               new PrioritySelector(
                                   PetManager.CastPetSummonSpell("Summon Imp", ret => !Me.IsMoving && !Me.GotAlivePet, "Summoning Pet Imp"),
                                   Buff.CastBuff("Soul Link", ret => Pet != null && Pet.IsAlive, "Soul Link")
                                  )));
            }
        }

        public override Composite Resting
        {
            get {
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get {
                return this.SingleRotation;
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
