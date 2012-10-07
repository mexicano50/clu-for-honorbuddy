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
using Styx.TreeSharp;

using CommonBehaviors.Actions;
using CLU.Settings;
using Action = Styx.TreeSharp.Action;
using CLU.Base;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Mage
{
    using global::CLU.Managers;

    class Arcane : RotationBase
    {
        public override string Name
        {
            get {
                return "Arcane Mage";
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
                return "Arcane Blast";
            }
        }
        public override int KeySpellId
        {
            get { return 30451; }
        }
        public override float CombatMinDistance
        {
            get {
                return 30f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get {
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       "1. Ice Block, Spellsteal\n" +
                       "2. Conjure Mana Gem\n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Flame Orb & Mana Gem & Mirror Image & Arcane Power \n" +
                       "4. AoE with Arcane Explosion & Frost Nova \n" +
                       "5. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        // this is used in conjuction with the CastDebuff overloaded method to cast nethertempest with Mage Bomb spell override..ikr blamae Apoc :P
        private static string Magebombtalent { 
            get
            {
                if (TalentManager.HasTalent(13)) return "Nether Tempest";
                if (TalentManager.HasTalent(14)) return "Living Bomb";
                if (TalentManager.HasTalent(15)) return "Frost Bomb";
                return "Mage Bomb";
            }
        }

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                    new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                    // For DS Encounters.
                    EncounterSpecific.ExtraActionButton(),

                    new Decorator(
                        ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                        new PrioritySelector(
                            Item.UseTrinkets(),
                            Racials.UseRacials(),
                            Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                            Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                            Item.UseEngineerGloves())),
                    // Comment: Dont break Invinsibility!!
                    new Decorator(
                        x => Buff.PlayerHasBuff("Invisibility"),
                        new Action(a => CLULogger.TroubleshootLog("Invisibility active"))),
                    // Interupts & Steal Buffs
                    Spell.CastSpell("Spellsteal", ret => Spell.TargetHasStealableBuff() && !Me.IsMoving, "[Steal] Spellsteal"),
                    Spell.CastInterupt("Counterspell", ret => true, "Counterspell"),
                    // Cooldowns
                    Spell.ChannelSelfSpell("Evocation", ret => Me.ManaPercent < 35 && !Me.IsMoving, "Evocation"),
                    Item.UseBagItem("Mana Gem", ret => Me.CurrentTarget != null && Me.ManaPercent < 90 && Unit.IsTargetWorthy(Me.CurrentTarget), "Mana Gem"),
                    Item.UseBagItem("Brilliant Mana Gem", ret => Me.CurrentTarget != null && Me.ManaPercent < 90 && Unit.IsTargetWorthy(Me.CurrentTarget), "Brilliant Mana Gem"),
                    Spell.CastSelfSpell("Mirror Image", ret => Buff.PlayerHasBuff("Arcane Power") && Unit.IsTargetWorthy(Me.CurrentTarget), "Mirror Image"),
                    Spell.CastSelfSpell("Presence of Mind", ret => !Buff.PlayerHasBuff("Invisibility"), "Presence of Mind"),
                    Spell.CastSelfSpell("Arcane Power", ret => Me.CurrentTarget != null && Buff.PlayerHasBuff("Improved Mana Gem") || Unit.IsTargetWorthy(Me.CurrentTarget), "Arcane Power"),
                    Item.RunMacroText("/cast Conjure Mana Gem",ret => Buff.PlayerHasBuff("Presence of Mind") && !Item.HaveManaGem() && Me.Level > 50,"Conjure Mana Gem"),
                    // AoE
                    new Decorator(ret => CLUSettings.Instance.UseAoEAbilities,
                        new PrioritySelector(

                            Spell.CastSpell("Fire Blast", ret => Me.CurrentTarget.HasMyAura("Mage Bomb") && Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 10f, false).Count(a => !a.IsFriendly && a.Combat) >= 3, "Fire Blast (Spreading the news...)"),
                            Spell.CastSpell("Mage Bomb", ret => Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 10f, false).Count(a => !a.IsFriendly && a.Combat) >= 3, "Frost Bomb"),
                            Spell.CastOnUnitLocation("Flametrike", ret => Me.CurrentTarget, ret => Unit.NearbyNonControlledUnits(Me.CurrentTarget.Location, 10f, false).Count(a => !a.IsFriendly && a.Combat) >= 3, "Flamestrike"),
                            Spell.CastSpell("Arcane Explosion", ret => Unit.NearbyNonControlledUnits(Me.Location, 10f, false).Count(a => !a.IsFriendly && a.Combat) >= 3, "Arcane Explsion")

                        )
                    ),
                    //Damage
                    //Tier5 Talent
                    Buff.CastDebuff("Mage Bomb", Magebombtalent, ret => true, "Frost/Living Bomb or Nether Tempest"),
                    Spell.CastSpell("Arcane Missiles", ret => Buff.GetAuraStack(Me, "Arcane Missiles!", true) == 2 || Buff.GetAuraStack(Me, "Arcane Charge", true) >= 5, "Arcane Missles"),
                    Spell.CastSpell("Arcane Barrage", ret => Buff.GetAuraStack(Me, "Arcane Charge", true) > 4, "Arcane Barrage"),
                    Spell.CastSpell("Arcane Blast", ret => Buff.GetAuraStack(Me, "Arcane Charge", true) < 6,"Arcane Blast")
                    );
            }
        }

        public override Composite Pull
        {
             get { return this.SingleRotation; }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone",      ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Ice Block",          ret => Me.HealthPercent < 20 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Ice Block"),
                               Buff.CastBuff("Mage Ward",          ret => Me.HealthPercent < 50, "Mage Ward")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Buff.CastBuff("Frost Armor",                     ret => true, "Frost Armor"),
                    //Buff.CastRaidBuff("Dalaran Brilliance",         ret => true, "Dalaran Brilliance"), //Commentet out as it is of no real importance except for 10yrd extra range.
                               Buff.CastRaidBuff("Arcane Brilliance",          ret => true, "Arcane Brilliance"),
                               Item.RunMacroText("/cast Conjure Mana Gem", ret => !Me.IsMoving && !Item.HaveManaGem() && Me.Level > 50, "Conjure Mana Gem")));
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
