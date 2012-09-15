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

// This was part of Singular - A community driven Honorbuddy CC

namespace CLU.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Styx;
    using Styx.Combat.CombatRoutine;
    using Styx.WoWInternals;

    using global::CLU.Base;
    using global::CLU.Settings;

    internal static class TalentManager
    {
        static TalentManager()
        {
            Talents = new List<Talent>();
            Glyphs = new HashSet<string>();
            Lua.Events.AttachEvent("CHARACTER_POINTS_CHANGED", UpdateTalentManager);
            Lua.Events.AttachEvent("GLYPH_UPDATED", UpdateTalentManager);
            Lua.Events.AttachEvent("ACTIVE_TALENT_GROUP_CHANGED", UpdateTalentManager);
        }

        public static WoWSpec CurrentSpec { get; private set; }

        public static List<Talent> Talents { get; private set; }

        public static HashSet<string> Glyphs { get; private set; }

        public static int GetCount(int index)
        {
            return Talents.FirstOrDefault(t => t.Index == index).Count;
        }

        /// <summary>
        ///   Checks if we have a glyph or not
        /// </summary>
        /// <param name = "glyphName">Name of the glyph without "Glyph of". i.e. HasGlyph("Aquatic Form")</param>
        /// <returns></returns>
        public static bool HasGlyph(string glyphName)
        {
            return Glyphs.Count > 0 && Glyphs.Contains(glyphName);
        }

        /// <summary>
        ///   Checks if we have the talent or not
        /// </summary>
        /// <param name = "index">The index in the Talent Pane from left to right.</param>
        /// <returns></returns>
        public static bool HasTalent(int index)
        {
            return Talents.FirstOrDefault(t => t.Index == index).Count != 0;
        }



        private static void UpdateTalentManager(object sender, LuaEventArgs args)
        {
            WoWSpec oldSpec = CurrentSpec;

            Update();

            if (CurrentSpec != oldSpec)
            {
                CLU.TroubleshootLog("Your spec has been changed. Rebuilding rotation");
                //SpellManager.Update();
                CLU.Instance.CreateBehaviors();
            }
        }

        public static void Update()
        {
            // Don't bother if we're < 10
            if (StyxWoW.Me.Level < 10)
            {
                CurrentSpec = WoWSpec.None;
                return;
            }

            // Keep the frame stuck so we can do a bunch of injecting at once.
            using (StyxWoW.Memory.AcquireFrame())
            {
                CurrentSpec = StyxWoW.Me.Specialization;

                Talents.Clear();

                var numTalents = Lua.GetReturnVal<int>("return GetNumTalents()", 0);
                for (int index = 0; index <= numTalents; index++)
                {

                    var selected = Lua.GetReturnVal<int>(string.Format("return GetTalentInfo({0})", index), 4);
                    switch (selected)
                    {
                        case 1:
                            {
                                var t = new Talent { Index = index, Count = 1 }; //Name = talentName
                                Talents.Add(t);
                            }
                            break;
                    }
                }

                foreach (var talent in Talents)
                {
                    if (StyxWoW.Me.Class == WoWClass.DeathKnight)
                    {
                        // Set the Blood DK Tier one Talent here
                        switch (talent.Index)
                        {
                            case 1:
                                CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent = DeathKnightTierOneTalent.RoilingBlood;
                                CLU.TroubleshootLog("TalentManager - Setting DeathKnightTierOneTalent to {0}", DeathKnightTierOneTalent.RoilingBlood);
                                break;
                            case 2:
                                CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent = DeathKnightTierOneTalent.PlagueLeech;
                                CLU.TroubleshootLog("TalentManager - Setting DeathKnightTierOneTalent to {0}", DeathKnightTierOneTalent.PlagueLeech);
                                break;
                            case 3:
                                CLUSettings.Instance.DeathKnight.DeathKnightTierOneTalent = DeathKnightTierOneTalent.UnholyBlight;
                                CLU.TroubleshootLog("TalentManager - Setting DeathKnightTierOneTalent to {0}", DeathKnightTierOneTalent.UnholyBlight);
                                break;
                        }
                    }
                }

                Glyphs.Clear();

                var glyphCount = Lua.GetReturnVal<int>("return GetNumGlyphSockets()", 0);

                CLU.TroubleshootLog("Glyphdetection - GetNumGlyphSockets {0}", glyphCount);

                if (glyphCount != 0)
                {

                    for (int i = 1; i <= glyphCount; i++)
                    {
                        List<string> glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i),"glyphs.lua");
                        var lua = String.Format("local enabled, glyphType, glyphTooltipIndex, glyphSpellID, icon = GetGlyphSocketInfo({0});if (enabled) then return glyphSpellID else return 0 end",i);
                        int glyphSpellId = Lua.GetReturnVal<int>(lua,0);
                        try
                        {
                            if (glyphSpellId != null && glyphSpellId > 0)
                            {
                                CLU.TroubleshootLog("Glyphdetection - SpellId: {0},Name:{1} ,WoWSpell: {2}", glyphSpellId, WoWSpell.FromId(glyphSpellId).Name, WoWSpell.FromId(glyphSpellId));
                                Glyphs.Add(WoWSpell.FromId(glyphSpellId).Name.Replace("Glyph of ", ""));
                            }
                            else
                            {
                                CLU.TroubleshootLog("Glyphdetection - Couldn't find all values to detect the Glyph in slot {0}", i);
                            }
                        }
                        catch (Exception ex)
                        {
                            CLU.DiagnosticLog("We couldn't detect your Glyphs");
                            CLU.DiagnosticLog("Report this message to us: " + ex);
                        }
                    }
                }
            }

        }
        public struct Talent
        {
            public int Count;
            public int Index;
            public string Name;
        }
    }
}