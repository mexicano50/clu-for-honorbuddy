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
using CLU.Classes.DeathKnight;
using CLU.Base;
using Rest = CLU.Base.Rest;


namespace CLU.Classes.Deathknight
{

    class Blood : RotationBase
    {
        // public static readonly HealerBase Healer = HealerBase.Instance;

        public override string Name
        {
            get {
                return "Blood Deathknight";
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
                return "Rune Tap";
            }
        }
        public override int KeySpellId
        {
            get { return 48982; }
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
Blood:
[*] *IMPORTANT* Please Spec into Blood Tap for Maximum effectiveness.
[*] Choose your own presence (blood is recommended)
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)
[*] UnholyBlight and RoilingBlood support for spreading diseases
[*] new tallent Asphyxiate supported
[*] Plague Leech supported.
[*] Blood Boil to refresh diseases.
[*] Soul Reaper added before heartstrike if target < 35%
[*] Death Siphon (only if movement enabled.)
This Rotation will:
1. Heal using AMS, IBF, Healthstone, Deathstrike, RuneTap, Vampiric Blood, Lichbourne
2. Intelligently Heal with Deathstrikes
3. Maintain HoW only if similar buff is not present
4. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Pillar of Frost & Raise Dead & Death and Decay & Empower Rune Weapon 
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
Credits to Weischbier, because he owns the buisness and I want him to have my babys! -- Sincerely Wulf (Bahahaha :P)
----------------------------------------------------------------------";

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

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Item.UseEngineerGloves(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"))),
                           // Encounter Specific (Uncomment the two lines below for HC Deathwing) //thanks to gniegsch fo his input!
                           // Spell.CastSelfSpell("Bone Shield", ret => Me.CurrentTarget != null && Me.CurrentTarget.ChanneledCastingSpellId == 109632 && Me.CurrentTarget.IsTargetingMeOrPet, "Bone Shield for Impale"),
                           // Spell.CastSpell("Death Strike", ret =>Me.CurrentTarget != null && Me.CurrentTarget.ChanneledCastingSpellId == 109632 && Me.CurrentTarget.IsTargetingMeOrPet, "DS for shield"),
                           // Interupts
                           Spell.CastInterupt("Mind Freeze",              ret => Me.CurrentTarget != null && Me.CurrentTarget.IsWithinMeleeRange, "Mind Freeze"), 
                           Spell.CastInterupt("Strangulate",              ret => true, "Strangulate"),
                           Spell.CastInterupt("Asphyxiate",               ret => true, "Asphyxiate"),// Replaces Strangulate -- Darth Vader like ability
                           Spell.CastSelfSpell("Bone Shield",             ret => Spell.SpellCooldown("Death Strike").TotalSeconds > 3 && CLUSettings.Instance.DeathKnight.UseBoneShieldonCooldown, "Bone Shield"),
                            // Apply/Refresh Diseases (Single Target) TODO: Decide if we want to refresh Weakened Blows with blood boil here. -- wulf
                           Common.ApplyDiseases(ret => Me.CurrentTarget),
                           // Spread Diseases (Multiple Targets)
                           Common.SpreadDiseasesBehavior(ret => Me.CurrentTarget), // Used to spread your Diseases based upon your Tier one Talent.
                           Spell.CastAreaSpell("Death and Decay", 10, true, CLUSettings.Instance.DeathKnight.BloodDeathAndDecayCount, 0.0, 0.0, ret => !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) || (!BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Buff.PlayerHasBuff("Crimson Scourge")), "Death and Decay"),
                           // Main Rotation
                           // TODO: Scent of Blood Procs monitoring for fully stacked Death Strikes as soon as you take damage. --wulf
                           Spell.CastSpell("Death Strike", 				  ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikePercent || (Me.UnholyRuneCount + Me.FrostRuneCount + Me.DeathRuneCount >= 4) || (Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathStrikeBloodShieldPercent && (Buff.PlayerBuffTimeLeft("Blood Shield") < CLUSettings.Instance.DeathKnight.DeathStrikeBloodShieldTimeRemaining)), "Death Strike"),
                           Spell.CastSpell("Blood Tap", ret => Buff.PlayerCountBuff("Blood Charge") >= 5 && (Common.FrostRuneSlotsActive == 0 || Common.UnholyRuneSlotsActive == 0 || Common.BloodRuneSlotsActive == 0), "Blood Tap (Refreshed a depleted Rune)"), 
                           Spell.CastSelfSpell("Rune Tap",                ret => Me.HealthPercent <= CLUSettings.Instance.DeathKnight.RuneTapPercent && Me.BloodRuneCount >= 1 && CLUSettings.Instance.DeathKnight.UseRuneTap, "Rune Tap"),
                           Spell.CastSpell("Rune Strike",                 ret => (Me.CurrentRunicPower >= CLUSettings.Instance.DeathKnight.RuneStrikePercent || Me.HealthPercent > 90) && Me.CurrentRunicPower >= 30 && (Me.UnholyRuneCount == 0 || Me.FrostRuneCount == 0) && !Buff.PlayerHasBuff("Lichborne"), "Rune Strike"),
                           Spell.CastAreaSpell("Blood Boil", 10, false, CLUSettings.Instance.DeathKnight.BloodBloodBoilCount, 0.0, 0.0, ret => Me.BloodRuneCount >= 1 || Buff.PlayerHasActiveBuff("Crimson Scourge"), "Blood Boil"),
                           Spell.CastSpell("Soul Reaper",                 ret => Me.BloodRuneCount > 0 && Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 35, "Soul Reaping"), // Never use Soul Reaping if you have no Blood runes, as this will cause Heart Strike to consume a Death rune, which should be saved for Death Strike.
                           Spell.CastSpell("Blood Strike",                ret => Me.BloodRuneCount > 0, "Heart Strike"), // Never use Heart Strike if you have no Blood runes, as this will cause Heart Strike to consume a Death rune, which should be saved for Death Strike.
                           Spell.CastSpell("Death Coil",                  ret => Me.CurrentTarget != null && !Spell.CanCast("Rune Strike", Me.CurrentTarget) && Me.CurrentRunicPower >= 90, "Death Coil"),
                           Spell.CastSpell("Horn of Winter", ret => (Me.CurrentRunicPower < 34 || !Buff.UnitHasStrAgiBuff(Me)) && CLUSettings.Instance.DeathKnight.UseHornofWinter, "Horn of Winter for RP"));
            }
        }

        public override Composite Pull
        {
             get { return this.SingleRotation; }
        }

        public override Composite Medic
        {
            get {
                return new PrioritySelector(
                        Buff.CastBuff("Anti-Magic Shell", ret => Me.CurrentTarget != null && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.DeathKnight.UseAntiMagicShell && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.ChanneledCastingSpellId != 0), "AMS"), // TODO: Put this back in when its fixed. && Me.CurrentTarget.IsTargetingMeOrPet
                        Spell.CastSelfSpell("Bone Shield",                    ret => CLUSettings.Instance.DeathKnight.UseBoneShieldDefensively && CLUSettings.Instance.EnableSelfHealing && Spell.SpellCooldown("Death Strike").TotalSeconds > 3 && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Lichborne"), "Bone Shield"),
                        new Decorator(
                            ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                            new PrioritySelector(
                               Spell.CastSelfSpell("Death Pact",              ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.BloodPetSacrificePercent && Me.Minions.FirstOrDefault(q => q.CreatureType == WoWCreatureType.Undead || q.CreatureType == WoWCreatureType.Totem) != null && CLUSettings.Instance.DeathKnight.BloodUsePetSacrifice, "Death Pact"),
                               Spell.CastSelfSpell("Rune Tap",                ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.RuneTapWoTNPercent && Buff.PlayerHasBuff("Will of the Necropolis") && CLUSettings.Instance.DeathKnight.UseRuneTapWoTN, "Rune Tap & WotN "),
                               Spell.CastSpell("Death Coil", ret => Me,       ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathCoilHealPercent && Buff.PlayerHasBuff("Lichborne"), "Death Coil Heal"),
                               Spell.CastSelfSpell("Vampiric Blood",          ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.VampiricBloodPercent && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.UseVampiricBlood, "Vampiric Blood"),
                               Spell.CastSpell("Dancing Rune Weapon",         ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DancingRuneWeaponPercent && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.UseDancingRuneWeapon, "Dancing Rune Weapon"),
                               Spell.CastSelfSpell("Lichborne",               ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.LichbornePercent && Me.CurrentRunicPower >= 60 && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && CLUSettings.Instance.DeathKnight.UseLichborne, "Lichborne"),
                               Spell.CastSelfSpell("Raise Dead",              ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.BloodPetSacrificePercent && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Icebound Fortitude") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.BloodUsePetSacrifice, "Raise Dead"),
                               Spell.CastSelfSpell("Icebound Fortitude",      ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.BloodIceboundFortitudePercent && !Buff.PlayerHasBuff("Bone Shield") && !Buff.PlayerHasBuff("Vampiric Blood") && !Buff.PlayerHasBuff("Dancing Rune Weapon") && !Buff.PlayerHasBuff("Lichborne") && CLUSettings.Instance.DeathKnight.UseIceboundFortitude, "Icebound Fortitude "),
                               Spell.CastSelfSpell("Empower Rune Weapon", ret => Me.CurrentTarget != null && CLUSettings.Instance.DeathKnight.UseEmpowerRuneWeapon && Unit.IsTargetWorthy(Me.CurrentTarget) && StyxWoW.Me.FrostRuneCount < 1 && StyxWoW.Me.UnholyRuneCount < 2 && StyxWoW.Me.DeathRuneCount < 1 && !Buff.UnitHasHasteBuff(Me), "Empower Rune Weapon"),
                               Spell.CastSpell("Death Siphon",                ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.DeathSiphonPercent && CLUSettings.Instance.EnableMovement, "Death Siphon"), // Only if movement is enabled..i.e. we are questing, gathering, etc
                               // Conversion lasts until canceled!!..I am reluctant to put this in atm as it seems underpowered
                               // Buff.CastBuff("Conversion",                 ret => (Buff.PlayerHasBuff("Unholy Presence") && Buff.PlayerHasBuff("Anti-Magic Shell")/*Since conversion stops extra rune regen from Frost Presence but not from AMS we will go this way only for Unholy Dks*/), "Conversion (Restoring 3% HP every 1s for 10RP"),//Tricky One
                               Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < CLUSettings.Instance.DeathKnight.HealthstonePercent, "HealthstonePercent"))));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                               new PrioritySelector(
                                   Buff.CastBuff("Bone Shield",        ret => (Buff.PlayerCountBuff("Bone Shield") < 2 || !Buff.PlayerHasBuff("Bone Shield")) && (CLUSettings.Instance.DeathKnight.UseBoneShieldDefensively || CLUSettings.Instance.DeathKnight.UseBoneShieldonCooldown), "Bone Shield"),
                                   Buff.CastRaidBuff("Horn of Winter", ret => CLUSettings.Instance.DeathKnight.UseHornofWinter && Me.CurrentTarget != null && !Me.CurrentTarget.IsFriendly, "Horn of Winter"),
                                   Buff.CastBuff("Blood Presence", ret => !Me.HasMyAura("Blood Presence"), "Blood Presence We need it!"))));
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