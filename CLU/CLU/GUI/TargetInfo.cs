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

using System;
using System.Windows.Forms;
using Styx.WoWInternals;

namespace CLU.GUI
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;


    using Styx.WoWInternals.WoWObjects;
    using global::CLU.Base;
    using Styx;

    public partial class TargetInfo : Form
    {
        private static TargetInfo instance = new TargetInfo();
        // private static readonly HealableUnit Myself = new HealableUnit(StyxWoW.Me).TimeToLive(2);

        public static void Display()
        {
            if (instance == null || instance.IsDisposed)
                instance = new TargetInfo();
            if (!instance.Visible)
                instance.Show();
        }

        private TargetInfo()
        {
            this.InitializeComponent();
            GUIHelpers.SetDoubleBuffered(this.textBox1);
        }

        private void update()
        {

            var target = ObjectManager.Me.CurrentTarget;
            var color = StyxWoW.Me.IsBehind(target) ? Color.DarkGreen : Color.Red;


            tname.Text = target == null ? "<No target>" : target.Name;
            tguid.Text = target == null ? string.Empty : target.Guid.ToString(CultureInfo.InvariantCulture);
            distance.Text = target == null ? string.Empty : Math.Round(target.Distance, 3).ToString(CultureInfo.InvariantCulture);
            realDistance.Text = target == null ? string.Empty : Math.Round(Unit.DistanceToTargetBoundingBox(target), 3).ToString(CultureInfo.InvariantCulture);
            issafelybehindtarget.ForeColor = color;
            issafelybehindtarget.Text = (target != null && StyxWoW.Me.IsBehind(target)).ToString(CultureInfo.InvariantCulture);
            IsWithinMeleeRange_lbl.Text = (target != null && MeleeRangeCheck(target)).ToString(CultureInfo.InvariantCulture);
            // ttl_label.Text = new HealableUnit(StyxWoW.Me.CurrentTarget).TimeToLive(4).ToString(CultureInfo.InvariantCulture);


            if (target != null) {
                // facing
                var me = ObjectManager.Me.Location;
                var ta = target.Location;
                facing.Text = Math.Round(Unit.FacingTowardsUnitDegrees(me, ta), 2) + "°";
            }

            if (checkBox1.Checked)
            {
                checkaura();
                // if (StyxWoW.Me.Class == WoWClass.DeathKnight) checkrunes(); enable this and it will kill HB.
            }

        }

        /// <summary>
        /// Small check fo melee range.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private bool MeleeRangeCheck(WoWUnit unit)
        {
            return unit.IsWithinMeleeRange;
        }

        void Timer1Tick(object sender, EventArgs e)
        {
            try {
                this.update();
            } catch { }
        }

        private void checkaura()
        {
            textBox1.Clear();
            string dump = null;
            var unit = checkmybuffs.Checked ? StyxWoW.Me : StyxWoW.Me.CurrentTarget;
            if (unit != null)
            {
                foreach (KeyValuePair<string, WoWAura> au in unit.Auras)
                {
                    WoWAura aura;
                    if (unit.Auras.TryGetValue(au.Key, out aura)) // unit.CreatedByUnitGuid 
                    {
                        dump = dump + "Name: " + aura.Name + " ID: " + aura.SpellId + " StackCount: " + aura.StackCount + " TimeLeft: " + aura.TimeLeft + " Duration: " + aura.Duration + " CreatorGuid: " + aura.CreatorGuid + " Me.Guid: " + StyxWoW.Me.Guid + "\r\n";
                    }
                    else
                    {
                        dump = dump + au.Key + "\r\n";
                    }
                }
            }

            textBox1.Text += dump;
        }

        public void checkrunes()
        {
            var blood1Lblcolor = Spell.IsRuneCooldown(1) ? Color.DarkGreen : Color.Red;
            var blood2Lblcolor = Spell.IsRuneCooldown(2) ? Color.DarkGreen : Color.Red;
            var frost1Lblcolor = Spell.IsRuneCooldown(3) ? Color.DarkGreen : Color.Red;
            var frost2Lblcolor = Spell.IsRuneCooldown(4) ? Color.DarkGreen : Color.Red;
            var unholy1Lblcolor = Spell.IsRuneCooldown(5) ? Color.DarkGreen : Color.Red;
            var unholy2Lblcolor = Spell.IsRuneCooldown(6) ? Color.DarkGreen : Color.Red;


            blood1lbl.ForeColor = blood1Lblcolor;
            blood2lbl.ForeColor = blood2Lblcolor;
            Frost1lbl.ForeColor = frost1Lblcolor;
            Frost2lbl.ForeColor = frost2Lblcolor;
            Unholy1lbl.ForeColor = unholy1Lblcolor;
            Unholy2lbl.ForeColor = unholy2Lblcolor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkaura();
            if (StyxWoW.Me.Class == WoWClass.DeathKnight) checkrunes();
        }
    }
}
