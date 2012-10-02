using CLU.Helpers;
using Styx.TreeSharp;
using CommonBehaviors.Actions;
using Styx;
using Styx.WoWInternals;
using CLU.Settings;
using CLU.Base;
using CLU.Managers;

namespace CLU.Classes.Priest
{

    class Holy : HealerRotationBase
    {
        public override string Name
        {
            get
            {
                return "Holy Priest";
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
                return "Chakra: Chastise";
            }
        }
        public override int KeySpellId
        {
            get { return 81209; }
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

        public override Composite Pull
        {
            get { return this.SingleRotation; }
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

        // Chakra
        // Chakra: Serenity --  ideal for single target and tank healing. Heal, Flash Heal, Greater Heal, Binding Heal (Holy Word: Serenity (88684)) [buff = Holy Word: Serenity crit + 25%]
        // Chakra: Sanctuary --  is therefore suited for encounters where AoE healing is preferred.  Prayer of Healing, Prayer of Mending (Holy Word: Sanctuary (88685))
        // Chakra: Chastise -- ideal for situations where damage, not healing, is the priority. smite, Mind Spike (Holy Word: Chastise (88625))
        // Spirit of Redemption -- Dont target me!
        // implement HasSerendipity for greater heals on tank and prayer of healing on group

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

                           // emergency heals on me
                           Spell.CastSpell("Desperate Prayer", ret => Me, a => Me.HealthPercent < 40 && TalentManager.HasTalent(4) && !IsSpiritofRedemption, "Desperate Prayer"),
                           Spell.CastSpell("Flash Heal", ret => Me, a => Me.HealthPercent < 40 && !IsSpiritofRedemption, "flash heal on me, emergency"),


                           // Threat
                    //Buff.CastBuff("Fade", ret => (CLUSettings.Instance.UseCooldowns || CLUSettings.Instance.Priest.UseFade) && Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Fade (Threat)"),

                           // Spell.WaitForCast(true),

                           //don't break PlayerIsChanneling
                           new Decorator(x => Spell.PlayerIsChanneling || Me.IsCasting, new Action()),

                           // Inner fire normally
                           Buff.CastBuff("Inner Fire", ret => NotMoving, "Inner Fire"),

                           // Regain some mana
                           Spell.CastSpell("Shadowfiend", u => ShadowfiendTarget, ret => NotMoving && (Me.ManaPercent < CLUSettings.Instance.Priest.ShadowfiendMana || (Buff.UnitHasHasteBuff(Me) && Me.ManaPercent < CLUSettings.Instance.Priest.ShadowfiendManaHasted && Me.GroupInfo.IsInRaid)) && StyxWoW.Me.Combat && CLUSettings.Instance.UseCooldowns, "Shadowfiend"),
                           Spell.ChannelSelfSpell("Hymn of Hope", a => NotMoving && (Me.ManaPercent < CLUSettings.Instance.Priest.HymnofHopeMana || (Buff.UnitHasHasteBuff(Me) && Me.ManaPercent < CLUSettings.Instance.Priest.HymnofHopeManaHasted && Me.GroupInfo.IsInRaid)) && StyxWoW.Me.Combat && CLUSettings.Instance.UseCooldowns, "Hymn of Hope"),

                           // ensure Chakra state
                           new Decorator(
                               ret => CLUSettings.Instance.Priest.ChakraStanceSelection == ChakraStance.Serenity && !Buff.PlayerHasBuff("Chakra: Serenity") && !IsSpiritofRedemption,
                               new Sequence(
                                   Spell.CastSpell("Chakra: Serenity", ret => Me, a => true, "Chakra (Ensure Serenity)")
                               )),

                           new Decorator(
                               ret => CLUSettings.Instance.Priest.ChakraStanceSelection == ChakraStance.Sanctuary && !Buff.PlayerHasBuff("Chakra: Sanctuary") && !IsSpiritofRedemption,
                               new Sequence(
                                   Spell.CastSpell("Chakra: Sanctuary", ret => Me, a => true, "Chakra for Sanctuary AoE target healing")
                               )),


                           //// Cooldowns
                    //Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && !IsSpiritofRedemption, 10, 65, 40f, (Me.GroupInfo.IsInRaid ? 6 : 3), "Cooldowns: Avg: 10-65, 40yrds, count: 6 or 3",
                    //                    Item.UseTrinkets(),
                    //                    Racials.UseRacials(),
                    //                    Spell.CastSelfSpell("Power Infusion", a => !Buff.UnitHasHasteBuff(Me), "Power Infusion"),
                    //                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                    //                    Item.UseEngineerGloves()
                    //),

                           // emergency heals on most injured tank
                           Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 50, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency heals on most injured tank",
                                           Spell.CastHeal("Flash Heal", a => Buff.PlayerHasActiveBuff("Surge of Light"), "Flash Heal (Surge of Light)"),
                                           Item.RunMacroText("/cast Holy Word: Serenity", a => Buff.PlayerHasBuff("Chakra: Serenity") && !WoWSpell.FromId(88684).Cooldown, "Holy Word: Serenity (Chakra: Serenity)"),
                    //Spell.CastHeal("Holy Word: Serenity", a => Buff.PlayerHasBuff("Chakra: Serenity"), "Holy Word: Serenity (emergency)"),
                                           Spell.CastHeal("Power Word: Shield", a => CanShield, "Shield tank (emergency)"),
                                           Spell.CastHeal("Greater Heal", a => HasSerendipity, "Greater heal"),
                                           Spell.CastHeal("Guardian Spirit", a => CLUSettings.Instance.UseCooldowns, "Guardian Spirit (emergency)"),
                                           Spell.CastHeal("Renew", a => !Buff.TargetHasBuff("Renew", HealTarget), "Renew (emergency)"),
                                           Spell.CastHeal("Flash Heal", a => true, "Flash heal (emergency)")
                                          ),

                            // Urgent Dispel Magic
                           Healer.FindRaidMember(a => CLUSettings.Instance.EnableDispel, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasAuraToDispel(true), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Urgent Dispel Magic",
                                                 Spell.CastHeal("Dispel Magic", a => HealTarget != null && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Magic), "Dispel Magic (Urgent)"),
                                                 Spell.CastHeal("Cure Disease", a => HealTarget != null && Buff.GetDispelType(HealTarget).Contains(WoWDispelType.Curse), "Cure Disease (Urgent)")
                                                ),

