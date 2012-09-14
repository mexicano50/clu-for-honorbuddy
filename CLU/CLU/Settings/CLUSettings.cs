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

// This file was part of Singular - A community driven Honorbuddy CC


using System.ComponentModel;
using Styx;
using Styx.Helpers;
using CLU.Base;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace CLU.Settings
{
    using System;

    using Styx.Common;

    internal class CLUSettings : Styx.Helpers.Settings
    {
        private static CLUSettings _instance;

        public CLUSettings()
        : base(SettingsPath + ".xml")
        {
        }

        public static string SettingsPath
        {
            get {
                return string.Format("{0}\\Settings\\CLUSettings_{1}", Utilities.AssemblyDirectory, StyxWoW.Me.Name); //todo: fix my path
            }
        }

        public static CLUSettings Instance
        {
            get {
                return _instance ?? (_instance = new CLUSettings());
            }
        }

        //

        #region Raid/Party Healing

        [Setting]
        [DefaultValue(1)]
        [Category("Healing")]
        [DisplayName("Healing TTL Delta")]
        [Description("Time to Live Delta for HealableUnit")]
        public int TTLDelta
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(HealingAquisitionMethod.RaidParty)]
        [Category("Healing")]
        [DisplayName("Selected Healing Aquisition")]
        [Description("Select to automaticly add/remove all players based on Raid/Party member info. Select Proximity to automaticly add/remove all players within a proximity of 40 * 40 around you.")]
        public HealingAquisitionMethod SelectedHealingAquisition
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Healing")]
        //[DisplayName("Enable isTank Selection")]
        //[Description("When this is set to true, you will be able to set the tank under the healing helper tab (Click the checkbox)")]
        //public bool EnableTankSelection
        //{
        //    get;
        //    set;
        //}



        //[Setting]
        //[DefaultValue(false)]
        //[Category("Healing")]
        //[DisplayName("Enable isHealer Selection")]
        //[Description("When this is set to true, you will be able to set the healers under the healing helper tab (Click the checkboxs)")]
        //public bool EnableHealerSelection
        //{
        //    get;
        //    set;
        //}

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Healing")]
        //[DisplayName("Enable MainTank Selection")]
        //[Description("When this is set to true, you will be able to set the main tank under the healing helper tab (Click the checkbox). Otherwise CLU will automagicly detect the maintank by 1) blizzards MAINTANK/MAINASSIST method (ilevel) 2) Highest Health.")]
        //public bool EnableMainTankSelection
        //{
        //    get;
        //    set;
        //}

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Healing")]
        //[DisplayName("Enable OffTank Selection")]
        //[Description("When this is set to true, you will be able to set the Off tank under the healing helper tab (Click the checkbox). Otherwise CLU will select it for you based on maintank selection (ie: the tank that is not the maintank)")]
        //public bool EnableOffTankSelection
        //{
        //    get;
        //    set;
        //}

        #endregion


        #region Category: General

        [Setting]
        [DefaultValue(65)]
        [Category("General")]
        [DisplayName("Min Health")]
        [Description("Minimum health to eat at.")]
        public int MinHealth
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(65)]
        [Category("General")]
        [DisplayName("Min Mana")]
        [Description("Minimum mana to drink at.")]
        public int MinMana
        {
            get;
            set;
        }



        //[Setting]
        //[DefaultValue(30)]
        //[Category("General")]
        //[DisplayName("Potion Health")]
        //[Description("Minimum health to use a health pot or health stone at.")]
        //public int PotionHealth { get; set; }

        //[Setting]
        //[DefaultValue(30)]
        //[Category("General")]
        //[DisplayName("Potion Mana")]
        //[Description("Minimum mana to use a mana pot at.")]
        //public int PotionMana { get; set; }


        [Setting]
        [DefaultValue(true)]
        [Category("General")]
        [DisplayName("Use Cooldowns")]
        [Description("When this is set to true, CLU will always use Cooldowns")]
        public bool UseCooldowns
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Burst.onBoss)]
        [Category("General")]
        [DisplayName("Burst (Pop all Cooldowns)")]
        [Description("This will allow for cooldowns to be popped either on a boss (default) or on a mob (for questing) (Enable Cooldowns (General Tab) must be enabled as well.)")]
        public Burst BurstOn
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("General")]
        [DisplayName("Burst onMob Add Count")]
        [Description("Will use All Cooldowns when agro mob count is equal to or higher then this value. (Enable Cooldowns (General Tab) must be enabled as well.)")]
        public int BurstOnMobCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("General")]
        [DisplayName("Use AoE Abilities")]
        [Description("When this is set to true, CLU will always use AoE abilities such as DnD, Multishot, MindSear, HellFire, etc")]
        public bool UseAoEAbilities
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("General")]
        [DisplayName("Enable Raid and Party Buffing")]
        [Description("When this is set to true, CLU will always buff the Raid or Party with thier specific abilitie (Kings, MoTW, Fortitude, etc)")]
        public bool EnableRaidPartyBuffing
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("General")]
        [DisplayName("Enable Interupts")]
        [Description("When this is set to true, CLU will always interupt the current target with thier specific abilitie such as Kick, Rebuke, Mind Freeze, etc")]
        public bool EnableInterupts
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("General")]
        [DisplayName("Enable Dispel")]
        [Description("When this is set to true, CLU will Dispel Harmful debuffs (Includes Filters for Urgent Debuffs and Blacklisted Debuffs) ONLY USED WITH HEALING ROTATIONS")]
        public bool EnableDispel
        {
            get;
            set;
        }

        #endregion


        #region Category: Keybinds


        [Setting]
        [DefaultValue(false)]
        [Category("Keybinds")]
        [DisplayName("Enable Keybinds")]
        [Description("Enable the use of Keybinds.")]
        public bool EnableKeybinds
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Keybinds")]
        [DisplayName("Enable Keybind Sounds")]
        [Description("When this is set to true, CLU will play Keybind Sounds to indicate Toggled State.")]
        public bool EnableKeybindSounds
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Pause Rotation")]
        [Description("Select the Keybind for [PauseRotation]  DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindPauseRotation
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Use Cooldowns")]
        [Description("Select the Keybind for [UseCooldowns] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindUseCooldowns
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Click Extra Action Button")]
        [Description("Select the Keybind for [ClickExtraActionButton] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindClickExtraActionButton
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Use AoE Abilities")]
        [Description("Select the Keybind for [UseAoEAbilities] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindUseAoEAbilities
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Enable Raid Party Buffing")]
        [Description("Select the Keybind for [EnableRaidPartyBuffing] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindEnableRaidPartyBuffing
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Self Healing")]
        [Description("Select the Keybind for [EnableSelfHealing] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindHealEnableSelfHealing
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Enable Multi-Dotting")]
        [Description("Select the Keybind for [EnableMultiDotting] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindEnableMultiDotting
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Enable Interupts")]
        [Description("Select the Keybind for [EnableInterupts] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindEnableInterupts
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(Keyboardfunctions.Nothing)]
        [Category("Keybinds")]
        [DisplayName("Toggle Chakra Stance")]
        [Description("Select the Keybind for [ChakraStanceSelection] DO NOT SET THE SAME KEYBIND MORE THAN ONCE!!!")]
        public Keyboardfunctions KeybindChakraStanceSelection
        {
            get;
            set;
        }

        #endregion

        #region Category: Misc

        [Setting]
        [DefaultValue(false)]
        [Category("Misc")]
        [DisplayName("Enable Client Lag Detection")]
        [Description("When this is set to true, CLU will automaticly detect your Client Lag (via Client Latency:)")]
        public bool EnableClientLagDetection
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Misc")]
        [DisplayName("Pause Rotation")]
        [Description("When this is set to true, CLU will Pause the current Rotation (Combat Buffs and Rest Behavior will still function)")]
        public bool PauseRotation
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Misc")]
        [DisplayName("Enable Multi-Dotting")]
        [Description("When this is set to true, CLU will always Multi DoT targets")]
        public bool EnableMultiDotting
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Misc")]
        [DisplayName("Click Extra Action Button")]
        [Description("When this is set to true, CLU will always Click the Extra Action Button in DeathWing Encounters")]
        public bool ClickExtraActionButton
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(false)]
        [Category("Misc")]
        [DisplayName("Debug Logging")]
        [Description("Enables debug logging from CLU. This will cause quite a bit of spam. Use it for diagnostics only.")]
        public bool EnableDebugLogging
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(false)]
        [Category("Misc")]
        [DisplayName("Enable Movement/Targeting Logging")]
        [Description("Enable movement/Targeting  logging to HB output")]
        public bool MovementLogging
        {
            get;
            set;
        }

        #endregion

        #region Category: Movement

        


        [Setting]
        [DefaultValue(false)]
        [Category("Movement")]
        [DisplayName("Enable Movement")]
        [Description("Enable movement within the CC.")]
        public bool EnableMovement
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Movement")]
        [DisplayName("Enable Move Behind Target")]
        [Description("Enable Moving Behind the Target within the CC. (when movement is enabled). Enable for rogues, kittys, etc")]
        public bool EnableMoveBehindTarget
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Movement")]
        [DisplayName("Enable Targeting")]
        [Description("When this is true CLU will attempt to aquire the best target. Only a Valid setting when Movement is enabled!!")]
        public bool EnableTargeting
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Movement")]
        [DisplayName("Never Dismount")]
        [Description("When this is set to true, CLU will never dismount")]
        public bool NeverDismount
        {
            get;
            set;
        }

        #endregion

        #region Category: Healing

        [Setting]
        [DefaultValue(true)]
        [Category("General")]
        [DisplayName("Self Healing")]
        [Description("When this is set to true, CLU will automaticly handle Healing such as Vampiric Blood, Lay on Hands, Healthstones, etc")]
        public bool EnableSelfHealing
        {
            get;
            set;
        }

        #endregion

        #region Category: Items

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Items")]
        //[DisplayName("Use Flasks")]
        //[Description("Uses Flask of the North or Flask of Enhancement.")]
        //public bool UseAlchemyFlasks { get; set; }

        //[Setting]
        //[DefaultValue(TrinketUsage.Never)]
        //[Category("Items")]
        //[DisplayName("Trinket 1 Usage")]
        //public TrinketUsage Trinket1Usage { get; set; }

        //[Setting]
        //[DefaultValue(TrinketUsage.Never)]
        //[Category("Items")]
        //[DisplayName("Trinket 2 Usage")]
        //public TrinketUsage Trinket2Usage { get; set; }

        #endregion

        #region Category: Racials

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Racials")]
        //[DisplayName("Use Racials")]
        //public bool UseRacials { get; set; }

        //[Setting]
        //[DefaultValue(30)]
        //[Category("Racials")]
        //[DisplayName("Gift of the Naaru HP")]
        //[Description("Uses Gift of the Naaru when HP falls below this %.")]
        //public int GiftNaaruHP { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Racials")]
        //[DisplayName("Shadowmeld Threat Drop")]
        //[Description("When in a group (and not a tank), uses shadowmeld as a threat drop.")]
        //public bool ShadowmeldThreatDrop { get; set; }

        #endregion

        #region Category: Tanking

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Tanking")]
        //[DisplayName("Enable Taunting for tanks")]
        //public bool EnableTaunting { get; set; }

        #endregion

        #region Class Late-Loading Wrappers

        // Do not change anything within this region.
        // It's written so we ONLY load the settings we're going to use.
        // There's no reason to load the settings for every class, if we're only executing code for a Druid.

        private DeathKnightSettings _dkSettings;

        private DruidSettings _druidSettings;

        private HunterSettings _hunterSettings;

        private MageSettings _mageSettings;

        private MonkSettings _monkSettings;

        private PaladinSettings _pallySettings;

        private PriestSettings _priestSettings;

        private RogueSettings _rogueSettings;

        private ShamanSettings _shamanSettings;

        private WarlockSettings _warlockSettings;

        private WarriorSettings _warriorSettings;

        [Browsable(false)]
        public DeathKnightSettings DeathKnight
        {
            get {
                return _dkSettings ?? (_dkSettings = new DeathKnightSettings());
            }
        }

        [Browsable(false)]
        public DruidSettings Druid
        {
            get {
                return _druidSettings ?? (_druidSettings = new DruidSettings());
            }
        }

        [Browsable(false)]
        public HunterSettings Hunter
        {
            get {
                return _hunterSettings ?? (_hunterSettings = new HunterSettings());
            }
        }

        [Browsable(false)]
        public MageSettings Mage
        {
            get {
                return _mageSettings ?? (_mageSettings = new MageSettings());
            }
        }

        [Browsable(false)]
        public MonkSettings Monk
        {
            get {
                return _monkSettings ?? (_monkSettings = new MonkSettings());
            }
        }

        [Browsable(false)]
        public PaladinSettings Paladin
        {
            get {
                return _pallySettings ?? (_pallySettings = new PaladinSettings());
            }
        }

        [Browsable(false)]
        public PriestSettings Priest
        {
            get {
                return _priestSettings ?? (_priestSettings = new PriestSettings());
            }
        }

        [Browsable(false)]
        public RogueSettings Rogue
        {
            get {
                return _rogueSettings ?? (_rogueSettings = new RogueSettings());
            }
        }

        [Browsable(false)]
        public ShamanSettings Shaman
        {
            get {
                return _shamanSettings ?? (_shamanSettings = new ShamanSettings());
            }
        }

        [Browsable(false)]
        public WarlockSettings Warlock
        {
            get {
                return _warlockSettings ?? (_warlockSettings = new WarlockSettings());
            }
        }

        [Browsable(false)]
        public WarriorSettings Warrior
        {
            get {
                return _warriorSettings ?? (_warriorSettings = new WarriorSettings());
            }
        }

        #endregion
    }
}