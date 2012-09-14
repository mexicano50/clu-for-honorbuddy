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
using System.Linq;
using CommonBehaviors.Actions;
using CLU.Lists;
using Styx.WoWInternals.WoWObjects;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Shaman
{

    class Elemental : RotationBase
    {
        public override string Name
        {
            get {
                return "Elemental Shaman";
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
                return "Thunderstorm";
            }
        }
        public override int KeySpellId
        {
            get { return 51490; }
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
                       "1. Enchant Weapon: Flametongue Weapon(MainHand)\n" +
                       "2. Totem Bar: Stoneskin Totem, Wrath of Air, Healing Stream Totem, Searing (with range check) \n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Elemental Mastery \n" +
                       "==> Fire Elemental Totem \n" +
                       "==> Earth Elemental Totem \n" +
                       "4. AoE with Magma Totem, Chain Lightning, Earthquake\n" +
                       "4. Heal using: Lightning Shield, Shamanistic Rage, Healing Surge\n" +
                       "6. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to Digitalmocking for his Initial Version and Stormchasing\n" +
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

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Spell.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                   Item.UseEngineerGloves())),
                           // PvP TRICK.
                           // Spell.CastSpell("Purge",              ret => Spell.TargetHasPurgableBuff(), "Purge"), // this is working but it needs a hashset to prioritise what to purge as we dont want to purge blessings and shit, its on my to-do
                           // Interupts
                           Spell.CastInterupt("Wind Shear",                    ret => true, "Wind Shear"),
                           // Threat
                           Buff.CastBuff("Wind Shear",                         ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Wind Shear (Threat)"),
                           // Weapon enchants
                           //Buff.CastBuff("Flametongue Weapon",                 ret => !Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Flametongue", 8024) && Item.HasSuitableWeapon(WoWInventorySlot.MainHand), "Flametongue Weapon"),
                           // Totem management
                           Totems.CreateSetTotems(),
                           // AoE
                           new Decorator(
                               ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 1 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry),
                               new PrioritySelector(
                                   Spell.CastSpellAtLocation("Earthquake", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Me.IsMoving && !Me.CurrentTarget.IsMoving && Me.ManaPercent > 60 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 10) > 4, "Earthquake"),
                                   Spell.CastSelfSpell("Thunderstorm",    ret => Me.ManaPercent < 16 || Unit.CountEnnemiesInRange(Me.Location, 10) > 1, "Thunderstorm"),
                                   Spell.CastSpell("Earth Shock",         ret => Buff.PlayerCountBuff("Lightning Shield") == 9, "Earth Shock"),
                                   Spell.CastSpell("Chain Lightning",     ret => true, "Chain Lightning")
                               )),
                           // Default Rotaion
                           Spell.CastSelfSpell("Elemental Mastery",           ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Elemental Mastery"),
                           Buff.CastDebuff("Flame Shock",                     ret => true, "Flame Shock"),
                           Spell.CastSpell("Lava Burst",                      ret => true, "Lava Burst"),
                           Spell.CastSpell("Earth Shock",                     ret => Buff.PlayerCountBuff("Lightning Shield") > 7, "Earth Shock"),
                           Spell.CastTotem("Fire Elemental Totem",            ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Totems.Any(t => t.WoWTotem != WoWTotem.FireElemental), "Fire Elemental Totem"),
                           Spell.CastTotem("Earth Elemental Totem",           ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Totems.Any(t => t.WoWTotem != WoWTotem.EarthElemental), "Earth Elemental Totem"),
                           Spell.CastTotem("Searing Totem",                   ret => Me.CurrentTarget != null && Me.CurrentTarget.Distance < Totems.GetTotemRange(WoWTotem.Searing) - 2f && !Me.Totems.Any(t => t.Unit != null && t.WoWTotem == WoWTotem.Searing && t.Unit.Location.Distance(Me.CurrentTarget.Location) < Totems.GetTotemRange(WoWTotem.Searing)) && Me.Totems.Any(t => t.WoWTotem != WoWTotem.FireElemental), "Searing Totem"),
                           Buff.CastBuff("Spiritwalker's Grace",              ret => Me.IsMoving, "Spiritwalker's Grace"),
                           Spell.CastSpell("Lightning Bolt",                  ret => true, "Lightning Bolt"),
                           Spell.CastSpell("Unleash Elements",                ret => Me.IsMoving, "Unleash Elements - Moving"),
                           Spell.CastSelfSpell("Thunderstorm",                ret => Me.ManaPercent < 16 || Unit.CountEnnemiesInRange(Me.Location, 10) > 1, "Thunderstorm")
                       );
            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               new Decorator(
                                   ret => Totems.NeedToRecallTotems,
                                   new Sequence(
                                       new Action(ret => CLU.Log(" [Totems] Recalling Totems")),
                                       new Action(ret => Totems.RecallTotems()))),
                               Item.UseBagItem("Healthstone",          ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Lightning Shield",       ret => true, "Lightning Shield"),
                               Buff.CastBuff("Shamanistic Rage",       ret => Me.CurrentTarget != null && (Me.HealthPercent < 60 || (Me.ManaPercent < 65 && Me.CurrentTarget.HealthPercent >= 75)), "Shamanistic Rage"),
                               Spell.CastSelfSpell("Healing Surge",    ret => Me.HealthPercent < 35, "Healing Surge")
                               //Buff.CastBuff("Flametongue Weapon",     ret => !Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Flametongue", 8024) && Item.HasSuitableWeapon(WoWInventorySlot.MainHand), "Flametongue Weapon")
                           ));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               new Decorator(
                                   ret => Totems.NeedToRecallTotems,
                                   new Sequence(
                                       new Action(ret => CLU.Log(" [Totems] Recalling Totems")),
                                       new Action(ret => Totems.RecallTotems()))),
                               Buff.CastBuff("Lightning Shield",   ret => true, "Lightning Shield")
                              // Buff.CastBuff("Flametongue Weapon", ret => !Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Flametongue", 8024) && Item.HasSuitableWeapon(WoWInventorySlot.MainHand), "Flametongue Weapon")
                           ));
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
