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

namespace CLU.GUI
{

    partial class ConfigurationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnSaveAndClose = new System.Windows.Forms.Button();
            this.pgGeneralContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pulsetimer = new System.Windows.Forms.Timer(this.components);
            this.autorefreshtimer = new System.Windows.Forms.Timer(this.components);
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.autorefreshHealingGridchecked = new System.Windows.Forms.CheckBox();
            this.autoselectcurrenttarget = new System.Windows.Forms.CheckBox();
            this.refreshHealingGrid = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.updateListofHealableUnitsGrid = new System.Windows.Forms.Button();
            this.addListofHealableUnitsGrid = new System.Windows.Forms.Button();
            this.removeListofHealableUnitsGrid = new System.Windows.Forms.Button();
            this.clearListofHealableUnitsGrid = new System.Windows.Forms.Button();
            this.pruneListofHealableUnitsGrid = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.HealingGrid = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.Checker = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.debugbutton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.printhealreport = new System.Windows.Forms.Button();
            this.clearstats_button = new System.Windows.Forms.Button();
            this.printreport = new System.Windows.Forms.Button();
            this.targetinfo_grpbox = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.debuffstatus = new System.Windows.Forms.Label();
            this.Value = new System.Windows.Forms.Label();
            this.Key = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pgClass = new System.Windows.Forms.PropertyGrid();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pgGeneral = new System.Windows.Forms.PropertyGrid();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.num_Prayer = new System.Windows.Forms.NumericUpDown();
            this.num_Hyme = new System.Windows.Forms.NumericUpDown();
            this.num_Barrier = new System.Windows.Forms.NumericUpDown();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.HealingtrackBar = new System.Windows.Forms.TrackBar();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.button8 = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.button9 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.footer = new System.Windows.Forms.Panel();
            this.button6 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.pgGeneralContextMenu.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HealingGrid)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.targetinfo_grpbox.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_Prayer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_Hyme)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_Barrier)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HealingtrackBar)).BeginInit();
            this.tabPage7.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.footer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox10.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Location = new System.Drawing.Point(587, 23);
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.Size = new System.Drawing.Size(96, 23);
            this.btnSaveAndClose.TabIndex = 3;
            this.btnSaveAndClose.Text = "Save && Close";
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.BtnSaveAndCloseClick);
            // 
            // pgGeneralContextMenu
            // 
            this.pgGeneralContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToolStripMenuItem});
            this.pgGeneralContextMenu.Name = "pgGeneralContextMenu";
            this.pgGeneralContextMenu.Size = new System.Drawing.Size(149, 26);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.resetToolStripMenuItem.Text = "Future Setting";
            // 
            // pulsetimer
            // 
            this.pulsetimer.Enabled = true;
            this.pulsetimer.Tick += new System.EventHandler(this.PulsetimerTick);
            // 
            // autorefreshtimer
            // 
            this.autorefreshtimer.Interval = 1000;
            this.autorefreshtimer.Tick += new System.EventHandler(this.AutomaticRefreshHealingGridTick);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.groupBox5);
            this.tabPage5.Controls.Add(this.groupBox4);
            this.tabPage5.Controls.Add(this.label5);
            this.tabPage5.Controls.Add(this.label4);
            this.tabPage5.Controls.Add(this.HealingGrid);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(689, 380);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "HealingHelper";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.autorefreshHealingGridchecked);
            this.groupBox5.Controls.Add(this.autoselectcurrenttarget);
            this.groupBox5.Controls.Add(this.refreshHealingGrid);
            this.groupBox5.Location = new System.Drawing.Point(402, 316);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(282, 56);
            this.groupBox5.TabIndex = 36;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Healing Grid Controls";
            // 
            // autorefreshHealingGridchecked
            // 
            this.autorefreshHealingGridchecked.AutoSize = true;
            this.autorefreshHealingGridchecked.Location = new System.Drawing.Point(16, 16);
            this.autorefreshHealingGridchecked.Name = "autorefreshHealingGridchecked";
            this.autorefreshHealingGridchecked.Size = new System.Drawing.Size(168, 17);
            this.autorefreshHealingGridchecked.TabIndex = 28;
            this.autorefreshHealingGridchecked.Text = "Update Current Data Realtime";
            this.autorefreshHealingGridchecked.UseVisualStyleBackColor = true;
            this.autorefreshHealingGridchecked.CheckedChanged += new System.EventHandler(this.autorefreshHealingGridchecked_CheckedChanged);
            // 
            // autoselectcurrenttarget
            // 
            this.autoselectcurrenttarget.AutoSize = true;
            this.autoselectcurrenttarget.Location = new System.Drawing.Point(16, 33);
            this.autoselectcurrenttarget.Name = "autoselectcurrenttarget";
            this.autoselectcurrenttarget.Size = new System.Drawing.Size(149, 17);
            this.autoselectcurrenttarget.TabIndex = 31;
            this.autoselectcurrenttarget.Text = "Auto Select CurrentTarget";
            this.autoselectcurrenttarget.UseVisualStyleBackColor = true;
            // 
            // refreshHealingGrid
            // 
            this.refreshHealingGrid.Location = new System.Drawing.Point(189, 20);
            this.refreshHealingGrid.Name = "refreshHealingGrid";
            this.refreshHealingGrid.Size = new System.Drawing.Size(84, 23);
            this.refreshHealingGrid.TabIndex = 22;
            this.refreshHealingGrid.Text = "Refresh";
            this.refreshHealingGrid.UseVisualStyleBackColor = true;
            this.refreshHealingGrid.Click += new System.EventHandler(this.refreshHealingGrid_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.updateListofHealableUnitsGrid);
            this.groupBox4.Controls.Add(this.addListofHealableUnitsGrid);
            this.groupBox4.Controls.Add(this.removeListofHealableUnitsGrid);
            this.groupBox4.Controls.Add(this.clearListofHealableUnitsGrid);
            this.groupBox4.Controls.Add(this.pruneListofHealableUnitsGrid);
            this.groupBox4.Location = new System.Drawing.Point(6, 316);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(386, 56);
            this.groupBox4.TabIndex = 35;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "List of Healable Units Control";
            // 
            // updateListofHealableUnitsGrid
            // 
            this.updateListofHealableUnitsGrid.Location = new System.Drawing.Point(306, 21);
            this.updateListofHealableUnitsGrid.Name = "updateListofHealableUnitsGrid";
            this.updateListofHealableUnitsGrid.Size = new System.Drawing.Size(68, 23);
            this.updateListofHealableUnitsGrid.TabIndex = 31;
            this.updateListofHealableUnitsGrid.Text = "Update";
            this.updateListofHealableUnitsGrid.UseVisualStyleBackColor = true;
            this.updateListofHealableUnitsGrid.Click += new System.EventHandler(this.updateListofHealableUnitsGrid_Click);
            // 
            // addListofHealableUnitsGrid
            // 
            this.addListofHealableUnitsGrid.Location = new System.Drawing.Point(10, 21);
            this.addListofHealableUnitsGrid.Name = "addListofHealableUnitsGrid";
            this.addListofHealableUnitsGrid.Size = new System.Drawing.Size(68, 23);
            this.addListofHealableUnitsGrid.TabIndex = 25;
            this.addListofHealableUnitsGrid.Text = "Add";
            this.addListofHealableUnitsGrid.UseVisualStyleBackColor = true;
            this.addListofHealableUnitsGrid.Click += new System.EventHandler(this.addHealableUnit_Click);
            // 
            // removeListofHealableUnitsGrid
            // 
            this.removeListofHealableUnitsGrid.Location = new System.Drawing.Point(84, 21);
            this.removeListofHealableUnitsGrid.Name = "removeListofHealableUnitsGrid";
            this.removeListofHealableUnitsGrid.Size = new System.Drawing.Size(68, 23);
            this.removeListofHealableUnitsGrid.TabIndex = 27;
            this.removeListofHealableUnitsGrid.Text = "Remove";
            this.removeListofHealableUnitsGrid.UseVisualStyleBackColor = true;
            this.removeListofHealableUnitsGrid.Click += new System.EventHandler(this.removeUnitHealingGrid_Click);
            // 
            // clearListofHealableUnitsGrid
            // 
            this.clearListofHealableUnitsGrid.Location = new System.Drawing.Point(158, 21);
            this.clearListofHealableUnitsGrid.Name = "clearListofHealableUnitsGrid";
            this.clearListofHealableUnitsGrid.Size = new System.Drawing.Size(68, 23);
            this.clearListofHealableUnitsGrid.TabIndex = 30;
            this.clearListofHealableUnitsGrid.Text = "Clear";
            this.clearListofHealableUnitsGrid.UseVisualStyleBackColor = true;
            this.clearListofHealableUnitsGrid.Click += new System.EventHandler(this.clearUnitHealingGrid_Click);
            // 
            // pruneListofHealableUnitsGrid
            // 
            this.pruneListofHealableUnitsGrid.Location = new System.Drawing.Point(232, 21);
            this.pruneListofHealableUnitsGrid.Name = "pruneListofHealableUnitsGrid";
            this.pruneListofHealableUnitsGrid.Size = new System.Drawing.Size(68, 23);
            this.pruneListofHealableUnitsGrid.TabIndex = 29;
            this.pruneListofHealableUnitsGrid.Text = "Prune";
            this.pruneListofHealableUnitsGrid.UseVisualStyleBackColor = true;
            this.pruneListofHealableUnitsGrid.Click += new System.EventHandler(this.pruneUnitHealingGrid_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(542, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 34;
            this.label5.Text = "HealableUnits:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(9, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 13);
            this.label4.TabIndex = 33;
            this.label4.Text = "Currently Casting:";
            // 
            // HealingGrid
            // 
            this.HealingGrid.AllowUserToOrderColumns = true;
            dataGridViewCellStyle11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(205)))), ((int)(((byte)(217)))));
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(94)))), ((int)(((byte)(114)))));
            this.HealingGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle11;
            this.HealingGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.HealingGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.HealingGrid.Location = new System.Drawing.Point(3, 22);
            this.HealingGrid.Name = "HealingGrid";
            this.HealingGrid.Size = new System.Drawing.Size(681, 288);
            this.HealingGrid.TabIndex = 0;
            this.HealingGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.HealingGridDataError);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Controls.Add(this.targetinfo_grpbox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(689, 380);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Debugging";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.Checker);
            this.groupBox3.Location = new System.Drawing.Point(410, 17);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(270, 350);
            this.groupBox3.TabIndex = 33;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "HealingHelper";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 292);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(186, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Select a row on the healing helper tab";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(22, 313);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 23);
            this.button2.TabIndex = 24;
            this.button2.Text = "Clear";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.Black;
            this.textBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.textBox1.Location = new System.Drawing.Point(7, 20);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(250, 264);
            this.textBox1.TabIndex = 23;
            this.textBox1.WordWrap = false;
            // 
            // Checker
            // 
            this.Checker.Location = new System.Drawing.Point(152, 313);
            this.Checker.Name = "Checker";
            this.Checker.Size = new System.Drawing.Size(90, 23);
            this.Checker.TabIndex = 22;
            this.Checker.Text = "Check";
            this.Checker.UseVisualStyleBackColor = true;
            this.Checker.Click += new System.EventHandler(this.CheckerClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.debugbutton);
            this.groupBox2.Location = new System.Drawing.Point(276, 165);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(122, 67);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ObjectManager";
            // 
            // debugbutton
            // 
            this.debugbutton.Location = new System.Drawing.Point(17, 27);
            this.debugbutton.Name = "debugbutton";
            this.debugbutton.Size = new System.Drawing.Size(84, 23);
            this.debugbutton.TabIndex = 2;
            this.debugbutton.Text = "Update";
            this.debugbutton.UseVisualStyleBackColor = true;
            this.debugbutton.Click += new System.EventHandler(this.DebugbuttonClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.printhealreport);
            this.groupBox1.Controls.Add(this.clearstats_button);
            this.groupBox1.Controls.Add(this.printreport);
            this.groupBox1.Location = new System.Drawing.Point(276, 17);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(122, 142);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CLU Statistal Reports";
            // 
            // printhealreport
            // 
            this.printhealreport.Location = new System.Drawing.Point(18, 65);
            this.printhealreport.Name = "printhealreport";
            this.printhealreport.Size = new System.Drawing.Size(84, 23);
            this.printhealreport.TabIndex = 24;
            this.printhealreport.Text = "Print Healing Report";
            this.printhealreport.UseVisualStyleBackColor = true;
            this.printhealreport.Click += new System.EventHandler(this.PrinthealreportClick);
            // 
            // clearstats_button
            // 
            this.clearstats_button.Location = new System.Drawing.Point(18, 102);
            this.clearstats_button.Name = "clearstats_button";
            this.clearstats_button.Size = new System.Drawing.Size(84, 23);
            this.clearstats_button.TabIndex = 23;
            this.clearstats_button.Text = "Clear Reports";
            this.clearstats_button.UseVisualStyleBackColor = true;
            this.clearstats_button.Click += new System.EventHandler(this.ClearstatsButtonClick);
            // 
            // printreport
            // 
            this.printreport.Location = new System.Drawing.Point(18, 28);
            this.printreport.Name = "printreport";
            this.printreport.Size = new System.Drawing.Size(84, 23);
            this.printreport.TabIndex = 22;
            this.printreport.Text = "Print Report";
            this.printreport.UseVisualStyleBackColor = true;
            this.printreport.Click += new System.EventHandler(this.PrintreportClick);
            // 
            // targetinfo_grpbox
            // 
            this.targetinfo_grpbox.Controls.Add(this.textBox2);
            this.targetinfo_grpbox.Controls.Add(this.label6);
            this.targetinfo_grpbox.Controls.Add(this.button4);
            this.targetinfo_grpbox.Controls.Add(this.button3);
            this.targetinfo_grpbox.Location = new System.Drawing.Point(8, 17);
            this.targetinfo_grpbox.Name = "targetinfo_grpbox";
            this.targetinfo_grpbox.Size = new System.Drawing.Size(253, 350);
            this.targetinfo_grpbox.TabIndex = 31;
            this.targetinfo_grpbox.TabStop = false;
            this.targetinfo_grpbox.Text = "Detailed Target Information";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.Color.Black;
            this.textBox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.textBox2.Location = new System.Drawing.Point(8, 20);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(237, 264);
            this.textBox2.TabIndex = 26;
            this.textBox2.WordWrap = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(31, 292);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(178, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Information about your current target";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(139, 313);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(90, 23);
            this.button4.TabIndex = 26;
            this.button4.Text = "Check";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(16, 313);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 23);
            this.button3.TabIndex = 26;
            this.button3.Text = "Clear";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.AccessibleName = "";
            this.tabPage4.Controls.Add(this.debuffstatus);
            this.tabPage4.Controls.Add(this.Value);
            this.tabPage4.Controls.Add(this.Key);
            this.tabPage4.Controls.Add(this.panel1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(689, 380);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "SpellLockWatcher";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // debuffstatus
            // 
            this.debuffstatus.AutoSize = true;
            this.debuffstatus.Location = new System.Drawing.Point(251, 8);
            this.debuffstatus.Name = "debuffstatus";
            this.debuffstatus.Size = new System.Drawing.Size(39, 13);
            this.debuffstatus.TabIndex = 15;
            this.debuffstatus.Text = "Debuff";
            // 
            // Value
            // 
            this.Value.AutoSize = true;
            this.Value.Location = new System.Drawing.Point(188, 8);
            this.Value.Name = "Value";
            this.Value.Size = new System.Drawing.Size(30, 13);
            this.Value.TabIndex = 14;
            this.Value.Text = "Time";
            // 
            // Key
            // 
            this.Key.AutoSize = true;
            this.Key.Location = new System.Drawing.Point(20, 8);
            this.Key.Name = "Key";
            this.Key.Size = new System.Drawing.Size(35, 13);
            this.Key.TabIndex = 12;
            this.Key.Text = "Name";
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(8, 33);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(316, 334);
            this.panel1.TabIndex = 13;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.pgClass);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(689, 380);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Class Specific";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // pgClass
            // 
            this.pgClass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgClass.Location = new System.Drawing.Point(3, 3);
            this.pgClass.Name = "pgClass";
            this.pgClass.Size = new System.Drawing.Size(683, 374);
            this.pgClass.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pgGeneral);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(689, 380);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pgGeneral
            // 
            this.pgGeneral.ContextMenuStrip = this.pgGeneralContextMenu;
            this.pgGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgGeneral.Location = new System.Drawing.Point(3, 3);
            this.pgGeneral.Name = "pgGeneral";
            this.pgGeneral.Size = new System.Drawing.Size(683, 374);
            this.pgGeneral.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(697, 406);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage6
            // 
            this.tabPage6.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tabPage6.Controls.Add(this.num_Prayer);
            this.tabPage6.Controls.Add(this.num_Hyme);
            this.tabPage6.Controls.Add(this.num_Barrier);
            this.tabPage6.Controls.Add(this.groupBox6);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(689, 380);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Simplified Healing Control";
            // 
            // num_Prayer
            // 
            this.num_Prayer.Location = new System.Drawing.Point(463, 183);
            this.num_Prayer.Name = "num_Prayer";
            this.num_Prayer.Size = new System.Drawing.Size(120, 20);
            this.num_Prayer.TabIndex = 3;
            // 
            // num_Hyme
            // 
            this.num_Hyme.Location = new System.Drawing.Point(463, 120);
            this.num_Hyme.Name = "num_Hyme";
            this.num_Hyme.Size = new System.Drawing.Size(120, 20);
            this.num_Hyme.TabIndex = 2;
            // 
            // num_Barrier
            // 
            this.num_Barrier.Location = new System.Drawing.Point(463, 70);
            this.num_Barrier.Name = "num_Barrier";
            this.num_Barrier.Size = new System.Drawing.Size(120, 20);
            this.num_Barrier.TabIndex = 1;
            // 
            // groupBox6
            // 
            this.groupBox6.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupBox6.Controls.Add(this.label9);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.HealingtrackBar);
            this.groupBox6.Location = new System.Drawing.Point(8, 12);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(354, 353);
            this.groupBox6.TabIndex = 0;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "AoE and Single Target Healing Control";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(308, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "The slider increases or decreases those values *independently*.";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(263, 273);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "AoE Healing";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 273);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Single Target";
            // 
            // HealingtrackBar
            // 
            this.HealingtrackBar.Location = new System.Drawing.Point(24, 289);
            this.HealingtrackBar.Maximum = 100;
            this.HealingtrackBar.Name = "HealingtrackBar";
            this.HealingtrackBar.Size = new System.Drawing.Size(305, 45);
            this.HealingtrackBar.TabIndex = 0;
            this.HealingtrackBar.Scroll += new System.EventHandler(this.HealingtrackBar_Scroll);
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.groupBox10);
            this.tabPage7.Controls.Add(this.groupBox9);
            this.tabPage7.Controls.Add(this.groupBox8);
            this.tabPage7.Controls.Add(this.groupBox7);
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(689, 380);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "Spell/Aura/Target Checker";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.button8);
            this.groupBox9.Location = new System.Drawing.Point(384, 16);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(200, 100);
            this.groupBox9.TabIndex = 36;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Spell Checker";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(57, 36);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(84, 23);
            this.button8.TabIndex = 0;
            this.button8.Text = "SpellInfo";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.DisplaySpellchecker);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.button9);
            this.groupBox8.Controls.Add(this.button7);
            this.groupBox8.Controls.Add(this.button5);
            this.groupBox8.Location = new System.Drawing.Point(242, 16);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(122, 169);
            this.groupBox8.TabIndex = 35;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Dump to Log";
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(17, 100);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(84, 23);
            this.button9.TabIndex = 35;
            this.button9.Text = "GroupInfo";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.DumpGroupMembers);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(17, 21);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(84, 23);
            this.button7.TabIndex = 2;
            this.button7.Text = "Spells";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.DumpSpells);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(17, 61);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(84, 23);
            this.button5.TabIndex = 34;
            this.button5.Text = "Auras";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.DumpAuras);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.button1);
            this.groupBox7.Location = new System.Drawing.Point(20, 16);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(200, 100);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "TargetInfo + Target Aura Info";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(55, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 23);
            this.button1.TabIndex = 34;
            this.button1.Text = "TargetInfo";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.TargetinfoButtonClick);
            // 
            // footer
            // 
            this.footer.BackColor = System.Drawing.SystemColors.Control;
            this.footer.Controls.Add(this.button6);
            this.footer.Controls.Add(this.pictureBox1);
            this.footer.Controls.Add(this.label1);
            this.footer.Controls.Add(this.label2);
            this.footer.Controls.Add(this.lblVersion);
            this.footer.Controls.Add(this.btnSaveAndClose);
            this.footer.Location = new System.Drawing.Point(0, 405);
            this.footer.Name = "footer";
            this.footer.Size = new System.Drawing.Size(697, 58);
            this.footer.TabIndex = 5;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(485, 23);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(96, 23);
            this.button6.TabIndex = 34;
            this.button6.Text = "SwapRotation";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(5, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(50, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Impact", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(53, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "CLU";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Codified Likeness Utility";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(55, 40);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(46, 13);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "v0.1.0.0";
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.label15);
            this.groupBox10.Controls.Add(this.label14);
            this.groupBox10.Controls.Add(this.label13);
            this.groupBox10.Controls.Add(this.label12);
            this.groupBox10.Controls.Add(this.label11);
            this.groupBox10.Controls.Add(this.label10);
            this.groupBox10.Controls.Add(this.button12);
            this.groupBox10.Controls.Add(this.button11);
            this.groupBox10.Controls.Add(this.button10);
            this.groupBox10.Location = new System.Drawing.Point(481, 147);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(198, 221);
            this.groupBox10.TabIndex = 35;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "DPSMeter";
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(98, 56);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(84, 23);
            this.button10.TabIndex = 34;
            this.button10.Text = "Initialize";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(98, 85);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(84, 23);
            this.button11.TabIndex = 35;
            this.button11.Text = "Shutdown";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(98, 114);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(84, 23);
            this.button12.TabIndex = 36;
            this.button12.Text = "Update";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(111, 25);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(28, 13);
            this.label10.TabIndex = 37;
            this.label10.Text = "XXX";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(65, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 38;
            this.label11.Text = "Status:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(65, 159);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 39;
            this.label12.Text = "GetDps_Me:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(139, 159);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(37, 13);
            this.label13.TabIndex = 40;
            this.label13.Text = "00000";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 185);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(125, 13);
            this.label14.TabIndex = 41;
            this.label14.Text = "GetCombatTimeLeft_Me:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(139, 185);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(37, 13);
            this.label15.TabIndex = 42;
            this.label15.Text = "00000";
            // 
            // ConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 463);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.footer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ConfigurationForm";
            this.Text = "CLU Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigurationFormFormClosing);
            this.Load += new System.EventHandler(this.ConfigurationForm_Load);
            this.pgGeneralContextMenu.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HealingGrid)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.targetinfo_grpbox.ResumeLayout(false);
            this.targetinfo_grpbox.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.num_Prayer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_Hyme)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_Barrier)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HealingtrackBar)).EndInit();
            this.tabPage7.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.footer.ResumeLayout(false);
            this.footer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSaveAndClose;
        private System.Windows.Forms.Timer pulsetimer;
        private System.Windows.Forms.Timer autorefreshtimer;
        private System.Windows.Forms.ContextMenuStrip pgGeneralContextMenu;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button clearListofHealableUnitsGrid;
        private System.Windows.Forms.Button pruneListofHealableUnitsGrid;
        private System.Windows.Forms.CheckBox autorefreshHealingGridchecked;
        private System.Windows.Forms.Button removeListofHealableUnitsGrid;
        private System.Windows.Forms.Button addListofHealableUnitsGrid;
        private System.Windows.Forms.Button refreshHealingGrid;

        public System.Windows.Forms.DataGridView HealingGrid;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button debugbutton;
        private System.Windows.Forms.GroupBox targetinfo_grpbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button printhealreport;
        private System.Windows.Forms.Button clearstats_button;
        private System.Windows.Forms.Button printreport;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label debuffstatus;
        private System.Windows.Forms.Label Value;
        private System.Windows.Forms.Label Key;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.PropertyGrid pgClass;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PropertyGrid pgGeneral;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Panel footer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox autoselectcurrenttarget;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button Checker;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button updateListofHealableUnitsGrid;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar HealingtrackBar;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown num_Prayer;
        private System.Windows.Forms.NumericUpDown num_Hyme;
        private System.Windows.Forms.NumericUpDown num_Barrier;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label15;
    }
}