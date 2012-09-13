using Styx;
using System.Collections.Generic;
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
                CLU.Log(" [BotChecker] LazyRaider Detected. *MOVEMENT DISABLED*");
            }

            // Check for Raid Bot and Disable movement
            if (BotBaseInUse("Tyrael"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLU.Log(" [BotChecker] Tyrael Detected. *MOVEMENT DISABLED*");
            }

            // Check for Raid Bot and Disable movement
            if (BotBaseInUse("Raid Bot"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLU.Log(" [BotChecker] Raid Bot Detected. *MOVEMENT DISABLED*");
            }

            // Check for Combat Bot and Disable movement
            if (BotBaseInUse("Combat Bot"))
            {
                CLUSettings.Instance.EnableMovement = false;
                CLU.Log(" [BotChecker] Combat Bot Detected. *MOVEMENT DISABLED*");
            }

            // Check for BGBuddy and MultiDotting
            if (BotBaseInUse("BGBuddy"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.Rogue.UseTricksOfTheTrade = false;
                CLUSettings.Instance.Rogue.UseTricksOfTheTradeForce = false;
                CLUSettings.Instance.EnableMovement = true;
                CLU.Log(" [BotChecker] BGBuddy Bot Detected. *MULTI-DOTTING DISABLED* && *MOVEMENT ENABLED* && *TotT DISABLED*");
            }

            // Check for Questing
            if (BotBaseInUse("Questing"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLU.Log(" [BotChecker] Questing Bot Detected. *MULTI-DOTTING DISABLED* && *MOVEMENT ENABLED*");
            }

            // Check for Grind Bot
            if (BotBaseInUse("Grind Bot"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLU.Log(" [BotChecker] Grind Bot Bot Detected. *MULTI-DOTTING DISABLED* && *MOVEMENT ENABLED*");
            }

            // Check for Gatherbuddy2
            if (BotBaseInUse("Gatherbuddy2"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLU.Log(" [BotChecker] Gatherbuddy2 Bot Detected. *MULTI-DOTTING DISABLED* && *MOVEMENT ENABLED*");
            }

            // Check for ArchaeologyBuddy
            if (BotBaseInUse("ArchaeologyBuddy"))
            {
                CLUSettings.Instance.EnableMultiDotting = false;
                CLUSettings.Instance.EnableMovement = true;
                CLU.Log(" [BotChecker] ArchaeologyBuddy Bot Detected. *MULTI-DOTTING DISABLED* && *MOVEMENT ENABLED*");
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
