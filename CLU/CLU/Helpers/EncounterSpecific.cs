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

#endregion Revision info

using System.Linq;
using CLU.Base;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;

namespace CLU.Helpers
{
    public class EncounterSpecific
    {
        /* putting all the EncounterSpecific logic here */
        private static readonly EncounterSpecific EncounterSpecificInstance = new EncounterSpecific();

        /// <summary>
        /// An instance of the EncounterSpecific Class
        /// </summary>
        public static EncounterSpecific Instance
        {
            get
            {
                return EncounterSpecificInstance;
            }
        }

        private static LocalPlayer Me
        {
            get
            {
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
        }

        /// <summary>
        /// Returns true if Ultraxion is casting Hour of Twilight
        /// </summary>
        /// <returns>true or false</returns>
        private static bool IsHourofTwilight()
        {
            return false;
        }

        /// <summary>
        /// Returns true if Ultraxion
        /// </summary>
        /// <returns>true or false</returns>
        public static bool IsUltraxion()
        {
            return false;
        }

        /// <summary>
        /// Returns true if Ultraxion is casting Fading Light
        /// </summary>
        /// <returns>true or false</returns>
        private static bool IsFadingLight()
        {
            return Buff.PlayerHasActiveBuff("Fading Light") && Buff.PlayerDebuffTimeLeft("Fading Light").TotalSeconds <= 2;
        }

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