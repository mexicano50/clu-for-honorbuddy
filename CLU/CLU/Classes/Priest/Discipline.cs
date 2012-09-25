using CLU.Helpers;
using Styx.TreeSharp;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx;
using Styx.WoWInternals;
using CLU.Base;
using CLU.Managers;
using CLU.CombatLog;


namespace CLU.Classes.Priest
{

    class Discipline : HealerRotationBase
    {
        public override string Name
        {
            get
            {
                return "Discipline Priest";
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
                return "Penance";
            }
        }
        public override int KeySpellId
        {
            get { return 47540; }
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


        // adding some help about cooldown management
        public override string Help
        {
            get
            {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. \n" +
                       "2. \n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==>  \n" +
                       "4.  \n" +
                       "5. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           //new Action(delegate { CrabbyProfiler.Instance.Runs.Add(new Run("name")); }),

                           // if someone dies make sure we retarget.
                    //TargetBase.EnsureTarget(ret => StyxWoW.Me.CurrentTarget == null),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           // fuck it use it or lose it
                           Spell.CastSelfSpell("Archangel", ret => IsAtonementSpec && !Spell.PlayerIsChanneling && CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Evangelism") > 3 && Buff.PlayerBuffTimeLeft("Evangelism") < (2 + CombatLogEvents.ClientLag), "Archangel...use it or lose it!"),

                           // emergency heals on me
                           Spell.CastSpell("Desperate Prayer", ret => Me, a => Me.HealthPercent < CLUSettings.Instance.Priest.DesperatePrayerEHOM && TalentManager.HasTalent(4), "Desperate Prayer"),
                           Spell.CastSpell("Flash Heal", ret => Me, a => Me.HealthPercent < CLUSettings.Instance.Priest.FlashHealEHOM, "flash heal on me, emergency"),

                           // Threat
                    // Buff.CastBuff("Fade", ret => (CLUSettings.Instance.UseCooldowns || CLUSettings.Instance.Priest.UseFade) && Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Fade (Threat)"),

                           // Spell.WaitForCast(true),

                           //don't break PlayerIsChanneling
                           new Decorator(x => Spell.PlayerIsChanneling, new ActionAlwaysSucceed()),

                           // Inner fire normally
                           Buff.CastBuff("Inner Fire", ret => NotMoving, "Inner Fire"),

                           // Regain some mana
                           Spell.CastSpell("Shadowfiend", u => ShadowfiendTarget, ret => NotMoving && (Me.ManaPercent < CLUSettings.Instance.Priest.ShadowfiendMana || (Buff.UnitHasHasteBuff(Me) && Me.ManaPercent < CLUSettings.Instance.Priest.ShadowfiendManaHasted && Me.GroupInfo.IsInRaid)) && StyxWoW.Me.Combat && CLUSettings.Instance.UseCooldowns, "Shadowfiend"),
                           Spell.ChannelSelfSpell("Hymn of Hope", a => NotMoving && (Me.ManaPercent < CLUSettings.Instance.Priest.HymnofHopeMana || (Buff.UnitHasHasteBuff(Me) && Me.ManaPercent < CLUSettings.Instance.Priest.HymnofHopeManaHasted && Me.GroupInfo.IsInRaid)) && StyxWoW.Me.Combat && CLUSettings.Instance.UseCooldowns, "Hymn of Hope"),

                           // Cooldowns
                           Spell.CastSelfSpell("Archangel", ret => IsAtonementSpec && Buff.PlayerCountBuff("Evangelism") > CLUSettings.Instance.Priest.EvangelismStackCount && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, "Archangel"),
                    //Spell.CastSpell("Power Infusion", ret => Me, a => !Buff.UnitHasHasteBuff(Me) && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, "Power Infusion"),

                           //// Cooldowns // currently slowing the CC down
                    //Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat, 10, 65, 40f, (Me.GroupInfo.IsInRaid ? 6 : 3), "Cooldowns: Avg: 10-65, 40yrds, count: 6 or 3",
                    //                    Item.UseTrinkets(),
                    //                    Racials.UseRacials(),
                    //                    Spell.CastSelfSpell("Archangel", ret => IsAtonementSpec && Buff.PlayerCountBuff("Evangelism") > 4, "Archangel"),
                    //                    Spell.CastSpell("Power Infusion", ret => Me, a => !Buff.UnitHasHasteBuff(Me), "Power Infusion"),
                    //                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                    //                    Item.UseEngineerGloves()
                    //),

                           // emergency heals on most injured tank
                           Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < CLUSettings.Instance.Priest.EmergencyhealsonmostinjuredtankHealthPercent, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency heals on most injured tank",
                                           Spell.CastSelfSpell("Archangel", ret => IsAtonementSpec && CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Evangelism") > 4, "Archangel...Tank Time!"),
                                           Spell.CastHeal("Flash Heal", a => Buff.PlayerHasActiveBuff("Surge of Light"), "Flash Heal (Surge of Light)"),
                                           Spell.CastHeal("Power Word: Shield", a => CanShield && GraceStacks < 3, "Shield before penance (emergency)"),
                                           Spell.CastHeal("Penance", a => GraceStacks < 3 && NotMoving, "Penance (emergency)"),
                                           Spell.CastHeal("Pain Suppression", a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && !IsMe, "Pain Suppression (emergency)"),
                                           Spell.CastHeal("Power Word: Shield", a => CanShield, "Shield (emergency)"),
                                           Spell.CastHeal("Flash Heal", a => true, "Flash heal (emergency)")
                                          ),

                           // Urgent Dispel Magic
                           Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(true), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Urgent Dispel Magic",
                                                 Spell.CastHeal("Dispel Magic", a => HealTarget != null && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic), "Dispel Magic (Urgent)"),
                                                 Spell.CastHeal("Cure Disease", a => HealTarget != null && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Curse), "Cure Disease (Urgent)")
                                                ),

                           // I'm fine and tanks are not dying => ensure nobody is REALLY low life
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < CLUSettings.Instance.Priest.emergencyhealsonmostinjuredraidpartymemberHealthPercent, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "I'm fine and tanks are not dying => ensure nobody is REALLY low life",
                                                 Spell.CastSelfSpell("Archangel", ret => IsAtonementSpec && CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Evangelism") > 4, "Archangel...Single Target Time!"),
                                                 Spell.CastHeal("Flash Heal", a => Buff.PlayerHasActiveBuff("Surge of Light"), "Flash Heal (Surge of Light)"),
                                                 Spell.CastHeal("Power Word: Shield", a => CanShield && GraceStacks < 3, "Shield before penance (emergency)"),
                                                 Spell.CastHeal("Penance", a => GraceStacks < 3 && NotMoving, "Penance (emergency)"),
                                                 Spell.CastHeal("Flash Heal", a => true, "Flash heal (emergency)")
                                                ),

                           // Prayer of Mending on tank
                           Healer.FindTank(a => !Buff.AnyHasMyAura("Prayer of Mending"), x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && !x.ToUnit().HasAura("Prayer of Mending"), (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Prayer of Mending on tank",
                                           Spell.CastHeal("Prayer of Mending", a => true, "PoM on tank")
                                          ),

                           // Smite to build stacks of Evangelism
                           //Spell.CastSpellOnMostFocusedTarget("Holy Fire", u => Unit.MostFocusedUnit.Unit, ret => IsAtonementSpec && StyxWoW.Me.Combat && (Buff.PlayerCountBuff("Evangelism") < 5 || Buff.PlayerBuffTimeLeft("Evangelism") < (3.5 + CombatLogEvents.ClientLag)), "Holy Fire for Evangelism stacks.", true),
                           //Spell.CastSpellOnMostFocusedTarget("Smite", u => Unit.MostFocusedUnit.Unit, ret => IsAtonementSpec && StyxWoW.Me.Combat && (Buff.PlayerCountBuff("Evangelism") < 5 || Buff.PlayerBuffTimeLeft("Evangelism") < (3.5 + CombatLogEvents.ClientLag)), "Smite for Evangelism stacks.", true),

                           // party healing // FindParty
                           Healer.FindParty(a => true, CLUSettings.Instance.Priest.PrayerofHealingPartyminAH, CLUSettings.Instance.Priest.PrayerofHealingPartymaxAH, CLUSettings.Instance.Priest.PrayerofHealingPartymaxDBP, CLUSettings.Instance.Priest.PrayerofHealingPartyminPlayers, "party healing: Prayer of Healing",
                                            Spell.CastSelfSpell("Archangel", ret => IsAtonementSpec && CLUSettings.Instance.UseCooldowns && Buff.PlayerCountBuff("Evangelism") > 4, "Archangel...Party Time!"),
                                            new Decorator(
                                                ret => Spell.SpellCooldown("Inner Focus").TotalSeconds == 0,
                                                new Sequence(
                                                    Spell.CastHeal("Inner Focus", a => true, "Inner Focus"),
                                                    Spell.CastHeal("Prayer of Healing", a => Buff.PlayerHasActiveBuff("Inner Focus"), "Prayer of Healing")
                                                )),
                                            Spell.CastHeal("Prayer of Healing", a => true, "Prayer of healing")
                                           ),


                           // party healing
                           Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && Spell.SpellCooldown("Divine Hymn").TotalSeconds < 0.5, CLUSettings.Instance.Priest.DivineHymnPartyminAH, CLUSettings.Instance.Priest.DivineHymnPartymaxAH, CLUSettings.Instance.Priest.DivineHymnPartymaxDBP, (Me.GroupInfo.IsInRaid ? 5 : CLUSettings.Instance.Priest.DivineHymnPartyminPlayers), "party healing: Avg: 10-70, 30yrds, count: 5 or 4",
                                               Spell.ChannelSelfSpell("Divine Hymn", a => CLUSettings.Instance.UseCooldowns && NotMoving, "Divine Hymn")
                                              ),

                           // Party Barrier
                           Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && Spell.SpellCooldown("Power Word: Barrier").TotalSeconds < 0.5, CLUSettings.Instance.Priest.PowerWordBarrierPartyminAH, CLUSettings.Instance.Priest.PowerWordBarrierPartymaxAH, CLUSettings.Instance.Priest.PowerWordBarrierPartymaxDBP, (Me.GroupInfo.IsInRaid ? 5 : CLUSettings.Instance.Priest.PowerWordBarrierPartyminPlayers), "party barrier: Avg: 10-70, 30yrds, count: 5 or 4",
                                               Spell.CastOnUnitLocation("Power Word: Barrier", u => HealTarget, a => NotMoving, "Power Word: Barrier")),

