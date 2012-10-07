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

using CLU.Helpers;

namespace CLU.Base
{
    using System;
    using Styx;
    using Styx.CommonBot;
    using Styx.Helpers;
    //using Styx.Logic.Pathing;
    using Styx.Pathing;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using CommonBehaviors.Actions;
    using Styx.TreeSharp;
        //using Styx.Logic;
    using global::CLU.Settings;
    using Action = Styx.TreeSharp.Action;

    public static class Movement
    {
        /* putting all the Movement logic here */

        private const bool EnableBotTargetingOveride = false;

        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }

        /// <summary>
        /// Movement Behaviour
        /// </summary>
        public static Composite MovingFacingBehavior()
        {
            return MovingFacingBehavior(ret => StyxWoW.Me.CurrentTarget);
        }

        private static Composite MovingFacingBehavior(CLU.UnitSelection onUnit)
        // TODO: Check if we have an obstacle in our way and clear it ALSO need a mounted CHECK!!.
        {
            return new Sequence(
                       // No Target?
                       // Targeting Enabled?
                       // Aquire Target
                       new DecoratorContinue(ret => (onUnit == null || onUnit(ret) == null || onUnit(ret).IsDead) && CLUSettings.Instance.EnableTargeting,
                                             new PrioritySelector(
                                             	ctx => {
                                                    // Clear our current target if its Dead.
                                                    if (onUnit(ctx).IsDead)
                                                    {
                                                        CLULogger.TroubleshootLog(" Target Appears to be dead. Clearing Current Target [" + CLULogger.SafeName((WoWUnit)ctx) + "]");
                                                        StyxWoW.Me.ClearTarget();
                                                    }
                                             		// Aquires a target.
                                             		if (Unit.EnsureUnitTargeted != null)
                                             		{
                                             			// Clear our current target if its blacklisted.
                                             			if (Blacklist.Contains(Unit.EnsureUnitTargeted.Guid)) {
                                             				StyxWoW.Me.ClearTarget();
                                             			}
                                             			return Unit.EnsureUnitTargeted;
                                             		}

                                             		return null;
                                             	},
                                             	new Decorator(
                                             		ret => ret != null, //checks that the above ctx returned a valid target.
                                             		new Sequence(
                                                        //new Action(ret => SysLog.DiagnosticLog(" CLU targeting activated. Targeting " + SysLog.SafeName((WoWUnit)ret))),
                                             			new Action(ret => ((WoWUnit)ret).Target()),
                                             			new WaitContinue(2, ret => onUnit(ret) != null && onUnit(ret) == (WoWUnit)ret, new ActionAlwaysSucceed()))))),


                       // Are we Facing the target?
                       // Is the Target in line of site?
                       // Face Target
                       new DecoratorContinue(ret => onUnit(ret) != null && !StyxWoW.Me.IsSafelyFacing(onUnit(ret), 45f) && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => WoWMovement.Face(onUnit(ret).Guid)))),

