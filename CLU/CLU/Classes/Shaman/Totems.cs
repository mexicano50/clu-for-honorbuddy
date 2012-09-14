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

namespace CLU.Classes.Shaman
{
    using System.Collections.Generic;
    using System.Linq;
    using Styx;
    using Styx.Combat.CombatRoutine;
    //using Styx.Logic.Combat;
    using Styx.CommonBot;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;
    using Styx.TreeSharp;
    using CommonBehaviors.Actions;
    using global::CLU.Base;
    using global::CLU.Managers;
    using global::CLU.Settings;

    internal static class Totems
    {

        private static readonly Dictionary<MultiCastSlot, WoWTotem> LastSetTotems = new Dictionary<MultiCastSlot, WoWTotem>();

        public static Composite CreateSetTotems()
        {
            return new PrioritySelector(

                       // gtfo if the user dosnt want us to mess with his totems.
                       new Decorator(ret => !CLUSettings.Instance.Shaman.HandleTotems, new ActionAlwaysSucceed()),

                       new Decorator(
                           ret => !SpellManager.HasSpell("Call of the Elements"),
                           new PrioritySelector(
                               new PrioritySelector(
                                   ctx => StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == GetEarthTotem()),
                                   new Decorator(
                                       ret => GetEarthTotem() != WoWTotem.None && (ret == null || ((WoWTotemInfo)ret).Unit == null ||
                                               ((WoWTotemInfo)ret).Unit.Distance > GetTotemRange(GetEarthTotem())),
                                       new Sequence(
                                           new Action(ret => CLU.Log("Casting {0} Totem", GetEarthTotem().ToString().CamelToSpaced())),
                                           new Action(ret => SpellManager.Cast(GetEarthTotem().GetTotemSpellId()))))),
                               new PrioritySelector(
                                   ctx => StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == GetAirTotem()),
                                   new Decorator(
                                       ret => GetAirTotem() != WoWTotem.None && (ret == null || ((WoWTotemInfo)ret).Unit == null ||
                                               ((WoWTotemInfo)ret).Unit.Distance > GetTotemRange(GetAirTotem())),
                                       new Sequence(
                                           new Action(ret => CLU.Log("Casting {0} Totem", GetAirTotem().ToString().CamelToSpaced())),
                                           new Action(ret => SpellManager.Cast(GetAirTotem().GetTotemSpellId()))))),
                               new PrioritySelector(
                                   ctx => StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == GetWaterTotem()),
                                   new Decorator(
                                       ret => GetWaterTotem() != WoWTotem.None && (ret == null || ((WoWTotemInfo)ret).Unit == null ||
                                               ((WoWTotemInfo)ret).Unit.Distance > GetTotemRange(GetWaterTotem())),
                                       new Sequence(
                                           new Action(ret => CLU.Log("Casting {0} Totem", GetWaterTotem().ToString().CamelToSpaced())),
                                           new Action(ret => SpellManager.Cast(GetWaterTotem().GetTotemSpellId())))))
                               //new PrioritySelector(
                               //    ctx => StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == GetFireTotem()),
                               //    new Decorator(
                               //        ret => GetFireTotem() != WoWTotem.None && (ret == null || ((WoWTotemInfo)ret).Unit == null ||
                               //                ((WoWTotemInfo)ret).Unit.Distance > GetTotemRange(GetFireTotem())),
                               //        new Sequence(
                               //            new Action(ret => CLU.Log("Casting {0} Totem", GetFireTotem().ToString().CamelToSpaced())),
                               //            new Action(ret => SpellManager.Cast(GetFireTotem().GetTotemSpellId())))))
                           )),
                       new Decorator(
            ret => {
                // Hell yeah this is long, but its all clear to read
                if (!SpellManager.HasSpell("Call of the Elements"))
                    return false;

                // Air Checks
                var bestAirTotem = GetAirTotem();
                var currentAirTotem = StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == bestAirTotem);

                if (currentAirTotem == null)
                {
                    return true;
                }

                var airTotemAsUnit = currentAirTotem.Unit;
                if (airTotemAsUnit == null)
                {
                    return true;
                }

                if (airTotemAsUnit.Distance > GetTotemRange(bestAirTotem))
                {
                    return true;
                }

                // Earth Checks
                var bestEarthTotem = GetEarthTotem();
                var currentEarthTotem = StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == bestEarthTotem);

                if (currentEarthTotem == null)
                {
                    return true;
                }

                var earthTotemAsUnit = currentEarthTotem.Unit;
                if (earthTotemAsUnit == null)
                {
                    return true;
                }

