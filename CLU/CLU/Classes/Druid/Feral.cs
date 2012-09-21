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
using CLU.Lists;
using CLU.Managers;
using CLU.Settings;
using CommonBehaviors.Actions;

using Styx.CommonBot;
using Styx.TreeSharp;
using System.Linq;
using CLU.Base;

using Rest = CLU.Base.Rest;


namespace CLU.Classes.Druid
{
    using Styx;

    class Feral : RotationBase
    {
        private const int ItemSetId = 1058; // Tier set ID

        public override string Name
        {
            get {
                return "Feral Druid";
            }
        }

        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
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
                return "Tiger's Fury";
            }
        }
        public override int KeySpellId
        {
            get { return 5217; }
        }
        public override string Help
        {
            get {
                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return
                    @"
----------------------------------------------------------------------
Feral MoP:
[*] 
[*] AutomaticCooldowns now works with Boss's or Mob's (See: General Settings)
This Rotation will:
1. Heal using Frenzied Regeneration, Survival Instincts, Barkskin
	==> Healthstone.
2. AutomaticCooldowns has:
    ==> UseTrinkets 
    ==> UseRacials 
    ==> UseEngineerGloves
    ==> Enrage, Berserk, Berserking, Lifeblood
3. Hot swap rotation from bear to cat if you change form
NOTE: PvP uses single target rotation - It's not designed for PvP use until Dagradt changes that.
CREDITS TO: HandNavi - because he owns the business.
----------------------------------------------------------------------" + twopceinfo + "\n" + fourpceinfo + "\n";
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

                           // Handle shapeshift form
                           Common.HandleShapeshiftForm,

                           // Rotations.
                          new Decorator(ret => Buff.PlayerHasBuff("Bear Form"), GuardianRotation), //Me.Shapeshift == ShapeshiftForm.Bear
                           new Decorator(ret => Buff.PlayerHasBuff("Cat Form"), FeralRotation)); //Me.Shapeshift == ShapeshiftForm.CreatureCat
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               new Decorator(
                                   ret => Me.Shapeshift == ShapeshiftForm.Bear,
                                   new PrioritySelector(
                                       Spell.CastSelfSpell("Frenzied Regeneration",   ret => Me.HealthPercent <= 25 && !Buff.PlayerHasBuff("Survival Instincts"), "Frenzied Regeneration"),
                                       Spell.CastSelfSpell("Survival Instincts",      ret => Me.HealthPercent <= 40 && !Buff.PlayerHasBuff("Frenzied Regeneration"), "Survival Instincts"),
                                       Spell.CastSelfSpell("Barkskin",                ret => Me.HealthPercent <= 80, "Barkskin"),
                                       Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone"))),
                               new Decorator(
                                   ret => Me.Shapeshift == ShapeshiftForm.Cat,
                                   new PrioritySelector(
                                       
                                       Spell.CastSelfSpell("Survival Instincts",      ret => Me.HealthPercent <= 40, "Survival Instincts"),
                                       Spell.CastSelfSpell("Barkskin",                ret => Me.HealthPercent <= 80, "Barkskin"),
                                       Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone")))));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastRaidBuff("Mark of the Wild", ret => true, "Mark of the Wild")));
            }
        }

        public override Composite Resting
        {
            get
            {
                return
                    new PrioritySelector(
                        Spell.CastSpell("Rejuvenation", ret => Me, ret => !Buff.PlayerHasBuff("Rejuvenation") && Me.HealthPercent < 65 && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "Rejuvenation on me"),
                        Rest.CreateDefaultRestBehaviour());
            }
        }

        public override Composite PVPRotation
        {
            get {
                return this.SingleRotation;
            }
        }

        public override Composite PVERotation
        {
            get {
                return this.SingleRotation;
            }
        }

        // NOTE: USE the Overide![SpellManager] Incarnation: Son of Ursoc (102558) overrides Incarnation (106731)
        private static Composite GuardianRotation
        {
            get
            {
                return new PrioritySelector(
                    // Cooldowns
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                  new PrioritySelector(
                                           Item.UseTrinkets(),
                                           Racials.UseRacials(),
                                           Spell.CastSelfSpell("Enrage", ret => !Spell.CanCast("Berserk", Me) && Me.CurrentRage < 80, "Enrage"),
                                           Spell.CastSelfSpell("Berserk", ret => true, "Berserk"),
                                           Item.UseEngineerGloves(),
                                           Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"))),
                           Spell.CastSpell("Mangle", ret => true, "Mangle"),
                           Spell.CastSpell("Thrash", ret => Buff.TargetDebuffTimeLeft("Weakened Blows").TotalSeconds < 2 || Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 4 || Unit.EnemyUnits.Count() > 2, "Thrash (Bear)"),
                           Spell.CastAreaSpell("Swipe", 8, false, 3, 0.0, 0.0, ret => true, "Swipe"),
                           Spell.CastSpell("Faerie Fire", ret => Buff.TargetDebuffTimeLeft("Weakened Armor").TotalSeconds < 2 || Buff.TargetCountDebuff("Weakened Armor") < 3, "Faerie Fire"),
                           Buff.CastDebuff("Lacerate", ret => true, "Lacerate"),
                           Spell.CastSpell("Maul", ret => Me.RagePercent > 70 || Buff.PlayerHasBuff("Clearcasting"), "Maul"),
                           Spell.CastSpell("Lacerate", ret => true, "Lacerate"));
            }
        }

        // NOTE: USE the Overide! [SpellManager] Incarnation: King of the Jungle (102543) overrides Incarnation (106731)
        private static Composite FeralRotation
        {
            get
            {
                return new PrioritySelector(
                                    //LETS START WITH EPIC DRUID ROTATION!


                                    Spell.CastSpell("Faerie Fire", ret => Buff.TargetCountDebuff("Weakened Armor") < 3, "Faerie Fire"),
                                    Spell.CastSpell("Savage Roar", ret => !Buff.PlayerHasBuff("Savage Roar"), "Savage Roar"),
                                    //Only use this shit if we have got DoC
                                    new Decorator(
                                        ret => SpellManager.HasSpell("Dream of Cenarius") ,
                                        new PrioritySelector(
                                    Spell.CastSpell("Healing Touch", ret => Buff.PlayerHasBuff("Predatory Swiftness") && Me.ComboPoints > 4 && Buff.PlayerCountBuff("Dream of Cenarius") < 2, "Healing Touch"),
                                    Spell.CastSpell("Healing Touch", ret => Buff.PlayerHasBuff("Predatory Swiftness") && Buff.PlayerBuffTimeLeft("Predatory Swiftness") <= 1 && !Buff.PlayerHasBuff("Dream of Cenarius"), "Healing Touch"),
                                    Spell.CastSpell("Healing Touch", ret => Common.PrevNaturesSwiftness, "Healing Touch")
                                    )),
                                    //We only use Trinkets && Hands here
                                    new Decorator(
                                       ret => Buff.PlayerHasBuff("Tiger's Fury") && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                       new PrioritySelector(
                                           Item.UseTrinkets(),
                                           Item.UseEngineerGloves()

                                           )),  // Thanks Kink#

                                    Spell.CastSpell("Tiger's Fury", ret => SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds < 1 && Common.PlayerEnergy <= 35 && !Buff.PlayerHasActiveBuff("Omen of Clarity"), "Tiger's Fury"),
                                    Spell.CastSpell("Berserk", ret => SpellManager.Spells["Berserk"].CooldownTimeLeft.TotalSeconds < 1 && (Buff.PlayerHasBuff("Tiger's Fury") || (Unit.TimeToDeath(Me.CurrentTarget) < 15 && SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds > 6)), "Berserk"),
                                    Spell.CastSpell("Nature's Vigil", ret => SpellManager.HasSpell("Nature's Vigil") && SpellManager.Spells["Nature's Vigil"].CooldownTimeLeft.TotalSeconds < 1 && Buff.PlayerHasBuff("Berserk") , "Nature's Vigil"),
                                    Spell.CastSpell("Incarnation", ret =>SpellManager.HasSpell("Incarnation") && SpellManager.Spells["Incarnation"].CooldownTimeLeft.TotalSeconds < 1 && Buff.PlayerHasBuff("Berserk") , "Incarnation"),
                                    //Use Racials!
                                    new Decorator(
                                       ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                           Racials.UseRacials()
                                           ),
                                    Spell.CastSpell("Ferocious Bite", ret => StyxWoW.Me.ComboPoints >= 1 && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 2 && StyxWoW.Me.CurrentTarget.HealthPercent <= 25, "Ferocious Bite"),
                                    Spell.CastSpell(106832, ret => Buff.PlayerHasActiveBuff("Omen of Clarity") && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 3 && !Buff.PlayerHasBuff("Dream of Cenarius"), "Thrash"),
                                    Spell.CastSpell("Savage Roar", ret => Buff.PlayerBuffTimeLeft("Savage Roar") <= 1 || (Buff.PlayerBuffTimeLeft("Savage Roar") <= 3 && StyxWoW.Me.ComboPoints > 0) && StyxWoW.Me.CurrentTarget.HealthPercent < 25, "Savage Roar"),
                                    //Only use this shit if we have got DoC
                                    new Decorator(
                                        ret => SpellManager.HasSpell("Dream of Cenarius"),
                                    Spell.CastSpell("Nature's Swiftness", ret => !Buff.PlayerHasBuff("Dream of Cenarius") && !Buff.PlayerHasBuff("Predatory Swiftness") && StyxWoW.Me.ComboPoints >= 5 && StyxWoW.Me.CurrentTarget.HealthPercent <= 25, "Nature's Swiftness")
                                    ),

                                    new Decorator(
                                       ret => (SpellManager.HasSpell("Dream of Cenarius") && StyxWoW.Me.ComboPoints >= 5 && StyxWoW.Me.CurrentTarget.HealthPercent <= 25 &&
                                    Buff.PlayerHasBuff("Dream of Cenarius")) || (!SpellManager.HasSpell("Dream of Cenarius") && Buff.PlayerHasBuff("Berserk") && StyxWoW.Me.CurrentTarget.HealthPercent <= 25) || Unit.TimeToDeath(Me.CurrentTarget) <= 40,
                                    Item.UseItem(76089)
                                    ),
                                    Spell.CastSpell("Rip", ret => StyxWoW.Me.ComboPoints >= 5 && Buff.PlayerHasBuff("Virmen's Bite") && Buff.PlayerHasBuff("Dream of Cenarius") && Common.RipMultiplier < Common.tick_multiplier && StyxWoW.Me.CurrentTarget.HealthPercent <= 25 && Unit.TimeToDeath(Me.CurrentTarget) > 30, "Rip"),
                                    Spell.CastSpell("Ferocious Bite", ret => StyxWoW.Me.ComboPoints >= 5 && Buff.TargetHasDebuff("Rip") && StyxWoW.Me.CurrentTarget.HealthPercent <= 25, "Ferocious Bite"),
                                    Spell.CastSpell("Rip", ret => StyxWoW.Me.ComboPoints >= 5 && Unit.TimeToDeath(Me.CurrentTarget) >= 6 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 2.0 && Buff.PlayerHasBuff("Dream of Cenarius"), "Rip"),
                                    Spell.CastSpell("Rip", ret => StyxWoW.Me.ComboPoints >= 5 && Unit.TimeToDeath(Me.CurrentTarget) >= 6 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 6.0 && Buff.PlayerHasBuff("Dream of Cenarius") && Common.RipMultiplier <= Common.tick_multiplier & StyxWoW.Me.CurrentTarget.HealthPercent > 25, "Rip"),
                                    Spell.CastSpell("Savage Roar", ret => Buff.PlayerBuffTimeLeft("Savage Roar") <= 1 || (Buff.PlayerBuffTimeLeft("Savage Roar") <= 3 && StyxWoW.Me.ComboPoints > 0), "Savage Roar"),
                                    //Only use this shit if we have got DoC
                                    new Decorator(
                                        ret => SpellManager.HasSpell("Dream of Cenarius"),
                                    Spell.CastSpell("Nature's Swiftness", ret => !Buff.PlayerHasBuff("Dream of Cenarius") && !Buff.PlayerHasBuff("Predatory Swiftness") && StyxWoW.Me.ComboPoints >= 5 &&Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 3 && (Buff.PlayerHasBuff("Berserk") || Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds) &&StyxWoW.Me.CurrentTarget.HealthPercent > 25, "Nature's Swiftness")
                                    ),
                                    Spell.CastSpell("Rip", ret => StyxWoW.Me.ComboPoints >= 5 && Unit.TimeToDeath(Me.CurrentTarget) >= 6 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 2.0 && (Buff.PlayerHasBuff("Berserk") || Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds), "Rip"),
                                    Spell.CastSpell(106832, ret => Buff.PlayerHasActiveBuff("Omen of Clarity") && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 3, "Thrash"),
                                    Spell.CastSpell("Ravage", ret => Common.CanRavage && Common.ExtendedRip < 3 && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4, "Ravage"),
                                    new Decorator(
                                        ret => StyxWoW.Me.CurrentTarget.MeIsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry) ||(TalentManager.HasGlyph("Shred") && (StyxWoW.Me.HasAura(5217) || StyxWoW.Me.HasAura(106951))),
                                            Spell.CastSpell("Shred", ret => !Common.CanRavage && Common.ExtendedRip < 3 && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4, "Shred")
                                        ),
                                    Spell.CastSpell("Ferocious Bite", ret => (Unit.TimeToDeath(Me.CurrentTarget) <= 4 && StyxWoW.Me.ComboPoints >= 5) || Unit.TimeToDeath(Me.CurrentTarget) <= 1, "Ferocious Bite"),
                                    Spell.CastSpell("Savage Roar", ret => Buff.PlayerBuffTimeLeft("Savage Roar") <= 6 && StyxWoW.Me.ComboPoints >= 5 &&(((Buff.TargetDebuffTimeLeft("Rip").TotalSeconds + (8 - (Common.ExtendedRip * 2))) > 6 &&(SpellManager.HasSpell("Soul of the Forest") || Buff.PlayerHasBuff("Berserk"))) ||(Buff.TargetDebuffTimeLeft("Rip").TotalSeconds + (8 - (Common.ExtendedRip * 2))) > 10) && Buff.TargetHasDebuff("Rip"), "Savage Roar"),
                                    Spell.CastSpell("Ferocious Bite", ret => StyxWoW.Me.ComboPoints >= 5 && (Buff.TargetDebuffTimeLeft("Rip").TotalSeconds + (8 - (Common.ExtendedRip * 2))) > 6 && Buff.TargetHasDebuff("Rip") && (SpellManager.HasSpell("Soul of the Forest") || Buff.PlayerHasBuff("Berserk")), "Ferocious Bite"),
                                    Spell.CastSpell("Ferocious Bite", ret => StyxWoW.Me.ComboPoints >= 5 && (Buff.TargetDebuffTimeLeft("Rip").TotalSeconds + (8 - (Common.ExtendedRip * 2))) > 10 && Buff.TargetHasDebuff("Rip"), "Ferocious Bite"),
                                    Spell.CastSpell("Rake", ret => Unit.TimeToDeath(Me.CurrentTarget) >= 8.5 && Buff.PlayerHasBuff("Dream of Cenarius") && (Common.RipMultiplier < Common.tick_multiplier), "Rake"),
                                    Spell.CastSpell("Rake", ret => Unit.TimeToDeath(Me.CurrentTarget) >= 8.5 && Buff.TargetDebuffTimeLeft("Rake").TotalSeconds < 3.0 && (Buff.PlayerHasBuff("Berserk") || (SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds + 0.8) >= Buff.TargetDebuffTimeLeft("Rake").TotalSeconds), "Rake"),
                                    Spell.CastSpell("Ravage", ret => Common.CanRavage && Buff.PlayerHasActiveBuff("Omen of Clarity"), "Ravage"),
                                    new Decorator(
                                        ret => StyxWoW.Me.CurrentTarget.MeIsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry) || (TalentManager.HasGlyph("Shred") && (StyxWoW.Me.HasAura(5217) || StyxWoW.Me.HasAura(106951))),
                                            Spell.CastSpell("Shred", ret => !Common.CanRavage && Buff.PlayerHasActiveBuff("Omen of Clarity"), "Shred")
                                        ),
                                    new Decorator(
                                        ret => Common.CanRavage ,
                                        new PrioritySelector(
                                            Spell.CastSpell("Ravage", ret => Buff.PlayerBuffTimeLeft("Predatory Swiftness") > 1 && !(Common.PlayerEnergy + (Common.EnergyRegen * (Buff.PlayerBuffTimeLeft("Predatory Swiftness") - 1)) < (4 - StyxWoW.Me.ComboPoints) * 20), "Ravage"),
                                            Spell.CastSpell("Ravage", ret => ((StyxWoW.Me.ComboPoints < 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 3.0) || (StyxWoW.Me.ComboPoints == 0 && Buff.PlayerBuffTimeLeft("Savage Roar") < 2)), "Ravage"),
                                            Spell.CastSpell("Ravage", ret => Unit.TimeToDeath(Me.CurrentTarget) <= 8.5, "Ravage")
                                            )
                                        ),

                                    // Handnavi - what about taking away the MeIsBehind check and replace all shred calls with:
                                    // private static readonly List<string> feralComboList = new List<string> { "Shred", "Mangle" };
                                    // Spell.CastSpell(feralComboList.Find(SpellManager.CanCast), ret => true, "ComboSpell"));  -- wulf
                                    new Decorator(
                                        ret => StyxWoW.Me.CurrentTarget.MeIsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry) || (TalentManager.HasGlyph("Shred") && (StyxWoW.Me.HasAura(5217) || StyxWoW.Me.HasAura(106951))),
                                        new PrioritySelector(
                                            Spell.CastSpell("Shred", ret => !Common.CanRavage && Buff.PlayerBuffTimeLeft("Predatory Swiftness") > 1 && !(Common.PlayerEnergy + (Common.EnergyRegen * (Buff.PlayerBuffTimeLeft("Predatory Swiftness") - 1)) < (4 - StyxWoW.Me.ComboPoints) * 20), "Shred"),
                                            Spell.CastSpell("Shred", ret => !Common.CanRavage && ((StyxWoW.Me.ComboPoints < 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 3.0) || (StyxWoW.Me.ComboPoints == 0 && Buff.PlayerBuffTimeLeft("Savage Roar") < 2)), "Shred"),
                                            Spell.CastSpell("Shred", ret => !Common.CanRavage && Unit.TimeToDeath(Me.CurrentTarget) <= 8.5, "Shred")
                                            )
                                        ),
                                    Spell.CastSpell(106832, ret => StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 6 && (Buff.PlayerHasBuff("Tiger's Fury") || Buff.PlayerHasBuff("Berserk")), "Thrash"),
                                    Spell.CastSpell(106832, ret => StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 6 && SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds < 3.0, "Thrash"),
                                    Spell.CastSpell(106832, ret => StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 6 && Common.TimetoEnergyCap <= 1.0, "Thrash"),
                                    
                                    new Decorator(
                                        ret => !(StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds <6) && Common.CanRavage,
                                        new PrioritySelector(
                                            Spell.CastSpell("Ravage", ret => Buff.PlayerHasBuff("Tiger's Fury") || Buff.PlayerHasBuff("Berserk"), "Ravage"),
                                            Spell.CastSpell("Ravage", ret => SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds < 3.0, "Ravage"),
                                            Spell.CastSpell("Ravage", ret => Common.TimetoEnergyCap <= 1.0, "Ravage")
                                            )
                                        ),

                                    new Decorator(
                                        ret => StyxWoW.Me.CurrentTarget.MeIsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry) || (TalentManager.HasGlyph("Shred") && (StyxWoW.Me.HasAura(5217) || StyxWoW.Me.HasAura(106951))),
                                        new PrioritySelector(
                                            Spell.CastSpell("Shred", ret => !(StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 6) && !Common.CanRavage && Buff.PlayerHasBuff("Tiger's Fury") || Buff.PlayerHasBuff("Berserk"), "Shred"),
                                            Spell.CastSpell("Shred", ret => !(StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 6) && !Common.CanRavage && SpellManager.Spells["Tiger's Fury"].CooldownTimeLeft.TotalSeconds < 3.0, "Shred"),
                                            Spell.CastSpell("Shred", ret => !(StyxWoW.Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 6) && !Common.CanRavage && Common.TimetoEnergyCap <= 1.0, "Shred")
                                            )
                                        ),

                                    Spell.CastOnUnitLocation("Force of Nature",u => Me.CurrentTarget, ret => Me.CurrentTarget != null && SpellManager.HasSpell("Force of Nature"), "Force of Nature")
                                   );

            }
        }
    }
}