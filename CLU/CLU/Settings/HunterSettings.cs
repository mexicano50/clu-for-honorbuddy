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

    internal class HunterSettings : Styx.Helpers.Settings
    {
        public HunterSettings()
        : base(CLUSettings.SettingsPath + "_Hunter.xml")
        {
        }

        #region Pet

        [Setting]
        [DefaultValue((int)PetSlot.FirstSlot)]
        [Category("Pet")]
        [DisplayName("Pet Slot")]
        [Description("CLU will attempt to call the pet in the specified pet slot.")]
        public PetSlot PetSlotSelection
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(70)]
        [Category("Pet")]
        [DisplayName("Mend Pet Percent")]
        [Description("CLU will use Mend Pet at this Pet Health Percent.")]
        public double MendPetPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Pet")]
        [DisplayName("Revive Pet in Combat")]
        [Description("If set to true CLU Will Revive Pet in combat")]
        public bool ReviveInCombat
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Pet")]
        [DisplayName("Use Heart of the Phoenix")]
        [Description("If set to true CLU attempt to bring your pet back to lige usinf Heart of the Phoenix")]
        public bool UseHeartofthePhoenix
        {
            get;
            set;
        }




        #endregion

        #region Common

        [Setting]
        [DefaultValue(false)]
        [Category("Common")]
        [DisplayName("Always Camouflage")]
        [Description("If set to true CLU Will maintain Camouflage always")]
        public bool EnableAlwaysCamouflage
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Common")]
        [DisplayName("Enable Hunter Traps")]
        [Description("If set to true CLU Will cast the appropriate traps")]
        public bool EnableHunterTraps
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(false)]
        [Category("Common")]
        [DisplayName("Misdirection")]
        [Description("If set to true CLU Will use Misdirection with thefollowing target priority: Focus, Pet, RafLeader, Tank")]
        public bool UseMisdirection
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Feign Death")]
        [Description("If set to true CLU Will use Feign Death on agro")]
        public bool UseFeignDeath
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Concussive Shot")]
        [Description("If set to true CLU Will use Concussive Shot")]
        public bool UseConcussiveShot
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Tranquilizing Shot")]
        [Description("If set to true CLU Will use Tranquilizing Shot when your target is enraged")]
        public bool UseTranquilizingShot
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Handle Aspect Switching")]
        [Description("If set to true CLU Will Switch to Apect of the fox while moving then back to aspect of the hawk (or Iron Hawk)")]
        public bool HandleAspectSwitching
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(0)]
        [Category("Common")]
        [DisplayName("Explosive Trap Count")]
        [Description("Will use Explosive Trap when agro mob count is greater than this value. Warning! - Shared setting across all specs")]
        public int ExplosiveTrapCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(1)]
        [Category("Common")]
        [DisplayName("Barrage Count")]
        [Description("Will use Barrage when agro mob count is greater than or equal to this value. Warning! - Shared setting across all specs")]
        public int BarrageCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(1)]
        [Category("Common")]
        [DisplayName("Powershot Count")]
        [Description("Will use Powershot when agro mob count is greater than or equal to this value. Warning! - Shared setting across all specs")]
        public int PowershotCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(1)]
        [Category("Common")]
        [DisplayName("Glaive Toss Count")]
        [Description("Will use Glaive Toss when agro mob count is greater than or equal to this value. Warning! - Shared setting across all specs")]
        public int GlaiveTossCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(50)]
        [Category("Common")]
        [DisplayName("Exhilaration Percent")]
        [Description("Will use Exhilaration at the set Health Percent")]
        public int ExhilarationPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(40)]
        [Category("Common")]
        [DisplayName("Healthstone Percent")]
        [Description("Will use Healthstone at the set Health Percent")]
        public int HealthstonePercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(50)]
        [Category("Common")]
        [DisplayName("Deterrence Percent")]
        [Description("Will use Deterrence at the set Health Percent")]
        public int DeterrencePercent
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Common")]
        //[DisplayName("Use Disengage")]
        //[Description("Will be used in battlegrounds no matter what this is set")]
        //public bool UseDisengage { get; set; }

        #endregion

        #region Marksmanship

        [Setting]
        [DefaultValue(4)]
        [Category("Marksmanship")]
        [DisplayName("Multi Shot Count")]
        [Description("Will use Multi Shot when agro mob count is equal to or higher then this value.")]
        public int MarksMultiShotCount
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(66)]
        [Category("Marksmanship")]
        [DisplayName("Arcane Shot FocusPercent")]
        [Description("Will use Arcane Shot at the set FocusPercent")]
        public int MarksArcaneShotFocusPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(50)]
        [Category("Marksmanship")]
        [DisplayName("Fevor FocusPercent")]
        [Description("Will use Fevor at the set FocusPercent")]
        public int MarksFevorFocusPercent
        {
            get;
            set;
        }

        #endregion

        #region BeastMastery

        [Setting]
        [DefaultValue(65)]
        [Category("BeastMastery")]
        [DisplayName("Fevor FocusPercent")]
        [Description("Will use Fevor at the set FocusPercent")]
        public int BmFevorFocusPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("BeastMastery")]
        [DisplayName("Bestial Wrath FocusPercent")]
        [Description("Will use Bestial Wrath at the set FocusPercent")]
        public int BestialWrathFocusPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(59)]
        [Category("BeastMastery")]
        [DisplayName("Arcane Shot FocusPercent")]
        [Description("Will use Arcane Shot at the set FocusPercent")]
        public int BmArcaneShotFocusPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(4)]
        [Category("BeastMastery")]
        [DisplayName("Multi Shot Count")]
        [Description("Will use Multi Shot when agro mob count is equal to or higher then this value.")]
        public int BmMultiShotCount
        {
            get;
            set;
        }


        #endregion

        #region Survival

        [Setting]
        [DefaultValue(67)]
        [Category("Survival")]
        [DisplayName("Arcane Shot FocusPercent")]
        [Description("Will use Arcane Shot at the set FocusPercent")]
        public int SurArcaneShotFocusPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(50)]
        [Category("Survival")]
        [DisplayName("Fevor FocusPercent")]
        [Description("Will use Fevor at the set FocusPercent")]
        public int SurFevorFocusPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(2)]
        [Category("Survival")]
        [DisplayName("Multi Shot Count")]
        [Description("Will use Multi Shot when agro mob count is equal to or higher then this value.")]
        public int SurvMultiShotCount
        {
            get;
            set;
        }

        #endregion
    }
}