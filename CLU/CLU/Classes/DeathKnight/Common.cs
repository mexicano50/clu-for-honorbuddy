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

using CLU.Helpers;
using Styx.WoWInternals;

namespace CLU.Classes.DeathKnight
{
    using System;
    using System.Linq;

    using Styx;
    using Styx.WoWInternals.WoWObjects;
    using Styx.TreeSharp;
    using global::CLU.Base;
    using global::CLU.Managers;
    using global::CLU.Settings;

    /// <summary>
    /// Common Deathnight Functions.
    /// </summary>
    public static class Common
    {

        /// <summary>
        /// erm..me, us?
        /// </summary>
        private static LocalPlayer Me
        {
            get
            {
                return StyxWoW.Me;
            }
        }


        // Credit to Singular
        internal static int BloodRuneSlotsActive
        {
            get { return StyxWoW.Me.GetRuneCount(0) + StyxWoW.Me.GetRuneCount(1); }
        }

        // Credit to Singular
        internal static int FrostRuneSlotsActive
        {
            get { return StyxWoW.Me.GetRuneCount(2) + StyxWoW.Me.GetRuneCount(3); }
        }
        
        // Credit to Singular
        internal static int UnholyRuneSlotsActive
        {
            get { return StyxWoW.Me.GetRuneCount(4) + StyxWoW.Me.GetRuneCount(5); }
        }

        // Credit to Singular
        internal static int ActiveRuneCount
        {
            get
            {
                return StyxWoW.Me.BloodRuneCount + StyxWoW.Me.FrostRuneCount + StyxWoW.Me.UnholyRuneCount +
                       StyxWoW.Me.DeathRuneCount;
            }
        }

