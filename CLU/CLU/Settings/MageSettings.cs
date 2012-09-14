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

namespace CLU.Settings
{
    internal class MageSettings : Styx.Helpers.Settings
    {
        public MageSettings()
        : base(CLUSettings.SettingsPath + "_Mage.xml")
        {

        }

        #region Common

        //[SettingAttribute]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Summon Table If In A Party")]
        //[Description("Summons a food table instead of using conjured food if in a party")]
        //public bool SummonTableIfInParty { get; set; }

        #endregion

        #region Frost
        #endregion

        #region Fire

        [Setting]
        [System.ComponentModel.DefaultValue(false)]
        [Category("Fire")]
        [DisplayName("Use Combustion")]
        [Description("Enables the usage of Combustion (not recommended)")]
        public bool EnableCombustion { get; set; }
        #endregion

        #region Arcane
        #endregion
    }
}