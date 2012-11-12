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

    internal class RogueSettings : Styx.Helpers.Settings
    {
        public RogueSettings()
        : base(CLUSettings.SettingsPath + "_Rogue.xml")
        {
        }

        #region Common

        [Setting]
        [DefaultValue(MHPoisonType.Deadly)]
        [Category("Common")]
        [DisplayName("Main Hand Poison")]
        [Description("Main Hand Poison")]
        public MHPoisonType MainHandPoison
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(OHPoisonType.Crippling)]
        [Category("Common")]
        [DisplayName("Off Hand Poison")]
        [Description("Off Hand Poison")]
        public OHPoisonType OffHandPoison
        {
            get;
            set;
        }

        
        [Setting]
        [DefaultValue(false)]
        [Category("Common")]
        [DisplayName("Enable Always Stealth")]
        [Description("When set to true CLU will always maintain stealth")]
        public bool EnableAlwaysStealth
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Interrupt Spells")]
        //[Description("Interrupt Spells")]
        //public bool InterruptSpells { get; set; }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Use Tricks Of The Trade")]
        [Description("Use Tricks Of The Trade")]
        public bool UseTricksOfTheTrade
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Force use Tricks Of The Trade")]
        [Description("Forces use Tricks Of The Trade in a questing or RaF environment")]
        public bool UseTricksOfTheTradeForce
        {
            get;
            set;
        }

       

        #endregion

        #region Combat

        [Setting]
        [DefaultValue(6)]
        [Category("Combat Spec")]
        [DisplayName("Fan of Knives Add Count")]
        [Description("Will use Fan of Knives when agro mob count is equal to or higher then this value.")]
        public int CombatFanOfKnivesCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Combat Spec")]
        [DisplayName("Use Killing Spree")]
        [Description("Use Killing Spree in our rotation")]
        public bool CombatUseKillingSpree { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Combat Spec")]
        //[DisplayName("Use Expose Armor")]
        //[Description("Use Expose Armor")]
        //public bool UseExposeArmor { get; set; }

        #endregion

        #region Subtlety

        [Setting]
        [DefaultValue(SubtletyRogueRotation.ImprovedTestVersion)]
        [Category("Subtlety Spec")]
        [DisplayName("Rotation Selector")]
        [Description("This rotation was developed by kbrebel04 on the HB forums and is included for consideration and testing. Please report to the CLU thread if you think this rotation should be made default.")]
        public SubtletyRogueRotation SubtletyRogueRotationSelection
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(6)]
        [Category("Subtlety Spec")]
        [DisplayName("Fan of Knives Add Count")]
        [Description("Will use Fan of Knives when agro mob count is equal to or higher then this value.")]
        public int SubtletyFanOfKnivesCount
        {
            get;
            set;
        }

        #endregion

        #region Assassination

        [Setting]
        [DefaultValue(5)]
        [Category("Assasination Spec")]
        [DisplayName("Fan of Knives Add Count")]
        [Description("Will use Fan of Knives when agro mob count is equal to or higher then this value.")]
        public int AssasinationFanOfKnivesCount
        {
            get;
            set;
        }

        #endregion
    }
}