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

// This class was directly taken from Singular and full credit goes to the developers...lets not re-invent the wheel.


using CLU.Helpers;

namespace CLU.Base
{

    using Styx;
    using Styx.CommonBot;
    using Styx.CommonBot.Inventory;
    using Styx.CommonBot.POI;
    using Styx.Pathing;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using System.Linq;
    using CommonBehaviors.Actions;
    using Styx.TreeSharp;
    using global::CLU.Settings;

    internal static class Rest
    {

        private static bool CorpseAround
        {
            get {
                return ObjectManager.GetObjectsOfType<WoWUnit>(true, false).Any(
                           u => u.DistanceSqr < 5 * 5 && u.IsDead &&
                           (u.CreatureType == WoWCreatureType.Humanoid || u.CreatureType == WoWCreatureType.Undead));
            }
        }

        private static bool PetInCombat
        {
            get {
                return StyxWoW.Me.GotAlivePet && StyxWoW.Me.PetInCombat;
            }
        }

        public static Composite CreateDefaultRestBehaviour()
        {
            return

                // Don't fucking run the rest behavior (or any other) if we're dead or a ghost. Thats all.
                new Decorator(
                    ret => !StyxWoW.Me.IsDead && !StyxWoW.Me.IsGhost && !StyxWoW.Me.IsCasting && CLUSettings.Instance.EnableMovement && BotPoi.Current.Type != PoiType.Loot,
                    new PrioritySelector(
                        // Make sure we wait out res sickness. Fuck the classes that can deal with it. :O
                        new Decorator(
                            ret => StyxWoW.Me.HasAura("Resurrection Sickness"),
                            new Action(ret => { })),
                        // Wait while cannibalizing
                        new Decorator(
                            ret => StyxWoW.Me.CastingSpell != null && StyxWoW.Me.CastingSpell.Name == "Cannibalize" &&
                            (StyxWoW.Me.HealthPercent < 95 || (StyxWoW.Me.PowerType == WoWPowerType.Mana && StyxWoW.Me.ManaPercent < 95)),
                            new Sequence(
                                new Action(ret => CLULogger.Log("Waiting for Cannibalize")),
                                new ActionAlwaysSucceed())),
                        // Cannibalize support goes before drinking/eating
                        new Decorator(
                            ret =>
                            (StyxWoW.Me.HealthPercent <= CLUSettings.Instance.MinHealth ||
                             (StyxWoW.Me.PowerType == WoWPowerType.Mana && StyxWoW.Me.ManaPercent <= CLUSettings.Instance.MinMana)) &&
                            SpellManager.CanCast("Cannibalize") && CorpseAround,
                            new Sequence(
                                new Action(ret => Navigator.PlayerMover.MoveStop()),
                                Spell.CreateWaitForLagDuration(),
                                new Action(ret => SpellManager.Cast("Cannibalize")),
                                new WaitContinue(1, ret => false, new ActionAlwaysSucceed()))),
                        // Check if we're allowed to eat (and make sure we have some food. Don't bother going further if we have none.
                        new Decorator(
                            ret =>
                            !StyxWoW.Me.IsSwimming && StyxWoW.Me.HealthPercent <= CLUSettings.Instance.MinHealth && !StyxWoW.Me.HasAura("Food") &&
                            Consumable.GetBestFood(false) != null && !StyxWoW.Me.IsCasting,
                            new PrioritySelector(
                                new Decorator(
                                    ret => StyxWoW.Me.IsMoving,
                                    new Action(ret => Navigator.PlayerMover.MoveStop())),
                                new Sequence(
                                    new Action(
                            			ret => {
                            				Styx.CommonBot.Rest.FeedImmediate();
                            			}),
                                    Spell.CreateWaitForLagDuration()))),
                        // Make sure we're a class with mana, if not, just ignore drinking all together! Other than that... same for food.
                        new Decorator(
                            ret =>
                                                                                                   // TODO: Does this Druid check need to be still in here?
                            !StyxWoW.Me.IsSwimming && (StyxWoW.Me.PowerType == WoWPowerType.Mana || StyxWoW.Me.Class == WoWClass.Druid) &&
                            StyxWoW.Me.ManaPercent <= CLUSettings.Instance.MinMana &&
                            !StyxWoW.Me.HasAura("Drink") && Consumable.GetBestDrink(false) != null && !StyxWoW.Me.IsCasting,
                            new PrioritySelector(
                                new Decorator(
                                    ret => StyxWoW.Me.IsMoving,
                                    new Action(ret => Navigator.PlayerMover.MoveStop())),
                                new Sequence(
                            		new Action(ret => {
                            		           	Styx.CommonBot.Rest.DrinkImmediate();
                            		           }),
                                    Spell.CreateWaitForLagDuration()))),
                        // This is to ensure we STAY SEATED while eating/drinking. No reason for us to get up before we have to.
                        new Decorator(
                            ret =>
                            (StyxWoW.Me.HasAura("Food") && StyxWoW.Me.HealthPercent < 95) ||
                            (StyxWoW.Me.HasAura("Drink") && StyxWoW.Me.PowerType == WoWPowerType.Mana && StyxWoW.Me.ManaPercent < 95),
                            new ActionAlwaysSucceed()),
                        new Decorator(
                            ret =>
                            ((StyxWoW.Me.PowerType == WoWPowerType.Mana && StyxWoW.Me.ManaPercent <= CLUSettings.Instance.MinMana) ||
                             StyxWoW.Me.HealthPercent <= CLUSettings.Instance.MinHealth) && !StyxWoW.Me.CurrentMap.IsBattleground,
                            new Action(ret => CLULogger.Log("We have no food/drink. Waiting to recover our health/mana back")))
                    ));
        }

    }
}
