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
using System.Linq;
using CommonBehaviors.Actions;
using CLU.Helpers;
using Styx.WoWInternals.WoWObjects;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Styx;
using Styx.CommonBot;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Rogue
{

    class Combat : RotationBase
    {

        public override string Name
        {
            get
            {
                return "Combat Rogue";
            }
        }

        // public static readonly HealerBase Healer = HealerBase.Instance;

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
                return "Blade Flurry";
            }
        }
        public override int KeySpellId
        {
            get { return 13877; }
        }
        public override float CombatMaxDistance
        {
            get
            {
                return 3.2f;
            }
        }

        // adding some help
        public override string Help
        {
            get
            {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Attempt to evade/escape crowd control with Evasion, Cloak of Shadows, Smoke Bomb, Combat Readiness.\n" +
                       "2. Rotation is set up for Hemorrhage\n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Adrenaline Rush & Killing Spree\n" +
                       "4. Attempt to reduce threat with Feint\n" +
                       "5. Will interupt with Kick\n" +
                       "6. Tricks of the Trade on best target (tank, then class)\n" +
                       "7. Will heal with Recuperate and a Healthstone\n" +
                       "8. Expose Armor on Bosses only if similar buff is not present\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude, alxaw\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static bool IsBehind(WoWUnit target)
        {
            // WoWMathHelper.this.IsBehind(Me.Location, target.Location, target.Rotation, (float)Math.PI * 5 / 6)
            return target != null && target.MeIsBehind;
        }

        public override Composite SingleRotation
        {
            get
            {
                WoWPlayer TricksTarget = null;
                return new PrioritySelector(
                    ctx => TricksTarget = Unit.BestTricksTarget as WoWPlayer,
                    // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(), // EncounterSpecific.ExtraActionButton(), new Decorator(x => Me.IsInInstance, EncounterSpecific.ExtraActionButton())

                           // Don't do anything if we have cast vanish
                           // new Decorator( ret => Buff.PlayerHasActiveBuff("Vanish"), new ActionAlwaysSucceed()),

                           // Stealth
                           Buff.CastBuff("Stealth", ret => CLUSettings.Instance.Rogue.EnableAlwaysStealth && !CLU.IsMounted, "Stealth"),

                           // Questing and PvP helpers
                           new Decorator(
                               ret => CLUSettings.Instance.EnableMovement && Buff.PlayerHasBuff("Stealth"),
                               new PrioritySelector(
                                    // Spell.CastSpell("Pick Pocket", ret => Buff.PlayerHasBuff("Stealth"), "Gimme the caaash (Pick Pocket)"),
                                   Spell.CastSelfSpell("Sprint",        ret => Me.IsMoving && Unit.DistanceToTargetBoundingBox() >= 15, "Sprint"),
                                   Spell.CastSpell("Garrote",           ret => Me.CurrentTarget != null && IsBehind(Me.CurrentTarget), "Garrote"),
                                   Spell.CastSpell("Cheap Shot",        ret => Me.CurrentTarget != null && !SpellManager.HasSpell("Garrote") || !IsBehind(Me.CurrentTarget), "Cheap Shot"),
                                   Spell.CastSpell("Ambush",            ret => !SpellManager.HasSpell("Cheap Shot") && IsBehind(Me.CurrentTarget), "Ambush"))),

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Spell.CastSelfSpell("Preparation", ret => SpellManager.HasSpell(14185) && Unit.UseCooldowns() && SpellManager.Spells["Vanish"].Cooldown, "Preparation"),
                                   Item.UseEngineerGloves())),
                            
                            //Spell.CastSpell("Feint", ret => Me.CurrentTarget != null && (Me.CurrentTarget.ThreatInfo.RawPercent > 80 || Encounte  rSpecific.IsMorchokStomp()) && CLUSettings.Instance.EnableSelfHealing, "Feint"),
                           Spell.CastInterupt("Kick",           ret => true, "Kick"),
                           Spell.CastSpell("Redirect",          ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"),
                           Spell.CancelMyAura("Blade Flurry",   ret => CLUSettings.Instance.UseAoEAbilities && Buff.PlayerHasBuff("Blade Flurry") && (Unit.EnemyMeleeUnits.Count() < 2 || Unit.EnemyMeleeUnits.Count() > 6), "Blade Flurry"),
                           Spell.CastSpell("Crimson Tempest",   ret => Unit.EnemyMeleeUnits.Count() >= 7 && Me.ComboPoints == 5 && CLUSettings.Instance.UseAoEAbilities, "Crimson Tempest"),
                           Spell.CastSelfSpell("Blade Flurry",  ret => CLUSettings.Instance.UseAoEAbilities && Unit.EnemyMeleeUnits.Count() > 1 && Unit.EnemyMeleeUnits.Count() < 7 && !Buff.PlayerHasBuff("Blade Flurry"), "Blade Flurry"),
                           Spell.CastSpell("Ambush",            ret => Me.IsBehind(Me.CurrentTarget), "Ambush"),
                           Spell.CastAreaSpell("Fan of Knives", 8, false, CLUSettings.Instance.Rogue.CombatFanOfKnivesCount, 0.0, 0.0, ret => CLUSettings.Instance.UseAoEAbilities, "Fan of Knives"),
                           Spell.CastSpell("Tricks of the Trade", u => TricksTarget, ret => TricksTarget != null && TricksTarget.IsAlive && !TricksTarget.IsHostile && TricksTarget.IsInMyParty && CLUSettings.Instance.Rogue.UseTricksOfTheTrade, "Tricks of the Trade"),
                            //Spell.CastSpell("Expose Armor", ret => Me.CurrentTarget != null && Me.ComboPoints == 5 && Unit.UseCooldowns() && !Buff.UnitHasWeakenedArmor(Me.CurrentTarget), "Expose Armor"),
                           Spell.CastSelfSpell("Slice and Dice",    ret => Me.ComboPoints >= 1 && Buff.PlayerBuffTimeLeft("Slice and Dice") < 2, "Slice and Dice"),
                           Vanish,
                           Spell.CastSelfSpell("Killing Spree", ret => Me.CurrentEnergy < 35 && Buff.PlayerBuffTimeLeft("Slice and Dice") > 4 && !Buff.PlayerHasActiveBuff("Adrenaline Rush") && Unit.UseCooldowns(), "Killing Spree"),
                           Spell.CastSelfSpell("Adrenaline Rush", ret => Me.CurrentTarget != null && Me.CurrentEnergy < 35 && Spell.SpellCooldown("Killing Spree").TotalSeconds > 10 && Unit.UseCooldowns(), "Adrenaline Rush"),
                           Spell.CastSelfSpell("Shadow Blades", ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Adrenaline Rush") && Unit.UseCooldowns(), "Shadow Blades"),
                           Spell.CastSpell("Revealing Strike",      ret => SpellManager.HasSpell(114015) && Buff.TargetDebuffTimeLeft("Revealing Strike").TotalSeconds < 2 && (Buff.PlayerCountBuff("Anticipation") < 5 || Me.ComboPoints < 5), "Revealing Strike"),
                           Spell.CastSpell("Revealing Strike",      ret => !SpellManager.HasSpell(114015) && Buff.TargetDebuffTimeLeft("Revealing Strike").TotalSeconds < 2 && Me.ComboPoints < 5, "Revealing Strike"),
                           Spell.CastSpell("Rupture",               ret => Unit.EnemyMeleeUnits.Count() < 2 && (Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds < 3 && Unit.TimeToDeath(Me.CurrentTarget) > 10 || Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds < 2 && Buff.PlayerHasBuff("Deep Insight") && Unit.TimeToDeath(Me.CurrentTarget) > 10 || Me.ComboPoints <= 3 && !Buff.TargetHasDebuff("Rupture") && Unit.TimeToDeath(Me.CurrentTarget) > 10 && Me.CurrentEnergy > 30), "Rupture"),
						   Spell.CastSpell("Rupture",               ret => Unit.EnemyMeleeUnits.Count() > 1 && !CLUSettings.Instance.UseAoEAbilities && (Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds < 3 && Unit.TimeToDeath(Me.CurrentTarget) > 10 || Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds < 2 && Buff.PlayerHasBuff("Deep Insight") && Unit.TimeToDeath(Me.CurrentTarget) > 10 || Me.ComboPoints <= 3 && !Buff.TargetHasDebuff("Rupture") && Unit.TimeToDeath(Me.CurrentTarget) > 10 && Me.CurrentEnergy > 30), "Rupture No AoE"),
                           Spell.CastSpell("Eviscerate",            ret => SpellManager.HasSpell(114015) && (Me.ComboPoints == 5 && Buff.PlayerHasBuff("Deep Insight") || Me.ComboPoints > 4 && Buff.PlayerCountBuff("Anticipation") > 4 && Me.CurrentEnergy >= 50), "Eviscerate"),
                           Spell.CastSpell("Eviscerate",            ret => !SpellManager.HasSpell(114015) && (Me.ComboPoints > 4 && Me.CurrentEnergy >= 60 || Me.ComboPoints > 4 && Buff.TargetDebuffTimeLeft("Revealing Strike").TotalSeconds <= 2 || Me.ComboPoints >= 1 && Buff.PlayerHasBuff("Fury of the Destroyer") || Me.ComboPoints == 5 && Buff.PlayerHasBuff("Shadow Blades")), "Eviscerate Pre-90"),
                           Spell.CastSpell("Sinister Strike",       ret => !SpellManager.HasSpell(114015) && (Buff.TargetHasDebuff("Revealing Strike") && Me.ComboPoints < 5), "Sinister Strike"),
                           Spell.CastSpell("Sinister Strike",       ret => SpellManager.HasSpell(114015) && !Buff.PlayerHasActiveBuff("Shadow Blades") && Buff.PlayerCountBuff("Anticipation") < 4 || Buff.PlayerCountBuff("Anticipation") < 5 || Me.ComboPoints < 5, "Sinister Strike")
                       );
            }
        }


        public override Composite Pull
        {
            get { return this.SingleRotation; }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone",           ret => Me.HealthPercent < 40, "Healthstone"),
                               Spell.CastSelfSpell("Smoke Bomb",        ret => Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet, "Smoke Bomb"),
                               Spell.CastSelfSpell("Combat Readiness",  ret => Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet, "Combat Readiness"),
                               Spell.CastSelfSpell("Evasion",           ret => Me.HealthPercent < 35 && Unit.EnemyMeleeUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                               Spell.CastSelfSpell("Cloak of Shadows",  ret => Unit.EnemyMeleeUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1, "Cloak of Shadows"),
                               Poisons.CreateApplyPoisons()));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                            // Stealth
                           Spell.CastSelfSpell("Stealth", ret => !Buff.PlayerHasBuff("Stealth") && CLUSettings.Instance.Rogue.EnableAlwaysStealth && !CLU.IsMounted, "Stealth"),
                           Spell.CancelMyAura("Blade Flurry", ret => Buff.PlayerHasBuff("Blade Flurry") && (Unit.EnemyMeleeUnits.Count() < 2 || Unit.EnemyMeleeUnits.Count() > 6), "Blade Flurry"),
                               Poisons.CreateApplyPoisons()));
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

        private static bool HasShadowFocus
        {
            get { return TalentManager.HasTalent(3); }
        }

        private static bool BuffsSafeForVanish
        {
            get
            {
                return Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6 &&
                       Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 4;
            }
        }

        private static bool EnergySafeForVanish
        {
            get { return ((HasShadowFocus && Me.EnergyPercent < 50) || !HasShadowFocus && Me.CurrentEnergy >= 60); }
        }

        private static bool SafeToBreakStealth
        {
            get
            {
                return ((Me.Combat || Me.RaidMembers.Any(rm => rm.Combat) || Unit.IsTrainingDummy(Me.CurrentTarget)) &&
                         Unit.UseCooldowns());
            }
        }

        private static Composite Vanish
        {
            get
            {
                // Only Do this if SnD is up, Rupture is up, Target is CD-worthy and we've got spare points.
                return new Decorator
                    (x =>
                     BuffsSafeForVanish && Unit.UseCooldowns() && Me.ComboPoints < 4 &&
                     EnergySafeForVanish && Me.CurrentTarget.IsWithinMeleeRange,
                     Spell.CastSelfSpell("Vanish", x => true, "Vanish"));
            }
        }

        internal override void OnPulse()
        {
            StealthedCombat();
        }

        private static void StealthedCombat()
        {
            if (Me.CurrentTarget == null || Me.CurrentTarget.IsDead ||
                 (!Me.CurrentTarget.IsHostile && !Unit.IsTrainingDummy(Me.CurrentTarget)) ||
                 !Me.CurrentTarget.Attackable)
            {
                return;
            }

            if ((!Me.IsStealthed && !Buff.PlayerHasActiveBuff("Vanish")) || !SafeToBreakStealth)
            {
                return;
            }

            // If we're not behind, attempt to shadowstep and wait for next pulse.
            if (SpellManager.HasSpell("Shadowstep") && !StyxWoW.Me.IsBehind(Me.CurrentTarget) &&
                      Spell.CanCast("Shadowstep", Me.CurrentTarget))
            {
                CLULogger.Log(" [Casting] Shadowstep on {0} @ StealthedCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Shadowstep");
            }
            else if (Me.Behind(Me.CurrentTarget) && (Me.CurrentEnergy >= 60 || HasShadowFocus))
            {
                CLULogger.Log(" [Casting] Ambush on {0} @ StealthCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Ambush");
            }
        }
    }
}
