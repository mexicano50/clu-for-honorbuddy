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
using CLU.Base;
using CLU.Helpers;
using CLU.Managers;
using CLU.Settings;
using System.Linq;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Rest = CLU.Base.Rest;

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

                           new Decorator(ret => Me.GotAlivePet,
                                         new PrioritySelector(
                                             PetManager.CastPetSpell("Axe Toss", ret => Me.CurrentTarget != null && Me.CurrentTarget.IsCasting && Me.CurrentTarget.CanInterruptCurrentSpellCast && PetManager.CanCastPetSpell("Axe Toss"), "Axe Toss")
                                         )
                                        ),
                    // lets get our pet back
                            PetManager.CastPetSummonSpell("Summon Felguard", ret => (!Me.IsMoving || Me.ActiveAuras.ContainsKey("Demonic Rebirth")) && !Me.GotAlivePet && !Me.ActiveAuras.ContainsKey(WoWSpell.FromId(108503).Name), "Summon Pet"),



                    //Raid Debuff
                            Buff.CastDebuff("Curse of the Elements", ret => Me.CurrentTarget != null && !Me.HasMyAura(103965) && !Me.CurrentTarget.HasAura(1490), "Curse of the Elements"), //debuff not under Meta
                            Spell.CastSpell("Curse of the Elements", ret => Me.CurrentTarget != null && Me.HasMyAura(103965) && !Me.HasMyAura(116202), "Aura of the Elements"), //buff under Meta
                    //Cooldowns                            

                            new Decorator(ret => CLUSettings.Instance.UseCooldowns && Unit.UseCooldowns(),
                                new PrioritySelector(
                                    Racials.UseRacials(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    Item.UseEngineerGloves(), 
                                    Item.UseBagItem("Jade Serpent Potion", ret => (Buff.UnitHasHasteBuff(Me) || Me.CurrentTarget.HealthPercent < 20), "Jade Serpent Potion"),
                                    Item.UseBagItem("Volcanic Potion", ret => (Buff.UnitHasHasteBuff(Me) || Me.CurrentTarget.HealthPercent < 20), "Volcanic Potion"),
                                    Spell.CastSelfSpell("Dark Soul", ret => !WoWSpell.FromId(103958).Cooldown && !WoWSpell.FromId(113861).Cooldown && !Me.HasAnyAura(Common.DarkSoul) && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900 || Unit.IsBoss(Me.CurrentTarget) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= (Unit.TimeToDeath(Me.CurrentTarget) * 30)), "Dark Soul"),
                                    Spell.CastSpell("Summon Doomguard", ret => !WoWSpell.FromId(18540).Cooldown && Me.CurrentTarget != null, "Summon Doomguard"),
                                    Common.WarlockGrimoire,
                                    Spell.CastSelfSpell("Metamorphosis", ret => !Me.HasMyAura(103965) && !WoWSpell.FromId(103958).Cooldown && Unit.TimeToDeath(Me.CurrentTarget) > 30 && Me.CurrentTarget.HasMyAura(172) && (Me.HasAnyAura(Common.DarkSoul) || !Me.CurrentTarget.HasMyAura(603) || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900 || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= (Unit.TimeToDeath(Me.CurrentTarget) * 30)), "Meta Boss High"),
                                    Spell.CastSelfSpell("Metamorphosis", ret =>  !Me.HasMyAura(103965) && !WoWSpell.FromId(103958).Cooldown && Unit.TimeToDeath(Me.CurrentTarget) <= 30 && Me.CurrentTarget.HasMyAura(172) && (Me.HasAnyAura(Common.DarkSoul) || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900 || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= (Unit.TimeToDeath(Me.CurrentTarget) * 30)), "Meta Boss Low"),
                                    Spell.CancelMyAura("Metamorphosis", ret => Me.HasMyAura(103965) && !Me.HasAnyAura(Common.DarkSoul) && Unit.TimeToDeath(Me.CurrentTarget) > 30 && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI <= 750 && Me.CurrentTarget.HasMyAura(603) && Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds > 20 || !Me.CurrentTarget.HasMyAura(172)), "Cancel Meta Boss High"),
                                    Spell.CancelMyAura("Metamorphosis", ret =>  Me.HasMyAura(103965) && !Me.HasAnyAura(Common.DarkSoul) && Unit.TimeToDeath(Me.CurrentTarget) <= 30 && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI < (Unit.TimeToDeath(Me.CurrentTarget) * 30) && Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds > 20 || Unit.TimeToDeath(Me.CurrentTarget) > 15 && !Me.CurrentTarget.HasMyAura(172)), "Cancel Meta Boss Low")
                                    )),
                            Spell.CastSelfSpell("Command Demon", ret => Me.CurrentTarget != null && (TalentManager.HasTalent(13) && !WoWSpell.FromId(119915).Cooldown || !TalentManager.HasTalent(13) && !WoWSpell.FromId(89751).Cooldown) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < Spell.MeleeRange, "Command Demon"),

                    // AoE
                            new Decorator(
                               ret => CLUSettings.Instance.UseAoEAbilities && Me.CurrentTarget != null && !CLUSettings.Instance.Warlock.ImTheFuckingBoss && Unit.EnemyMeleeUnits.Count() > 3,
                               new PrioritySelector(
                                   Spell.CancelMyAura("Dark Apotheosis", ret => Me.HasMyAura(114168), "Cancel Dark Apotheosis"),
                                   Spell.CancelMyAura("Hellfire", ret => Me.HasMyAura(1949) && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 1000 || !Me.CurrentTarget.HasMyAura(172) && Unit.TimeToDeath(Me.CurrentTarget) > 20 || Me.HealthPercent < 50), "Cancel Hellfire"),
                                   Spell.CastSpell("Corruption", ret => !Me.HasMyAura(103965) && Unit.TimeToDeath(Me.CurrentTarget) > 20 && (!Me.CurrentTarget.HasMyAura(172) || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds < 1.89), "Corruption"),
                                   Spell.CastSpell("Corruption", ret => Me.HasMyAura(103965) && Unit.TimeToDeath(Me.CurrentTarget) > 30 && (!Me.CurrentTarget.HasMyAura(603) || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Doom", true).TotalSeconds < 14.2), "Doom"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => !WoWSpell.FromId(103958).Cooldown && !Me.HasMyAura(103965) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 970, "Metamorphosis"),
                                   Spell.CastSpell("Hellfire", ret => Me.HasMyAura(103965) && !Me.HasMyAura(104025), "Immolation Aura"),
                                   Spell.CastSpell("Fel Flame", ret => Me.IsFacing(Me.CurrentTarget) && Me.HasMyAura(103965) && Me.CurrentTarget.Distance < 20 && Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds < 10, "Void Ray"),
                                   Spell.CastSpell("Fel Flame", ret => Me.IsFacing(Me.CurrentTarget) && Me.HasMyAura(103965) && Me.CurrentTarget.Distance < 20, "Void Ray"),
                                   Spell.CastSpell("Carrion Swarm", ret => Me.HasMyAura(103965), "Carrion Swarm"),
                                   Spell.CastSpell("Hellfire", ret => !Me.HasMyAura(103965) && Me.HealthPercent >= 50 && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI < 980, "Hellfire"),
                                   Spell.ChannelSpell("Drain Life", ret => !Me.HasMyAura(103965) && Me.HealthPercent < 50, "Harvest Life"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 15 && Me.HealthPercent > 50, "Life tap - mana < 15%")
                               )),

                           //Single Target
                           new Decorator(ret => Me.CurrentTarget != null && !CLUSettings.Instance.Warlock.ImTheFuckingBoss && (Unit.EnemyMeleeUnits.Count() <= 3 || !CLUSettings.Instance.UseAoEAbilities),
                               new PrioritySelector(
                                   Spell.CancelMyAura("Dark Apotheosis", ret => Me.HasMyAura(114168), "Cancel Dark Apotheosis"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => !Unit.IsBoss(Me.CurrentTarget) && !Me.HasMyAura(103965) && !WoWSpell.FromId(103958).Cooldown && Unit.TimeToDeath(Me.CurrentTarget) > 30 && Me.CurrentTarget.HasMyAura(172) && (Me.HasAnyAura(Common.DarkSoul) || !Me.CurrentTarget.HasMyAura(603) || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900), "Metamorphosis High"),
                                   Spell.CastSelfSpell("Metamorphosis", ret => !Unit.IsBoss(Me.CurrentTarget) && !Me.HasMyAura(103965) && !WoWSpell.FromId(103958).Cooldown && Unit.TimeToDeath(Me.CurrentTarget) <= 30 && Me.CurrentTarget.HasMyAura(172) && (Me.HasAnyAura(Common.DarkSoul) || Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900), "Metamorphosis Low"),
                                   Spell.CancelMyAura("Metamorphosis", ret => !Unit.IsBoss(Me.CurrentTarget) && Me.HasMyAura(103965) && !Me.HasAnyAura(Common.DarkSoul) && Unit.TimeToDeath(Me.CurrentTarget) > 30 && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI <= 750 && Me.CurrentTarget.HasMyAura(603) && Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds > 20 || !Me.CurrentTarget.HasMyAura(172)), "Cancel Meta High"),
                                   Spell.CancelMyAura("Metamorphosis", ret => !Unit.IsBoss(Me.CurrentTarget) && Me.HasMyAura(103965) && !Me.HasAnyAura(Common.DarkSoul) && Unit.TimeToDeath(Me.CurrentTarget) <= 30 && (Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI <= 750 && Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds > 20 || !Me.CurrentTarget.HasMyAura(172)), "Cancel Meta Low"),
                                   Spell.CastSpell("Corruption", ret => !Me.HasMyAura(103965) && (!Me.CurrentTarget.HasMyAura(172) || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds <= 3), "Corruption"),
                                   Spell.CastSpell("Corruption", ret => Me.HasMyAura(103965) && Unit.TimeToDeath(Me.CurrentTarget) > 30 && (Buff.GetAuraTimeLeft(Me.CurrentTarget, "Doom", true).TotalSeconds < 14.2 || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Doom", true).TotalSeconds < 28 && Me.HasAnyAura(Common.DarkSoul) || !Me.CurrentTarget.HasMyAura(603)), "Doom"),
                                   Buff.CastDebuff("Hand of Gul'dan", ret => !Me.HasMyAura(103965) && !Me.CurrentTarget.MovementInfo.IsMoving && (!Me.CurrentTarget.HasMyAura(47960) || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Shadowflame", true).TotalSeconds < 1.89), "Hand of Gul'dan"),
                                   Spell.CastSpell("Shadow Bolt", ret => Me.HasMyAura(103965) && Me.CurrentTarget.HasMyAura(172) && Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds < 20, "Touch of Chaos"),
                                   Buff.CastDebuff("Soul Fire", ret => Me.HasMyAura(122355), "Soul Fire"),
                                   Spell.CastSpell("Shadow Bolt", ret => Me.HasMyAura(103965) && (Unit.TimeToDeath(Me.CurrentTarget) > 30 && Me.CurrentTarget.HasMyAura(603) || Unit.TimeToDeath(Me.CurrentTarget) <= 30), "Touch of Chaos"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 50 && Me.HealthPercent > 50, "Life tap - mana < 50%"),
                                   Spell.CastSpell("Shadow Bolt", ret => !Me.HasMyAura(103965) && !Me.HasMyAura(122355) && Me.CurrentTarget.HasMyAura(172), "Shadow Bolt"),
                                   Spell.CastSpecialSpell("Kil'jaeden's Cunning", ret => Me.IsMoving && TalentManager.HasTalent(17) && !WoWSpell.FromId(119049).Cooldown, "Kil'jaeden's Cunning"),
                                   Spell.CastSpecialSpell("Soul Fire", ret => Me.HasMyAura(122355) && Buff.PlayerHasActiveBuff("Kil'jaeden's Cunning") && Me.IsMoving && !Me.HasMyAura(103965), "Shadow Bolt"),
                                   Spell.CastSpecialSpell("Shadow Bolt", ret => Buff.PlayerHasActiveBuff("Kil'jaeden's Cunning") && Me.IsMoving, "Shadow Bolt"),
                                   Spell.CastSpell("Fel Flame", ret => !Me.HasMyAura(103965) && Me.IsMoving, "Fel Flame"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 20 && !Spell.PlayerIsChanneling && Me.HealthPercent > 50, "Life tap - mana < 20%"))),

                           //Dark Apotheosis Tanking
                           new Decorator(ret => CLUSettings.Instance.Warlock.ImTheFuckingBoss && Me.CurrentTarget != null,
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Dark Apotheosis", ret => !Me.HasMyAura(114168), "Dark Apotheosis"),
                                   Spell.CastSpell("Corruption", ret => !Me.CurrentTarget.HasMyAura(172) || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Corruption", true).TotalSeconds < 1.89, "Corruption"),
                                   Buff.CastDebuff("Hand of Gul'dan", ret => !Me.CurrentTarget.MovementInfo.IsMoving && !Me.CurrentTarget.HasMyAura(47960) || Buff.GetAuraTimeLeft(Me.CurrentTarget, "Shadowflame", true).TotalSeconds < 1.89, "Hand of Gul'dan"),
                                   Spell.CastSpell("Hellfire", ret => !Me.HasMyAura(104025) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900, "Immolation Aura"),
                                   Spell.CastSpell("Fel Flame", ret => Me.CurrentTarget.HasMyAura(172) && Me.GetPowerInfo(Styx.WoWPowerType.DemonicFury).CurrentI >= 900, "Fel Flame"),
                                   Spell.CastSpell("Shadow Bolt", ret => Buff.TargetDebuffTimeLeft("Corruption").TotalSeconds < 20 && Me.CurrentTarget.HasMyAura(172), "Touch of Chaos"),
                                   Spell.CastSpell("Shadow Bolt", ret => true, "Touch of Chaos"),
                                   Buff.CastDebuff("Soul Fire", ret => Me.HasMyAura(122355), "Soul Fire"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 50 && Me.HealthPercent > 50, "Life tap - mana < 50%"),
                                   Spell.CastSpell("Fel Flame", ret => Me.IsMoving, "Fel Flame"),
                                   Spell.CastSelfSpell("Life Tap", ret => Me.ManaPercent < 20 && !Spell.PlayerIsChanneling && Me.HealthPercent > 50, "Life tap - mana < 20%")))
                           );
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
                           ret => Me.HealthPercent < 50 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Spell.CastSelfSpell("Archimonde's Vengeance", ret => TalentManager.HasTalent(16) && !WoWSpell.FromId(108505).Cooldown, "Archimonde's Vengeance"),
                               Spell.CastSelfSpell("Twilight Ward", ret => !WoWSpell.FromId(6229).Cooldown && !Me.HasMyAura(6229), "Twilight Ward (Protect me from magical damage)"),
                               Spell.CastSpell("Mortal Coil", ret => true, "Mortal Coil"), //possible option to use for emergency heal
                               Item.UseBagItem("Healthstone", ret => true, "Healthstone"),
                               Spell.CastSelfSpell("Unending Resolve", ret => !WoWSpell.FromId(104773).Cooldown, "Unending Resolve (Save my life)"),
                               Spell.CastSelfSpell("Dark Bargain", ret => !WoWSpell.FromId(110913).Cooldown, "Dark Bargain (Save my life)"),
                               Spell.ChannelSpell("Drain Life", ret => true, "Harvest Life")
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

