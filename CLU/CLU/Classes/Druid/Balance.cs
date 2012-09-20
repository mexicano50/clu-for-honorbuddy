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
    using global::CLU.Managers;

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
                "Has Incarnation talened: " + TalentManager.HasTalent(11) + "\n" +
                "This Rotation will:\n" + TalentManager.HasTalent(11) +
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
                "Credits to kbrebel04 for helping with this rotation\n" +
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

                    new Decorator(
                        ret => Buff.PlayerHasBuff("Moonkin Form") && !Buff.PlayerHasActiveBuff("Shadowmeld"),
                        new PrioritySelector(
                            new Decorator(
                                ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    Item.UseTrinkets(),
                                    Racials.UseRacials(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                    Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                                    Item.UseEngineerGloves())),
                    //Interupt
                    Spell.CastInterupt("Solar Beam", ret => Me.CurrentTarget != null && !Me.CurrentTarget.IsMoving, "Solar Beam"),
                    // AoE Rotation
                    Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount == 3, "Detonate Shrooms!"),
                    Spell.CastSpell("Wild Mushroom: Detonate", ret => MushroomCount > 0 && Buff.PlayerHasBuff("Eclipse (Solar)"), "Detonate Shrooms!"),
                    Spell.CastOnUnitLocation("Force of Nature", u => Me.CurrentTarget, ret => TalentManager.HasTalent(12) && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry), "Force of Nature"),
                    Spell.CastOnUnitLocation("Wild Mushroom", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 6) >= 3 && MushroomCount < 3, "Wild Mushroom"),
                    //Main Rotation [SpellManager] Incarnation: Chosen of Elune (102560) overrides Incarnation (106731)
                    Item.RunMacroText("/cast Incarnation", ret => Unit.IsTargetWorthy(Me.CurrentTarget) && !WoWSpell.FromId(102560).Cooldown && TalentManager.HasTalent(11) && (Buff.PlayerHasBuff("Eclipse Visual (Solar)") || Buff.PlayerHasBuff("Eclipse Visual (Lunar)")), "ncarnation: Chosen of Elune"),
                    Spell.CastSelfSpell("Incarnation", ret => TalentManager.HasTalent(11) && (Buff.PlayerHasBuff("Eclipse Visual (Solar)") || Buff.PlayerHasBuff("Eclipse Visual (Lunar)")), "Incarnation: Chosen of Elune"),
                    Spell.CastSelfSpell("Celestial Alignment", ret => Me.CurrentEclipse >= -20 && Me.CurrentEclipse <= 20 && (Buff.PlayerHasBuff("Incarnation: Chosen of Elune") || !TalentManager.HasTalent(11)), "Celestial Alignment"),
                    Spell.CastSpell("Moonfire",         ret => Buff.PlayerHasBuff("Eclipse Visual (Lunar)") && !Buff.TargetHasBuff("Moonfire"), "Moonfire @ Lunar"),
                    Spell.CastSpell("Sunfire",          ret => Buff.PlayerHasBuff("Eclipse Visual (Solar)") && !Buff.TargetHasBuff("Sunfire"), "Sunfire @ Solar"),
                    Spell.CastSpell("Starsurge",        ret => !Me.IsMoving, "Starsurge"),
                    Spell.CastSpell("Wrath",            ret => !Me.IsMoving && Me.CurrentEclipse <= 100 && !Buff.PlayerHasBuff("Eclipse (Lunar)") && Me.CurrentEclipse >= -80, "Wrath"),
                    Spell.CastSpell("Starfire",         ret => !Me.IsMoving && Me.CurrentEclipse >= -100 && !Buff.PlayerHasBuff("Eclipse (Solar)") && Me.CurrentEclipse <= 79, "Starfire"),
                    Spell.CastSpell("Moonfire",         ret => Me.IsMoving && !Buff.TargetHasBuff("Moonfire"), "Moonfire (Moving)"),
                    Spell.CastSpell("Sunfire",          ret => Me.IsMoving && !Buff.TargetHasBuff("Sunfire"), "Sunfire (Moving)"),
                    Spell.CastSpell("Starsurge",        ret => Me.IsMoving && Buff.PlayerHasBuff("Shooting Stars"), "Starsurge"),
                    Spell.CastSpell("Typhoon",          ret => Me.IsMoving, "Typhoon (Moving)"),
                    Spell.CastSpell("Sunfire",          ret => Me.IsMoving, "Sunfire (Moving)"))));
            }
        }

        public override Composite Medic
        {
            get
            {
                return  new PrioritySelector(
                        Spell.CastSelfSpell("Innervate", ret => Me.ManaPercent < 50, "Innvervate"),
                        new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"))));
                
                
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
