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
                       "to be done\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }



        //Storm for your sanity I have put this here
        /*[SpellManager] Spell Lock (119910) overrides Command Demon (119898)
            [SpellManager] Dark Soul: Misery (113860) overrides Dark Soul (77801)
            [SpellManager] Malefic Grasp (103103) overrides Shadow Bolt (686)
            [SpellManager] Soul Link (108415) overrides Health Funnel (755)*/
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
                        ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                            Item.UseEngineerGloves())),
                            //Buffs
                    // Threat
                    Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Soulshatter"),                            
                    Buff.CastBuff("Dark Intent", ret => true, "Dark Intent"),
                    //Call Pet
                    new Decorator(ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Soulburn")) && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503),
                        new Sequence(
                            Buff.CastBuff("Soulburn", ret => !Me.ActiveAuras.ContainsKey("Soulburn"), "Soulburn for Pet"),
                            new WaitContinue(new System.TimeSpan(0, 0, 0, 0, 50), ret => false, new ActionAlwaysSucceed()),
                            Spell.CreateWaitForLagDuration(),
                            PetManager.CastPetSummonSpell(691, ret => true, "Summon Pet"))),
                            Common.WarlockGrimoire,
                    //Cooldowns
                    new Decorator(ret=> CLUSettings.Instance.UseCooldowns,
                        new PrioritySelector(
                            Buff.CastBuff("Dark Soul", ret => !WoWSpell.FromId(113860).Cooldown && Me.CurrentTarget != null && !Me.IsMoving && !Me.HasAnyAura(Common.DarkSoul), "Dark Soul"),
                            Spell.CastSpell("Summon Doomguard", ret => !WoWSpell.FromId(18540).Cooldown && Me.CurrentTarget != null, "Summon Doomguard"),
                            Spell.CastSelfSpell("Unending Resolve", ret => Me.CurrentTarget != null && Me.HealthPercent < 40, "Unending Resolve (Save my life)"),
                            Spell.CastSelfSpell("Twilight Warden", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Me.HealthPercent < 80, "Twilight Warden (Protect me from magical damage)"))),
                    //Basic DPSing
                    Buff.CastDebuff("Curse of the Elements", ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent > 70 && !Buff.UnitHasMagicVulnerabilityDeBuffs(Me.CurrentTarget), "Curse of the Elements"),
                    //fast application of debuffs
                    new Decorator(ret => Me.CurrentTarget != null && !Me.CurrentTarget.HasAnyAura(Common.Debuffs),
                        new PrioritySelector(
                    Buff.CastBuff("Soulburn", ret => !Me.HasAura("Soulburn"), "Soulburn"),
                    Spell.CastSpell("Soul Swap", ret => Me.HasAura("Soulburn"), "Soul Swap"))),
                    //Slow Application
                    Buff.CastDebuff("Agony", ret => !Me.HasAura("Soulburn"), "Agony"), //Should never met in regular situations
                    Buff.CastDebuff("Corruption", ret => !Me.HasAura("Soulburn"), "Corruption"), //Should never met in regular situations
                    Buff.CastDebuff("Unstable Affliction", ret => !Me.HasAura("Soulburn"), "Unstable Affliction"),
                    //Basic DPSing
                    Buff.CastDebuff("Haunt", ret => !Spell.PlayerIsChanneling  , "Haunt"),
                    Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 20 && Me.HealthPercent > 40, "Life Tap"),
                    Spell.CastSpell("Fel Flame", ret => Me.IsMoving, "Fel flame while moving"),
                    new Decorator(ret=> CLUSettings.Instance.EnableSelfHealing && Me.CurrentHealth <= 70, Common.WarlockTierOneTalents),
                    Spell.ChannelSpell("Shadow Bolt", ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent >= 20 && Me.GetPowerInfo(Styx.WoWPowerType.SoulShards).CurrentI > 0) && !Me.IsMoving && !Spell.PlayerIsChanneling, "Malefic Grasp"),
                    Spell.ChannelSpell("Drain Soul", ret => Me.CurrentTarget != null && (Me.CurrentTarget.HealthPercent < 20 || Me.GetPowerInfo(Styx.WoWPowerType.SoulShards).CurrentI == 0) && !Me.IsMoving && !Spell.PlayerIsChanneling, "Drain Soul"));
            }
        }

        public override Composite Pull
        {
             get { return this.SingleRotation; }
        }

        public override Composite Medic
        {
            get { return Common.WarlockMedic; }
        }

        public override Composite PreCombat
        {
            get { return Common.WarlockPreCombat; }
        }

        public override Composite Resting
        {
            get {return Rest.CreateDefaultRestBehaviour();}
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation;}
        }

        public override Composite PVERotation
        {
            get {return this.SingleRotation;}
        }
    }
}