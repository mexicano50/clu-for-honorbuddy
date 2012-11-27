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
using Styx.TreeSharp;

using CommonBehaviors.Actions;
using Styx;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;


namespace CLU.Classes.Shaman
{

    class Restoration : HealerRotationBase
    {
        public override string Name
        {
            get
            {
                return "Restoration Shaman";
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
            get
            {
                return "Riptide";
            }
        }
        public override int KeySpellId
        {
            get { return 61295; }
        }
        public override float CombatMaxDistance
        {
            get
            {
                return 34f;
            }
        }

        public override float CombatMinDistance
        {
            get
            {
                return 28f;
            }
        }

        /// <summary>
        /// We shouldnt need Pull behavior in the healing rotations so lets just return an empty priority selector.
        /// </summary>
        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    Movement.CreateMoveToLosBehavior(),
                    Movement.CreateFaceTargetBehavior(),
                    Spell.CastSpell("Judgment", ret => true, "Pull Judgment"),
                    Spell.CastSpell("Exorcism", ret => true, "Pull Excorcism"),
                    this.SingleRotation,
                    Movement.CreateMoveToMeleeBehavior(true));
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get
            {
                return "\n" +
                    "----------------------------------------------------------------------\n" +
                    "This Rotation will:\n" +
                    "1. Spec into Improved Cleanse Spirit to Dispel Curses *and* Magic!! Do you have it talented? " + TalentManager.HasTalent(12) + " \n" +
                    "2. Enchant Weapon: EarthLiving Weapon(MainHand)\n" +
                    "3. Totem Bar: Stoneskin Totem, Wrath of Air, Healing Stream Totem, Flametongue \n" +
                    "4. AutomaticCooldowns has: \n" +
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


        //[SpellManager] Purify Spirit (77130) overrides Cleanse Spirit (51886)
        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                    // if someone dies make sure we retarget.
                    //TargetBase.EnsureTarget(ret => StyxWoW.Me.CurrentTarget == null),

                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),

                    //Spell.WaitForCast(true),

                    // emergency heals on me
                    Spell.CastSpell("Riptide", ret => Me, a => !Buff.PlayerHasActiveBuff("Riptide") && Me.HealthPercent < 40, "Riptide on me, emergency"),
                    Spell.CastSpell("Healing Surge", ret => Me, a => Me.HealthPercent < 40, "Healing Surge on me, emergency"),

                    // Totem management
                    new Decorator(
                        ret => StyxWoW.Me.Combat,
                        Totems.CreateTotemsBehavior()),

