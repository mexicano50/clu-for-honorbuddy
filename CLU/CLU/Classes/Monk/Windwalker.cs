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

using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using System.Collections.Generic;
using CLU.Base;
using CLU.Managers;
using Styx.CommonBot;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Monk
{
    using Styx;

    internal class Windwalker : RotationBase
    {
        public override string Name
        {
            get { return "Windwalker Monk"; }
        }

        public override string Revision
        {
            get { return "$Rev$"; }
        }

        public override string KeySpell
        {
            get { return "Fists of Fury"; }
        }

        public override int KeySpellId
        {
            get { return 113656; }
        }

        public override float CombatMaxDistance
        {
            get { return 3.2f; }
        }

        public override string Help
        {
            get
            {
                return "----------------------------------------------------------------------\n" +
                       "Credits: alxaw , Kbrebel04" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static uint Chi
        {
            get
            {
                return Me.CurrentChi; // StyxWoW.Me.GetCurrentPower(WoWPowerType.LightForce);
            }
        }

        // TODO: CHECK COMBO BREAKER NAMES.
        // TODO: CHECK CHI
        // TODO: FIND AWAY TO RETURN ENERGY REGEN RATE
        // TODO: CHECK ALL SPELL NAMES FROM "SPELLS" DUMP
        // TODO: CHECK ALL AURAS
        // TODO: CHECK JAB IS NOT AFFECTED BY THE WEAPON YOU ARE CARRYING AND WE ONLY NEED TO USE JAB AND THE SPELLID AND ICON WILL CHANGE.


        private static readonly List<string> JabSpellList = new List<string> { "Jab", "Club", "Slice", "Sever", "Pike", "Clobber" };

        private static bool RisingSunKickCoolDown
        {
            get { return Spell.SpellCooldown("Rising Sun Kick").TotalSeconds > 40.0; }
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
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                            Item.UseEngineerGloves())),
                    //Single Target
                    Spell.CastSelfSpell("Tigereye Brew",
                                        ret =>
                                        !Buff.PlayerHasBuff("Tigereye Brew Use") &&
                                        Buff.PlayerCountBuff("Tigereye Brew") == 10 && CLUSettings.Instance.UseCooldowns,
                                        "Tigerye Brew"),
                    //Spell.CastSpell("Chi Brew", ret => TalentManager.HasTalent(9) && Buff.PlayerHasBuff("Tigereye Brew Use" && Chi == 0 && Me.CurrentEnergy <= 50, "Chi Brew"),     is not identifying "tigereye brew use" buff
                    Spell.CastSpell("Energizing Brew", ret => Me.CurrentEnergy <= 30, "Energizing Brew"),
                    Spell.CastSpell("Touch of Death", ret => Buff.PlayerHasBuff("Death Note"), "Touch of Death"),
                    Spell.CastSpell("Expel Harm", ret => Me.HealthPercent < 80 && Me.CurrentEnergy >= 40 && Chi <= 2,
                                    "Expel Harm"),
                    Spell.CastSpell("Rising Sun Kick",
                                    ret =>
                                    !Buff.TargetHasDebuff("Rising Sun Kick") ||
                                    Buff.PlayerBuffTimeLeft("Tiger Power") < 5 &&
                                    Buff.PlayerBuffTimeLeft("Tiger Power") > 3 && Me.CurrentEnergy >= 40 ||
                                    Buff.PlayerBuffTimeLeft("Tiger Power") >= 5, "Rising Sun Kick"),
                    Spell.CastSpell("Tiger Palm",
                                    ret =>
                                    Buff.PlayerCountBuff("Tiger Power") < 3 ||
                                    Buff.PlayerBuffTimeLeft("Tiger Power") <= 10, "Tiger Palm"),
                    Spell.CastSelfSpell("Invoke Xuen, the White Tiger",
                                        ret =>
                                        TalentManager.HasTalent(17) && Buff.PlayerCountBuff("Tiger Power") == 3 &&
                                        Buff.TargetHasDebuff("Rising Sun Kick") && Me.CurrentEnergy <= 80, "Invoke Xuen"),
                    Spell.CastSpell("Rushing Jade Wind",
                                    ret =>
                                    TalentManager.HasTalent(16) && Buff.PlayerCountBuff("Tiger Power") == 3 &&
                                    Buff.TargetHasDebuff("Rising Sun Kick") && Me.CurrentEnergy <= 80,
                                    "Rushing Jade Wind"),
                    Spell.CastSpell("Fists of Fury",
                                    ret =>
                                    !Me.IsMoving && !Buff.PlayerHasActiveBuff("Energizing Brew") &&
                                    Me.CurrentEnergy <= 50 && Buff.PlayerBuffTimeLeft("Tiger Power") > 5 &&
                                    Buff.PlayerCountBuff("Tiger Power") == 3 &&
                                    Spell.SpellCooldown("Rising Sun Kick").TotalSeconds >= 2, "Fists of Fury"),
                    Spell.CastSpell("Blackout Kick", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Blackout Kick"),
                                    "Blackout Kick"),
                    Spell.CastSpell("Tiger Palm",
                                    ret =>
                                    (Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") && Me.CurrentEnergy < 90) ||
                                    (Buff.PlayerBuffTimeLeft("Combo Breaker: Tiger Palm") < 2 &&
                                     Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm")), "Tiger Palm"),
                    Spell.CastSpell("Blackout Kick",
                                    ret =>
                                    Chi >= 2 && Spell.SpellOnCooldown("Fists of Fury") &&
                                    SpellManager.HasSpell("Rising Sun Kick") &&
                                    SpellManager.Spells["Rising Sun Kick"].CooldownTimeLeft.Seconds >= 2 ||
                                    Chi >= 2 && Spell.SpellOnCooldown("Fists of Fury") &&
                                    !SpellManager.HasSpell("Rising Sun Kick") ||
                                    Chi >= 3 && Me.CurrentEnergy <= 50 && Me.IsMoving &&
                                    !Spell.SpellOnCooldown("Fists of Fury") && SpellManager.HasSpell("Rising Sun Kick") &&
                                    SpellManager.Spells["Rising Sun Kick"].CooldownTimeLeft.Seconds >= 2 ||
                                    Chi >= 3 && Me.CurrentEnergy > 50 && SpellManager.HasSpell("Rising Sun Kick") &&
                                    SpellManager.Spells["Rising Sun Kick"].CooldownTimeLeft.Seconds >= 2,
                                    "Blackout Kick"),
                    //Spell.CastSpell("Jab", ret => TalentManager.HasTalent(8) && Chi <= 3, "Jab"), is not identifying the talent, is using jab at 3 chi without the talent
                    Spell.CastSpell("Expel Harm", ret => Chi <= 2 && Me.HealthPercent <= 80, "Expel Harm"),
                    Spell.CastSpell("Jab",
                                    ret =>
                                    Chi <= 2 && Me.HealthPercent > 80 ||
                                    Chi <= 2 && Spell.SpellOnCooldown("Expel Harm") && Me.HealthPercent <= 80, "Jab"),
                    //Spell.CastSpell("Blackout Kick", ret => (Me.CurrentEnergy > 30 && RisingSunKickCoolDown) || (Chi > 4 && Buff.PlayerHasActiveBuff("Ascension")) || (Chi > 5 && Buff.PlayerHasActiveBuff("Ascension")), "Blackout Kick"),

                    // Interupt
                    Spell.CastInterupt("Spear Hand Strike", ret => true, "Spear Hand Strike"),
                    // AoE
                    Spell.CastAreaSpell("Fists of Fury", 8, false, 3, 0.0, 0.0, ret => true, "Fists of Fury"),
                    Spell.CastAreaSpell("Spinning Crane Kick", 8, false, 3, 0.0, 0.0, ret => true, "Spinning Crane Kick"),
                    Spell.CastSpell(JabSpellList.Find(SpellManager.CanCast), ret => true, "JabSpell"));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Buff.CastBuff("Fortifying Brew", ret => Me.HealthPercent < 50, "Fortifying Brew"),
                    // Turns your skin to stone, increasing your health by 20%, and reducing damage taken by 20%. Lasts 20 sec.
                        Buff.CastBuff("Guard", ret => Me.HealthPercent < 50, "Guard"),
                    // absorbs damage for 30secs and increases any healing by 30%
                        Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                    ret =>
                    !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") &&
                    !Me.HasAura("Drink"),
                    new PrioritySelector(
                        Buff.CastRaidBuff("Legacy of the Emperor", ret => true, "Legacy of the Emperor"),
                        Buff.CastRaidBuff("Legacy of the White Tiger", ret => true, "Legacy of the White Tiger")));
            }
        }

        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}