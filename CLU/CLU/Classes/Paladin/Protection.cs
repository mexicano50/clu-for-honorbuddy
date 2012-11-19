// $Author$
// $Date$
// $Id$
// $HeadURL$
// $Revision$

using System.Linq;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Paladin
{
    using global::CLU.Managers;

    class Protection : RotationBase
    {
        public override string Name
        {
            get {
                return "Protection Paladin";
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
                return "Avenger's Shield";
            }
        }
        public override int KeySpellId
        {
            get { return 31935; }
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
                return
                    @"
----------------------------------------------------------------------
Protection MoP:
[*] DamageReductionDebuff checks for Weakened Blows 
[*] Removed Inqisition
[*] Added Holy Prism and Light's Hammer for AoE
[*] Suggested talent choice is Sacred Shield
[*] Added Holy Avenger for ShoR
[*] Added Divine Purpose proc to ShoR or WoG
[*] Execution Sentence on the boss for threat..I know you can use it to self heal..soon 
This Rotation will:
1. Heal using Divine Protection, Lay on Hands, Divine Shield and Hand of Protection
	==> Word of glory & Guardian of Ancient Kings
2. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Avenging Wrath & Lifeblood
3. Seal of Truth & Seal of Truth swapping for low mana 
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
----------------------------------------------------------------------" + TalentManager.HasGlyph("Consecration");
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

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Buff.CastBuff("Avenging Wrath", ret => true, "Avenging Wrath"),
                                   Item.UseEngineerGloves())),

                           //Main Rotation
                           Spell.CastSpell("Hammer of the Righteous",     ret => Me.CurrentTarget != null && !Buff.UnitHasWeakenedBlows(Me.CurrentTarget), "Hammer of the Righteous for Weakened Blows"),
                           Buff.CastBuff("Holy Shield",                   ret => true, "Holy Shield"),
                           Buff.CastBuff("Sacred Shield",                 ret => true, "Sacred Shield"),
                           //Spell.CastInterupt("Rebuke",                   ret => true, "Rebuke"),
                           //Healing TODO: Check for Shield of the Righteous damage reduction buff and ensure we maintain 100% uptime.
                           Spell.CastSpell("Shield of the Righteous",     ret => (Me.CurrentHolyPower >= 3 && Buff.PlayerHasBuff("Sacred Duty")) || Buff.PlayerHasBuff("Divine Purpose"), "Shield of the Righteous Proc"),
                           Spell.CastSpell("Shield of the Righteous",     ret => Me.HealthPercent < CLUSettings.Instance.Paladin.ShoRPercent || (Me.CurrentHolyPower == 5), "Shield of the Righteous"),
                           // TODO: Shield of the Righteous damage reduction buff is all and well but we need to prioritise if we are getting hit with a shit load of magic damage, in this instance we can forsake ShoR and simply poor our holy power into Word of Glory.
                           Spell.CastSpell("Word of Glory",               ret=> Me,   ret => Me.HealthPercent < CLUSettings.Instance.Paladin.WordofGloryPercent && (Me.CurrentHolyPower > 1 || Buff.PlayerHasBuff("Divine Purpose")), "Word of Glory"),
                            // AOE
                           Spell.CastAreaSpell("Hammer of the Righteous", 8, false, CLUSettings.Instance.Paladin.ProtectionHoRCount, 0.0, 0.0, ret => true, "Hammer of the Righteous"),
                           Spell.CastSpell("Holy Prism", ret => Me.HealthPercent < CLUSettings.Instance.Paladin.HolyPrismPercent && Me.CurrentTarget != null && Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.HolyPrismCount, "Holy Prism"),
                           Spell.CastSpell("Light's Hammer", ret => Me.HealthPercent > CLUSettings.Instance.Paladin.HolyPrismPercent && Me.CurrentTarget != null && Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.LightsHammerCount, "Light's Hammer"),
                           // end AoE
                           Spell.CastSpell("Crusader Strike",             ret => true, "Crusader Strike"),
                           
                           Spell.CastSpell("Avenger's Shield",            ret => Buff.PlayerHasBuff("Grand Crusader"), "Avengers Shield Proc"),
                           Spell.CastSpell("Judgment",                    ret => true, "Judgment"),
                           Spell.CastSpell("Avenger's Shield",            ret => true, "Avengers Shield"),
                           Spell.CastSpell("Hammer of Wrath",             ret => Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent < 20, "Hammer of Wrath"),
                           Spell.CastSpell("Execution Sentence",          ret => Me.CurrentTarget != null && Unit.UseCooldowns(), "Execution Sentence"),
                           Spell.CastOnGround("Consecration", ret => Me.CurrentTarget.Location, a => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > CLUSettings.Instance.Paladin.ConsecrationManaPercent && TalentManager.HasGlyph("Consecration")),
                           Spell.CastSpell("Consecration",                ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > CLUSettings.Instance.Paladin.ConsecrationManaPercent && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Me.IsWithinMeleeRange && Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.ConsecrationCount && !TalentManager.HasGlyph("Consecration"), "Consecration"),
                           Spell.CastSpell("Holy Wrath",                  ret => true, "Holy Wrath")
                          );
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    new DecoratorContinue(ret => Me.CurrentTarget != null && !Me.IsSafelyFacing(Me.CurrentTarget, 45f), new Action(ret => Me.CurrentTarget.Face())),
                    this.SingleRotation);
            }
        }

        public override Composite Medic
        {
            get {
                return new PrioritySelector(
                    new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               new Decorator(
                                   ret => !Buff.PlayerHasBuff("Forbearance") && !Buff.PlayerHasBuff("Ardent Defender") && !Buff.PlayerHasBuff("Guardian of Ancient Kings"),
                                   new PrioritySelector(
                                       Buff.CastBuff("Lay on Hands",           ret => Me.HealthPercent < CLUSettings.Instance.Paladin.ProtectionLoHPercent, "Lay on Hands"),
                                       Buff.CastBuff("Divine Shield",          ret => Me.HealthPercent < CLUSettings.Instance.Paladin.ProtectionDSPercent, "Divine Shield")
                                   )),
                               Buff.CastBuff("Holy Avenger",               ret => Me.HealthPercent < CLUSettings.Instance.Paladin.HolyAvengerHealthProt && !Buff.PlayerHasBuff("Ardent Defender") && !Buff.PlayerHasBuff("Guardian of Ancient Kings"), "Holy Avenger"),
                               Buff.CastBuff("Guardian of Ancient Kings",  ret => Me.HealthPercent < CLUSettings.Instance.Paladin.GuardianofAncientKingsHealthProt && !Buff.PlayerHasBuff("Ardent Defender"), "Guardian of Ancient Kings"),
                               Buff.CastBuff("Ardent Defender",            ret => Me.HealthPercent < CLUSettings.Instance.Paladin.ArdentDefenderHealth && !Buff.PlayerHasBuff("Guardian of Ancient Kings"), "Ardent Defender"),
                               Item.UseBagItem("Healthstone",              ret => Me.HealthPercent < CLUSettings.Instance.Paladin.HealthstonePercent, "Healthstone"),
                               Buff.CastBuff("Divine Protection",          ret => Me.HealthPercent < CLUSettings.Instance.Paladin.ProtectionDPPercent, "Divine Protection"))),
                    // Seal Swapping for AoE
                    Buff.CastBuff("Seal of Righteousness",  ret => Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.SealofRighteousnessCount, "Seal of Righteousness"),
                    Buff.CastBuff("Seal of Truth",          ret => Me.ManaPercent > CLUSettings.Instance.Paladin.SealofInsightMana && Unit.EnemyMeleeUnits.Count() < CLUSettings.Instance.Paladin.SealofRighteousnessCount, "Seal of Truth"),
                    Buff.CastBuff("Seal of Insight",        ret => Me.ManaPercent < CLUSettings.Instance.Paladin.SealofInsightMana && !Buff.PlayerHasBuff("Seal of Righteousness"), "Seal of Insight for Mana Gen")
                    
                    );
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastBuff("Righteous Fury",          ret => true, "Righteous Fury"),
                               Buff.CastRaidBuff("Blessing of Kings",   ret => true, "[Blessing] of Kings"),
                               Buff.CastRaidBuff("Blessing of Might",   ret => true, "[Blessing] of Might")));
            }
        }

        public override Composite Resting
        {
            get
            {
                return
                    new PrioritySelector(
                        Spell.CastSpell("Flash of Light", ret => Me, ret => Me.HealthPercent < CLUSettings.Instance.Paladin.FlashHealRestingPercent && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "Flash of Light on me"),
                        Rest.CreateDefaultRestBehaviour());
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