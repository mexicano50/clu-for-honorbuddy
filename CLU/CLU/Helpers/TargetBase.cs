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
using System.Collections.Generic;
using System.Linq;
using Styx;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using System.Diagnostics;
using CLU.Base;
using Action = Styx.TreeSharp.Action;


namespace CLU.Helpers
{
    public class TargetBase
    {
        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }

        // Unfriendly's...
        public static IEnumerable<WoWUnit> Hostile
        {
            get
            {
                return ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Where(x => Unit.IsAttackable(x) && !Unit.UnitIsControlled(x, true));
            }
        }

        public delegate bool RefineFilter(HealableUnit unit);

        private static readonly TargetBase instance = new TargetBase();

        public static TargetBase Instance
        {
            get
            {
                return instance;
            }
        }

        private TargetBase() { }

        private static bool IsPlayingWoW()
        {
            return Me != null && Me.IsValid; //TODO: ObjectManager.IsInGame && ....WHERE HAS THIS MOVED TO? --wulf
        }

        private static void Log(string s, params object[] a)
        {
            CLULogger.DiagnosticLog(s, a);
        }

        private static void Logparty(string s, params object[] a)
        {
            CLULogger.DiagnosticLog(s, a);
        }

        private static void Logpartymatchs(string s, params object[] a)
        {
            CLULogger.DiagnosticLog(s, a);
        }

        private static IEnumerable<HealableUnit> UnitsFilter(TargetFilter filter)
        {
            var mobs = new List<WoWPlayer>();

            if (HealableUnit.ListofHealableUnits != null)
            {
                List<HealableUnit> grp = HealableUnit.ListofHealableUnits.ToList();

                switch (filter)
                {
                    case TargetFilter.None:
                        {
                            return grp.Where(x => x != null && !x.Blacklisted).Select(n => n);
                        }

                    case TargetFilter.Tanks:
                        {
                            return grp.Where(x => x != null && (x.Tank && !x.Blacklisted)).Select(t => t);
                        }

                    case TargetFilter.Healers:
                        {

                            return grp.Where(x => x != null && (x.Healer && !x.Blacklisted)).Select(h => h);
                        }

                    case TargetFilter.Damage:
                        {
                            return grp.Where(x => x != null && (x.Damage && !x.Blacklisted)).Select(d => d);
                        }

                    //// PvP Based targeting...
                    //case TargetFilter.EnemyHealers:
                    //    {
                    //        var enemyHealers = hostile.Where(x => x.IsPlayer && x.DistanceSqr < 30 * 30 && x.InLineOfSpellSight && x.IsCasting && Unit.HealerSpells.Contains(x.CastingSpell.Name));
                    //        return mobs.Where(x => enemyHealers.Any(y => y.Guid == x.Guid));
                    //    }

                    //case TargetFilter.FlagCarrier:
                    //    {
                    //        var flagCarriers = hostile.Where(x => x.IsPlayer && x.DistanceSqr < 30 * 30 && x.InLineOfSpellSight && (Me.IsHorde ? x.HasAura("Alliance Flag") : x.HasAura("Horde Flag")));
                    //        return mobs.Where(x => flagCarriers.Any(y => y.Guid == x.Guid));
                    //    }

                    //case TargetFilter.Threats:
                    //    {
                    //        var threats = hostile.Where(x => x.IsPlayer && x.DistanceSqr < 15 * 15 && x.InLineOfSpellSight && x.IsTargetingMeOrPet);
                    //        return mobs.Where(x => threats.Any(y => y.Guid == x.Guid));
                    //    }

                    //case TargetFilter.LowHealth:
                    //    {
                    //        var lowhealthers = hostile.Where(x => x.IsPlayer && x.DistanceSqr < 15 * 15 && x.InLineOfSpellSight && x.HealthPercent < 30);
                    //        return mobs.Where(x => lowhealthers.Any(y => y.Guid == x.Guid));
                    //    }

                    default:
                        return null;
                }
            }
            return null;
        }

