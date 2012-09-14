// Full Credit to singular for this class (why re-invent the wheel?)
// edited for MoP (8/09/2012)

namespace CLU.Classes.Rogue
{
    using CommonBehaviors.Actions;
    using Styx;
    using Styx.CommonBot;
    using Styx.Pathing;
    using Styx.TreeSharp;

    using Base;
    using Settings;

    public static class Poisons
    {
        private static bool NeedsPoison
        {
            get {
                return StyxWoW.Me.Inventory.Equipped.MainHand != null && 
                    StyxWoW.Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id == 0 && 
                    StyxWoW.Me.Inventory.Equipped.MainHand.ItemInfo.WeaponClass != WoWItemWeaponClass.FishingPole;
            }
        }

        private static int MainHandPoison
        {
            get {
                switch (CLUSettings.Instance.Rogue.MainHandPoison)
                {

                case MHPoisonType.Wound:
                        return 8679;
                case MHPoisonType.Deadly:
                        return 2823;
                default:
                    return 0;
                }
            }
        }

        private static int OffHandHandPoison
        {
            get
            {
                switch (CLUSettings.Instance.Rogue.OffHandPoison)
                {
                    case OHPoisonType.MindNumbing:
                        return 5761;
                    case OHPoisonType.Crippling:
                        return 3408;
                    case OHPoisonType.Paralytic:
                        return 108215;
                    case OHPoisonType.Leeching:
                        return 108211;
                    default:
                        return 0;
                }
            }
        }   

        public static Composite CreateApplyPoisons()
        {
            return new PrioritySelector(
                       new Decorator(
                           ret => NeedsPoison && !Buff.PlayerHasActiveBuff(MainHandPoison) && SpellManager.HasSpell(MainHandPoison),
                           new Sequence(
                               new Action(ret => CLU.TroubleshootLog("Applying {0} to main hand", CLUSettings.Instance.Rogue.MainHandPoison)),
                               new Action(ret => Navigator.PlayerMover.MoveStop()),
                               Spell.CreateWaitForLagDuration(),
                               new Action(ret => SpellManager.CastSpellById(MainHandPoison)),
                               Spell.CreateWaitForLagDuration(),
                               new WaitContinue(2, ret => StyxWoW.Me.IsCasting, new ActionAlwaysSucceed()),
                               new WaitContinue(10, ret => !StyxWoW.Me.IsCasting, new ActionAlwaysSucceed()),
                               new WaitContinue(1, ret => false, new ActionAlwaysSucceed()))),

                          new Decorator(
                           ret => NeedsPoison && !StyxWoW.Me.HasAura(OffHandHandPoison) && SpellManager.HasSpell(OffHandHandPoison),
                           new Sequence(
                               new Action(ret => CLU.TroubleshootLog("Applying {0} to off hand", CLUSettings.Instance.Rogue.OffHandPoison)),
                               new Action(ret => Navigator.PlayerMover.MoveStop()),
                               Spell.CreateWaitForLagDuration(),
                               new Action(ret => SpellManager.CastSpellById(OffHandHandPoison)),
                               Spell.CreateWaitForLagDuration(),
                               new WaitContinue(2, ret => StyxWoW.Me.IsCasting, new ActionAlwaysSucceed()),
                               new WaitContinue(10, ret => !StyxWoW.Me.IsCasting, new ActionAlwaysSucceed()),
                               new WaitContinue(1, ret => false, new ActionAlwaysSucceed())))
                   );
        }
    }
}
