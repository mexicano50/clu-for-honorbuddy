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

using System;
using CLU.Helpers;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.WoWInternals;
using Styx.TreeSharp;
using CLU.Base;
using CLU.Managers;

namespace CLU.Classes.Druid
{
    using Styx.CommonBot;

    class Restoration : HealerRotationBase
    {
        public override string Name
        {
            get {
                return "Restoration Druid";
            }
        }
        public override string Revision
        {
            get
            {
                return "$Rev$";
            }
        }
        public override string KeySpell
        {
            get {
                return "Swiftmend";
            }
        }
        public override int KeySpellId
        {
            get { return 18562; }
        }
        public override float CombatMaxDistance
        {
            get {
                return 34f;
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 28f;
            }
        }


        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Has Natures Cure? " + TalentManager.HasTalent(17) + " \n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "4. Heal using: \n" +
                       "6. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // Performance Timer (True = Enabled) returns Runstatus.Failure
                           // Spell.TreePerformance(true),

                           // Spell.WaitForCast(true),

                           // if someone dies make sure we retarget.
                           TargetBase.EnsureTarget(ret => ObjectManager.Me.CurrentTarget == null),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           // emergency heals on me
                           Spell.CastSelfSpell("Barkskin", a => !Buff.PlayerHasActiveBuff("Barkskin") && Me.HealthPercent < 40, "Barkskin on me, emergency"),
                           Spell.CastSpell("Regrowth", ret => Me, a => Me.HealthPercent < 40, "Regrowth on me, emergency"),

                           // Remove Corruption (Urgent)
                           Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(true), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Remove Corruption (Urgent)",
                                                 Spell.CastSpell("Remove Corruption", a => true, "Remove Corruption (Urgent)")
                                                ),
                           // Lifebloom to three stacks
                           Healer.FindTank(a => StyxWoW.Me.Combat, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && (!x.ToUnit().HasAura("Lifebloom") || x.ToUnit().Auras["Lifebloom"].StackCount < 3 || x.ToUnit().Auras["Lifebloom"].TimeLeft <= TimeSpan.FromSeconds(3)) && x.LifeBloom, (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Lifebloom to three stacks",
                                           Spell.CastSpell("Lifebloom", a => true, "Lifebloom on tank")
                                          ),

                           // don't break PlayerIsChanneling
                           new Decorator(x => Spell.PlayerIsChanneling, new ActionAlwaysSucceed()),


                           // Rebirth the tank first
                           Healer.FindTank(a => Spell.SpellCooldown("Rebirth").TotalSeconds < 0.2 && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, x => x.ToUnit().IsDead, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Rebirth the tank first",
                                           Spell.CastSpell("Rebirth", a => true, "Rebirth on Tank")
                                          ),

                           // Rebirth the Healers next
                           Healer.FindHealer(a => Spell.SpellCooldown("Rebirth").TotalSeconds < 0.2 && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, x => x.ToUnit().IsDead, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Rebirth the Healers next",
                                             Spell.CastSpell("Rebirth", a => true, "Rebirth on Healer")
                                            ),

                           // Mana
                           Spell.CastSpell("Innervate", ret => Me, ret => Me.ManaPercent <= 80 && StyxWoW.Me.Combat, "Innervate Me!"),


                           // Cooldowns
                           //Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, 10, 65, 38f, (Me.IsInRaid ? 6 : 4),"party healing: Avg: 10-65, 38yrds, count: 6 or 4",
                           //                    Item.UseTrinkets(),
                           //                    Spell.UseRacials(),
                           //                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                           //                    Item.UseEngineerGloves()
                           //),

                           // emergency heals on most injured tank
                           Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 50, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency heals on most injured tank",
                                           new Sequence(ret => Spell.CanCast("Nature's Swiftness", Me),
                                                        Spell.CastSelfSpell("Nature's Swiftness", ret => true, "Nature's Swiftness"),
                                                        Spell.CastSpell("Healing Touch", a => true, "Healing Touch")),
                                           Spell.CastSpell("Swiftmend", a => (Buff.TargetHasBuff("Regrowth") || Buff.TargetHasBuff("Rejuvenation")), "Swiftmend (emergency)"),
                                           Spell.CastSpell("Rejuvenation", a => !Buff.TargetHasBuff("Rejuvenation"), "Rejuvenation (emergency)"),
                                           Spell.CastSpell("Regrowth", a => true, "Regrowth (emergency)")
                                          ),

