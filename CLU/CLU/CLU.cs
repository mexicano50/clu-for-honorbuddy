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
using CLU.Base;
using CLU.Classes;
using CLU.CombatLog;
using CLU.GUI;
using CLU.Helpers;
using CLU.Managers;
using CLU.Settings;
using CLU.Sound;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.DBC;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;
using Timer = System.Timers.Timer;

/*Credits
=======
* cowdude for his initial work with Felmaster and giving me inspiration to create this CC.
* All the Singular Developers For there tiresome work and supporting code
* Weischbier for his Support with the new 5.0.4 MoP Changes.
* bobby53 For his Disclaimer (Couldn't have said it better)
* Kickazz006 for his BlankProfile.xml
* Shaddar & Mentally for thier initial detection code of HourofTwilight, FadingLight and Shrapnel
* bennyquest for his continued efforts in reporting issues!
* Jamjar0207 for his Protection Warrior Rotation and Fire Mage
* gniegsch for his Arms Warrior Rotation and improvements with blood deathknight
* Stormchasing for his help with warlocks and patchs
* Obliv For his Boomkin, Frost Mage & Assassination rotations and Arms warrior improvement
* ShinobiAoshi for his initial Affliction warlock rotation
* fluffyhusky for his initial Enhancement Shaman rotation
* Digitalmocking for his initial Elemental Shaman rotation
* Toney001 for his improvements to the Unholy Deathknight rotation
* kbrebel04  for his improvements to Subtlety Rogue
* alxaw (HB9806B90) for his initial Arms rotation
* Dagradt for his PvP rotations.
* LaoArchAngel for his help with core changes and Rogue changes.
* Condemned for Mage Frost Update
* Apoc for just about everything :P */

namespace CLU
{
    internal delegate void PulseHander();

    public class CLU : CombatRoutine
    {
        #region Constants and Fields

        public static readonly Version Version = new Version(3, 3, 7);
        private readonly Timer _clupulsetimer = new Timer(10000); // A timer for keybinds
        private RotationBase _rotationBase;
        private List<RotationBase> _rotations; // list of Rotations
        private Composite _combatBehavior;
        private Composite _combatBuffBehavior;
        private Composite _preCombatBuffBehavior;
        private Composite _pullBehavior;
        private Composite _restBehavior;
        internal static event EventHandler<LocationContextEventArg> OnLocationContextChanged;
        internal static GroupLogic LastLocationContext { get; set; }
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

        public override bool WantButton { get { return true; } }

        public static CLU Instance { get; private set; }

        public static bool IsHealerRotationActive { get; private set; }

        public override WoWClass Class { get { return StyxWoW.Me.Class; } }

        public override string Name { get { return "CLU (Codified Likeness Utility) " + Version; } }
        public static ulong LastTargetGuid = 0;
        /// <summary>
        /// If we havnt loaded a rotation for our character then do so by querying our character's class tree based on a Keyspell.
        /// </summary>
        public RotationBase ActiveRotation
        {
            get
            {
                if (this._rotationBase == null)
                {
                    CLULogger.TroubleshootLog("ActiveRotation is null..retrieving.");
                    this.QueryClassTree();
                    if (this._rotationBase == null)
                    {
                        if (TalentManager.CurrentSpec == WoWSpec.None)
                        {
                            CLULogger.Log(" Greetings, level {0} user. Unfortunelty CLU does not support such Low Level players at this time", Me.Level);
                            StopBot("Unable to find Active Rotation");
                        }
                        else
                        {
                            CLULogger.Log(" Greetings, level {0} user. Unfortunelty CLU could not find a rotation for you.", Me.Level);
                            StopBot("Unable to find Active Rotation");
                        }
                    }
                }

                return this._rotationBase;
            }
        }