                           // // party healing with Holy Nova
                    // Healer.FindAreaHeal(a => IsAtonementSpec, 10, 80, 11f, (Me.GroupInfo.IsInRaid ? 4 : 3), "party healing: Avg: 10-80, 30yrds, count: 4 or 3",
                    //    Spell.CastSelfSpell("Holy Nova", a => true, "Holy Nova")
                    // ),


                           // single target healing
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < CLUSettings.Instance.Priest.SingleTargethealingParty, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "single target healing",
                                                 Spell.CastHeal("Flash Heal", a => Buff.PlayerHasActiveBuff("Surge of Light"), "Flash Heal (Surge of Light)"),
                                                 Spell.CastHeal("Penance", a => (GraceStacks < 3 || Buff.PlayerHasActiveBuff("Borrowed Time")) && NotMoving, "Penance"),
                                                 new Decorator(
                                                     ret => HealTarget != null && (HealTarget.HealthPercent < CLUSettings.Instance.Priest.GreaterHealInnerFocusParty && Spell.SpellCooldown("Inner Focus").TotalSeconds < 0.5),
                                                     new Sequence(
                                                             Spell.CastSpell("Inner Focus", a => true, "Inner Focus"),
                                                             Spell.CastHeal("Greater Heal", a => Buff.PlayerHasActiveBuff("Inner Focus"), "Greater heal")
                                                     )),
                                                 //Spell.CastSpellOnCurrentTargetsTarget("Holy Fire", u => HealTarget, ret => HealTarget != null && IsAtonementSpec && StyxWoW.Me.Combat, "Holy Fire. Using " + CLU.SafeName(HealTarget) + "'s Target", true),
                                                 //Spell.CastSpellOnCurrentTargetsTarget("Smite", u => HealTarget, ret => HealTarget != null && IsAtonementSpec && StyxWoW.Me.Combat, "Smite. Using " + CLU.SafeName(HealTarget) + "'s Target", true),
                                                 Spell.CastHeal("Greater Heal", a => HealTarget != null && HealTarget.HealthPercent < CLUSettings.Instance.Priest.GreaterHealParty, "Greater heal"),
                                                 Spell.CastHeal("Heal", a => true, "heal")
                                                ),

