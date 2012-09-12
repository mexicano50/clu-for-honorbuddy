#region Revision Info

// This file was part of Singular - A community driven Honorbuddy CC
// $Author$
// $LastChangedBy$

#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

using CLU.Helpers;
using CLU.Settings;

using Styx;
using Styx.Combat.CombatRoutine;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Styx.Helpers;

namespace CLU.GUI
{
    using Styx.Common;
    using Styx.CommonBot;

    using global::CLU.Base;
    using global::CLU.CombatLog;

    public partial class ConfigurationForm : Form
    {
        private static LocalPlayer Me
        {
            get {
                return ObjectManager.Me;
            }
        }

        private static ConfigurationForm _instance;

        public static ConfigurationForm Instance
        {
            get {
                return _instance ?? (_instance = new ConfigurationForm());
            }
        }

        private List<int> maxAoEHealingValues;

        private static BindingSource healingGridBindingSource;

        public ConfigurationForm()
        {
            this.InitializeComponent();
            this.paint(); // Paints the spellLockWatcher
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {
            lblVersion.Text = string.Format("Version: {0}", CLU.Version);

            // setup the bindings
            pgGeneral.SelectedObject = CLUSettings.Instance;
            CLUSettings main = CLUSettings.Instance;
            Styx.Helpers.Settings toSelect = null;
            switch (StyxWoW.Me.Class) {
            case WoWClass.Warrior:
                toSelect = main.Warrior;
                break;
            case WoWClass.Paladin:
                toSelect = main.Paladin;
                break;
            case WoWClass.Hunter:
                toSelect = main.Hunter;
                break;
            case WoWClass.Rogue:
                toSelect = main.Rogue;
                break;
            case WoWClass.Priest:
                toSelect = main.Priest;
                break;
            case WoWClass.DeathKnight:
                toSelect = main.DeathKnight;
                break;
            case WoWClass.Shaman:
                toSelect = main.Shaman;
                break;
            case WoWClass.Mage:
                toSelect = main.Mage;
                break;
            case WoWClass.Warlock:
                toSelect = main.Warlock;
                break;
            case WoWClass.Druid:
                toSelect = main.Druid;
                break;
            //TODO: Monk Settings
            //case WoWClass.Monk:
            //    toSelect = main.Monk;
            //    break;

            default:
                break;
            }

            if (toSelect != null) {
                pgClass.SelectedObject = toSelect;
            }

            // Initialize shit
            this.InitializeHealingGrid();
            this.InitializeStyle();
        }

        /// <summary>
        /// Add some style to the GUI.
        /// </summary>
        private void InitializeStyle()
        {
            // Add a bit of style
            footer.BackColor = Color.FromArgb(70, 73, 88);
            footer.ForeColor = Color.FromArgb(230, 232, 242);
            btnSaveAndClose.ForeColor = Color.FromArgb(0, 0, 1);
            button6.ForeColor = Color.FromArgb(0, 0, 1);

            pgGeneral.HelpBackColor = Color.FromArgb(203, 205, 217); // Description
            pgGeneral.HelpForeColor = Color.FromArgb(0, 0, 1); // Description
            // pgGeneral.BackColor = Color.FromArgb(203, 205, 217); // Category
            pgGeneral.ForeColor = Color.FromArgb(66, 69, 84); // Category
            pgGeneral.LineColor = Color.FromArgb(203, 205, 217); // Category
            // pgGeneral.ViewBackColor = Color.FromArgb(66, 69, 84); // properties
            // pgGeneral.ViewForeColor = Color.FromArgb(230, 232, 242); // properties

            pgClass.HelpBackColor = Color.FromArgb(203, 205, 217); // Description
            pgClass.HelpForeColor = Color.FromArgb(0, 0, 1); // Description
            // pgClass.BackColor = Color.FromArgb(203, 205, 217); // Category
            pgClass.ForeColor = Color.FromArgb(66, 69, 84); // Category
            pgClass.LineColor = Color.FromArgb(203, 205, 217); // Category
            // pgClass.ViewBackColor = Color.FromArgb(66, 69, 84); // properties
            // pgClass.ViewForeColor = Color.FromArgb(230, 232, 242); // properties

            textBox1.BackColor = Color.FromArgb(70, 73, 88);
            textBox1.ForeColor = Color.FromArgb(230, 232, 242);
            textBox2.BackColor = Color.FromArgb(70, 73, 88);
            textBox2.ForeColor = Color.FromArgb(230, 232, 242);

            // hide the toolbar
            pgGeneral.ToolbarVisible = false;
            pgClass.ToolbarVisible = false;

            // Collapse All Grid Items
            pgGeneral.CollapseAllGridItems();
            pgClass.CollapseAllGridItems();

           // CLU.Log(Utilities.AssemblyDirectory); //todo: fix me

            if (File.Exists(Utilities.AssemblyDirectory + @"\Routines\CLU\GUI\Resources\CLU.png"))
            {
                pictureBox1.ImageLocation = Utilities.AssemblyDirectory + @"\Routines\CLU\GUI\Resources\CLU.png";
            }

            if (File.Exists(Utilities.AssemblyDirectory + @"\Routines\CLU\GUI\Resources\CLU.ico"))
            {
                this.Icon = new Icon(Utilities.AssemblyDirectory + @"\Routines\CLU\GUI\Resources\CLU.ico");
            }

            // AoE and Single target healing control ============================================================
            const string Firstrowtext = "Ok, so this is how it works. The slider below is linked to";
            const string Secondrowtext = "the AoE Healing abilities maxAverageHealthPercent for your";
            const string Thirdrowtext = "given class. You can still set the maxAverageHealthPercent";
            const string Fourthrowtext = "values for the individual AoE abilitys in the Class Specific Tab.";
            const string Fifthhrowtext = "The slider increases or decreases those values *independently*.";
            // Environment.NewLine
            const string Eighthhrowtext = "Currently this control is only available for Discipline/AA Priests.";
            // Environment.NewLine
            const string Eleventhhrowtext = "Moving the Slider closer to single target will reduce the";
            const string Twelthhrowtext = "maxAverageHealthPercent values thus allowing more single";
            const string Thirteenthhrowtext = "target healing to be done.";


            label9.Text = Firstrowtext + Environment.NewLine +
                          Secondrowtext + Environment.NewLine +
                          Thirdrowtext + Environment.NewLine +
                          Fourthrowtext + Environment.NewLine +
                          Fifthhrowtext + Environment.NewLine + Environment.NewLine +
                          Eighthhrowtext + Environment.NewLine + Environment.NewLine +
                          Eleventhhrowtext + Environment.NewLine +
                          Twelthhrowtext + Environment.NewLine +
                          Thirteenthhrowtext + Environment.NewLine;

            // setup initial Maximum, Minimum, and Value
            // HealingtrackBar.Minimum = 0;
            // HealingtrackBar.Maximum = 100;

            maxAoEHealingValues = new List<int> { CLUSettings.Instance.Priest.PowerWordBarrierPartymaxAH, CLUSettings.Instance.Priest.DivineHymnPartymaxAH, CLUSettings.Instance.Priest.PrayerofHealingPartymaxAH };
            double median = Median(this.maxAoEHealingValues);
            HealingtrackBar.Value = (int)median;

            // setup LargeChange and SmallChange
            HealingtrackBar.SmallChange = 2; //  keyboard arrows
            HealingtrackBar.LargeChange = 10; // page up and page down keys

            // TickFrequency and TickStyle
            HealingtrackBar.TickFrequency = 1;
            HealingtrackBar.TickStyle = TickStyle.BottomRight;

            // Attaching TrackBar to a Control
            this.HealingtrackBar.Scroll += this.HealingtrackBar_Scroll;

            HealingtrackBar.SetRange(40, 98);

            num_Barrier.Value = CLUSettings.Instance.Priest.PowerWordBarrierPartymaxAH;
            num_Hyme.Value = CLUSettings.Instance.Priest.DivineHymnPartymaxAH;
            num_Prayer.Value = CLUSettings.Instance.Priest.PrayerofHealingPartymaxAH;

            // =================================================================================================

            // Set the panel double buffered for smoother updates.
            GUIHelpers.SetDoubleBuffered(this.panel1);
            GUIHelpers.SetDoubleBuffered(this.HealingGrid);
        }


        /// <summary>
        /// Sets up the healing grid and binds our datasource to listOfHealableUnits
        /// </summary>
        private void InitializeHealingGrid()
        {
            try {
                HealingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                // Automatically generate the DataGridView columns.
                HealingGrid.AutoGenerateColumns = true;

                // Automatically resize the visible rows.
                HealingGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

                // Automatically resize the visible columns
                HealingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

                // Set the DataGridView control's border.
                HealingGrid.BorderStyle = BorderStyle.Fixed3D;

                // Put the cells in edit mode when user enters them.
                HealingGrid.EditMode = DataGridViewEditMode.EditOnEnter;

                // Set the RowHeaders visibility to invisible
                HealingGrid.RowHeadersVisible = false;

                // Dissalow user from adding rows
                HealingGrid.AllowUserToAddRows = false;

                // Clipboard copying.
                HealingGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

                // Bind to our Datasource (BindingList)
                healingGridBindingSource = new BindingSource { DataSource = HealableUnit.ListofHealableUnits };
                HealingGrid.DataSource = healingGridBindingSource;

                // Create events for HealingGrid:
                HealingGrid.CurrentCellDirtyStateChanged += this.HealingGrid_CurrentCellDirtyStateChanged;
                //HealingGrid.CellMouseClick += this.HealingGrid_OnCellMouseUp;
            } catch (Exception ex) {
                CLU.DiagnosticLog("InitializeHealingGrid : {0}", ex);
            }
        }



        /// <summary>
        /// Adds our current target to our listOfHealableUnits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addHealableUnit_Click(object sender, EventArgs e)
        {
            try {
                // Add a couple of HealableUnits to the list.
                if (Me.CurrentTarget != null && HealableUnit.Filter(Me.CurrentTarget)) {
                    if (!HealableUnit.Contains(Me.CurrentTarget)) {
                        CLU.TroubleshootLog( " Adding: {0} because of user request.", CLU.SafeName(Me.CurrentTarget));
                        HealableUnit.ListofHealableUnits.Add(new HealableUnit(Me.CurrentTarget));
                    }
                } else {
                    MessageBox.Show(
                        "Please make sure that you have a CurrentTarget",
                        "Important Note",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                }
                this.RefreshDataGridView();
            } catch (Exception ex) {
                CLU.DiagnosticLog("addHealableUnit_Click : {0}", ex);
            }
        }

        /// <summary>
        /// Removes a unit from our listOfHealableUnits by the currently selected row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeUnitHealingGrid_Click(object sender, EventArgs e)
        {
            try {

                if (this.HealingGrid.SelectedRows.Count > 0) {
                    CLU.TroubleshootLog( " Removing: {0} because of user request.", this.HealingGrid.SelectedRows[0].Cells[6].Value);
                    HealableUnit.ListofHealableUnits.RemoveAt(this.HealingGrid.SelectedRows[0].Index);
                } else {
                    MessageBox.Show(
                        "Please select a row to delete",
                        "Important Note",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                }
                this.RefreshDataGridView(); // update
            } catch (Exception ex) {
                this.RefreshDataGridView(); // update
                CLU.DiagnosticLog("removeUnitHealingGrid_Click : {0}", ex);
            }
        }

        /// <summary>
        /// Refreshs the HealingGrid
        /// </summary>
        private void refreshHealingGrid_Click(object sender, EventArgs e)
        {
            this.RefreshDataGridView();
        }

        private void RefreshDataGridView()
        {
            HealingGrid.DataSource = null;

            if (healingGridBindingSource.Count != 0) {
                HealingGrid.DataSource = healingGridBindingSource;
                HealingGrid.Refresh();
                //HealingGrid.Show();
                // HealingGrid.ClearSelection();
                // this.ResetGridColor();
            }
        }

        /// <summary>
        /// A check box to Auto refresh the HealingGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autorefreshHealingGridchecked_CheckedChanged(object sender, EventArgs e)
        {
            this.autorefreshtimer.Enabled = this.autorefreshHealingGridchecked.Checked;
        }

        /// <summary>
        ///  Prune the listOfHealableUnits that meet a criteria
        /// </summary>
        private void pruneUnitHealingGrid_Click(object sender, EventArgs e)
        {
            HealableUnit.Remove();
            HealingGrid.Refresh();
        }

        /// <summary>
        /// Clear the listOfHealableUnits
        /// </summary>
        private void clearUnitHealingGrid_Click(object sender, EventArgs e)
        {
            HealableUnit.ListofHealableUnits.Clear();
        }

        /// <summary>
        /// Update the listOfHealableUnits
        /// </summary>
        private void updateListofHealableUnitsGrid_Click(object sender, EventArgs e)
        {
            CLU.TroubleshootLog( "User clicked update - Re-Initialize list Of HealableUnits");
            HealableUnit.ListofHealableUnits.Clear();
            switch (CLUSettings.Instance.SelectedHealingAquisition) {
            case HealingAquisitionMethod.Proximity:
                HealableUnit.HealableUnitsByProximity();
                break;
            case HealingAquisitionMethod.RaidParty:
                HealableUnit.HealableUnitsByPartyorRaid();
                break;
            }
        }

        // No errors please!
        private void HealingGridDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        /// <summary>
        /// create events for HealingGrid
        /// </summary>
        void HealingGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (HealingGrid.IsCurrentCellDirty) {
                HealingGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// create events for HealingtrackBar
        /// </summary>
        private void HealingtrackBar_Scroll(object sender, EventArgs e)
        {
            try {
                // Get values

                // Do the calculation
                // Assign the results to text boxes
                double median = Median(this.maxAoEHealingValues);

                int barriervalue = this.GetNewValue(
                                       this.HealingtrackBar.Minimum,
                                       this.maxAoEHealingValues.Max(),
                                       this.HealingtrackBar.Value,
                                       this.HealingtrackBar.Minimum,
                                       this.HealingtrackBar.Maximum);

                CLUSettings.Instance.Priest.PowerWordBarrierPartymaxAH = barriervalue > this.HealingtrackBar.Maximum
                        ? this.HealingtrackBar.Maximum
                        : barriervalue;

                num_Barrier.Value = barriervalue > this.HealingtrackBar.Maximum
                                    ? this.HealingtrackBar.Maximum
                                    : barriervalue;

                int hymevalue = this.GetNewValue(
                                    this.HealingtrackBar.Minimum,
                                    (int)median,
                                    this.HealingtrackBar.Value,
                                    this.HealingtrackBar.Minimum,
                                    this.HealingtrackBar.Maximum);

                CLUSettings.Instance.Priest.DivineHymnPartymaxAH = hymevalue > this.HealingtrackBar.Maximum
                        ? this.HealingtrackBar.Maximum
                        : hymevalue;

                num_Hyme.Value = barriervalue > this.HealingtrackBar.Maximum
                                 ? this.HealingtrackBar.Maximum
                                 : hymevalue;

                int numprayer = this.GetNewValue(
                                    this.HealingtrackBar.Minimum,
                                    this.maxAoEHealingValues.Min(),
                                    this.HealingtrackBar.Value,
                                    this.HealingtrackBar.Minimum,
                                    this.HealingtrackBar.Maximum);
                CLUSettings.Instance.Priest.PrayerofHealingPartymaxAH = numprayer > this.HealingtrackBar.Maximum
                        ? this.HealingtrackBar.Maximum
                        : numprayer;

                num_Prayer.Value = barriervalue > this.HealingtrackBar.Maximum
                                   ? this.HealingtrackBar.Maximum
                                   : numprayer;
            } catch {

            }
        }

        private static double Median(IEnumerable<int> numbers)
        {
            var sorted = (from n in numbers
                          orderby n ascending
                          select n).ToList();

            int middle = (int)(sorted.Count() + 1) / 2;

            if (sorted.Count() % 2 != 0) {
                return sorted[middle - 1];
            } else {
                return (sorted[middle - 1] + sorted[middle]) / 2D;
            }
        }

        //Calculate a trackbar value(newValue) based on the other trackbar value(oldValue).
        private int GetNewValue(int oldBegin, int oldEnd, int oldValue, int newBegin, int newEnd)
        {
            double oldPosition = (oldValue - oldBegin) * 1.0 / (oldEnd - oldBegin);
            var newValue = (int)(oldPosition * (newEnd - newBegin) + newBegin);
            return newValue;
        }

        /// <summary>
        /// Saves and Closes the Configuration window.
        /// </summary>
        private void BtnSaveAndCloseClick(object sender, EventArgs e)
        {
            // prevent an exception from closing HB.
            try {
                ((Styx.Helpers.Settings)pgGeneral.SelectedObject).Save();
                if (pgClass.SelectedObject != null) {
                    ((Styx.Helpers.Settings)pgClass.SelectedObject).Save();
                }
                this.Close();
            } catch (Exception ex) {
                CLU.DiagnosticLog("ERROR saving settings: {0}", ex);
            }
        }

        /// <summary>
        /// Debug Button
        /// </summary>
        private void DebugbuttonClick(object sender, EventArgs e)
        {
            ObjectManager.Update();
        }

        /// <summary>
        /// Displays information about your current target
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetinfoButtonClick(object sender, EventArgs e)
        {
            TargetInfo.Display();
        }

        /// <summary>
        /// Prints the WoWstats Report
        /// </summary>
        private void PrintreportClick(object sender, EventArgs e)
        {
            WoWStats.Instance.PrintReport();
        }

        /// <summary>
        /// Prints the Healing part of the report
        /// </summary>
        private void PrinthealreportClick(object sender, EventArgs e)
        {
            WoWStats.Instance.PrintHealReport();
        }

        /// <summary>
        /// Clears all the WoWstats
        /// </summary>
        private void ClearstatsButtonClick(object sender, EventArgs e)
        {
            WoWStats.Instance.ClearStats();
        }

        /// <summary>
        /// Handle Form closing Events such as stop timers etc.
        /// </summary>
        private void ConfigurationFormFormClosing(object sender, FormClosingEventArgs e)
        {
        }

        /// <summary>
        /// Timer to update SpellLockWatcher and HealingGrid updates
        /// </summary>
        private void PulsetimerTick(object sender, EventArgs e)
        {
            try {
                if (Me.IsValid && StyxWoW.IsInGame) {
                    this.paint(); // SpellLockWatcher

                    // HealingGrid
                    if (Me.CastingSpell != null) {
                        if (Me.CurrentTarget != null) {
                            this.label4.Text = "Casting: " + Me.CastingSpell.Name + " on " + Me.CurrentTarget;
                        } else {
                            this.label4.Text = "Casting: " + Me.CastingSpell.Name;
                        }
                    } else {
                        this.label4.Text = "Casting: " + "Waiting...";
                    }

                    label5.Text = "HealableUnits: " + HealableUnit.ListofHealableUnits.Count;

                    if (this.autoselectcurrenttarget.Checked) {
                        this.TargetChange();
                    }
                }

            } catch { }

        }

        /// <summary>
        /// HealingGrid functions/colors
        /// </summary>
        private void TargetChange()
        {
            DataGridViewCellStyle RedCellStyle = null;
            RedCellStyle = new DataGridViewCellStyle { ForeColor = Color.Red };
            DataGridViewCellStyle GreenCellStyle = null;
            GreenCellStyle = new DataGridViewCellStyle { ForeColor = Color.Green };


            foreach (DataGridViewRow playerRow in HealingGrid.Rows) {
                if (Me.CurrentTarget != null && playerRow.Cells["UnitName"].Value.ToString().Contains(Me.CurrentTarget.Name)) {
                    playerRow.Selected = true;
                } else {
                    playerRow.Selected = false;
                }
            }
        }

        private void HealthChange()
        {
            var RedCellStyle = new DataGridViewCellStyle { ForeColor = Color.White, BackColor = Color.Red };

            var GreenCellStyle = new DataGridViewCellStyle { ForeColor = Color.Green };


            foreach (DataGridViewRow playerRow in HealingGrid.Rows) {
                if (Me.CurrentTarget != null && playerRow.Cells["MissingHealth"].Value.ToString().Equals("0")) {
                    playerRow.DefaultCellStyle = GreenCellStyle;
                } else {
                    playerRow.DefaultCellStyle = RedCellStyle;
                }
            }
        }
		
        // resets the colors of the healing grid back to default blacks and whites
        private void ResetGridColor()
        {
            var ResetCellStyle = new DataGridViewCellStyle
            { ForeColor = Color.Black, BackColor = Color.White };


            foreach (DataGridViewRow playerRow in HealingGrid.Rows) {
                playerRow.DefaultCellStyle = ResetCellStyle;
            }
        }

        /// <summary>
        /// Timer to update HealingGrid
        /// </summary>
        private void AutomaticRefreshHealingGridTick(object sender, EventArgs e)
        {
            try {
                HealingGrid.Refresh();
            } catch { }

        }


        /// <summary>
        /// Paints the WoWSpellLockWatcher information to the panel
        /// </summary>
        private void paint()
        {
            this.panel1.Controls.Clear();

            var W = 316;
            var dy = 20;

            var y = 0;
            var locks = CombatLogEvents.Instance.DumpSpellLocks();

            var col1 = 10;
            var col2 = (int)(col1 + (W - 20.0) * 0.6);
            var col3 = (int)(col2 + (W - 20.0) * 0.4 / 2.0);

            foreach (var x in locks) {
                var color = x.Value <= 0 ? Color.DarkGreen : Color.Red;

                var A = new Label {
                    Text = x.Key,
                    ForeColor = color,
                    Size = new Size(col2 - col1, dy),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(col1, y)
                };

                var B = new Label {
                    Text = x.Value.ToString(CultureInfo.InvariantCulture),
                    ForeColor = color,
                    Size = new Size(col3 - col2, dy),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(col2, y)
                };

                var C = new Label {
                    Text = Me.CurrentTarget != null && Buff.TargetHasDebuff(x.Key) ? "Up" : "Down",
                    ForeColor = color,
                    Size = new Size(100, dy),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(col3, y)
                };

                this.panel1.Controls.Add(A);
                this.panel1.Controls.Add(B);
                this.panel1.Controls.Add(C);

                y += dy;
            }
        }
		
        // Provides information from the healing helper window on the selected row item (displays it on teh debug tab.)
        private void CheckerClick(object sender, EventArgs e)
        {

            try {
                if (this.HealingGrid.SelectedRows.Count > 0) {
                    for (int i = 0; i < this.HealingGrid.SelectedRows[0].Cells.Count; i++) {
                        textBox1.Text += string.Format(" {0} : {1}", this.HealingGrid.SelectedRows[0].Cells[i].OwningColumn.HeaderText, this.HealingGrid.SelectedRows[0].Cells[i].Value)  + Environment.NewLine;
                    }

                } else {
                    MessageBox.Show(
                        "Please select a row to gather information. check the debug log for the output.",
                        "Important Note for Healing Helper",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                }
            } catch (Exception ex) {
                CLU.DiagnosticLog("Checker_Click : {0}", ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        // Displays information about the current target on the debug tab
        private void button4_Click(object sender, EventArgs e)
        {
            try {
                var target = ObjectManager.Me.CurrentTarget;

                string output = target == null ? "<No target>" : "Name : " + target.Name;
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "Guid : " + target.Guid.ToString(CultureInfo.InvariantCulture);
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "Distance : " + Math.Round(target.Distance, 3).ToString(CultureInfo.InvariantCulture);
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "DistanceToTargetBoundingBox : " + Math.Round(Unit.DistanceToTargetBoundingBox(target), 3).ToString(CultureInfo.InvariantCulture);
                textBox2.Text += output + Environment.NewLine;
                output = "IsBehind : " + (target != null && StyxWoW.Me.IsBehind(target)).ToString(CultureInfo.InvariantCulture);
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "ThreatInfo : " + target.ThreatInfo.RawPercent.ToString(CultureInfo.InvariantCulture);
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "Location : " + target.Location;
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "SingleTargethealingParty : " + CLUSettings.Instance.Priest.SingleTargethealingParty;
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "PrayerofHealingPartymaxAH : " + CLUSettings.Instance.Priest.PrayerofHealingPartymaxAH;
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "Me.CurrentHolyPower : " + Me.CurrentHolyPower;
                textBox2.Text += output + Environment.NewLine;
                output = target == null ? string.Empty : "PetSlotSelection : " + (int)CLUSettings.Instance.Hunter.PetSlotSelection;
                textBox2.Text += output + Environment.NewLine;
                //output = target == null ? string.Empty : "Thunder Clap (" + SpellManager.Spells["Thunder Clap"].Id + ") Cooldown : " + Spell.SpellCooldown("Thunder Clap").TotalSeconds;
                //textBox2.Text += output + Environment.NewLine;
                //output = target == null ? string.Empty : "Thunder Clap Cooldown : " + Spell.CooldownTimeLeft6343;
                //textBox2.Text += output + Environment.NewLine;
                //output = target == null ? string.Empty : "Explosive Shot (" + SpellManager.Spells["Explosive Shot"].Id + ") Cooldown : " + Spell.SpellCooldown("Explosive Shot").TotalSeconds;
                //textBox2.Text += output + Environment.NewLine;
                //output = target == null ? string.Empty : "Explosive Shot Cooldown : " + Spell.CooldownTimeLeft33878;
                //textBox2.Text += output + Environment.NewLine;

                


                
                

              


                if (target != null) {
                    // facing
                    var me = ObjectManager.Me.Location;
                    var ta = target.Location;
                    output = "FacingTowardsUnitDegrees : " + Math.Round(Unit.FacingTowardsUnitDegrees(me, ta), 2) + "°";
                }
                textBox2.Text += output + Environment.NewLine;
            } catch (Exception ex) {
                CLU.DiagnosticLog("Current Target Information : {0}", ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
		
        // Dumps the spells from the current target to the debug tab in honorbuddy
        private void DumpSpells(object sender, EventArgs e)
        {
            Spell.DumpSpells();
        }
        
		// Dumps the auras from the current target to the debug tab in honorbuddy
        private void DumpAuras(object sender, EventArgs e)
        {
            Buff.DumpAuras();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CLU.Instance.QueryClassTree();
        }

        private void DisplaySpellchecker(object sender, EventArgs e)
        {
            Spellchecker.Display();
        }
    }
}