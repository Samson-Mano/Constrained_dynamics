using OpenTK.Graphics.ES11;
using PBD_pendulum_simulation.src.global_variables;
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
    public partial class option_frm : Form
    {

        private bool isInitializing = false;

        public option_frm()
        {
            InitializeComponent();

            // Assign check box to function
            checkBox_show_velocity_vector.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_acceleration_vector.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            
            checkBox_show_displ_graph.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_velo_graph.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_accl_graph.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

            checkBox_show_time_label.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_displ_label.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_velo_label.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_accl_label.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

            checkBox_show_phase_portrait.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

            checkBox_show_larmour_fields.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);
            checkBox_show_fields_around_circle.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

            checkBox_show_mass_trail.CheckedChanged += new EventHandler(checkBoxes_CheckedChanged);

        }

        public void initialize_option_form()
        {
            isInitializing = true;

            // Initialize check box states based on global variables
            checkBox_show_velocity_vector.Checked = gvariables_static.is_show_velocity_vector;
            checkBox_show_acceleration_vector.Checked = gvariables_static.is_show_acceleration_vector;

            checkBox_show_displ_graph.Checked = gvariables_static.is_show_displacement_graph;
            checkBox_show_velo_graph.Checked = gvariables_static.is_show_velocity_graph;
            checkBox_show_accl_graph.Checked = gvariables_static.is_show_acceleration_graph;

            checkBox_show_time_label.Checked = gvariables_static.is_show_time_label;
            checkBox_show_displ_label.Checked = gvariables_static.is_show_displacement_label;
            checkBox_show_velo_label.Checked = gvariables_static.is_show_velocity_label;
            checkBox_show_accl_label.Checked = gvariables_static.is_show_acceleration_label;

            checkBox_show_phase_portrait.Checked = gvariables_static.is_show_phaseportrait;

            checkBox_show_larmour_fields.Checked  = gvariables_static.is_show_larmour_field;
            checkBox_show_fields_around_circle.Checked  = gvariables_static.is_show_field_around_circle;

            checkBox_show_mass_trail.Checked = gvariables_static.is_show_masstrail;

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

            gvariables_static.is_show_displacement_graph = checkBox_show_displ_graph.Checked;
            gvariables_static.is_show_velocity_graph = checkBox_show_velo_graph.Checked;
            gvariables_static.is_show_acceleration_graph = checkBox_show_accl_graph.Checked;

            gvariables_static.is_show_time_label = checkBox_show_time_label.Checked;
            gvariables_static.is_show_displacement_label = checkBox_show_displ_label.Checked;
            gvariables_static.is_show_velocity_label = checkBox_show_velo_label.Checked;
            gvariables_static.is_show_acceleration_label = checkBox_show_accl_label.Checked;

            gvariables_static.is_show_phaseportrait = checkBox_show_phase_portrait.Checked;

            gvariables_static.is_show_larmour_field = checkBox_show_larmour_fields.Checked;
            gvariables_static.is_show_field_around_circle = checkBox_show_fields_around_circle.Checked;

            gvariables_static.is_show_masstrail = checkBox_show_mass_trail.Checked;

        }


    }
}
