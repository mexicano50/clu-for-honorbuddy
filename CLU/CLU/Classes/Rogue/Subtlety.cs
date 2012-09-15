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
using Styx;
using CLU.Helpers;
using CLU.Lists;
using Styx.WoWInternals.WoWObjects;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Styx.CommonBot;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Rogue
{
    class Subtlety : RotationBase
    {

        public override string Name
        {
            get {
                return "Subtlety Rogue";
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
                return "Hemorrhage";
            }
        }
        public override int KeySpellId
        {
            get { return 16511; }
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
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Attempt to evade/escape crowd control with Evasion, Cloak of Shadows, Smoke Bomb, Combat Readiness.\n" +
                       "2. Rotation is set up for Hemorrhage\n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "4. Attempt to reduce threat with Feint\n" +
                       "5. Will interupt with Kick\n" +
                       "6. Tricks of the Trade on best target (tank, then class)\n" +
                       "7. Will heal with Recuperate and a Healthstone\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private static bool IsBehind(WoWUnit target)
        {
            // WoWMathHelper.this.IsBehind(Me.Location, target.Location, target.Rotation, (float)Math.PI * 5 / 6)
            return target != null && target.MeIsBehind;
        }

        //!Buff.UnitHasBleedDamageDebufMinusHemorrhage(Me.CurrentTarget)

        private static bool CanHemorrhage
        {
            get {
                return Me.CurrentTarget != null && ((Buff.TargetDebuffTimeLeft("Hemorrhage").TotalSeconds <= 1 && Buff.TargetHasDebuff("Hemorrhage")));
            }
        }

        private static bool CanHemorrhageDoT
        {
            get {
                return Me.CurrentTarget != null && !StyxWoW.Me.CurrentTarget.HasAura(89775) || StyxWoW.Me.CurrentTarget.GetAuraById(89775).TimeLeft.TotalSeconds <= 1;
            }
        }

        private static bool UseHemorrhage
        {
            get {
                return Me.CurrentTarget != null && TalentManager.HasGlyph("Hemorrhage") ? CanHemorrhage : CanHemorrhageDoT; //removed UnitHasBleedDamageDebuff no longer in game TODO: See how this affects the rotation. --wulf
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

                           // Don't do anything if we have cast vanish
                           // new Decorator( ret => Buff.PlayerHasActiveBuff("Vanish"), new ActionAlwaysSucceed()),

                           // Stealth
                           Buff.CastBuff("Stealth", ret => CLUSettings.Instance.Rogue.EnableAlwaysStealth, "Stealth"),

                           // Questing and PvP helpers
                           new Decorator(
                               ret => CLUSettings.Instance.EnableMovement,
                               new PrioritySelector(
                                   Spell.CastSpell("Premeditation",       ret => Buff.PlayerHasBuff("Stealth"), "Premeditation"),
                                   Spell.CastSpell("Shadowstep",          ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsWithinMeleeRange && !BossList.IgnoreShadowStep.Contains(Unit.CurrentTargetEntry) && !Buff.PlayerHasBuff("Sprint"), "Shadowstep"),
                                   Spell.CastSelfSpell("Sprint",          ret => Me.IsMoving && Unit.DistanceToTargetBoundingBox() >= 15, "Sprint"),
                                   Spell.CastSpell("Garrote",             ret => Me.CurrentTarget != null && IsBehind(Me.CurrentTarget) && Buff.PlayerHasBuff("Stealth"), "Garrote"),
                                   Spell.CastSpell("Cheap Shot",          ret => Me.CurrentTarget != null && !SpellManager.HasSpell("Garrote") || !IsBehind(Me.CurrentTarget) && Buff.PlayerHasBuff("Stealth"), "Cheap Shot"),
                                   Spell.CastSpell("Ambush",              ret => !SpellManager.HasSpell("Cheap Shot") && IsBehind(Me.CurrentTarget) && Buff.PlayerHasBuff("Stealth"), "Ambush"))),

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && (Unit.IsTargetWorthy(Me.CurrentTarget)),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),

                           // Experimental Rotation
                           new Decorator(
                               ret => CLUSettings.Instance.Rogue.SubtletyRogueRotationSelection == SubtletyRogueRotation.ImprovedTestVersion,
                               new PrioritySelector(
                                   //Spell.CastSpell("Feint", ret => Me.CurrentTarget != null && (Me.CurrentTarget.ThreatInfo.RawPercent > 80 || EncounterSpecific.IsMorchokStomp()) && CLUSettings.Instance.EnableSelfHealing, "Feint"),
                                   Spell.CastInterupt("Kick",                     ret => true, "Kick"),
                                   Spell.CastSpell("Redirect",                    ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"),
                                   Spell.CastSpell("Tricks of the Trade", u => Unit.BestTricksTarget, ret => Me.CurrentEnergy >= 60 && Me.ComboPoints < 5 && (!Buff.PlayerHasActiveBuff("Vanish") && !Spell.SpellOnCooldown("Premeditation") && !Spell.SpellOnCooldown("Shadow Dance") || !Buff.PlayerHasActiveBuff("Stealth") && !Spell.SpellOnCooldown("Premeditation") && !Spell.SpellOnCooldown("Shadow Dance")), "Tricks of the Trade"),
                                   //Spell.CastSpell("Tricks of the Trade", u => Unit.BestTricksTarget, ret => Buff.PlayerHasActiveBuff("Shadow Dance"), "Tricks of the Trade"),
                                   Spell.CastSpell("Shadow Dance",                ret => Me.CurrentEnergy >= 60 && Me.ComboPoints < 5 && (!Buff.PlayerHasActiveBuff("Vanish") && !Spell.SpellOnCooldown("Premeditation") || !Buff.PlayerHasActiveBuff("Stealth") && !Spell.SpellOnCooldown("Premeditation")), "Shadow Dance"),
                                   Spell.CastSpell("Vanish",                      ret => Me.CurrentEnergy >= 60 && Me.ComboPoints <= 1 && !Buff.PlayerHasActiveBuff("Shadow Dance") && !Buff.PlayerHasActiveBuff("Master of Subtlety") && !Buff.TargetHasDebuff("Find Weakness") && !Buff.PlayerHasActiveBuff("Vanish") && !Spell.SpellOnCooldown("Premeditation"), "Vanish"),
                                   Spell.CastSpell("Shadowstep",                  ret => ((Buff.PlayerHasActiveBuff("Shadow Dance") && Buff.TargetHasDebuff("Find Weakness")) || (Buff.PlayerHasActiveBuff("Vanish") || Buff.PlayerHasActiveBuff("Stealth"))) && !BossList.IgnoreShadowStep.Contains(Unit.CurrentTargetEntry), "Shadowstep"),
                                   Spell.CastSpell("Premeditation",               ret => Me.ComboPoints <= 2 || Buff.PlayerHasActiveBuff("Vanish"), "Premeditation"),
                                   Spell.CastSpell("Ambush",                      ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && Buff.PlayerHasActiveBuff("Vanish"), "Ambush"),
                                   Spell.CastSpell("Ambush",                      ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && Me.ComboPoints <= 4, "Ambush"),
                                   Spell.CastSpell("Preparation",                 ret => Spell.SpellCooldown("Vanish").TotalSeconds > 60 && !Buff.PlayerHasActiveBuff("Vanish") && !Buff.PlayerHasActiveBuff("Shadow Dance"), "Preparation"),
                                   Spell.CastSpell("Eviscerate",                  ret => Me.ComboPoints == 5 && Buff.PlayerHasActiveBuff("Shadow Dance"), "Rupture"),
                                   Spell.CastSpell("Rupture",                     ret => Me.ComboPoints == 5 && !Buff.TargetHasDebuff("Rupture") && Buff.PlayerHasActiveBuff("Master of Subtlety"), "Rupture"),
                                   Spell.CastSelfSpell("Slice and Dice",          ret => Buff.PlayerBuffTimeLeft("Slice and Dice") < 3 && Me.ComboPoints == 5 && !Buff.PlayerHasActiveBuff("Shadow Dance"), "Slice and Dice"),
                                   Spell.CastSpell("Rupture",                     ret => Me.ComboPoints == 5 && !Buff.TargetHasDebuff("Rupture"), "Rupture"),
                                   Spell.CastSpell("Eviscerate",                  ret => Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds < 3, "Eviscerate"),
                                   Spell.CastSelfSpell("Recuperate",              ret => Me.ComboPoints == 5 && Buff.PlayerBuffTimeLeft("Recuperate") < 3 && !Buff.PlayerHasActiveBuff("Shadow Dance"), "Recuperate"),
                                   Spell.CastSpell("Eviscerate",                  ret => Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 1, "Eviscerate"),
                                   Spell.CastSpell("Hemorrhage",                  ret => Me.ComboPoints < 4 && !Buff.PlayerHasActiveBuff("Shadow Dance") && !Buff.PlayerHasActiveBuff("Vanish") && UseHemorrhage, "Hemorrhage"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints < 4 && Spell.SpellOnCooldown("Shadow Dance") && !Spell.SpellOnCooldown("Vanish") && Spell.SpellOnCooldown("Premeditation"), "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints < 4 && !Spell.SpellOnCooldown("Shadow Dance") && Spell.SpellOnCooldown("Vanish") && Spell.SpellOnCooldown("Premeditation"), "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints < 4 && !Spell.SpellOnCooldown("Shadow Dance") && !Spell.SpellOnCooldown("Vanish") && Spell.SpellOnCooldown("Premeditation"), "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints >= 2 && Spell.SpellOnCooldown("Shadow Dance") && !Spell.SpellOnCooldown("Vanish") && !Spell.SpellOnCooldown("Premeditation"), "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints < 4 && Spell.SpellOnCooldown("Shadow Dance") && Spell.SpellOnCooldown("Vanish"), "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints == 4 && Spell.SpellOnCooldown("Shadow Dance"), "Backstab"),
                                   Spell.CastSpell("Hemorrhage",                  ret => !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints == 4 && !Spell.SpellOnCooldown("Shadow Dance") && Spell.SpellOnCooldown("Premeditation") && UseHemorrhage, "Hemorrhage"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints == 4 && !Spell.SpellOnCooldown("Shadow Dance") && Spell.SpellOnCooldown("Premeditation"), "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && !Buff.PlayerHasActiveBuff("Shadow Dance") && Me.ComboPoints == 4 && Me.CurrentEnergy > 70 && !Spell.SpellOnCooldown("Shadow Dance") && !Spell.SpellOnCooldown("Premeditation"), "Backstab"),
                                   Spell.CastSpell("Hemorrhage",                  ret => Me.CurrentTarget != null && (!IsBehind(Me.CurrentTarget)) && Me.ComboPoints < 4, "Hemorrhage"),
                                   Spell.CastSpell("Hemorrhage",                  ret => Me.CurrentTarget != null && (!IsBehind(Me.CurrentTarget)) && Me.ComboPoints < 5 && Me.CurrentEnergy > 70, "Hemorrhage"))),

                           // Default Rotation
                           new Decorator(
                               ret => CLUSettings.Instance.Rogue.SubtletyRogueRotationSelection == SubtletyRogueRotation.Default,
                               new PrioritySelector(
                                   //threat
                                   Spell.CastSpell("Feint",                       ret => Me.CurrentTarget != null && (Me.CurrentTarget.ThreatInfo.RawPercent > 80 || EncounterSpecific.IsMorchokStomp()), "Feint"),
                                   Spell.CastInterupt("Kick",                     ret => true, "Kick"),
                                   Spell.CastSpell("Redirect",                    ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"),
                                   // Spell.CastAreaSpell("Fan of Knives", 8, false, 6, 0.0, 0.0, ret => true, "Fan of Knives"),
                                   Spell.CastSpell("Tricks of the Trade", u => Unit.BestTricksTarget, ret => CLUSettings.Instance.Rogue.UseTricksOfTheTrade, "Tricks of the Trade"),
                                   Spell.CastSpell("Shadow Dance",                ret => Me.CurrentEnergy > 85 && Me.ComboPoints < 5 && (!Buff.PlayerHasActiveBuff("Vanish") || !Buff.PlayerHasActiveBuff("Stealth")), "Shadow Dance"),
                                   Spell.CastSpell("Vanish",                      ret => Me.CurrentEnergy > 60 && Me.ComboPoints <= 1 && Spell.SpellCooldown("Shadowstep").TotalSeconds <= 0 && !Buff.PlayerHasActiveBuff("Shadow Dance") && !Buff.PlayerHasActiveBuff("Master of Subtlety") && !Buff.TargetHasDebuff("Find Weakness"), "Vanish"),
                                   Spell.CastSpell("Shadowstep",                  ret => ((Buff.PlayerHasActiveBuff("Shadow Dance") && Buff.TargetHasDebuff("Find Weakness")) || (Buff.PlayerHasActiveBuff("Vanish") || Buff.PlayerHasActiveBuff("Stealth"))) && !BossList.IgnoreShadowStep.Contains(Unit.CurrentTargetEntry), "Shadowstep"),
                                   Spell.CastSpell("Premeditation",               ret => Me.ComboPoints <= 2, "Premeditation"),
                                   Spell.CastSpell("Ambush",                      ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && Me.ComboPoints <= 4, "Ambush"),
                                   Spell.CastSpell("Preparation",                 ret => Spell.SpellCooldown("Vanish").TotalSeconds > 60, "Preparation"),
                                   Spell.CastSelfSpell("Slice and Dice",          ret => Buff.PlayerBuffTimeLeft("Slice and Dice") < 3 && Me.ComboPoints == 5, "Slice and Dice"),
                                   Spell.CastSpell("Rupture",                     ret => Me.ComboPoints == 5 && !Buff.TargetHasDebuff("Rupture"), "Rupture"),
                                   Spell.CastSelfSpell("Recuperate",              ret => Me.ComboPoints == 5 && Buff.PlayerBuffTimeLeft("Recuperate") < 3, "Recuperate"),
                                   Spell.CastSpell("Eviscerate",                  ret => Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 1, "Eviscerate"),
                                   Spell.CastSpell("Hemorrhage",                  ret => Me.CurrentTarget != null && ((Me.ComboPoints < 4 && UseHemorrhage) || !IsBehind(Me.CurrentTarget)), "Hemorrhage"),
                                   Spell.CastSpell("Hemorrhage",                  ret => Me.CurrentTarget != null && ((Me.ComboPoints < 5 && Me.CurrentEnergy > 80 && UseHemorrhage) || !IsBehind(Me.CurrentTarget)), "Hemorrhage"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && Me.ComboPoints < 4, "Backstab"),
                                   Spell.CastSpell("Backstab",                    ret => Me.CurrentTarget != null && (IsBehind(Me.CurrentTarget) || BossList.BackstabIds.Contains(Unit.CurrentTargetEntry)) && Me.ComboPoints < 5 && Me.CurrentEnergy > 80, "Backstab")
                               )));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone",             ret => Me.HealthPercent < 40, "Healthstone"),
                               Spell.CastSelfSpell("Smoke Bomb",          ret => Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet, "Smoke Bomb"),
                               Spell.CastSelfSpell("Combat Readiness",    ret => Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet, "Combat Readiness"),
                               Spell.CastSelfSpell("Evasion",             ret => Me.HealthPercent < 35 && Unit.EnemyUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                               Spell.CastSelfSpell("Cloak of Shadows",    ret => Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1, "Cloak of Shadows"),
                               Poisons.CreateApplyPoisons()));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               // Stealth
                               Spell.CastSelfSpell("Stealth", ret => !Buff.PlayerHasBuff("Stealth") && CLUSettings.Instance.Rogue.EnableAlwaysStealth, "Stealth"),
                               Poisons.CreateApplyPoisons()));
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