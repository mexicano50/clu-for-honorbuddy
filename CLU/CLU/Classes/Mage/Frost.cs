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

using CommonBehaviors.Actions;
using CLU.Lists;
using CLU.Settings;
using Action = Styx.TreeSharp.Action;
using CLU.Base;
using CLU.Managers;
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Mage
{

    class Frost : RotationBase
    {
        public override string Name
        {
            get
            {
                return "Frost Mage";
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
                return "Frostbolt";
            }
        }
        public override int KeySpellId
        {
            get { return 116; }
        }
        public override float CombatMinDistance
        {
            get
            {
                return 30f;
            }
        }

        // adding some help about cooldown management
        public override string Help
        {
            get
            {
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
                       "Credits to Obliv, Condemned\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        // this is used in conjuction with the CastDebuff overloaded method to cast nethertempest with Mage Bomb spell override..ikr blamae Apoc :P
        private static string Magebombtalent
        {
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
                               x => Buff.PlayerHasBuff("Invisibility"), new Action(a => CLU.TroubleshootLog("Invisibility active"))),
                            // Interupts & Steal Buffs
                           Spell.CastSpell("Spellsteal", ret => Spell.TargetHasStealableBuff() && !Me.IsMoving, "[Steal] Spellsteal"),
                            // Rotation based on SimCraft - Build 15211
                           Spell.CastInterupt("Counterspell", ret => true, "Counterspell"),
                           Item.RunMacroText("/cast Conjure Mana Gem", ret => !Item.HaveManaGem() && Me.Level > 50, "Conjure Mana Gem"),
                            //Added Frost Bomb or Nether Tempest 9-20-2012
                           Buff.CastDebuff("Mage Bomb", Magebombtalent, ret => true, "Frost Bomb or Nether Tempest"),
                           Spell.ChannelSelfSpell("Evocation",  ret => Me.ManaPercent < 40 && !Me.IsMoving && (Buff.PlayerHasActiveBuff("Icy Veins") || Buff.UnitHasHasteBuff(Me)), "Evocation"),
                           Item.UseBagItem("Mana Gem",          ret => Me.CurrentTarget != null && Me.ManaPercent < 90 && Unit.IsTargetWorthy(Me.CurrentTarget), "Mana Gem"),
                           Spell.CastSelfSpell("Cold Snap",     ret => Spell.SpellCooldown("Deep Freeze").TotalSeconds > 15 && Spell.SpellCooldown("Flame Orb").TotalSeconds > 30 && Spell.SpellCooldown("Icy Veins").TotalSeconds > 30, "Cold Snap"),
                            //Changed Fire Orb to Frozen Orb 9-20-2012
                           Spell.CastSpell("Frozen Orb",        ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Frozen Orb"),
                           Spell.CastSelfSpell("Mirror Image",  ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Mirror Image"),
                           Spell.CastSelfSpell("Icy Veins",     ret => !Buff.PlayerHasActiveBuff("Icy Veins") && !Buff.UnitHasHasteBuff(Me) && (Buff.PlayerCountBuff("Stolen Time") > 7 || Spell.SpellCooldown("Cold Snap").TotalSeconds < 22), "Icy Veins"),
                           Spell.CastSpell("Deep Freeze",       ret => Buff.PlayerHasActiveBuff("Fingers of Frost"), "Deep Freeze (Fingers of Frost)"),
                           Spell.CastSpell("Frostfire Bolt",    ret => Buff.PlayerHasActiveBuff("Brain Freeze"), "Frostfire Bolt (Brain Freeze)"),
                            // AoE
                           new Decorator(
                               ret => Me.CurrentTarget != null && !Me.IsMoving && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 2,
                               new PrioritySelector(
                                   Spell.CastOnUnitLocation("Flamestrike", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Flamestrike") && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Flamestrike"),
                                   Spell.ChannelAreaSpell("Blizzard", 11.0, true, 4, 0.0, 0.0, ret => Me.CurrentTarget != null && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Blizzard")
                               )),
                           PetManager.CastPetSpellAtLocation("Freeze", u => Me.CurrentTarget, ret => true, "Freeze (Pet)"),
                           Spell.CastSpell("Ice Lance",         ret => Buff.PlayerCountBuff("Fingers of Frost") > 1, "Ice Lance"),
                            // PetSpellCooldown NOT returning correctly although it does return the correct value within the method, when you call it here it returns zero for some reason...still investigating
                            // K    ice_lance,if=buff.fingers_of_frost.react&pet.water_elemental.cooldown.freeze.remains<gcd
                           Spell.CastSpell("Ice Lance",         ret => Buff.PlayerHasActiveBuff("Fingers of Frost") && (PetManager.PetSpellCooldown("Freeze").TotalSeconds < Spell.GCD), "Ice Lance (water_elemental_Freeze=" + PetManager.PetSpellCooldown("Freeze").TotalSeconds + " < " + Spell.GCD + ")"),
                           Spell.CastSpell("Frostbolt",         ret => true, "Frostbolt (FreezeCD=" + PetManager.PetSpellCooldown("Freeze").TotalSeconds + ")"),
                           Spell.CastSpell("Ice Lance",         ret => Me.IsMoving, "Ice Lance (Moving)"),
                           Spell.CastSpell("Fire Blast",        ret => Me.IsMoving, "Fire Blast (Moving)")
                       );
            }
        }

        public override Composite Pull
        {
             get { return this.SingleRotation; }
        }

        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                    PetManager.CastPetSummonSpell("Summon Water Elemental", ret => !Me.GotAlivePet, "Calling Pet Water Elemental"),
                    Buff.CastBuff("Frost Armor", ret => true, "Frost Armor"),
                    
                    new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Ice Block", ret => Me.HealthPercent < 20 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Ice Block"),
                               Buff.CastBuff("Mage Ward", ret => Me.HealthPercent < 50, "Mage Ward")))
                    
                    ); 
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               PetManager.CastPetSummonSpell("Summon Water Elemental", ret => !Me.GotAlivePet, "Calling Pet Water Elemental"),
                               Buff.CastBuff("Frost Armor", ret => true, "Frost Armor"),
                               //Buff.CastRaidBuff("Dalaran Brilliance",         ret => true, "Dalaran Brilliance"), //Commentet out as it is of no real importance except for 10yrd extra range.
                               Buff.CastRaidBuff("Arcane Brilliance", ret => true, "Arcane Brilliance"),
                               Item.RunMacroText("/cast Conjure Mana Gem", ret => !Me.IsMoving && !Item.HaveManaGem() && Me.Level > 50, "Conjure Mana Gem")));
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