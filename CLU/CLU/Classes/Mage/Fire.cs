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
using Rest = CLU.Base.Rest;

namespace CLU.Classes.Mage
{
    using global::CLU.Managers;

    class Fire : RotationBase
    {
        public override string Name
        {
            get
            {
                return "Fire Mage";
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
                return "Pyroblast";
            }
        }
        public override int KeySpellId
        {
            get { return 11366; }
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
                       "==> Flame Orb & Mana Gem & Mirror Image & Combustion \n" +
                       "4. AoE with Blast Wave & Dragon's Breath & Fire Blast & Flamestrike\n" +
                       "5. Best Suited for end game raiding\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to jamjar0207\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        private Composite SkipFireball()
        {
            return Spell.StopCast(ret => (Me.HasMyAura("Heating Up") || Me.HasMyAura("Pyroblast!")) && Me.CastingSpell.Id == 133, "Stop casting Fireball, we have a proc!");
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
                                ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                                new PrioritySelector(
                                        Item.UseTrinkets(),
                                        Racials.UseRacials(),
                                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                                        Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                                        Item.UseEngineerGloves())),

                            // Comment: Dont break Invinsibility!!
                            new Decorator(x => Buff.PlayerHasBuff("Invisibility"), new Action(a => CLULogger.TroubleshootLog("Invisibility active"))),
                    // Interupts & Steal Buffs
                            Spell.CastSpell("Spellsteal", ret => Spell.TargetHasStealableBuff() && !Me.IsMoving, "[Steal] Spellsteal"),
                            Spell.CastInterupt("Counterspell", ret => true, "Counterspell"),
                            Spell.CastSpell("Blazing Speed", unit => Me, ret => Me.MovementInfo.ForwardSpeed < 8.05 && Me.IsMoving && TalentManager.HasTalent(5) && CLUSettings.Instance.EnableMovement, "Blazing Speed"),
                    // Cooldowns
                            new Decorator(ret => CLUSettings.Instance.UseCooldowns,
                                new PrioritySelector(
                                    Spell.ChannelSelfSpell("Evocation", ret => Me.ManaPercent < 35 && !Me.IsMoving, "Evocation"),
                                    Spell.ChannelSelfSpell("Evocation", ret => TalentManager.HasTalent(16) && Unit.UseCooldowns() && !Buff.PlayerHasActiveBuff("Invoker's Energy") && !Me.IsMoving, "Evocation"),
                            Item.UseBagItem("Mana Gem", ret => Me.CurrentTarget != null && Me.ManaPercent < 90, "Mana Gem"),
                            Item.UseBagItem("Brilliant Mana Gem", ret => Me.CurrentTarget != null && Me.ManaPercent < 90, "Brilliant Mana Gem"),
                            Spell.CastSelfSpell("Mirror Image", ret => true, "Mirror Image"),
                            Spell.CastSelfSpell("Presence of Mind", ret => !Me.HasMyAura("Pyroblast!"), "Presence of Mind"),
                            Spell.CastSpell("Pyroblast", ret => Me.HasMyAura("Presence of Mind") || Me.HasMyAura("Pyroblast!"), "Pyroblast with PoM or Pyroblast!"),
                            Spell.CastSelfSpell("Arcane Power", ret => Me.CurrentTarget != null, "Arcane Power"))),
                            Spell.CastSpell(759, ret => Spell.CanCast("Conjure Mana Gem") && Buff.PlayerHasBuff("Presence of Mind") && !Item.HaveManaGem() && Me.Level > 50, "Conjure Mana Gem"),
                    // Rune of Power
                            Spell.CastOnUnitLocation("Rune of Power", unit => Me, ret => !Buff.PlayerHasBuff("Rune of Power") && TalentManager.HasTalent(17), "Rune of Power"),
                    // AoE
                            new Decorator(
                               ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) >= CLUSettings.Instance.BurstOnMobCount && CLUSettings.Instance.UseAoEAbilities,
                               new PrioritySelector(
                                   Spell.CastOnUnitLocation("Flamestrike", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Flamestrike") && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Flamestrike"),
                                   Spell.CastConicSpell("Dragon's Breath", 12f, 33f, ret => !Me.HasMyAura("Heating Up") && !Me.HasMyAura("Pyroblast!"), "Dragon's Breath")
                               )),
                    // Default Rotaion
                    //Tier5 Talent
                            Spell.CastSpell("Combustion", ret => CLUSettings.Instance.Mage.EnableCombustion && Buff.TargetHasDebuff("Ignite") && Buff.TargetHasDebuff("Pyroblast") && Unit.UseCooldowns(), "Combustion"),
                            Spell.CastSelfSpell("Presence of Mind", ret => Unit.UseCooldowns() && Me.HasMyAura("Pyroblast!"), "Presence of Mind"),
                            Buff.CastBuff("Alter Time", ret => Unit.UseCooldowns() && Me.HasMyAura("Pyroblast!"), "Alter Time"),
                            Buff.CastDebuff("Mage Bomb", Magebombtalent, ret => true, Magebombtalent),
                            Spell.CastSpell("Inferno Blast", ret => Me.HasMyAura("Heating Up") && !Me.HasMyAura("Pyroblast!"), "Inferno Blast with Heating Up proc"),
                            Spell.CastSpell("Pyroblast", ret => Me.HasMyAura("Pyroblast!"), "Pyroblast with Pyroblast! proc"),
                            Spell.CastSpell("Fire Blast", ret => Me.HasMyAura("Heating Up") && !Spell.SpellOnCooldown("Fire Blast"), "Fire Blast with Heating Up proc"),
                            Spell.CastSpell("Fireball", ret => !Me.HasMyAura("Pyroblast!") && !Me.HasMyAura("Presence of Mind") && (!Me.HasMyAura("Heating Up") || Spell.SpellOnCooldown("Fire Blast") || (Me.HasMyAura("Heating Up") && Spell.SpellOnCooldown("Fire Blast"))), "Fireball"),
                            Spell.CastSpell("Scorch", ret => Me.IsMoving && TalentManager.HasTalent(2), false, "Scorch (Moving)"),
                            Spell.CastSpell("Ice Lance", ret => Me.IsMoving, false, "Ice Lance (Moving)")

                       );
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    new DecoratorContinue(ret => Me.CurrentTarget != null && !Me.IsSafelyFacing(Me.CurrentTarget, 45f), new Action(ret => Me.CurrentTarget.Face())),
                    this.SingleRotation);
            }
        }

        public override Composite Medic
        {
            get
            {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSpell("Ice Barrier", ret => Me.HealthPercent < 80 && TalentManager.HasTalent(6), "Ice Barrier"),
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Ice Block", ret => Me.HealthPercent < 20 && !Buff.PlayerHasActiveBuff("Hypothermia"), "Ice Block"),
                               Buff.CastBuff("Mage Ward", ret => Me.HealthPercent < 50, "Mage Ward")));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return new Decorator(
                           ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                           new PrioritySelector(
                               Spell.CastSpell("Blazing Speed", unit => Me, ret => Me.MovementInfo.ForwardSpeed < 8.05 && Me.IsMoving && TalentManager.HasTalent(5) && CLUSettings.Instance.EnableMovement, "Blazing Speed"),
                               Buff.CastBuff("Molten Armor", ret => true, "Molten Armor"),
                    //Buff.CastRaidBuff("Dalaran Brilliance",         ret => true, "Dalaran Brilliance"), //Commentet out as it is of no real importance except for 10yrd extra range.
                               Buff.CastRaidBuff("Arcane Brilliance", ret => true, "Arcane Brilliance"),
                               Spell.CastSpell(759, ret => Spell.CanCast("Conjure Mana Gem") && Buff.PlayerHasBuff("Presence of Mind") && !Item.HaveManaGem() && Me.Level > 50, "Conjure Mana Gem")));
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
