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

    class Unholy : RotationBase
    {
        public override string Name
        {
            get {
                return "Unholy Deathknight";
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
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Heal using AMS, IBF, Healthstone and Deathstrike < 40%\n" +
                       "2. AutomaticCooldowns has: \n" +
                           "==> UseTrinkets \n" +
                           "==> UseRacials \n" +
                           "==> UseEngineerGloves \n" +
                           "==> Unholy Frenzy & Summon Gargoyle & Death and Decay & Empower Rune Weapon \n" +
                       "3. Maintain HoW only if similar buff is not present\n" +
                       "4. Ensure we are in Unholy Presence\n" +
                       "5. Use Death and Decay and Pestilence and Blood boil for AoE\n" +
                       "6. Brez players (non-specific) using Raise Ally\n" +
                       "NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'. \n" +
                       "Credits to Weischbier, ossirian, kbrebel04, Toney001 and cowdude\n" +
                       "----------------------------------------------------------------------\n";
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
                return "Scourge Strike";
            }
        }

        public override int KeySpellId
        {
            get { return 55090; }
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

                    new Decorator(
                        ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                            Item.UseEngineerGloves(),
                            Spell.UseRacials()
                            )
                        ),

                    //Interrupts
                    Spell.CastInterupt("Mind Freeze",ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange,"Mind Freeze"),//Why does nobody check for the range of melee kicks? // CurrentTarget Null check, we are accessing the objects property ;) --wulf
                    Spell.CastInterupt("Strangulate", ret => true, "Strangulate"),
                    Spell.CastInterupt("Asphyxiate", ret => true, "Asphyxiate"),// Replaces Strangulate -- Darth Vader like ability
                    //Diseases
                    Common.ApplyDiseases(ret => Me.CurrentTarget),
                    // TIMMEEEEEEE
                    Spell.CastSpell("Dark Transformation", ret => true, "Dark Transformation"),
                    Spell.CastSelfSpell("Raise Dead", ret => (Me.Pet == null || Me.Pet.IsDead), "Raise Dead"),//Gettin' Timmy back
                    //Cooldowns
                    new Decorator(ret => Unit.IsTargetWorthy(Me.CurrentTarget) && Me.IsWithinMeleeRange,//Check for the damn range, we don't want to pop anything when the destination is shit away
                                  new PrioritySelector(
                                      Buff.CastBuffonUnit("Unholy Frenzy", u => Unit.BestUnholyFrenzyTarget,ret =>Me.CurrentRunicPower >= 60 && !Buff.UnitHasHasteBuff(Unit.BestUnholyFrenzyTarget),"Unholy Frenzy"),
                                      Buff.CastBuff("Summon Gargoyle",ret => Me.CurrentRunicPower >= 60 && Buff.UnitHasHasteBuff(Me),"Gargoyle"),
                                      Spell.CastSelfSpell("Empower Rune Weapon",ret =>Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && StyxWoW.Me.FrostRuneCount < 1 && StyxWoW.Me.UnholyRuneCount < 2 && StyxWoW.Me.DeathRuneCount < 1 && !Buff.UnitHasHasteBuff(Me),"Empower Rune Weapon")
                                      )
                        ),

                    // Short Duration AoE
                    new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.EnemyUnits.Count() > 2 && CLUSettings.Instance.UseAoEAbilities && Unit.EnemyUnits.Count() < 6,
                               new PrioritySelector(
                                    Common.SpreadDiseasesBehavior(ret => Me.CurrentTarget), // Used to spread your Diseases based upon your Tier one Talent. -- wulf
                                    Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving, "Death and Decay"),
                                    Spell.CastSpell("Scourge Strike",ret => Spell.SpellOnCooldown("Death and Decay"),""),
                                    Spell.CastAreaSpell("Blood Boil", 10, false, 3, 0.0, 0.0, ret => Me.BloodRuneCount >= 1, "Blood Boil"),
                                    Spell.CastSpell("Death Coil", ret => Me.ActiveAuras.ContainsKey("Sudden Doom"), "Death Coil (Sudden Doom)"),// need ActiveAuras, don't mess with me! Seriously... -- Weischbier
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret => Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"),  //Don't waste it on Unholy Runes
                                    Spell.CastSpell("Icy Touch", ret => Me.ActiveAuras.ContainsKey("Sudden Doom"), "Icy Touch")
                                   )
                               ),
                    // Long Duration AoE
                    new Decorator(ret => CLUSettings.Instance.UseAoEAbilities && Unit.EnemyUnits.Count() > 5,
                               new PrioritySelector(
                                    Common.SpreadDiseasesBehavior(ret => Me.CurrentTarget), // Used to spread your Diseases based upon your Tier one Talent. -- wulf
                                    Spell.CastAreaSpell("Death and Decay", 10, true, 3, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.UnholyRuneCount == 2 && Unit.EnemyUnits.Count() >= 3 && !Me.IsMoving && !Me.CurrentTarget.IsMoving, "Death and Decay"),
                                    Spell.CastSpell("Festering Strike", ret => Common.FrostRuneSlotsActive + Common.BloodRuneSlotsActive >= 2, "Festering Strike"),
                                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret => Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"),  //Don't waste it on Unholy Runes
                                    Spell.CastAreaSpell("Blood Boil", 10, false, 3, 0.0, 0.0, ret => Me.BloodRuneCount >= 1, "Blood Boil")
                                   )
                               ),
                    // Sustained Damage
                    Spell.CastSpell("Soul Reaper", ret => Me.CurrentTarget, ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"),
                    Spell.CastSpell("Scourge Strike",ret => Me.CurrentRunicPower < 90,"Scourge Strike (generating Runic Power)"),
                    Spell.CastSpell("Festering Strike", ret => Me.CurrentRunicPower < 90, "Festering Strike (generating Runic Power)"),
                    Spell.CastSpell("Death Coil", ret => Me.CurrentRunicPower >= 90, "Death Coil (dumping Runic Power)"),
                    Spell.CastSpell("Death Coil", ret => Me.ActiveAuras.ContainsKey("Sudden Doom"), "Death Coil (Sudden Doom)"),// need ActiveAuras, don't mess with me! Seriously... -- Weischbier
                    Spell.CastSpell("Blood Tap", ret => Me.CurrentTarget, ret => Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"),  //Don't waste it on Unholy Runes
                    Spell.CastSpell("Scourge Strike", ret => true, "Scourge Strike"),
                    Spell.CastSpell("Festering Strike", ret => true, "Festering Strike"),
                    Spell.CastSpell("Horn of Winter",ret => Me, ret => true, "Horn of Winter for RP"));
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
                        Spell.CastSpell("Chains of Ice", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                        Spell.CastSpell("Death Grip", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && !Buff.TargetHasDebuff("Chains of Ice") &&
                            !SpellManager.CanCast("Chains of Ice"), "Death Grip"),
                        //blood_fury,if=time>=2
                        Spell.UseRacials(),
                        //mogu_power_potion,if=buff.dark_transformation.up&target.time_to_die<=35
                        //unholy_frenzy,if=time>=4
                        Spell.CastSpell("Unholy Frenzy", ret => true, "Unholy Frenzy"),
                        //use_item,name=gauntlets_of_the_lost_catacomb,if=time>=4
                        Item.UseEngineerGloves(),
                        //outbreak,if=dot.frost_fever.remains<3|dot.blood_plague.remains<3
                        Spell.CastSpell("Outbreak", ret => Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 || Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3, "Outbreak"),
                        //soul_reaper,if=target.health.pct<=35|((target.health.pct-3*(target.health.pct%target.time_to_die))<=35)
                        Spell.CastSpell("Soul Reaper", ret => StyxWoW.Me.CurrentTarget.HealthPercent <= 35, "Soul Reaping"),
                        //unholy_blight,if=talent.unholy_blight.enabled&(dot.frost_fever.remains<3|dot.blood_plague.remains<3)
                        Spell.CastSpell("Unholy Blight", ret => SpellManager.HasSpell("Unholy Blight") && (Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 ||
                            Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3), "Unholy Blight"),
                        //chains_of_ice,if=!dot.frost_fever.ticking
                        Spell.CastSpell("Chains of Ice", ret => !Buff.TargetHasDebuff("Frost Fever"), "Chains of Ice"),
                        //plague_strike,if=!dot.blood_plague.ticking
                        Spell.CastSpell("Plague Strike", ret => !Buff.TargetHasDebuff("Blood Plague"), "Plague Strike"),
                        //plague_leech,if=talent.plague_leech.enabled&(cooldown.outbreak.remains<1)
                        Spell.CastSpell("Plague Leech", ret => SpellManager.HasSpell("Plague Leech") && SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds < 1, "Plague Leech"),
                        //summon_gargoyle
                        Buff.CastBuff("Summon Gargoyle", ret => true, "Summon Gargoyle"),
                        //dark_transformation
                        Spell.CastSpell("Dark Transformation", ret => true, "Dark Transformation"),
                        //empower_rune_weapon,if=target.time_to_die<=60&buff.mogu_power_potion.up
                        //necrotic_strike,if=base_rotation.disabled
                        Spell.CastSpell("Necrotic Strike", ret => !Macro.rotationSwap, "Necrotic Strike"),
                        //scourge_strike,if=base_rotation.enabled&unholy=2&runic_power<90
                        Spell.CastSpell("Scourge Strike", ret => StyxWoW.Me.UnholyRuneCount == 2 && StyxWoW.Me.CurrentRunicPower < 90, "Scourge Strike"),
                        //festering_strike,if=blood=2&frost=2&runic_power<90
                        Spell.CastSpell("Festering Strike", ret => StyxWoW.Me.BloodRuneCount == 2 && StyxWoW.Me.FrostRuneCount == 2 && StyxWoW.Me.CurrentRunicPower < 90, "Festering Strike"),
                        //death_coil,if=runic_power>90
                        Spell.CastSpell("Death Coil", ret => StyxWoW.Me.CurrentRunicPower > 90, "Death Coil"),
                        //death_coil,if=buff.sudden_doom.react
                        Spell.CastSpell("Death Coil", ret => Buff.PlayerHasBuff("Sudden Doom"), "Death Coil"),
                        //blood_tap,if=talent.blood_tap.enabled
                        Spell.CastSpell("Blood Tap", ret => SpellManager.HasSpell("Blood Tap") && Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap"),
                        //necrotic_strike,if=base_rotation.disabled
                        Spell.CastSpell("Necrotic Strike", ret => !Macro.rotationSwap, "Necrotic Strike"),
                        //scourge_strike
                        Spell.CastSpell("Scourge Strike", ret => true, "Scourge Strike"),
                        //festering_strike
                        Spell.CastSpell("Festering Strike", ret => true, "Festering Strike"),
                        //death_coil,if=cooldown.summon_gargoyle.remains>8
                        Spell.CastSpell("Death Coil", ret => SpellManager.Spells["Summon Gargoyle"].CooldownTimeLeft.Seconds > 8, "Death Coil"),
                        //horn_of_winter
                        Buff.CastBuff("Horn of Winter", ret => true, "Horn of Winter"),
                        //empower_rune_weapon
                        Spell.CastSpell("Empower Rune Weapon", ret => true, "Empower Rune Weapon")
                ));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Buff.CastBuff("Conversion", ret => (Me.HasAura("Unholy Presence") && Me.HasAura("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                               Spell.CastSelfSpell("Raise Dead",                  ret => (Me.Pet == null || Me.Pet.IsDead), "Raise Dead"),
                               Spell.CastSelfSpell("Icebound Fortitude",          ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.UnholyIceboundFortitudePercent && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                               Item.UseBagItem("Healthstone",                     ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "Healthstone"),
                               Spell.CastSpell("Death Strike",                    ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeEmergencyPercent, "Death Strike"),
                               Spell.CastSpell("Death Siphon", ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathSiphonPercent && CLUSettings.Instance.EnableMovement, "Death Siphon"), // Only if movement is enabled..i.e. we are questing, gathering, etc
                               // Conversion lasts until canceled!!..I am reluctant to put this in atm as it seems underpowered
                               // Buff.CastBuff("Conversion",                 ret => (Buff.PlayerHasBuff("Unholy Presence") && Buff.PlayerHasBuff("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                               Spell.CastSelfSpell("Death Pact",                  ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.UnholyPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.UnholyUsePetSacrifice    , "Death Pact")));
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
                            //unholy_presence,if=PvE
                            Buff.CastBuff("Unholy Presence", ret => CLU.LocationContext != GroupLogic.Battleground && !Me.HasMyAura("Unholy Presence"), "Unholy Presence"),
                            //unholy_presence,if=PvP
                            Buff.CastBuff("Frost Presence", ret => CLU.LocationContext == GroupLogic.Battleground && !Me.HasMyAura("Frost Presence"), "Frost Presence"),
                            //horn_of_winter
                            Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter"),
                            //army_of_the_dead
                            //raise_dead
                            Spell.CastSelfSpell("Raise Dead", ret => (Me.Pet == null || Me.Pet.IsDead), "Raise Dead"),
                            //mogu_power_potion
                            Spell.CastSpell("Chains of Ice", ret => Me.CurrentTarget != null && (CLU.LocationContext == GroupLogic.Battleground && Macro.Manual || Unit.IsTrainingDummy(Me.CurrentTarget)) &&
                                Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                            Spell.CastSpell("Death Grip", ret => Me.CurrentTarget != null && CLU.LocationContext == GroupLogic.Battleground && Macro.Manual && Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 &&
                                !Buff.TargetHasDebuff("Chains of Ice") && !SpellManager.CanCast("Chains of Ice"), "Death Grip")
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
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    //Spell.CastSpell("Chains of Ice", ret => Me.CurrentTarget != null &&Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                                    //Spell.CastSpell("Death Grip", ret => Me.CurrentTarget != null &&Me.CurrentTarget.DistanceSqr > 3.2 * 3.2 && !Buff.TargetHasDebuff("Chains of Ice") &&
                                        //!SpellManager.CanCast("Chains of Ice"), "Death Grip"),
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