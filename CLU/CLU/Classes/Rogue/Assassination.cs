﻿#region Revision info

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

using CLU.Base;
using CLU.Helpers;
using CLU.Managers;
using CLU.Settings;

using CommonBehaviors.Actions;

using JetBrains.Annotations;

using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;

using Rest = CLU.Base.Rest;

namespace CLU.Classes.Rogue
{
    [UsedImplicitly]
    public class Assassination : RotationBase
    {
        #region Public Properties

        public override float CombatMaxDistance
        {
            get { return 3.2f; }
        }

        public override string Help
        {
            get
            {
                return "\n" + "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" + "1. use defensive moves when holding aggro.\n" + "2. Pop cooldowns: \n" +
                       "==> UseTrinkets \n" + "==> UseRacials \n" + "==> UseEngineerGloves \n" +
                       "3. Kick for interrupts.\n" + "4. Tricks of the Trade on the best target (tank, then class)\n" +
                       "5. Maintain Slice and Dice and Rupture.\n" +
                       "6. Use Dispatch when Blindside is up and under 5 combo points.\n" +
                       "7. Dispatch when target is under 35% HP.\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits for this rotation: Weischbier, Wulf, Singularity team, LaoArchAngel\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override string KeySpell
        {
            get { return "Mutilate"; }
        }

        public override int KeySpellId
        {
            get { return 1329; }
        }

        /// <summary>
        /// Gets the healing rotation.
        /// Rotation created by wulf.
        /// </summary>
        public override Composite Medic
        {
            get
            {
                return new Decorator
                    (ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                     new PrioritySelector
                         (Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone"),
                          Spell.CastSelfSpell
                              ("Smoke Bomb",
                               ret =>
                               Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet,
                               "Smoke Bomb"),
                          Spell.CastSelfSpell
                              ("Combat Readiness",
                               ret =>
                               Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet,
                               "Combat Readiness"),
                          Spell.CastSelfSpell
                              ("Evasion",
                               ret =>
                               Me.HealthPercent < 35 &&
                               Unit.EnemyUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                          Spell.CastSelfSpell
                              ("Cloak of Shadows",
                               ret => Unit.EnemyUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1,
                               "Cloak of Shadows"), Poisons.CreateApplyPoisons()));
            }
        }

        public override string Name
        {
            get { return "Assassination Rotation"; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        /// <summary>
        /// Gets the combat preperation rotation.
        /// Rotation by wulf.
        /// </summary>
        public override Composite PreCombat
        {
            get
            {
                return new Decorator
                    (ret =>
                     !Me.Mounted && !Me.IsDead && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") &&
                     !Me.HasAura("Drink"), new PrioritySelector(OutOfCombat));
            }
        }

        /// <summary>
        /// Gets the rest rotation.
        /// Rotation created by the Singular devs.
        /// </summary>
        public override Composite Resting
        {
            get { return new PrioritySelector(Rest.CreateDefaultRestBehaviour()); }
        }

        public override string Revision
        {
            get { return "$Rev$"; }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector
                    (new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                     MovementHelpers, EncounterSpecific.ExtraActionButton(), Cooldowns,
                     Spell.CastSelfSpell
                         ("Feint",
                          ret =>
                          Me.CurrentTarget != null &&
                          ( /*Me.CurrentTarget.ThreatInfo.RawPercent > 80 ||*/ EncounterSpecific.IsMorchokStomp() ),
                          "Feint"),
                     Spell.CastSpell
                         ("Tricks of the Trade", u => Unit.BestTricksTarget, ret => Unit.BestTricksTarget != null,
                          "Tricks of the Trade"), Spell.CastInterupt("Kick", ret => Me.IsWithinMeleeRange, "Kick"),
                     Spell.CastSpell("Redirect", ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"), AoE,
                     Spell.CastSelfSpell
                         ("Slice and Dice", ret => !Buff.PlayerHasActiveBuff("Slice and Dice"), "Slice and Dice"),
                     Vanish, Rupture,
                     Spell.CastSpell
                         ("Vendetta",
                          ret =>
                          Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) &&
                          Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6, "Vendetta"),
                     Spell.CastSelfSpell
                         ("Preparation",
                          ret =>
                          SpellManager.HasSpell(14185) && Unit.IsTargetWorthy(Me.CurrentTarget) &&
                          SpellManager.Spells["Vanish"].Cooldown, "Preparation"),
                     Spell.CastSpell
                         ("Dispatch", ret => Me.ComboPoints < 5 && Buff.PlayerHasBuff("Blindside"),
                          "Dispatch @ Blindside"), Envenom,
                     Spell.CastSpell
                         ("Dispatch", ret => Me.ComboPoints < ReqCmbPts && Me.CurrentTarget.HealthPercent < 35,
                          "Dispatch"),
                     Spell.CastSpell
                         ("Mutilate", ret => Me.ComboPoints < ReqCmbPts && Me.CurrentTarget.HealthPercent >= 35,
                          "Mutilate"));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the area of effect rotaion.
        /// Rotation by Wulf.
        /// </summary>
        private static PrioritySelector AoE
        {
            get
            {
                return new PrioritySelector
                    (Spell.CastAreaSpell
                         ("Crimson Tempest", 8, false, 4, 0, 0,
                          ret => Unit.EnemyUnits.Any(a => !a.HasMyAura("Crimson Tempest") && Me.ComboPoints > 3),
                          "Crimson Tempest"),
                     Spell.CastAreaSpell
                         ("Fan of Knives", 8, false, 4, 0.0, 0.0, ret => Me.ComboPoints < 5, "Fan of Knives"));
            }
        }

        private static bool BuffsSafeForVanish
        {
            get
            {
                return Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6 &&
                       Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 4;
            }
        }

        private static Decorator Cooldowns
        {
            get
            {
                return new Decorator
                    (ret =>
                     Me.CurrentTarget != null &&
                     ( ( Unit.IsTargetWorthy(Me.CurrentTarget) || Buff.TargetHasDebuff("Vendetta") ) ),
                     //Switched to || instead of &&, we want to use trinkets on Cd and not every 2min
                     new PrioritySelector
                         (Item.UseTrinkets(), Racials.UseRacials(), Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                          // Thanks Kink
                          Item.UseEngineerGloves()));
            }
        }

        private static Composite Envenom
        {
            get
            {
                return new Decorator
                    (cond => Buff.PlayerHasActiveBuff("Slice and Dice"),
                     new PrioritySelector
                         (Spell.CastSpell
                              ("Envenom",
                               ret =>
                               Me.ComboPoints >= ReqCmbPts && !Buff.TargetHasDebuff("Envenom") &&
                               Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 5, "Envenom @ First"),
                          // Envenom if we have enough combo points, Envenom debuff is down and Rupture is safe.
                          Spell.CastSpell
                              ("Envenom",
                               ret =>
                               Me.ComboPoints >= ReqCmbPts && Me.CurrentEnergy > 90 &&
                               Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 2, "Envenom @ Second"),
                          // Envenom if we have enough combo points, Rupture is safe and we're about to cap.
                          Spell.CastSpell
                              ("Envenom",
                               ret =>
                               Me.ComboPoints >= 2 && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds < 2,
                               "Envenom @ Third (WARNING!)")
                         // Envenom if SnD is about to fall off. This should never happen.
                         ));
            }
        }

        private static bool HasShadowFocus
        {
            get { return TalentManager.HasTalent(3); }
        }

        /// <summary>
        /// Adds Movement support within BG's and Questing. -- wulf.
        /// </summary>
        private static Composite MovementHelpers
        {
            get
            {
                return new Decorator
                    (ret => CLUSettings.Instance.EnableMovement && Buff.PlayerHasBuff("Stealth"),
                     new PrioritySelector
                         ( // Spell.CastSpell("Pick Pocket", ret => Buff.PlayerHasBuff("Stealth"), "Gimme the caaash (Pick Pocket)"),
                         Spell.CastSelfSpell
                             ("Sprint", ret => Me.IsMoving && Unit.DistanceToTargetBoundingBox() >= 15, "Sprint"),
                         Spell.CastSpell
                             ("Garrote", ret => Me.CurrentTarget != null && StyxWoW.Me.IsBehind(Me.CurrentTarget),
                              "Garrote"),
                         Spell.CastSpell
                             ("Cheap Shot",
                              ret =>
                              Me.CurrentTarget != null && !SpellManager.HasSpell("Garrote") ||
                              !StyxWoW.Me.IsBehind(Me.CurrentTarget), "Cheap Shot"),
                         Spell.CastSpell
                             ("Ambush",
                              ret => !SpellManager.HasSpell("Cheap Shot") && StyxWoW.Me.IsBehind(Me.CurrentTarget),
                              "Ambush")));
            }
        }

        /// <summary>
        /// Gets the out-of-combat routine.
        /// </summary>
        private static Composite OutOfCombat
        {
            get
            {
                return new Decorator
                    (cond => !Me.Combat && !Me.PartyMembers.Any(pm => pm.Combat),
                     new PrioritySelector
                         (Spell.CastSelfSpell
                              ("Stealth",
                               ret => !Buff.PlayerHasBuff("Stealth") && CLUSettings.Instance.Rogue.EnableAlwaysStealth,
                               "Stealth"), Poisons.CreateApplyPoisons()));
            }
        }

        private static int ReqCmbPts
        {
            get { return ( Me.CurrentTarget != null ) && ( Me.CurrentTarget.HealthPercent < 35 ) ? 5 : 4; }
        }

        private static Decorator Rupture
        {
            get
            {
                return new Decorator
                    (x =>
                     Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6 &&
                     Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds <= 2,
                     // Do not rupture if SnD is about to come down.
                     new PrioritySelector
                         (Spell.CastSpell("Rupture", ret => !Buff.TargetHasDebuff("Rupture"), "Rupture @ Down"),
                          // Rupture if it's down.
                          Spell.CastSpell("Rupture", ret => Me.ComboPoints >= ReqCmbPts, "Rupture @ Low")));
                // Rupture if it's about to fall off and we have 4 or 5 combo points.
            }
        }

        private static bool SafeToBreakStealth
        {
            get
            {
                return ( Me.Combat || Me.RaidMembers.Any(rm => rm.Combat) || Unit.IsTrainingDummy(Me.CurrentTarget) ) &&
                       Unit.IsTargetWorthy(Me.CurrentTarget);
            }
        }

        /// <summary>
        /// Gets the vanish rotation
        /// Rotation by Weischbier.
        /// Logic by LaoArchAngel
        /// </summary>
        private static Composite Vanish
        {
            get
            {
                // Only Do this if SnD is up, Rupture is up, Target is CD-worthy and we've got spare points.
                return new Decorator
                    (x =>
                     BuffsSafeForVanish && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.ComboPoints < 4 &&
                     VanishWithShadowFocus && Me.CurrentTarget.IsWithinMeleeRange,
                     Spell.CastSelfSpell("Vanish", x => true, "Vanish"));
            }
        }

        private static bool VanishWithShadowFocus
        {
            get { return ( ( HasShadowFocus && Me.EnergyPercent < 50 ) || !HasShadowFocus ); }
        }

        #endregion

        #region Methods

        internal override void OnPulse()
        {
            StealthedCombat();
        }

        private static void StealthedCombat()
        {
            if ( Me.CurrentTarget == null || Me.CurrentTarget.IsDead ||
                 ( !Me.CurrentTarget.IsHostile && !Unit.IsTrainingDummy(Me.CurrentTarget) ) ||
                 !Me.CurrentTarget.Attackable )
            {
                return;
            }

            if ( ( !Me.IsStealthed && !Buff.PlayerHasActiveBuff("Vanish") ) || !SafeToBreakStealth )
            {
                return;
            }

            if ( StyxWoW.Me.IsBehind(Me.CurrentTarget) && Spell.CanCast("Ambush", Me.CurrentTarget) )
            {
                CLU.Log(" [Casting] Ambush on {0}", CLU.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Ambush");
            }
            else if ( SpellManager.HasSpell("Shadowstep") && !StyxWoW.Me.IsBehind(Me.CurrentTarget) &&
                      Spell.CanCast("Shadowstep", Me.CurrentTarget) )
            {
                CLU.Log(" [Casting] Shadowstep on {0}", CLU.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Shadowstep");
            }
            else if ( !StyxWoW.Me.IsBehind(Me.CurrentTarget) && Me.CurrentEnergy > 90 &&
                      Me.CurrentTarget.HealthPercent < 35 && Spell.CanCast("Dispatch", Me.CurrentTarget) )
            {
                CLU.Log(" [Casting] Dispatch on {0} @ StealthedCombat", CLU.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Dispatch");
            }
            else if ( !StyxWoW.Me.IsBehind(Me.CurrentTarget) && Me.CurrentEnergy > 90 &&
                      Spell.CanCast("Mutilate", Me.CurrentTarget) )
            {
                CLU.Log(" [Casting] Mutilate on {0} @ StealthedCombat", CLU.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Mutilate");
            }
        }

        #endregion
    }
}