                if (earthTotemAsUnit.Distance > GetTotemRange(bestEarthTotem))
                {
                    return true;
                }

                //Water Checks
                var bestWaterTotem = GetWaterTotem();
                var currentWaterTotem = StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == bestWaterTotem);

                if (currentWaterTotem == null)
                {
                    return true;
                }

                var waterTotemAsUnit = currentWaterTotem.Unit;
                if (waterTotemAsUnit == null)
                {
                    return true;
                }

                if (waterTotemAsUnit.Distance > GetTotemRange(bestWaterTotem))
                {
                    return true;
                }

                // Fire Checks
                //var bestFireTotem = GetFireTotem();
                //var currentFireTotem = StyxWoW.Me.Totems.FirstOrDefault(t => t.WoWTotem == bestFireTotem);

                //if (currentFireTotem == null)
                //{
                //    return true;
                //}

                //var fireTotemAsUnit = currentFireTotem.Unit;
                //if (fireTotemAsUnit == null)
                //{
                //    return true;
                //}

                //if (fireTotemAsUnit.Distance > GetTotemRange(bestFireTotem))
                //{
                //    return true;
                //}

                return false;
            },
            new Sequence(
                new Action(ret => SetupTotemBar()),
                Spell.CastSelfSpell("Call of the Elements", ret => true, "Call of the Elements"))));
        }

        public static void SetupTotemBar()
        {
            // If the user has given specific totems to use, then use them. Otherwise, fall back to our automagical ones
            WoWTotem earth;
            WoWTotem air;
            WoWTotem water;
            // WoWTotem fire;

            switch (TalentManager.CurrentSpec) {
            case WoWSpec.ShamanElemental:
                earth = CLUSettings.Instance.Shaman.ElementalEarthTotem;
                air = CLUSettings.Instance.Shaman.ElementalAirTotem;
                water = CLUSettings.Instance.Shaman.ElementalWaterTotem;
                // fire = CLUSettings.Instance.Shaman.ElementalFireTotem;
                break;
            case WoWSpec.ShamanEnhancement:
                earth = CLUSettings.Instance.Shaman.EnhancementEarthTotem;
                air = CLUSettings.Instance.Shaman.EnhancementAirTotem;
                water = CLUSettings.Instance.Shaman.EnhancementWaterTotem;
                //fire = CLUSettings.Instance.Shaman.EnhancementFireTotem;
                break;
            case WoWSpec.ShamanRestoration:
                earth = CLUSettings.Instance.Shaman.RestorationEarthTotem;
                air = CLUSettings.Instance.Shaman.RestorationAirTotem;
                water = CLUSettings.Instance.Shaman.RestorationWaterTotem;
                //fire = CLUSettings.Instance.Shaman.RestorationFireTotem;
                break;
            default:
                earth = WoWTotem.None;
                air = WoWTotem.None;
                water = WoWTotem.None;
                //fire = WoWTotem.None;
                break;
            }

            SetTotemBarSlot(MultiCastSlot.ElementsEarth, earth != WoWTotem.None ? earth : GetEarthTotem());
            SetTotemBarSlot(MultiCastSlot.ElementsAir, air != WoWTotem.None ? air : GetAirTotem());
            SetTotemBarSlot(MultiCastSlot.ElementsWater, water != WoWTotem.None ? water : GetWaterTotem());
            SetTotemBarSlot(MultiCastSlot.ElementsFire, StyxWoW.Me.Totems.All(t => t.WoWTotem == WoWTotem.FireElemental) ? WoWTotem.FireElemental : WoWTotem.None);
            //SetTotemBarSlot(MultiCastSlot.ElementsFire, fire != WoWTotem.None ? fire : GetFireTotem());
        }

        /// <summary>
        ///   Recalls any currently 'out' totems. This will use Totemic Recall if its known, otherwise it will destroy each totem one by one.
        /// </summary>
        /// <remarks>
        ///   Created 3/26/2011.
        /// </remarks>
        public static void RecallTotems()
        {
            //CLU.Log("Recalling totems!");
            if (SpellManager.HasSpell("Totemic Recall")) {
                Spell.CastMySpell("Totemic Recall");
                return;
            }

            List<WoWTotemInfo> totems = StyxWoW.Me.Totems;
            foreach (WoWTotemInfo t in totems) {
                if (t != null && t.Unit != null) {
                    DestroyTotem(t.Type);
                }
            }
        }

        /// <summary>
        ///   Destroys the totem described by type.
        /// </summary>
        /// <remarks>
        ///   Created 3/26/2011.
        /// </remarks>
        /// <param name = "type">The type.</param>
        public static void DestroyTotem(WoWTotemType type)
        {
            if (type == WoWTotemType.None) {
                return;
            }

            Lua.DoString("DestroyTotem({0})", (int)type);
        }

        /// <summary>
        ///   Sets a totem bar slot to the specified totem!.
        /// </summary>
        /// <remarks>
        ///   Created 3/26/2011.
        /// </remarks>
        /// <param name = "slot">The slot.</param>
        /// <param name = "totem">The totem.</param>
        public static void SetTotemBarSlot(MultiCastSlot slot, WoWTotem totem)
        {
            // Make sure we have the totem bars to set. Highest first kthx
            if (slot >= MultiCastSlot.SpiritsFire && !SpellManager.HasSpell("Call of the Spirits")) {
                return;
            }
            if (slot >= MultiCastSlot.AncestorsFire && !SpellManager.HasSpell("Call of the Ancestors")) {
                return;
            }
            if (!SpellManager.HasSpell("Call of the Elements")) {
                return;
            }

            if (LastSetTotems.ContainsKey(slot) && LastSetTotems[slot] == totem) {
                return;
            }

            if (!LastSetTotems.ContainsKey(slot)) {
                LastSetTotems.Add(slot, totem);
            } else {
                LastSetTotems[slot] = totem;
            }

            CLU.Log("Setting totem slot Call of the" + slot.ToString().CamelToSpaced() + " to " + totem.ToString().CamelToSpaced());

            Lua.DoString("SetMultiCastSpell({0}, {1})", (int)slot, totem.GetTotemSpellId());
        }

        private static WoWTotem GetEarthTotem()
        {
            if (TalentManager.CurrentSpec == WoWSpec.None)
            {
                return WoWTotem.None;
            }
            if (TotemIsKnown(WoWTotem.EarthElemental))
            {
                return WoWTotem.EarthElemental;
            }
            if (TotemIsKnown(WoWTotem.Earthbind))
            {
                return WoWTotem.Earthbind;
            }
            if (TotemIsKnown(WoWTotem.Earthgrab))
            {
                return WoWTotem.Earthgrab;
            }
            if (TotemIsKnown(WoWTotem.Tremor))
            {
                return WoWTotem.Tremor;
            }

            return WoWTotem.None;
        }

        public static WoWTotem GetAirTotem()
        {
            if (TalentManager.CurrentSpec == WoWSpec.None)
            {
                return WoWTotem.None;
            }

            if (TotemIsKnown(WoWTotem.Stormlash)) 
            {
                return WoWTotem.Stormlash;
            }
            if (TotemIsKnown(WoWTotem.Windwalk))
            {
                return WoWTotem.Windwalk;
            } 
            if (TotemIsKnown(WoWTotem.Grounding))
            {
                return WoWTotem.Grounding;
            }
            if (TotemIsKnown(WoWTotem.Capacitor))
            {
                return WoWTotem.Capacitor;
            }
            return WoWTotem.None;
        }

        public static WoWTotem GetWaterTotem()
        {
            switch (TalentManager.CurrentSpec) {
                case WoWSpec.ShamanElemental:
                if (CLUSettings.Instance.Shaman.ElementalWaterTotem != WoWTotem.None) return CLUSettings.Instance.Shaman.ElementalWaterTotem;
                break;
                case WoWSpec.ShamanEnhancement:
                if (CLUSettings.Instance.Shaman.EnhancementWaterTotem != WoWTotem.None) return CLUSettings.Instance.Shaman.EnhancementWaterTotem;
                break;
                case WoWSpec.ShamanRestoration:
                if (CLUSettings.Instance.Shaman.RestorationWaterTotem != WoWTotem.None) return CLUSettings.Instance.Shaman.RestorationWaterTotem;
                break;
            }

            // Plain and simple. If we're resto, we never want a different water totem out. Thats all there is to it.
            if (TalentManager.CurrentSpec == WoWSpec.ShamanRestoration)
            {
                if (TotemIsKnown(WoWTotem.HealingStream)) {
                    return WoWTotem.HealingStream;
                }

                return WoWTotem.None;
            }

            // Solo play. Only healing stream
            if (!StyxWoW.Me.IsInParty && !StyxWoW.Me.IsInRaid) {
                if (TotemIsKnown(WoWTotem.HealingStream)) {
                    return WoWTotem.HealingStream;
                }

                return WoWTotem.None;
            }

            if (TotemIsKnown(WoWTotem.HealingTide))
            {
                return WoWTotem.HealingTide;
            }

            // ... yea
            if (TotemIsKnown(WoWTotem.HealingStream)) {
                return WoWTotem.HealingStream;
            }

            return WoWTotem.None;
        }

        public static WoWTotem GetFireTotem()
        {
            if (TalentManager.CurrentSpec == WoWSpec.None)
            {
                return WoWTotem.None;
            }

            switch (TalentManager.CurrentSpec) {
                case WoWSpec.ShamanElemental:
                // if (CLUSettings.Instance.Shaman.ElementalFireTotem != WoWTotem.None) return CLUSettings.Instance.Shaman.ElementalFireTotem;
                break;
                case WoWSpec.ShamanEnhancement:
                //if (CLUSettings.Instance.Shaman.EnhancementFireTotem != WoWTotem.None) return CLUSettings.Instance.Shaman.EnhancementFireTotem;
                break;
            case WoWSpec.ShamanRestoration:
                // if (CLUSettings.Instance.Shaman.RestorationFireTotem != WoWTotem.None) return CLUSettings.Instance.Shaman.RestorationFireTotem;
                break;
            }

            bool isEnhance = TalentManager.CurrentSpec == WoWSpec.ShamanEnhancement;
            bool isElemental = TalentManager.CurrentSpec == WoWSpec.ShamanElemental;
            // Enhance || Elemental
            if (isEnhance || isElemental) {
                if (TotemIsKnown(WoWTotem.Searing)) {
                    return WoWTotem.Searing;
                }

                return WoWTotem.None;
            }

            // Well..Flametongue seems a good choice
            return TotemIsKnown(WoWTotem.Magma) ? WoWTotem.Magma : WoWTotem.FireElemental;
        }

        #region Helper shit

        public static bool NeedToRecallTotems
        {
            get {
                return TotemsInRange == 0 && StyxWoW.Me.Totems.Count(t => t.Unit != null) != 0;
            }
        }
        public static int TotemsInRange
        {
            get {
                return TotemsInRangeOf(StyxWoW.Me);
            }
        }

        public static int TotemsInRangeOf(WoWUnit unit)
        {
            return StyxWoW.Me.Totems.Where(t => t.Unit != null).Count(t => unit.Location.Distance(t.Unit.Location) < GetTotemRange(t.WoWTotem));
        }

        public static bool TotemIsKnown(WoWTotem totem)
        {
            return SpellManager.HasSpell(totem.GetTotemSpellId());
        }

        /// <summary>
        ///   Finds the max range of a specific totem, where you'll still receive the buff.
        /// </summary>
        /// <remarks>
        ///   Created 3/26/2011.
        /// </remarks>
        /// <param name = "totem">The totem.</param>
        /// <returns>The calculated totem range.</returns>
        public static float GetTotemRange(WoWTotem totem)
        {
            switch (totem) {
                case WoWTotem.Windwalk:
                case WoWTotem.HealingTide:
                case WoWTotem.HealingStream:
                return 40f;

                case WoWTotem.Stormlash:
                case WoWTotem.Tremor:
                return 30f;

                case WoWTotem.Searing:
                return 20f;

                case WoWTotem.Earthbind:
                return 10f;

                case WoWTotem.Grounding:
                case WoWTotem.Capacitor:
                case WoWTotem.Magma:
                return 8f;

                case WoWTotem.EarthElemental:
                case WoWTotem.FireElemental:
                // Not really sure about these 3.
                return 20f;
                case WoWTotem.ManaTide:
                // Again... not sure :S
                return 30f;
            }
            return 0f;
        }

        #endregion

        #region Nested type: MultiCastSlot

        /// <summary>
        ///   A small enum to make specifying specific totem bar slots easier.
        /// </summary>
        /// <remarks>
        ///   Created 3/26/2011.
        /// </remarks>
        internal enum MultiCastSlot {
            // Enums increment by 1 after the first defined value. So don't touch this. Its the way it is for a reason.
            // If these numbers change in the future, feel free to fill this out completely. I'm too lazy to do it - Apoc
            //
            // Note: To get the totem 'slot' just do MultiCastSlot & 3 - will return 0-3 for the totem slot this is for.
            // I'm not entirely sure how WoW shows which ones are 'current' in the slot, so we'll just set it up for ourselves
            // and remember which is which.
            ElementsFire = 133,
            ElementsEarth,
            ElementsWater,
            ElementsAir,

            AncestorsFire,
            AncestorsEarth,
            AncestorsWater,
            AncestorsAir,

            SpiritsFire,
            SpiritsEarth,
            SpiritsWater,
            SpiritsAir
        }

        #endregion
    }
}