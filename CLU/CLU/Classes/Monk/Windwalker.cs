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

using System;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using System.Linq;
using CLU.Managers;
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
                return
                    @"
----------------------------------------------------------------------
Windwalker:
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Setting)
[*] This rotation does not support the use of Ascencion
This Rotation will:
100% Optimal DPS rotation:
-Uses Mark of Death
-Rising Sun Kick on cooldown
-Maintain 3 stacks of Tiger Power
-Uses Summon Xuen, the White Tiger
-Use 10 stacks of Tigerseye brew (only on bosses, do it manually if u want it on trash, this is to ensure you go into a fight with 10 stacks for the most possible DPS.)
-Uses Fists of Fury on cooldown when certain requirements are met(Not Moving, Rising Sun Kick is on cooldown, not under the effects of energizing brew, and Tiger Power has more than 5 seconds left on it)
-Uses Blackout Kick and Tiger Palm Mastery procs
-Uses blackout kick will all of the above requirements are not met
-Uses Chi Brew appropriately if you have it talented
-Use Expel Harm instead of Jab at sum 80% health(this is a DPS increase) and uses Jab when expel harm is on cooldown
-Uses Jab
-Interrupts
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
Credits: alxaw , Kbrebel04
----------------------------------------------------------------------";
            }
        }

        private static uint Chi
        {
            get
            {
                return Me.CurrentChi;
            }
        }

        private static Composite HandleFlyingUnits
        {
            get
            {
                //Shoot flying targets		
                return new Decorator(
                     ret => StyxWoW.Me.CurrentTarget != null && (StyxWoW.Me.CurrentTarget.IsFlying || StyxWoW.Me.CurrentTarget.Distance2DSqr < 5 * 5 && Math.Abs(StyxWoW.Me.Z - StyxWoW.Me.CurrentTarget.Z) >= 5) && CLUSettings.Instance.EnableMovement,
                         new PrioritySelector(
                        Spell.ChannelSpell("Crackling Jade Lightning", ret => true, "Crackling Jade Lightning")));
            }
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

                    // Utility
                    new Decorator(ret => CLUSettings.Instance.EnableMovement,
                        new PrioritySelector(
                            Spell.CastSpell("Disable", ret => Me.CurrentEnergy >= 15 && (Me.CurrentTarget.IsPlayer || Me.CurrentTarget.Fleeing) && Me.CurrentTarget.MovementInfo.RunSpeed > 3.5, "Disable")
                            )),

                    HandleFlyingUnits,

                    //Cooldowns
                    new Decorator(
                        ret => CLUSettings.Instance.UseCooldowns && Me.CurrentTarget != null && !Spell.PlayerIsChanneling && Unit.UseCooldowns(),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Item.UseEngineerGloves(),
                    Spell.CastSelfSpell("Tigereye Brew", ret => !Buff.PlayerHasBuff("Tigereye Brew Use") && Buff.PlayerCountBuff("Tigereye Brew") == 10, "Tigerye Brew"),
                    Spell.CastSpell("Invoke Xuen, the White Tiger", ret => TalentManager.HasTalent(17) && Me.CurrentEnergy < 80, "Invoke Xuen"),
                    Spell.CastSpell("Rushing Jade Wind", ret => TalentManager.HasTalent(16) && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.TargetHasDebuff("Rising Sun Kick") && Me.CurrentEnergy <= 80, "Rushing Jade Wind"))),

                    //Class Cooldowns


                    //Interupts
                    Spell.CastInterupt("Spear Hand Strike", ret => Me.CurrentTarget.IsWithinMeleeRange, "Spear Hand Strike"),
                    //Spell.CastInterupt("Grapple Weapon", ret => true, "Grapple Weapon"),

                    // AoE
                    new Decorator(
                        ret => Unit.EnemyMeleeUnits.Count() > 3 && CLUSettings.Instance.UseAoEAbilities,
                        new PrioritySelector(
                        Spell.CastSpell("Rising Sun Kick", ret => !Buff.TargetHasDebuff("Rising Sun Kick"), "Rising Sun Kick"),
                    //Spell.CastAreaSpell("Fists of Fury", 8, false, 4, 0.0, 0.0, ret => true, "Fists of Fury"),
                        Spell.CastSpell("Spinning Crane Kick", ret => true, "Spinning Crane Kick"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.TargetHasDebuff("Rising Sun Kick") && (Buff.PlayerCountBuff("Tiger Power") < 3 || Buff.PlayerHasBuff("Tiger Power") && Buff.PlayerBuffTimeLeft("Tiger Power") < 3), "Tiger Palm"),
                        Spell.CastSpell("Blackout Kick", ret => Buff.TargetHasDebuff("Rising Sun Kick") && Buff.PlayerCountBuff("Tiger Power") > 2 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Me.CurrentEnergy < 40, "Blackout Kick"))),


                    //Single Target With Fists of Fury
                    new Decorator(
                        ret => CLUSettings.Instance.Monk.EnableFists && (Unit.EnemyMeleeUnits.Count() < 4 || !CLUSettings.Instance.UseAoEAbilities),
                        new PrioritySelector(
                        Spell.CastSpell("Touch of Death", ret => Buff.PlayerHasBuff("Death Note"), "Touch of Death"),
                        Spell.CastSelfSpell("Chi Brew", ret => TalentManager.HasTalent(9) && Chi == 0 && Me.CurrentEnergy <= 50, "Chi Brew"),
                        Spell.CastSpell("Energizing Brew", ret => Me.CurrentEnergy < 40 && !Spell.PlayerIsChanneling, "Energizing Brew"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.PlayerHasBuff("Tiger Power") && Buff.PlayerBuffTimeLeft("Tiger Power") < 3, "Refresh Tiger Power"),
                        Spell.CastSpell("Rising Sun Kick", ret => !Buff.TargetHasDebuff("Rising Sun Kick") || Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Me.CurrentEnergy >= 40 || Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Chi > 2 || Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") || Buff.PlayerBuffTimeLeft("Tiger Power") >= 5, "Rising Sun Kick"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.PlayerCountBuff("Tiger Power") < 3, "Tiger Palm"),
                        Spell.CastSpell("Fists of Fury", ret => !Me.IsMoving && !Buff.PlayerHasActiveBuff("Energizing Brew") && Me.CurrentEnergy <= 60 && Buff.PlayerBuffTimeLeft("Tiger Power") > 5 && Buff.PlayerCountBuff("Tiger Power") == 3 && Spell.SpellCooldown("Rising Sun Kick").TotalSeconds >= 2, "Fists of Fury"),
                        Spell.CastSpell("Blackout Kick", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Blackout Kick") && Spell.SpellOnCooldown("Rising Sun Kick") && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Me.CurrentEnergy < 80, "Combo Breaker: Blackout Kick"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") && Me.CurrentEnergy < 80 && Spell.SpellOnCooldown("Rising Sun Kick") || Buff.PlayerBuffTimeLeft("Combo Breaker: Tiger Palm") < 2 && Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm"), "Combo Breaker: Tiger Palm"),
                        Spell.CastSpell("Blackout Kick", ret => Chi <= 4 && Spell.SpellOnCooldown("Fists of Fury") && Spell.SpellOnCooldown("Rising Sun Kick") && Me.CurrentEnergy >= 70 && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 || Chi <= 4 && Spell.SpellOnCooldown("Fists of Fury") && Spell.SpellCooldown("Rising Sun Kick").TotalSeconds >= 2 && Me.CurrentEnergy >= 20 && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 || !Spell.SpellOnCooldown("Fists of Fury") && Spell.SpellOnCooldown("Rising Sun Kick") && Me.CurrentEnergy > 60 && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Chi >= 3 || !Spell.SpellOnCooldown("Fists of Fury") && Spell.SpellOnCooldown("Rising Sun Kick") && Me.CurrentEnergy <= 60 && Me.IsMoving && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 || !Spell.SpellOnCooldown("Fists of Fury") && Spell.SpellOnCooldown("Rising Sun Kick") && Me.CurrentEnergy <= 60 && Buff.PlayerHasActiveBuff("Energizing Brew") && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3, "Blackout Kick w/ FoF"),
                        Spell.CastSpell("Expel Harm", ret => Chi <= 2 && Me.HealthPercent <= 60 && Me.CurrentEnergy > 80 || Chi == 2 && Me.HealthPercent <= 60 && !Spell.SpellOnCooldown("Fists of Fury"), "Expel Harm w/FoF"),
                        Spell.CastSpell("Jab", ret => Chi <= 2 && Me.HealthPercent > 60 && Me.CurrentEnergy >= 80 || Chi < 2 && Me.HealthPercent > 60 || Chi < 2 && Spell.SpellOnCooldown("Expel Harm") && Me.HealthPercent <= 60 || Chi <= 2 && Me.HealthPercent > 60 && !Spell.SpellOnCooldown("Fists of Fury") || Chi <= 2 && Spell.SpellOnCooldown("Expel Harm") && Me.HealthPercent <= 60 && !Spell.SpellOnCooldown("Fists of Fury") || Chi <= 2 && Spell.SpellOnCooldown("Expel Harm") && Me.HealthPercent <= 60 && Me.CurrentEnergy >= 80, "Jab w/FoF"),
                    //Spell.CastSpell("Chi Wave", ret => TalentManager.HasTalent(4) && Chi >= 2 && Me.HealthPercent <= 40, "Chi Wave"),
                        Spell.CastSpell("Touch of Karma", ret => Chi >= 2 && Me.HealthPercent <= 40, "Touch of Karma"))),


                    //Single Target Without Fists of Fury
                    new Decorator(
                        ret => !CLUSettings.Instance.Monk.EnableFists && (Unit.EnemyMeleeUnits.Count() < 4 || !CLUSettings.Instance.UseAoEAbilities),
                        new PrioritySelector(
                        Spell.CastSpell("Touch of Death", ret => Buff.PlayerHasBuff("Death Note"), "Touch of Death"),
                        Spell.CastSelfSpell("Chi Brew", ret => TalentManager.HasTalent(9) && Chi == 0 && Me.CurrentEnergy <= 50, "Chi Brew"),
                        Spell.CastSpell("Energizing Brew", ret => Me.CurrentEnergy < 40 && !Spell.PlayerIsChanneling, "Energizing Brew"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.PlayerHasBuff("Tiger Power") && Buff.PlayerBuffTimeLeft("Tiger Power") < 3, "Refresh Tiger Power"),
                        Spell.CastSpell("Rising Sun Kick", ret => !Buff.TargetHasDebuff("Rising Sun Kick") || Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Me.CurrentEnergy >= 40 || Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Chi > 2 || Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") || Buff.PlayerBuffTimeLeft("Tiger Power") >= 5, "Rising Sun Kick"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.PlayerCountBuff("Tiger Power") < 3, "Tiger Palm"),
                        Spell.CastSpell("Blackout Kick", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Blackout Kick") && Spell.SpellOnCooldown("Rising Sun Kick") && Buff.PlayerCountBuff("Tiger Power") == 3 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Me.CurrentEnergy < 80, "Combo Breaker: Blackout Kick"),
                        Spell.CastSpell("Tiger Palm", ret => Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") && Me.CurrentEnergy < 80 && Spell.SpellOnCooldown("Rising Sun Kick") || Buff.PlayerBuffTimeLeft("Combo Breaker: Tiger Palm") < 2 && Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") && Me.CurrentEnergy < 90, "Combo Breaker: Tiger Palm"),
                        Spell.CastSpell("Blackout Kick", ret => Spell.SpellOnCooldown("Rising Sun Kick") && Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Me.CurrentEnergy >= 40 || Spell.SpellOnCooldown("Rising Sun Kick") && Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") >= 3 && Chi > 2 || Spell.SpellOnCooldown("Rising Sun Kick") && Buff.PlayerBuffTimeLeft("Tiger Power") < 5 && Buff.PlayerBuffTimeLeft("Tiger Power") < 3 && Buff.PlayerHasActiveBuff("Combo Breaker: Tiger Palm") || Spell.SpellOnCooldown("Rising Sun Kick") && Buff.PlayerBuffTimeLeft("Tiger Power") >= 5, "Blackout Kick"),
                        Spell.CastSpell("Expel Harm", ret => Chi <= 2 && Me.HealthPercent <= 60, "Expel Harm"),
                        Spell.CastSpell("Jab", ret => Chi <= 2 && Me.HealthPercent > 60 && Me.CurrentEnergy > 80 || Chi < 2 && Me.HealthPercent > 60 || Chi < 2 && Spell.SpellOnCooldown("Expel Harm") && Me.HealthPercent <= 60 || Chi <= 2 && Spell.SpellOnCooldown("Expel Harm") && Me.HealthPercent <= 60 && Me.CurrentEnergy > 80, "Jab"),
                    //Spell.CastSpell("Chi Wave",             ret => TalentManager.HasTalent(4) && Chi >= 2 && Me.HealthPercent <= 40, "Chi Wave"),
                        Spell.CastSpell("Touch of Karma", ret => Chi >= 2 && Me.HealthPercent <= 40, "Touch of Karma"))));

            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(ret => CLUSettings.Instance.EnableMovement,
                        new PrioritySelector(
                            Spell.CastSpell("Provoke", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8, "Provoke"),
                            Spell.CastSpell("Crackling Jade Lightning", ret => Me.CurrentTarget != null && Me.CurrentTarget.DistanceSqr >= 8 * 8 && Spell.SpellOnCooldown("Roll"), "Crackling Jade Lightning")
                            )),
                    this.SingleRotation
                    );
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableRaidPartyBuffing,
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
            get
            {
                return
                    new PrioritySelector(
                        Spell.CastSpell("Roll", ret => CLUSettings.Instance.EnableMovement && Me.Level < 20 && !CLU.IsMounted, "Roll"),
                        Rest.CreateDefaultRestBehaviour());
            }
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