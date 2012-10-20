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

        // Commit this shit...CLU svn doesn't like me :(

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
                        ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                            Item.UseBagItem("Golemblood Potion", ret => Buff.UnitHasHasteBuff(Me), "Golemblood Potion Heroism/Bloodlust"),
                            Item.UseEngineerGloves())),
                    // Interupt
                    Spell.CastInterupt("Rebuke",                                ret => CLU.LocationContext != GroupLogic.Battleground, "Rebuke"),
                    Spell.CastInterupt("Rebuke",                                ret => Unit.MeleePvPUnits.OrderBy(u => Me.IsFacing(u) && u.IsCasting && u.CastingSpell.CooldownTimeLeft > TimeSpan.FromMilliseconds(500) && (u.CanInteruptCastSpell() || u.CanInteruptChannelSpell())).FirstOrDefault(), ret => CLU.LocationContext == GroupLogic.Battleground, "Rebuke"),
                    // Threat
                    Buff.CastBuff("Hand of Salvation",                          ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && Unit.IsInGroup, "Hand of Salvation"),
                    //Inquisition
                    Buff.CastBuff("Inquisition", ret => Me.CurrentHolyPower>2 && Buff.GetAuraTimeLeft(Me, "Inquisition", true).TotalSeconds < 2, "Inquisition (Buff)"),
                    new Decorator(ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                        new PrioritySelector(
                                Spell.CastSpell("Guardian of Ancient Kings",    ret => Me.HasMyAura("Inquisition"), "GoAK on Boss"),
                                Buff.CastBuff("Avenging Wrath", ret => Me.HasMyAura("Ancient Power") && Buff.GetAuraTimeLeft(Me, "Guardian of Ancient Kings", true).TotalSeconds < 21, "Avenging Wrath with 20 seconds on GoAK"),
                                Buff.CastBuff("Avenging Wrath",                 ret => Me.HasMyAura(86700) && Buff.GetAuraStack(Me, 86700, true) >= 10, "Avenging Wrath with 10 stacks of Ancient Power"),
                                Buff.CastBuff("Holy Avenger",                   ret => Me.HasMyAura("Avenging Wrath"), "Holy Avenger with Avenging Wrath"),
                                Spell.CastSpell("Holy Prism",                   ret => true, "Holy Prism"))),
                    new Decorator(ret => Unit.EnemyMeleeUnits.Count() > 2 && CLUSettings.Instance.UseAoEAbilities,
                        new PrioritySelector(
                            Spell.CastSpell("Divine Storm", ret => Me.HasMyAura("Inquisition") && Buff.GetAuraTimeLeft(Me, "Inquisition", true).TotalSeconds >= 2 && (Me.CurrentHolyPower == 5 || Me.HasMyAura(90174)), "Divine Storm with 5 HP"),
                            Spell.CastSpell("Execution Sentence", ret => Me.HasMyAura("Inquisition"), "Execution Sentence"),
                            Spell.CastSpell("Light's Hammer", ret => Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.RetributionLightsHammerCount && Me.HasMyAura("Inquisition") && CLUSettings.Instance.UseAoEAbilities, "Light's Hammer"),
                            Spell.CastSpell("Hammer of Wrath",                  ret => true, "Hammer of Wrath on < 20% HP target"),
                            Spell.CastSpell("Exorcism",                         ret => Me.CurrentHolyPower < 5 || (Me.HasMyAura(59578) && Me.CurrentHolyPower < 5), "Excorcism to generate Holy Power"),
                            Spell.CastSpell("Hammer of the Righteous",          ret => Me.CurrentHolyPower < 5 && !Me.HasMyAura(59578), "Hammer of the Righteous to generate Holy Power"),
                            Spell.CastSpell("Judgment",                         ret => Me.CurrentHolyPower < 5 && !Me.HasMyAura(59578) ,"Judgment to generate Holy Power"),
                            Spell.CastSpell("Divine Storm", ret => Me.HasMyAura("Inquisition") && Buff.GetAuraTimeLeft(Me, "Inquisition", true).TotalSeconds >=2 && Me.CurrentHolyPower >= 3 || Me.HasMyAura(90174), "Divine Storm with 3+ HP"))),
                    new Decorator(ret => Unit.EnemyMeleeUnits.Count() <= 2 || !CLUSettings.Instance.UseAoEAbilities,
                        new PrioritySelector(
                            Spell.CastSpell("Templar's Verdict", ret => Me.HasMyAura("Inquisition") && Buff.GetAuraTimeLeft(Me, "Inquisition", true).TotalSeconds >= 2 && Me.CurrentHolyPower == 5 || Me.HasMyAura(90174), "Templar's Verdict with 5 HP"),
                            Spell.CastSpell("Execution Sentence", ret => Me.HasMyAura("Inquisition"), "Execution Sentence"),
                            Spell.CastSpell("Light's Hammer", ret => Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.RetributionLightsHammerCount && Me.HasMyAura("Inquisition") && CLUSettings.Instance.UseAoEAbilities, "Light's Hammer"),
                            Spell.CastSpell("Hammer of Wrath",                  ret => true, "Hammer of Wrath on < 20% HP target"),
                            Spell.CastSpell("Exorcism",                         ret => Me.CurrentHolyPower < 5 || (Me.HasMyAura(59578) && Me.CurrentHolyPower < 5), "Excorcism to generate Holy Power"),
                            Spell.CastSpell("Crusader Strike",                  ret => Me.CurrentHolyPower < 5 && !Me.HasMyAura(59578), "Crusader Strike to generate Holy Power"),
                            Spell.CastSpell("Judgment",                         ret => Me.CurrentHolyPower < 5 && !Me.HasMyAura(59578), "Judgment to generate Holy Power"),
                            Spell.CastSpell("Templar's Verdict", ret => Me.HasMyAura("Inquisition") && Me.CurrentHolyPower >= 3 && Buff.GetAuraTimeLeft(Me, "Inquisition", true).TotalSeconds >= 2 || Me.HasMyAura(90174), "Templar's Verdict with 3+ HP"))),
                    Buff.CastBuff("Sacred Shield", ret => !Me.HasMyAura("Sacred Shield"), "Sacred Shield as a filler"),
                    Spell.CastSelfSpell("Flash of Light",                       ret => Me.HealthPercent < 100 && Me.HasMyAura("Selfless Healer") && Buff.GetAuraStack(Me, "Selfless Healer", true) == 3, "Flash of Light with 3 stacks of Selfless Healer"),
                    Spell.CastSelfSpell("Arcane Torrent",                       ret => Me.ManaPercent < 80 && Me.CurrentHolyPower < 3, "Arcane Torrent"));
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
                    new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSpell("Word of Glory",            ret => Me, ret => Me.HealthPercent < CLUSettings.Instance.Paladin.WordofGloryPercent && (Me.CurrentHolyPower > 1 || Buff.PlayerHasBuff("Divine Purpose")) && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "Word of Glory"),
                               Buff.CastBuff("Hand of Freedom",            ret => Me.MovementInfo.ForwardSpeed < 8.05 && CLUSettings.Instance.Paladin.UseHandofFreedom, "Hand of Freedom"),
                               Buff.CastBuff("Cleanse",                    ret => Unit.UnitIsControlled(Me, false), "Cleanse"),
                               Item.UseBagItem("Healthstone",              ret => Me.HealthPercent < CLUSettings.Instance.Paladin.HealthstonePercent, "Healthstone"),
                               Buff.CastBuff("Divine Protection",          ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionDPPercent, "Divine Protection"),
                               new Decorator(
                                   ret => !Buff.PlayerHasBuff("Forbearance"),
                                   new PrioritySelector(
                                       Buff.CastBuff("Lay on Hands",       ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionLoHPercent, "Lay on Hands"),
                                       Buff.CastBuff("Divine Shield", ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionDSPercent && !Me.IsCarryingFlag(), "Divine Shield"),
                                       Buff.CastBuff("Hand of Protection", ret => Me.HealthPercent < CLUSettings.Instance.Paladin.RetributionHoPPercent, "Hand of Protection"))))),
                    
                     // Seal Swapping for AoE
                     Buff.CastBuff("Seal of Righteousness", ret => Unit.EnemyMeleeUnits.Count() >= CLUSettings.Instance.Paladin.SealofRighteousnessCount, "Seal of Righteousness"),
                     Buff.CastBuff("Seal of Truth", ret => Unit.EnemyMeleeUnits.Count() < CLUSettings.Instance.Paladin.SealofRighteousnessCount, "Seal of Truth")
                    ); 
                    
                    
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
                        Spell.CastSpell("Flash of Light", ret => Me, ret => Me.HealthPercent < CLUSettings.Instance.Paladin.FlashHealRestingPercent && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement && !Me.IsMoving, "Flash of Light on me"),
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
