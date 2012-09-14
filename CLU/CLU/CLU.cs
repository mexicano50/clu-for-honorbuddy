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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Media;

using CLU.Base;
using CLU.Classes;
using CLU.CombatLog;
using CLU.GUI;
using CLU.Helpers;
using CLU.Managers;
using CLU.Settings;
using CLU.Sound;

using Styx;
using Styx.Combat.CombatRoutine;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.DBC;
using Styx.WoWInternals.WoWObjects;

using Timer = System.Timers.Timer;

// Credits
//-------------
// * cowdude for his initial work with Felmaster and giving me inspiration to create this CC.
// * All the Singular Developers For there tiresome work and supporting code
// * bobby53 For his Disclaimer (Couldn't have said it better)
// * Kickazz006 for his BlankProfile.xml
// * Shaddar & Mentally for thier initial detection code of HourofTwilight, FadingLight and Shrapnel
// * bennyquest for his continued efforts in reporting issues!
// * Jamjar0207 for his Protection Warrior Rotation and Fire Mage
// * gniegsch for his Arms Warrior Rotation and improvements with blood deathknight
// * Stormchasing for his help with warlocks and patchs
// * Obliv For his Boomkin, Frost Mage & Assassination rotations and Arms warrior improvement
// * ShinobiAoshi for his initial Affliction warlock rotation
// * fluffyhusky for his initial Enhancement Shaman rotation
// * Digitalmocking for his initial Elemental Shaman rotation
// * Toney001 for his improvements to the Unholy Deathknight rotation
// * kbrebel04  for his improvements to Subtlety Rogue

//[assembly: CLSCompliant(true)]

namespace CLU
{
    internal delegate void PulseHander();

    public class CLU : CombatRoutine
    {
        #region Constants and Fields

        public static readonly Version Version = new Version(3, 3, 0);
        private readonly Timer _clupulsetimer = new Timer(10000); // A timer for keybinds

        private RotationBase _rotationBase;

        /// <summary>This will: loop assemblies,
        /// loop types,
        /// filter types that are a subclass of RotationBase and not the abstract,
        /// create an instance of a RotationBase subclass so we can interigate KeySpell within the RotationBase subclass,
        /// Check if the character has the Keyspell,
        /// Set the active rotation to the matching RotationBase subclass.</summary>
        private List<RotationBase> _rotations; // list of Rotations

        #endregion

        #region Constructors and Destructors

        public CLU()
        {
            Instance = this;
        }

        #endregion

        #region Delegates

        public delegate WoWUnit UnitSelection(object context);

        #endregion

        #region Events

        internal event PulseHander PulseEvent;

        #endregion

        #region Public Properties

        public static CLU Instance { get; private set; }

        public static bool IsHealerRotationActive { get; private set; }

        /// <summary>
        /// If we havnt loaded a rotation for our character then do so by querying our character's class tree based on a Keyspell.
        /// </summary>
        public RotationBase ActiveRotation
        {
            get
            {
                if ( this._rotationBase == null )
                {
                    TroubleshootLog("ActiveRotation is null..retrieving.");
                    this.QueryClassTree();
                    if ( this._rotationBase == null )
                    {
                        if ( TalentManager.CurrentSpec.ToString() == "Lowbie" )
                        {
                            Log
                                (" Greetings, level {0} user. Unfortunelty CLU does not support such Low Level players at this time",
                                 Me.Level);
                            StopBot("Unable to find Active Rotation");
                        }
                        else
                        {
                            Log
                                (" Greetings, level {0} user. Unfortunelty CLU could not find a rotation for you.",
                                 Me.Level);
                            StopBot("Unable to find Active Rotation");
                        }
                    }
                }

                return this._rotationBase;
            }
        }

        public override WoWClass Class
        {
            get { return StyxWoW.Me.Class; }
        }

        /// <summary>
        /// HB Behavior used in combat. - pulses in combat
        /// </summary>
        public override Composite CombatBehavior
        {
            get { return new Decorator(ret => AllowPulse, this.Rotation); }
        }

        /// <summary>
        /// HB Behavior used for combat buffs. eg; 'Horn of Winter', 'Power Infusion' etc..
        /// </summary>
        public override Composite CombatBuffBehavior
        {
            get { return new Decorator(ret => AllowPulse, this.Medic); }
        }