        private static Composite FindTarget(CanRunDecoratorDelegate cond, TargetFilter filter, RefineFilter refineFilter, Comparison<HealableUnit> compare, string label, params Composite[] children)
        {
            return new Decorator(
                       cond,
                       new Sequence(
                         // get a target
                           new Action(
                            delegate
                            {
                                var targetPerformanceTimer = new Stopwatch(); // lets see if we can get some performance on this one.
                                targetPerformanceTimer.Start(); // lets see if we can get some performance on this one.

                                //CrabbyProfiler.Instance.Runs.Add(new Run("FindTarget"));
                                
                                // Nothing to filter against
                                if (!UnitsFilter(filter).Any())
                                {
                                    HealableUnit.HealTarget = null;
                                    return RunStatus.Failure;
                                }


                                // Filter the Healable Units
                                var raid = UnitsFilter(filter).Where(x => x != null && (ObjectManager.ObjectList.Any(y => y.Guid == x.ToUnit().Guid) && refineFilter(x)) && x.ToUnit().Distance2DSqr < 40 * 40 && !x.ToUnit().ToPlayer().IsGhost && !x.ToUnit().HasAura("Deep Corruption")).ToList();

                                // Nothing to heal.
                                if (!IsPlayingWoW() || !raid.Any())
                                {
                                    HealableUnit.HealTarget = null;
                                    return RunStatus.Failure;
                                }

                                raid.Sort(compare);
                                var target = raid.FirstOrDefault();
                                if (target != null)
                                {
                                    Log(
                                        label,
                                        CLULogger.SafeName(target.ToUnit()),
                                        target.MaxHealth,
                                        target.CurrentHealth,
                                        target.MaxHealth - target.CurrentHealth,
                                        targetPerformanceTimer.ElapsedMilliseconds); // lets see if we can get some performance on this one.

                                    //target.ToUnit().Target();
                                    HealableUnit.HealTarget = target;
                                    return RunStatus.Success;
                                }
                                HealableUnit.HealTarget = null;
                                //CrabbyProfiler.Instance.EndLast();
                                return RunStatus.Failure;
                            }),
                           new Action(a => StyxWoW.SleepForLagDuration()),
                // if success, keep going. Else quit sub routine
                           new PrioritySelector(children)));
        }

