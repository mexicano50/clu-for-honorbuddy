// This file is part of Singular - A community driven Honorbuddy CC
// $Author: raphus $
// $LastChangedBy: wulf $

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
                CLU.Instance.QueryClassTree();
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
                CLU.TroubleshootLog("TalentManager - numTalents {0}", numTalents.ToString(CultureInfo.InvariantCulture));

                for (int index = 0; index <= numTalents; index++)
                {

                    var selected = Lua.GetReturnVal<int>(string.Format("return GetTalentInfo({0})", index), 4);
                    //var talentName = Lua.GetReturnValues("return select(1, GetSpellInfo(" + index + "))")[0];
                    //CLU.TroubleshootLog("TalentManager - Name {10}", talentName);
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
                    if (talent.Name != null)
                    {
                        CLU.TroubleshootLog("TalentManager - talent {0} == Name", talent.Index);
                    }
                }

                Glyphs.Clear();

                var glyphCount = Lua.GetReturnVal<int>("return GetNumGlyphSockets()", 0);

                CLU.TroubleshootLog("TalentManager - GetNumGlyphSockets {0}", glyphCount);

                if (glyphCount != 0)
                {

                    for (int i = 1; i <= glyphCount; i++)
                    {
                        CLU.TroubleshootLog("TalentManager - {0} <= {1}", i, glyphCount);
                        List<string> glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i));
                        //CLU.TroubleshootLog("TalentManager - {0} glyphInfo = {1}", i, glyphInfo[i]);

                        //CLU.TroubleshootLog("TalentManager - {0}, {1}, {2}, {3}, {4}", i, glyphInfo[1], glyphInfo[2]);

                        if (glyphInfo != null && glyphInfo[0] != "nil" && !string.IsNullOrEmpty(glyphInfo[0]))
                        {
                            CLU.TroubleshootLog("TalentManager - {0}", glyphInfo[0]);
                        }

                        if (glyphInfo != null && glyphInfo[1] != "nil" && !string.IsNullOrEmpty(glyphInfo[1]))
                        {
                            CLU.TroubleshootLog("TalentManager - {0}", glyphInfo[1]);
                        }

                        if (glyphInfo != null && glyphInfo[2] != "nil" && !string.IsNullOrEmpty(glyphInfo[2]))
                        {
                            CLU.TroubleshootLog("TalentManager - {0}", glyphInfo[2]);
                        }

                        //if (glyphInfo != null && glyphInfo[2] != "nil" && !string.IsNullOrEmpty(glyphInfo[2]))
                        //{
                        //    CLU.TroubleshootLog("TalentManager -  Glyphs.Add {0}", WoWSpell.FromId(int.Parse(glyphInfo[2])).Name.Replace("Glyph of ", ""));
                        //    Glyphs.Add(WoWSpell.FromId(int.Parse(glyphInfo[2])).Name.Replace("Glyph of ", ""));
                        //}
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