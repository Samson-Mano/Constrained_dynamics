using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace String_vibration_openTK.other_windows
{
    public partial class option_frm : Form
    {

        private bool isInitializing = false;


        public option_frm()
        {
            InitializeComponent();

            // Assign check box to function
            checkBox_show_velocity_vector.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_acceleration_vector.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

            checkBox_show_time_label.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

        }


        public void initialize_option_form()
        {
            isInitializing = true;

            // Initialize check box states based on global variables
            checkBox_show_velocity_vector.Checked = gvariables_static.is_show_velocity_vector;
            checkBox_show_acceleration_vector.Checked = gvariables_static.is_show_acceleration_vector;

            checkBox_show_time_label.Checked = gvariables_static.is_show_time_label;

            isInitializing = false;

        }


        private void button_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void checkBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (isInitializing)
                return;    // prevent updates while loading

            // Update global variables based on check box states
            gvariables_static.is_show_velocity_vector = checkBox_show_velocity_vector.Checked;
            gvariables_static.is_show_acceleration_vector = checkBox_show_acceleration_vector.Checked;

            gvariables_static.is_show_time_label = checkBox_show_time_label.Checked;

        }


    }
}
