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

using CommonBehaviors.Actions;
using Styx.TreeSharp;
using Styx.WoWInternals;
using CLU.Base;
using CLU.Helpers;
using CLU.Managers;
using CLU.Settings;
using Rest = CLU.Base.Rest;

using Styx;
namespace CLU.Classes.Warlock
{

    class Demonology : RotationBase
    {
        public override string Name
        {
            get {
                return "Demonology Warlock";
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
                return "Metamorphosis";
            }
        }
        public override int KeySpellId
        {
            get { return 103958; }
        }
        // I want to keep moving at melee range while morph is available
        // note that this info is used only if you enable moving/facing in the CC settings.
        public override float CombatMaxDistance
        {
            get {
                return 35f;
            }
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
                       "This CC will:\n" +
                       "1. Soulshatter, Interupts with Axe Toss, Soul Harvest < 2 shards out of combat\n" +
                       "2. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Demon Soul & Summon Doomguard & Metamorphosis & Curse of the Elements & Lifeblood\n" +
                       "==> Felgaurd to Felhunter Swap\n" +
                       "3. AoE with Hellfire and Felstorm and Shadowflame\n" +
                       "4. Best Suited for end game raiding\n" +
                       "Ensure you're at least at 10yards from your target to maximize your dps, even during burst phase if possible.\n" +
                       "NOTE: PvP uses single target rotation - It's not designed for PvP use. \n" +
                       "Credits to cowdude\n" +
                       "----------------------------------------------------------------------\n";
            }
        }


        // Storm placing this here for your sanity
        /*SpellManager] Felstorm (119914) overrides Command Demon (119898)
[SpellManager] Dark Soul: Knowledge (113861) overrides Dark Soul (77801)
[SpellManager] Soul Link (108415) overrides Health Funnel (755)*/
        public override Composite SingleRotation
        {
            get {
                return new PrioritySelector(
                           // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),

                           // Performance Timer (True = Enabled) returns Runstatus.Failure
                           // Spell.TreePerformance(true),

                           Spell.WaitForCast(),

                           // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),

                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Item.UseEngineerGloves())),

                           // START Interupts & Spell casts
                           new Decorator(ret => Me.GotAlivePet,
                                         new PrioritySelector(
                                             PetManager.CastPetSpell("Axe Toss", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && PetManager.CanCastPetSpell("Axe Toss"), "Axe Toss")
                                         )
                                        ),

                           // lets get our pet back
                            PetManager.CastPetSummonSpell(105174, ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Soulburn")) && !Me.GotAlivePet && !Me.ActiveAuras.ContainsKey(WoWSpell.FromId(108503).Name), "Summon Pet"),
                            //Grimoire of Service
                            //Spell.CastSpell(111897, ret => Me.GotAlivePet && !WoWSpell.FromId(111897).Cooldown && TalentManager.HasTalent(14), "Grimoire of Service"),
                            //Sacrifice Pet
                            Spell.CastSelfSpell("Grimoire of Sacrifice", ret => Me.GotAlivePet && TalentManager.HasTalent(15) && !Me.ActiveAuras.ContainsKey(WoWSpell.FromId(108503).Name), "Grimoire of Sacrifice"),

                           // Threat
                           Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "[High Threat] Soulshatter - Stupid Tank"),
                            //Cooldowns
                            new Decorator(ret=> CLUSettings.Instance.UseCooldowns,
                                new PrioritySelector(
                                    Buff.CastBuff("Dark Soul", "Dark Soul: Knowledge", ret => !WoWSpell.FromId(113861).Cooldown && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Me.IsMoving, "Dark Soul: Knowledge")
                                    //// TODO: Remove this when Apoc fixs Spellmanager. -- wulf 
                                   // Spell.CastSpell(18540, ret => !WoWSpell.FromId(18540).Cooldown && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Summon Doomguard")
                                    )),
                           //Demonic Fury or Pull
                           new Decorator(ret => Me.CurrentTarget != null && ((!Me.CurrentTarget.HasMyAura(603) || (Me.CurrentTarget.ActiveAuras.ContainsKey("Corruption") && Spell.CurrentDemonicFury > 800 && Me.CurrentTarget.ActiveAuras.ContainsKey("Shadowflame")))),
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Metamorphosis", ret => Me.Shapeshift!=ShapeshiftForm.Metamorphosis, "Metamorphosis for Doom"),
                                   Buff.CastDebuff("Corruption", "Doom", ret => true, "Doom"), // changed to the base spell - wulf
                                   Spell.CastSpell("Shadow Bolt", ret => true, "Metamorphosis: Touch of Chaos") // changed to the base spell - wulf
                                   )),
                           Spell.CancelMyAura("Metamorphosis", ret => Me.Shapeshift == ShapeshiftForm.Metamorphosis && Me.CurrentTarget != null && Me.CurrentTarget.HasMyAura(603) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI < 200, "Metamorphosis"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.ManaPercent <= 30 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40 && !Buff.UnitHasHasteBuff(Me) && !Buff.PlayerHasBuff("Metamorphosis") && !Buff.PlayerHasBuff("Demon Soul: Felguard"), "Life tap - mana < 30%"),
                           Spell.CastSelfSpell("Life Tap",                ret => Me.IsMoving && Me.HealthPercent > Me.ManaPercent && Me.ManaPercent < 80, "Life tap while moving"),
                           new Decorator(ret => Me.CurrentTarget != null && Me.Shapeshift != ShapeshiftForm.Metamorphosis && Me.CurrentTarget.HasMyAura(603) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI < 800,
                               new PrioritySelector(
                                    Buff.CastDebuff("Corruption", ret => true, "Corruption"),
                                    Spell.CastSpell("Hand of Gul'dan", ret => !Me.CurrentTarget.ActiveAuras.ContainsKey("Shadowflame"), "Hand of Gul'dan"),
                                    Spell.CastSpell("Soul Fire",ret => Me.ActiveAuras.ContainsKey("Molten Core"),"Soul Fire"),
                                    Spell.CastSpell("Shadow Bolt",ret => !Me.IsMoving,"Shadow Bolt"),
                                    Spell.CastSpell("Fel Flame",ret => Me.IsMoving,"Fel Flame")
                                   )),
                           Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 100 && !Spell.PlayerIsChanneling && Me.HealthPercent > 40, "Life tap - mana < 100%"));

            }
        }

        public override Composite Medic
        {
            get {
                return new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 40, "Healthstone")));
            }
        }

        public override Composite PreCombat
        {
            get {
                return new PrioritySelector(
                           new Decorator(
                               ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                               new PrioritySelector(
                                   Buff.CastBuff("Dark Intent", ret => true, "Dark Intent"),
                                   PetManager.CastPetSummonSpell(30146, ret => !Me.IsMoving && !Me.GotAlivePet, " Summoning Pet Felguard")
                                  )));
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
