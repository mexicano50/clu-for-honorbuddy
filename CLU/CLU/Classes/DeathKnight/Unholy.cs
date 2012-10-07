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
using global::CLU.Managers;

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
                            Racials.UseRacials()
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
                                    Spell.CastSpell("Scourge Strike", ret => Spell.SpellOnCooldown("Death and Decay"), "Scourge Strike"),
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
                    Spell.CastSpell("Horn of Winter", ret => Me, ret => CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter for RP"));
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
                        Spell.CastSpell("Chains of Ice",        ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                        Spell.CastSpell("Death Grip",           ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice") && !SpellManager.CanCast("Chains of Ice"), "Death Grip"),

                        //Rotation
                        Racials.UseRacials(),
                        //mogu_power_potion,if=buff.dark_transformation.up&target.time_to_die<=35
                        Spell.CastSpell("Unholy Frenzy",        ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Unholy Frenzy"),//~> unholy_frenzy,if=time>=4
                        Item.UseEngineerGloves(),//~> use_item,name=gauntlets_of_the_lost_catacomb,if=time>=4
                        Spell.CastSpell("Outbreak",             ret => Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 || Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3, "Outbreak"),
                        Spell.CastSpell("Soul Reaper",          ret => Me.CurrentTarget.HealthPercent <= 35, "Soul Reaping"),//~> soul_reaper,if=target.health.pct<=35|((target.health.pct-3*(target.health.pct%target.time_to_die))<=35)
                        Buff.CastBuff("Unholy Blight",          ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 10 * 10 && TalentManager.HasTalent(3) && (Buff.TargetDebuffTimeLeft("Frost Fever").Seconds < 3 || Buff.TargetDebuffTimeLeft("Blood Plague").Seconds < 3), "Unholy Blight"),
                        Spell.CastSpell("Chains of Ice",        ret => !Buff.TargetHasDebuff("Frost Fever"), "Chains of Ice"),
                        Spell.CastSpell("Plague Strike",        ret => !Buff.TargetHasDebuff("Blood Plague"), "Plague Strike"),
                        Spell.CastSpell("Plague Leech",         ret => TalentManager.HasTalent(2) && SpellManager.Spells["Outbreak"].CooldownTimeLeft.Seconds < 1, "Plague Leech"),
                        Buff.CastBuff("Summon Gargoyle",        ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr <= 30 * 30, "Summon Gargoyle"),
                        Spell.CastSpell("Dark Transformation",  ret => true, "Dark Transformation"),
                        //empower_rune_weapon,if=target.time_to_die<=60&buff.mogu_power_potion.up
                        Spell.CastSpell("Necrotic Strike",      ret => !Macro.rotationSwap, "Necrotic Strike"),
                        Spell.CastSpell("Scourge Strike",       ret => Me.UnholyRuneCount == 2 && Me.CurrentRunicPower < 90, "Scourge Strike"),
                        Spell.CastSpell("Festering Strike",     ret => Me.BloodRuneCount == 2 && Me.FrostRuneCount == 2 && Me.CurrentRunicPower < 90, "Festering Strike"),
                        Spell.CastSpell("Death Coil",           ret => Me.CurrentRunicPower > 90, "Death Coil"),
                        Spell.CastSpell("Death Coil",           ret => Buff.PlayerHasBuff("Sudden Doom"), "Death Coil"),
                        Spell.CastSpell("Blood Tap",            ret => TalentManager.HasTalent(13) && Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap"),
                        Spell.CastSpell("Necrotic Strike",      ret => !Macro.rotationSwap, "Necrotic Strike"),
                        Spell.CastSpell("Scourge Strike",       ret => true, "Scourge Strike"),
                        Spell.CastSpell("Festering Strike",     ret => true, "Festering Strike"),
                        Spell.CastSpell("Death Coil",           ret => SpellManager.Spells["Summon Gargoyle"].CooldownTimeLeft.Seconds > 8, "Death Coil"),
                        Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter"),
                        Spell.CastSpell("Empower Rune Weapon",  ret => true, "Empower Rune Weapon")
                ));
            }
        }

        public override Composite Pull
        {
             get { return this.SingleRotation; }
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
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            //flask,type=winters_bite
                            //food,type=black_pepper_ribs_and_shrimp
                            Buff.CastBuff("Unholy Presence",        ret => CLU.LocationContext != GroupLogic.Battleground && !Me.HasMyAura("Unholy Presence"), "Unholy Presence"),
                            Buff.CastBuff("Frost Presence",         ret => CLU.LocationContext == GroupLogic.Battleground && !Me.HasMyAura("Frost Presence"), "Frost Presence"),
                            Buff.CastRaidBuff("Horn of Winter",     ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter"),
                            //army_of_the_dead
                            Spell.CastSelfSpell("Raise Dead",       ret => (Me.Pet == null || Me.Pet.IsDead), "Raise Dead"),
                            //mogu_power_potion
                            Spell.CastSpell("Chains of Ice",        ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice"), "Chains of Ice"),
                            Spell.CastSpell("Death Grip",           ret => Me.CurrentTarget != null && Macro.Manual && (CLU.LocationContext == GroupLogic.Battleground || Unit.IsTrainingDummy(Me.CurrentTarget)) && !Me.CurrentTarget.IsWithinMeleeRange && Me.CurrentTarget.DistanceSqr <= 30 * 30 && !Buff.TargetHasDebuff("Chains of Ice") && !SpellManager.CanCast("Chains of Ice"), "Death Grip"),
                            new Action(delegate
                            {
                                Macro.isMultiCastMacroInUse();
                                return RunStatus.Failure;
                            })
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
                        //new Action(a => { SysLog.Log("I am the start of public override Composite PVPRotation"); return RunStatus.Failure; }),
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