        /// <summary>
        /// Depending on if we are grouped or solo, and if we are in a dungeon or battleground, return the rotation we will use.
        /// </summary>
        private Composite Rotation
        {
            get
            {
                Composite currentrotation = null;
                        switch (LocationContext)
                        {
                           
                            case GroupLogic.PVE:
                                currentrotation = this.ActiveRotation.PVERotation;
                                CLULogger.TroubleshootLog(" Setting Current rotation to {0}", GroupLogic.PVE.ToString());
                                break;
                            case GroupLogic.Battleground:
                                currentrotation = this.ActiveRotation.PVPRotation;
                                CLULogger.TroubleshootLog(" Setting Current rotation to {0}", GroupLogic.Battleground.ToString());
                                break;
                            default:
                                currentrotation = this.ActiveRotation.SingleRotation;
                                CLULogger.TroubleshootLog(" Setting Current rotation to {0}", GroupLogic.Solo.ToString());
                                break;
                        }
                return new Sequence
                    (new DecoratorContinue(x => CLUSettings.Instance.EnableMovement && (!Me.IsCasting || !Spell.PlayerIsChanneling), Movement.MovingFacingBehavior()),
                     new DecoratorContinue(x => Me.CurrentTarget != null || IsHealerRotationActive, currentrotation)); 
            }
        }

        /// <summary>
        /// CLU Combat heal/buffs Behavior
        /// </summary>
        private Composite Medic { get { return this.ActiveRotation.Medic; } }

        /// <summary>
        /// CLU Pre Combat buffs Behavior
        /// </summary>
        private Composite PreCombat { get { return this.ActiveRotation.PreCombat; } }

        /// <summary>
        /// CLU Rest Behavior
        /// </summary>
        private Composite Resting { get { return this.ActiveRotation.Resting; } }

        /// <summary>
        /// CLU Pulling Behavior
        /// </summary>
        private Composite Pulling
        {
            get
            {
                return new Sequence(
                    new DecoratorContinue(x => CLUSettings.Instance.EnableMovement && (!Me.IsCasting || !Spell.PlayerIsChanneling), Movement.MoveToPull()),
                    new DecoratorContinue(x => true, ActiveRotation.Pull));
            }
        }

        public override Composite CombatBehavior { get { return this._combatBehavior; } }

        public override Composite CombatBuffBehavior { get { return this._combatBuffBehavior; } }

        public override Composite PreCombatBuffBehavior { get { return this._preCombatBuffBehavior; } }

        public override Composite PullBehavior { get { return this._pullBehavior; } }

        public override Composite RestBehavior { get { return this._restBehavior; } }
        public bool CreateBehaviors()
        {
            CLULogger.TroubleshootLog("CreateBehaviors called.");
            // let behaviors be notified if context changes.
            if (OnLocationContextChanged != null)
                OnLocationContextChanged(this, new LocationContextEventArg(LocationContext, LastLocationContext));

            //Caching the context to not recreate same behaviors repeatedly.
            LastLocationContext = LocationContext;

            _rotationBase = null;

            if (_combatBehavior != null) this._combatBehavior = new Decorator(ret => AllowPulse, new LockSelector(this.Rotation));

            if (_combatBuffBehavior != null) this._combatBuffBehavior = new Decorator(ret => AllowPulse, new LockSelector(this.Medic));

            if (_preCombatBuffBehavior != null) this._preCombatBuffBehavior = new Decorator(ret => AllowPulse, new LockSelector(this.PreCombat));

            if (_restBehavior != null) this._restBehavior = new Decorator(ret => !(CLUSettings.Instance.NeverDismount && IsMounted) && !Me.IsFlying, new LockSelector(this.Resting));

            if (_pullBehavior != null) this._pullBehavior = new Decorator(ret => AllowPulse,  new LockSelector(this.Pulling));

            return true;
        }

        #endregion

        #region Properties

        internal static GroupType GroupType
        {
            get
            {
                if (Me.GroupInfo.IsInRaid)
                {
                    return GroupType.Raid;
                }

                return Me.GroupInfo.IsInParty ? GroupType.Party : GroupType.Solo;
            }
        }


        internal static GroupLogic LocationContext
        {
            get
            {
                // setting to overide the LocationContext so that you can test and PvP, PvE, Single rotation without being in the normal correct context
                if (CLUSettings.Instance.EnableRotationOveride) return CLUSettings.Instance.RotationOveride; 

                Map map = StyxWoW.Me.CurrentMap;

                if (map.IsBattleground || map.IsArena)
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
                //if (Me.IsOnTransport)
                //{
                //    return false; // TODO: WARNING THIS WILL KILL THE ROTATION FOR THE SHIP IN DEEPHOLME AND ELEGON
                //}
                if (CLUSettings.Instance.NeverDismount && IsMounted)
                {
                    return false;
                }
                // if (Buff.PlayerBuffTimeLeft("Cannibalize") > 1) return false;
                if (Buff.PlayerHasBuff("Health Funnel"))
                {
                    return false;
                }
                return true;
            }
        }

