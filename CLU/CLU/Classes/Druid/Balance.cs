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
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using Styx;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Druid
{

    class Balance : RotationBase
    {
        public override string Name
        {
            get
            {
                return "Balance Druid";
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
                return "Starfire";
            }
        }
        public override int KeySpellId
        {
            get { return 2912; }
        }
        public override float CombatMinDistance
        {
            get
            {
                return 30f;
            }
        }

        private static string _oldDps = "Wrath";

        private static string BoomkinDpsSpell
        {
            get
            {
                if (StyxWoW.Me.HasAura("Eclipse (Solar)"))
                {
                    _oldDps = "Wrath";
                }
                // This doesn't seem to register for whatever reason.
                else if (StyxWoW.Me.HasAura("Eclipse (Lunar)")) //Eclipse (Lunar) => 48518
                {
                    _oldDps = "Starfire";
                }

                return _oldDps;
            }
        }

        private static int MushroomCount
        {
            get
            {
                return ObjectManager.GetObjectsOfType<WoWUnit>().Where(o => o.Entry == 47649 && o.Distance <= 40).Count(o => o.CreatedByUnitGuid == Me.Guid);
            }
            // Thanks to Singular for the logic and code.
        }

        // adding some help about cooldown management
        public override string Help
        {
            get
            {
                return "\n" +
                "----------------------------------------------------------------------\n" +
                "This Rotation will:\n" +
                "1. Attempt to heal with healthstone\n" +
                "2. Raid buff Mark of the Wild\n" +
                "3. AutomaticCooldowns has: \n" +
                "==> UseTrinkets \n" +
                "==> UseRacials \n" +
                "==> UseEngineerGloves \n" +
                "==> Force of Nature & Volcanic Potion & Faerie Fire\n" +
                "4. AoE with Wild Mushroom, Starfall, \n" +
                "5. Best Suited for end game raiding\n" +
                "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                "Credits to Obliv for creating this rotation\n" +
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
                    Spell.WaitForCast(true),
                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),
                    // HandleMovement? If so, Choose our form!
                    new Decorator(
                        ret => CLUSettings.Instance.EnableMovement,
                        new PrioritySelector(
                            Spell.CastSelfSpell(
                                "Moonkin Form", ret => !Buff.PlayerHasBuff("Moonkin Form"), "Moonkin Form"))),
                    //new Decorator(
                    //    ret => Buff.PlayerHasBuff("Moonkin Form") && !Buff.PlayerHasActiveBuff("Shadowmeld"),
                    //    new PrioritySelector(
                    //        new Decorator(
                    //            ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                    //            new PrioritySelector(
                    //                Item.UseTrinkets(),
                    //                Racials.UseRacials(),
                    //                Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                    //                Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                    //                Item.UseEngineerGloves())),
                    // Spell.CastSpell("Faerie Fire", ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsImmune(WoWSpellSchool.Nature) && Unit.IsTargetWorthy(Me.CurrentTarget) && (Buff.TargetCountDebuff("Faerie Fire") < 3 && !Buff.UnitHasArmorReductionDebuff(Me.CurrentTarget)), "Faerie Fire"),
                    // Spell.CastSpell("Faerie Fire", ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsImmune(WoWSpellSchool.Nature) && (!Buff.UnitHasArmorReductionDebuff(Me.CurrentTarget) || Buff.TargetCountDebuff("Faerie Fire") < 3), "Faerie Fire"),
                    // 8    wild_mushroom_detonate,if=buff.wild_mushroom.stack=3
                    //Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount == 3, "Detonate Shrooms!"),
                    //Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount > 0 && Buff.PlayerHasBuff("Eclipse (Solar)"), "Detonate Shrooms!"),
                    // Spell.CastSpell("Typhoon",                      ret => Me.IsMoving, "Typhoon (Moving)"),
                    // Big Stuff
                    // actions+=/incarnation,if=talent.incarnation.enabled
                    //Item.RunMacroText("/cast Incarnation",             ret => Buff.PlayerHasBuff("Eclipse (Lunar)") && !Spell.SpellOnCooldown("Incarnation: Chosen of Elune") || Buff.PlayerHasBuff("Eclipse (Solar)") && !Spell.SpellOnCooldown("Incarnation: Chosen of Elune"), "Incarnation"),
                    //Spell.CastSelfSpell("Incarnation: Chosen of Elune",                 ret => true, "Incarnation: Chosen of Elune"),
                    //Spell.CastSelfSpell("Celestial Alignment",         ret => Me.CurrentEclipse >= -20 && Me.CurrentEclipse <= 20, "Celestial Alignment"),
                    //Spell.CastSelfSpell("Starfall",                    ret => Me.CurrentTarget != null && (Unit.IsTargetWorthy(Me.CurrentTarget) && !Buff.PlayerHasActiveBuff("Starfall")), "Starfall"),
                    // Moonfire / Sunfire
                    //Spell.CastSpell("Sunfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 6 && Me.CurrentEclipse == 5 && Buff.PlayerHasBuff("Eclipse (Solar)") && Spell.SpellOnCooldown("Celestial Alignment"), "Sunfire @ 5 Solar"),
                    //Spell.CastSpell("Sunfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 6 && Me.CurrentEclipse == 10 && Buff.PlayerHasBuff("Eclipse (Solar)") && Spell.SpellOnCooldown("Celestial Alignment"), "Sunfire @ 10 Solar"),	
                    //Spell.CastSpell("Sunfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 6 && Me.CurrentEclipse == 15 && Buff.PlayerHasBuff("Eclipse (Solar)") && Spell.SpellOnCooldown("Celestial Alignment"), "Sunfire @ 15 Solar"),							
                    //Buff.CastDebuff("Sunfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds <= 12 && Me.CurrentEclipse == 100 && Buff.PlayerHasBuff("Eclipse (Solar)"), "Sunfire Start Solar"),
                    //Buff.CastDebuff("Sunfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 2 && Me.CurrentEclipse < 80 && !Buff.PlayerHasBuff("Eclipse (Solar)"), "Sunfire @ Last"),
                    //Buff.CastDebuff("Sunfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && !Buff.TargetHasDebuff("Sunfire") && Me.CurrentEclipse < 80 && !Buff.PlayerHasBuff("Eclipse (Solar)"), "Sunfire @ Last"),
                    //Buff.CastDebuff("Moonfire",                         ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 6 && Me.CurrentEclipse == -20 && Buff.PlayerHasBuff("Eclipse (Lunar)") && Spell.SpellOnCooldown("Celestial Alignment"), "Moonfire @ 20 Lunar"),						
                    //Buff.CastDebuff("Moonfire",                        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds <= 12 && Me.CurrentEclipse == -100 && Buff.PlayerHasBuff("Eclipse (Lunar)"), "Moonfire Start Lunar"),
                    //Buff.CastDebuff("Moonfire",                        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 2 && Me.CurrentEclipse > -65 && !Buff.PlayerHasBuff("Eclipse (Lunar)"), "Moonfire @ Last"),
                    //Buff.CastDebuff("Moonfire",                        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.PlayerBuffTimeLeft("Celestial Alignment") >= 14 , "Moonfire"),
                    //Buff.CastDebuff("Moonfire",                        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && Buff.PlayerBuffTimeLeft("Celestial Alignment") <= 3 && Buff.PlayerBuffTimeLeft("Celestial Alignment") >= 1 && Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 12, "Moonfire"),
                    // Make sure we cast it unless we're about to Eclipse
                    //Spell.CastSpell("Starsurge",                       ret => Buff.PlayerHasBuff("Celestial Alignment"), "Starsurge"),
                    //Buff.CastDebuff("Starsurge",                       ret => Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse >= 5 || Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse <= -5, "Starsurge"),
                    //Spell.CastSelfSpell("Innervate", ret => Me.ManaPercent < 50, "Innvervate"),
                    //Spell.CastOnUnitLocation("Force of Nature", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Force of Nature"),
                    //Spell.CastOnUnitLocation("Wild Mushroom", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 6) >= 3 && MushroomCount < 3, "Wild Mushroom"),
                    Spell.CastSpell(
                        "Sunfire",
                        ret =>
                       Buff.PlayerHasBuff("Eclipse Visual (Solar)") && //Buff.HasAura(Me, "Eclipse (Solar)", Me)
                        !Buff.TargetHasBuff("Sunfire"),
                        "Sunfire @ Solar"),
                    Spell.CastSpell(
                        "Moonfire",
                        ret =>
                        Buff.PlayerHasBuff("Eclipse Visual (Lunar)")
                        && !Buff.TargetHasBuff("Moonfire"),
                        "Moonfire @ Lunar"),
                    Spell.CastSpell(
                        "Wrath",
                        ret =>
                        Me.CurrentEclipse <= 100 && !Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse >= -80,
                        "Wrath"),
                    Spell.CastSpell(
                        "Starfire",
                        ret =>
                        Me.CurrentEclipse >= -100 && !Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse <= 79,
                        "Starfire"));
                //Spell.CastSpell("Wrath", ret => BoomkinDpsSpell == "Wrath", "Wrath"),
                //Spell.CastSpell("Starfire", ret => BoomkinDpsSpell == "Starfire", "Starfire"),
                //Spell.CastSpell("Starfire",                        ret => true, "Starfire"),
                //Spell.CastOnUnitLocation("Wild Mushroom", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.IsMoving && !Me.CurrentTarget.IsMoving && MushroomCount < 3, "Wild Mushroom"))));
                // Not working for some reason
                // Item.RunMacroText("/cast Starsurge",               ret => Me.IsMoving && Buff.PlayerHasActiveBuff("Shooting Stars"), "Starsurge")
                //Spell.CastSpell("Moonfire"/,                        ret => Me.IsMoving, "Moonfire")
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
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
                return Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return this.SingleRotation;
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return this.SingleRotation;
            }
        }
    }
}
