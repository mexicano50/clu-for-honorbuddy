#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author$
// $LastChangedBy$

#endregion

using System.ComponentModel;
using Styx.Helpers;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;
using Styx.WoWInternals.WoWObjects;

namespace CLU.Settings
{
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
        [DefaultValue(WoWTotem.None)]
        [Category("Elemental Totems")]
        [DisplayName("EarthTotem")]
        [Description("The totem to use for this slot. Select 'None' for automatic usage.")]
        public WoWTotem ElementalEarthTotem
        {
            get;
            set;
        }

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
    }
}