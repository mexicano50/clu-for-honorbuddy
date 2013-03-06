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

#endregion Revision info

namespace CLU.CombatLog
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Styx.WoWInternals;
    using Styx.WoWInternals.WoWObjects;

    internal class CombatLogEventArgs : LuaEventArgs
    {
        public CombatLogEventArgs(string eventName, uint fireTimeStamp, object[] args)
            : base(eventName, fireTimeStamp, args)
        {
        }

        public double Timestamp
        {
            get
            {
                return (double)this.Args[0];
            }
        }

        public string Event
        {
            get
            {
                return this.Args[1].ToString();
            }
        }

        // Is this a string? bool? what? What the hell is it even used for?
        // it's a boolean, and it doesn't look like it has any real impact codewise apart from maybe to break old addons? - exemplar 4.1
        public string HideCaster
        {
            get
            {
                return this.Args[2].ToString();
            }
        }

        public ulong SourceGuid
        {
            get
            {
                if (Args[3].ToString() == "") return 0;
                return ulong.Parse(Args[3].ToString().Replace("0x", string.Empty), NumberStyles.HexNumber);
            }
        }

        public WoWUnit SourceUnit
        {
            get
            {
                return
                    ObjectManager.GetObjectsOfType<WoWUnit>(true, true).FirstOrDefault(
                        o => o.IsValid && (o.Guid == this.SourceGuid || o.DescriptorGuid == this.SourceGuid));
            }
        }

        public string SourceName
        {
            get
            {
                return this.Args[4].ToString();
            }
        }

        public int SourceFlags
        {
            get
            {
                return (int)(double)this.Args[5];
            }
        }

        public ulong DestGuid
        {
            get
            {
                return ulong.Parse(this.Args[7].ToString().Replace("0x", string.Empty), NumberStyles.HexNumber);
            }
        }

        public WoWUnit DestUnit
        {
            get
            {
                return
                    ObjectManager.GetObjectsOfType<WoWUnit>(true, true).FirstOrDefault(
                        o => o.IsValid && (o.Guid == this.DestGuid || o.DescriptorGuid == this.DestGuid));
            }
        }

        public string DestName
        {
            get
            {
                return this.Args[8].ToString();
            }
        }

        public int DestFlags
        {
            get
            {
                return (int)(double)this.Args[9];
            }
        }

        public int SpellId
        {
            get
            {
                return (int)(double)this.Args[11];
            }
        }

        public WoWSpell Spell
        {
            get
            {
                return WoWSpell.FromId(this.SpellId);
            }
        }

        public string SpellName
        {
            get
            {
                return this.Args[12].ToString();
            }
        }

        public WoWSpellSchool SpellSchool
        {
            get
            {
                return (WoWSpellSchool)(int)(double)this.Args[13];
            }
        }

        public object[] SuffixParams
        {
            get
            {
                var args = new List<object>();
                for (int i = 11; i < this.Args.Length; i++)
                {
                    if (this.Args[i] != null)
                    {
                        args.Add(args[i]);
                    }
                }

                return args.ToArray();
            }
        }
    }
}