                       // Target in Line of site?
                       // We are not casting?
                       // We are not channeling?
                       // Move to Location
                       new DecoratorContinue(ret => onUnit(ret) != null && !onUnit(ret).InLineOfSight && !Me.IsCasting && !Spell.PlayerIsChanneling,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Target not in LoS. Moving closer.")),
                                                 new Action(ret => Navigator.MoveTo(onUnit(ret).Location)))),

                       // Blacklist targets TODO:  check this.
                       new DecoratorContinue(ret => onUnit(ret) != null && !Me.IsInInstance && Navigator.GeneratePath(Me.Location, onUnit(ret).Location).Length <= 0,
                                             new Action(delegate {
                                                        	Blacklist.Add(Me.CurrentTargetGuid, TimeSpan.FromDays(365));
                                                            CLULogger.MovementLog("[CLU] " + CLU.Version + ": Failed to generate path to: {0} blacklisted!", Me.CurrentTarget.Name);
                                                        	return RunStatus.Success;
                                                        })),

                       // Move Behind Target enabled?
                       // Target is Alive?
                       // Target is not Moving?
                       // Are we behind the target?
                       // We are not casting?
                       // We are not the tank?
                       // We are not channeling?
                       // Move Behind Target
                       new DecoratorContinue(ret => CLUSettings.Instance.EnableMoveBehindTarget && onUnit(ret) != null && onUnit(ret) != StyxWoW.Me  && // && !onUnit(ret).IsMoving
                                             onUnit(ret).InLineOfSight && onUnit(ret).IsAlive && !onUnit(ret).MeIsBehind && !Me.IsCasting && !Spell.PlayerIsChanneling &&
                                             Unit.DistanceToTargetBoundingBox() >= 10,// && (Unit.Tanks != null && Unit.Tanks.Any(x => x.Guid != Me.Guid))
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Not behind the target. Moving behind target.")),
                                                 new Action(ret => Navigator.MoveTo(CalculatePointBehindTarget())))),

                       // Target is greater than CombatMinDistance?
                       // Target is Moving?
                       // We are not moving Forward?
                       // Move Forward towards target
                       new DecoratorContinue(ret => onUnit(ret) != null && Unit.DistanceToTargetBoundingBox() >= CLU.Instance.ActiveRotation.CombatMinDistance &&
                                             onUnit(ret).IsMoving && !Me.MovementInfo.MovingForward && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too far away from moving target (T[{0}] >= P[{1}]). Moving forward.", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMinDistance)),
                                                 new Action(ret => WoWMovement.Move(WoWMovement.MovementDirection.Forward)))),

                       // Target is less than CombatMinDistance?
                       // Target is Moving?
                       // We are moving Forward
                       // Stop Moving Forward
                       new DecoratorContinue(ret => onUnit(ret) != null && Unit.DistanceToTargetBoundingBox() < CLU.Instance.ActiveRotation.CombatMinDistance &&
                                             onUnit(ret).IsMoving && Me.MovementInfo.MovingForward && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too close to target (T[{0}] < P[{1}]). Movement Stopped.", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMinDistance)),
                                                 new Action(ret => WoWMovement.MoveStop()))),

                       // Target is not Moving?
                       // Target is greater than CombatMaxDistance?
                       // We are not Moving?
                       // Move Forward
                       new DecoratorContinue(ret => onUnit(ret) != null && !onUnit(ret).IsMoving &&
                                             Unit.DistanceToTargetBoundingBox() >= CLU.Instance.ActiveRotation.CombatMaxDistance && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too far away from non moving target (T[{0}] >= P[{1}]). Moving forward.", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMaxDistance)),
                                                 new Action(ret => WoWMovement.Move(WoWMovement.MovementDirection.Forward, new TimeSpan(99, 99, 99))))),

                       // Target is less than CombatMaxDistance?
                       // We are Moving?
                       // We are moving Forward?
                       // Stop Moving
                       new DecoratorContinue(ret => onUnit(ret) != null && Unit.DistanceToTargetBoundingBox() < CLU.Instance.ActiveRotation.CombatMaxDistance &&
                                             Me.IsMoving && Me.MovementInfo.MovingForward && onUnit(ret).InLineOfSight,
                                             new Sequence(
                                                 new Action(ret => CLULogger.MovementLog(" [CLU Movement] Too close to target  (T[{0}] < P[{1}]). Movement Stopped", Unit.DistanceToTargetBoundingBox(), CLU.Instance.ActiveRotation.CombatMaxDistance)),
                                                 new Action(ret => WoWMovement.MoveStop()))));
        }

        /// <summary>
        /// Calculates the point to move behind the targets location
        /// </summary>
        /// <returns>the WoWpoint location to move to</returns>
        private static WoWPoint CalculatePointBehindTarget()
        {
            return
                StyxWoW.Me.CurrentTarget.Location.RayCast(
                    StyxWoW.Me.CurrentTarget.Rotation + WoWMathHelper.DegreesToRadians(150), Spell.MeleeRange - 2f);
        }

        /// <summary>
        /// Will stop the player if we are moving.
        /// </summary>
        /// <returns>Runstatus.Success if stopped</returns>
        public static Composite EnsureMovementStoppedBehavior()
        {
            return new Decorator(
                ret => CLUSettings.Instance.EnableMovement && StyxWoW.Me.IsMoving,
                new Action(ret => Navigator.PlayerMover.MoveStop()));
        }
    }
}
