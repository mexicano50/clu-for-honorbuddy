using System.Linq;
using CLU.Helpers;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Paladin
{
    using System;

    class Retribution : RotationBase
    {
        private const int ItemSetId = 1064; // Tier set ID


        public override string Name
        {
            get {
                return "Retribution Paladin";
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
                return "Templar's Verdict";
            }
        }
        public override int KeySpellId
        {
            get { return 85256; }
        }
        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {

                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return
                    @"
----------------------------------------------------------------------
Retribution MoP:
[*] Zealotry replaced with Holy Avenger
[*] Guardian of Ancient and Avenging Wrath are now stacked
[*] Execution Sentence added
[*] consecration removed
[*] Holy Wrath removed
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Settings)
This Rotation will:
1. Heal using Divine Protection, Lay on Hands, Divine Shield and Hand of Protection
	==> Healthstone. Flash Heal if movement enabled.
2. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Holy Avenger, Guardian of Ancient Kings and Avenging Wrath
3. Seal of Righteousness & Seal of Truth swapping for AoE 
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
----------------------------------------------------------------------" + twopceinfo + "\n" + fourpceinfo + "\n";
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
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseBagItem("Golemblood Potion", ret => Buff.UnitHasHasteBuff(Me), "Golemblood Potion Heroism/Bloodlust"),
                                   Item.UseEngineerGloves())),
                           // Interupt
                           Spell.CastInterupt("Rebuke", ret => CLU.LocationContext != GroupLogic.Battleground, "Rebuke"),
                           Spell.CastInterupt("Rebuke", ret => Unit.EnemyHealer.OrderBy(u => u.IsCasting && u.CastingSpell.CooldownTimeLeft > TimeSpan.FromMilliseconds(500)).FirstOrDefault(), ret => CLU.LocationContext == GroupLogic.Battleground, "Rebuke"),
                           // Threat
                           Buff.CastBuff("Hand of Salvation",      ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Hand of Salvation"),
                           // Seal Swapping for AoE
                           Buff.CastBuff("Seal of Righteousness",  ret => Unit.EnemyUnits.Count() >= CLUSettings.Instance.Paladin.SealofRighteousnessCount, "Seal of Righteousness"),
                           Buff.CastBuff("Seal of Truth",          ret => Unit.EnemyUnits.Count() < CLUSettings.Instance.Paladin.SealofRighteousnessCount, "Seal of Truth"),
                           new Decorator(
                               ret => Buff.PlayerHasBuff("Holy Avenger"),
                               new PrioritySelector(
                                   // Cooldowns
                                   Buff.CastBuff("Guardian of Ancient Kings",      ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerHasBuff("Avenging Wrath") && Buff.PlayerHasBuff("Inquisition"), "Guardian of Ancient Kings"),
                                   Buff.CastBuff("Avenging Wrath",                 ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Inquisition") && Unit.IsTargetWorthy(Me.CurrentTarget), "Avenging Wrath"),
                                   Buff.CastBuff("Execution Sentence",             ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Inquisition") && Unit.IsTargetWorthy(Me.CurrentTarget), "Execution Sentence"),
                                   // Holy Avenger Rotation
                                   Spell.CastSelfSpell("Inquisition",               ret => (!Buff.PlayerHasBuff("Inquisition") || Buff.PlayerBuffTimeLeft("Inquisition") <= 2) && (Me.CurrentHolyPower >= 3 || Buff.PlayerHasBuff("Divine Purpose")), "Inquisition"),
                                   Spell.CastAreaSpell("Hammer of the Righteous", 8, false, CLUSettings.Instance.Paladin.RetributionHoRCount, 0.0, 0.0, ret => Me.CurrentHolyPower <= 5, "Hammer of the Righteous"),
                                   Spell.CastSpell("Hammer of Wrath",               ret => Me.CurrentTarget != null && (Buff.PlayerHasBuff("Avenging Wrath") || Me.CurrentTarget.HealthPercent <= 20), "Hammer of Wrath"), Spell.CastAreaSpell("Divine Storm", 10, false, CLUSettings.Instance.Paladin.DivineStormCount, 0.0, 0.0, ret => (Buff.PlayerHasBuff("Divine Purpose") || Me.CurrentHolyPower >= 3), "Divine Storm"),
                                   Spell.CastSpell("Templar's Verdict",             ret => (Buff.PlayerHasBuff("Divine Purpose") || Me.CurrentHolyPower == 5), "Templar's Verdict"),
                                   Spell.CastSpell("Exorcism",                      ret => true, "Exorcism"),
                                   Spell.CastSpell("Crusader Strike",               ret => Me.CurrentHolyPower < 5, "Crusader Strike"),
                                   Spell.CastSpell("Judgment",                      ret => Me.CurrentHolyPower < 5, "Judgment"),
                                   Spell.CastSpell("Templar's Verdict",             ret => (Buff.PlayerHasBuff("Divine Purpose") || Me.CurrentHolyPower >= 3), "Templar's Verdict"),
                                   Spell.CastSelfSpell("Arcane Torrent",            ret => Me.ManaPercent < 80 && Me.CurrentHolyPower < 3, "Arcane Torrent"),
                                   Buff.CastBuff("Divine Plea",                     ret => Me.ManaPercent < 75 && Me.CurrentHolyPower < 3, "Divine Plea"))),
                           new Decorator(
                               ret => !Buff.PlayerHasBuff("Holy Avenger"),
                               new PrioritySelector(
                                   // Cooldowns
                                   Buff.CastBuff("Holy Avenger",                      ret => Me.CurrentTarget != null && (Me.CurrentHolyPower >= 3 || Buff.PlayerHasBuff("Divine Purpose")) && Unit.IsTargetWorthy(Me.CurrentTarget), "Holy Avenger"),
                                   // Main Rotation
                                   Spell.CastSelfSpell("Inquisition",               ret => (!Buff.PlayerHasBuff("Inquisition") || Buff.PlayerBuffTimeLeft("Inquisition") <= 2) && (Me.CurrentHolyPower >= 3 || Buff.PlayerHasBuff("Divine Purpose")), "Inquisition"),
                                   Spell.CastAreaSpell("Hammer of the Righteous", 8, false, CLUSettings.Instance.Paladin.RetributionHoRCount, 0.0, 0.0, ret => Me.CurrentHolyPower <= 5, "Hammer of the Righteous"),
                                   Spell.CastAreaSpell("Divine Storm", 10, false, CLUSettings.Instance.Paladin.DivineStormCount, 0.0, 0.0, ret => (Buff.PlayerHasBuff("Divine Purpose") || Me.CurrentHolyPower >= 3), "Divine Storm"),
                                   Spell.CastSpell("Templar's Verdict",             ret => (Buff.PlayerHasBuff("Divine Purpose") || Me.CurrentHolyPower == 5), "Templar's Verdict"), 
                                   Spell.CastSpell("Hammer of Wrath",               ret => Me.CurrentTarget != null && (Buff.PlayerHasBuff("Avenging Wrath") || Me.CurrentTarget.HealthPercent <= 20), "Hammer of Wrath"),
                                   Spell.CastSpell("Exorcism",                      ret => true, "Exorcism"),
                                   Spell.CastSpell("Crusader Strike",               ret => Me.CurrentHolyPower < 5, "Crusader Strike"),
                                   Spell.CastSpell("Judgment",                      ret => !Buff.PlayerHasBuff("Holy Avenger") && Me.CurrentHolyPower < 5, "Judgment"),
                                   Spell.CastSpell("Templar's Verdict",             ret => (Buff.PlayerHasBuff("Divine Purpose") || Me.CurrentHolyPower >= 3), "Templar's Verdict"), 
                                   Spell.CastSelfSpell("Arcane Torrent",            ret => Me.ManaPercent < 80 && Me.CurrentHolyPower < 3, "Arcane Torrent"),
                                   Buff.CastBuff("Divine Plea",                     ret => Me.ManaPercent < 75 && Me.CurrentHolyPower < 3, "Divine Plea"))));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Buff.CastBuff("Hand of Freedom",            ret => Me.MovementInfo.ForwardSpeed < 8.05 && CLUSettings.Instance.Paladin.UseHandofFreedom, "Hand of Freedom"),
                               Buff.CastBuff("Cleanse",                    ret => Unit.UnitIsControlled(Me, false), "Cleanse"),
                               Item.UseBagItem("Healthstone",              ret => Me.HealthPercent < CLUSettings.Instance.Paladin.HealthstonePercent, "Healthstone"),
                               Buff.CastBuff("Divine Protection",          ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionDPPercent, "Divine Protection"),
                               new Decorator(
                                   ret => !Buff.PlayerHasBuff("Forbearance"),
                                   new PrioritySelector(
                                       Buff.CastBuff("Lay on Hands",       ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionLoHPercent, "Lay on Hands"),
                                       Buff.CastBuff("Divine Shield",      ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionDSPercent, "Divine Shield"),
                                       Buff.CastBuff("Hand of Protection", ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionHoPPercent, "Hand of Protection")))));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastBuff("Seal of Truth",             ret => !Buff.PlayerHasBuff("Seal of Truth"), "Seal of Truth"),
                               Buff.CastRaidBuff("Blessing of Kings",     ret => true, "[Blessing] of Kings"),
                               Buff.CastRaidBuff("Blessing of Might",     ret => true, "[Blessing] of Might")));
            }
        }

        public override Composite Resting
        {
            get {
                return
                    new PrioritySelector(
                        Spell.CastSpell("Flash Heal", ret => Me, ret => Me.HealthPercent < CLUSettings.Instance.Paladin.FlashHealRestingPercent && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "flash heal on me"),
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
