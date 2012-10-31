using System.Windows.Media;
using CLU.Settings;
using Styx;
using Styx.Common;
using Styx.WoWInternals.WoWObjects;

namespace CLU.Helpers
{
    public static class CLULogger
    {
        /// <summary>writes debug messages to the log file (false by default)</summary>
        /// <param name="msg">the message to write to the log</param>
        /// <param name="args">the arguments that accompany the message</param>
        public static void DiagnosticLog(string msg, params object[] args)
        {
            if (msg != null)
            {
                Logging.Write(LogLevel.Diagnostic, Colors.White, "[CLU] " + CLU.Version + ": " + msg, args);
            }
        }

        //private static string lastLine { get; set; }

        public static void Log(string msg, params object[] args)
        {
            //if (msg == lastLine) return;
            Logging.Write(LogLevel.Normal, Colors.Yellow, "[CLU] " + CLU.Version + ": " + msg, args);
            //lastLine = msg;
        }

        /// <summary>writes debug messages to the log file. Only enable movement/Targeting  logs.</summary>
        /// <param name="msg">the message to write to the log</param>
        /// <param name="args">the arguments that accompany the message</param>
        public static void MovementLog(string msg, params object[] args)
        {
            if (msg != null && CLUSettings.Instance.MovementLogging)
            {
                Logging.Write(LogLevel.Quiet, Colors.DimGray, "[CLU] " + CLU.Version + ": " + msg, args);
            }
        }

        /// <summary>writes debug messages to the log file. This is necassary information for CLU's programmer</summary>
        /// <param name="msg">the message to write to the log</param>
        /// <param name="args">the arguments that accompany the message</param>
        public static void TroubleshootLog(string msg, params object[] args)
        {
            if (msg != null)
            {
                Logging.Write(LogLevel.Quiet, Colors.DimGray, "[CLU] " + CLU.Version + ": " + msg, args);
            }
        }

        /// <summary>
        /// Returns the string "Myself" if the unit name is equal to our name.
        /// </summary>
        /// <param name="unit">the unit to check</param>
        /// <returns>a safe name for the log</returns>
        public static string SafeName(WoWUnit unit)
        {
            if (unit != null)
            {
                return (unit.Name == StyxWoW.Me.Name) ? "Myself" : unit.Name;
            }

            return "No Target";
        }
    }
}