                           // I'm fine and tanks are not dying => ensure nobody is REALLY low life
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 45, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "I'm fine and tanks are not dying => ensure nobody is REALLY low life",
                                                 Spell.CastSpell("Swiftmend", a => (Buff.TargetHasBuff("Regrowth") || Buff.TargetHasBuff("Rejuvenation")), "Swiftmend (emergency)"),
                                                 Spell.CastSpell("Rejuvenation", a => !Buff.TargetHasBuff("Rejuvenation"), "Rejuvenation (emergency)"),
                                                 new Sequence(ret => Spell.CanCast("Nature's Swiftness", Me),
                                                         Spell.CastSelfSpell("Nature's Swiftness", ret => true, "Nature's Swiftness"),
                                                         Spell.CastSpell("Healing Touch", a => true, "Healing Touch")),
                                                 Spell.CastSpell("Regrowth", a => true, "Regrowth (emergency)")
                                                ),

                           // party healing
                           Healer.FindAreaHeal(a => SpellManager.CanCast("Wild Growth"), 10, 95, 30f, (Me.IsInRaid ? 4 : 3), "Wild Growth party healing: Avg: 10-95, 30yrds, count: 5 or 3",
                                               Spell.CastSpell("Wild Growth", ret => true, "Wild Growth")
                                              ),

                           // Lifebloom during Tree of Life
                           Healer.FindTank(a => StyxWoW.Me.Shapeshift == ShapeshiftForm.TreeOfLife && StyxWoW.Me.Combat, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && (!x.ToUnit().HasAura("Lifebloom") || x.ToUnit().Auras["Lifebloom"].StackCount < 3 || x.ToUnit().Auras["Lifebloom"].TimeLeft <= TimeSpan.FromSeconds(3)), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Lifebloom during Tree of Life",
                                           Spell.CastSpell("Lifebloom", a => true, "Lifebloom tree of life")
                                          ),


                           // Tree of Life oh shit
                           Healer.FindAreaHeal(a => SpellManager.CanCast("Tree of Life") && StyxWoW.Me.Combat, 10, (Me.IsInRaid ? 75 : 65), 30f, (Me.IsInRaid ? 5 : 3), "Tree of Life party healing: Avg: 10-70 or 65, 30yrds, count: 5 or 3",
                                               Spell.CastSelfSpell("Tree of Life", ret => CLUSettings.Instance.UseCooldowns, "Tree of Life") //TODO: Check.
                                              ),

                           // Tranquility oh shit
                           Healer.FindAreaHeal(a => SpellManager.CanCast("Tranquility") && !Buff.PlayerHasBuff("Tree of Life") && CLUSettings.Instance.UseCooldowns, 10, (Me.IsInRaid ? 80 : 65), 40f, (Me.IsInRaid ? 5 : 4), "Tranquility party healing: Avg: 10-70 or 65, 40yrds, count: 5 or 3",
                                               Spell.ChannelSelfSpell("Tranquility", ret => true, "Tranquility")
                                              ),

                           // single target healing
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 95, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "single target healing",
                                                 Spell.CastSpell("Swiftmend", a =>  (Buff.TargetHasBuff("Regrowth") || Buff.TargetHasBuff("Rejuvenation")), "Swiftmend (Single)"),
                                                 Spell.CastSpell("Regrowth", a => !Buff.TargetHasBuff("Regrowth") && Buff.PlayerHasActiveBuff("Clearcasting"), "Regrowth (Single-Clearcasting)"),
                                                 Spell.CastSpell("Rejuvenation", a => !Buff.TargetHasBuff("Rejuvenation"), "Rejuvenation (Single)"),
                                                 Spell.CastSpell("Regrowth", a => !Buff.TargetHasBuff("Regrowth") && (CurrentTargetHealthPercent < (Me.IsInRaid ? 70 : 80) || !Buff.PlayerHasBuff("Harmony")), "Regrowth (single)"),
                                                 Spell.CastSpell("Healing Touch", a => CurrentTargetHealthPercent < 70 && !Buff.TargetHasBuff("Wild Growth"), "Healing Touch (Single)"),
                                                 Spell.CastSpell("Nourish", a => CanNourish, "Nourish (Single)"),
                                                 Spell.CastSpell("Rejuvenation", a => true, "Rejuvenation (Single)")
                                                ),


                           // Remove Corruption // Assumes you have Nature's Cure talented as well!
                           Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(false), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Remove Corruption",
                                                 Spell.CastSpell("Remove Corruption", a => true, "Remove Corruption")
                                                ),

                           // cast Rejuvenation while moving
                           Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 90 && !x.ToUnit().ToPlayer().HasAura("Rejuvenation"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast Rejuvenation while moving",
                                                 Spell.CastSpell("Rejuvenation", a => !Buff.TargetHasBuff("Rejuvenation"), "Rejuvenation while moving")
                                                ),

                           // cast Swiftmend while moving
                           Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 90 && (x.ToUnit().HasAura("Rejuvenation") || x.ToUnit().HasAura("Regrowth")), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast Swiftmend while moving",
                                                 Spell.CastSpell("Swiftmend", a => true, "Swiftmend while moving")
                                                )
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone")
                    ));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                    ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                    new PrioritySelector(
                        Buff.CastRaidBuff("Mark of the Wild", ret => !Spell.PlayerIsChanneling, "Mark of the Wild")));
            }
        }

        public override Composite Resting
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}
