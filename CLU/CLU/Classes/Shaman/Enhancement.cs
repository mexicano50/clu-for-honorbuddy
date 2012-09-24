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
using System.Linq;
using CommonBehaviors.Actions;
using CLU.Lists;
using CLU.Settings;
using CLU.Base;
using Rest = CLU.Base.Rest;
using global::CLU.Managers;

namespace CLU.Classes.Shaman
{
    using Styx.WoWInternals;
    using Styx.CommonBot;

    class Enhancement : RotationBase
    {
        private const int ItemSetId = 1071;

        public override string Name
        {
            get
            {
                return "Enhancement Shaman";
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
                return "Lava Lash";
            }
        }

        public override int KeySpellId
        {
            get { return 60103; }
        }

        public override float CombatMaxDistance
        {
            get
            {
                return 3.2f;
            }
        }

        public override string Help
        {
            get
            {

                var twopceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                var fourpceinfo = Item.Has2PcTeirBonus(ItemSetId) ? "2Pc Teir Bonus Detected" : "User does not have 2Pc Teir Bonus";
                return "\n" +
                       "----------------------------------------------------------------------\n" +
                       "This Rotation will:\n" +
                       twopceinfo + "\n" +
                       fourpceinfo + "\n" +
                       "1. Enchant Weapons: Windfury Weapon(MainHand) & Flametongue Weapon(OffHand)\n" +
                       "2. Totems: Strength of earth, Windfury, Mana Spring, Searing (with range check) \n" +
                       "3. AutomaticCooldowns has: \n" +
                       "==> UseTrinkets \n" +
                       "==> UseRacials \n" +
                       "==> UseEngineerGloves \n" +
                       "==> Feral Spirit \n" +
                       "4. AoE with Magma Totem, Chain Lightning, Fire Nova\n" +
                       "4. Heal using: Lightning Shield\n" +
                       "6. Best Suited for end game raiding\n" +
                       "NOTE: PvP rotations have been implemented in the most basic form, once MoP is released I will go back & revise the rotations for optimal functionality 'Dagradt'. \n" +
                       "Credits to fluffyhusky, sjussju , Stormchasing, alxaw\n" +
                       "----------------------------------------------------------------------\n";
            }
        }

