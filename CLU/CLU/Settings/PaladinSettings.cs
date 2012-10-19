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
using Styx.Helpers;
using CLU.Base;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;


namespace CLU.Settings
{

    internal class PaladinSettings : Styx.Helpers.Settings
    {
        public PaladinSettings()
        : base(CLUSettings.SettingsPath + "_Paladin.xml")
        {
        }

        #region Common

        [Setting]
        [DefaultValue(PaladinBlessing.Kings)]
        [Category("Common")]
        [DisplayName("Paladin Blessing Selector")]
        [Description("Choose a Paladin Blessing. This is on applicable to solo play (ie: not in party or raid.)")]
        public PaladinBlessing BlessingSelection
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(55)]
        [Category("Common")]
        [DisplayName("Word of Glory Health")]
        [Description("Word of Glory will be used at this value (Warning Shared setting for Protection and Retribution, Holy does not use this setting.)")]
        public int WordofGloryPercent
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(40)]
        [Category("Common")]
        [DisplayName("Healthstone Percent")]
        [Description("Will use a Healthstone for self heal at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int HealthstonePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Common")]
        [DisplayName("Seal of Righteousness Add Count")]
        [Description("Will use Seal of Righteousness when agro mob count is equal to or higher then this value.")]
        public int SealofRighteousnessCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Hand of Freedom")]
        [Description("If set to true CLU will use Hand of Freedom. (Self Healing (General Tab) must be enabled as well.)")]
        public bool UseHandofFreedom
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(75)]
        [Category("Common")]
        [DisplayName("Flash of Light Resting Percent")]
        [Description("Will use Flash of Light for self heal when resting at this healthpercent. (Self Healing, and Enable Movement (General Tab) must be enabled as well.)")]
        public int FlashHealRestingPercent
        {
            get;
            set;
        }

        #endregion

        #region Holy

        [Setting]
        [DefaultValue(true)]
        [Category("Holy Spec")]
        [DisplayName("Automaticly apply Beacon of Light")]
        [Description("When this is set to true, CLU will automaticly select the appropriate target for Beacon of Light")]
        public bool HandlePaliBeaconTarget
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(80)]
        //[Category("Holy")]
        //[DisplayName("Light of Dawn Health")]
        //[Description("Light of Dawn will be used at this value")]
        //public int LightOfDawnHealth { get; set; }

        //[Setting]
        //[DefaultValue(2)]
        //[Category("Holy")]
        //[DisplayName("Light of Dawn Count")]
        //[Description("Light of Dawn will be used when there are more then that many players with lower health then LoD Health setting")]
        //public int LightOfDawnCount { get; set; }

        //[Setting]
        //[DefaultValue(90)]
        //[Category("Holy")]
        //[DisplayName("Holy Shock Health")]
        //[Description("Holy Shock will be used at this value")]
        //public int HolyShockHealth { get; set; }

        //[Setting]
        //[DefaultValue(65)]
        //[Category("Holy")]
        //[DisplayName("Divine Light Health")]
        //[Description("Divine Light will be used at this value")]
        //public int DivineLightHealth { get; set; }

        //[Setting]
        //[DefaultValue(50)]
        //[Category("Holy")]
        //[DisplayName("Divine Plea Mana")]
        //[Description("Divine Plea will be used at this value")]
        //public double DivinePleaMana { get; set; }

        #endregion

        #region Protection

        [Setting]
        [DefaultValue(40)]
        [Category("Protection")]
        [DisplayName("Shield of the Righteous Health")]
        [Description("Shield of the Righteous will be used at this value")]
        public int ShoRPercent
        {
            get;
            set;
        }



        [Setting]
        [DefaultValue(50)]
        [Category("Protection")]
        [DisplayName("Holy Prism Health")]
        [Description("Holy Prism will be used at this value")]
        public int HolyPrismPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Protection")]
        [DisplayName("Holy Prism Add Count")]
        [Description("Will use Holy Prism when agro mob count is equal to or higher then this value.")]
        public int HolyPrismCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Protection")]
        [DisplayName("Lights Hammer Add Count")]
        [Description("Will use Lights Hammer when agro mob count is equal to or higher then this value.")]
        public int LightsHammerCount
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(3)]
        [Category("Protection")]
        [DisplayName("Consecration Count")]
        [Description("Consecration will be used when agro mob count is equal to or higher then this value.")]
        public int ConsecrationCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(70)]
        [Category("Protection")]
        [DisplayName("Consecration Mana Percent")]
        [Description("Consecration will be used at this value")]
        public int ConsecrationManaPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(2)]
        [Category("Protection")]
        [DisplayName("Hammer of the Righteous Add Count")]
        [Description("Will use Hammer of the Righteous when agro mob count is equal to or higher then this value.")]
        public int ProtectionHoRCount
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(60)]
        [Category("Protection")]
        [DisplayName("Divine Protection Percent")]
        [Description("Will use Divine Protection at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int ProtectionDPPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Protection")]
        [DisplayName("Lay on Hands Percent")]
        [Description("Will use Lay on Hands at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int ProtectionLoHPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(25)]
        [Category("Protection")]
        [DisplayName("Divine Shield Percent")]
        [Description("Will use Divine Shield at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int ProtectionDSPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Protection")]
        [DisplayName("Holy Avenger Health")]
        [Description("Holy Avenger will be used at this value for additional survivability with ShoR buff.")]
        public int HolyAvengerHealthProt
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(50)]
        [Category("Protection")]
        [DisplayName("Guardian of Ancient Kings Health")]
        [Description("Guardian of Ancient Kings will be used at this value")]
        public int GuardianofAncientKingsHealthProt
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Protection")]
        [DisplayName("Ardent Defender Health")]
        [Description("Ardent Defender will be used at this value")]
        public int ArdentDefenderHealth
        {
            get;
            set;
        }




        [Setting]
        [DefaultValue(15)]
        [Category("Protection")]
        [DisplayName("Seal of Insight Mana")]
        [Description("Seal of Insight for Mana Generation will be used at this Percent")]
        public int SealofInsightMana
        {
            get;
            set;
        }

        #endregion

        #region Retribution

        [Setting]
        [DefaultValue(4)]
        [Category("Retribution")]
        [DisplayName("Hammer of the Righteous Add Count")]
        [Description("Will use Hammer of the Righteous when agro mob count is equal to or higher then this value.")]
        public int RetributionHoRCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(4)]
        [Category("Retribution")]
        [DisplayName("Divine Storm Add Count")]
        [Description("Will use Divine Storm when agro mob count is equal to or higher then this value.")]
        public int DivineStormCount
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(80)]
        [Category("Retribution")]
        [DisplayName("Divine Protection Percent")]
        [Description("Will use Divine Protection at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int RetributionDPPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Retribution")]
        [DisplayName("Lay on Hands Percent")]
        [Description("Will use Lay on Hands at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int RetributionLoHPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(25)]
        [Category("Retribution")]
        [DisplayName("Divine Shield Percent")]
        [Description("Will use Divine Shield at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int RetributionDSPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(20)]
        [Category("Retribution")]
        [DisplayName("Hand of Protection Percent")]
        [Description("Will use Hand of Protection at this healthpercent. (Self Healing (General Tab) must be enabled as well.)")]
        public int RetributionHoPPercent
        {
            get;
            set;
        }
        [Setting]
        [DefaultValue(2)]
        [Category("Retribution")]
        [DisplayName("Light's Hammer Count")]
        [Description("Will use Light's Hammer if AddCount >= this value (AOE Must be enabled)")]
        public int RetributionLightsHammerCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Retribution")]
        [DisplayName("Prevent Inquisition from falling off")]
        [Description("Enable this to prevent Inquisition from fallin off")]
        public bool RetributionInquisitionFallOffProtection
        {
            get;
            set;
        }

        #endregion
    }
}