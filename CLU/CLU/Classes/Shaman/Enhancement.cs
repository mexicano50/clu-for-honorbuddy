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

    class Enhancement : RotationBase
    {
        private const int ItemSetId = 1071;

        public override string Name
        {
            get {
                return "Enhancement Shaman";
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
                return "Lava Lash";
            }
        }
        public override int KeySpellId
        {
            get { return 60103; }
        }
        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {

                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       twopceinfo + "\n" +
                       fourpceinfo + "\n" +
                       "1. Enchant Weapons: Windfury Weapon(MainHand) & Flametongue Weapon(OffHand)\n" +
                       "2. Totems: Strength of earth, Windfury, Mana Spring, Searing (with range check) \n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Feral Spirit \n" +
                       "4. AoE with Magma Totem, Chain Lightning, Fire Nova\n" +
                       "4. Heal using: Lightning Shield\n" +
                       "6. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to fluffyhusky, sjussju , Stormchasing\n" +
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
                           // Interupts
                           Spell.CastInterupt("Wind Shear",           ret => true, "Wind Shear"),
                           // Threat
                           Buff.CastBuff("Wind Shear",                ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Wind Shear (Threat)"),
                           // Weapon enchants
                           //Buff.CastBuff("Windfury Weapon",           ret => !Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Windfury", 8232) && Item.HasSuitableWeapon(WoWInventorySlot.MainHand), "Windfury Weapon"),
                          // Buff.CastBuff("Flametongue Weapon",        ret => !Item.HasWeaponImbue(WoWInventorySlot.OffHand, "Flametongue", 8024) && Item.HasSuitableWeapon(WoWInventorySlot.OffHand), "Flametongue Weapon"),
                           // Totem management
                           Totems.CreateSetTotems(),
                           // AoE
                           new Decorator(
                               ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 2 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry),
                               new PrioritySelector(
                                   Spell.CastTotem("Magma Totem",        ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 6 && Me.Totems.All(t => t.WoWTotem != WoWTotem.Magma), "Magma Totem"),
                                   Spell.CastSpell("Chain Lightning",    ret => Buff.PlayerCountBuff("Maelstrom Weapon") == 5, "Chain Lightning"),
                                   Spell.CastSpell("Flame Shock",        ret => true, "Flame Shock"),
                                   Spell.CastSpell("Lava Lash",          ret => Buff.TargetHasDebuff("Flame Shock"), "Lava Lash"),
                                   Spell.CastSpell("Fire Nova",          ret => true, "Fire Nova"),
                                   Spell.CastSpell("Stormstrike",        ret => true, "Stormstrike")
                               )),
                           // Default Rotaion
                           // Fire Elemental removed from List, if called manually it will still not be replaced, thanks to luv4tigger for the hint
                           //Spell.CastTotem("Fire Elemental Totem",          ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Totems.All(t => t.WoWTotem != WoWTotem.FireElemental), "Fire Elemental Totem"),
                           Spell.CastTotem("Earth Elemental Totem",          ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Totems.Any(t => t.WoWTotem != WoWTotem.EarthElemental), "Earth Elemental Totem"),
                           Spell.CastTotem("Searing Totem",                  ret => Me.CurrentTarget != null && Me.CurrentTarget.Distance < Totems.GetTotemRange(WoWTotem.Searing) - 2f && !Me.Totems.Any(t => t.Unit != null && t.WoWTotem == WoWTotem.Searing && t.Unit.Location.Distance(Me.CurrentTarget.Location) < Totems.GetTotemRange(WoWTotem.Searing)) && Me.Totems.Any(t => t.WoWTotem != WoWTotem.FireElemental), "Searing Totem"),
                           Spell.CastSpell("Stormstrike",                    ret => true, "Stormstrike"),
                           Spell.CastSpell("Lava Lash",                      ret => true, "Lava Lash"),
                           Spell.CastSpell("Lightning Bolt",                 ret => Buff.PlayerCountBuff("Maelstrom Weapon") == 5 || (Item.Has4PcTeirBonus(ItemSetId) ? Buff.PlayerCountBuff("Maelstrom Weapon") == 5 : Buff.PlayerCountBuff("Maelstrom Weapon") >= 4 && (Spell.SpellCooldown("Feral Spirit").TotalSeconds > 90 && Spell.SpellOnCooldown("Feral Spirit"))), "Lightning Bolt"),
                           Spell.CastSpell("Unleash Elements",               ret => true, "Unleash Elements"),
                           Spell.CastSpell("Flame Shock",                    ret => Buff.PlayerHasBuff("Unleash Flame"), "Flame Shock (Unleash Flame)"),
                           Buff.CastDebuff("Flame Shock",                    ret => true, "Flame Shock"),
                           Spell.CastSpell("Earth Shock",                    ret => Buff.TargetDebuffTimeLeft("Flame Shock").TotalSeconds > 5, "Earth Shock"),
                           Spell.CastSpell("Feral Spirit",                   ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Unit.TimeToDeath(Me.CurrentTarget) > 30, "Feral Spirit"),
                           Spell.CastTotem("Earth Elemental Totem",          ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && Me.Totems.All(t => t.WoWTotem != WoWTotem.EarthElemental), "Earth Elemental Totem"),
                           Buff.CastBuff("Spiritwalker's Grace",             ret => Me.IsMoving, "Spiritwalker's Grace"),
                           Spell.CastSpell("Lightning Bolt",                 ret => Buff.PlayerCountBuff("Maelstrom Weapon") > 2, "Lightning Bolt"));
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
                               Item.UseBagItem("Healthstone",              ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Lightning Shield",           ret => true, "Lightning Shield"),
                               Buff.CastBuff("Shamanistic Rage",           ret => Me.CurrentTarget != null && (Me.HealthPercent < 60 || (Me.ManaPercent < 65 && Me.CurrentTarget.HealthPercent >= 75)), "Shamanistic Rage"),
                               Spell.CastSelfSpell("Healing Surge",        ret => Me.HealthPercent < 35, "Healing Surge")
                               //Buff.CastBuff("Windfury Weapon",            ret => !Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Windfury", 8232) && Item.HasSuitableWeapon(WoWInventorySlot.MainHand), "Windfury Weapon"),
                               //Buff.CastBuff("Flametongue Weapon",         ret => !Item.HasWeaponImbue(WoWInventorySlot.OffHand, "Flametongue", 8024) && Item.HasSuitableWeapon(WoWInventorySlot.OffHand), "Flametongue Weapon")
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
                               Buff.CastBuff("Lightning Shield",       ret => true, "Lightning Shield")
                              // Buff.CastBuff("Windfury Weapon",        ret => !Item.HasWeaponImbue(WoWInventorySlot.MainHand, "Windfury", 8232) && Item.HasSuitableWeapon(WoWInventorySlot.MainHand), "Windfury Weapon"),
                              // Buff.CastBuff("Flametongue Weapon",     ret => !Item.HasWeaponImbue(WoWInventorySlot.OffHand, "Flametongue", 8024) && Item.HasSuitableWeapon(WoWInventorySlot.OffHand), "Flametongue Weapon")
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
