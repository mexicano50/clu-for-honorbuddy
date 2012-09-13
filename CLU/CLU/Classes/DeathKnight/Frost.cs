using System.Linq;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using CLU.Base;
using Rest = CLU.Base.Rest;
using Styx.CommonBot;

namespace CLU.Classes.DeathKnight
{
    class Frost : RotationBase
    {
        public override string Name
        {
            get {
                return "Frost Deathknight";
            }
        }

        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        // adding some help
        public override string Help
        {
            get {
                return
                    @"
----------------------------------------------------------------------
Frost:
[*] Handles Killing Machine differently; Dual Wield (Frost Strike); 2Handed (Obliterate)
[*] Unholy runes are gamed to force RE procs or Blood Tap or Plague Leech on blood/frost
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)
[*] Death Siphon (only if movement enabled.)
This Rotation will:
1. Heal using AMS, IBF, Healthstone, Deathstrike
2. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Pillar of Frost & Raise Dead & Death and Decay & Empower Rune Weapon 
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
Credits to Weischbier, because he owns the buisness and I want him to have my babys! -- Sincerely Wulf (Bahahaha :P)
----------------------------------------------------------------------";
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
                return "Howling Blast";
            }
        }

        public override int KeySpellId
        {
            get { return 49184; }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),
                           // Items
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           
                           //Interrupts
                           Spell.CastInterupt("Mind Freeze", 		ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Mind Freeze"), //Why does nobody check for the range of melee kicks? // CurrentTarget Null check, we are accessing the objects property ;) --wulf
                           Spell.CastInterupt("Strangulate", 		ret => true, "Strangulate"),
                           Spell.CastInterupt("Asphyxiate", 		ret => true, "Asphyxiate"),// Replaces Strangulate -- Darth Vader like ability
                           //Diseases
                           Common.ApplyDiseases(ret => Me.CurrentTarget),
                           //Cooldowns
                           new Decorator(ret => Unit.IsTargetWorthy(Me.CurrentTarget) && Me.IsWithinMeleeRange,//Check for the damn range, we don't want to pop anything when the destination is shit away
                                         new PrioritySelector(
                                             Buff.CastBuff("Raise Dead", 				ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Pillar of Frost") && Buff.PlayerBuffTimeLeft("Pillar of Frost") <= 10 && Buff.PlayerHasBuff("Unholy Strength"), "Raise Dead"),
                                             Buff.CastBuff("Pillar of Frost", 			ret => Me.CurrentTarget != null, "Pillar of Frost"),
                                             Spell.CastSelfSpell("Empower Rune Weapon", ret => Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && StyxWoW.Me.FrostRuneCount < 1 && StyxWoW.Me.UnholyRuneCount < 2 && StyxWoW.Me.DeathRuneCount < 1 && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon")
                                         )
                                        ),
                           //Aoe
                           new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.EnemyUnits.Count() >= 3,
                               new PrioritySelector(
                                    Spell.CastSpell("Howling Blast",ret => (Me.FrostRuneCount >= 1 || Me.DeathRuneCount >= 1),"Howling Blast [Aoe]"),
                                    Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, 	ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget), "Death and Decay"),
                                    Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, 	ret => Me.CurrentTarget != null && (!BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && (Spell.RuneCooldown(4) == 0 && Spell.RuneCooldown(3) <= 1) || (Spell.RuneCooldown(3) == 0 && Spell.RuneCooldown(4) <= 1) && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Unit.IsTargetWorthy(Me.CurrentTarget)), "Death and Decay"),
                                    Common.SpreadDiseasesBehavior(ret => Me.CurrentTarget), // Used to spread your Diseases based upon your Tier one Talent. -- wulf
                                    Spell.CastSpell("Plague Strike", ret => Spell.SpellCooldown("Death and Decay").TotalSeconds > 6 && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3, "Plague Strike [Aoe]")
                                   )
                               ),
                           //Operation: Do Damage[Eyes only]
                           new Decorator(ret => Common.IsWieldingTwoHandedWeapon(),
                               new PrioritySelector(
                                    Spell.CastSpell("Soul Reaper", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"),
                                    Spell.CastSpell("Howling Blast", ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast (Rime)"),
                                    Spell.CastSpell("Obliterate", ret => Me.CurrentTarget, ret => Buff.PlayerHasBuff("Killing Machine"), "Obliterate (2 Hand Killing Machine)"),
                                    Spell.CastSpell("Obliterate", ret => Me.CurrentTarget, ret => Me.CurrentRunicPower < 85, "Obliterate (Utilize Runes)"),
                                    Spell.CastSpell("Frost Strike", ret => Me.CurrentTarget, ret => Me.RunicPowerPercent >= 90, "Frost Strike (Dumping Runic Power)"),
                                    Spell.CastSpell("Frost Strike", ret => Me.CurrentTarget, ret => true, "Frost Strike (Because we can)"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret =>  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"), //Don't waste it on Unholy Runes
                                    Buff.CastBuff("Horn of Winter", ret => true, "Horn of Winter (Because we can)")
                                   )
                               ),
                           //Operation: Do Damage[Eyes only]
                           new Decorator(ret => !Common.IsWieldingTwoHandedWeapon(),
                               new PrioritySelector(
                                    Spell.CastSpell("Soul Reaper", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"),
                                    Spell.CastSpell("Howling Blast", ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast (Rime)"),
                                    Spell.CastSpell("Frost Strike", ret => Me.CurrentTarget, ret => Buff.PlayerHasBuff("Killing Machine"), "Frost Strike (Dual Wield Killing Machine)"),
                                    Spell.CastSpell("Frost Strike", ret => Me.CurrentTarget, ret => Me.RunicPowerPercent > 84, "Frost Strike (Dumping Runic Power)"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret =>  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"), //Don't waste it on Unholy Runes
                                    Spell.CastSpell("Obliterate", ret => (Me.FrostRuneCount == 2 && Me.UnholyRuneCount == 2) || Me.DeathRuneCount == 2, "Obliterate (Utilizing Killing Machine)"),
                                    Spell.CastSpell("Obliterate", ret => Buff.PlayerHasBuff("Killing Machine") && Me.CurrentRunicPower <= 10, "Obliterate (Utilizing Killing Machine)"),
                                    Spell.CastSpell("Howling Blast", ret => true, "Howling Blast (Because we can)"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret =>  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"),  //Don't waste it on Unholy Runes
                                    Spell.CastSpell("Frost Strike", ret => true, "Frost Strike (Because we can)"),
                                    Spell.CastSpell("Horn of Winter",ret => Me, ret => true, "Horn of Winter for RP")
                                   )
                               )
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
                        //pillar_of_frost
                        Buff.CastBuff("Pillar of Frost", ret => Unit.IsTargetWorthy(StyxWoW.Me.CurrentTarget) && StyxWoW.Me.IsWithinMeleeRange && StyxWoW.Me.CurrentTarget != null, "Pillar of Frost"),
                        //C	4.31	raise_dead

                        //outbreak,if=dot.frost_fever.remains<3|dot.blood_plague.remains<3
                        Spell.CastSpell("Outbreak", ret => Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 || Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3, "Outbreak"),
                        //soul_reaper,if=target.health.pct<=35|((target.health.pct-3*(target.health.pct%target.time_to_die))<=35)
                        Spell.CastSpell("Soul Reaper", ret => StyxWoW.Me.CurrentTarget.HealthPercent <= 35, "Soul Reaping"),
                        //unholy_blight,if=talent.unholy_blight.enabled&(dot.frost_fever.remains<3|dot.blood_plague.remains<3)
                        Spell.CastSpell("Unholy Blight", ret => SpellManager.HasSpell("Unholy Blight") && (Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 ||
                            Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3), "Unholy Blight"),
                        //howling_blast,if=!dot.frost_fever.ticking
                        Spell.CastSpell("Howling Blast", ret => !Buff.TargetHasDebuff("Frost Fever"), "Howling Blast"),
                        //plague_strike,if=!dot.blood_plague.ticking
                        Spell.CastSpell("Plague Strike", ret => !Buff.TargetHasDebuff("Blood Plague"), "Plague Strike"),
                        new Decorator(ret => Common.IsWieldingTwoHandedWeapon(),
                            new PrioritySelector(
                                //plague_leech,if=talent.plague_leech.enabled&((cooldown.outbreak.remains<1)|(buff.rime.react&dot.blood_plague.remains<3&(unholy>=1|death>=1)))
                                Spell.CastSpell("Plague Leech", ret => SpellManager.HasSpell("Plague Leech") && ((SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds < 1) ||
                                    (Buff.PlayerHasBuff("Freezing Fog") && Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3 && (StyxWoW.Me.UnholyRuneCount >= 1 || StyxWoW.Me.DeathRuneCount >= 1))),
                                    "Plague Leech"),
                                //necrotic_strike,if=bsae_rotation.enabled
                                Spell.CastSpell("Necrotic Strike", ret => !Macro.rotationSwap, "Necrotic Strike"),
                                //howling_blast,if=buff.rime.react
                                Spell.CastSpell("Howling Blast", ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast"),
                                //obliterate,if=base_rotation.enabled&runic_power<=76
                                Spell.CastSpell("Obliterate", ret => Macro.rotationSwap && StyxWoW.Me.CurrentRunicPower <= 76, "Obliterate"),
                                //obliterate,if=base_rotation.disabled&runic_power<=76&frost>=1|unholy>=1
                                Spell.CastSpell("Obliterate", ret => !Macro.rotationSwap && StyxWoW.Me.CurrentRunicPower <= 76 && StyxWoW.Me.FrostRuneCount >= 1 && StyxWoW.Me.UnholyRuneCount >= 1, "Obliterate"),
                                //L	0.15	empower_rune_weapon,if=target.time_to_die<=60&buff.mogu_power_potion.up

                                //frost_strike,if=!buff.killing_machine.react
                                Spell.CastSpell("Frost Strike", ret => !Buff.PlayerHasBuff("Killing Machine"), "Frost Strike"),
                                //obliterate,if=base_rotation.enabled&buff.killing_machine.react
                                Spell.CastSpell("Obliterate", ret => Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine"), "Obliterate"),
                                //obliterate,if=base_rotation.disabled&buff.killing_machine.react&frost>=1|unholy>=1
                                Spell.CastSpell("Obliterate", ret => !Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine") && StyxWoW.Me.FrostRuneCount >= 1 && StyxWoW.Me.UnholyRuneCount >= 1,
                                    "Obliterate"),
                                //blood_tap,if=talent.blood_tap.enabled
                                Spell.CastSpell("Blood Tap", ret => SpellManager.HasSpell("Blood Tap") &&  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 ||
                                    Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap"),
                                //frost_strike
                                Spell.CastSpell("Frost Strike", ret => true, "Frost Strike"),
                                //horn_of_winter
                                Buff.CastBuff("Horn of Winter", ret => true, "Horn of Winter"),
                                //empower_rune_weapon
                                Spell.CastSpell("Empower Rune Weapon", ret => true, "Empower Rune Weapon"))),
                        new Decorator(ret => !Common.IsWieldingTwoHandedWeapon(),
                            new PrioritySelector(
                                //plague_leech,if=talent.plague_leech.enabled&!((buff.killing_machine.react&runic_power<10)|(unholy=2|frost=2|death=2))
                                Spell.CastSpell("Plague Leech", ret => SpellManager.HasSpell("Plague Leech") && !((Buff.PlayerHasBuff("Killing Machine") && StyxWoW.Me.CurrentRunicPower < 10) ||
                                    (StyxWoW.Me.UnholyRuneCount == 2 || StyxWoW.Me.FrostRuneCount == 2 || StyxWoW.Me.DeathRuneCount == 2)), "Plague Leech"),
                                //necrotic_strike,if=bsae_rotation.enabled
                                Spell.CastSpell("Necrotic Strike", ret => !Macro.rotationSwap, "Necrotic Strike"),
                                //howling_blast,if=buff.rime.react
                                Spell.CastSpell("Howling Blast", ret => Buff.PlayerHasBuff("Freezing Fog"), "Howling Blast"),
                                //frost_strike,if=runic_power>=88
                                Spell.CastSpell("Frost Strike", ret => StyxWoW.Me.CurrentRunicPower >= 88, "Frost Strike"),
                                //L	0.28	empower_rune_weapon,if=target.time_to_die<=60&buff.mogu_power_potion.up

                                //frost_strike,if=buff.killing_machine.react
                                Spell.CastSpell("Frost Strike", ret => Buff.PlayerHasBuff("Killing Machine"), "Frost Strike"),
                                //obliterate,if=base_rotation.enabled&buff.killing_machine.react&runic_power<10
                                Spell.CastSpell("Obliterate", ret => Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine") && StyxWoW.Me.CurrentRunicPower < 10, "Obliterate"),
                                //obliterate,if=base_rotation.disabled&buff.killing_machine.react&runic_power<10&frost>=1|unholy>=1
                                Spell.CastSpell("Obliterate", ret => !Macro.rotationSwap && Buff.PlayerHasBuff("Killing Machine") && StyxWoW.Me.CurrentRunicPower < 10 && StyxWoW.Me.FrostRuneCount >= 1 &&
                                    StyxWoW.Me.UnholyRuneCount >= 1, "Obliterate"),
                                //obliterate,if=base_rotation.enabled&(unholy=2|frost=2|death=2)
                                Spell.CastSpell("Obliterate", ret => Macro.rotationSwap && (StyxWoW.Me.UnholyRuneCount == 2 || StyxWoW.Me.FrostRuneCount == 2 || StyxWoW.Me.DeathRuneCount == 2), "Obliterate"),
                                //obliterate,if=base_rotation.disabled&(unholy=2|frost=2)
                                Spell.CastSpell("Obliterate", ret => !Macro.rotationSwap && (StyxWoW.Me.UnholyRuneCount == 2 || StyxWoW.Me.FrostRuneCount == 2), "Obliterate"),
                                //howling_blast
                                Spell.CastSpell("Howling Blast", ret => true, "Howling Blast"),
                                //frost_strike
                                Spell.CastSpell("Frost Strike", ret => true, "Frost Strike"),
                                //death_and_decay
                                Spell.CastSpellAtLocation("Death and Decay", u => StyxWoW.Me.CurrentTarget, ret => true, "Death and Decay"),
                                //plague_strike
                                Spell.CastSpell("Plague Strike", ret => true, "Plague Strike"),
                                //blood_tap,if=talent.blood_tap.enabled
                                Spell.CastSpell("Blood Tap", ret => SpellManager.HasSpell("Blood Tap") &&  Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 ||
                                    Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap"),
                                //horn_of_winter
                                Buff.CastBuff("Horn of Winter", ret => true, "Horn of Winter"),
                                //empower_rune_weapon
                                Spell.CastSpell("Empower Rune Weapon", ret => true, "Empower Rune Weapon")))
                ));
            }
        }

        public override Composite Medic
        {
            get {
                return
                    new PrioritySelector(
                        Buff.CastBuff("Anti-Magic Shell", ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0), "AMS"), // TODO: Put this back in when its fixed. && Me.CurrentTarget.IsTargetingMeOrPet
                        new Decorator(
                            ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                            new PrioritySelector(
                                Spell.CastSelfSpell("Death Pact",			ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Death Pact"),
                                Spell.CastSelfSpell("Raise Dead",			ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostPetSacrificePercent && !Buff.PlayerHasBuff("Icebound Fortitude") && CLUSettings.Instance.DeathKnight.FrostUsePetSacrifice, "Raise Dead"),
                                Spell.CastSelfSpell("Icebound Fortitude",	ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.FrostIceboundFortitudePercent && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                                Spell.CastSpell("Death Strike",				ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeEmergencyPercent, "Death Strike"),
                                Spell.CastSpell("Death Siphon",             ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathSiphonPercent && CLUSettings.Instance.EnableMovement, "Death Siphon"), // Only if movement is enabled..i.e. we are questing, gathering, etc...
                                // Conversion lasts until canceled!!..I am reluctant to put this in atm as its so underpowered
                                // Buff.CastBuff("Conversion",                 ret => (Buff.PlayerHasBuff("Unholy Presence") && Buff.PlayerHasBuff("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                                Item.UseBagItem("Healthstone",				ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "Healthstone"))
                        )
                    );
            }
        }

        public override Composite PreCombat
        {
            get {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            //flask,type=winters_bite
                            //food,type=black_pepper_ribs_and_shrimp
                            //frost_presence
                            Buff.CastBuff("Frost Presence", ret => !Me.HasMyAura("Frost Presence"), "We need it!"),
                            //horn_of_winter
                            Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter")
                            //army_of_the_dead
                            //mogu_power_potion
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
            get
            {
                return (
                    new PrioritySelector(
                        new Decorator(ret => Macro.Manual,
                            new Decorator(ret => StyxWoW.Me.CurrentTarget != null && Unit.IsTargetWorthy(StyxWoW.Me.CurrentTarget),
                                new PrioritySelector(
                                    Spell.CastSpell("Chains of Ice", ret => !StyxWoW.Me.CurrentTarget.IsWithinMeleeRange && !Unit.IsCrowdControlled(StyxWoW.Me.CurrentTarget), "Chains of Ice"),
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
            get {
                return this.SingleRotation;
            }
        }
    }
}