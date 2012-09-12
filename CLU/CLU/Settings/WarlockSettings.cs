#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author: exemplar $
// $LastChangedBy: wulf $

#endregion

using System.ComponentModel;
using Styx.Helpers;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;


namespace CLU.Settings
{

    internal class WarlockSettings : Styx.Helpers.Settings
    {
        public WarlockSettings()
        : base(CLUSettings.SettingsPath + "_Warlock.xml")
        {
        }

        #region Common
        #endregion

        #region Affliction
        #endregion

        #region Demonology
        #endregion

        #region Destruction

        [Setting]
        [DefaultValue(true)]
        [Category("Destruction Spec")]
        [DisplayName("Automaticly Apply Bane Of Havoc")]
        [Description("When this is set to true, CLU will always apply Bane Of Havoc to the most appropriate target. If the player has a focus target it will apply bane of havoc on that target otherwise it will choose the closest target that is not crowdcontrolled")] // TODO: Describe how CLU selects an appropriate target.
        public bool ApplyBaneOfHavoc
        {
            get;
            set;
        }

        #endregion

    }
}