        public override string Name
        {
            get { return "CLU (Codified Likeness Utility) " + Version; }
        }

        /// <summary>
        /// HB Behavior used for buffing, regular buffs like 'Power Word: Fortitude', 'MotW' etc..
        /// </summary>
        public override Composite PreCombatBuffBehavior
        {
            get { return new Decorator(ret => AllowPulse, this.PreCombat); }
        }

        /// <summary>
        /// HB Behavior used when engaging mobs in combat.
        /// </summary>
        public override Composite PullBehavior
        {
            get { return new Decorator(ret => AllowPulse, this.Rotation); }
        }

        /// <summary>
        /// HB Pulldistance.
        /// </summary>
        public override double? PullDistance
        {
            get { return this.ActiveRotation.CombatMaxDistance; }
        }

        /// <summary>
        /// HB Behavior used when resting. - pulses out of combat
        /// </summary>
        public override Composite RestBehavior
        {
            get
            {
                return new Decorator
                    (ret => !( CLUSettings.Instance.NeverDismount && IsMounted ) && !Me.IsFlying, this.Resting);
            }
        }

        public override bool WantButton
        {
            get { return true; }
        }

        #endregion

        #region Properties

        internal static GroupType GroupType
        {
            get
            {
                if ( Me.IsInParty )
                {
                    return GroupType.Party;
                }

                return Me.IsInRaid ? GroupType.Raid : GroupType.Solo;
            }
        }

        internal static GroupLogic LocationContext
        {
            get
            {
                Map map = StyxWoW.Me.CurrentMap;

                if ( map.IsBattleground || map.IsArena )
                {
                    return GroupLogic.Battleground;
                }

                return map.IsDungeon ? GroupLogic.PVE : GroupLogic.Solo;
            }
        }

        /// <summary>
        /// Allow HB to pulse our shit on our conditions.
        /// </summary>
        private static bool AllowPulse
        {
            get
            {
                if ( Me.IsOnTransport )
                {
                    return false;
                }
                if ( CLUSettings.Instance.NeverDismount && IsMounted )
                {
                    return false;
                }
                // if (Buff.PlayerBuffTimeLeft("Cannibalize") > 1) return false;
                if ( Buff.PlayerHasBuff("Health Funnel") )
                {
                    return false;
                }
                return true;
            }
        }

        private static bool IsMounted
        {
            get
            {
                switch (StyxWoW.Me.Shapeshift)
                {
                    case ShapeshiftForm.FlightForm:
                    case ShapeshiftForm.EpicFlightForm:
                        return true;
                }
                return StyxWoW.Me.Mounted;
            }
        }

        private static LocalPlayer Me
        {
            get { return ObjectManager.Me; }
        }

        /// <summary>
        /// CLU Combat heal/buffs Behavior
        /// </summary>
        private Composite Medic
        {
            get { return this.ActiveRotation.Medic; }
        }

        /// <summary>
        /// CLU Pre Combat buffs Behavior
        /// </summary>
        private Composite PreCombat
        {
            get { return this.ActiveRotation.PreCombat; }
        }

        /// <summary>
        /// CLU Rest Behavior
        /// </summary>
        private Composite Resting
        {
            get { return this.ActiveRotation.Resting; }
        }