                           // Dispel Magic
                           Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(false), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Dispel Magic",
                                                 Spell.CastSelfSpell("Dispel Magic", a => HealTarget != null && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic), "Dispel Magic"),
                                                 Spell.CastSelfSpell("Cure Disease", a => HealTarget != null && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Curse), "Cure Disease")
                                                ),

                           // Inner will while moving and low on mana
                           Buff.CastBuff("Inner Will", ret => Me.IsMoving && Me.ManaPercent < 20, "Inner Will"),

                           // Holy Nova with Inner will while moving
                           Healer.FindAreaHeal(a => Me.IsMoving && Buff.PlayerHasActiveBuff("Inner Will") && StyxWoW.Me.Combat, 10, 90, 10f, 2, "Holy Nova with inner will: Avg: 10-90, 10yrds, count: 2",
                                               Spell.CastSelfSpell("Holy Nova", a => true, "Holy Nova")),

                           // cast shields while moving
                           Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < CLUSettings.Instance.Priest.PowerWordShieldMovingParty && !x.ToUnit().HasAura("Weakened Soul"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast shields while moving",
                                                 Spell.CastHeal("Power Word: Shield", a => CanShield && !Buff.TargetHasBuff("Power Word: Shield"), "Shield while moving")
                                                ),

                           // cast renew while moving
                           Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < CLUSettings.Instance.Priest.RenewMovingParty && !x.ToUnit().HasAura("Renew"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast renew while moving",
                                                 Spell.CastHeal("Renew", a => true, "Renew while moving")
                                                )
                                                
                       );
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                    ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                    new PrioritySelector(
                        Item.UseBagItem("Healthstone", ret => Me.HealthPercent < CLUSettings.Instance.Priest.UseHealthstone, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    new Decorator(
                        ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            Buff.CastRaidBuff("Power Word: Fortitude", ret => CLUSettings.Instance.Priest.UsePowerWordFortitude, "Power Word: Fortitude"),
                            Buff.CastRaidBuff("Shadow Protection", ret => CLUSettings.Instance.Priest.UseShadowProtection, "Shadow Protection"),
                            Buff.CastBuff("Inner Fire", ret => NotMoving && CLUSettings.Instance.Priest.UseInnerFire, "Inner Fire"))));
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
