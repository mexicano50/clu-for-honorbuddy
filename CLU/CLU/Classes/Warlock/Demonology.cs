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
            get
            {
                return "Demonology Warlock - Jamjar0207 Edit";
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
            get
            {
                return 35f;
            }
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
                       "to be done\n" +
                       "----------------------------------------------------------------------\n";

            }
        }


        // Storm placing this here for your sanity
        /*SpellManager] Felstorm (119914) overrides Command Demon (119898)
[SpellManager] Dark Soul: Knowledge (113861) overrides Dark Soul (77801)
[SpellManager] Soul Link (108415) overrides Health Funnel (755)*/
        //Corruption --> doom
        //Hand of Gul'dan --> Chaos Wave
        //Shadow Bolt --> Touch of Chaos
        //Curse of the elements --> Aura of the Elements
        //Hellfire --> Immolation Aura
        //Fel Flame --> Void Ray
        //Drain Life --> Harvest Life

        public override Composite SingleRotation
        {
            get
            {
                return new PrioritySelector(
                    // Pause Rotation
                           new Decorator(ret => CLUSettings.Instance.PauseRotation, new ActionAlwaysSucceed()),
                    // Performance Timer (True = Enabled) returns Runstatus.Failure
                    // Spell.TreePerformance(true),
                           Spell.WaitForCast(),
                    // For DS Encounters.
                           EncounterSpecific.ExtraActionButton(),
                    // Threat
                            Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "[High Threat] Soulshatter - Stupid Tank"),
                           new Decorator(
                               ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                               new PrioritySelector(
                                   Item.UseTrinkets(),
                                   Racials.UseRacials(),
                                   Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                   Item.UseEngineerGloves()
                                   )),
                    // START Interupts & Spell casts
                    //untested Interupts
                    //Spell.CastInterupt("Carrion Swarm",ret => Me.HasGlyphInSlot(42458) && Me.HasMyAura(103965) && CLUSettings.Instance.CarrionSwarm,"Carrion Swarm Interupt"), //needs setting and works best with Carrion Swarm Glyph
                    //Spell.CastInterupt("Fear",ret => Me.HasGlyphInSlot(42458) && CLUSettings.Instance.FearInterupt,"FearInterupt"), //needs setting and works best with Fear Glyph
                    //Spell.CastInterupt("Mortal Coil", ret => CLUSettings.Instance.MortalCoilInterupt, "Mortal Coil Interrupt"), //needs setting
                           new Decorator(ret => Me.GotAlivePet,
                                         new PrioritySelector(
                                             PetManager.CastPetSpell("Axe Toss", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && PetManager.CanCastPetSpell("Axe Toss"), "Axe Toss")
                                         )
                                        ),
                    // lets get our pet back
                            PetManager.CastPetSummonSpell(105174, ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Demonic Rebirth")) && !Me.GotAlivePet && !Me.ActiveAuras.ContainsKey(WoWSpell.FromId(108503).Name), "Summon Pet"),
                            Common.WarlockGrimoire,
                    
                    //Raid Debuff
                            Buff.CastDebuff("Curse of the Elements", ret => Me.CurrentTarget != null && !Me.HasMyAura(103965) && !Me.CurrentTarget.HasAura(1490), "Curse of the Elements"), //debuff not under Meta
                            Spell.CastSpell("Curse of the Elements", ret => Me.CurrentTarget != null && Me.HasMyAura(103965) && !Me.HasMyAura(116202), "Aura of the Elements"), //buff under Meta
                    //Cooldowns                            
                            
                            new Decorator(ret => CLUSettings.Instance.UseCooldowns,
                                new PrioritySelector(                                    
                                    Item.UseBagItem("Jade Serpent Potion", ret => (Buff.UnitHasHasteBuff(Me) || Me.CurrentTarget.HealthPercent < 20) && Unit.IsTargetWorthy(Me.CurrentTarget), "Jade Serpent Potion"),
                                    Item.UseBagItem("Volcanic Potion", ret => (Buff.UnitHasHasteBuff(Me) || Me.CurrentTarget.HealthPercent < 20) && Unit.IsTargetWorthy(Me.CurrentTarget), "Volcanic Potion"),
                                    Buff.CastBuff("Dark Soul", ret => !WoWSpell.FromId(113861).Cooldown && !Me.HasAnyAura(Common.DarkSoul), "Dark Soul")
                                    )),
                            PetManager.CastPetSpell("Wrathstorm", ret => Me.CurrentTarget != null && PetManager.CanCastPetSpell("Wrathstorm") && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Wrathstorm"),
                            
                    // AoE
                            new Decorator(
                               ret => CLUSettings.Instance.UseAoEAbilities && !Me.IsMoving && Me.CurrentTarget != null,
                               new PrioritySelector(
                                   Spell.CastSpell("Corruption", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 30) > 3 && !Me.HasMyAura(103965) && (!Me.CurrentTarget.HasMyAura(172) || Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 1.89) && Unit.TimeToDeath(Me.CurrentTarget) > 30, "Corruption"),
                                   Spell.CastAreaSpell("Hand of Gul'dan", 12, false, 4, 0.0, 0.0, ret => !Me.HasMyAura(103965) , "Hand of Gul'dan"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 30) > 3 && !WoWSpell.FromId(103958).Cooldown && !Me.HasMyAura(103965) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 1000 || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= (Unit.TimeToDeath(Me.CurrentTarget) * 31), "Metamorphosis"),
                                   Spell.CastAreaSpell("Hellfire", 12, false, 4, 0.0, 0.0, ret => Me.HasMyAura(103965) && !Me.HasMyAura(104025), "Immolation Aura"),
                                   Spell.CastSpell("Fel Flame", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) > 3 && Me.IsFacing(Me.CurrentTarget) && Me.HasMyAura(103965) && Me.CurrentTarget.Distance < 20 && Me.CurrentTarget.HasMyAura(172) && Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 10, "Void Ray"),
                                   Spell.CastSpell("Corruption", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 30) > 3 && Me.HasMyAura(103965) && (!Me.CurrentTarget.HasMyAura(603) || (Me.CurrentTarget.HasMyAura(603) && Buff.TargetDebuffTimeLeft("Doom").TotalSeconds < 40)) && Unit.TimeToDeath(Me.CurrentTarget) > 30, "Doom"),
                                   Spell.CastSpell("Fel Flame", ret => Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 20) > 3 && Me.IsFacing(Me.CurrentTarget) && Me.HasMyAura(103965) && Me.CurrentTarget.Distance < 20, "Void Ray"),
                                   Spell.ChannelAreaSpell("Drain Life", 15, false, 4, 0.0, 0.0, ret => !Me.HasMyAura(103965), "Harvest Life"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 15 && Me.HealthPercent > 50, "Life tap - mana < 50%")                                                                                                          
                               )),

                            new Decorator(ret => CLUSettings.Instance.UseCooldowns,
                                new PrioritySelector(
                                    Spell.CastSpell("Summon Doomguard", ret => !WoWSpell.FromId(18540).Cooldown && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Summon Doomguard")
                                    )),

                           new Decorator(ret => Me.CurrentTarget != null,
                               new PrioritySelector(
                                   Spell.CastSpell("Corruption", ret => (!Me.HasMyAura(103965) && (!Me.CurrentTarget.HasMyAura(172) || Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 1.89)) && Unit.TimeToDeath(Me.CurrentTarget) >= 6, "Corruption"),
                                   Spell.CastSpell("Corruption", ret => Me.HasMyAura(103965) && (!Me.CurrentTarget.HasMyAura(603) || Buff.TargetDebuffTimeLeft("Doom").TotalSeconds < 14.2 || (Buff.TargetDebuffTimeLeft("Doom").TotalSeconds < 28 && Me.HasAnyAura(Common.DarkSoul))) && Unit.TimeToDeath(Me.CurrentTarget) >= 30, "Doom"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => !Me.HasMyAura(103965) && !WoWSpell.FromId(103958).Cooldown
                                       && (Me.HasAnyAura(Common.DarkSoul) || Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 5 || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900 || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= (Unit.TimeToDeath(Me.CurrentTarget)*30)), "Metamorphosis"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => Me.HasMyAura(103965) && !WoWSpell.FromId(103958).Cooldown
                                       && (Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds > 20
                                       && !Me.HasAnyAura(Common.DarkSoul) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI <= 750 && Unit.TimeToDeath(Me.CurrentTarget) > 30), "Cancel Metamorphosis"),
                                   Spell.CastSpell("Hand of Gul'dan", ret => !Me.HasMyAura(103965) && !Me.CurrentTarget.MovementInfo.IsMoving
                                       && !Me.CurrentTarget.HasMyAura(47960), "Hand of Gul'dan"),                            
                                   Spell.CastSpell("Shadow Bolt", ret => Me.HasMyAura(103965) && Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 20 && Me.CurrentTarget.HasMyAura(172), "Touch of Chaos"),
                                   Spell.CastSpell("Soul Fire", ret => Me.HasMyAura(122355), "Soul Fire"),   
                                   Spell.CastSpell("Shadow Bolt", ret => Me.HasMyAura(103965), "Touch of Chaos"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 50 && Me.HealthPercent > 50, "Life tap - mana < 50%"),
                                   Spell.CastSpell("Shadow Bolt", ret => !Me.HasMyAura(103965), "Shadow Bolt"),
                                   Spell.CastSpell("Fel Flame", ret => !Me.HasMyAura(103965) && Me.IsMoving, "Fel Flame"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 100 && !Spell.PlayerIsChanneling && Me.HealthPercent > 50, "Life tap - mana < 50%")                                        
                              ))                            
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
                return new Decorator(
                           ret => Me.HealthPercent < 55 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Twilight Ward", ret => !WoWSpell.FromId(6229).Cooldown && !Me.HasMyAura(6229) && Me.HealthPercent < 50, "Twilight Ward (Protect me from magical damage)"),
                               Spell.CastSpell("Mortal Coil", ret => Me.HealthPercent < 50, "Mortal Coil"), //possible option to use for emergency heal
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 50, "Healthstone"),
                               Spell.CastSelfSpell("Unending Resolve", ret => !WoWSpell.FromId(104773).Cooldown && Me.HealthPercent < 50, "Unending Resolve (Save my life)"),
                               Spell.CastSelfSpell("Dark Bargain", ret => !WoWSpell.FromId(110913).Cooldown && Me.HealthPercent < 50, "Dark Bargain (Save my life)"),                                                              
                               Spell.ChannelSpell("Drain Life", ret => Me.HealthPercent < 50, "Harvest Life")
                               ));
            }
        }
        public override Composite PreCombat
        {
            get { return Common.WarlockPreCombat; }
        }


        public override Composite Resting
        {
            get { return Rest.CreateDefaultRestBehaviour(); }
        }

        public override Composite PVPRotation
        {
            get { return this.SingleRotation; }
        }

        public override Composite PVERotation
        {
            get { return this.SingleRotation; }
        }
    }
}
