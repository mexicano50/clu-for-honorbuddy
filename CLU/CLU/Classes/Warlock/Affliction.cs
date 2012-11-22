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

using System.Linq;
using CommonBehaviors.Actions;
using Styx;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using CLU.Helpers;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Warlock
{
    class Affliction : RotationBase
    {
        public override string Name
        {
            get {
                return "Affliction Warlock";
            }
        }

        public override string Revision
        {
            get
            {
                return "$Revision$";
            }
        }

        public override string KeySpell
        {
            get {
                return "Unstable Affliction";
            }
        }
        public override int KeySpellId
        {
            get { return 30108; }
        }
        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "to be done\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 40f;
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),
                    // START Interupts & Spell casts
                    new Decorator(
                        ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                            Item.UseEngineerGloves())),
                    // Threat
                    Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "Soulshatter"),                            
                    Buff.CastBuff("Dark Intent", ret => true, "Dark Intent"),
                    //Call Pet
                    new Decorator(ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Soulburn")) && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503),
                        new Sequence(
                            Buff.CastBuff("Soulburn", ret => !Me.ActiveAuras.ContainsKey("Soulburn"), "Soulburn for Pet"),
                            new WaitContinue(new System.TimeSpan(0, 0, 0, 0, 50), ret => false, new ActionAlwaysSucceed()),
                            Spell.CreateWaitForLagDuration(),
                            PetManager.CastPetSummonSpell(691, ret => true, "Summon Pet"))),
                    Common.WarlockGrimoire,
                    HandleCommon(),
                    HandleDefensiveCooldowns(), // Stay Alive!                       
                    new Decorator(ret => Me.CurrentTarget != null && Unit.NearbyAttackableUnits(Me.CurrentTarget.Location, 25).Count() > 2 && CLUSettings.Instance.UseAoEAbilities, HandleAoeCombat()), //x => !x.IsBoss()
                    HandleSingleTarget()
                    );
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    Movement.CreateFaceTargetBehavior(),
                    this.SingleRotation);
            }
        }

        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                        HandleOffensiveCooldowns(),                                    // Utility abilitys...
                        new Decorator(ret => Me.HealthPercent < 100,
                            new PrioritySelector(
                                HandleDefensiveCooldowns())));
            }
        }

        public override Composite PreCombat
        {
            get { return Common.WarlockPreCombat; }
        }

        public override Composite Resting
        {
            get {return Rest.CreateDefaultRestBehaviour();}
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation;}
        }

        public override Composite PVERotation
        {
            get {return this.SingleRotation;}
        }

        private static Composite HandleDefensiveCooldowns()
        {
            return new PrioritySelector(
                Spell.CastSelfSpell("Unending Resolve",  ret => Me.HealthPercent < 50,"[HandleDefensiveCooldowns] Unending Resolve"),
                Spell.CastSelfSpell("Twilight Ward", ret => NeedTwilightWard, "[HandleDefensiveCooldowns] Twilight Ward"));
        }

        private static Composite HandleOffensiveCooldowns()
        {
            return new PrioritySelector(
                Spell.CastSelfSpell("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.IsChanneling, "[HandleOffensiveCooldowns] Soulshatter"));
        }

        private static Composite HandleCommon()
        {
            return new PrioritySelector(
                Spell.CastSelfSpell("Dark Soul: Misery", ret => true, "[HandleCommon] Dark Soul"),
                Common.HandleGrimoire);
        }

        private static Composite HandleSingleTarget()
        {
            return new PrioritySelector(
                Spell.PreventDoubleCast("Curse of the Elements", 1, ret => NeedCurseOfElements),
                HandleStopChannelToCurse(),
                Spell.PreventDoubleCast("Summon Doomguard", 1, ret => NeedToSummonDoomgaurd),
                HandleSoulSwap(),
                Spell.PreventDoubleCast("Haunt", CurseRefreshTotalSeconds(1, Traveltime(20.0), Spell.GetSpellCastTime("Haunt")), ret => NeedHaunt),
                Spell.PreventDoubleCast("Soulburn", 1.5, ret => NeedSoulBurn),
                Spell.PreventDoubleCast("Agony", ThrottleTime, ret => NeedAgony),
                Spell.CastSpell("Corruption", ret => NeedCorruption,"Corruption"),
                Spell.PreventDoubleCast("Unstable Affliction", CurseRefreshTotalSeconds(1, Traveltime(0.0), Spell.GetSpellCastTime("Unstable Affliction")), ret => NeedUnstableAffliction),
                Spell.PreventDoubleChannel("Drain Soul", ThrottleTime, true, on => Me.CurrentTarget, ret => NeedDrainSoul),
                Spell.PreventDoubleCast("Life Tap", 1, on => Me, ret => Me.ManaPercent < 35 && Me.HealthPercent > 40),
                Spell.PreventDoubleChannel("Malefic Grasp", ThrottleTime, true, on => Me.CurrentTarget, ret => !Me.IsMoving && !NeedaCurse),
                Spell.PreventDoubleCast("Life Tap", ThrottleTime, on => Me, ret => Me.CurrentTarget != null && (Me.IsMoving && Me.ManaPercent < 80 && Me.ManaPercent < Me.CurrentTarget.HealthPercent)),
                Spell.CastSpell("Fel Flame", ret => Me.IsMoving,"Fel Flame")); // Our Filler.
        }

        private static Composite HandleAoeCombat()
        {
            return new PrioritySelector(
                    new Decorator(a => CurrentSoulShards > 1 && Me.ChanneledCastingSpellId == 1120, new Action(delegate { SpellManager.StopCasting(); return RunStatus.Failure; })),
                    HandleSoulSwapAoE(), // this is king...
                    HandleSeedofCorruptionAoE(), // this can be used manually....but...
                    HandleMultiDoT(Debuffs), // this is debatable with the use of soulswap...
                    Spell.PreventDoubleChannel("Drain Soul", 0.5, true, onUnit => Me.CurrentTarget, ret => true));
        }

        private static Composite HandleStopChannelToCurse(int spellId, bool reason) // <-- no idea why HandleStopChannelToCurse(103103, NeedaCurse) dosnt work...:/
        {
            return new Decorator(a => reason && Me.ChanneledCastingSpellId == spellId, new Action(delegate { SpellManager.StopCasting(); return RunStatus.Failure; }));
        }

        private static Composite HandleStopChannelToCurse()
        {
            return new Decorator(a => NeedaCurse && Me.ChanneledCastingSpellId == 103103, new Action(delegate { SpellManager.StopCasting(); return RunStatus.Failure; }));
        }

        private static Composite HandleSoulSwap()
        {
            return new Decorator(a => Me.ActiveAuras.ContainsKey("Soulburn") && ((NeedSoulSwap && !NeedHaunt) || Buff.GetAuraDoubleTimeLeft(Me,"Soulburn", true) < 4), new Action(a => SpellManager.Cast(119678)));
        }

        private static double ThrottleTime { get { return 0.5; } }

        #region Curse Calculations

        // buffer (start casting when it hits this many seconds)
        // traveltime (1.5 seconds default..but haunt has a longer time..see Traveltime(double travelSpeed).)
        // casttime (provide the casttime of the spell)
        // Latency?? hrm.. see Clientlag

        private static readonly double Clientlag = StyxWoW.WoWClient.Latency * 2 / 1000.0;

        private static double CurseRefreshTotalSeconds(double buffer, double traveltime, double casttime)
        {
            {
                //Logger.DebugLog("CurseRefreshTotalSeconds: Buffer: {0} + traveltime: {1} + casttime: {2} + Clientlag: {3}", buffer, traveltime, casttime, Clientlag);
                return buffer + traveltime + casttime + Clientlag;
            }
        }

        private static double Traveltime(double travelSpeed)
        {
            {
                if (travelSpeed < 1) return 0;

                if (Me.CurrentTarget != null && Me.CurrentTarget.Distance < 1) return 0;

                if (Me.CurrentTarget != null)
                {
                    double t = Me.CurrentTarget.Distance / travelSpeed;
                    CLULogger.DiagnosticLog("Travel time: {0}", t);
                    return t;
                }

                return 0;
            }
        }

        private static bool NeedaCurse
        {
            get
            {
                return (NeedHaunt && CurrentSoulShards > 0) || NeedUnstableAffliction || NeedAgony || NeedCorruption;
            }
        }

        #endregion

        #region MultiDoT Targets

        private static Composite HandleMultiDoT(string[] spellNames)
        {
            return new PrioritySelector(ctx => GetMultiDoTTarget(spellNames),
                //Spell.Cast(Debuffs.FirstOrDefault(x => !GetMultiDoTTarget(spellNames).HasAura(x)), onUnit => ((WoWUnit)onUnit), ret => true)); //<-- needs work -- wulf
                        Spell.CastSpell("Agony", onUnit => ((WoWUnit)onUnit), ret => ret != null && !((WoWUnit)ret).HasAura("Agony"),"[Multidotting] Agony"),
                        Spell.CastSpell("Corruption", onUnit => ((WoWUnit)onUnit), ret => ret != null && !((WoWUnit)ret).HasAura("Corruption"), "[Multidotting] Corruption"));
        }

        private static WoWUnit GetMultiDoTTarget(string[] debuffs)
        {
            return Me.CurrentTarget != null ? Unit.NearbyAttackableUnits(Me.CurrentTarget.Location, 25).FirstOrDefault(x => !x.IsBoss && !x.HasAllAuras(debuffs)) : null; //!x.IsBoss() &&           
        }

        private static readonly string[] Debuffs = new[]
        {
            "Corruption",
            "Agony"
        };


        #endregion

        #region Soul Swap Multiple Targets

        private static Composite HandleSoulSwapAoE()
        {
            return
                new Decorator(ret => Me.CurrentTarget != null && Unit.NearbyAttackableUnits(Me.CurrentTarget.Location, 20).Count() >= 2 && CurrentSoulShards > 0,
                        new PrioritySelector(ctx => GetSoulSwapTarget(SoulSwapDebuffs),
                            new Decorator(ret => !Me.HasAura("Soulburn"), Spell.PreventDoubleCast("Soulburn", 1, ret => true)),
                            new Action(a => SpellManager.Cast(119678, ((WoWUnit)a)))));
        }

        private static WoWUnit GetSoulSwapTarget(string[] debuffs)
        {
            return Me.CurrentTarget != null ? Unit.NearbyAttackableUnits(Me.CurrentTarget.Location, 25).FirstOrDefault(x => !x.HasAllAuras(debuffs)) : null; //!x.IsBoss() && 
        }

        private static readonly string[] SoulSwapDebuffs = new[]
        {
            "Corruption",
            "Agony",
            "Unstable Affliction"
        };

        #endregion

        #region Seed of Curruption

        private static Composite HandleSeedofCorruptionAoE()
        {
            return
                new Decorator(ret => Me.CurrentTarget != null && Unit.NearbyAttackableUnits(Me.CurrentTarget.Location, 20).Count() > (Me.MaxSoulShards + 1) && !HasSeedofCorruption,
                        new Sequence(
                            Spell.CastSpell(27243, onUnit => Me.CurrentTarget, ret => true, "[HandleSeedofCorruptionAoE] Sed of Corruption"),
                            new WaitContinue(2, ret => HasSeedofCorruption, new ActionAlwaysSucceed())));
        }

        private static bool HasSeedofCorruption
        {
            get
            {
                if (StyxWoW.Me.CurrentTarget != null)
                {
                    var result = StyxWoW.Me.CurrentTarget.GetAllAuras().FirstOrDefault(a => a.Name == "Seed of Corruption");
                    return result != null;
                }
                return false;
            }
        }

        #endregion

        #region booleans

        private static int CurrentSoulShards { get { return Me.GetPowerInfo(WoWPowerType.SoulShards).CurrentI; } }
        private static bool NeedSoulSwap { get { return Me.CurrentTarget != null && (Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget,"Unstable Affliction",true) < CurseRefreshTotalSeconds(2, Traveltime(0), Spell.GetSpellCastTime("Unstable Affliction")) && (Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget,"Corruption", true) < CurseRefreshTotalSeconds(2, Traveltime(0), Spell.GetSpellCastTime("Corruption")) || Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget,"Agony",true) < CurseRefreshTotalSeconds(2.5, Traveltime(0), Spell.GetSpellCastTime("Agony")))); } }
        private static bool NeedHaunt
        {
            get
            {
                return !SpellManager.GlobalCooldown && Me.CurrentTarget != null && (!Me.CurrentTarget.HasAura("Haunt") ||
                                                    (Me.CurrentTarget.HasAura("Haunt") &&
                                                     Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, "Haunt", true) <
                                                     CurseRefreshTotalSeconds(2, Traveltime(20.0), Spell.GetSpellCastTime("Haunt"))));
            }
        }
        private static bool NeedUnstableAffliction
        {
            get
            {
                return !SpellManager.GlobalCooldown && Me.CurrentTarget != null && (!Me.CurrentTarget.HasAura("Unstable Affliction") ||
                                                    (Me.CurrentTarget.HasAura("Unstable Affliction") &&
                                                     Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, "Unstable Affliction", true) <
                                                     CurseRefreshTotalSeconds(2, Traveltime(0), Spell.GetSpellCastTime("Unstable Affliction"))));
            }
        }
        private static bool NeedAgony { get { return Me.CurrentTarget != null && (!Me.CurrentTarget.HasMyAura("Agony") || (Me.CurrentTarget.HasMyAura("Agony") && Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, "Agony", true) < CurseRefreshTotalSeconds(2.5, Traveltime(0), Spell.GetSpellCastTime("Agony")))); } }
        private static bool NeedCorruption { get { return Me.CurrentTarget != null && (!Me.CurrentTarget.HasMyAura("Corruption") || (Me.CurrentTarget.HasMyAura("Corruption") && Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, "Corruption", true) < CurseRefreshTotalSeconds(2, Traveltime(0), Spell.GetSpellCastTime("Corruption")))); } }
        private static bool NeedDrainSoul { get { return Me.CurrentTarget != null && Me.CurrentTarget.HealthPercent <= 20 && !Me.IsMoving; } }
        private static bool NeedTwilightWard { get { return Me.CurrentTarget != null && (Me.CurrentTarget.CastingSpell != null && (Me.CurrentTarget != null && ((Me.CurrentTarget.CastingSpell.School == WoWSpellSchool.Shadow || Me.CurrentTarget.CastingSpell.School == WoWSpellSchool.Holy) && Me.HealthPercent < 50))); } }
        private static bool NeedToSummonDoomgaurd { get { return Me.CurrentTarget != null && Me.CurrentTarget.IsBoss && Unit.UseCooldowns(); } }
        private static bool NeedCurseOfElements { get { return Me.CurrentTarget != null && !Me.CurrentTarget.HasAura("Curse of the Elements"); } }
        private static bool NeedSoulBurnForAoE { get { return Me.CurrentTarget != null && !Me.CurrentTarget.ActiveAuras.ContainsKey("Seed of Corruption"); } }
        private static bool NeedSoulBurn { get { return !Me.ActiveAuras.ContainsKey("Soulburn") && NeedSoulSwap; } }


        #endregion
    }
}