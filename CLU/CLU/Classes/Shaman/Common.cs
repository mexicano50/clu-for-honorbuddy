#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author bobby53$
// $Date$
// $HeadURL$
// $LastChangedBy wulf$
// $LastChangedDate$
// $LastChangedRevision$
// $Revision$

#endregion

using System;
using System.Linq;
using CLU.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using Action = Styx.TreeSharp.Action;

namespace CLU.Classes.Shaman
{
    using global::CLU.Base;
    using global::CLU.Managers;

    /// <summary>
    /// Temporary Enchant Id associated with Shaman Imbue
    /// Note: each enum value and Imbue.GetSpellName() must be maintained in a way to allow tranlating an enum into a corresponding spell name
    /// </summary>
    public enum Imbue
    {
        None = 0,

        Flametongue = 5,
        Windfury = 283,
        Earthliving = 3345,
        Frostbrand = 2,
        Rockbiter = 3021
    }

    public static class Common
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        // Full credit goes to the Singular dev team for imbue support.
        #region IMBUE SUPPORT 
        
        public static Decorator CreateShamanImbueMainHandBehavior(params Imbue[] imbueList)
        {
            return new Decorator(ret => CanImbue(Me.Inventory.Equipped.MainHand),
                new PrioritySelector(
                    imb => imbueList.FirstOrDefault(i => SpellManager.HasSpell(i.ToSpellName())),

                    new Decorator(
                        ret => Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id != (int)ret
                            && SpellManager.HasSpell(((Imbue)ret).ToSpellName())
                            && SpellManager.CanCast(((Imbue)ret).ToSpellName(), null, false, false),
                        new Sequence(
                            new Action(ret => CLULogger.TroubleshootLog("Main hand currently imbued: " + ((Imbue)Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id).ToString())),
                            new Action(ret => Lua.DoString("CancelItemTempEnchantment(1)")),
                            new WaitContinue(1,
                                ret => Me.Inventory.Equipped.MainHand != null && (Imbue)Me.Inventory.Equipped.MainHand.TemporaryEnchantment.Id == Imbue.None,
                                new ActionAlwaysSucceed()),
                            new DecoratorContinue(ret => ((Imbue)ret) != Imbue.None,
                                new Sequence(
                                    new Action(ret => CLULogger.TroubleshootLog("Imbuing main hand weapon with " + ((Imbue)ret).ToString())),
                                    new Action(ret => SpellManager.Cast(((Imbue)ret).ToSpellName(), null)),
                                    new Action(ret => SetNextAllowedImbueTime())
                                    )
                                )
                            )
                        )
                    )
                );
        }

        public static Composite CreateShamanImbueOffHandBehavior(params Imbue[] imbueList)
        {
            return new Decorator(ret => CanImbue(Me.Inventory.Equipped.OffHand),
                new PrioritySelector(
                    imb => imbueList.FirstOrDefault(i => SpellManager.HasSpell(i.ToSpellName())),

                    new Decorator(
                        ret => Me.Inventory.Equipped.OffHand.TemporaryEnchantment.Id != (int)ret
                            && SpellManager.HasSpell(((Imbue)ret).ToSpellName())
                            && SpellManager.CanCast(((Imbue)ret).ToSpellName(), null, false, false),
                        new Sequence(
                            new Action(ret => CLULogger.TroubleshootLog("Off hand currently imbued: " + ((Imbue)Me.Inventory.Equipped.OffHand.TemporaryEnchantment.Id).ToString())),
                            new Action(ret => Lua.DoString("CancelItemTempEnchantment(2)")),
                            new WaitContinue(1,
                                ret => Me.Inventory.Equipped.OffHand != null && (Imbue)Me.Inventory.Equipped.OffHand.TemporaryEnchantment.Id == Imbue.None,
                                new ActionAlwaysSucceed()),
                            new DecoratorContinue(ret => ((Imbue)ret) != Imbue.None,
                                new Sequence(
                                    new Action(ret => CLULogger.TroubleshootLog("Imbuing Off hand weapon with " + ((Imbue)ret).ToString())),
                                    new Action(ret => SpellManager.Cast(((Imbue)ret).ToSpellName(), null)),
                                    new Action(ret => SetNextAllowedImbueTime())
                                    )
                                )
                            )
                        )
                    )
                );
        }

