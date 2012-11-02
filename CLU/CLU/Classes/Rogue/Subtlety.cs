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

using CLU.Base;
using CLU.Helpers;
using CLU.Managers;
using CLU.Settings;

using JetBrains.Annotations;

using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using Action = Styx.TreeSharp.Action;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Rogue
{
    [UsedImplicitly]
    public class Subtlety : RotationBase
    {
        #region Constants and Fields

        private const string HemorrhageSubstitute = "Sinister Strike";

        private static IEnumerable<WoWUnit> _aoeTargets;
        private static bool _tricksTargetChecked;
        private static WoWUnit _tricksTarget;

        #endregion

        #region Public Properties

        public override float CombatMaxDistance
    {
            get { return 3.2f; }
        }

        // adding some help
        public override string Help
        {
            get
            {
                return "\n" + "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Attempt to evade/escape crowd control with Evasion, Cloak of Shadows, Smoke Bomb, Combat Readiness.\n" +
                       "2. Rotation is set up for Hemorrhage\n" + "3. AutomaticCooldowns has: \n" + "==> UseTrinkets \n" +
                       "==> UseRacials \n" + "==> UseEngineerGloves \n" + "4. Attempt to reduce threat with Feint\n" +
                       "5. Will interupt with Kick\n" + "6. Tricks of the Trade on best target (tank, then class)\n" +
                       "7. Will heal with Recuperate and a Healthstone\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude, kbrebel04\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override string KeySpell
        {
            get { return HemorrhageSubstitute; }
            }

        public override int KeySpellId
        {
            get { return 16511; }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator
                    (
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector
                        (
                        Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone"),
                        Spell.CastSelfSpell
                            (
                             "Smoke Bomb",
                             ret =>
                             Me.CurrentTarget != null && Me.HealthPercent < 30 && Me.CurrentTarget.IsTargetingMeOrPet,
                             "Smoke Bomb"),
                        Spell.CastSelfSpell
                            (
                             "Combat Readiness",
                             ret =>
                             Me.CurrentTarget != null && Me.HealthPercent < 40 && Me.CurrentTarget.IsTargetingMeOrPet,
                             "Combat Readiness"),
                        Spell.CastSelfSpell
                            (
                             "Evasion",
                             ret =>
                             Me.HealthPercent < 35 &&
                             Unit.EnemyMeleeUnits.Count(u => u.DistanceSqr < 6 * 6 && u.IsTargetingMeOrPet) >= 1, "Evasion"),
                        Spell.CastSelfSpell
                            (
                             "Cloak of Shadows",
                             ret => Unit.EnemyMeleeUnits.Count(u => u.IsTargetingMeOrPet && u.IsCasting) >= 1,
                             "Cloak of Shadows"), Poisons.CreateApplyPoisons()));
            }
        }

        public override string Name
        {
            get { return "Subtlety Rogue"; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
            }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator
                    (
                    ret =>
                    !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") &&
                    !Me.HasAura("Drink"), new PrioritySelector
                                              (
                                              // Stealth
                                              Spell.CastSelfSpell
                                                  (
                                                   "Stealth",
                                                   ret =>
                                                   !Buff.PlayerHasBuff("Stealth") &&
                                                   CLUSettings.Instance.Rogue.EnableAlwaysStealth && !CLU.IsMounted,
                                                   "Stealth"), Poisons.CreateApplyPoisons()));
            }
        }

        public override Composite Pull
        {
            get { return new PrioritySelector(Spell.CastSpell("Throw", ret => Me.CurrentTarget.Distance <= 30, "Throw for Pull")); }
        }

        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
            }

        public override string Revision
        {
            get { return "$Rev$"; }
        }

        private static Action Reset
        {
            get
            {
                return new Action
                    (delegate
        {
                        _aoeTargets = null;
                        return RunStatus.Failure;
                    });
            }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new Decorator
                    (
                    cond =>
                    Me.CurrentTarget != null && Me.CurrentTarget.Attackable && Me.CurrentTarget.IsWithinMeleeRange,
                    new PrioritySelector(Reset, Stealthed, NotStealthed));
            }
        }

        #endregion

        #region Properties

        private static bool AmStealthed
        {
            get
            {
                return Me.IsStealthed || Buff.PlayerActiveBuffTimeLeft("Subterfuge") > TimeSpan.Zero ||
                       Buff.PlayerActiveBuffTimeLeft("Shadow Dance") > TimeSpan.Zero;
            }
        }

        private static bool AnticipationSafe
        {
            get { return ( HasAnticipation && Me.Auras["Anticipation"].StackCount < 4 ); }
        }

        private static IEnumerable<WoWUnit> AoETargets
        {
            get
            {
                if ( _aoeTargets == null )
                {
                    try
                    {
                        _aoeTargets = ObjectManager.GetObjectsOfType<WoWUnit>(false, false).Where
                            (
                             unit =>
                             !unit.IsFriendly && !unit.IsCritter && !unit.IsNonCombatPet && !unit.IsPlayer && unit.Attackable &&
                             unit.DistanceSqr <= 8 * 8 && !unit.IsDead);
                    }
                    catch (Exception)
                    {

                        _aoeTargets = new HashSet<WoWUnit>();
                    }
                }
                return _aoeTargets;
            }
        }

        private static bool BehindTarget
        {
            get { return Me.CurrentTarget != null && Me.IsBehind(Me.CurrentTarget); }
        }

        private static Composite ComboPointGen
        {
            get
            {
                return new Decorator
                    (
                    cond => ( AnticipationSafe || HATSafe ) && !AmStealthed,
                    new PrioritySelector
                        (
                        Spell.CastSpell
                            (
                             "Fan of Knives",
                             ret =>
                             AoETargets.All(FoKSafe) &&
                             AoETargets.Count() >= CLUSettings.Instance.Rogue.SubtletyFanOfKnivesCount &&
                             CLUSettings.Instance.UseAoEAbilities, "Fan of Knives"),
                        Spell.CastSpell
                            (
                             HemorrhageSubstitute,
                             ret => Buff.TargetDebuffTimeLeft("Hemorrhage") < TimeSpan.FromSeconds(3), "Hemo Debuff"),
                        Spell.CastSpell
                            (
                             "Backstab", ret => Buff.TargetDebuffTimeLeft("Hemorrhage") >= TimeSpan.FromSeconds(3) && BehindTarget,
                             "Backstab"),
                        Spell.CastSpell(HemorrhageSubstitute, ret => Me.CurrentEnergy > 35, "Hemorrhage")));
            }
        }

        private static Composite Cooldowns
        {
            get
            {
                return new Decorator
                    (
                    cond => CLUSettings.Instance.UseCooldowns,
                    new PrioritySelector
                        (
                        Spell.CastSpell("Shadow Dance", ret => BuffsSafeForSD, "Shadow Dance"),
                        Spell.CastSpell("Shadow Blades", ret => BuffsSafeForSB, "Shadow Blades"),
                        Spell.CastSpell("Vanish", ret => SnDSafe && RuptureSafe && !AmStealthed
                            && !Me.CurrentTarget.IsTargetingMeOrPet && Me.IsInMyPartyOrRaid
                            && Buff.PlayerActiveBuffTimeLeft("Shadow Dance") == TimeSpan.Zero, "Vanish"),
                        Spell.CastSpell
                            (
                             "Preparation",
                             ret => SpellManager.HasSpell(14185) && SpellManager.Spells["Vanish"].Cooldown,
                             "Preparation")));
            }
        }

        private static bool BuffsSafeForSB
        {
            get { return Buff.PlayerActiveBuffTimeLeft("Slice and Dice") > TimeSpan.FromSeconds(12) && Buff.PlayerActiveBuffTimeLeft("Rupture") > TimeSpan.FromSeconds(12); }
        }

        private static bool BuffsSafeForSD
        {
            get { return Buff.PlayerActiveBuffTimeLeft("Slice and Dice") > TimeSpan.FromSeconds(8) && Buff.PlayerActiveBuffTimeLeft("Rupture") > TimeSpan.FromSeconds(8); }
        }

        private static bool CrimsonTempestDown
        {
            get
            {
                return AoETargets.Any
                    (
                     x =>
                     !x.Debuffs.Any(y => y.Key.Equals("Crimson Tempest", StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        private static Composite Finishers
        {
            get
        {
                return new Decorator
                    (
                    cond => Me.ComboPoints > 1,
                    new PrioritySelector
                        (
                        Spell.CastSpell
                            (
                             "Crimson Tempest",
                             ret =>
                             AoETargets.Count() > 4 && AoETargets.All(FoKSafe) && CrimsonTempestDown &&
                             CLUSettings.Instance.UseAoEAbilities, "Crimson Tempest"),
                        Spell.CastSpell
                            ("Eviscerate", ret => Me.ComboPoints >= 5 && SnDSafe && RuptureSafe, "Eviscerate"),
                        Spell.CastSpell("Rupture", ret => Me.ComboPoints >= 5 && SnDSafe && !RuptureSafe, "Rupture"),
                        Spell.CastSpell
                            (
                             "Slice and Dice", ret => Buff.PlayerActiveBuffTimeLeft("Slice and Dice").TotalSeconds < 2,
                             "SnD")));
            }
        }

        private static bool HATSafe
        {
            get
            {
                return ( Me.ComboPoints == 3 && Me.EnergyPercent > 40 ) ||
                       ( Me.ComboPoints == 4 && Me.EnergyPercent > 70 );
            }
        }

        private static bool HasAnticipation
        {
            get { return TalentManager.HasTalent(18); }
        }

        private static Composite NotStealthed
        {
            get
            {
                return new Decorator
                    (cond => !AmStealthed, new PrioritySelector(Situationals, Cooldowns, Finishers, ComboPointGen));
            }
        }

        private static bool RuptureSafe
        {
            get { return Buff.TargetDebuffTimeLeft("Rupture") >= TimeSpan.FromSeconds(5); }
        }

        private static bool SafeToBreakStealth  
        {
            get
            {
                return (Me.Combat || Me.RaidMembers.Any(rm => rm.Combat) || Unit.IsTrainingDummy(Me.CurrentTarget)) &&
                       Unit.UseCooldowns();
            }
        }

        private static Composite Situationals
        {
            get
            {
                return new PrioritySelector
                    (Spell.CastSelfSpell("Feint", ret => Me.CurrentTarget != null && ( EncounterSpecific.IsMorchokStomp() ), "Feint"),
                     Spell.CastSpell("Tricks of the Trade", u => TricksTarget, ret => TricksTarget != null, "Tricks of the Trade"),
                     Spell.CastInterupt("Kick", ret => Me.IsWithinMeleeRange, "Kick"),
                     Spell.CastSpell("Redirect", ret => Me.RawComboPoints > 0 && Me.ComboPoints < 1, "Redirect"));
            }
        }

        private static bool SnDSafe
        {
            get { return Buff.PlayerActiveBuffTimeLeft("Slice and Dice") >= TimeSpan.FromSeconds(3); }
        }

        private static Composite StealthCPGen
        {
            get
            {
                return new Decorator
                    (
                    cond => AnticipationSafe || HATSafe,
                    new PrioritySelector
                        (
                        Spell.CastSpell
                            ("Garrote", ret => Buff.TargetDebuffTimeLeft("Garrote") == TimeSpan.Zero, "Garrote"),
                        Spell.CastSpell("Ambush", ret => Me.ComboPoints < 4 || AnticipationSafe, "Ambush"),
                        Spell.CastSpell
                            (
                             HemorrhageSubstitute, ret => ( Me.ComboPoints < 5 || AnticipationSafe ) && !BehindTarget,
                             "Hemorrhage @ Stealthed")));
            }
        }

        private static Composite Stealthed
        {
            get
            {
                return new Decorator
                    (
                    cond => AmStealthed,
                    new PrioritySelector
                        (Spell.CastSpell(14183, ret => Me.ComboPoints < 4, "Premeditation"), Finishers, StealthCPGen));
            }
        }

        private static WoWUnit TricksTarget
        {
            get
            {
                if (_tricksTargetChecked)
                {
                    return _tricksTarget;
                }

                if (_tricksTarget == null)
                {
                    _tricksTarget = Unit.BestTricksTarget;
                    _tricksTargetChecked = true;
                }

                return _tricksTarget;
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
            return !Unit.UnitIsControlled(unit, true);
        }


        private static void StealthedCombat()
        {
            if ( Me.CurrentTarget == null || Me.CurrentTarget.IsDead ||
                 ( !Me.CurrentTarget.IsHostile && !Unit.IsTrainingDummy(Me.CurrentTarget) ) ||
                 !Me.CurrentTarget.Attackable )
            {
                return;
            }

            if ( ( !Me.IsStealthed && !Buff.PlayerHasActiveBuff("Vanish") ) || !SafeToBreakStealth ||
                 Buff.PlayerActiveBuffTimeLeft("Subterfuge") > TimeSpan.Zero )
            {
                return;
        }

            // If we're not behind, attempt to shadowstep and wait for next pulse.
            if ( SpellManager.HasSpell("Shadowstep") && !StyxWoW.Me.IsBehind(Me.CurrentTarget) &&
                 SpellManager.CanCast("Shadowstep", Me.CurrentTarget) )
            {
                CLULogger.Log(" [Casting] Shadowstep on {0} @ StealthedCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Shadowstep");
            }
            else if ( Buff.TargetDebuffTimeLeft("Garrote") ==
                      TimeSpan.Zero )
            {
                CLULogger.Log(" [Casting] Garrote on {0} @ StealthCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Garrote");
            }
            else if ( BehindTarget )
        {
                CLULogger.Log(" [Casting] Ambush on {0} @ StealthCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast("Ambush");
            }
            else
            {
                CLULogger.Log(" [Casting] Hemorrhage on {0} @ StealthedCombat", CLULogger.SafeName(Me.CurrentTarget));
                SpellManager.Cast(HemorrhageSubstitute);
            }
        }

        #endregion
    }
}