        public Composite FindTank(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.Tanks, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting TANK {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindHealer(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.Healers, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting HEALER {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindDPS(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.Damage, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting DPS {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindRaidMember(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.None, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting RAID MEMBER {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindEnemyHealers(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.EnemyHealers, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting ENEMY HEALER {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindFlagCarrier(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.FlagCarrier, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting FLAG CARRIER {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindThreats(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.Threats, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting THREAT {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        public Composite FindLowHealth(CanRunDecoratorDelegate cond, RefineFilter filter, Comparison<HealableUnit> compare, string reason, params Composite[] children)
        {
            return FindTarget(cond, TargetFilter.LowHealth, filter, compare, "[CLU TARGETING] " + CLU.Version + ": " + "Targeting LOW HEALTHER {0}:  Max {1}, Current {2}, Defecit {3}, TimeTaken: {4} ms, REASON: " + reason, children);
        }

        /// <summary>
        /// Finds a Party that needs healing
        /// </summary>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="minAverageHealth">Minimum Average health of the Party Members</param>
        /// <param name="maxAverageHealth">MaximumAverage health of the Party Members</param>
        /// <param name="maxDistanceBetweenPlayers">The maximum distance between other party members from the targeted party member</param>
        /// <param name="minUnits">Minumum units to be affected</param>
        /// <param name="reason">text to indicate the reason for using this method </param>
        /// <param name="children">Execute the child subroutines</param>
        /// <returns>A party member</returns>
        public Composite FindParty(CanRunDecoratorDelegate cond, int minAverageHealth, int maxAverageHealth, float maxDistanceBetweenPlayers, int minUnits, string reason, params Composite[] children)
        {
            return new Decorator(
                       cond,
                       new Sequence(
                           new PrioritySelector(
                               FindPartySubroutine(0, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 1 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(1, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 2 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(2, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 3 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(3, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 4 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(4, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 5 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(5, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 6 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(6, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 7 MEMBER: {0} REASON: " + reason),
                               FindPartySubroutine(7, minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target PARTY 8 MEMBER: {0} REASON: " + reason)
                           ),
                           new Action(a => StyxWoW.SleepForLagDuration()),
                // if success, keep going. Else quit sub routine
                           new PrioritySelector(children)
                       )
                   );
        }

        /// <summary>
        /// Finds Raid or Party members that need healing
        /// </summary>
        /// <param name="cond">The conditions that must be true</param>
        /// <param name="minAverageHealth">Minimum Average health of the Party Members</param>
        /// <param name="maxAverageHealth">MaximumAverage health of the Party Members</param>
        /// <param name="maxDistanceBetweenPlayers">The maximum distance between other party members from the targeted party member</param>
        /// <param name="minUnits">Minumum units to be affected</param>
        /// <param name="reason">text to indicate the reason for using this method </param>
        /// <param name="children">Execute the child subroutines</param>
        /// <returns>A Raid/Party member</returns>
        public Composite FindAreaHeal(CanRunDecoratorDelegate cond, int minAverageHealth, int maxAverageHealth, float maxDistanceBetweenPlayers, int minUnits, string reason, params Composite[] children)
        {
            return new Decorator(
                       cond,
                       new Sequence(
                           new PrioritySelector(
                               FindAreaHealSubroutine(minAverageHealth, maxAverageHealth, maxDistanceBetweenPlayers, minUnits, "[CLU TARGETING] " + CLU.Version + ": " + "Target AREA MEMBER: {0} REASON: " + reason)
                           ),
                           new Action(a => StyxWoW.SleepForLagDuration()),
                // if success, keep going. Else quit sub routine
                           new PrioritySelector(children)
                       )
                   );
        }

        /// <summary>
        /// Finds a Party that needs healing
        /// </summary>
        /// <param name="partyIndex">the party index</param>
        /// <param name="minAverageHealth">Minimum Average health of the Party Members</param>
        /// <param name="maxAverageHealth">MaximumAverage health of the Party Members</param>
        /// <param name="maxDistanceBetweenPlayers">The maximum distance between other party members from the targeted party member</param>
        /// <param name="minUnits">Minumum units to be affected</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>A party member</returns>
        private Composite FindPartySubroutine(int partyIndex, int minAverageHealth, int maxAverageHealth, float maxDistanceBetweenPlayers, int minUnits, string label)
        {

            return new Action(a =>
            {
                var targetPartyPerformanceTimer = new Stopwatch(); // lets see if we can get some performance on this one.
                targetPartyPerformanceTimer.Start(); // lets see if we can get some performance on this one.

                // copy
                var grp = HealableUnit.ListofHealableUnits.ToList();

                // gtfo if there is no heal list.
                if (!grp.Any())
                {
                    HealableUnit.HealTarget = null;
                    return RunStatus.Failure;
                }
                // setup a quick filter and exctract our players.
                RefineFilter refineFilter = x => x.ToUnit().Distance2DSqr < 40 * 40 && ObjectManager.ObjectList.Any(y => y.Guid == x.ToUnit().Guid) && x.ToUnit().IsAlive && !x.ToUnit().ToPlayer().IsGhost && x.ToUnit().IsPlayer && x.ToUnit().ToPlayer() != null && !x.ToUnit().IsFlying && !x.ToUnit().OnTaxi;

                var players = grp.Where(x => refineFilter(x) && (x.GroupNumber == partyIndex) && !x.Blacklisted && !x.ToUnit().HasAura("Deep Corruption"));

                // Nothing to heal.
                if (!players.Any())
                {
                    {
                        HealableUnit.HealTarget = null;
                        return RunStatus.Failure;
                    }
                }

                var matchs = new List<HealableUnit>();
                HealableUnit best = null;
                int score = minUnits - 1;
                foreach (var player in players)
                {
                    var hits = players.Where(p => p.Location.Distance2DSqr(player.Location) < maxDistanceBetweenPlayers * maxDistanceBetweenPlayers);
                    if (hits.Any())
                    {
                        var avgHealth = hits.Average(p => p.CurrentHealth * 100 / p.MaxHealth);
                        var count = hits.Count();
                        if (avgHealth >= minAverageHealth && avgHealth < maxAverageHealth && count > score)
                        {
                            best = player;
                            score = count;
                        }
                    }

                    matchs.Add(best); // TODO: Print this out, I want to see if we are choosing the best target for our party/area heal to hit the surrounding targets.
                }

                if (best != null)
                {
                    Logparty(label + " Time Taken: " + targetPartyPerformanceTimer.ElapsedMilliseconds + " ms", CLULogger.SafeName(best.ToUnit()));
                    //best.ToUnit().Target();
                    HealableUnit.HealTarget = best;
                    return RunStatus.Success;
                }

                HealableUnit.HealTarget = null;
                return RunStatus.Failure;
            });
        }

        /// <summary>
        /// Finds units within range of each other that need healing
        /// </summary>
        /// <param name="minAverageHealth">Minimum Average health of the Party Members</param>
        /// <param name="maxAverageHealth">MaximumAverage health of the Party Members</param>
        /// <param name="maxDistanceBetweenPlayers">The maximum distance between other members from the targeted member</param>
        /// <param name="minUnits">Minumum units to be affected</param>
        /// <param name="label">A descriptive label for the clients GUI logging output</param>
        /// <returns>A Riad/Party member</returns>
        private Composite FindAreaHealSubroutine(int minAverageHealth, int maxAverageHealth, float maxDistanceBetweenPlayers, int minUnits, string label)
        {
            return new Action(a =>
            {

                var targetAreaPerformanceTimer = new Stopwatch(); // lets see if we can get some performance on this one.
                targetAreaPerformanceTimer.Start(); // lets see if we can get some performance on this one

                // copy
                List<HealableUnit> grp = HealableUnit.ListofHealableUnits.ToList();

                // gtfo if there is no heal list.
                if (!grp.Any())
                {
                    HealableUnit.HealTarget = null;
                    return RunStatus.Failure;
                }

                // setup a quick filter and exctract our players.
                RefineFilter refineFilter = x => x.ToUnit().Location.DistanceSqr(Me.Location) < 40 * 40 && ObjectManager.ObjectList.Any(y => y.Guid == x.ToUnit().Guid) && x.ToUnit().IsAlive && !x.ToUnit().ToPlayer().IsGhost && x.ToUnit().IsPlayer && x.ToUnit().ToPlayer() != null && !x.ToUnit().IsFlying && !x.ToUnit().OnTaxi;

                var players = grp.Where(x => refineFilter(x) && !x.Blacklisted && !x.ToUnit().HasAura("Deep Corruption"));

                // Nothing to heal.
                if (!players.Any())
                {
                    {
                        HealableUnit.HealTarget = null;
                        return RunStatus.Failure;
                    }
                }

                HealableUnit best = null;
                int score = minUnits - 1;
                foreach (var player in players)
                {
                    var hits = players.Where(p => p.Location.Distance2DSqr(player.Location) < maxDistanceBetweenPlayers * maxDistanceBetweenPlayers);

                    if (hits.Any())
                    {
                        var avgHealth = hits.Average(p => p.CurrentHealth * 100 / p.MaxHealth);
                        var count = hits.Count();
                        if (avgHealth >= minAverageHealth && avgHealth < maxAverageHealth && count > score)
                        {
                            best = player;
                            score = count;
                        }
                    }
                }

                if (best != null)
                {
                    Logparty(label + " Time Taken: " + targetAreaPerformanceTimer.ElapsedMilliseconds + " ms", CLULogger.SafeName(best.ToUnit()));
                    //best.ToUnit().Target();
                    HealableUnit.HealTarget = best;
                    return RunStatus.Success;
                }
                HealableUnit.HealTarget = null;
                return RunStatus.Failure;
            });
        }

        /// <summary>Targeting myself when no target</summary>
        /// <param name="cond">The conditions that must be true</param>
        /// <returns>Target myself</returns>
        public static Composite EnsureTarget(CanRunDecoratorDelegate cond)
        {
            return new Decorator(
                       cond,
                       new Sequence(
                           new Action(a => CLULogger.TroubleshootLog("[CLU] " + CLU.Version + ": CLU targeting activated. I dont have a target, someone must have died. Targeting myself")),
                           new Action(a => Me.Target())));
        }
    }
}
