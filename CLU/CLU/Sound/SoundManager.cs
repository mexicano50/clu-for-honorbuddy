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

namespace CLU.Sound
{
    using System.IO;
    using System.Media;

    using Styx.Common;

    public static class SoundManager
    {
        /* A quick class to handle sound for Keybinds */

        private static SoundPlayer player;

        // Sets up the SoundPlayer object.
        public static void Initialize()
        {
            // Create an instance of the SoundPlayer class.
            player = new SoundPlayer();
        }

        /// <summary>
        /// Takes a string in the format of @"\Routines\CLU\Sound\rotationenabled.wav"
        /// and checks if the file exists within the application folder\path
        /// </summary>
        /// <param name="path">the path to the sound file within the applications directory</param>
        public static void LoadSoundFilePath(string path)
        {
            // Make sure we check for existence of the the
            // selected file.
            var fileExists = File.Exists(Utilities.AssemblyDirectory + path); 

            try {
                // Check the file exists.
                if (fileExists) {
                    // Assign the selected file's path to
                    // the SoundPlayer object.
                    player.SoundLocation = Utilities.AssemblyDirectory + path;
                }

                // Load the .wav file.
                player.Load();
            } catch {
            }
        }

        // Asynchronously plays the selected .wav file once.
        public static void SoundPlay()
        {
            player.Play();
        }

        // Stops the currently playing .wav file, if any.
        public static void SoundStop()
        {
            player.Stop();
        }
    }
}
