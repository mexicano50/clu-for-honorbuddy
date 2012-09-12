using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using Styx.TreeSharp;
using CLU.Base;
using CLU.Classes;


namespace CLU.GUI
{

    public static class GUIHelpers
    {


        /// <summary>
        ///  set protected property Control.Double­Buffered to true This is useful, if
        ///  you want to avoid flickering of controls such as ListView (when updating)
        ///  or Panel (when you draw on it).
        /// </summary>
        /// <param name="control">the control to set DoubleBuffered</param>
        public static void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember(
                "DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                control,
                new object[] { true });
        }

        /// <summary>
        /// Gets the Enum string value from the Combo Box
        /// </summary>
        /// <param name="cmbobox">the combo box to retreive the Enum</param>
        /// <returns>the string value at the selected index of the combo box</returns>
        private static string GetEnumValues(ComboBox cmbobox)
        {
            var type = (EnumWithName<Keyboardfunctions>)cmbobox.SelectedItem;
            return type.ToString();
        }

        /// <summary>
        /// map Enum values to a string
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public class EnumWithName<T>
        {
            private string Name
            {
                get;
                set;
            }

            private T Value
            {
                get;
                set;
            }

            public static EnumWithName<T>[] ParseEnum()
            {
                return (from object o in Enum.GetValues(typeof(T)) select new EnumWithName<T> { Name = Enum.GetName(typeof(T), o).Replace('_', ' '), Value = (T)o }).ToArray();
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private static void ProcessControls(Control ctrlContainer, ComboBox cmbobox)
        {
            foreach (Control ctrl in ctrlContainer.Controls) {
                if (ctrl.GetType() == typeof(ComboBox)) {
                    try {
                        if (ctrl.Name != cmbobox.Name) {
                            bool matchs = ((ComboBox)ctrl).SelectedValue.ToString() == cmbobox.SelectedValue.ToString();
                            if (matchs && ((ComboBox)ctrl).SelectedValue.ToString() != "Nothing") {
                                CLU.TroubleshootLog(" [Keybind Match] {0} == {1}. Please Make another selection", ctrl.Name, cmbobox.Name);
                                cmbobox.SelectedIndex = cmbobox.FindStringExact("Nothing");
                            }
                        }
                    } catch {
                    }
                }
                // if a control contains sub controls check
                if (ctrl.HasChildren) {
                    ProcessControls(ctrl, cmbobox);
                }
            }
        }

        // Dynamic Dialog
        public static DialogResult RotationSelector(string title, List<RotationBase> rotations, string headerText, ref string value)
        {
            var form = new Form();
            var buttonOk = new Button();
            var buttonCancel = new Button();
            var header = new Label();
            string radioClick = null;

            int Y = 40;
            int X = 25;
            int columnX = 290;
            foreach (RotationBase rotation in rotations) {
                var radio = new RadioButton { Text = rotation.Name };
                var label = new Label { Text = "" };
                try {
                    Composite tmp = rotation.SingleRotation;
                    label.Text += (label.Text == "" ? "" : ", ") + "Single";
                } catch (NotImplementedException) {
                }
                try {
                    Composite tmp = rotation.PVERotation;
                    label.Text += (label.Text == "" ? "" : ", ") + "Party, Raid";
                } catch (NotImplementedException) {
                }
                try {
                    Composite tmp = rotation.PVPRotation;
                    label.Text += (label.Text == "" ? "" : ", ") + "PvP";
                } catch (NotImplementedException) {
                }

                if (label.Text == "") {
                    label.Text = "Not supported";
                    label.ForeColor = Color.Red;
                }

                radio.Location = new Point(X, Y);
                radio.Width = columnX - X;
                radio.Height = 20;
                radio.TextAlign = ContentAlignment.TopLeft;

                label.Location = new Point(columnX, Y);
                label.Width = 435 - columnX;
                label.Height = 20;
                label.TextAlign = ContentAlignment.TopLeft;

                radio.Click += delegate(object sender, EventArgs e) {
                    var tmpradio = sender as RadioButton;
                    if (tmpradio != null) {
                        radioClick = tmpradio.Text;
                    }
                };

                form.Controls.Add(radio);
                form.Controls.Add(label);

                Y += radio.Height;
            }

            form.Text = title;
            form.ClientSize = new Size(435, Y + 80);
            form.MaximumSize = form.ClientSize;
            form.MinimumSize = form.ClientSize;

            header.Text = headerText;
            header.AutoSize = true;
            header.SetBounds(9, 20, 372, 13);

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonOk.SetBounds(248, Y + 10, 75, 23);
            buttonCancel.SetBounds(329, Y + 10, 75, 23);
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.Controls.AddRange(new Control[] { header, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = radioClick;
            return dialogResult;
        }
    }
}