        public static bool IsMounted
        {
            get
            {
                 //[Aquatic Form] [Bear Form] [Cat Form] [Flight Form] [Swift Flight Form] [Travel Form]
                //switch (StyxWoW.Me.Shapeshift)
                //{
                //    case ShapeshiftForm.FlightForm:
                //    case ShapeshiftForm.EpicFlightForm:
                //        return true;
                //}
                // TODO: This is temporary fix until shapeshift is fixed.
                if (Buff.PlayerHasBuff("Flight Form")) return true;
                if (Buff.PlayerHasBuff("Swift Flight Form")) return true;
                if (Buff.PlayerHasBuff("Travel Form")) return true;
                if (Buff.PlayerHasBuff("Aquatic Form")) return true;
                return StyxWoW.Me.Mounted;
            }
        }

        private static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        #endregion

        #region Public Methods

       
        

        public override void Initialize()
        {
            CLULogger.TroubleshootLog("Attatching BotEvents");
            BotEvents.OnBotStarted += CombatLogEvents.Instance.CombatLogEventsOnStarted;
            BotEvents.OnBotStopped += CombatLogEvents.Instance.CombatLogEventsOnStopped;
            BotEvents.OnBotChanged += CombatLogEvents.Instance.BotBaseChange;
            BotEvents.Player.OnMapChanged += CombatLogEvents.Instance.Player_OnMapChanged;
            RoutineManager.Reloaded += RoutineManagerReloaded;

            ///////////////////////////////////////////////////////////////////
            // Start non invasive user information
            ///////////////////////////////////////////////////////////////////
            CLULogger.TroubleshootLog("Character level: {0}", Me.Level);
            CLULogger.TroubleshootLog("Character Faction: {0}", Me.IsAlliance ? "Alliance" : "Horde");
            CLULogger.TroubleshootLog("Character Race: {0}", Me.Race);
            CLULogger.TroubleshootLog("Character Mapname: {0}", Me.MapName);
            CLULogger.TroubleshootLog("GroupType: {0}", GroupType.ToString());
            CLULogger.TroubleshootLog("LocationContext: {0}", LocationContext.ToString());
            // Talents
            CLULogger.TroubleshootLog("Retrieving Talent Spec");
            try
            {
                TalentManager.Update();
            }
            catch (Exception e)
            {
                StopBot(e.ToString());
            }
            CLULogger.TroubleshootLog(" Character Current Build: {0}", TalentManager.CurrentSpec.ToString());

            // Intialize Behaviors....TODO: Change this ?
            if (_combatBehavior == null)
                _combatBehavior = new PrioritySelector();

            if (_combatBuffBehavior == null)
                _combatBuffBehavior = new PrioritySelector();

            if (_preCombatBuffBehavior == null)
                _preCombatBuffBehavior = new PrioritySelector();

            if (_pullBehavior == null)
                _pullBehavior = new PrioritySelector();

            if (_restBehavior == null)
                _restBehavior = new PrioritySelector();


            // Behaviors
            if (!CreateBehaviors())
            {
                return;
            }
            CLULogger.TroubleshootLog(" Behaviors created!");
            // Racials
            CLULogger.TroubleshootLog("Retrieving Racial Abilities");
            foreach (WoWSpell racial in Racials.CurrentRacials)
            {
                CLULogger.TroubleshootLog(" Character Racial Abilitie: {0} ", racial.Name);
            }
            CLULogger.TroubleshootLog(" {0}", Me.IsInInstance ? "Character is currently in an Instance" : "Character seems to be outside an Instance");
            CLULogger.TroubleshootLog(" {0}", StyxWoW.Me.CurrentMap.IsArena ? "Character is currently in an Arena" : "Character seems to be outside an Arena");
            CLULogger.TroubleshootLog(" {0}", StyxWoW.Me.CurrentMap.IsBattleground ? "Character is currently in a Battleground  " : "Character seems to be outside a Battleground");
            CLULogger.TroubleshootLog(" {0}", StyxWoW.Me.CurrentMap.IsDungeon ? "Character is currently in a Dungeon  " : "Character seems to be outside a Dungeon");
            CLULogger.TroubleshootLog("Character HB Pull Range: {0}", Targeting.PullDistance);
            ///////////////////////////////////////////////////////////////////
            // END non invasive user information
            ///////////////////////////////////////////////////////////////////

            // Create the new List of HealableUnit type.
            CLULogger.TroubleshootLog("Initializing list of HealableUnits");
            switch (CLUSettings.Instance.SelectedHealingAquisition)
            {
                case HealingAquisitionMethod.Proximity:
                    HealableUnit.HealableUnitsByProximity();
                    break;
                case HealingAquisitionMethod.RaidParty:
                    HealableUnit.HealableUnitsByPartyorRaid();
                    break;
            }
            CLULogger.TroubleshootLog(" {0}", IsHealerRotationActive ? "Healer Base Detected" : "No Healer Base Detectected");

            // Initialize Botchecks
            CLULogger.TroubleshootLog("Initializing Bot Checker");
            BotChecker.Initialize();

            CLULogger.TroubleshootLog("Initializing Sound Player");
            SoundManager.Initialize();

            CLULogger.TroubleshootLog("Initializing Keybinds");
            this._clupulsetimer.Interval = 1000; // 1second
            this._clupulsetimer.Elapsed += ClupulsetimerElapsed; // Attatch
            this._clupulsetimer.Enabled = true; // Enable
            this._clupulsetimer.AutoReset = true; // To keep raising the Elapsed event

            GC.KeepAlive(this._clupulsetimer);

            //TroubleshootLog("Initializing DpsMeter");
            //DpsMeter.Initialize();
            CLULogger.TroubleshootLog("Initialization Complete");
        }

