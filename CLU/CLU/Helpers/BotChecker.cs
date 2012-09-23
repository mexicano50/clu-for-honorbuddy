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
using CLU.Base;
using CLU.Settings;

namespace CLU.Helpers
{
    using Styx.CommonBot;

    internal class BotChecker
    {
        /* putting all the BotBase logic here */

        static BotChecker()
        {
            Instance = new BotChecker();
        }

        public static BotChecker Instance { get; private set; }

        private static readonly HashSet<string> Bots = new HashSet<string> {
            "ArchaeologyBuddy", "BGBuddy", "Combat Bot", "Gatherbuddy2", "Grind Bot", "Instancebuddy", "LazyRaider",
             "Mixed Mode", "PartyBot", "ProfessionBuddy", "Questing", "Raid Bot", "Tyrael"
        };

        private static readonly HashSet<string> SupportedBots = new HashSet<string> {
            "BGBuddy", "Combat Bot", "LazyRaider", "Questing", "Raid Bot", "Grind Bot", "Gatherbuddy2","ArchaeologyBuddy","Tyrael"
        };

        public static void Initialize()
        {
            // Check for LazyRaider and Disable movement
            if (BotBaseInUse("LazyRaider"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLUSettings.Instance.BurstOn = Burst.onBoss;
                CLU.Log(" [BotChecker] LazyRaider Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON BOSS SET*");
            }

            // Check for Raid Bot and Disable movement
            if (BotBaseInUse("Tyrael"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLUSettings.Instance.BurstOn = Burst.onBoss;
                CLU.Log(" [BotChecker] Tyrael Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON BOSS SET*");
            }

            // Check for Raid Bot and Disable movement
            if (BotBaseInUse("Raid Bot"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLUSettings.Instance.BurstOn = Burst.onBoss;
                CLU.Log(" [BotChecker] Raid Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON BOSS SET*");
            }

            // Check for Combat Bot and Disable movement
            if (BotBaseInUse("Combat Bot"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLUSettings.Instance.BurstOn = Burst.onBoss;
                CLU.Log(" [BotChecker] Combat Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON BOSS SET*");
            }

            // Check for BGBuddy and MultiDotting
            if (BotBaseInUse("BGBuddy"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.Rogue.UseTricksOfTheTrade = false;
                CLUSettings.Instance.Rogue.UseTricksOfTheTradeForce = false;
                CLUSettings.Instance.EnableMovement = true;
                CLUSettings.Instance.BurstOn = Burst.onBoss;
                CLU.Log(" [BotChecker] BGBuddy Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT ENABLED*");
                CLU.Log(" [BotChecker] *MULTI-DOTTING DISABLED*");
                CLU.Log(" [BotChecker] *TotT DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON BOSS SET*");
            }

            // Check for Questing
            if (BotBaseInUse("Questing"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLUSettings.Instance.BurstOn = Burst.onMob;
                CLU.Log(" [BotChecker] Questing Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT ENABLED*");
                CLU.Log(" [BotChecker] *MULTI-DOTTING DISABLED*");
                CLU.Log(" [BotChecker] *TotT DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON MOB SET*");
                CLU.Log(" [BotChecker] *BURST ON MOB COUNT = " + CLUSettings.Instance.BurstOnMobCount + "*");
            }

            // Check for Grind Bot
            if (BotBaseInUse("Grind Bot"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLUSettings.Instance.BurstOn = Burst.onMob;
                CLU.Log(" [BotChecker] Grind Bot Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT ENABLED*");
                CLU.Log(" [BotChecker] *MULTI-DOTTING DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON MOB SET*");
                CLU.Log(" [BotChecker] *BURST ON MOB COUNT = " + CLUSettings.Instance.BurstOnMobCount + "*");
            }

            // Check for Gatherbuddy2
            if (BotBaseInUse("Gatherbuddy2"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLUSettings.Instance.BurstOn = Burst.onMob;
                CLU.Log(" [BotChecker] Gatherbuddy2 Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT ENABLED*");
                CLU.Log(" [BotChecker] *MULTI-DOTTING DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON MOB SET*");
                CLU.Log(" [BotChecker] *BURST ON MOB COUNT = " + CLUSettings.Instance.BurstOnMobCount + "*");
            }

            // Check for ArchaeologyBuddy
            if (BotBaseInUse("ArchaeologyBuddy"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLUSettings.Instance.BurstOn = Burst.onMob;
                CLU.Log(" [BotChecker] ArchaeologyBuddy Bot Detected.");
                CLU.Log(" [BotChecker] *MOVEMENT ENABLED*");
                CLU.Log(" [BotChecker] *MULTI-DOTTING DISABLED*");
                CLU.Log(" [BotChecker] *BURST ON MOB SET*");
                CLU.Log(" [BotChecker] *BURST ON MOB COUNT = " + CLUSettings.Instance.BurstOnMobCount + "*");
            }
           
        }

        /// <summary>
        ///  Checks to see if the botbase is supported
        /// </summary>
        /// <returns>true if the botbase is currently supported</returns>
        public static bool SupportedBotBase()
        {
            return SupportedBots.Contains(BotManager.Current.Name);
        }

        /// <summary>
        ///  Checks to see if the botbase is currently in use
        /// </summary>
        /// <returns>true if the botbase is currently being used</returns>
        public static bool BotBaseInUse(string botname)
        {
            return BotManager.Current.Name == botname;
        }
    }
}
