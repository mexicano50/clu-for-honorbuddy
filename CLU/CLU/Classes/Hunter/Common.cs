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

using CommonBehaviors.Actions;
using Styx;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using CLU.Base;
using CLU.Managers;
using CLU.Settings;

namespace CLU.Classes.Hunter
{

    /// <summary>
    /// Common Hunter Functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// erm..me
        /// </summary>
        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }

        private static WoWUnit Pet
        {
            get { return StyxWoW.Me.Pet; }
        }

        /// <summary>
        /// Switch to Apect of the fox while moving then back to aspect of the hawk (or Iron Hawk)
        /// </summary>
        public static Composite HandleAspectSwitching()
        {
            return new PrioritySelector(
                    new Decorator( ret => Me.IsMoving && CLUSettings.Instance.Hunter.HandleAspectSwitching,
                        new Sequence(
                            // Waiting for a bit just incase we are only moving outa the fire!
                            new WaitContinue(2, ret => false, new ActionAlwaysSucceed()), // Hmm..check this...-- wulf
                            Buff.CastBuff("Aspect of the Fox", ret => Me.IsMoving, "[Aspect] of the Fox - Moving"),
                            new PrioritySelector(
                                Buff.CastBuff("Aspect of the Hawk", ret => !Me.IsMoving && !TalentManager.HasTalent(8), "[Aspect] of the Hawk"),
                                Buff.CastBuff("Aspect of the Hawk", "Aspect of the Iron Hawk", ret => !Me.IsMoving && TalentManager.HasTalent(8), "Aspect of the Iron Hawk")
                                )
                                )),
                    new Decorator( ret => !Me.IsMoving && CLUSettings.Instance.Hunter.HandleAspectSwitching,
                        new PrioritySelector(
                                Buff.CastBuff("Aspect of the Hawk", ret => !Me.IsMoving && !TalentManager.HasTalent(8), "[Aspect] of the Hawk"),
                                Buff.CastBuff("Aspect of the Hawk", "Aspect of the Iron Hawk", ret => !Me.IsMoving && TalentManager.HasTalent(8), "Aspect of the Iron Hawk")
                                )));
        }

        /// <summary>
        /// Will use Misdirection on the on the following untis in order of priority : Focus, Pet, RafLeader, Tank
        /// </summary>
        public static Composite HandleMisdirection()
        {
            return new Decorator(ret => CLUSettings.Instance.Hunter.UseMisdirection,
                        new PrioritySelector(
                            Spell.CastSpell("Misdirection", u => Unit.BestMisdirectTarget, ret => Me.CurrentTarget != null && !Buff.PlayerHasBuff("Misdirection") && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Misdirection")));
        }

        /// <summary>
        /// Pet helper for misc pet abilitys.
        /// </summary>
        public static Composite HandlePetHelpers()
        {
            return new Decorator(ret => !Buff.PlayerHasBuff("Feign Death"),
                               new PrioritySelector(
                                  // Spell.CastSpell("Mend Pet", ret => Me.GotAlivePet && (Me.Pet.HealthPercent < CLUSettings.Instance.Hunter.MendPetPercent) && !PetManager.PetHasBuff("Mend Pet"), "Mend Pet"),
                                   PetManager.CastPetSpell("Heart of the Phoenix",  ret => !Me.GotAlivePet && CLUSettings.Instance.Hunter.UseHeartofthePhoenix, "Heart of the Phoenix")));
        }

        public static Composite HunterCallPetBehavior(bool reviveInCombat)
        {
            return new Decorator(ret => !StyxWoW.Me.GotAlivePet && PetManager.PetTimer.IsFinished,
                new PrioritySelector(
                    Spell.WaitForCast(false),
                    new Decorator(ret => StyxWoW.Me.Pet != null && (!StyxWoW.Me.Combat || reviveInCombat),
                        new PrioritySelector(
                            Movement.EnsureMovementStoppedBehavior(),
                            Spell.CastSelfSpell("Revive Pet", ret => true, "Revive Pet"))),
                    new Sequence(
                        new Action(ret => PetManager.CallPet("" + (int)CLUSettings.Instance.Hunter.PetSlotSelection)),
                        Spell.CreateWaitForLagDuration(),
                        new WaitContinue(2, ret => StyxWoW.Me.GotAlivePet || StyxWoW.Me.Combat, new ActionAlwaysSucceed()),
                        new Decorator(ret => !StyxWoW.Me.GotAlivePet && (!StyxWoW.Me.Combat || reviveInCombat),
                            Spell.CastSelfSpell("Revive Pet", ret => true, "Revive Pet")))
                    )
                );
        }

        
    }
}