        // imbues are sometimes slow to appear on client... need to allow time
        // .. for buff to appear, otherwise will get in an imbue spam loop

        private static DateTime nextImbueAllowed = DateTime.Now;

        public static bool CanImbue(WoWItem item)
        {
            if (item != null && item.ItemInfo.IsWeapon)
            {
                // during combat, only mess with imbues if they are missing
                if (Me.Combat && item.TemporaryEnchantment.Id != 0)
                    return false;

                // check if enough time has passed since last imbue
                // .. guards against detecting is missing immediately after a cast but before buff appears
                // .. (which results in imbue cast spam)
                if (nextImbueAllowed > DateTime.Now)
                    return false;

                switch (item.ItemInfo.WeaponClass)
                {
                    case WoWItemWeaponClass.Axe:
                        return true;
                    case WoWItemWeaponClass.AxeTwoHand:
                        return true;
                    case WoWItemWeaponClass.Dagger:
                        return true;
                    case WoWItemWeaponClass.Fist:
                        return true;
                    case WoWItemWeaponClass.Mace:
                        return true;
                    case WoWItemWeaponClass.MaceTwoHand:
                        return true;
                    case WoWItemWeaponClass.Polearm:
                        return true;
                    case WoWItemWeaponClass.Staff:
                        return true;
                    case WoWItemWeaponClass.Sword:
                        return true;
                    case WoWItemWeaponClass.SwordTwoHand:
                        return true;
                }
            }

            return false;
        }

        public static void SetNextAllowedImbueTime()
        {
            // 2 seconds to allow for 0.5 seconds plus latency for buff to appear
            nextImbueAllowed = DateTime.Now + new TimeSpan(0, 0, 0, 0, 500); // 1500 + (int) StyxWoW.WoWClient.Latency << 1);
        }

        public static string ToSpellName(this Imbue i)
        {
            return i.ToString() + " Weapon";
        }

        public static Imbue GetImbue(WoWItem item)
        {
            if (item != null)
                return (Imbue)item.TemporaryEnchantment.Id;

            return Imbue.None;
        }

        public static bool IsImbuedForDPS(WoWItem item)
        {
            Imbue imb = GetImbue(item);
            return imb == Imbue.Flametongue || imb == Imbue.Windfury;
        }

        public static bool IsImbuedForHealing(WoWItem item)
        {
            return GetImbue(item) == Imbue.Earthliving;
        }

        #endregion

        public static Composite HandleTotemRecall()
        {
            return new Decorator(
                ret => Totems.NeedToRecallTotems,
                new Sequence(
                    new Action(ret => CLULogger.Log(" [Totems] Recalling Totems")), 
                    new Action(ret => Totems.RecallTotems())));
        }

        public static Composite HandleCompulsoryShamanBuffs()
        {
            return new PrioritySelector(
                new Switch<WoWSpec>(ctx => TalentManager.CurrentSpec,
                                    new SwitchArgument<WoWSpec>(WoWSpec.ShamanElemental,
                                        new PrioritySelector(
                                            Buff.CastBuff("Lightning Shield", ret => true, "Lightning Shield"),
                                            CreateShamanImbueMainHandBehavior(Imbue.Flametongue))),
                                    new SwitchArgument<WoWSpec>(WoWSpec.ShamanEnhancement,
                                        new PrioritySelector(
                                            Buff.CastBuff("Lightning Shield", ret => true, "Lightning Shield"),
                                            CreateShamanImbueMainHandBehavior(Imbue.Windfury, Imbue.Flametongue ),
                                            CreateShamanImbueOffHandBehavior(Imbue.Flametongue ))),
                                    new SwitchArgument<WoWSpec>(WoWSpec.ShamanRestoration,
                                        new PrioritySelector(
                                            Buff.CastBuff("Water Shield", ret => true, "Water Shield"),
                                            CreateShamanImbueMainHandBehavior(Imbue.Earthliving, Imbue.Flametongue)))));
        }
    }
}