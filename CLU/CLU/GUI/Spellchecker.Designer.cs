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

namespace CLU.GUI
{
    partial class Spellchecker
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.spellid_input = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.spellname_txt = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.smCooldownTimeLeft_lbl = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.smCooldownid_lbl = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.smCooldownTimeLeftid_lbl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.smIsValid_lbl = new System.Windows.Forms.Label();
            this.spellCancast_lbl = new System.Windows.Forms.Label();
            this.spellname_lbl = new System.Windows.Forms.Label();
            this.smCanCast_lbl = new System.Windows.Forms.Label();
            this.smHasSpell_lbl = new System.Windows.Forms.Label();
            this.spellid_lbl = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spellid_input)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.spellid_input);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.spellname_txt);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(551, 86);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Spell Input";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(229, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Enter Spell ID";
            // 
            // spellid_input
            // 
            this.spellid_input.Location = new System.Drawing.Point(229, 40);
            this.spellid_input.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.spellid_input.Name = "spellid_input";
            this.spellid_input.Size = new System.Drawing.Size(120, 20);
            this.spellid_input.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Enter Spell name";
            // 
            // spellname_txt
            // 
            this.spellname_txt.Location = new System.Drawing.Point(20, 41);
            this.spellname_txt.Name = "spellname_txt";
            this.spellname_txt.Size = new System.Drawing.Size(165, 20);
            this.spellname_txt.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Location = new System.Drawing.Point(13, 105);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(554, 191);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.smCooldownTimeLeft_lbl);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.smCooldownid_lbl);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.smCooldownTimeLeftid_lbl);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.smIsValid_lbl);
            this.panel1.Controls.Add(this.spellCancast_lbl);
            this.panel1.Controls.Add(this.spellname_lbl);
            this.panel1.Controls.Add(this.smCanCast_lbl);
            this.panel1.Controls.Add(this.smHasSpell_lbl);
            this.panel1.Controls.Add(this.spellid_lbl);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(548, 172);
            this.panel1.TabIndex = 0;
            // 
            // smCooldownTimeLeft_lbl
            // 
            this.smCooldownTimeLeft_lbl.AutoSize = true;
            this.smCooldownTimeLeft_lbl.Location = new System.Drawing.Point(199, 67);
            this.smCooldownTimeLeft_lbl.Name = "smCooldownTimeLeft_lbl";
            this.smCooldownTimeLeft_lbl.Size = new System.Drawing.Size(22, 13);
            this.smCooldownTimeLeft_lbl.TabIndex = 20;
            this.smCooldownTimeLeft_lbl.Text = "xxx";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(22, 67);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(166, 13);
            this.label12.TabIndex = 18;
            this.label12.Text = "SpellManager.CooldownTimeLeft:";
            // 
            // smCooldownid_lbl
            // 
            this.smCooldownid_lbl.AutoSize = true;
            this.smCooldownid_lbl.Location = new System.Drawing.Point(450, 115);
            this.smCooldownid_lbl.Name = "smCooldownid_lbl";
            this.smCooldownid_lbl.Size = new System.Drawing.Size(22, 13);
            this.smCooldownid_lbl.TabIndex = 16;
            this.smCooldownid_lbl.Text = "xxx";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(292, 115);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(111, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "WoWSpell.Cooldown:";
            // 
            // smCooldownTimeLeftid_lbl
            // 
            this.smCooldownTimeLeftid_lbl.AutoSize = true;
            this.smCooldownTimeLeftid_lbl.Location = new System.Drawing.Point(450, 139);
            this.smCooldownTimeLeftid_lbl.Name = "smCooldownTimeLeftid_lbl";
            this.smCooldownTimeLeftid_lbl.Size = new System.Drawing.Size(22, 13);
            this.smCooldownTimeLeftid_lbl.TabIndex = 14;
            this.smCooldownTimeLeftid_lbl.Text = "xxx";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(292, 139);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "WoWSpell.CooldownTimeLeft:";
            // 
            // smIsValid_lbl
            // 
            this.smIsValid_lbl.AutoSize = true;
            this.smIsValid_lbl.Location = new System.Drawing.Point(450, 92);
            this.smIsValid_lbl.Name = "smIsValid_lbl";
            this.smIsValid_lbl.Size = new System.Drawing.Size(22, 13);
            this.smIsValid_lbl.TabIndex = 12;
            this.smIsValid_lbl.Text = "xxx";
            // 
            // spellCancast_lbl
            // 
            this.spellCancast_lbl.AutoSize = true;
            this.spellCancast_lbl.Location = new System.Drawing.Point(450, 70);
            this.spellCancast_lbl.Name = "spellCancast_lbl";
            this.spellCancast_lbl.Size = new System.Drawing.Size(22, 13);
            this.spellCancast_lbl.TabIndex = 11;
            this.spellCancast_lbl.Text = "xxx";
            // 
            // spellname_lbl
            // 
            this.spellname_lbl.AutoSize = true;
            this.spellname_lbl.Location = new System.Drawing.Point(450, 45);
            this.spellname_lbl.Name = "spellname_lbl";
            this.spellname_lbl.Size = new System.Drawing.Size(22, 13);
            this.spellname_lbl.TabIndex = 10;
            this.spellname_lbl.Text = "xxx";
            // 
            // smCanCast_lbl
            // 
            this.smCanCast_lbl.AutoSize = true;
            this.smCanCast_lbl.Location = new System.Drawing.Point(199, 45);
            this.smCanCast_lbl.Name = "smCanCast_lbl";
            this.smCanCast_lbl.Size = new System.Drawing.Size(22, 13);
            this.smCanCast_lbl.TabIndex = 9;
            this.smCanCast_lbl.Text = "xxx";
            // 
            // smHasSpell_lbl
            // 
            this.smHasSpell_lbl.AutoSize = true;
            this.smHasSpell_lbl.Location = new System.Drawing.Point(199, 20);
            this.smHasSpell_lbl.Name = "smHasSpell_lbl";
            this.smHasSpell_lbl.Size = new System.Drawing.Size(22, 13);
            this.smHasSpell_lbl.TabIndex = 8;
            this.smHasSpell_lbl.Text = "xxx";
            // 
            // spellid_lbl
            // 
            this.spellid_lbl.AutoSize = true;
            this.spellid_lbl.Location = new System.Drawing.Point(450, 20);
            this.spellid_lbl.Name = "spellid_lbl";
            this.spellid_lbl.Size = new System.Drawing.Size(22, 13);
            this.spellid_lbl.TabIndex = 7;
            this.spellid_lbl.Text = "xxx";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(292, 92);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "WoWSpell.IsValid:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(292, 70);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "WoWSpell.Cancast:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(292, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "WoWSpell.Name:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(292, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "WoWSpell.ID:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(118, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "SpellManager.CanCast:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "SpellManager.HasSpell:";
            // 
            // Timer1
            // 
            this.Timer1.Enabled = true;
            this.Timer1.Interval = 500;
            this.Timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // Spellchecker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 308);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Spellchecker";
            this.ShowIcon = false;
            this.Text = "Spellchecker";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spellid_input)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown spellid_input;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox spellname_txt;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Timer Timer1;
        private System.Windows.Forms.Label spellid_lbl;
        private System.Windows.Forms.Label smHasSpell_lbl;
        private System.Windows.Forms.Label smCanCast_lbl;
        private System.Windows.Forms.Label spellname_lbl;
        private System.Windows.Forms.Label spellCancast_lbl;
        private System.Windows.Forms.Label smIsValid_lbl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label smCooldownTimeLeftid_lbl;
        private System.Windows.Forms.Label smCooldownid_lbl;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label smCooldownTimeLeft_lbl;
        private System.Windows.Forms.Label label12;

    }
}