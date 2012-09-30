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
        //Helfire --> Immolation Aura
        //Fel Flame --> Void Ray
        //Drain Life --> Harvest Life

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
                                   Item.UseEngineerGloves()                                   
                                   )),
                           // START Interupts & Spell casts
                           //untested Interupts
                           //Spell.CastInterupt("Carrion Swarm",ret => Me.HasGlyphInSlot(42458) && Me.HasMyAura(103965) && CLUSettings.Instance.CarrionSwarm,"Carrion Swarm Interupt"), //needs setting and works best with Carrion Swarm Glyph
                           //Spell.CastInterupt("Fear",ret => Me.HasGlyphInSlot(42458) && CLUSettings.Instance.FearInterupt,"FearwInterupt"), //needs setting and works best with Fear Glyph
                           //Spell.CastInterupt("Mortal Coil", ret => CLUSettings.Instance.MortalCoilInterupt, "Mortal Coil Interrupt"), //needs setting
                           new Decorator(ret => Me.GotAlivePet,
                                         new PrioritySelector(
                                             PetManager.CastPetSpell("Axe Toss", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && PetManager.CanCastPetSpell("Axe Toss"), "Axe Toss")
                                         ) 
                                        ),                                        
                           // lets get our pet back
                            PetManager.CastPetSummonSpell(105174, ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Demonic Rebirth")) && !Me.GotAlivePet && !Me.ActiveAuras.ContainsKey(WoWSpell.FromId(108503).Name), "Summon Pet"),
                            Common.WarlockGrimoire,                            
                           // Threat
                            Buff.CastBuff("Soulshatter", ret => Me.CurrentTarget != null && Me.GotTarget && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && !Spell.PlayerIsChanneling, "[High Threat] Soulshatter - Stupid Tank"),
                            //Raid Debuff
                            Buff.CastDebuff("Curse of the Elements", ret => Me.HasMyAura(103965),"Aura of the Elements"), //Better debuff under Meta
                            //Cooldowns                            
                            PetManager.CastPetSpell("Wrathstorm", ret => Me.CurrentTarget != null && PetManager.CanCastPetSpell("Wrathstorm") && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Wrathstorm"),
                            new Decorator(ret=> CLUSettings.Instance.UseCooldowns,
                                new PrioritySelector(
                                    Spell.CastSelfSpell("Twilight Ward", ret => !WoWSpell.FromId(6229).Cooldown && !Me.HasMyAura(6229) && Me.CurrentTarget.IsCasting && Me.HealthPercent < 66, "Twilight Ward (Protect me from magical damage)"),
                                    Spell.CastSelfSpell("Unending Resolve", ret => !WoWSpell.FromId(104773).Cooldown && Me.HealthPercent < 60, "Unending Resolve (Save my life)"),
                                    Spell.CastSelfSpell("Dark Bargain", ret => !WoWSpell.FromId(110913).Cooldown && Me.HealthPercent < 40, "Dark Bargain (Save my life)"),
                                    Item.UseBagItem("Jade Serpent Potion", ret => (Buff.UnitHasHasteBuff(Me) || Me.CurrentTarget.HealthPercent < 20) && Unit.IsTargetWorthy(Me.CurrentTarget), "Jade Serpent Potion"), 
                                    Buff.CastBuff("Dark Soul", ret => !WoWSpell.FromId(113861).Cooldown && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget) && !Me.IsMoving && !Me.HasAnyAura(Common.DarkSoul), "Dark Soul"),
                                    Spell.CastSpell("Summon Doomguard", ret => !WoWSpell.FromId(18540).Cooldown && Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Summon Doomguard")                                                                        
                                    )),
                            // AoE
                            new Decorator(
                               ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 15) > 3,
                               new PrioritySelector(
                                   Spell.CastSpell("Corruption", ret => (!Me.CurrentTarget.HasMyAura(172) || Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 2) && !Me.HasMyAura(103965), "Corruption"),
                                   Spell.CastSpell("Hand of Gul'dan", ret => !Me.HasMyAura(103965), "Hand of Gul'dan"),
                                   Spell.CastAreaSpell("Hand of Gul'dan",20,false,1,0.0,0.0, ret => Me.HasMyAura(103965) && Buff.PlayerActiveBuffTimeLeft("Molten Core").TotalSeconds < 5, "Chaos Wave"), //to stop us losing molten core
                                   Spell.CastSelfSpell("Metamorphosis", ret => !Me.HasMyAura(103965) && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 1000 || Buff.PlayerActiveBuffTimeLeft("Molten Core").TotalSeconds < 10), "Metamorphosis"),
                                   Spell.CastAreaSpell("Helfire",12,false,4,0.0,0.0 ,ret => Me.HasMyAura(103965) && !Me.HasMyAura(104025),"Immolation Aura"),
                                   Spell.CastSpell("Fel Flame", ret => Me.IsFacing(Me.CurrentTarget) && Me.HasMyAura(103965) && Me.CurrentTarget.Distance < 20 && Me.CurrentTarget.HasMyAura(172) && Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 10, "Void Ray"),
                                   Spell.CastSpell("Corruption", ret => Me.HasMyAura(103965) && (!Me.CurrentTarget.HasMyAura(603) || Buff.TargetDebuffTimeLeft("Doom").TotalSeconds < 40), "Doom"),
                                   Spell.CastSpell("Fel Flame", ret => Me.IsFacing(Me.CurrentTarget) && Me.HasMyAura(103965) && Me.CurrentTarget.Distance < 20, "Void Ray"),
                                   Spell.ChannelAreaSpell("Drain Life",12,false,4,0.0,0.0, ret => true,"Harvest Life"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 50 && !Spell.PlayerIsChanneling && Me.HealthPercent > 50, "Life tap - mana < 50%")
                               )),                               
                           //Has Metamorphosis
                           new Decorator(ret => Me.CurrentTarget != null && Me.HasMyAura(103965),
                               new PrioritySelector(
                                    Spell.CastSpell("Corruption", ret => !Me.CurrentTarget.HasMyAura(603) || (Buff.TargetDebuffTimeLeft("Doom").TotalSeconds < 14 && Me.HasAnyAura(Common.DarkSoul)), "Doom"),
                                    Spell.CancelMyAura("Metamorphosis", ret => !WoWSpell.FromId(103958).Cooldown && (!Me.CurrentTarget.HasMyAura(172) || Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds > 20) && !Me.HasAnyAura(Common.DarkSoul) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI <= 750, "Cancel Metamorphosis"),
                                    Spell.CastSpell("Shadow Bolt", ret => Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 20 && Me.CurrentTarget.HasMyAura(172), "Touch of Chaos"),
                                    Spell.CastSpell("Soul Fire",ret => Me.ActiveAuras.ContainsKey("Molten Core"),"Soul Fire"),
                                    Spell.CastSpell("Shadow Bolt", ret => true, "Touch of Chaos")                                                                                                       
                                   )),
                           //No Metamorphosis
                           new Decorator(ret => Me.CurrentTarget != null && !Me.HasMyAura(103965),
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 50 && !Spell.PlayerIsChanneling && Me.HealthPercent > 50, "Life tap - mana < 50%"),
                                   Buff.CastDebuff("Corruption", ret => !Me.CurrentTarget.HasMyAura(172) || Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 1, "Corruption"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => !WoWSpell.FromId(103958).Cooldown && (Me.CurrentTarget.HasMyAura(172) && Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 5) && (Me.HasAnyAura(Common.DarkSoul) || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900), "Metamorphosis"),
                                   Spell.CastSpell("Hand of Gul'dan", ret => !Me.CurrentTarget.MovementInfo.IsMoving && (!Me.CurrentTarget.ActiveAuras.ContainsKey("Shadowflame") || Buff.TargetDebuffTimeLeft("Shadowflame").TotalSeconds < 2), "Hand of Gul'dan"),
                                   Spell.CastSpell("Soul Fire",ret => Me.ActiveAuras.ContainsKey("Molten Core"),"Soul Fire"),
                                   Spell.CastSpell("Shadow Bolt", ret => !Me.IsMoving, "Shadow Bolt"),
                                   Spell.CastSpell("Fel Flame", ret => Me.IsMoving, "Fel Flame")
                                   )),
                            Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 50 && !Spell.PlayerIsChanneling && Me.HealthPercent > 50, "Life tap - mana < 50%")                           
                           );
            }
        }

        public override Composite Medic
        {
            get
            { //tbd - rework to support all Warlock-Speccs and to support Settings
                return new Decorator(
                           ret => Me.HealthPercent < 55 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(                                                                                             
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 50, "Healthstone"),
                               Spell.CastSpell("Mortal Coil", ret => Me.HealthPercent < 50, "Mortal Coil"), //possible option to use for emergency heal
                               Spell.ChannelSpell("Drain Life", ret => Me.HealthPercent < 50, "Harvest Life")
                               ));
            }
        }
        public override Composite PreCombat
        {
            get {return Common.WarlockPreCombat;}
        }


        public override Composite Resting
        {
            get {return Rest.CreateDefaultRestBehaviour();}
        }

        public override Composite PVPRotation
        {
            get {return this.SingleRotation;}
        }

        public override Composite PVERotation
        {
            get {return this.SingleRotation;}
        }
    }
}
