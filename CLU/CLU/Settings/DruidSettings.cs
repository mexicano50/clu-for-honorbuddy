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
using System.IO;
using System.ComponentModel;
using Styx.Helpers;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;


namespace CLU.Settings
{

    internal class DruidSettings : Styx.Helpers.Settings
    {
        public DruidSettings()
            : base(Path.Combine(CLUSettings.SettingsPath, "Druid.xml"))
        {
        }

        #region Common

        //[Setting]
        //[DefaultValue(40)]
        //[Category("Common")]
        //[DisplayName("Innervate Mana")]
        //[Description("Innervate will be used when your mana drops below this value")]
        //public int InnervateMana { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Common")]
        //[DisplayName("Disable Healing for Balance and Feral")]
        //public bool NoHealBalanceAndFeral { get; set; }

        //[Setting]
        //[DefaultValue(20)]
        //[Category("Common")]
        //[DisplayName("Healing Touch Health (Balance and Feral)")]
        //[Description("Healing Touch will be used at this value.")]
        //public int NonRestoHealingTouch { get; set; }

        //[Setting]
        //[DefaultValue(40)]
        //[Category("Common")]
        //[DisplayName("Rejuvenation Health (Balance and Feral)")]
        //[Description("Rejuvenation will be used at this value")]
        //public int NonRestoRejuvenation { get; set; }

        //[Setting]
        //[DefaultValue(40)]
        //[Category("Common")]
        //[DisplayName("Regrowth Health (Balance and Feral)")]
        //[Description("Regrowth will be used at this value")]
        //public int NonRestoRegrowth { get; set; }

        #endregion

        #region Balance

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Balance")]
        //[DisplayName("Starfall")]
        //[Description("Use Starfall.")]
        //public bool UseStarfall { get; set; }

        #endregion

        #region Resto

        [Setting]
        [DefaultValue(true)]
        [Category("Restoration")]
        [DisplayName("Automaticly apply LifeBloom")]
        [Description("When this is set to true, CLU will automaticly select the appropriate target for LifeBloom")]
        public bool HandleLifeBloomTarget
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(60)]
        //[Category("Restoration")]
        //[DisplayName("Tranquility Health")]
        //[Description("Tranquility will be used at this value")]
        //public int TranquilityHealth { get; set; }

        //[Setting]
        //[DefaultValue(3)]
        //[Category("Restoration")]
        //[DisplayName("Tranquility Count")]
        //[Description("Tranquility will be used when count of party members whom health is below Tranquility health mets this value ")]
        //public int TranquilityCount { get; set; }

        //[Setting]
        //[DefaultValue(65)]
        //[Category("Restoration")]
        //[DisplayName("Swiftmend Health")]
        //[Description("Swiftmend will be used at this value")]
        //public int Swiftmend { get; set; }

        //[Setting]
        //[DefaultValue(80)]
        //[Category("Restoration")]
        //[DisplayName("Wild Growth Health")]
        //[Description("Wild Growth will be used at this value")]
        //public int WildGrowthHealth { get; set; }

        //[Setting]
        //[DefaultValue(2)]
        //[Category("Restoration")]
        //[DisplayName("Wild Growth Count")]
        //[Description("Wild Growth will be used when count of party members whom health is below Wild Growth health mets this value ")]
        //public int WildGrowthCount { get; set; }

        //[Setting]
        //[DefaultValue(70)]
        //[Category("Restoration")]
        //[DisplayName("Regrowth Health")]
        //[Description("Regrowth will be used at this value")]
        //public int Regrowth { get; set; }

        //[Setting]
        //[DefaultValue(60)]
        //[Category("Restoration")]
        //[DisplayName("Healing Touch Health")]
        //[Description("Healing Touch will be used at this value")]
        //public int HealingTouch { get; set; }

        //[Setting]
        //[DefaultValue(75)]
        //[Category("Restoration")]
        //[DisplayName("Nourish Health")]
        //[Description("Nourish will be used at this value")]
        //public int Nourish { get; set; }

        //[Setting]
        //[DefaultValue(90)]
        //[Category("Restoration")]
        //[DisplayName("Rejuvenation Health")]
        //[Description("Rejuvenation will be used at this value")]
        //public int Rejuvenation { get; set; }

        //[Setting]
        //[DefaultValue(80)]
        //[Category("Restoration")]
        //[DisplayName("Tree of Life Health")]
        //[Description("Tree of Life will be used at this value")]
        //public int TreeOfLifeHealth { get; set; }

        //[Setting]
        //[DefaultValue(3)]
        //[Category("Restoration")]
        //[DisplayName("Tree of Life Count")]
        //[Description("Tree of Life will be used when count of party members whom health is below Tree of Life health mets this value ")]
        //public int TreeOfLifeCount { get; set; }