                           // I'm fine and tanks are not dying => ensure nobody is REALLY low life
                           Healer.FindRaidMember(a => !Buff.PlayerHasBuff("Chakra: Sanctuary"), x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 45, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "I'm fine and tanks are not dying => ensure nobody is REALLY low life",
                                                 Spell.CastHeal("Flash Heal", a => Buff.PlayerHasActiveBuff("Surge of Light"), "Flash Heal (Surge of Light)"),
                                                 Item.RunMacroText("/cast Holy Word: Serenity", a => Buff.PlayerHasBuff("Chakra: Serenity") && !WoWSpell.FromId(88684).Cooldown, "Holy Word: Serenity (Chakra: Serenity)"),
                    //Spell.CastHeal("Holy Word: Serenity", a => Buff.PlayerHasBuff("Chakra: Serenity"), "Holy Word: Serenity (emergency)"),
                                                 Spell.CastHeal("Greater Heal", a => HasSerendipity, "Greater heal"),
                                                 Spell.CastHeal("Renew", a => !Buff.TargetHasBuff("Renew"), "Renew (emergency)"),
                                                 Spell.CastHeal("Flash Heal", a => true, "Flash heal (emergency)")
                                                ),


                           // Prayer of Mending on tank
                           Healer.FindTank(a => !Buff.AnyHasMyAura("Prayer of Mending"), x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && !x.ToUnit().HasAura("Prayer of Mending"), (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Prayer of Mending on tank",
                                           Spell.CastHeal("Prayer of Mending", a => true, "PoM on tank")
                                          ),

                           // party healing
                           Healer.FindAreaHeal(a => CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && Spell.SpellCooldown("Divine Hymn").TotalSeconds < 0.5, 10, (Buff.PlayerHasBuff("Chakra: Sanctuary") ? 90 : 70), 40f, (Me.GroupInfo.IsInRaid ? 5 : 4), "party healing Divine Hymn: Avg: 10-70, 30yrds, count: 5 or 4",
                                               Spell.ChannelSelfSpell("Divine Hymn", a => NotMoving, "Divine Hymn")
                                              ),

                           // Holy Word: Sanctuary
                           Healer.FindAreaHeal(a => NotMoving && !Spell.PlayerIsChanneling && CLUSettings.Instance.UseCooldowns && StyxWoW.Me.Combat && Buff.PlayerHasBuff("Chakra: Sanctuary") && !WoWSpell.FromId(88685).Cooldown, 10, 95, 12f, (Me.GroupInfo.IsInRaid ? 4 : 3), "Holy Word: Sanctuary: Avg: 10-70, 30yrds, count: 3",
                                               Spell.CastSanctuaryAtLocation(u => HealTarget, a => Me.ManaPercent > 20, "Holy Word: Sanctuary")),

                           // party healing // FindParty
                           Healer.FindParty(a => true, 10, 93, 30f, 3, "FindParty healing: Avg: 10-93, 30yrds, count: 3",
                                            Spell.CastHeal("Circle of Healing", a => true, "Circle of Healing"),
                                            Spell.CastHeal("Prayer of Healing", a => true, "Prayer of healing")
                                           ),


                           // Lightwell
                           Healer.FindAreaHeal(a => CLUSettings.Instance.Priest.PlaceLightwell && StyxWoW.Me.Combat && Spell.SpellCooldown("Lightwell").TotalSeconds < 0.5, 10, 80, 20f, 2, "Lightwell: Avg: 10-80, 30yrds, count: 2",
                                               Spell.CastOnUnitLocation("Lightwell", u => HealTarget, a => !IsSpiritofRedemption, "Lightwell")),

                           // single target healing
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 92, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "single target healing",
                                                 Spell.CastHeal("Flash Heal", a => Buff.PlayerHasActiveBuff("Surge of Light"), "Flash Heal (Surge of Light)"),
                                                 Item.RunMacroText("/cast Holy Word: Serenity", a => Buff.PlayerHasBuff("Chakra: Serenity") && !WoWSpell.FromId(88684).Cooldown, "Holy Word: Serenity (Chakra: Serenity)"),
                    //Spell.CastHeal("Holy Word: Serenity", a => Buff.PlayerHasBuff("Chakra: Serenity"), "Holy Word: Serenity"),
                                                 Spell.CastHeal("Greater Heal", a => HealTarget != null && HasSerendipity && HealTarget.HealthPercent < 70, "Greater heal"),
                                                 Spell.CastHeal("Renew", a => !Buff.TargetHasBuff("Renew", HealTarget), "Renew (single target healing)"),
                                                 Spell.CastHeal("Flash Heal", a => HealTarget != null && !HasSerendipity && HealTarget.HealthPercent < (Me.GroupInfo.IsInRaid ? 50 : 75), "Flash heal (Single Target Healing)"),
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
                           Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 90 && !x.ToUnit().HasAura("Weakened Soul"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast shields while moving",
                                                 Spell.CastHeal("Power Word: Shield", a => CanShield && !Buff.TargetHasBuff("Power Word: Shield", HealTarget), "Shield while moving")
                                                ),

                           // cast renew while moving
                           Healer.FindRaidMember(a => Me.IsMoving, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 95 && !x.ToUnit().ToPlayer().HasAura("Renew"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "cast renew while moving",
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
                return
                new PrioritySelector(
                    new Decorator(
                        ret =>
                        !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport
                        && !Me.HasAura("Food") && !Me.HasAura("Drink"),
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
