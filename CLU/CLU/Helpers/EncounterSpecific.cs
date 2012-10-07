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

using System.Linq;
using Styx;
using Styx.WoWInternals.WoWObjects;

using CommonBehaviors.Actions;
using Styx.TreeSharp;
using CLU.Settings;
using CLU.Base;

namespace CLU.Helpers
{

    public class EncounterSpecific
    {
        /* putting all the EncounterSpecific logic here */

        //TODO: This all needs removed or fixed its throwing a bunch of errors and filling the logs

        /*
         * 
         * 
        [06:36:21.851 D] System.NullReferenceException: Object reference not set to an instance of an object.
   at CLU.Helpers.EncounterSpecific.<IsHourofTwilight>b__2(WoWUnit u)
   at System.Linq.Enumerable.Any[TSource](IEnumerable`1 source, Func`2 predicate)
   at CLU.Helpers.EncounterSpecific.IsHourofTwilight()
   at CLU.Helpers.EncounterSpecific.<ExtraActionButton>b__9(Object x)
   at Styx.TreeSharp.Decorator.CanRun(Object context)
   at Styx.TreeSharp.Decorator..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.TreeSharp.PrioritySelector..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.TreeSharp.DecoratorContinue..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.TreeSharp.Sequence..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.TreeSharp.Decorator..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.TreeSharp.PrioritySelector..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Weischbier.LookIntoMe.Bots.Tyrael.Tyrael.LockSelector.Tick(Object context)
   at Styx.TreeSharp.Decorator..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.TreeSharp.PrioritySelector..()
   at Styx.TreeSharp.Composite.Tick(Object context)
   at Styx.CommonBot.TreeRoot.()
         * 
         * */

        private static readonly EncounterSpecific EncounterSpecificInstance = new EncounterSpecific();

        /// <summary>
        /// An instance of the EncounterSpecific Class
        /// </summary>
        public static EncounterSpecific Instance
        {
            get {
                return EncounterSpecificInstance;
            }
        }

        private static LocalPlayer Me
        {
            get {
                return StyxWoW.Me;
            }
        }

        /// <summary>
        /// Returns true if Morchok is casting Stomp
        /// </summary>
        /// <returns>true or false</returns>
        public static bool IsMorchokStomp()
        {
            return false;
            //ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Any(
            //    u => u != null && u.IsAlive &&
            //    u.Guid != Me.Guid &&
            //    u.IsHostile &&
            //    u.IsCasting &&
            //    u.CastingSpell.Name == "Stomp" &&
            //    u.Name == "Morchok");
        }

        /// <summary>
        /// Returns true if Ultraxion is casting Hour of Twilight
        /// </summary>
        /// <returns>true or false</returns>
        private static bool IsHourofTwilight()
        {
            return false;
            //ObjectManager.GetObjectsOfType<WoWUnit>(true, true).Any(
            //    u => u != null && u.IsAlive &&
            //    u.Guid != Me.Guid &&
            //    u.IsHostile &&
            //    u.IsCasting &&
            //    u.CastingSpell.Name == "Hour of Twilight" &&
            //    u.CurrentCastTimeLeft.TotalSeconds <= 2);
        }

        /// <summary>
        /// Returns true if Ultraxion
        /// </summary>
        /// <returns>true or false</returns>
        public static bool IsUltraxion()
        {
            return false;
            //ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Any(
            //    u => u != null && u.IsAlive &&
            //    u.Guid != Me.Guid &&
            //    u.IsHostile &&
            //    u.Name == "Ultraxion");
        }

        /// <summary>
        /// Returns true if Ultraxion is casting Fading Light
        /// </summary>
        /// <returns>true or false</returns>
        private static bool IsFadingLight()
        {
            return Buff.PlayerHasActiveBuff("Fading Light") && Buff.PlayerDebuffTimeLeft("Fading Light").TotalSeconds <= 2;
        }

        /// <summary>
        /// Returns true if DeathWing is casting Shrapnel
        /// </summary>
        /// <returns>true or false</returns>
        //private static bool IsShrapnel()
        //{
        //    return
        //        ObjectManager.GetObjectsOfType<WoWUnit>(true, true).Any(
        //            u => u != null && u.IsAlive &&
        //            u.Guid != Me.Guid &&
        //            u.IsHostile &&
        //            (u.IsTargetingMyPartyMember ||
        //             u.IsTargetingMyRaidMember ||
        //             u.IsTargetingMeOrPet ||
        //             u.IsTargetingAnyMinion) &&
        //            u.IsCasting &&
        //            u.CastingSpell.Name == "Shrapnel" &&
        //            u.CurrentCastTimeLeft.TotalMilliseconds <= 2000) && !Buff.PlayerHasBuff("Dream");
        //}

        /// <summary>Returns true if Ultraxion is casting Hour of Twilight and its my turn to soak</summary>
        /// <param name="myGroupNumber">The my Group Number.</param>
        /// <returns>true or false</returns>
        public bool IsMyHourofTwilightSoak(int myGroupNumber)
        {
            return Me.GroupInfo.RaidMembers.Any(x => x.GroupNumber == myGroupNumber && x.Guid == Me.Guid);
        }

        /// <summary>Handles pushing the extra action button in DS Encounters</summary>
        /// <returns>The extra action button.</returns>
        public static Composite ExtraActionButton()
        {
            return new Decorator(
                       x => (IsFadingLight() || IsHourofTwilight()) && CLUSettings.Instance.ClickExtraActionButton, //|| IsShrapnel() todo: removed...cos its retarted.
                       new Sequence(
                           new Action(a => CLULogger.TroubleshootLog(" [ExtraActionButton] Time to get Lazy!")),
                           Item.RunMacroText("/click ExtraActionButton1", ret => true, "[Push Button] ExtraActionButton1"),
                           new ActionAlwaysFail())); // continue down the tree
        }
    }
}