        /// <summary>
        /// Depending on if we are grouped or solo, and if we are in a dungeon or battleground, return the rotation we will use.
        /// </summary>
        private Composite Rotation
        {
            get
            {
                Composite currentrotation = null;
                switch (GroupType)
                {
                    case GroupType.Party:
                    case GroupType.Raid:
                        switch (LocationContext)
                        {
                            case GroupLogic.PVE:
                                currentrotation = this.ActiveRotation.PVERotation;
                                break;
                            case GroupLogic.Battleground:
                                currentrotation = this.ActiveRotation.PVPRotation;
                                break;
                        }
                        break;
                    default:
                        currentrotation = this.ActiveRotation.SingleRotation;
                        break;
                }

                return new Sequence
                    (new DecoratorContinue(x => CLUSettings.Instance.EnableMovement, Movement.MovingFacingBehavior()),
                     new DecoratorContinue(x => Me.CurrentTarget != null, currentrotation));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>writes debug messages to the log file (false by default)</summary>
        /// <param name="msg">the message to write to the log</param>
        /// <param name="args">the arguments that accompany the message</param>
        public static void DiagnosticLog(string msg, params object[] args)
        {
            if ( msg != null && CLUSettings.Instance.EnableDebugLogging )
            {
                Logging.Write(LogLevel.Diagnostic, Colors.White, "[CLU] " + Version + ": " + msg, args);
            }
        }

        public static void Log(string msg, params object[] args)
        {
            //if (msg == lastLine) return;
            Logging.Write(LogLevel.Normal, Colors.Yellow, "[CLU] " + Version + ": " + msg, args);
            //lastLine = msg;
        }

        /// <summary>writes debug messages to the log file. Only enable movement/Targeting  logs.</summary>
        /// <param name="msg">the message to write to the log</param>
        /// <param name="args">the arguments that accompany the message</param>
        public static void MovementLog(string msg, params object[] args)
        {
            if ( msg != null && CLUSettings.Instance.MovementLogging )
            {
                Logging.Write(LogLevel.Quiet, Colors.DimGray, "[CLU] " + Version + ": " + msg, args);
            }
        }

        /// <summary>
        /// Returns the string "Myself" if the unit name is equal to our name.
        /// </summary>
        /// <param name="unit">the unit to check</param>
        /// <returns>a safe name for the log</returns>
        public static string SafeName(WoWUnit unit)
        {
            if ( unit != null )
            {
                return ( unit.Name == Me.Name ) ? "Myself" : unit.Name;
            }

            return "No Target";
        }

        /// <summary>writes debug messages to the log file. This is necassary information for CLU's programmer</summary>
        /// <param name="msg">the message to write to the log</param>
        /// <param name="args">the arguments that accompany the message</param>
        public static void TroubleshootLog(string msg, params object[] args)
        {
            if ( msg != null )
            {
                Logging.Write(LogLevel.Quiet, Colors.DimGray, "[CLU] " + Version + ": " + msg, args);
            }
        }

        public override void Initialize()
        {
            TroubleshootLog("Attatching BotEvents");
            BotEvents.OnBotStarted += WoWStats.Instance.WoWStatsOnStarted;
            BotEvents.OnBotStopped += WoWStats.Instance.WoWStatsOnStopped;
            BotEvents.OnBotStarted += CombatLogEvents.Instance.CombatLogEventsOnStarted;
            BotEvents.OnBotStopped += CombatLogEvents.Instance.CombatLogEventsOnStopped;
            BotEvents.Player.OnMapChanged += CombatLogEvents.Instance.Player_OnMapChanged;

            ///////////////////////////////////////////////////////////////////
            // Start non invasive user information
            ///////////////////////////////////////////////////////////////////
            TroubleshootLog("Character level: {0}", Me.Level);
            TroubleshootLog("Character Faction: {0}", Me.IsAlliance ? "Alliance" : "Horde");
            TroubleshootLog("Character Race: {0}", Me.Race);
            TroubleshootLog("Character Mapname: {0}", Me.MapName);
            // Talents
            TroubleshootLog("Retrieving Talent Spec");
            try
            {
                TalentManager.Update();
            }
            catch (Exception e)
            {
                StopBot(e.ToString());
            }
            TroubleshootLog(" Character Current Build: {0}", TalentManager.CurrentSpec.ToString());
            // Racials
            TroubleshootLog("Retrieving Racial Abilities");
            foreach ( WoWSpell racial in Spell.CurrentRacials )
            {
                TroubleshootLog(" Character Racial Abilitie: {0} ", racial.Name);
            }
            TroubleshootLog
                (" {0}",
                 Me.IsInInstance ? "Character is currently in an Instance" : "Character seems to be outside an Instance");
            TroubleshootLog
                (" {0}",
                 StyxWoW.Me.CurrentMap.IsArena
                     ? "Character is currently in an Arena"
                     : "Character seems to be outside an Arena");
            TroubleshootLog
                (" {0}",
                 StyxWoW.Me.CurrentMap.IsBattleground
                     ? "Character is currently in a Battleground  "
                     : "Character seems to be outside a Battleground");
            TroubleshootLog
                (" {0}",
                 StyxWoW.Me.CurrentMap.IsDungeon
                     ? "Character is currently in a Dungeon  "
                     : "Character seems to be outside a Dungeon");
            TroubleshootLog("Character HB Pull Range: {0}", Targeting.PullDistance);
            ///////////////////////////////////////////////////////////////////
            // END non invasive user information
            ///////////////////////////////////////////////////////////////////

            // Create the new List of HealableUnit type.
            TroubleshootLog("Initializing list of HealableUnits");
            switch (CLUSettings.Instance.SelectedHealingAquisition)
            {
                case HealingAquisitionMethod.Proximity:
                    HealableUnit.HealableUnitsByProximity();
                    break;
                case HealingAquisitionMethod.RaidParty:
                    HealableUnit.HealableUnitsByPartyorRaid();
                    break;
            }
            TroubleshootLog(" {0}", IsHealerRotationActive ? "Healer Base Detected" : "No Healer Base Detectected");

            // Initialize Botchecks
            TroubleshootLog("Initializing Bot Checker");
            BotChecker.Initialize();

            TroubleshootLog("Initializing Sound Player");
            SoundManager.Initialize();

            TroubleshootLog("Initializing Keybinds");
            this._clupulsetimer.Interval = 1000; // 1second
            this._clupulsetimer.Elapsed += ClupulsetimerElapsed; // Attatch
            this._clupulsetimer.Enabled = true; // Enable
            this._clupulsetimer.AutoReset = true; // To keep raising the Elapsed event

            GC.KeepAlive(this._clupulsetimer);
        }

        public override void OnButtonPress()
        {
            new ConfigurationForm().ShowDialog();
        }

        public override void Pulse()
        {
            PulseHander handler = this.PulseEvent;

            if ( handler != null )
            {
                handler();
            }

            if ( !Me.IsValid || !StyxWoW.IsInGame )
            {
                return;
            }
            switch (StyxWoW.Me.Class)
            {
                case WoWClass.Hunter:
                case WoWClass.DeathKnight:
                case WoWClass.Warlock:
                case WoWClass.Mage:
                    PetManager.Pulse();
                    break;
            }

            if ( IsHealerRotationActive )
            {
                HealableUnit.Pulse(); //
            }
            //ManageOracle();
        }

        public void QueryClassTree()
        {
            try
            {
                Type type = typeof (RotationBase);
                IEnumerable<Type> types =
                    AppDomain.CurrentDomain.GetAssemblies().Where(q => q.Location.Contains(Utilities.AssemblyDirectory))
                        .ToList().SelectMany(s => s.GetTypes()).Where(p => p.IsSubclassOf(type) && !p.IsAbstract);

                this._rotations = new List<RotationBase>();
                foreach ( Type x in types )
                {
                    ConstructorInfo constructorInfo = x.GetConstructor(new Type[] {});
                    if ( constructorInfo != null )
                    {
                        var rb = constructorInfo.Invoke(new object[] {}) as RotationBase;
                        if ( rb != null && SpellManager.HasSpell(rb.KeySpellId) )
                        {
                            TroubleshootLog(" Using " + rb.Name + " rotation. Character has " + rb.KeySpell);
                            this._rotations.Add(rb);
                        }
                        else
                        {
                            if ( rb != null )
                            {
                                TroubleshootLog
                                    (" Skipping " + rb.Name + " rotation. Character is missing " + rb.KeySpell);
                            }
                        }
                    }
                }
                // If there is more than one rotation then display the selector for the user, otherwise just load the one and only.

                if ( this._rotations.Count > 1 )
                {
                    string value = "null";
                    if (
                        GUIHelpers.RotationSelector
                            ("[CLU] " + Version + " Rotation Selector", this._rotations,
                             "Please select your prefered rotation:", ref value) == DialogResult.OK )
                    {
                        this.SetActiveRotation(this._rotations.First(x => value != null && x.Name == value));
                    }
                }
                else
                {
                    if ( this._rotations.Count == 0 )
                    {
                        Log("Couldn't finde a rotation for you, Contact us!");
                        StopBot("Unable to find Active Rotation");
                    }
                    else
                    {
                        RotationBase r = this._rotations.FirstOrDefault();
                        if ( r != null )
                        {
                            Log("Found rotation: " + r.Name);
                            this.SetActiveRotation(r);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach ( Exception exSub in ex.LoaderExceptions )
                {
                    sb.AppendLine(exSub.Message);
                    if ( exSub is FileNotFoundException )
                    {
                        var exFileNotFound = exSub as FileNotFoundException;
                        if ( !string.IsNullOrEmpty(exFileNotFound.FusionLog) )
                        {
                            sb.AppendLine("CLU Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Log(" Woops, we could not set the rotation.");
                Log(errorMessage);
                StopBot(" Unable to find Active Rotation: " + ex);
            }
        }

        #endregion

        #region Methods

        private static void ClupulsetimerElapsed(object sender, ElapsedEventArgs e)
        {
            if ( !Me.IsValid || !StyxWoW.IsInGame )
            {
                return;
            }
            if ( CLUSettings.Instance.EnableKeybinds )
            {
                Keybinds.Pulse();
            }
        }

        private static void StopBot(string reason)
        {
            TroubleshootLog(reason);
            TreeRoot.Stop();
        }

        /// <summary>
        /// sets the character's active rotation.
        /// Print the help located in the specific rotation
        /// </summary>
        /// <param name="rb">an instance of the rotationbase</param>
        private void SetActiveRotation(RotationBase rb)
        {
            Log(" Greetings, level {0} user!", Me.Level);
            Log(" I am CLU.");
            Log(" I will create the perfect system for you.");
            Log(" I suggest we use the " + rb.Name + " rotation. Revision: " + rb.Revision);
            Log(" as I know you have " + rb.KeySpell);
            Log
                (" BotBase: {0}  ({1})", BotManager.Current.Name,
                 BotChecker.SupportedBotBase() ? "Supported" : "Currently Not Supported");
            Log(rb.Help);
            Log(" You can Access CLU's Settings by clicking the CLASS CONFIG button");
            Log(" Let's execute the plan!");

            this._rotationBase = rb;

            this.PulseEvent = null;
            this.PulseEvent += rb.OnPulse;

            // Check for Instancebuddy and warn user that healing is not supported.
            if ( BotChecker.BotBaseInUse("Instancebuddy") &&
                 this.ActiveRotation.GetType().BaseType == typeof (HealerRotationBase) )
            {
                Log(" [BotChecker] Instancebuddy Detected. *UNABLE TO HEAL WITH CLU*");
                StopBot("You cannot use CLU with InstanceBuddy as Healer");
            }

            if ( this.ActiveRotation.GetType().BaseType == typeof (HealerRotationBase) )
            {
                TroubleshootLog
                    (" [HealingChecker] HealerRotationBase Detected. *Activating Automatic HealableUnit refresh*");
                IsHealerRotationActive = true;
            }
        }

        #endregion

        //private const OracleWatchMode OracleWatchFlags = OracleWatchMode.Tank;

        //private static void ManageOracle()
        //{
        //    switch (OracleWatchFlags) {
        //    case OracleWatchMode.Healer:
        //        UnitOracle.WatchUnit(ObjectManager.Me, UnitOracle.Watch.HealthVariance);
        //        if (ObjectManager.Me.IsInRaid) {
        //            ObjectManager.Me.RaidMembers.ForEach(x => UnitOracle.WatchUnit(x, UnitOracle.Watch.HealthVariance));
        //        } else if (ObjectManager.Me.IsInParty) {
        //            ObjectManager.Me.PartyMembers.ForEach(x => UnitOracle.WatchUnit(x, UnitOracle.Watch.HealthVariance));
        //        }
        //        break;
        //    case OracleWatchMode.DPS:
        //        UnitOracle.WatchUnit(ObjectManager.Me, UnitOracle.Watch.HealthVariance);
        //        break;
        //    case OracleWatchMode.Tank:
        //        UnitOracle.WatchUnit(ObjectManager.Me, UnitOracle.Watch.HealthVariance);
        //        if (ObjectManager.Me.IsInRaid) {
        //            ObjectManager.Me.RaidMembers.ForEach(x => UnitOracle.WatchUnit(x, UnitOracle.Watch.HealthVariance));
        //        } else if (ObjectManager.Me.IsInParty) {
        //            ObjectManager.Me.PartyMembers.ForEach(x => UnitOracle.WatchUnit(x, UnitOracle.Watch.HealthVariance));
        //        }
        //        break;
        //    }
        //    // tick
        //    UnitOracle.Pulse();
        //}
    }
}