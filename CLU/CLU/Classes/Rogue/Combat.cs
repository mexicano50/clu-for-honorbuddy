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
using Styx.CommonBot;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Rogue
{

    class Combat : RotationBase
    {

        public override string Name
        {
            get {
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
            get {
                return "Blade Flurry";
            }
        }
        public override int KeySpellId
        {
            get { return 13877; }
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
                       "==> Adrenaline Rush & Killing Spree\n" +
                       "4. Attempt to reduce threat with Feint\n" +
                       "5. Will interupt with Kick\n" +
                       "6. Tricks of the Trade on best target (tank, then class)\n" +
                       "7. Will heal with Recuperate and a Healthstone\n" +
                       "8. Expose Armor on Bosses only if similar buff is not present\n" +
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

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
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
                                   Spell.CastSelfSpell("Sprint",  ret => Me.IsMoving && Unit.DistanceToTargetBoundingBox() >= 15, "Sprint"),
                                   Spell.CastSpell("Garrote",     ret => Me.CurrentTarget != null && IsBehind(Me.CurrentTarget), "Garrote"),
                                   Spell.CastSpell("Cheap Shot",  ret => Me.CurrentTarget != null && !SpellManager.HasSpell("Garrote") || !IsBehind(Me.CurrentTarget), "Cheap Shot"),
                                   Spell.CastSpell("Ambush",      ret => !SpellManager.HasSpell("Cheap Shot") && IsBehind(Me.CurrentTarget), "Ambush"))),

                           // Trinkets & Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           //Spell.CastSpell("Feint", ret => Me.CurrentTarget != null && (Me.CurrentTarget.ThreatInfo.RawPercent > 80 || EncounterSpecific.IsMorchokStomp()) && CLUSettings.Instance.EnableSelfHealing, "Feint"),
                           Spell.CastInterupt("Kick",                     ret => true, "Kick"),
                           Spell.CastSpell("Redirect",                    ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"),
                           Item.RunMacroText("/cancelaura Blade Flurry",  ret => Unit.EnemyUnits.Count() < 2 && Buff.PlayerHasBuff("Blade Flurry"), "[CancelAura] Blade Flurry"),
                           Buff.CastBuff("Blade Flurry",                  ret => Unit.EnemyUnits.Count() >= 2 && CLUSettings.Instance.UseAoEAbilities, "Blade Flurry"),
                           Spell.CastAreaSpell("Fan of Knives", 8, false, CLUSettings.Instance.Rogue.CombatFanOfKnivesCount, 0.0, 0.0, ret => Me.CurrentEnergy > 85, "Fan of Knives"),
                           Spell.CastSpell("Tricks of the Trade", u => Unit.BestTricksTarget, ret => CLUSettings.Instance.Rogue.UseTricksOfTheTrade, "Tricks of the Trade"),
                           Spell.CastSpell("Expose Armor",                ret => Me.CurrentTarget != null && Me.ComboPoints == 5 && Unit.IsTargetWorthy(Me.CurrentTarget) && !Buff.UnitHasWeakenedArmor(Me.CurrentTarget), "Expose Armor"),
                           Spell.CastSelfSpell("Slice and Dice",          ret => Buff.PlayerBuffTimeLeft("Slice and Dice") < 2 && Me.CurrentEnergy >= 25 && Me.ComboPoints > 3, "Slice and Dice"),
                           Spell.CastSelfSpell("Killing Spree",           ret => Me.CurrentEnergy < 35 && Buff.PlayerBuffTimeLeft("Slice and Dice") > 4 && !Buff.PlayerHasBuff("Adrenaline Rush") && CLUSettings.Instance.UseCooldowns, "Killing Spree"),
                           Spell.CastSelfSpell("Adrenaline Rush",         ret => Me.CurrentTarget != null && Me.CurrentEnergy < 35 && Unit.IsTargetWorthy(Me.CurrentTarget), "Adrenaline Rush"),
                           Spell.CastSpell("Eviscerate",                  ret => Me.ComboPoints == 5 && (Buff.PlayerHasBuff("Moderate Insight") || Buff.PlayerHasBuff("Deep Insight")), "Eviscerate & Moderate Insight or Deep Insight"),
                           Spell.CastSpell("Rupture",                     ret => Me.CurrentTarget != null && Me.ComboPoints == 5 && !Buff.TargetHasDebuff("Rupture"), "Rupture"), //removed bleed check no longer ingame --  wulf
                           Spell.CastSpell("Eviscerate",                  ret => Me.ComboPoints == 5, "Eviscerate"),
                           Spell.CastSpell("Revealing Strike",            ret => Me.ComboPoints == 4 && !Buff.TargetHasDebuff("Revealing Strike"), "Revealing Strike"),
                           Spell.CastSpell("Sinister Strike",             ret => Me.ComboPoints < 5, "Sinister Strike")
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Recuperate",              ret => Me.HealthPercent < 55 && !Buff.PlayerHasBuff("Recuperate") && CLUSettings.Instance.EnableMovement, "Recuperate"),
                               Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone"),
                               Spell.CastSelfSpell("Smoke Bomb",              ret => Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet, "Smoke Bomb"),
                               Spell.CastSelfSpell("Combat Readiness",        ret => Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet, "Combat Readiness"),
                               Spell.CastSelfSpell("Evasion",                 ret => Me.HealthPercent < 35 && Unit.EnemyUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                               Spell.CastSelfSpell("Cloak of Shadows",        ret => Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1, "Cloak of Shadows"),
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
                               Spell.CastSelfSpell("Stealth", ret => !Buff.PlayerHasBuff("Stealth") && CLUSettings.Instance.Rogue.EnableAlwaysStealth && !CLU.IsMounted, "Stealth"),
                               Item.RunMacroText("/cancelaura Blade Flurry", ret => Unit.EnemyUnits.Count() < 2 && Buff.PlayerHasBuff("Blade Flurry"), "[CancelAura] Blade Flurry"),
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