        private void RoutineManagerReloaded(object sender, EventArgs e)
        {
            CLULogger.TroubleshootLog("Routines were reloaded, re-creating behaviors");
            CreateBehaviors();
        }

        private static ConfigurationForm _a = null;
        public override void OnButtonPress()
        {
            if (_a == null) _a = new ConfigurationForm();
            if (_a != null || _a.IsDisposed) _a.ShowDialog();
        }

        public override void Pulse()
        {
            if (!Me.IsValid || !StyxWoW.IsInGame)
            {
                return;
            }

            PulseHander handler = this.PulseEvent;

            if (handler != null)
            {
                handler();
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

            if (IsHealerRotationActive)
            {
                HealableUnit.Pulse(); //
            }
            else
            {
                // Make sure we have the proper target from Targeting. 
                // The Botbase should give us the best target in targeting.
                var firstUnit = Targeting.Instance.FirstUnit;
                if (CLUSettings.Instance.EnableTargeting && firstUnit != null)
                {
                    if (StyxWoW.Me.CurrentTarget != firstUnit)
                        firstUnit.Target();
                }
            }
        }

        /// <summary>This will: loop assemblies,
        /// loop types,
        /// filter types that are a subclass of RotationBase and not the abstract,
        /// create an instance of a RotationBase subclass so we can interigate KeySpell within the RotationBase subclass,
        /// Check if the character has the Keyspell,
        /// Set the active rotation to the matching RotationBase subclass.</summary>
        private void QueryClassTree()
        {
            try
            {
                Type type = typeof(RotationBase);
                //no need to get ALL Assemblies, need only the executed ones
                IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(p => p.IsSubclassOf(type));
                //_rotations.AddRange(new TypeLoader<RotationBase>(null));

                this._rotations = new List<RotationBase>();
                foreach (Type x in types)
                {
                    ConstructorInfo constructorInfo = x.GetConstructor(new Type[] { });
                    if (constructorInfo != null)
                    {
                        var rb = constructorInfo.Invoke(new object[] { }) as RotationBase;
                        if (rb != null && SpellManager.HasSpell(rb.KeySpellId))
                        {
                            CLULogger.TroubleshootLog(" Using " + rb.Name + " rotation. Character has " + rb.KeySpell);
                            this._rotations.Add(rb);
                        }
                        else
                        {
                            if (rb != null)
                            {
                                //TroubleshootLog(" Skipping " + rb.Name + " rotation. Character is missing " + rb.KeySpell);
                            }
                        }
                    }
                }
                // If there is more than one rotation then display the selector for the user, otherwise just load the one and only.

                if (this._rotations.Count > 1)
                {
                    string value = "null";
                    if (
                        GUIHelpers.RotationSelector
                            ("[CLU] " + Version + " Rotation Selector", this._rotations,
                             "Please select your prefered rotation:", ref value) == DialogResult.OK)
                    {
                        this.SetActiveRotation(this._rotations.First(x => value != null && x.Name == value));
                    }
                }
                else
                {
                    if (this._rotations.Count == 0)
                    {
                        CLULogger.Log("Couldn't finde a rotation for you, Contact us!");
                        StopBot("Unable to find Active Rotation");
                    }
                    else
                    {
                        RotationBase r = this._rotations.FirstOrDefault();
                        if (r != null)
                        {
                            CLULogger.Log("Found rotation: " + r.Name);
                            this.SetActiveRotation(r);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException)
                    {
                        var exFileNotFound = exSub as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("CLU Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                CLULogger.Log(" Woops, we could not set the rotation.");
                CLULogger.Log(errorMessage);
                StopBot(" Unable to find Active Rotation: " + ex);
            }
        }

        #endregion

        #region Methods

        private static void ClupulsetimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Me.IsValid || !StyxWoW.IsInGame)
            {
                return;
            }
            if (CLUSettings.Instance.EnableKeybinds)
            {
                Keybinds.Pulse();
            }
        }

        private static void StopBot(string reason)
        {
            CLULogger.TroubleshootLog(reason);
            TreeRoot.Stop();
        }

        /// <summary>
        /// sets the character's active rotation.
        /// Print the help located in the specific rotation
        /// </summary>
        /// <param name="rb">an instance of the rotationbase</param>
        private void SetActiveRotation(RotationBase rb)
        {
            CLULogger.Log(" Greetings, level {0} user!", Me.Level);
            CLULogger.Log(" I am CLU.");
            CLULogger.Log(" I will create the perfect system for you.");
            CLULogger.Log(" I suggest we use the " + rb.Name + " rotation. Revision: " + rb.Revision);
            CLULogger.Log(" as I know you have " + rb.KeySpell);
            CLULogger.Log(" BotBase: {0}  ({1})", BotManager.Current.Name, BotChecker.SupportedBotBase() ? "Supported" : "Currently Not Supported");
            CLULogger.Log(rb.Help);
            CLULogger.Log(" You can Access CLU's Settings by clicking the CLASS CONFIG button");
            CLULogger.Log(" Let's execute the plan!");

            this._rotationBase = rb;

            this.PulseEvent = null;
            this.PulseEvent += rb.OnPulse;

            // Check for Instancebuddy and warn user that healing is not supported.
            if (BotChecker.BotBaseInUse("Instancebuddy") && this.ActiveRotation.GetType().BaseType == typeof(HealerRotationBase))
            {
                CLULogger.Log(" [BotChecker] Instancebuddy Detected. *UNABLE TO HEAL WITH CLU*");
                StopBot("You cannot use CLU with InstanceBuddy as Healer");
            }

            if (this.ActiveRotation.GetType().BaseType == typeof(HealerRotationBase))
            {
                CLULogger.TroubleshootLog(" [HealingChecker] HealerRotationBase Detected. *Activating Automatic HealableUnit refresh*");
                IsHealerRotationActive = true;
            }
        }

        #endregion

        #region Nested type: LocationContextEventArg
        public class LocationContextEventArg : EventArgs
        {
            public readonly GroupLogic CurrentLocationContext;
            public readonly GroupLogic PreviousLocationContext;

            public LocationContextEventArg(GroupLogic currentLocationContext, GroupLogic prevLocationContext)
            {
                CurrentLocationContext = currentLocationContext;
                PreviousLocationContext = prevLocationContext;
            }
        }
        #endregion
        /* LockSelector taken from Singular to test some stuff and maybe fix some weird behavior - done by Stormchasing */
        /// <summary>
        /// This behavior wraps the child behaviors in a 'FrameLock' which can provide a big performance improvement 
        /// if the child behaviors makes multiple api calls that internally run off a frame in WoW in one CC pulse.
        /// </summary>
        private class LockSelector : PrioritySelector
        {
            public LockSelector(params Composite[] children)
                : base(children)
            {
            }

            public override RunStatus Tick(object context)
            {
                if (!CLUSettings.Instance.EnableFrameLock)
                {
                    return base.Tick(context);
                }
                using (StyxWoW.Memory.AcquireFrame())
                {
                    return base.Tick(context);
                }
            }
        }
    }
}