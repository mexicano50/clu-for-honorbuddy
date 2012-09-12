using System.Linq;
using CLU.Helpers;
using CLU.Lists;
using CLU.Settings;
using CommonBehaviors.Actions;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Styx.TreeSharp;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Druid
{

    class Balance : RotationBase
    {
        public override string Name
        {
            get {
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
            get {
                return "Starfire";
            }
        }

        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        private static int MushroomCount
        {
            get {
                return ObjectManager.GetObjectsOfType<WoWUnit>().Where(o => o.Entry == 47649 && o.Distance <= 40).Count(o => o.CreatedByUnitGuid == Me.Guid); 
            }
            // Thanks to Singular for the logic and code.
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {
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
            get {
                return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),

                    // HandleMovement? If so, Choose our form!
                    new Decorator(
                        ret => CLUSettings.Instance.EnableMovement,
                        new PrioritySelector(
                            Spell.CastSelfSpell("Moonkin Form", ret => !Buff.PlayerHasBuff("Moonkin Form"), "Moonkin Form")
                        )),

                    new Decorator(
                        ret => Buff.PlayerHasBuff("Moonkin Form") && !Buff.PlayerHasActiveBuff("Shadowmeld"),
                        new PrioritySelector(
                            new Decorator(
                                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Spell.UseRacials(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                    Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                                    Item.UseEngineerGloves())),
                            // Spell.CastSpell("Faerie Fire", ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsImmune(WoWSpellSchool.Nature) && Unit.IsTargetWorthy(Me.CurrentTarget) && (Buff.TargetCountDebuff("Faerie Fire") < 3 && !Buff.UnitHasArmorReductionDebuff(Me.CurrentTarget)), "Faerie Fire"),
                            // Spell.CastSpell("Faerie Fire", ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsImmune(WoWSpellSchool.Nature) && (!Buff.UnitHasArmorReductionDebuff(Me.CurrentTarget) || Buff.TargetCountDebuff("Faerie Fire") < 3), "Faerie Fire"),
                            // 8    wild_mushroom_detonate,if=buff.wild_mushroom.stack=3
                            Spell.CastSpell("Wild Mushroom: Detonate",         ret => MushroomCount == 3, "Detonate Shrooms!"),
                            Buff.CastDebuff("Insect Swarm",                    ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 30, "Insect Swarm"),
                            // B    wild_mushroom_detonate,moving=0,if=buff.wild_mushroom.stack>0&buff.solar_eclipse.up
                            Spell.CastSpell("Wild Mushroom: Detonate",         ret => MushroomCount > 0 && Buff.PlayerHasBuff("Eclipse (Solar)"), "Detonate Shrooms!"),
                            // Spell.CastSpell("Typhoon",                      ret => Me.IsMoving, "Typhoon (Moving)"),
                            // Starfall when we Lunar Eclipse
                            Spell.CastSelfSpell("Starfall",                    ret => Me.CurrentTarget != null && (Unit.IsTargetWorthy(Me.CurrentTarget) || Unit.CountEnnemiesInRange(Me.Location, 40) >= 3) && Me.CurrentEclipse < -80 && Buff.PlayerHasBuff("Eclipse (Lunar)"), "Starfall"),
                            // Moonfire / Sunfire
                            Spell.CastSpell("Moonfire",                        ret => Me.CurrentTarget != null && Unit.TimeToDeath(Me.CurrentTarget) > 12 && (!Buff.PlayerHasBuff("Eclipse (Solar)") && Buff.TargetDebuffTimeLeft("Moonfire").TotalSeconds < 1 && !Buff.TargetHasDebuff("Sunfire")) || (Buff.PlayerHasBuff("Eclipse (Solar)") && Buff.TargetDebuffTimeLeft("Sunfire").TotalSeconds < 1 && !Buff.TargetHasDebuff("Moonfire")), "Moonfire/Sunfire"),
                            // Make sure we cast it unless we're about to Eclipse
                            Spell.CastSpell("Starsurge",                       ret => (Me.CurrentEclipse >= -85 && Me.CurrentEclipse <= 85) || (Buff.PlayerHasBuff("Eclipse (Solar)") || Buff.PlayerHasBuff("Eclipse (Lunar)")), "Starsurge"),
                            Spell.CastSelfSpell("Innervate",                   ret => Me.ManaPercent < 50, "Innvervate"),
                            Spell.CastSpellAtLocation("Force of Nature", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Force of Nature"),
                            Spell.CastSpellAtLocation("Wild Mushroom", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 6) >= 3 && MushroomCount < 3, "Wild Mushroom"),
                            // Spell.CastSpell("Wrath",                       ret => Me.CurrentEclipse >= 80 && Me.CastingSpell.Name == "Starfire", "Wrath"),
                            // Spell.CastSpell("Starfire",                    ret => Me.CurrentEclipse <= -87 && Me.CastingSpell.Name == "Wrath", "Starfire"),
                            Spell.CastSpell("Starfire",                        ret => Buff.PlayerHasBuff("Eclipse (Lunar)") || Me.CurrentEclipse == -100, "Starfire"),
                            Spell.CastSpell("Wrath",                           ret => Buff.PlayerHasBuff("Eclipse (Solar)") || Me.CurrentEclipse == 100, "Wrath"),
                            Spell.CastSpell("Starfire",                        ret => Me.CurrentEclipse > 0, "Starfire"),
                            Spell.CastSpell("Wrath",                           ret => Me.CurrentEclipse < 0, "Wrath"),
                            Spell.CastSpell("Starfire",                        ret => true, "Starfire"),
                            Spell.CastSpellAtLocation("Wild Mushroom", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.IsMoving && !Me.CurrentTarget.IsMoving && MushroomCount < 3, "Wild Mushroom"),
                            // Not working for some reason
                            Item.RunMacroText("/cast Starsurge",               ret => Me.IsMoving && Buff.PlayerHasActiveBuff("Shooting Stars"), "Starsurge"),
                            Spell.CastSpell("Moonfire",                        ret => Me.IsMoving, "Moonfire"))));
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone")));
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
    }
}
