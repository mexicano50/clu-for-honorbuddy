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

using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx.TreeSharp;
using System.Linq;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Druid
{
    using Styx;
    using Styx.WoWInternals;

    class Guardian : RotationBase
    {
        private const int ItemSetId = 1058; // Tier set ID

        public override string Name
        {
            get {
                return "Guardian Druid";
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
                return "Savage Defense";
            }
        }
        public override int KeySpellId
        {
            get { return 62606; }
        }
        public override string Help
        {
            get {
                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return
                    @"
----------------------------------------------------------------------
Guardian MoP:
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
                           //EncounterSpecific.ExtraActionButton(),
                           
                           // Handle shapeshift form
                           //Common.HandleShapeshiftForm,

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
                                   ret => Buff.PlayerHasBuff("Bear Form"),
                                   new PrioritySelector(
                                       Spell.CastSelfSpell("Frenzied Regeneration",   ret => Me.HealthPercent <= 25 && !Buff.PlayerHasBuff("Survival Instincts"), "Frenzied Regeneration"),
                                       Spell.CastSelfSpell("Survival Instincts",      ret => Me.HealthPercent <= 40 && !Buff.PlayerHasBuff("Frenzied Regeneration"), "Survival Instincts"),
                                       Spell.CastSelfSpell("Barkskin",                ret => Me.HealthPercent <= 80, "Barkskin"),
                                       Item.UseBagItem("Healthstone",                 ret => Me.HealthPercent < 40, "Healthstone"))),
                               new Decorator(
                                   ret => Buff.PlayerHasBuff("Cat Form"),
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
            get {
                return Rest.CreateDefaultRestBehaviour();
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
                                           //Spell.CastSelfSpell("Berserk", ret => Buff.PlayerHasBuff("Pulverize") && Buff.TargetCountDebuff("Lacerate") >= 1, "Berserk"),
                                           Item.UseEngineerGloves(),
                                           Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"))),

                           Buff.CastDebuff("Mangle", ret => !WoWSpell.FromId(33878).Cooldown, "Mangle (Bear)"),
                           Spell.CastAreaSpell("Swipe", 8, false, 3, 0.0, 0.0, ret => true, "Swipe"),
                           new Decorator(ret => !WoWSpell.FromId(77758).Cooldown && Buff.TargetDebuffTimeLeft("Weakened Blows").TotalSeconds < 2 || Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 4 || Unit.EnemyUnits.Count() > 2,
                               new Sequence(
                                    new Action(a => CLU.Log(" [Casting] Thrash ")),
                                    new Action(ret => Spell.CastSpellByName("Thrash")  //todo: change this to WoWSpell.FromId(33878).Cast()
                                   ))),
                           new Decorator(ret => Buff.TargetDebuffTimeLeft("Weakened Armor").TotalSeconds < 2 || Buff.TargetCountDebuff("Weakened Armor") < 3,
                               new Sequence(
                                    new Action(a => CLU.Log(" [Casting] Faerie Fire ")),
                                    new Action(ret => Spell.CastSpellByName("Faerie Fire")  //todo: change this to WoWSpell.FromId(33878).Cast()
                                   ))),
                    //Spell.CastSpellByID(33917, ret => !WoWSpell.FromId(33878).Cooldown, "Mangle"),
                    //Spell.CastSpellByID(77758, ret => !WoWSpell.FromId(77758).Cooldown && Buff.TargetDebuffTimeLeft("Weakened Blows").TotalSeconds < 2 || Buff.TargetDebuffTimeLeft("Thrash").TotalSeconds < 4 || Unit.EnemyUnits.Count() > 2, "Thrash"),

                           //Spell.CastSpell("Faerie Swarm", ret => Buff.TargetDebuffTimeLeft("Weakened Armor").TotalSeconds < 2 || Buff.TargetCountDebuff("Weakened Armor") < 3, "Faerie Swarm"),
                    //Spell.CastSpell("Faerie Fire", ret => Buff.TargetDebuffTimeLeft("Weakened Armor").TotalSeconds < 2 || Buff.TargetCountDebuff("Weakened Armor") < 3, "Faerie Fire"),
                           Buff.CastDebuff("Lacerate", ret => true, "Lacerate"),
                           Spell.CastSpell("Maul", ret => Me.RagePercent > 70 || Buff.PlayerHasBuff("Clearcasting"), "Maul"),
                           Spell.CastSpell("Lacerate", ret => true, "Lacerate"));
            }
        }

        private static Composite FeralRotation
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
                                           Item.UseEngineerGloves()
                    //Buff.CastBuff("Lifeblood", ret => true, "Lifeblood")
                                           )),  // Thanks Kink
                    // Interupts
                                   Spell.CastInterupt("Skull Bash", ret => true, "Skull Bash"),
                                   Spell.CastSelfSpell("Tiger's Fury", ret => Me.CurrentEnergy <= (Item.Has2PcTeirBonus(ItemSetId) ? 45 : 35) && !Buff.PlayerHasBuff("Clearcasting"), "Tigers Fury"),
                                   //Spell.CastSelfSpell("Berserk", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && (Buff.PlayerHasBuff("Tiger's Fury") || Buff.TargetDebuffTimeLeft("Tiger's Fury").TotalSeconds > 6), "Berserk"),
                    // actions+=/natures_vigil,if=buff.berserk.up&talent.natures_vigil.enabled
                    // actions+=/incarnation,if=buff.berserk.up&talent.incarnation.enabled
                                   Spell.CastSelfSpell("Berserking", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.PlayerHasBuff("Tiger's Fury"), "Berserking"),
                                   Spell.CastAreaSpell("Swipe", 8, false, 3, 0.0, 0.0, ret => true, "Swipe"),
                                   Spell.CastSpell("Savage Roar", ret => (Buff.PlayerBuffTimeLeft("Savage Roar") <= 1 || (Buff.PlayerBuffTimeLeft("Savage Roar") <= 3) && Me.ComboPoints > 0 && (!Buff.PlayerHasBuff("Dream of Cenarius") || Me.ComboPoints < 5)) && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds > 4, "Savage Roar"),
                                   Spell.CastSpell("Faerie Fire", ret => Buff.TargetCountDebuff("Weakened Armor") < 3, "Faerie Fire"),
                                   //Spell.CastSpell("Faerie Swarm", ret => true, "Faerie Swarm"),
                                   Spell.CastSpell("Ferocious Bite", ret => Me.CurrentTarget != null && Me.ComboPoints > 4 && Buff.TargetHasDebuff("Rip") && ((Me.CurrentTarget.HealthPercent <= (Item.Has2PcTeirBonus(ItemSetId) ? 60 : 25)) || Me.CurrentTarget.MaxHealth == 1), "Ferocious Bite"),
                                   Spell.CastSpell("Ferocious Bite", ret => Me.CurrentTarget != null && Me.ComboPoints >= 1 && Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 2.1 && ((Me.CurrentTarget.HealthPercent <= (Item.Has2PcTeirBonus(ItemSetId) ? 60 : 25)) || Me.CurrentTarget.MaxHealth == 1), "Ferocious Bite Rip"),
                    //Item.RunMacroText("/cast Ravage",            ret => Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4, "Ravage"),
                                   Spell.CastSpell("Ravage", ret => Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4 && Common.CanRavage, "Ravage (Extend Rip)"),
                                   Spell.CastSpell("Shred", ret => Buff.TargetHasDebuff("Rip") && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 4 && (Common.IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)), "Shred (Extend Rip)"),
                                   Spell.CastSpell("Rip", ret => Me.ComboPoints > 4 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 2 && (Buff.PlayerHasBuff("Berserk") || (Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= Spell.SpellCooldown("Tiger's Fury").TotalSeconds)), "Rip"),
                                   Spell.CastSpell("Ferocious Bite", ret => Me.ComboPoints > 4 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds > 5 && Buff.PlayerBuffTimeLeft("Savage Roar") >= 1 && Buff.PlayerHasBuff("Berserk"), "Ferocious Bite"),
                    //Item.RunMacroText("/cast Savage Roar", ret => Me.ComboPoints >= 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 12 && Buff.PlayerBuffTimeLeft("Savage Roar") <= (Buff.TargetDebuffTimeLeft("Rip").TotalSeconds + 4), "Savage Roar"),
                                   Spell.CastSpell("Savage Roar", ret => Me.ComboPoints > 4 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds < 12 && Buff.PlayerBuffTimeLeft("Savage Roar") <= (Buff.TargetDebuffTimeLeft("Rip").TotalSeconds + 4), "Savage Roar"),
                                   Spell.CastSpell("Rake", ret => Buff.PlayerHasBuff("Tiger's Fury") && Buff.TargetDebuffTimeLeft("Rake").TotalSeconds < 8.9, "Rake (TF.up & Rake < 9)"),
                                   Spell.CastSpell("Rake", ret => Buff.TargetDebuffTimeLeft("Rake").TotalSeconds < 2.9 && (Buff.PlayerHasBuff("Berserk") || ((Spell.SpellCooldown("Tiger's Fury").TotalSeconds + 0.8) > Buff.TargetDebuffTimeLeft("rake").TotalSeconds)), "Rake"),
                    //Item.RunMacroText("/cast Ravage",            ret => Buff.PlayerHasBuff("Clearcasting"), "Ravage (Clearcasting)"),
                                   Spell.CastSpell("Ravage", ret => Buff.PlayerHasBuff("Clearcasting") && Common.CanRavage, "Ravage (Clearcasting)"),
                                   Spell.CastSpell("Shred", ret => Buff.PlayerHasBuff("Clearcasting") && (Common.IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)), "Shred (Clearcasting)"),
                                   Spell.CastSpell("Ferocious Bite", ret => Me.CurrentTarget != null && (Me.ComboPoints > 4 && Unit.TimeToDeath(Me.CurrentTarget) < 4) || Unit.TimeToDeath(Me.CurrentTarget) < 1, "Ferocious Bite (TTD)"),
                                   Spell.CastSpell("Ferocious Bite", ret => Me.ComboPoints > 4 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds >= 8 && Buff.PlayerHasBuff("Savage Roar"), "Ferocious Bite"),
                    //Item.RunMacroText("/cast Ravage",           ret =>  Buff.PlayerHasBuff("Tiger's Fury") && Buff.PlayerHasBuff("Berserk"), "Ravage"),
                                   Spell.CastSpell("Ravage", ret => Buff.PlayerHasBuff("Tiger's Fury") && Buff.PlayerHasBuff("Berserk") && Common.CanRavage, "Ravage"),
                    //Item.RunMacroText("/cast Ravage",           ret =>  (Me.ComboPoints <= 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 3) || (Me.ComboPoints == 0 && Buff.TargetDebuffTimeLeft("Savage Roar").TotalSeconds <= 2), "Ravage"),
                                   Spell.CastSpell("Ravage", ret => (Me.ComboPoints <= 5 && Buff.TargetDebuffTimeLeft("Rip").TotalSeconds <= 3) || (Me.ComboPoints == 0 && Buff.TargetDebuffTimeLeft("Savage Roar").TotalSeconds <= 2) && Common.CanRavage, "Ravage"),
                    //Item.RunMacroText("/cast Ravage",           ret =>  Spell.SpellCooldown("Tiger's Fury").TotalSeconds <= 3, "Ravage"),
                                   Spell.CastSpell("Ravage", ret => Spell.SpellCooldown("Tiger's Fury").TotalSeconds <= 3 && Common.CanRavage, "Ravage"),
                    //Item.RunMacroText("/cast Ravage",           ret =>  Unit.TimeToDeath(Me.CurrentTarget) < 8.5, "Ravage"),
                                   Spell.CastSpell("Ravage", ret => Unit.TimeToDeath(Me.CurrentTarget) < 8.5 && Common.CanRavage, "Ravage"),
                                   Spell.CastSpell("Shred", ret => (Common.IsBehind || BossList.CanShred.Contains(Unit.CurrentTargetEntry)), "Shred"),
                                    new Decorator(ret => !WoWSpell.FromId(33878).Cooldown && (!Common.IsBehind),
                                        new Sequence(
                                            new Action(a => CLU.Log(" [Casting] Mangle [NotBehind]")),
                                            new Action(ret => Spell.CastSpellByName("Mangle") // Spell.CastSpell("Mangle",                    ret => !WoWSpell.FromId(33878).Cooldown && (!Common.IsBehind || !BossList.CanShred.Contains(Unit.CurrentTargetEntry)), "Mangle [NotBehind]")
                                   )))

                                   );

            }
        }
    }
}