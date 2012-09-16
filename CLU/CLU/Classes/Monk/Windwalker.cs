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

    class Windwalker : RotationBase
    {
        public override string Name
        {
            get
            {
                return "Windwalker Monk";
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
            get
            {
                return "Fists of Fury";
            }
        }
        public override int KeySpellId
        {
            get { return 0; }
        }
        public override float CombatMaxDistance
        {
            get
            {
                return 3.2f;
            }
        }

        public override string Help
        {
            get
            {
                return "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. \n" +
                       "==> \n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> \n" +
                       "3. \n" +
                       "4. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits: alxaw for extending the basic rotation" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static uint Chi
        {
            get
            {
                return StyxWoW.Me.GetCurrentPower(WoWPowerType.LightForce);    // Me.Chi
            }
        }

        // TODO: CHECK COMBO BREAKER NAMES.
        // TODO: CHECK CHI
        // TODO: FIND AWAY TO RETURN ENERGY REGEN RATE
        // TODO: CHECK ALL SPELL NAMES FROM "SPELLS" DUMP
        // TODO: CHECK ALL AURAS
        // TODO: CHECK JAB IS NOT AFFECTED BY THE WEAPON YOU ARE CARRYING AND WE ONLY NEED TO USE JAB AND THE SPELLID AND ICON WILL CHANGE.


        private static readonly List<string> JabSpellList = new List<string> { "Jab", "Club", "Slice", "Sever", "Pike", "Clobber" };
        private static bool RisingSunKickCoolDown { get { return Spell.SpellCooldown("Rising Sun Kick").TotalSeconds > 40.0; } }

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
                           Spell.CastSpell("Chi Sphere", ret => TalentManager.HasTalent(7) && Chi <= 4, "Chi Sphere"),
                           Spell.CastSpell("Chi Brew", ret => TalentManager.HasTalent(9) && Chi < 0, "Chi Brew"),
                           Spell.CastSpell("Rising Sun Kick", ret => !Buff.TargetHasDebuff("Rising Sun Kick") || Buff.TargetCountDebuff("Rising Sun Kick") < 3, "Rising Sun Kick"),
                           Spell.CastSpell("Tiger Plam", ret => Buff.PlayerCountBuff("Tiger Power") < 3 || Buff.PlayerBuffTimeLeft("Tiger Power") < 3, "Tiger Power"),
                           Spell.CastSpell("Tigereye Brew", ret => !Buff.PlayerHasBuff("Tigereye Brew Use") && Buff.PlayerCountBuff("Tigereye Brew") > 10 && CLUSettings.Instance.UseCooldowns, "Tigerye Brew"),
                           //Spell.CastSpell("Energizing Brew",   ret => Buff. //Get back to this later.
                           Spell.CastSelfSpell("Invoke Xuen, the White Tiger", ret => TalentManager.HasTalent(17), "Invoke Xuen"),
                           Spell.CastSpell("Rushing Jade Wind", ret => TalentManager.HasTalent(16), "Rushing Jade Wind"),
                           Spell.CastSpell("Fists of Fury", ret => !Buff.PlayerHasActiveBuff("Energizing Brew") && Me.CurrentEnergy > 80 && Buff.PlayerBuffTimeLeft("Tiger Power") > 4 && Buff.PlayerCountBuff("Tiger Power") > 3, "Fists of Fury"),
                           Spell.CastSpell("Blackout Kick", ret => Buff.PlayerHasActiveBuff("Combo Breaker"), "Blackout Kick"),
                           Spell.CastSpell("Blackout Kick", ret => Chi > 3 && Me.CurrentEnergy > 50, "Blackout Kick"),
                           Spell.CastSpell("Tiger Palm", ret => (Buff.PlayerHasActiveBuff("Combo Breaker") && Me.CurrentEnergy > 70) || (Buff.PlayerBuffTimeLeft("Combo Breaker") < 2 && Buff.PlayerHasActiveBuff("Combo Breaker")), "Tiger Palm"),
                           Spell.CastSpell("Jab", ret => Buff.PlayerHasActiveBuff("Ascension") && Chi < 3, "Jab"),
                           Spell.CastSpell("Jab", ret => Buff.PlayerHasActiveBuff("Chi Brew") && Chi < 2, "Jab"),
                           Spell.CastSpell("Blackout Kick", ret => (Me.CurrentEnergy > 30 && RisingSunKickCoolDown) || (Chi > 4 && Buff.PlayerHasActiveBuff("Ascension")) || (Chi > 5 && Buff.PlayerHasActiveBuff("Ascension")), "Blackout Kick"),

                           // Interupt
                           Spell.CastInterupt("Spear Hand Strike", ret => true, "Spear Hand Strike"),
                           Spell.CastSelfSpell("Tigereye Brew", ret => Buff.PlayerCountBuff("Tigereye Brew") == 10 && CLUSettings.Instance.UseCooldowns, "Tigereye Brew"),
                           Spell.CastSpell("Rising Sun Kick", ret => true, "Rising Sun Kick"),
                            // AoE
                           Spell.CastAreaSpell("Fists of Fury", 8, false, 3, 0.0, 0.0, ret => true, "Fists of Fury"),
                           Spell.CastAreaSpell("Spinning Crane Kick", 8, false, 3, 0.0, 0.0, ret => true, "Spinning Crane Kick"), // no really required but will leave in for low levels
                           Spell.CastSelfSpell("Invoke Xuen, the White Tiger", ret => TalentManager.HasTalent(17) && CLUSettings.Instance.UseCooldowns, "Invoke Xuen, the White Tiger"), // if=talent.invoke_xuen.enabled (TalentManager.HasTalent(1, 1) is bogus but I put it in here to remind me to check for this to be talented)
                           Spell.CastSpell("Blackout Kick", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Blackout Kick"), "Blackout Kick"),
                           Spell.CastSpell("Tiger Palm", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm"), "Tiger Palm"),
                           Spell.CastSpell("Blackout Kick", ret => Chi >= 1, "Blackout Kick"),
                            // Spells.CastSpell("TigerPalm",             ret => Buff.PlayerCountBuff("Tiger Power") == 3, "TigerPalm"), //posible dps loss
                            // Spells.CastSpell("TigerPalm",             ret => Buff.PlayerCountBuff("Tiger Power") <= 3, "TigerPalm"), //posible dps loss
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
                               Buff.CastBuff("Fortifying Brew", ret => Me.HealthPercent < 50, "Fortifying Brew"), // Turns your skin to stone, increasing your health by 20%, and reducing damage taken by 20%. Lasts 20 sec.
                               Buff.CastBuff("Guard", ret => Me.HealthPercent < 50, "Guard"), // absorbs damage for 30secs and increases any healing by 30%
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                              Buff.CastRaidBuff("Legacy of the Emperor", ret => true, "Legacy of the Emperor"),
                              Buff.CastRaidBuff("Legacy of the White Tiger", ret => true, "Legacy of the White Tiger")));
            }
        }

        public override Composite Resting
        {
            get
            {
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return this.SingleRotation;
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return this.SingleRotation;
            }
        }
    }
}