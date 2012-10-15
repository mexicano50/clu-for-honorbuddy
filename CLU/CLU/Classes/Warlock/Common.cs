#region Revision info
/*
 * $Author: clutwopointzero@gmail.com $
 * $Date: 2012-09-14 07:56:17 +0200 (Fr, 14 Sep 2012) $
 * $ID$
 * $Revision: 271 $
 * $URL: https://clu-for-honorbuddy.googlecode.com/svn/trunk/CLU/CLU/Classes/DeathKnight/Common.cs $
 * $LastChangedBy: clutwopointzero@gmail.com $
 * $ChangesMade$
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Styx;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using global::CLU.Base;
using global::CLU.Managers;
using global::CLU.Settings;
namespace CLU.Classes.Warlock
{
    class Common
    {
        public static HashSet<int> DarkSoul = new HashSet<int>
        {
            113860, // Affliction 
            113861, // Another Warlock Specc
            113858  // Another Warlock Specc
        };
        public static HashSet<int> Debuffs = new HashSet<int>
        {
        172,  // Corruption
        980,  // Agony
        30108 // Unstable Affliction
        };
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
        /// <summary>
        ///  Warlock SelfHealing for all Speccs
        /// </summary>
        public static Composite WarlockMedic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }
        public static Composite WarlockGrimoire
        {
            get 
            {
                return new PrioritySelector(
                            //Grimoire of Service
                            Spell.CastSpell("Grimoire: Felhunter", ret => Me.GotAlivePet && !WoWSpell.FromId(111897).Cooldown && TalentManager.HasTalent(14) && (Me.Specialization == WoWSpec.WarlockAffliction || Me.Specialization == WoWSpec.WarlockDestruction), "Grimoire of Service"),
                            Spell.CastSpell("Grimoire: Felguard", ret => Me.GotAlivePet && !WoWSpell.FromId(111898).Cooldown && TalentManager.HasTalent(14) && Me.Specialization == WoWSpec.WarlockDemonology, "Grimoire of Service"),
                            //Sacrifice Pet
                            Spell.CastSelfSpell("Grimoire of Sacrifice", ret => Me.GotAlivePet && TalentManager.HasTalent(15) && !Buff.PlayerHasActiveBuff(108503) && !WoWSpell.FromId(108503).Cooldown, "Grimoire of Sacrifice")
                            );
            }
        }
        public static Composite WarlockPreCombat
        {
            get
            {
                return new PrioritySelector(
                        new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                            new PrioritySelector(
                                Buff.CastBuff("Dark Intent", ret => true, "Dark Intent"),
                                Spell.CastSelfSpell(108503, ret => Me.GotAlivePet && TalentManager.HasTalent(15) && !Buff.PlayerHasActiveBuff(108503) && !WoWSpell.FromId(108503).Cooldown, "Grimoire of Sacrifice"),
                                new Decorator(ret => CLUSettings.Instance.Warlock.CurrentPetType == PetType.None && !Me.IsMoving && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503),
                                    new Sequence(
                                        new Switch<WoWSpec>(ctx => Me.Specialization,
                                            new SwitchArgument<WoWSpec>(WoWSpec.None, PetManager.CastPetSummonSpell("Summon Imp",ret=>true,"Summon Pet")),
                                            new SwitchArgument<WoWSpec>(WoWSpec.WarlockAffliction, PetManager.CastPetSummonSpell("Summon Felhunter", ret => true, "Summon Pet")),
                                            new SwitchArgument<WoWSpec>(WoWSpec.WarlockDemonology, PetManager.CastPetSummonSpell("Summon Felguard", ret => true, "Summon Pet")),
                                            new SwitchArgument<WoWSpec>(WoWSpec.WarlockDestruction, PetManager.CastPetSummonSpell("Summon Felhunter", ret => true, "Summon Pet")))
                                        )
                                ),
                                new Decorator(ret => CLUSettings.Instance.Warlock.CurrentPetType != PetType.None && !Me.IsMoving && !Me.GotAlivePet && !Buff.PlayerHasActiveBuff(108503),
                                    new Sequence(
                                        new Switch<PetType>(ctx => CLUSettings.Instance.Warlock.CurrentPetType,
                                            new SwitchArgument<PetType>(PetType.Felguard, PetManager.CastPetSummonSpell("Summon Felguard", ret => true, "Summon Pet")),
                                            new SwitchArgument<PetType>(PetType.Felhunter, PetManager.CastPetSummonSpell("Summon Felhunter", ret => true, "Summon Pet")),
                                            new SwitchArgument<PetType>(PetType.Imp, PetManager.CastPetSummonSpell("Summon Imp", ret => true, "Summon Pet")),
                                            new SwitchArgument<PetType>(PetType.Voidwalker, PetManager.CastPetSummonSpell("Summon Voidwalker", ret => true, "Summon Pet")),
                                            new SwitchArgument<PetType>(PetType.Succubus, PetManager.CastPetSummonSpell("Summon Succubus", ret => true, "Summon Pet")))
                                        )
                                ))));
            }
        }
        public static Composite WarlockTierOneTalents 
        {
            get 
            {
                return new PrioritySelector(
                    new Decorator(ret=> TalentManager.HasTalent(1) && !WoWSpell.FromId(108359).Cooldown, Spell.CastSelfSpell(108359,ret => true,"Dark Regneration")),
                    new Decorator(ret=> TalentManager.HasTalent(3) && !Me.IsChanneling && ObjectManager.GetObjectsOfType<WoWUnit>(false,false).Count(q=> q.DistanceSqr<=15*15)>=2, Spell.ChannelSpell("Drain Life",ret=>true,"Harvest Life")),
                    Spell.ChannelSpell("Drain Life",ret=> true,"Drain Life")
                    );
            }
        }
    }
}
