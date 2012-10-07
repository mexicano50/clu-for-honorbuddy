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

using Styx.WoWInternals;
using CLU.Settings;
using CLU.Base;
using CLU.Sound;

namespace CLU.Helpers
{

    public static class Keybinds
    {
        /* putting all the Keybind logic here */

        // Sounds from http://www2.research.att.com/~ttsweb/tts/demo.php

        internal static void Pulse()
        {

            // KeybindUseCooldowns \\
            if (IsKeyDown(CLUSettings.Instance.KeybindUseCooldowns)) {
                switch (CLUSettings.Instance.UseCooldowns) {
                case true:
                    CLUSettings.Instance.UseCooldowns = !CLUSettings.Instance.UseCooldowns;
                    CLULogger.Log(" UseCooldowns= {0}", CLUSettings.Instance.UseCooldowns);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\cooldownsdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.UseCooldowns = !CLUSettings.Instance.UseCooldowns;
                    CLULogger.Log(" UseCooldowns= {0}", CLUSettings.Instance.UseCooldowns);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\cooldownsenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindClickExtraActionButton \\
            if (IsKeyDown(CLUSettings.Instance.KeybindClickExtraActionButton)) {
                switch (CLUSettings.Instance.ClickExtraActionButton) {
                case true:
                    CLUSettings.Instance.ClickExtraActionButton = !CLUSettings.Instance.ClickExtraActionButton;
                    CLULogger.Log(" ClickExtraActionButton= {0}", CLUSettings.Instance.ClickExtraActionButton);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\dsextraactionbuttondisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.ClickExtraActionButton = !CLUSettings.Instance.ClickExtraActionButton;
                    CLULogger.Log(" ClickExtraActionButton= {0}", CLUSettings.Instance.ClickExtraActionButton);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\dsextraactionbuttonenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindUseAoEAbilities \\
            if (IsKeyDown(CLUSettings.Instance.KeybindUseAoEAbilities)) {
                switch (CLUSettings.Instance.UseAoEAbilities) {
                case true:
                    CLUSettings.Instance.UseAoEAbilities = !CLUSettings.Instance.UseAoEAbilities;
                    CLULogger.Log(" UseAoEAbilities= {0}", CLUSettings.Instance.UseAoEAbilities);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\aoeabilitiesdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.UseAoEAbilities = !CLUSettings.Instance.UseAoEAbilities;
                    CLULogger.Log(" UseAoEAbilities= {0}", CLUSettings.Instance.UseAoEAbilities);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\aoeabilitiesenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindEnableRaidPartyBuffing \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableRaidPartyBuffing)) {
                switch (CLUSettings.Instance.EnableRaidPartyBuffing) {
                case true:
                    CLUSettings.Instance.EnableRaidPartyBuffing = !CLUSettings.Instance.EnableRaidPartyBuffing;
                    CLULogger.Log(" EnableRaidPartyBuffing= {0}", CLUSettings.Instance.EnableRaidPartyBuffing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\raidandpartybuffingdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableRaidPartyBuffing = !CLUSettings.Instance.EnableRaidPartyBuffing;
                    CLULogger.Log(" EnableRaidPartyBuffing= {0}", CLUSettings.Instance.EnableRaidPartyBuffing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\raidandpartybuffingenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindEnableInterupts \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableInterupts)) {
                switch (CLUSettings.Instance.EnableInterupts) {
                case true:
                    CLUSettings.Instance.EnableInterupts = !CLUSettings.Instance.EnableInterupts;
                    CLULogger.Log(" EnableInterupts= {0}", CLUSettings.Instance.EnableInterupts);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\interuptsdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableInterupts = !CLUSettings.Instance.EnableInterupts;
                    CLULogger.Log(" EnableInterupts= {0}", CLUSettings.Instance.EnableInterupts);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\interuptsenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindHealDefensiveManagement \\
            if (IsKeyDown(CLUSettings.Instance.KeybindHealEnableSelfHealing)) {
                switch (CLUSettings.Instance.EnableSelfHealing) {
                case true:
                    CLUSettings.Instance.EnableSelfHealing = !CLUSettings.Instance.EnableSelfHealing;
                    CLULogger.Log(" EnableSelfHealing= {0}", CLUSettings.Instance.EnableSelfHealing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\selfhealingdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableSelfHealing = !CLUSettings.Instance.EnableSelfHealing;
                    CLULogger.Log(" EnableSelfHealing= {0}", CLUSettings.Instance.EnableSelfHealing);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\selfhealingenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindEnableMultiDotting \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableMultiDotting)) {
                switch (CLUSettings.Instance.EnableMultiDotting) {
                case true:
                    CLUSettings.Instance.EnableMultiDotting = !CLUSettings.Instance.EnableMultiDotting;
                    CLULogger.Log(" EnableMultiDotting= {0}", CLUSettings.Instance.EnableMultiDotting);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\multidottingdisabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                case false:
                    CLUSettings.Instance.EnableMultiDotting = !CLUSettings.Instance.EnableMultiDotting;
                    CLULogger.Log(" EnableMultiDotting= {0}", CLUSettings.Instance.EnableMultiDotting);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\multidottingenabled.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindPauseRotation \\
            if (IsKeyDown(CLUSettings.Instance.KeybindPauseRotation)) {
                switch (CLUSettings.Instance.PauseRotation) {
                case true:
                    CLUSettings.Instance.PauseRotation = !CLUSettings.Instance.PauseRotation;
                    CLULogger.Log(" PauseRotation= {0}", CLUSettings.Instance.PauseRotation);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\rotationenabled.wav");
                            SoundManager.SoundPlay();
                        } catch {}

                    }
                    break;
                case false:
                    CLUSettings.Instance.PauseRotation = !CLUSettings.Instance.PauseRotation;
                    CLULogger.Log(" PauseRotation= {0}", CLUSettings.Instance.PauseRotation);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\rotationpaused.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }
                    break;
                default:
                    return;
                }
            }

            // KeybindChakraStanceSelection \\
            if (IsKeyDown(CLUSettings.Instance.KeybindChakraStanceSelection)) {
                switch (CLUSettings.Instance.Priest.ChakraStanceSelection) {
                case ChakraStance.Sanctuary:
                    CLUSettings.Instance.Priest.ChakraStanceSelection = ChakraStance.Serenity;
                    CLULogger.Log(" ChakraStanceSelection= {0}", CLUSettings.Instance.Priest.ChakraStanceSelection);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\chakraserenity.wav");
                            SoundManager.SoundPlay();
                        } catch {}
                    }
                    break;
                case ChakraStance.Serenity:
                    CLUSettings.Instance.Priest.ChakraStanceSelection = ChakraStance.Sanctuary;
                    CLULogger.Log(" ChakraStanceSelection= {0}", CLUSettings.Instance.Priest.ChakraStanceSelection);
                    if (CLUSettings.Instance.EnableKeybindSounds) {
                        try {
                            SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\chakrasanctuary.wav");
                            SoundManager.SoundPlay();
                        } catch { }
                    }

                    break;
                default:
                    return;
                }
            }




            // KeybindEnableMovement \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableMovement))
            {
                switch (CLUSettings.Instance.EnableMovement)
                {
                    case true:
                        CLUSettings.Instance.EnableMovement = !CLUSettings.Instance.EnableMovement;
                        CLULogger.Log(" EnableMovement= {0}", CLUSettings.Instance.EnableMovement);
                        if (CLUSettings.Instance.EnableKeybindSounds)
                        {
                            try
                            {
                                SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\movementdisabled.wav");
                                SoundManager.SoundPlay();
                            }
                            catch { }

                        }
                        break;
                    case false:
                        CLUSettings.Instance.EnableMovement = !CLUSettings.Instance.EnableMovement;
                        CLULogger.Log(" EnableMovement= {0}", CLUSettings.Instance.EnableMovement);
                        if (CLUSettings.Instance.EnableKeybindSounds)
                        {
                            try
                            {
                                SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\movementenabled.wav");
                                SoundManager.SoundPlay();
                            }
                            catch { }
                        }
                        break;
                    default:
                        return;
                }
            }


            // KeybindEnableFists \\
            if (IsKeyDown(CLUSettings.Instance.KeybindEnableFists))
            {
                switch (CLUSettings.Instance.Monk.EnableFists)
                {
                    case true:
                        CLUSettings.Instance.Monk.EnableFists = !CLUSettings.Instance.Monk.EnableFists;
                        CLULogger.Log(" EnableFists= {0}", CLUSettings.Instance.Monk.EnableFists);
                        if (CLUSettings.Instance.EnableKeybindSounds)
                        {
                            try
                            {
                                SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\fistsoffurydisabled.wav");
                                SoundManager.SoundPlay();
                            }
                            catch { }

                        }
                        break;
                    case false:
                        CLUSettings.Instance.Monk.EnableFists = !CLUSettings.Instance.Monk.EnableFists;
                        CLULogger.Log(" EnableFists= {0}", CLUSettings.Instance.Monk.EnableFists);
                        if (CLUSettings.Instance.EnableKeybindSounds)
                        {
                            try
                            {
                                SoundManager.LoadSoundFilePath(@"\Routines\CLU\Sound\fistsoffuryenabled.wav");
                                SoundManager.SoundPlay();
                            }
                            catch { }
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// checks to see if the specified key has been pressed within wow.
        /// </summary>
        /// <param name="key">The key to check for (see Keyboardfunctions)</param>
        /// <returns>true if the player has pressed the key</returns>
        private static bool IsKeyDown(Keyboardfunctions key)
        {
            try {
                if (key == Keyboardfunctions.Nothing) return false;
                var raw = Lua.GetReturnValues("if " + key.ToString("g") + "() then return 1 else return 0 end");
                return raw[0] == "1";
            } catch {
                CLULogger.DiagnosticLog("Lua failed in IsKeyDown" + key.ToString("g"));
                return false;
            }
        }
    }
}
