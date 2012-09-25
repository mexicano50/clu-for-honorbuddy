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
using CLU.Base;
using Styx.Helpers;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace CLU.Settings
{
    using Styx.WoWInternals;

    internal class ShamanSettings : Styx.Helpers.Settings
    {
        public ShamanSettings()
        : base(CLUSettings.SettingsPath + "_Shaman.xml")
        {
        }

        #region Totems

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Enable Totem Control")]
        [Description("When this is set to true, CLU will either automaticly select the appropriate totem or select the totem you have assigned and drop it. Set this to false if you do not want CLU to do anything with totems.")]
        public bool HandleTotems
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(StormlashTotem.OnHaste)]
        [Category("Common")]
        [DisplayName("Stormlash Totem Usage")]
        [Description("Will pop Stormlash Totem. Will pop Stormlash Totem on Heroism, Bloodlust, or any of the Haste buffs, or Cooldown or never.")]
        public StormlashTotem UseStormlashTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Elemental Totems")]
        [DisplayName("EarthTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem ElementalEarthTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Elemental Totems")]
        [DisplayName("WaterTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem ElementalWaterTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Elemental Totems")]
        [DisplayName("AirTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem ElementalAirTotem
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(WoWTotem.None)]
        //[Category("Elemental Totems")]
        //[DisplayName("FireTotem")]
        //[Description("The totem to use for this slot.")]
        //public WoWTotem ElementalFireTotem
        //{
        //    get;
        //    set;
        //}

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Enhancement Totems")]
        [DisplayName("EarthTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem EnhancementEarthTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Enhancement Totems")]
        [DisplayName("WaterTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem EnhancementWaterTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Enhancement Totems")]
        [DisplayName("AirTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem EnhancementAirTotem
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(WoWTotem.None)]
        //[Category("Enhancement Totems")]
        //[DisplayName("FireTotem")]
        //[Description("The totem to use for this slot.")]
        //public WoWTotem EnhancementFireTotem
        //{
        //    get;
        //    set;
        //}

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Restoration Totems")]
        [DisplayName("EarthTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem RestorationEarthTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Restoration Totems")]
        [DisplayName("WaterTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem RestorationWaterTotem
        {
            get;
            set;
        }

        [Browsable(false)]
        [Setting]
        [DefaultValue(WoWTotem.None)]
        [Category("Restoration Totems")]
        [DisplayName("AirTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem RestorationAirTotem
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(WoWTotem.None)]
        //[Category("Restoration Totems")]
        //[DisplayName("FireTotem")]
        //[Description("The totem to use for this slot.")]
        //public WoWTotem RestorationFireTotem
        //{
        //    get;
        //    set;
        //}

        #endregion

        #region Enhancement
        [Setting]
        [DefaultValue(true)]
        [Category("Enhancement")]
        [DisplayName("Ascendance")]
        [Description("Let's you change to use Ascendance cooldown on Boss or just blow it on cooldown")]
        public bool Ascendance
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Enhancement")]
        [DisplayName("Feral Spirit")]
        [Description("Let's you change to use Feral Spirit on cooldown on Boss or just blow it on cooldown")]
        public bool FeralSpirit
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(OnBoss)]
        [Category("Enhancement")]
        [DisplayName("Elemental Mastery")]
        [Description("Let's you change to use Elemental Mastery on cooldown on Boss or just blow it on cooldown")]
        public bool ElementalMastery
        {
            get;
            set;
        }

        #endregion

        #region Elemental


        #endregion

        #region Restoration
        [Setting]
        [DefaultValue(true)]
        [Category("Restoration")]
        [DisplayName("Automaticly apply Earth Shield")]
        [Description("When this is set to true, CLU will automaticly select the appropriate target for Earth Shield")]
        public bool HandleEarthShieldTarget
        {
            get;
            set;
        }

        #endregion

        #region PvP

        [Setting]
        [DefaultValue("Input the name of your Main-Hand weapon here")]
        [Category("PvP")]
        [DisplayName("PvP Main-Hand weapon item name")]
        [Description("Type in the name of your Main-Hand to use when switching to Offensive & Devensive Modes. (Only used in PvP when using Dagradt's PvP Rotations)")]
        public string PvPMainHandItemName { get; set; }

        [Setting]
        [DefaultValue("Input the name of your Off-Hand weapon here")]
        [Category("PvP")]
        [DisplayName("PvP Off-Hand weapon item name")]
        [Description("Type in the name of your Off-Hand to use when switching to Offensive Modes. (Only used in PvP when using Dagradt's PvP Rotations)")]
        public string PvPOffHandItemName { get; set; }

        [Setting]
        [DefaultValue("Input the name of your Shield here")]
        [Category("PvP")]
        [DisplayName("PvP Shield item name")]
        [Description("Type in the name of your Shield to use when switching to Devensive Mode. (Only used in PvP when using Dagradt's PvP Rotations)")]
        public string PvPShieldItemName { get; set; }

        #endregion
    }
}