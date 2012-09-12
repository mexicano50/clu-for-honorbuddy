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

    class Demonology : RotationBase
    {
        public override string Name
        {
            get {
                return "Demonology Warlock";
            }
        }

        public override string Revision
        {
            get
            {
                return "1";
            }
        }

        public override string KeySpell
        {
            get {
                return "Incinerate";
            }
        }

        // I want to keep moving at melee range while morph is available
        // note that this info is used only if you enable moving/facing in the CC settings.
        public override float CombatMaxDistance
        {
            get {
                return (Spell.CanCast("Metamorphosis", Me) || Buff.PlayerHasBuff("Metamorphosis")) ? 10f : 35f;
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
                       "This CC will:\n" +
                       "1. Soulshatter, Interupts with Axe Toss, Soul Harvest < 2 shards out of combat\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Demon Soul & Summon Doomguard & Metamorphosis & Curse of the Elements & Lifeblood\n" +
                       "==> Felgaurd to Felhunter Swap\n" +
                       "3. AoE with Hellfire and Felstorm and Shadowflame\n" +
                       "4. Best Suited for end game raiding\n" +
                       "Ensure you're at least at 10yards from your target to maximize your dps, even during burst phase if possible.\n" +
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
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Item.UseEngineerGloves())),

                           // START Interupts & Spell casts
                           new Decorator(ret => Me.GotAlivePet,
                                         new PrioritySelector(
                                             PetManager.CastPetSpell("Axe Toss", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && PetManager.CanCastPetSpell("Axe Toss"), "Axe Toss")
                                         )
                                        ),

                           // lets get our pet back
                           new Decorator(ret => !Me.GotAlivePet,
                                         new PrioritySelector(
                                             Spell.CastSelfSpell("Soulburn", ret => !Buff.UnitHasHasteBuff(Me), "Soulburn for Pet"),
                                             PetManager.CastPetSummonSpell("Summon Felhunter", ret => true, "Summoning Pet Felhunter (lets get our pet back)")
                                         )
                                        ),

                           // Threat
                           Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "[High Threat] Soulshatter - Stupid Tank"),

                           // Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Buff.CastBuff("Metamorphosis",             ret => true, "Metamorphosis"),
                                   Spell.CastSelfSpell("Demon Soul",          ret => true, "Demon Soul"),
                                  // Spell.CastSelfSpell("Summon Doomguard",    ret => true, "Doomguard"),
                                   PetManager.CastPetSpell("Felstorm",              ret => Me.GotAlivePet && Me.CurrentTarget != null && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Felstorm(Special Ability)"),
                                   // Make sure our Felhunter did Axe Toss, before calling new pet -> Axe Toss needs to be on Cooldown
                                   Spell.CastSelfSpell("Soulburn", ret => !PetManager.CanCastPetSpell("Axe Toss") && !PetManager.PetHasBuff("Felstorm"), "Soulburn to raise Felhunter"),
                                   // Call pet - regardless if we have Soulburn or not, we need this pet!
                                   Spell.CastSelfSpell("Summon Felhunter", ret => !PetManager.PetHasBuff("Felstorm") && !PetManager.HasSpellPet("Devour Magic") && !PetManager.CanCastPetSpell("Felstorm") && Buff.PlayerHasBuff("Soulburn"), "Felhunter (after axe toss)")
                               )),
                           Spell.CastSelfSpell("Soulburn",                ret => !Buff.UnitHasHasteBuff(Me) && !PetManager.PetHasBuff("Felstorm"), "Soulburn"),
                           Spell.CastSpell("Soul Fire",                   ret => Buff.PlayerHasBuff("Soulburn"), "Soul Fire with soulburn"),

                           // AoE
                           PetManager.CastPetSpell("Felstorm",                  ret =>  Me.GotAlivePet && Me.CurrentTarget != null && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange && !PetManager.PetHasBuff("Felstorm") && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 12) > 0 && (Me.CurrentTarget.CurrentHealth > 310000 || Me.CurrentTarget.MaxHealth == 1), "Felstorm(Special Ability)"),
                           Spell.ChannelAreaSpell("Hellfire", 11.0, false, 4, 0.0, 0.0, ret => !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Hellfire"),

                           // Main Rotation
                           Buff.CastDebuff("Curse of the Elements",       ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 70 && !Buff.UnitHasMagicVulnerabilityDeBuffs(Me.CurrentTarget), "Curse of the Elements"),
                           Buff.CastDebuff("Immolate",                    ret => true, "Immolate"),
                           Buff.CastDebuff("Bane of Doom",                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Unit.TimeToDeath(Me.CurrentTarget) > 60 && !Buff.UnitsHasMyBuff("Bane of Doom"), "Bane of Doom TTL=" + Unit.TimeToDeath(Me.CurrentTarget)),
                           Buff.CastDebuff("Agony",               ret => Me.CurrentTarget != null && !Unit.IsTargetWorthy(Me.CurrentTarget) || (Unit.IsTargetWorthy(Me.CurrentTarget) && (Unit.TimeToDeath(Me.CurrentTarget) < 60 || Unit.TimeToDeath(Me.CurrentTarget) == 9999) && (!Buff.UnitsHasMyBuff("Bane of Doom") || !Buff.TargetHasDebuff("Bane of Doom"))), "Agony TTL=" + Unit.TimeToDeath(Me.CurrentTarget)),
                           Buff.CastDebuff("Corruption",                  ret => true, "Corruption"),
                           Spell.CastConicSpell("Shadowflame", 11f, 33f,  ret => true, "ShadowFlame"),
                           Spell.CastSpell("Hand of Gul'dan",             ret => true, "Hand of Gul'dan"),
                           Spell.CastSelfSpell("Immolation Aura",         ret => Buff.PlayerBuffTimeLeft("Metamorphosis") > 10 && Unit.DistanceToTargetBoundingBox() <= 10f, "Immolation Aura"),
                           Spell.CastSpell("Incinerate",                  ret => Buff.PlayerHasActiveBuff("Molten Core"), "Incinerate"),
                           Spell.CastSpell("Shadow Bolt",                 ret => Buff.PlayerHasBuff("Shadow Trance"), "Instant Shadow Bolt"),
                           Spell.CastSpell("Soul Fire",                   ret => Buff.PlayerBuffTimeLeft("Decimation") > 0, "Soul Fire with Decimation"),
                           // Spell.CastSpell("Soul Fire",                ret => Buff.PlayerHasActiveBuff("Decimation"), "Soul Fire with Decimation"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.ManaPercent <= 30 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40 && !Buff.UnitHasHasteBuff(Me) && !Buff.PlayerHasBuff("Metamorphosis") && !Buff.PlayerHasBuff("Demon Soul: Felguard"), "Life tap - mana < 30%"),
                           Spell.CastSpell("Incinerate",                  ret => true, "Incinerate"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.IsMoving && Me.HealthPercent > Me.ManaPercent && Me.ManaPercent < 80, "Life tap while moving"),
                           Spell.CastSpell("Fel Flame",                   ret => Me.IsMoving, "Fel flame while moving"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.ManaPercent < 100 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40, "Life tap - mana < 100%"));
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
                                   PetManager.CastPetSummonSpell("Summon Felguard", ret => !Me.IsMoving && !Me.GotAlivePet, " Summoning Pet Felguard"),
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