        /// <summary>
        /// Checks if the player is weilding a TwoHanded Weapon
        /// </summary>
        /// <returns>returns true if the player is weilding a TwoHanded Weapon</returns>
        public static bool IsWieldingTwoHandedWeapon()
        {
            try
            {
                switch (Me.Inventory.Equipped.MainHand.ItemInfo.WeaponClass)
                {
                    case WoWItemWeaponClass.ExoticTwoHand:
                    case WoWItemWeaponClass.MaceTwoHand:
                    case WoWItemWeaponClass.AxeTwoHand:
                    case WoWItemWeaponClass.SwordTwoHand:
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                CLULogger.DiagnosticLog("IsWieldingBigWeapon : {0}", ex);
            }

            return false;
        }

        /// <summary>
        /// Checks if we can use Plague Leech
        /// We need to use Plague Leech right before Outbreak drops off so we dont 
        /// waste two globals on Icy Touch and Plague strike.
        /// Of course we are checking we have a rune to refresh!
        /// </summary>
        /// <returns>true if we have Blood Plague and Frost Fever up and Outbreak is about to come off cooldown.</returns>
        private static bool CanPlagueLeech
        {
            get
            {
                try
                {
                    if (!StyxWoW.Me.GotTarget)
                        return false;

                    WoWAura frostFever =
                        StyxWoW.Me.CurrentTarget.GetAllAuras().FirstOrDefault(
                            u => u.CreatorGuid == StyxWoW.Me.Guid && u.Name == "Frost Fever");
                    WoWAura bloodPlague =
                        StyxWoW.Me.CurrentTarget.GetAllAuras().FirstOrDefault(
                            u => u.CreatorGuid == StyxWoW.Me.Guid && u.Name == "Blood Plague");
                    // if there is 3 or less seconds left on the diseases and we have a fully depleted rune then return true.
                    return frostFever != null && frostFever.TimeLeft <= TimeSpan.FromSeconds(3) ||
                           bloodPlague != null && bloodPlague.TimeLeft <= TimeSpan.FromSeconds(3) &&
                           ((BloodRuneSlotsActive == 0 || FrostRuneSlotsActive == 0) && UnholyRuneSlotsActive < 2);
                }
                catch (Exception ex)
                {
                    CLULogger.TroubleshootLog("CanPlagueLeech : {0}", ex);
                }

                return false;
            }
        }

        /// <summary>
        /// Spreads your Diseases depending on what Tier One Talent you have
        /// </summary>
        /// <param name="onUnit">the unit to begin spreading Diseases on</param>
        public static Composite SpreadDiseasesBehavior(CLU.UnitSelection onUnit)
        {
            var pestilenceRange = TalentManager.HasGlyph("Pestilence") ? 14.5 : 10;
            return new PrioritySelector(
                new Decorator(
                    ret => onUnit != null && onUnit(ret) != null && onUnit(ret).DistanceSqr < 40 * 40
                        && CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent != DeathKnightTierOneTalent.None,
                    new PrioritySelector(
                        new Sequence(
                                new Switch<DeathKnightTierOneTalent>(ctx => CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent,
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.PlagueLeech,
                                        new PrioritySelector(
                                        Spell.CastAreaSpell("Pestilence", pestilenceRange, false, CLUSettings.Instance.DeathKnight.BloodPestilenceCount, 0.0, 0.0, ret => Me.Specialization == WoWSpec.DeathKnightUnholy && Buff.TargetHasDebuff("Blood Plague") && Buff.TargetHasDebuff("Frost Fever") && (from enemy in Unit.EnemyMeleeUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") && enemy.Distance2DSqr < pestilenceRange * pestilenceRange select enemy).Any(), "Pestilence"))),
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.UnholyBlight,
                                        Spell.CastAreaSpell("Unholy Blight", 10, false, CLUSettings.Instance.DeathKnight.UnholyBlightCount, 0.0, 0.0, ret => (from enemy in Unit.EnemyMeleeUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Unholy Blight")),
                                    new SwitchArgument<DeathKnightTierOneTalent>(DeathKnightTierOneTalent.RoilingBlood,
                                        Spell.CastAreaSpell("Blood Boil", 10, false, CLUSettings.Instance.DeathKnight.RoilingBloodCount, 0.0, 0.0, ret => Me.Specialization == WoWSpec.DeathKnightUnholy || Me.Specialization == WoWSpec.DeathKnightBlood && (from enemy in Unit.EnemyMeleeUnits where !enemy.HasAura("Blood Plague") && !enemy.HasAura("Frost Fever") select enemy).Any(), "Blood Boil for Roiling Blood")))

                       ))));
        }

        /// <summary>
        /// Applys your Diseases depending on what disease is falling off or utilizes Plague Leech when Outbreak comes off cooldown.
        /// </summary>
        /// <param name="onUnit">the unit to begin spreading Diseases on</param>
        public static Composite ApplyDiseases(CLU.UnitSelection onUnit)
        {
            return
                new Decorator(ret => onUnit != null && onUnit(ret) != null,
                    new PrioritySelector(
                    //Diseases -- The most important stuff for playing a Death Knight is keeping them up at all times
                    Spell.CastSpell("Blood Boil", ret => Buff.PlayerHasActiveBuff("Crimson Scourge"), "Blood Boil (Crimson Scourge)"),
                    Spell.CastSpell("Plague Leech" , ret => CanPlagueLeech, "Plague Leech"), // should be used just as your diseases are about to expire, and each time that you can refresh them right away with Outbreak
                    Spell.CastSpell("Outbreak"     , ret => Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 1 ||Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 1, "Outbreak"),
                    Buff.CastDebuff("Icy Touch",    ret => TalentManager.CurrentSpec != WoWSpec.DeathKnightFrost && Spell.SpellOnCooldown("Outbreak") && Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 1, "Icy Touch for Frost Fever"),
                    Spell.CastSpell("Howling Blast", ret => TalentManager.CurrentSpec == WoWSpec.DeathKnightFrost && Spell.SpellOnCooldown("Outbreak") && Buff.TargetDebuffTimeLeft("Frost Fever").TotalSeconds < 1,"Howling Blast (Frost Fever)"),
                    Spell.CastSpell("Plague Strike", ret => Spell.SpellOnCooldown("Outbreak") && Buff.TargetDebuffTimeLeft("Blood Plague").TotalSeconds < 1, "Plague Strike"))
                    );
        }
    }
}
