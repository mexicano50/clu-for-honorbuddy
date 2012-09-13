#region Author
/*
 * $Author$
 * $Date$
 * $ID$
 * $Revision$
 * $URL$
 */
#endregion
using CLU.Helpers;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using Styx.WoWInternals;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Warlock
{


    class Affliction : RotationBase
    {
        public override string Name
        {
            get {
                return "Affliction Warlock";
            }
        }

        public override string Revision
        {
            get
            {
                return "$Revision$";
            }
        }

        public override string KeySpell
        {
            get {
                return "Unstable Affliction";
            }
        }
        public override int KeySpellId
        {
            get { return 30108; }
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
                       "Credits to ShinobiAoshi, Stormchasing\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                            // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                            // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),
                            // START Interupts & Spell casts
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                            //Buffs
                            Buff.CastBuff("Dark Intent", ret => !Me.ActiveAuras.ContainsKey("Dark Intent"), "Dark Intent"),
                            // Threat
                            Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Soulshatter"),                            
                            //Call Pet
                            new PrioritySelector(
                              Buff.CastBuff("Soulburn", ret => !Me.ActiveAuras.ContainsKey("Soulburn") && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503), "Soulburn"),
                              PetManager.CastPetSummonSpell(691, ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Soulburn")) && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503), "Summon Pet")
                             ),
                            
                            //Grimoire of Service
                            Spell.CastSpellByID(111897, ret => Me.GotAlivePet && !WoWSpell.FromId(111897).Cooldown && TalentManager.HasTalent(14), "Grimoire of Service"),
                            //Sacrifice Pet
                            Spell.CastSelfSpellByID(108503,ret => Me.GotAlivePet && TalentManager.HasTalent(15) && !Buff.PlayerHasActiveBuff(108503), "Grimoire of Sacrifice"),
                    //Cooldowns
                    new Decorator(ret=> CLUSettings.Instance.UseCooldowns,
                        new PrioritySelector(
                            Buff.CastBuff("Dark Soul: Misery", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Me.IsMoving, "Dark Soul: Misery"),
                            //// TODO: Remove this when Apoc fixs Spellmanager. -- wulf 
                            new Decorator(ret => !WoWSpell.FromId(112927).Cooldown && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    new Decorator(ret=>TalentManager.HasTalent(13),
                                        new Sequence(
                                            new Action(a => CLU.Log(" [Casting] Summon Terrorguard ")),
                                            new Action(ret => Spell.CastfuckingSpell("Summon Terrorguard")))),
                                    new Decorator(ret=>!TalentManager.HasTalent(13),
                                        new Sequence(
                                            new Action(a => CLU.Log(" [Casting] Summon Doomguard ")),
                                            new Action(ret => Spell.CastfuckingSpell("Summon Doomguard"))))))
                            )),
                            Spell.CastSelfSpell("Unending Resolve", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.HealthPercent < 40, "Unending Resolve (Save my life)"),
                            Spell.CastSelfSpell("Twilight Warden", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.HealthPercent < 80, "Twilight Warden (Protect me from magical damage)"),
                            //Basic DPSing
                            Buff.CastDebuff("Curse of the Elements", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 70 && !Buff.UnitHasMagicVulnerabilityDeBuffs(Me.CurrentTarget), "Curse of the Elements"),
                    //fast application of debuffs
                    new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Me.CurrentTarget.HasAnyAura("Agony", "Corruption", "Unstable Affliction"),
                          new Sequence(
                              Buff.CastBuff("Soulburn", ret => !Me.ActiveAuras.ContainsKey("Soulburn"), "Soulburn"),
                              new WaitContinue(new System.TimeSpan(0,0,0,0,50), ret => false, new ActionAlwaysSucceed()),
                              Spell.CreateWaitForLagDuration(),
                              Spell.CastSpell("Soul Swap", ret => Me.ActiveAuras.ContainsKey("Soulburn"), "Soul Swap")
                             )),
                    //Slow Application
                     Buff.CastDebuff("Agony", ret => true, "Agony"), //Should never met in regular situations
                     Buff.CastDebuff("Corruption", ret => true, "Corruption"), //Should never met in regular situations
                     Buff.CastDebuff("Unstable Affliction", ret => true, "Unstable Affliction"),
                    //Basic DPSing
                    Buff.CastDebuff("Haunt", ret => true, "Haunt"),
                    Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 20 && Me.HealthPercent > 40, "Life Tap"),
                    Spell.CastSpell("Fel Flame", ret => Me.IsMoving, "Fel flame while moving"),
                    Spell.ChannelSpell(103103, ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent >= 20 || Me.CurrentSoulShards > 0) && !Me.IsMoving && !Me.IsCasting, "Malefic Grasp"),
                    Spell.ChannelSpell(1120, ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Me.CurrentSoulShards == 0) && !Me.IsMoving, "Drain Soul"));
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
                                   Buff.CastBuff("Dark Intent", ret => true, "Dark Intent"),
                                   PetManager.CastPetSummonSpell(691, ret => !Me.IsMoving && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503), "Summon Pet")
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