        //[SpellManager] Stormstrike (17364) overrides Primal Strike (73899)
        private static bool EverythingOnCoolDown { get { return Spell.SpellCooldown("Primal Strike").TotalSeconds > 1.0 && Spell.SpellCooldown("Lava Lash").TotalSeconds > 1.0 && Spell.SpellCooldown("Unleash Elements").TotalSeconds > 1.0 && Spell.SpellCooldown("Earth Shock").Seconds > 1.0; } }

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
                                   Item.UseEngineerGloves())),
                    // Interupts
                           Spell.CastInterupt("Wind Shear", ret => true, "Wind Shear"),
                    // Threat
                           Buff.CastBuff("Wind Shear", ret => Me.CurrentTarget != null && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Wind Shear (Threat)"),
                    // Totem management
                           Totems.CreateTotemsBehavior(),
                    // AoE
                           new Decorator(
                               ret => !Me.IsMoving && Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 2 && !BossList.IgnoreAoE.Contains(Unit.CurrentTargetEntry),
                               new PrioritySelector(
                                   Spell.CastSelfSpell("Magma Totem",
                                        ret => Me.CurrentTarget != null && Unit.CountEnnemiesInRange(Me.CurrentTarget.Location, 8) >= 6 && Me.Totems.All(t => t.WoWTotem != WoWTotem.Magma)
                                            && !Totems.Exist(WoWTotem.FireElemental), "Magma Totem"),
                                   Spell.CastSpell("Chain Lightning", ret => Buff.PlayerCountBuff("Maelstrom Weapon") == 5, "Chain Lightning"),
                                   Spell.CastSpell("Flame Shock", ret => !Buff.TargetHasDebuff("Flame Shock") || Buff.TargetDebuffTimeLeft("Flame Shock").Seconds <= 3, "Flame Shock"),
                                   Spell.CastSpell("Lava Lash", ret => Buff.TargetHasDebuff("Flame Shock"), "Lava Lash"),
                                   Spell.CastSpell("Fire Nova", ret => true, "Fire Nova"),
                                   Spell.CastSpell("Primal Strike", ret => true, "Stormstrike")
                               )),
                    // Default Rotaion
                           Spell.CastSpell("Searing Totem", ret => !Totems.Exist(WoWTotemType.Fire) && !Totems.Exist(WoWTotem.FireElemental), "Searing Totem"),
                           Spell.CastSpell("Lightning Bolt", ret => Buff.PlayerCountBuff("Maelstrom Weapon") == 5 || (Item.Has4PcTeirBonus(ItemSetId) ? Buff.PlayerCountBuff("Maelstrom Weapon") == 5 : Buff.PlayerCountBuff("Maelstrom Weapon") >= 4 && (Spell.SpellCooldown("Feral Spirit").TotalSeconds > 90 && Spell.SpellOnCooldown("Feral Spirit"))), "Lightning Bolt"),
                           Buff.CastDebuff("Flame Shock", ret => !Buff.TargetHasDebuff("Flame Shock") || Buff.TargetDebuffTimeLeft("Flame Shock").Seconds <= 3, "Flame Shock"),
                           Spell.CastSpell("Flame Shock", ret => Buff.PlayerHasActiveBuff("Unleash Flame"), "Flame Shock"),
                           Spell.CastSpell("Primal Strike", ret => true, "Stormstrike"),
                           Spell.CastSpell("Lava Lash", ret => true, "Lava Lash"),
                           Spell.CastSpell("Unleash Elements", ret => true, "Unleash Elements"),
                           Spell.CastSpell("Earth Shock", ret => Buff.TargetHasDebuff("Flame Shock"), "Earth Shock"),
                           Spell.CastSelfSpell("Elemental Mastery", ret => TalentManager.HasTalent(10) && Unit.IsTargetWorthy(Me.CurrentTarget), "Elemental Mastery"),
                            Spell.CastSpell("Stormlash Totem", ret => CLUSettings.Instance.Shaman.UseStormlashTotem != StormlashTotem.Never
                                        && ((CLUSettings.Instance.Shaman.UseStormlashTotem == StormlashTotem.OnHaste && Me.HasAnyAura(Me.IsHorde ? "Bloodlust" : "Heroism", "Timewarp", "Ancient Hysteria") 
                                        || CLUSettings.Instance.Shaman.UseStormlashTotem == StormlashTotem.OnCooldown)
                                        && !Totems.Exist(WoWTotemType.Air)),"Stormlash Totem"),
                           Spell.CastSpell("Feral Spirit", ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget), "Feral Spirit"),
                           Spell.CastSpell("Fire Elemental Totem", ret => Unit.IsTargetWorthy(Me.CurrentTarget) && Buff.TargetCountDebuff("Searing Flames") <= 4, "Fire Elemental Totem"),
                           Spell.CastSpell("Earth Elemental Totem", ret => Unit.IsTargetWorthy(Me.CurrentTarget), "Earth Elemental Totem"),
                           Buff.CastBuff("Spiritwalker's Grace", ret => Me.IsMoving, "Spiritwalker's Grace"),
                           Spell.CastSpell("Lightning Bolt", ret => Buff.PlayerCountBuff("Maelstrom Weapon") > 1 && !Buff.PlayerHasActiveBuff("Ascendance") && EverythingOnCoolDown && Buff.TargetHasDebuff("Flame Shock"), "Lightning Bolt"));
            }
        }

        public Composite burstRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                ));
            }
        }

        public Composite baseRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                    //new Action(a => { CLU.Log("I am the start of public Composite baseRotation"); return RunStatus.Failure; }),
                    //PvP Utilities

                        //Rotation
                    //7	0.00	wind_shear
                    //8	0.99	bloodlust,if=target.health.pct<25|time>5
                    //use_item,name=firebirds_grips
                        Item.UseEngineerGloves(),
                    //virmens_bite_potion,if=time>60&(pet.primal_fire_elemental.active|pet.greater_fire_elemental.active|target.time_to_die<=60)
                    //blood_fury
                        Racials.UseRacials(),
                    //elemental_mastery,if=talent.elemental_mastery.enabled
                        Spell.CastSelfSpell("Elemental Mastery", ret => TalentManager.HasTalent(10), "Elemental Mastery"),
                    //fire_elemental_totem,if=!active&(buff.bloodlust.up|buff.elemental_mastery.up|target.time_to_die<=totem.fire_elemental_totem.duration+10|(talent.elemental_mastery.enabled&(cooldown.elemental_mastery.remains=0|cooldown.elemental_mastery.remains>80)|time>=60))
                        Spell.CastSpell("Fire Elemental Totem", ret => true, "Fire Elemental Totem"),
                    //X	2.94	ascendance,if=cooldown.strike.remains>=3
                    //searing_totem,if=!totem.fire.active
                        Spell.CastSpell("Searing Totem", ret => !Totems.Exist(WoWTotemType.Fire), "Searing Totem"),
                    //unleash_elements,if=talent.unleashed_fury.enabled
                        Spell.CastSpell("Unleash Elements", ret => TalentManager.HasTalent(16), "Unleash Elements"),
                    //elemental_blast,if=talent.elemental_blast.enabled
                        Spell.CastSpell("Elemental Blast", ret => TalentManager.HasTalent(18), "Elemental Blast"),
                    //lightning_bolt,if=buff.maelstrom_weapon.react=5|(set_bonus.tier13_4pc_melee=1&buff.maelstrom_weapon.react>=4&pet.spirit_wolf.active)
                        Spell.CastSpell("Lightning Bolt", ret => Buff.PlayerCountBuff("Maelstrom Weapon") == 5, "Lightning Bolt"),
                    //stormblast
                        Spell.CastSpell("Stormblast", ret => true, "Stormblast"),
                    //flame_shock,if=buff.unleash_flame.up&!ticking
                        Spell.CastSpell("Flame Shock", ret => Buff.PlayerHasActiveBuff("Unleash Flame") && !Buff.TargetHasDebuff("Flame Shock"), "Flame Shock"),
                    //stormstrike
                        Spell.CastSpell("Primal Strike", ret => true, "Stormstrike"),
                    //lava_lash
                        Spell.CastSpell("Lava Lash", ret => true, "Lava Lash"),
                    //unleash_elements
                        Spell.CastSpell("Unleash Elements", ret => true, "Unleash Elements"),
                    //lightning_bolt,if=buff.maelstrom_weapon.react>=3&target.debuff.unleashed_fury_ft.up&!buff.ascendance.up
                        Spell.CastSpell("Lightning Bolt", ret => Buff.PlayerCountBuff("Maelstrom Weapon") >= 3 && Buff.TargetHasDebuff("Unleashed Fury") && !Buff.PlayerHasActiveBuff("Ascendance"), "Lightning Bolt"),
                    //ancestral_swiftness,if=talent.ancestral_swiftness.enabled&buff.maelstrom_weapon.react<2
                        Spell.CastSelfSpell("Ancestral Swiftness", ret => TalentManager.HasTalent(11) && Buff.PlayerCountBuff("Maelstrom Weapon") < 2, "Ancestral Swiftness"),
                    //lightning_bolt,if=buff.ancestral_swiftness.up
                        Spell.CastSpell("Lightning Bolt", ret => Buff.PlayerHasActiveBuff("Ancestral Swiftness"), "Lightning Bolt"),
                    //flame_shock,if=buff.unleash_flame.up&dot.flame_shock.remains<=3
                        Spell.CastSpell("Flame Shock", ret => Buff.PlayerHasActiveBuff("Unleash Flame") && Buff.TargetDebuffTimeLeft("Flame Shock").Seconds <= 3, "Flame Shock"),
                    //earth_shock
                        Spell.CastSpell("Earth Shock", ret => true, "Earth Shock"),
                    //feral_spirit
                        Spell.CastSpell("Feral Spirit", ret => true, "Feral Spirit"),
                    //earth_elemental_totem,if=!active&cooldown.fire_elemental_totem.remains>=50
                        Spell.CastSpell("Earth Elemental Totem", ret => SpellManager.Spells["Fire Elemental Totem"].CooldownTimeLeft.Seconds >= 50, "Earth Elemental Totem"),
                    //spiritwalkers_grace,moving=1
                        Spell.CastSelfSpell("Spiritwalker's Grace", ret => Me.IsMoving, "Spiritwalker's Grace"),
                    //lightning_bolt,if=buff.maelstrom_weapon.react>1&!buff.ascendance.up
                        Spell.CastSpell("Lightning Bolt", ret => Buff.PlayerCountBuff("Maelstrom Weapon") > 1 && !Buff.PlayerHasActiveBuff("Ascendance"), "Lightning Bolt")
                ));
            }
        }

        public override Composite Medic
        {
            get
            {
                return new PrioritySelector(
                    Common.HandleCompulsoryShamanBuffs(),
                    Common.HandleTotemRecall(),
                    // Healing shit.
                    new Decorator(
                           ret => Me.HealthPercent < 100 && CLUSettings.Instance.EnableSelfHealing,
                           new PrioritySelector(
                               Item.UseBagItem("Healthstone", ret => Me.HealthPercent < 30, "Healthstone"),
                               Buff.CastBuff("Shamanistic Rage", ret => Me.CurrentTarget != null && (Me.HealthPercent < 60 || (Me.ManaPercent < 65 && Me.CurrentTarget.HealthPercent >= 75)), "Shamanistic Rage"),
                               Spell.CastSpell("Healing Surge", ret => Me, ret => Me.HealthPercent < 75 && CLUSettings.Instance.EnableSelfHealing && CLUSettings.Instance.EnableMovement, "Healing Surge"))));
            }
        }

        public override Composite PreCombat
        {
            get
            {
                return (
                    new Decorator(ret => !Me.Mounted && !Me.IsDead && !Me.Combat && !Me.IsFlying && !Me.IsOnTransport && !Me.HasAura("Food") && !Me.HasAura("Drink"),
                        new PrioritySelector(
                            //flask,type=spring_blossoms
                            //food,type=sea_mist_rice_noodles
                            Common.HandleCompulsoryShamanBuffs(),
                            //virmens_bite_potion
                            new Decorator(ret => Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapDefensive),
                            new Decorator(ret => !Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapOffensive),
                            Common.HandleTotemRecall()
                )));
            }
        }

        public override Composite Resting
        {
            get
            {
                return Base.Rest.CreateDefaultRestBehaviour();
            }
        }

        public override Composite PVPRotation
        {
            get
            {
                return (
                    new PrioritySelector(
                    //new Action(a => { CLU.Log("I am the start of public override Composite PVPRotation"); return RunStatus.Failure; }),
                        CrowdControl.freeMe(),
                        new Decorator(ret => Macro.Manual || BotChecker.BotBaseInUse("BGBuddy"),
                            new Decorator(ret => Me.CurrentTarget != null && Unit.IsTargetWorthy(Me.CurrentTarget),
                                new PrioritySelector(
                                    new Decorator(ret => Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapDefensive),
                                    new Decorator(ret => !Macro.weaponSwap && !BotChecker.BotBaseInUse("BGbuddy"), wepSwapOffensive),
                                    Item.UseTrinkets(),
                                    Buff.CastBuff("Lifeblood", ret => true, "Lifeblood"),
                                    new Action(delegate
                                    {
                                        Macro.isMultiCastMacroInUse();
                                        return RunStatus.Failure;
                                    }),
                                    new Decorator(ret => Macro.Burst, burstRotation),
                                    new Decorator(ret => !Macro.Burst || BotChecker.BotBaseInUse("BGBuddy"), baseRotation)))
                )));
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return this.SingleRotation;
            }
        }

        #region Weapon swap stuff

        public Composite wepSwapDefensive
        {
            get
            {
                return (
                    new Decorator(ret => Me.Inventory.Equipped.OffHand.Name != CLUSettings.Instance.Shaman.PvPShieldItemName && !string.IsNullOrEmpty(CLUSettings.Instance.Shaman.PvPMainHandItemName) && !string.IsNullOrEmpty(CLUSettings.Instance.Shaman.PvPShieldItemName) && CLUSettings.Instance.Shaman.PvPMainHandItemName != "Input the name of your Main-Hand weapon here" && CLUSettings.Instance.Shaman.PvPShieldItemName != "Input the name of your Shield here",
                        new Action(delegate
                        {
                            CLU.Log("Switching to Defensive Mode. Using Main-Hand: [{0}] Using Off-Hand: [{1}]", CLUSettings.Instance.Shaman.PvPMainHandItemName, CLUSettings.Instance.Shaman.PvPShieldItemName);
                            Lua.DoString("RunMacroText(\"/equipslot 16 " + CLUSettings.Instance.Shaman.PvPMainHandItemName + "\")");
                            Lua.DoString("RunMacroText(\"/equipslot 17 " + CLUSettings.Instance.Shaman.PvPShieldItemName + "\")");
                            return RunStatus.Failure;
                        })
                ));
            }
        }

        public Composite wepSwapOffensive
        {
            get
            {
                return (
                    new Decorator(ret => Me.Inventory.Equipped.OffHand.Name != CLUSettings.Instance.Shaman.PvPOffHandItemName && !string.IsNullOrEmpty(CLUSettings.Instance.Shaman.PvPMainHandItemName) && !string.IsNullOrEmpty(CLUSettings.Instance.Shaman.PvPOffHandItemName) && CLUSettings.Instance.Shaman.PvPMainHandItemName != "Input the name of your Main-Hand weapon here" && CLUSettings.Instance.Shaman.PvPOffHandItemName != "Input the name of your Off-Hand weapon here",
                        new Action(delegate
                        {
                            CLU.Log("Switching to Offensive Mode. Using Main-Hand: [{0}] Using Off-Hand: [{1}]", CLUSettings.Instance.Shaman.PvPMainHandItemName, CLUSettings.Instance.Shaman.PvPOffHandItemName);
                            Lua.DoString("RunMacroText(\"/equipslot 16 " + CLUSettings.Instance.Shaman.PvPMainHandItemName + "\")");
                            Lua.DoString("RunMacroText(\"/equipslot 17 " + CLUSettings.Instance.Shaman.PvPOffHandItemName + "\")");
                            return RunStatus.Failure;
                        })
                ));
            }
        }
        #endregion
    }
}