                    // Urgent Dispel
                    Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(true), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Urgent Dispel",
                                          Spell.CastHeal("Cleanse Spirit", a => HealTarget != null && (TalentManager.HasTalent(12) && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic)) || Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Curse), "Cleanse Spirit (Urgent)"),
                                          Spell.CastHeal("Purge", a => HealTarget != null && !TalentManager.HasTalent(12) && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic), "Purge (Urgent)")
                                         ),

                    //don't break PlayerIsChanneling
                    new Decorator(x => Spell.PlayerIsChanneling, new Action()),

                    // Threat
                    Buff.CastHealBuff("Wind Shear", ret => CLUSettings.Instance.UseCooldowns && HealTarget != null && HealTarget.ThreatInfo.RawPercent > 90, "Wind Shear (Threat)"),

                    new Decorator(
                        ret => StyxWoW.Me.Combat,
                        new PrioritySelector(
                             Spell.CastSpell("Healing Tide Totem", ret => TalentManager.HasTalent(13) && StyxWoW.Me.HealthPercent < 50 && !Totems.Exist(WoWTotem.ManaTide), "Healing Tide Totem"),
                             Spell.CastSpell("Healing Stream Totem", ret => StyxWoW.Me.HealthPercent < 80 && !Totems.Exist(WoWTotemType.Water), "Healing Stream Totem")
                        )),


                    //// Cooldowns
                    //Healer.FindAreaHeal(a => Unit.UseCooldowns() && StyxWoW.Me.Combat, 10, 65, 38f, (Me.GroupInfo.IsInRaid ? 6 : 4), "Cooldowns: Avg: 10-65, 38yrds, count: 6 or 3",
                    //                    Item.UseTrinkets(),
                    //                    Racials.UseRacials(),
                    //                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                    //                    Item.UseEngineerGloves()
                    //),


                    // emergency heals on most injured tank
                    Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 60, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency heals on most injured tank",
                                    Spell.CastSpell("Unleash Elements", ret => Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Earthliving", 3345), "Unleash Elements"),
                                    Buff.CastHealBuff("Riptide", a => true, "Riptide (emergency)"),
                                    Spell.CastSelfSpell("Nature’s Swiftness", ret => CLUSettings.Instance.UseCooldowns, "Nature’s Swiftness (emergency)"),
                                    Spell.CastHeal("Greater Healing Wave", a => Buff.PlayerHasActiveBuff("Nature's Swiftness"), "Greater Healing Wave"),
                                    Spell.CastHeal("Healing Surge", a => HealTarget.HealthPercent < (Me.GroupInfo.IsInRaid ? 20 : 45), "Healing Surge (emergency)")
                                   ),


                    // I'm fine and tanks are not dying => ensure nobody is REALLY low life
                    Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 45, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "I'm fine and tanks are not dying => ensure nobody is REALLY low life",
                                          Spell.CastSpell("Unleash Elements", ret => Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Earthliving", 3345), "Unleash Elements (emergency)"),
                                          Buff.CastHealBuff("Riptide", a => true, "Riptide (emergency)"),
                                          Spell.CastSelfSpell("Nature’s Swiftness", ret => true, "Nature’s Swiftness (emergency)"),
                                          Spell.CastHeal("Greater Healing Wave", a => Buff.PlayerHasActiveBuff("Nature's Swiftness"), "Greater Healing Wave"),
                                          Spell.CastHeal("Healing Surge", a => HealTarget.HealthPercent < (Me.GroupInfo.IsInRaid ? 15 : 25), "Healing Surge (emergency)")
                                         ),

                    // Earth Shield Earth Shield on tank
                    Healer.FindTank(a => true, x => !x.ToUnit().IsDead && x.ToUnit().InLineOfSight && !x.ToUnit().HasMyAura("Earth Shield") && x.EarthShield, (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Earth Shield on tank",
                                    Buff.CastHealBuff("Earth Shield", a => true, "Earth Shield on tank")
                                   ),

                    // party healing
                    Healer.FindAreaHeal(a => Spell.SpellCooldown("Healing Rain").TotalSeconds < 0.4 && Me.ManaPercent > 30, 10, (Me.GroupInfo.IsInRaid ? 90 : 85), 20f, (Me.GroupInfo.IsInRaid ? 5 : 4), "Healing Rain party healing: Avg: 10-90 or 85, 30yrds, count: 6 or 4",
                                        Spell.CastOnUnitLocation("Healing Rain", u => HealTarget, a => true, "Healing Rain")
                                       ),


                    // party healing
                    Healer.FindAreaHeal(a => Spell.SpellCooldown("Chain Heal").TotalSeconds < 0.4, 10, (Me.GroupInfo.IsInRaid ? 93 : 90), TalentManager.HasGlyph("Chaining") ? 25f : 12f, 3, "Chain Heal party healing: Avg: 10- 90, 12yrds, count: 3",
                                        Spell.CastHeal("Chain Heal", a => true, "Chain Heal")
                                       ),


                    // Spirit Link Totem
                    Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, 10, (Me.GroupInfo.IsInRaid ? 55 : 65), 11f, 4, "Spirit link totem: Avg: 10-35 or 50, 10yrds, count: 3",
                                        Spell.CastSelfSpell("Spirit Link Totem", ret => true, "Spirit Link Totem")
                                       ),

                    // single target healing
                    Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < (Me.GroupInfo.IsInRaid ? 95 : 90), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "single target healing",
                                          Spell.CastSpell("Unleash Elements", ret => Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Earthliving", 3345), "Unleash Elements"),
                                          Buff.CastHealBuff("Riptide", a => true, "Riptide (single target healing)"),
                                          Spell.CastHeal("Greater Healing Wave", a => HealTarget.HealthPercent < (Me.GroupInfo.IsInRaid ? 20 : 70) && Buff.PlayerHasActiveBuff("Tidal Waves"), "Greater Healing Wave"),
                                          Spell.CastHeal("Healing Wave", a => true, "Healing Wave")
                                         ),

                    // Dispel
                    Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(false), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Dispel",
                                          Spell.CastHeal("Cleanse Spirit", a => HealTarget != null && (TalentManager.HasTalent(12) && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic)) || Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Curse), "Cleanse Spirit"),
                                          Spell.CastHeal("Purge", a => HealTarget != null && !TalentManager.HasTalent(12) && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic), "Purge")
                                         ),

                    // cast Riptide while moving
                    Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 90 && !x.ToUnit().ToPlayer().HasAura("Riptide"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast Riptide while moving",
                                          Spell.CastHeal("Riptide", a => true, "Riptide while moving")
                                         ),

                    // cast Unleash Elements while moving
                    Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 90, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast Unleash Elements while moving",
                                          Spell.CastSpell("Unleash Elements", ret => Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Earthliving", 3345), "Unleash Elements while moving")
                                         )
                );
            }
        }


        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                    Common.HandleCompulsoryShamanBuffs(),
                    Common.HandleTotemRecall(),
                    // Healing shit.
                    new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Spiritwalker's Grace", ret => Me.IsMoving, "Spiritwalker's Grace"))));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                                Common.HandleCompulsoryShamanBuffs(),
                                Common.HandleTotemRecall()
                              ));
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
