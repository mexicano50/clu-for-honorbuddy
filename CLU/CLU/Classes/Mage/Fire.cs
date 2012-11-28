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

using System.Collections.Generic;
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
                return FireMageRotation();
            }
        }

        public override Composite Pull
        {
            get
            {
                return new PrioritySelector(
                    Unit.EnsureTarget(),
                    Movement.CreateMoveToLosBehavior(),
                    Movement.CreateFaceTargetBehavior(),
                    Spell.WaitForCast(true),
                    this.SingleRotation,
                    Movement.CreateMoveToTargetBehavior(true, 39f)
                    );
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
        /* atm unused */
        private static HashSet<int> MageBomb = new HashSet<int>
        {
            44457, // Living Bomb 
            114923, // Nether Tempest
            113092  // Frost Bomb
        };
        /* atm unused, not finished */
        private Composite FireMageRotation()
        {
            return new PrioritySelector(
                new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                new Decorator(ret => Me.CurrentTarget != null && Unit.UseCooldowns(),
                    new PrioritySelector(
                        Item.UseTrinkets(),
                        Racials.UseRacials(),
                        Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"), // Thanks Kink
                        Item.UseBagItem("Volcanic Potion", ret => Buff.UnitHasHasteBuff(Me), "Volcanic Potion Heroism/Bloodlust"),
                        Item.UseEngineerGloves())),
                Spell.CastSpell("Spellsteal", ret => Spell.TargetHasStealableBuff() && !Me.IsMoving, "[Steal] Spellsteal"),
                Spell.CastInterupt("Counterspell", ret => true, "Counterspell"),
                Spell.CastSpell("Blazing Speed", unit => Me, ret => Me.MovementInfo.ForwardSpeed < 8.05 && Me.IsMoving && TalentManager.HasTalent(5) && CLUSettings.Instance.EnableMovement, "Blazing Speed"),
                // Cooldowns
                new Decorator(ret => CLUSettings.Instance.UseCooldowns,
                    new PrioritySelector(
                        //Spell.ChannelSelfSpell("Evocation", ret => Unit.UseCooldowns() && TalentManager.HasTalent(16) && !Me.HasAura("Invoker's Energy") && !Me.HasAura("Pyroblast!"), "Evocation"),
                        Spell.PreventDoubleChannel("Evocation", 0.5, true, On=>Me, ret => Unit.UseCooldowns() && TalentManager.HasTalent(16) && !Me.HasAura("Invoker's Energy") && !Me.HasAura("Pyroblast!")),
                        Spell.PreventDoubleCast("Presence of Mind", 0.5,on => Me, ret => Unit.UseCooldowns() && Me.HasAura("Pyroblast!") && Spell.CanCast("Alter Time")),
                        Spell.PreventDoubleCast("Alter Time", 0.5, on => Me, ret => (Me.HasAura("Invoker's Energy") && Me.HasAura("Pyroblast!") && Me.HasAura("Presence of Mind") && !Me.HasAura("Alter Time")) || (!Me.HasAura("Presence of Mind") && !Me.HasAura("Pyroblast!"))),
                        Item.UseBagItem("Mana Gem", ret => Me.CurrentTarget != null && Me.ManaPercent < 90, "Mana Gem"),
                        Item.UseBagItem("Brilliant Mana Gem", ret => Me.CurrentTarget != null && Me.ManaPercent < 90, "Brilliant Mana Gem"),
                        Spell.PreventDoubleCast("Mirror Image", 0.5, on => Me, ret => true))),
                 Spell.CastSpell(759, ret => Spell.CanCast("Conjure Mana Gem") && Buff.PlayerHasBuff("Presence of Mind") && !Item.HaveManaGem() && Me.Level > 50, "Conjure Mana Gem"),
                // Rune of Power
                Spell.CastOnUnitLocation("Rune of Power", unit => Me, ret => !Buff.PlayerHasBuff("Rune of Power") && TalentManager.HasTalent(17), "Rune of Power"),
                // AoE
                new Decorator(ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) >= CLUSettings.Instance.BurstOnMobCount && CLUSettings.Instance.UseAoEAbilities,
                    new PrioritySelector(
                        Spell.CastOnUnitLocation("Flamestrike", u => Me.CurrentTarget, ret => Me.CurrentTarget != null && !Buff.TargetHasDebuff("Flamestrike") && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry) && Me.ManaPercent > 30 && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3, "Flamestrike"),
                        Spell.CastConicSpell("Dragon's Breath", 12f, 33f, ret => !Me.HasMyAura("Heating Up") && !Me.HasMyAura("Pyroblast!"), "Dragon's Breath")
                        )),
                new Decorator(ret=> Me.GotTarget && Me.CurrentTarget!=null && Me.CurrentTarget.IsAlive && Me.CurrentTarget.Attackable,
                    new PrioritySelector(
                        //SkipFireball(),
                        Spell.CastSpell("Combustion", ret => CLUSettings.Instance.Mage.EnableCombustion && Buff.TargetHasDebuff("Ignite") && Unit.UseCooldowns(), "Combustion"),
                        /* Remove start - this is experimental und SHOULDN'T work*/
                        Spell.CastSpell("Living Bomb", ret => !Me.CurrentTarget.HasAura("Living Bomb") || Me.CurrentTarget.HasAura("Living Bomb") && Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, "Living Bomb", true) <= 1, "Living Bomb"),
                        Spell.CastSpell("Nether Tempest", ret => !Me.CurrentTarget.HasAura("Nether Tempest") || Me.CurrentTarget.HasAura("Nether Tempest") && Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, "Nether Tempest", true) <= 1, "Nether Tempest"),
                        Spell.CastSpell("Frost Bomb", ret => !Me.CurrentTarget.HasAura("Frost Bomb"), "Frost Bomb"),
                        /* Remove end */
                        Spell.CastSpell("Pyroblast", ret => Me.HasAura("Pyroblast!") || Me.HasAura("Presence of Mind"),"Pyroblast"),
                        Spell.PreventDoubleCast("Inferno Blast", 0.5, ret => true),
                        Spell.CastSpell("Mage Bomb", ret => !Me.CurrentTarget.HasAnyAura(MageBomb) || (Me.CurrentTarget.HasAnyAura(MageBomb) && Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, Magebombtalent, true) <= 1), "Mage Bomb"),
                        Spell.PreventDoubleCast("Fireball", 0.5, ret => Buff.GetAuraDoubleTimeLeft(Me.CurrentTarget, Magebombtalent, true)>2),
                        Spell.CastSpell("Scorch",ret=>Me.IsMoving,false,"Scorch")
                        ))
                );
        }
    }
}
