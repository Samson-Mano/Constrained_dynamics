using PBD_pendulum_simulation.Properties;
using PBD_pendulum_simulation.src.fe_objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PBD_pendulum_simulation.other_windows
{
    public partial class setmodel_frm : Form
    {
        private fedata_store fe_data;

        public setmodel_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }

        public void initialize_model_form()
        {
            // Set the mass value
            textBox_mass1.Text = Settings.Default.sett_mass1.ToString();
            textBox_mass2.Text = Settings.Default.sett_mass2.ToString();
            textBox_mass3.Text = Settings.Default.sett_mass3.ToString();

            // Set the length value
            textBox_length1.Text = Settings.Default.sett_length1.ToString();
            textBox_length2.Text = Settings.Default.sett_length2.ToString();
            textBox_length3.Text = Settings.Default.sett_length3.ToString();

            // Set the track bar
            trackBar_initialangle1.Value = (int)Settings.Default.sett_initialangle1;
            trackBar_initialangle2.Value = (int)Settings.Default.sett_initialangle2;
            trackBar_initialangle3.Value = (int)Settings.Default.sett_initialangle3;

            label_initialangle1.Text = trackBar_initialangle1.Value.ToString();
            label_initialangle2.Text = trackBar_initialangle2.Value.ToString();
            label_initialangle3.Text = trackBar_initialangle3.Value.ToString();

        }


        private void button_update_Click(object sender, EventArgs e)
        {
            // -------------------------------
            // Validate inputs
            // -------------------------------
            if (!TryGetPositiveDouble(textBox_mass1, "Mass 1", out double m1)) return;
            if (!TryGetPositiveDouble(textBox_mass2, "Mass 2", out double m2)) return;
            if (!TryGetPositiveDouble(textBox_mass3, "Mass 3", out double m3)) return;

            if (!TryGetPositiveDouble(textBox_length1, "Length 1", out double l1)) return;
            if (!TryGetPositiveDouble(textBox_length2, "Length 2", out double l2)) return;
            if (!TryGetPositiveDouble(textBox_length3, "Length 3", out double l3)) return;

            // Angles come from trackbars → already numeric
            double a1 = trackBar_initialangle1.Value;
            double a2 = trackBar_initialangle2.Value;
            double a3 = trackBar_initialangle3.Value;

            // -------------------------------
            // Update settings
            // -------------------------------
            Settings.Default.sett_mass1 = m1;
            Settings.Default.sett_mass2 = m2;
            Settings.Default.sett_mass3 = m3;

            Settings.Default.sett_length1 = l1;
            Settings.Default.sett_length2 = l2;
            Settings.Default.sett_length3 = l3;

            Settings.Default.sett_initialangle1 = a1;
            Settings.Default.sett_initialangle2 = a2;
            Settings.Default.sett_initialangle3 = a3;

            Settings.Default.Save();

            // -------------------------------
            // Update model data
            // -------------------------------
            fe_data.set_triple_pendulum_model(
                m1, m2, m3,
                l1, l2, l3,
                a1, a2, a3);

            //// -------------------------------
            //// Feedback
            //// -------------------------------
            //MessageBox.Show(
            //    "Model updated successfully.",
            //    "Update Complete",
            //    MessageBoxButtons.OK,
            //    MessageBoxIcon.Information);


        }

        private void trackBar_initialangle1_Scroll(object sender, EventArgs e)
        {
            label_initialangle1.Text = trackBar_initialangle1.Value.ToString();
        }

        private void trackBar_initialangle2_Scroll(object sender, EventArgs e)
        {
            label_initialangle2.Text = trackBar_initialangle2.Value.ToString();
        }

        private void trackBar_initialangle3_Scroll(object sender, EventArgs e)
        {
            label_initialangle3.Text = trackBar_initialangle3.Value.ToString();
        }


        private bool TryGetPositiveDouble(TextBox tb, string name, out double value)
        {
            if (!double.TryParse(tb.Text, out value))
            {
                MessageBox.Show(
                    $"{name} must be a numeric value.",
                    "Invalid Input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                tb.Focus();
                return false;
            }

            if (value <= 0)
            {
                MessageBox.Show(
                    $"{name} must be a positive number.",
                    "Invalid Input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                tb.Focus();
                return false;
            }

            return true;
        }


    }
}