        //[Setting]
        //[DefaultValue(70)]
        //[Category("Restoration")]
        //[DisplayName("Barkskin Health")]
        //[Description("Barkskin will be used at this value")]
        //public int Barkskin { get; set; }

        #endregion

        #region Feral

        //[Setting]
        //[DefaultValue(50)]
        //[Category("Feral")]
        //[DisplayName("Barkskin Health")]
        //[Description("Barkskin will be used at this value. Set this to 100 to enable on cooldown usage.")]
        //public int FeralBarkskin { get; set; }

        //[Setting]
        //[DefaultValue(55)]
        //[Category("Feral")]
        //[DisplayName("Survival Instincts Health")]
        //[Description("SI will be used at this value. Set this to 100 to enable on cooldown usage. (Recommended: 55)")]
        //public int SurvivalInstinctsHealth { get; set; }

        //[Setting]
        //[DefaultValue(30)]
        //[Category("Feral PvP")]
        //[DisplayName("Frenzied Regeneration Health")]
        //[Description("FR will be used at this value. Set this to 100 to enable on cooldown usage. (Recommended: 30 if glyphed. 15 if not.)")]
        //public int FrenziedRegenerationHealth { get; set; }

        //[Setting]
        //[DefaultValue(0)]
        //[Category("Form Selection")]
        //[DisplayName("Form Selection")]
        //[Description("Form Selection!")]
        //public int Shapeform { get; set; }

        //[Setting]
        //[DefaultValue(60)]
        //[Category("Feral")]
        //[DisplayName("Predator's Swiftness heal")]
        //[Description("Healing with Predator's Swiftness will be used at this value")]
        //public int NonRestoprocc { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Disable Healing for Balance and Feral")]
        //public bool RaidCatProwl { get; set; }

        //[Setting]
        //[DefaultValue(15)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Predator's Swiftness (Balance and Feral)")]
        //public int RaidCatProccHeal { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Interrupt")]
        //[Description("Automatically interrupt spells while in an instance if this value is set to true.")]
        //public bool Interrupt { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Buff raid with Motw")]
        //[Description("If set to true, we will buff the raid automatically.")]
        //public bool BuffRaidWithMotw { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Warn if not behind boss")]
        //public bool CatRaidWarning { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Use dash as gap closer")]
        //public bool CatRaidDash { get; set; }


        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Use Stampeding Roar")]
        //[Description("If set to true, it will cast Stampeding Roar to close gap to target.")]
        //public bool CatRaidStampeding { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral PvP")]
        //[DisplayName("Cat - Stealth Pull")]
        //[Description("Always try to pull while in stealth. If disabled it pulls with FFF instead.")]
        //public bool CatNormalPullStealth { get; set; }

        //[Setting]
        //[DefaultValue(4)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Adds to AOE")]
        //[Description("Number of adds needed to start Aoe rotation.")]
        //public int CatRaidAoe { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Auto Berserk")]
        //[Description("If set to true, it will cast Berserk automatically to do max dps.")]
        //public bool CatRaidBerserk { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Auto Tiger's Fury")]
        //[Description("If set to true, it will cast Tiger's Fury automatically to do max dps.")]
        //public bool CatRaidTigers { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Rebuff infight")]
        //[Description("If set to true, it will rebuff Mark of the Wild infight.")]
        //public bool CatRaidRebuff { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Rez infight")]
        //[Description("If set to true, it will rez while infight.")]
        //public bool CatRaidRezz { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Cat - Feral Charge")]
        //[Description("Use Feral Charge to close gaps. It should handle bosses where charge is not" +
        //             "possible || best solution automatically.")]
        //public bool CatRaidUseFeralCharge { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Bear - Feral Charge")]
        //[Description("Use Feral Charge to close gaps. It should handle bosses where charge is not" +
        //             "possible || best solution automatically.")]
        //public bool BearRaidUseFeralCharge { get; set; }

        //[Setting]
        //[DefaultValue(2)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Bear - Adds to AOE")]
        //[Description("Number of adds needed to start Aoe rotation.")]
        //public int BearRaidAoe { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Auto Berserk")]
        //[Description("If set to true, it will cast Berserk automatically to do max threat.")]
        //public bool BearRaidBerserk { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Bear - Berserk Burst")]
        //[Description("If set to true, it will SPAM MANGLE FOR GODS SAKE while Berserk is active.")]
        //public bool BearRaidBerserkFun { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Feral Raid / Instance")]
        //[DisplayName("Bear - Auto defensive cooldowns")]
        //[Description("If set to true, it will cast defensive cooldowns automatically.")]
        //public bool BearRaidCooldown { get; set; }

        #endregion
    }
}