using String_vibration_openTK.Properties;
using String_vibration_openTK.src.fe_objects;
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
            // Set the default values
            textBox_no_of_nodes.Text = fe_data.stringintension_data.no_of_nodes.ToString();    
            textBox_tension.Text = fe_data.stringintension_data.string_tension.ToString();
            textBox_length.Text = fe_data.stringintension_data.string_length.ToString();
            textBox_density.Text = fe_data.stringintension_data.string_density.ToString();

        }

        private void button_update_model_Click(object sender, EventArgs e)
        {
            // Update the model data
            // -------------------------------
            // Validate inputs
            // -------------------------------
            if (!gvariables_static.TryGetPositiveInt(textBox_no_of_nodes, "Number of Nodes", out int no_of_nodes)) return;
            if (!gvariables_static.TryGetPositiveDouble(textBox_tension, "Tension", out double string_tension)) return;
            if (!gvariables_static.TryGetPositiveDouble(textBox_length, "Length", out double string_length)) return;
            if (!gvariables_static.TryGetPositiveDouble(textBox_density, "Density", out double string_density)) return;


            // -------------------------------
            // Update model data
            // -------------------------------
            fe_data.stringintension_data.update_model(no_of_nodes, string_tension, string_length, string_density);

            fe_data.update_string_in_tension_model();

            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }

        }


        private void button_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
