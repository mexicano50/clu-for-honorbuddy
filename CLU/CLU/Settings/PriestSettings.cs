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
    internal class PriestSettings : Styx.Helpers.Settings
    {
        public PriestSettings()
        : base(CLUSettings.SettingsPath + "_Priest.xml")
        {
        }

        #region Common

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Fade")]
        [Description("When this is set to true, CLU will automaticly fade when ThreatInfo.RawPercent > 90 during combat")]
        public bool UseFade
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Use Shadow Protection")]
        [Description("Use Shadow Protection buff")]
        public bool UseShadowProtection
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Use Power Word: Fortitude")]
        [Description("Use Power Word: Fortitude buff")]
        public bool UsePowerWordFortitude
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(true)]
        [Category("Common")]
        [DisplayName("Use Inner Fire")]
        [Description("Use Inner Fire")]
        public bool UseInnerFire
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(40)]
        [Category("Common")]
        [DisplayName("Healthstone HP")]
        [Description("Healthstone will be consumed at this Healthpercent")]
        public int UseHealthstone
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Use Psychic Scream")]
        //[Description("Use Psychic Scream")]
        //public bool UsePsychicScream { get; set; }

        //[Setting]
        //[DefaultValue(2)]
        //[Category("Common")]
        //[DisplayName("Use Psychic Scream Count")]
        //[Description("Uses Psychic Scream when there's >= these number of adds (not including current target)")]
        //public int PsychicScreamAddCount { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Use Fear Ward")]
        //[Description("Use Fear Ward buff")]
        //public bool UseFearWard { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Common")]
        //[DisplayName("Use Wand")]
        //[Description("Uses wand if we're oom")]
        //public bool UseWand { get; set; }

        #endregion

        #region Shadow

        [Setting]
        [DefaultValue(0.6)]
        [Category("Shadow Spec")]
        [DisplayName("Mind Flay Clipping")]
        [Description("Will attempt to clip mind flay at this time value.")]
        public double MindFlayClippingDuration
        {
            get;
            set;
        }

        

        [Setting]
        [DefaultValue(ShadowPriestRotation.Default)]
        [Category("Shadow Spec")]
        [DisplayName("Rotation Selector")]
        [Description("Choose MindSpike rotation if you have at least 2x T13")]
        public ShadowPriestRotation SpriestRotationSelection
        {
            get;
            set;
        }


        [Setting]
        [DefaultValue(70)]
        [Category("Shadow Spec")]
        [DisplayName("Flash Heal Health Percent")]
        [Description("Health for Flash Heal for shadow spec")]
        public double ShadowFlashHealHealth
        {
            get;
            set;
        }

        //[Setting]
        //[DefaultValue(50)]
        //[Category("Shadow")]
        //[DisplayName("Mind Flay Mana")]
        //[Description("Will only use Mind Flay while manapercent is above this value")]
        //public double MindFlayMana { get; set; }

        [Setting]
        [DefaultValue(30)]
        [Category("Shadow Spec")]
        [DisplayName("PW: Shield Health Percent")]
        [Description("Use PW:Shield on set HP.")]
        public double ShieldHealthPercent
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(60)]
        [Category("Shadow Spec")]
        [DisplayName("Renew Health Percent")]
        [Description("Use Renew on set HP.")]
        public double RenewHealthPercent
        {
            get;
            set;
        }

        

        //[Setting]
        //[DefaultValue(15)]
        //[Category("Shadow")]
        //[DisplayName("Mind Blast Timer")]
        //[Description("Casts mind blast anyway after this many seconds if we haven't got 3 shadow orbs")]
        //public int MindBlastTimer { get; set; }

        //[Setting]
        //[DefaultValue(2)]
        //[Category("Shadow")]
        //[DisplayName("Mind Blast Orbs")]
        //[Description("Casts mind blast once we get (at least) this many shadow orbs")]
        //public int MindBlastOrbs { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Shadow")]
        //[DisplayName("Devouring Plague First")]
        //[Description("Casts devouring plague before anything else, useful for farming low hp mobs")]
        //public bool DevouringPlagueFirst { get; set; }

        //[Setting]
        //[DefaultValue(false)]
        //[Category("Shadow")]
        //[DisplayName("Archangel on 5")]
        //[Description("Always archangel on 5 dark evangelism, ignoring mana %")]
        //public bool AlwaysArchangel5 { get; set; }

        //[Setting]
        //[DefaultValue(20)]
        //[Category("Shadow")]
        //[DisplayName("Dispersion Mana")]
        //[Description("Dispersion will be used when mana percentage is less than this value")]
        //public int DispersionMana { get; set; }

        //[Setting]
        //[DefaultValue(40)]
        //[Category("Shadow")]
        //[DisplayName("Healing Spells Health")]
        //[Description("Won't attempt to use healing spells unless below this health percent")]
        //public int DontHealPercent { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Shadow")]
        //[DisplayName("No Shadowform Below Heal")]
        //[Description("Won't attempt to re-enter shadowform while below healing spells health threshold")]
        //public bool DontShadowFormHealth { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Shadow")]
        //[DisplayName("Psychic Horror Adds")]
        //[Description("Attempt to psychic horror adds")]
        //public bool UsePsychicHorrorAdds { get; set; }

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Shadow")]
        //[DisplayName("Psychic Horror Interrupt")]
        //[Description("Attempt to psychic horror target as interrupt (on top of silence)")]
        //public bool UsePsychicHorrorInterrupt { get; set; }

        #endregion

        #region Emergency heals on me [Discipline]

        [Setting]
        [DefaultValue(40)]
        [Category("Discipline - Emergency heals on me")]
        [DisplayName("Desperate Prayer")]
        [Description("Desperate Prayer will be used on me at this value in an emergency")]
        public int DesperatePrayerEHOM
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(40)]
        [Category("Discipline - Emergency heals on me")]
        [DisplayName("Flash Heal")]
        [Description("Flash Heal will be used on me at this value in an emergency")]
        public int FlashHealEHOM
        {
            get;
            set;
        }

        #endregion

        #region Regain some mana [Discipline]

        [Setting]
        [DefaultValue(20)]
        [Category("Common - Mana Regen")]
        [DisplayName("Shadowfiend Mana")]
        [Description("Shadowfiend will be used at this value (UseCooldowns needs to be enabled)")]
        public int ShadowfiendMana
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(80)]
        [Category("Common - Mana Regen")]
        [DisplayName("Shadowfiend Mana with Haste")]
        [Description("Shadowfiend will be used at this value when heroism or bloodlust,etc is active and we are in Raid (UseCooldowns needs to be enabled)")]
        public int ShadowfiendManaHasted
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(20)]
        [Category("Common - Mana Regen")]
        [DisplayName("Hymn of Hope Mana")]
        [Description("Hymn of Hope will be used at this value (UseCooldowns needs to be enabled)")]
        public int HymnofHopeMana
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(80)]
        [Category("Common - Mana Regen")]
        [DisplayName("Hymn of Hope Mana with Haste")]
        [Description("Hymn of Hope will be used at this value when heroism or bloodlust,etc is active and we are in Raid (UseCooldowns needs to be enabled)")]
        public int HymnofHopeManaHasted
        {
            get;
            set;
        }


        #endregion


        #region AA-Spec [Discipline]

        [Setting]
        [DefaultValue(4)]
        [Category("Discipline (AA-Spec)")]
        [DisplayName("Evangelism Stack Count")]
        [Description("When Evangelism Stack Count >= to specified number Archangel will be used (UseCooldowns needs to be enabled)")]
        public int EvangelismStackCount
        {
            get;
            set;
        }

        #endregion

        #region emergency heals on most injured tank [Discipline]

        [Setting]
        [DefaultValue(50)]
        [Category("Discipline - Emergency heals on most injured tank")]
        [DisplayName("FindTank with HealthPercent less than")]
        [Description("This includes Flash Heal, Power Word: Shield, Penance, Pain Suppression")]
        public int EmergencyhealsonmostinjuredtankHealthPercent
        {
            get;
            set;
        }


        #endregion

        #region emergency heals on most injured raid/party member [Discipline]

        [Setting]
        [DefaultValue(50)]
        [Category("Discipline - Emergency heals on most injured raid/party member")]
        [DisplayName("FindRaidMember with HealthPercent less than")]
        [Description("This includes Flash Heal, Power Word: Shield, Penance")]
        public int emergencyhealsonmostinjuredraidpartymemberHealthPercent
        {
            get;
            set;
        }

        #endregion

        #region maintanable abilities [Discipline]

        //[Setting]
        //[DefaultValue(true)]
        //[Category("Discipline (R&P) - maintainable abilities raid/party member")]
        //[DisplayName("Prayer of Mending on tank")]
        //[Description("Maintain this abilitie (Ensure it is up all the time)")]
        //public int PrayerofMendingontank { get; set; }

        #endregion

        #region Party healing AoE [Discipline]

        [Setting]
        [DefaultValue(10)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Prayer of Healing - minAverageHealth")]
        [Description("the minumum average health to consider using Prayer of Healing (Calculated as p.CurrentHealth * 100 / p.MaxHealth)")]
        public int PrayerofHealingPartyminAH
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(93)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Prayer of Healing - maxAverageHealth")]
        [Description("the maximum average health to consider using Prayer of Healing (Calculated as p.CurrentHealth * 100 / p.MaxHealth)")]
        public int PrayerofHealingPartymaxAH
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Prayer of Healing - maxDistanceBetweenPlayers")]
        [Description("the maximum Distance Between Players to consider using Prayer of Healing (clustered players)")]
        public int PrayerofHealingPartymaxDBP
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(3)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Prayer of Healing - minPlayers")]
        [Description("the minumum Players to consider using Prayer of Healing (#clusterd targets - 1] eg: a value of three will be 2 players within the maximum distance between players)")]
        public int PrayerofHealingPartyminPlayers
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(10)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Divine Hymn - minAverageHealth")]
        [Description("the minumum average health to consider using Divine Hymn (Calculated as p.CurrentHealth * 100 / p.MaxHealth)")]
        public int DivineHymnPartyminAH
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(70)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Divine Hymn - maxAverageHealth")]
        [Description("the maximum average health to consider using Divine Hymn (Calculated as p.CurrentHealth * 100 / p.MaxHealth)")]
        public int DivineHymnPartymaxAH
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Divine Hymn - maxDistanceBetweenPlayers")]
        [Description("the maximum Distance Between Players to consider using Divine Hymn (clustered players)")]
        public int DivineHymnPartymaxDBP
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(4)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Divine Hymn - minPlayers")]
        [Description("the minumum Players to consider using Divine Hymn ([#clusterd targets - 1] eg: a value of three will be 2 players within the maximum distance between players)")]
        public int DivineHymnPartyminPlayers
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(10)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Power Word: Barrier - minAverageHealth")]
        [Description("the minumum average health to consider using Power Word: Barrier (Calculated as p.CurrentHealth * 100 / p.MaxHealth)")]
        public int PowerWordBarrierPartyminAH
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(70)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Power Word: Barrier - maxAverageHealth")]
        [Description("the maximum average health to consider using Power Word: Barrier (Calculated as p.CurrentHealth * 100 / p.MaxHealth)")]
        public int PowerWordBarrierPartymaxAH
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(30)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Power Word: Barrier - maxDistanceBetweenPlayers")]
        [Description("the maximum Distance Between Players to consider using Power Word: Barrier (clustered players)")]
        public int PowerWordBarrierPartymaxDBP
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(4)]
        [Category("Discipline - AoE Healing Abilities [Party]")]
        [DisplayName("Power Word: Barrier - minPlayers")]
        [Description("the minumum Players to consider using Power Word: Barrier ([#clusterd targets - 1] eg: a value of three will be 2 players within the maximum distance between players)")]
        public int PowerWordBarrierPartyminPlayers
        {
            get;
            set;
        }

        #endregion

        #region Party healing Single Target [Discipline]

        [Setting]
        [DefaultValue(70)]
        [Category("Discipline - Single Target Healing [Party]")]
        [DisplayName("Greater Heal (Inner Focus) - HealthPercent")]
        [Description("Greater Heal will be used with Inner Focus at this Healthpercent")]
        public int GreaterHealInnerFocusParty
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(70)]
        [Category("Discipline - Single Target Healing [Party]")]
        [DisplayName("Greater Heal - HealthPercent")]
        [Description("Greater Heal will be used at this Healthpercent")]
        public int GreaterHealParty
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(92)]
        [Category("Discipline - Single Target Healing [Party]")]
        [DisplayName("Single Target Healing - HealthPercent")]
        [Description("CLU will Single Target Heal at this Healthpercent")]
        public int SingleTargethealingParty
        {
            get;
            set;
        }

        #endregion

        #region Party healing cast heals while moving [Discipline]

        [Setting]
        [DefaultValue(90)]
        [Category("Discipline - Cast heals while moving [Party]")]
        [DisplayName("Power Word: Shield - HealthPercent")]
        [Description("Power Word: Shield will be used at this Healthpercent while moving")]
        public int PowerWordShieldMovingParty
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(95)]
        [Category("Discipline - Cast heals while moving [Party]")]
        [DisplayName("Renew - HealthPercent")]
        [Description("Renew will be used at this Healthpercent while moving")]
        public int RenewMovingParty
        {
            get;
            set;
        }

        #endregion

        #region Holy

        [Setting]
        [DefaultValue(true)]
        [Category("Holy Spec")]
        [DisplayName("Place Lightwell")]
        [Description("When this is set to true, CLU will automaticly place a Lightwell on the ground during combat")]
        public bool PlaceLightwell
        {
            get;
            set;
        }

        [Setting]
        [DefaultValue(ChakraStance.Serenity)]
        [Category("Holy Spec")]
        [DisplayName("Chakra Stance")]
        [Description("Choose a Chakra Stance (Default is Serenity)")]
        public ChakraStance ChakraStanceSelection
        {
            get;    // was HandleChakra
            set;
        }

        //[Setting]
        //[DefaultValue(95)]
        //[Category("Holy")]
        //[DisplayName("Heal Health")]
        //[Description("Heal will be used at this value")]
        //public int HolyHeal { get; set; }

        //[Setting]
        //[DefaultValue(50)]
        //[Category("Holy")]
        //[DisplayName("Greater Heal Health")]
        //[Description("Greater Heal will be used at this value")]
        //public int HolyGreaterHeal { get; set; }

        //[Setting]
        //[DefaultValue(25)]
        //[Category("Holy")]
        //[DisplayName("Flash Heal Health")]
        //[Description("Flash Heal will be used at this value")]
        //public int HolyFlashHeal { get; set; }

        //[Setting]
        //[DefaultValue(40)]
        //[Category("Holy")]
        //[DisplayName("Divine Hymn Health")]
        //[Description("Divine Hymn will be used at this value")]
        //public int DivineHymnHealth { get; set; }

        //[Setting]
        //[DefaultValue(6)]
        //[Category("Holy")]
        //[DisplayName("Divine Hymn Count")]
        //[Description("Divine Hymn will be used when this many heal targets below the Divine Hymn Health percent")]
        //public int DivineHymnCount { get; set; }

        //[Setting]
        //[DefaultValue(80)]
        //[Category("Holy")]
        //[DisplayName("Prayer of Healing with Serendipity Health")]
        //[Description("Prayer of Healing with Serendipity will be used at this value")]
        //public int PrayerOfHealingSerendipityHealth { get; set; }

        //[Setting]
        //[DefaultValue(4)]
        //[Category("Holy")]
        //[DisplayName("Prayer of Healing with Serendipity Count")]
        //[Description("Prayer of Healing with Serendipity will be used when this many heal targets below the Prayer of Healing with Serendipity Health percent")]
        //public int PrayerOfHealingSerendipityCount { get; set; }

        //[Setting]
        //[DefaultValue(95)]
        //[Category("Holy")]
        //[DisplayName("Circle of Healing Health")]
        //[Description("Circle of Healing will be used at this value")]
        //public int CircleOfHealingHealth { get; set; }

        //[Setting]
        //[DefaultValue(5)]
        //[Category("Holy")]
        //[DisplayName("Circle of Healing Count")]
        //[Description("Circle of Healing will be used when this many heal targets below the Circle of Healing Health percent")]
        //public int CircleOfHealingCount { get; set; }

        #endregion
    }
}