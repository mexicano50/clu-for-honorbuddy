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

using System.Collections.Generic;
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
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using Rest = CLU.Base.Rest;

namespace CLU.Classes.Rogue
{
    [UsedImplicitly]
    public class Assassination : RotationBase
    {
        #region Constants and Fields

        private const string DispatchOverride = "Sinister Strike";
        private const string EnvenomOverride = "Eviscerate";
        private static IEnumerable<WoWUnit> _aoeTargets;
        private static WoWUnit _tricksTarget;

        private static bool _tricksTargetChecked;

        #endregion

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
                       "6. Use Sinister Strike when Blindside is up and under 5 combo points.\n" +
                       "7. Sinister Strike when target is under 35% HP.\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits for this rotation: Weischbier, Wulf, Singularity team, LaoArchAngel, kbrebel04\n" +
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

        public override Composite Pull
        {
            get { return new PrioritySelector(Spell.CastSpell("Throw", ret => Me.CurrentTarget.Distance <= 30, "Throw for Pull")); }
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
                    (ret => Me.HealthPercent <= 100 && CLUSettings.Instance.EnableSelfHealing,
                     new PrioritySelector
                         (Item.UseBagItem("Kafa'Kota Berry", ret => Me.CurrentTarget != null && !Buff.PlayerHasBuff("Kafa Rush"), "Healthstone"),
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
                               Unit.EnemyMeleeUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                          Spell.CastSelfSpell
                              ("Cloak of Shadows",
                               ret => Unit.EnemyMeleeUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1,
                               "Cloak of Shadows"),
                          Poisons.CreateApplyPoisons()));
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
                     !Me.HasAura("Drink"),
                     new PrioritySelector(OutOfCombat));
            }
            }

        public override Composite Pull
        {
            get { return this.SingleRotation; }
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
                    Reset,
                     MovementHelpers,
                     EncounterSpecific.ExtraActionButton(),
                     Cooldowns,
                     Spell.CastSelfSpell
                         ("Feint", ret => Me.CurrentTarget != null && (EncounterSpecific.IsMorchokStomp()), "Feint"),
                     Spell.CastSpell
                         ("Tricks of the Trade", u => TricksTarget, ret => TricksTarget != null, "Tricks of the Trade"),
                     Spell.CastInterupt("Kick", ret => Me.IsWithinMeleeRange, "Kick"),
                     Spell.CastSpell("Redirect", ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"),
                     //AoEDebug,
                     //AoE,
                     Spell.CastSelfSpell
                         ("Slice and Dice", ret => !Buff.PlayerHasActiveBuff("Slice and Dice"), "Slice and Dice"),
                     Vanish,
                     Buff.CastMyDebuff("Rupture", ret => true,"Rupture default"),
                     Rupture,
                     Spell.CastSpell
                         ("Vendetta",
                          ret =>
                          Me.CurrentTarget != null && Unit.UseCooldowns() && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 2 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 2,
                          "Vendetta"),
                     Spell.CastSpell
                         ("Shadow Blades",
                          ret =>
                          Me.CurrentTarget != null && Unit.UseCooldowns() && Me.ComboPoints > 4 && Me.CurrentEnergy > 50,
                          "Shadow Blades"),
                     Spell.CastSelfSpell
                         ("Preparation",
                          ret =>
                          SpellManager.HasSpell(14185) && Unit.UseCooldowns() &&
                          SpellManager.Spells["Vanish"].Cooldown,
                          "Preparation"),
                     Envenom,
                     Spell.CastSpell
                         (DispatchOverride,
                          ret => ( Me.ComboPoints < 5 || AnticipationSafe ) && Me.CurrentTarget.HealthPercent >= 35,
                          "Dispatch @ Blindside"),
                     Spell.CastSpell
                         (DispatchOverride,
                          ret => Me.ComboPoints < ReqCmbPts && Me.CurrentTarget.HealthPercent < 35,
                          "Dispatch"),
                     Spell.CastSpell
                         ("Mutilate",
                          ret => Me.ComboPoints < ReqCmbPts && Me.CurrentTarget.HealthPercent >= 35,
                          "Mutilate"));
            }
        }

        #endregion

        #region Properties

        private static bool AnticipationSafe
        {
            get { return ( HasAnticipation && Me.Auras["Anticipation"].StackCount < 4 ); }
        }

        /// <summary>
        /// Gets the area of effect rotaion.
        /// Rotation by Wulf.
        /// </summary>
        private static Composite AoE
        {
            get
            {
                return new Decorator
                    (ret =>
                     AoETargets.All(x => FoKSafe(x) && x.Combat) &&
                     AoETargets.Count() >= CLUSettings.Instance.Rogue.AssasinationFanOfKnivesCount,
                     new PrioritySelector
                         (Spell.CastSelfSpell
                              ("Crimson Tempest",
                               ret => AoETargets.Any(a => !a.HasMyAura("Crimson Tempest")) && Me.ComboPoints > 3,
                               "Crimson Tempest"),
                          Spell.CastSelfSpell
                              ("Fan of Knives", ret => Me.ComboPoints < 5 || AnticipationSafe, "Fan of Knives"),
                          new ActionAlwaysSucceed()));
            }
        }

        //private static Action AoEDebug
        //{
        //    get
        //    {
        //        return new Action
        //            (delegate
        //                {
        //                    CLU.Log("AoETargets != null: {0}", AoETargets != null);
                            
        //                    if(AoETargets == null)
        //                        return RunStatus.Failure;

        //                    CLU.Log("AoETargets.Count(): {0}", AoETargets.Count());

        //                    foreach ( var aoeTarget in AoETargets.Where(x => !FoKSafe(x)))
        //                    {
        //                        var spell = aoeTarget.Debuffs.First(x => IsCcMechanic(x.Value.Spell)).Value;
        //                        CLU.Log("Target {0} affected by {1} which is {2}", aoeTarget.Name, spell.Name, spell.Spell.Mechanic);
        //                    }

        //                    CLU.Log("AoETargets in combat: {0}", AoETargets.All(x => x.Combat));

        //                    return RunStatus.Failure;
        //                });

        //        //(ret => AoETargets.All(x => !Unit.IsCrowdControlled(x)) && AoETargets.Count() >= CLUSettings.Instance.Rogue.AssasinationFanOfKnivesCount,
        //    }
        //}

        private static IEnumerable<WoWUnit> AoETargets
        {
            get
            {
                return _aoeTargets ??
                       ( _aoeTargets =
                         ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where
                             (unit => unit.Attackable && unit.DistanceSqr <= 100 && unit.IsAlive) );
            }
                }

        //private static bool BleedSafe
        //{
        //    get
        //    {
        //        return Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 4 ||
        //               Buff.TargetDebuffTimeLeft("Garrote").TotalSeconds > 2;
        //    }
        //}

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
                     ((Unit.UseCooldowns() || Buff.TargetHasDebuff("Vendetta"))),
                    //Switched to || instead of &&, we want to use trinkets on Cd and not every 2min
                     new PrioritySelector
                         (Item.UseTrinkets(),
                          Racials.UseRacials(),
                          Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                    // Thanks Kink
                          Item.UseEngineerGloves()));
            }
        }

        /// <summary>
        /// If we have Shadow Focus, spells cost 0 energy.  Thus, we should vanish only when there's no fear of capping.
        /// If we do not have Shadow Focus, we should Vanish only if we have enough energy for Mutilate.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [vanish with shadow focus]; otherwise, <c>false</c>.
        /// </value>
        private static bool EnergySafeForVanish
        {
            get { return ( ( HasShadowFocus && Me.EnergyPercent < 50 ) || !HasShadowFocus && Me.CurrentEnergy >= 60 ); }
        }

        private static Composite Envenom
        {
            get
            {
                return new Decorator
                    (cond => Buff.PlayerHasActiveBuff("Slice and Dice"),
                     new PrioritySelector
                         (Spell.CastSpell
                              (EnvenomOverride,
                               ret =>
                               Me.ComboPoints > 0 && Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds < 2,
                               "SnD Refresh Envenom"),
                          Spell.CastSpell
                              (EnvenomOverride,
                               ret =>
                               Me.ComboPoints == 5 && Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 2 && (!Unit.UseCooldowns() && Me.CurrentTarget.HealthPercent < 35 || Unit.UseCooldowns() && Spell.SpellOnCooldown("Shadow Blades") && Me.CurrentTarget.HealthPercent < 35 || Buff.PlayerHasBuff("Shadow Blades")),
                               "Execute Envenom"),
                          Spell.CastSpell
                              (EnvenomOverride,
                               ret =>
                               Me.ComboPoints >= ReqCmbPts && RuptureSafe &&
                               ( HasAnticipation || Me.CurrentEnergy > 70 ),
                               "Pooling Envenom"),
                          Spell.CastSpell
                              (EnvenomOverride,
                               ret => Me.ComboPoints >= 1 && Buff.PlayerHasBuff("Fury of the Destroyer") && RuptureSafe,
                               "FoTF Envenom")));
            }
            }

        private static bool HasAnticipation
        {
            get { return TalentManager.HasTalent(18); }
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
                             ("Cheap Shot",
                              ret =>
                              Me.CurrentTarget != null && !SpellManager.HasSpell("Garrote") ||
                              !StyxWoW.Me.IsBehind(Me.CurrentTarget),
                              "Cheap Shot"),
                         Spell.CastSpell
                             ("Mutilate",
                              ret => !SpellManager.HasSpell("Cheap Shot") && StyxWoW.Me.IsBehind(Me.CurrentTarget),
                              "Cheap Shot")));
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
                               ret =>
                               !Buff.PlayerHasBuff("Stealth") && CLUSettings.Instance.Rogue.EnableAlwaysStealth &&
                               !CLU.IsMounted,
                               "Stealth"),
                          Poisons.CreateApplyPoisons()));
            }
        }

        private static int ReqCmbPts
        {
            get
            {
                if ( HasAnticipation )
                {
                    return 5;
                }

                return ( Me.CurrentTarget.HealthPercent < 35 ) ? 5 : 4;
            }
        }

        private static Action Reset
        {
            get
            {
                return new Action
                    (delegate
                        {
                            _aoeTargets = null;
                            _tricksTarget = null;
                            _tricksTargetChecked = false;
                            return RunStatus.Failure;
                        });
            }
        }

        private static Decorator Rupture
        {
            get
            {
                return new Decorator
                    (x => SnDSafe && !RuptureSafe,
                    // Do not rupture if SnD is about to come down.
                     new PrioritySelector
                         (Spell.CastSpell
                              ("Rupture",
                               ret => !Buff.TargetHasDebuff("Rupture") && !Buff.TargetHasDebuff("Garrote"),
                               "Rupture @ Down"),
                          // Rupture if it's down.  Rupture is not considered down if Garrote is up, since it follows the same energy benefits, but does not stack.
                          Spell.CastSpell("Rupture", ret => Me.ComboPoints >= ReqCmbPts, "Rupture @ Low")));
                // Rupture if it's about to fall off and we have 4 or 5 combo points.
            }
        }

        private static bool RuptureSafe
        {
            get { return Buff.TargetDebuffTimeLeft("Rupture").TotalSeconds > 4; }
        }

        private static bool SafeToBreakStealth
        {
            get
            {
                return ((Me.Combat || Me.RaidMembers.Any(rm => rm.Combat) || Unit.IsTrainingDummy(Me.CurrentTarget)) &&
                         Unit.UseCooldowns());
            }
        }

        private static bool SnDSafe
        {
            get { return Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds > 6; }
        }

        private static WoWUnit TricksTarget
        {
            get
            {
                if ( _tricksTargetChecked )
                {
                    return _tricksTarget;
                }

                if ( _tricksTarget == null )
                {
                    _tricksTarget = Unit.BestTricksTarget;
                    _tricksTargetChecked = true;
                }

                return _tricksTarget;
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
                     BuffsSafeForVanish && Unit.UseCooldowns() &&
                     ( Me.ComboPoints < 4 || AnticipationSafe ) && EnergySafeForVanish &&
                     Me.CurrentTarget.IsWithinMeleeRange && !Buff.PlayerHasActiveBuff("Blindside"),
                     Spell.CastSelfSpell("Vanish", x => true, "Vanish"));
            }
        }

        #endregion

        #region Methods

        internal override void OnPulse()
        {
            StealthedCombat();
        }

        private static bool FoKSafe(WoWUnit unit)
        {
            return
                !unit.Debuffs.Select(kvp => kvp.Value).Any
                     (x =>
                      x.Spell.Mechanic == WoWSpellMechanic.Banished || x.Spell.Mechanic == WoWSpellMechanic.Charmed ||
                      x.Spell.Mechanic == WoWSpellMechanic.Horrified ||
                      x.Spell.Mechanic == WoWSpellMechanic.Incapacitated ||
                      x.Spell.Mechanic == WoWSpellMechanic.Polymorphed || x.Spell.Mechanic == WoWSpellMechanic.Sapped ||
                      x.Spell.Mechanic == WoWSpellMechanic.Shackled || x.Spell.Mechanic == WoWSpellMechanic.Asleep ||
                      x.Spell.Mechanic == WoWSpellMechanic.Frozen);
        }

        //private static bool IsCcMechanic(WoWSpell spell)
        //{
        //    return spell.Mechanic == WoWSpellMechanic.Banished || spell.Mechanic == WoWSpellMechanic.Charmed ||
        //           spell.Mechanic == WoWSpellMechanic.Horrified || spell.Mechanic == WoWSpellMechanic.Incapacitated ||
        //           spell.Mechanic == WoWSpellMechanic.Polymorphed || spell.Mechanic == WoWSpellMechanic.Sapped ||
        //           spell.Mechanic == WoWSpellMechanic.Shackled || spell.Mechanic == WoWSpellMechanic.Asleep ||
        //           spell.Mechanic == WoWSpellMechanic.Frozen;
        //}

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
                CLULogger.Log(" [Casting] Mutilate on {0} @ StealthCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Mutilate");
            }
            else if (Me.CurrentTarget.HealthPercent < 35 || Buff.PlayerHasBuff("Blindside"))
            {
                CLULogger.Log(" [Casting] Dispatch on {0} @ StealthedCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast(DispatchOverride);
            }
            else
            {
                CLULogger.Log(" [Casting] Mutilate on {0} @ StealthedCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Mutilate");
            }
        }

        #endregion
    }
}
