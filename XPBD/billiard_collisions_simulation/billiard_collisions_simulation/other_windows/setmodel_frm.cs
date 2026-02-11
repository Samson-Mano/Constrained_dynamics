using billiard_collisions_simulation.src.fe_objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using billiard_collisions_simulation.src.global_variables;


namespace billiard_collisions_simulation.other_windows
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
            textBox_no_of_balls.Text = Properties.Settings.Default.Sett_number_of_balls.ToString();

            textBox_min_radius.Text = Properties.Settings.Default.Sett_minimum_radius.ToString();
            textBox_max_radius.Text = Properties.Settings.Default.Sett_maximum_radius.ToString();


        }

        private void button_update_model_Click(object sender, EventArgs e)
        {

            // -------------------------------
            // Validate inputs
            // -------------------------------
            if (!gvariables_static.TryGetPositiveInteger(textBox_no_of_balls, "Number of balls", out int no_of_balls)) return;

            if (!gvariables_static.TryGetPositiveDouble(textBox_min_radius, "Minimum Radius", out double min_radius)) return;
            if (!gvariables_static.TryGetPositiveDouble(textBox_max_radius, "Maximum Radius", out double max_radius)) return;


            // Set the billiard ball model
            this.fe_data.set_billiardball_model(no_of_balls, min_radius, max_radius);



            // -------------------------------
            // Update settings
            // -------------------------------
            Properties.Settings.Default.Sett_number_of_balls = no_of_balls;

            Properties.Settings.Default.Sett_minimum_radius = min_radius;
            Properties.Settings.Default.Sett_maximum_radius = max_radius;

            Properties.Settings.Default.Save();



        }




    }
}
