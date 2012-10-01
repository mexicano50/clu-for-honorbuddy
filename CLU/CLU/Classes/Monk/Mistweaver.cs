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

using Styx.TreeSharp;
using CommonBehaviors.Actions;
using CLU.Helpers;
using CLU.Settings;
using System.Collections.Generic;
using CLU.Base;
using Styx.CommonBot;
using Rest = CLU.Base.Rest;
using System.Linq;


namespace CLU.Classes.Monk
{
    using Styx;

    class Mistweaver : HealerRotationBase
    {
        public override string Name
        {
            get {
                return "Mistweaver Monk";
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
                return "Soothing Mist";
            }
        }
        public override int KeySpellId
        {
            get { return 115175; }
        }
        public override float CombatMaxDistance
        {
            get {
                return 3.2f;
            }
        }

        public override string Help
        {
            get {
                return "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. \n" +
                       "==> \n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> \n" +
                       "3. \n" +
                       "4. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits: Me\n" +
                       "----------------------------------------------------------------------\n";
            }
        }       

        private static readonly List<string> JabSpellList = new List<string> { "Jab", "Club", "Slice", "Sever", "Pike", "Clobber" };

        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           //Trinkets, Racials, whatever.. I dont really care
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Item.UseEngineerGloves())),

                           //All These are cast with soothing mist. Previous heal target should have the buff
                           //If so, we cast on of these on him or we break our mist
                           Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasMyAura("Soothing Mist"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Looking for mist heals on most injured tank",
                                    Spell.CastHealSpecial("Surging Mist", a => HealthCheck(77) && HealTarget.HasMyAura("Soothing Mist"), "Surging Mist"),
                                    Spell.CastHealSpecial("Enveloping Mist", a => HealthCheck(60) && Chi >= 3 && HealTarget.HasMyAura("Soothing Mist"), "Enveloping Mist")
                           ),

                           //Save any other lives that need saving
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.ToUnit().HasMyAura("Soothing Mist"), (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Looking for mist heals on raid members",
                                    Spell.CastHealSpecial("Surging Mist", a => HealthCheck(77) && HealTarget.HasMyAura("Soothing Mist"), "Surging Mist"),
                                    Spell.CastHealSpecial("Enveloping Mist", a => HealthCheck(60) && Chi >= 3 && HealTarget.HasMyAura("Soothing Mist"), "Enveloping Mist")                                    
                           ),
                           Spell.BreakMist(),
                                   
                           //Save my life
                           Spell.CastSpell("Dampen Harm", ret => Me, ret => Me.HealthPercent < 40, "Dampen Harm  on me, emergency"),
                           Spell.CastSpell("Expel Harm", ret => Me, ret => Me.HealthPercent < 40, "Expel Harm  on me, emergency"),
                           Spell.CastSpell(JabSpellList.Find(SpellManager.CanCast), ret => Buff.PlayerHasActiveBuff("Power Strikes"), "Jab"),
                           
                           //Moar Mana
                           Spell.CastSpell("Mana Tea", ret => Me, ret => Me.ManaPercent < 85 && GotTea, "Me want moar mana! Me drink Mana Tea!"),

                           //Save Tank's life
                           Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 60, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "emergency heals on most injured tank",
                                    Spell.CastHeal("Life Cocoon", a => HealthCheck(25), "Life Cocoon"),
                                    Buff.CastHealBuff("Soothing Mist", a => true, "Soothing Mist (emergency)")
                           ),

                           //Save any other lives that need saving
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 55, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "I'm fine and tanks are not dying => ensure nobody is REALLY low life",
                                    Buff.CastHealBuff("Soothing Mist", a => true, "Soothing Mist")
                           ),

                           //Dump Chi for buffs
                           Spell.CastSpell("Blackout Kick", ret => Me.CurrentTarget != null && Chi >= 2 && Buff.PlayerCountBuff("Serpent's Zeal") < 2, "Blackout Kick"),
                           Spell.CastSpell("Tiger Palm", ret => Me.CurrentTarget != null && Buff.PlayerCountBuff("Serpent's Zeal") >= 2 && (Buff.PlayerCountBuff("Tiger Power") < 3 || Buff.PlayerCountBuff("Vital Mist") <= 5), "Tiger Palm"),
                                                    
                           //AOE
                           Healer.FindTank(a => !Buff.AnyHasMyAura("Renewing Mist"), x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && !x.ToUnit().HasAura("Renewing Mist"), (a, b) => (int)(a.MaxHealth - b.MaxHealth), "Renewing Mist on tank",
                                           Spell.CastHeal("Renewing Mist", a => true, "Renewing Mist on tank")
                           ),
                    
                           Healer.FindAreaHeal(a => true, 10, 77, 30f, (Me.GroupInfo.IsInRaid ? 3 : 2), "AOE Thunder Focus Tea: Avg: 10-75, 30yrds, count: 3 or 2",
                                    Spell.CastSpell("Thunder Focus Tea", ret => Me, ret => Chi >= 1, "Tea Popped"),
                                    Spell.CastSpell("Rushing Jade Wind", ret => Me, ret => Chi >= 1, "Rushing Jade Wind"),
                                    Spell.CastSpell("Uplift", ret => Me, ret => Chi >= 2, "Uplift"),
                                    Spell.CastSpell("Spinning Crane Kick", ret => Me.CurrentTarget, ret => Chi >= 2 && Unit.EnemyUnits.Count() > 2, "Spinning Crane Kick")
                           ),

                           //Regular Healing
                           Healer.FindTank(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 70, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Single target Tank healing",
                                    Buff.CastHealBuff("Soothing Mist", a => true, "Soothing Mist")                                   
                           ),
                           
                           Healer.FindRaidMember(a => true, x => x.ToUnit().InLineOfSight && !x.ToUnit().IsDead && x.HealthPercent < 70, (a, b) => (int)(a.CurrentHealth - b.CurrentHealth), "Single target healing",
                                    Buff.CastHealBuff("Soothing Mist", a => true, "Soothing Mist")
                           ),

                           //Build Chi
                           Spell.CastSpell(JabSpellList.Find(SpellManager.CanCast), ret => Me.CurrentTarget != null, "Jab"),
                           Spell.CastSpell("Expel Harm", ret => Me, ret => Me.HealthPercent < 40, "Expel Harm  on me, emergency"),

                           // Interupt
                           Spell.CastInterupt("Spear Hand Strike",       ret => true, "Spear Hand Strike"),
                           
                           //Nothing else going on
                           Spell.CastSpell("Spinning Crane Kick", ret => Me.CurrentTarget, ret => Chi >= 2 && Unit.EnemyUnits.Count() > 3, "Spinning Crane Kick"),
                           Spell.CastSpell(JabSpellList.Find(SpellManager.CanCast), ret => Me.CurrentTarget != null, "JabSpell"));
            }
        }

        /// <summary>
        /// We shouldnt need Pull behavior in the healing rotations so lets just return an empty priority selector.
        /// </summary>
        public override Composite Pull
        {
            get { return new PrioritySelector(); }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Buff.CastBuff("Fortifying Brew",            ret => Me.HealthPercent < 50, "Fortifying Brew"), // Turns your skin to stone, increasing your health by 20%, and reducing damage taken by 20%. Lasts 20 sec.
                               Buff.CastBuff("Guard",                      ret => Me.HealthPercent < 50, "Guard"), // absorbs damage for 30secs and increases any healing by 30%
                               Item.UseBagItem("Healthstone",              ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                             Buff.CastRaidBuff("Legacy of the Emperor", ret => true, "Legacy of the Emperor"),
                             Buff.CastRaidBuff("Legacy of the White Tiger", ret => true, "Legacy of the White Tiger")));
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