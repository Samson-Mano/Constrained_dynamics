using String_vibration_openTK.src.fe_objects;
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
    public partial class load_frm : Form
    {

        private fedata_store fe_data;

        enum ActiveTrackbar
        {
            Start,
            End
        }

        public load_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }

        public void initialize_load_form()
        {
            // Set the track bar
            int nodeCount = fe_data.stringintension_data.no_of_nodes;

            trackBar_startnode.Minimum = 0;
            trackBar_endnode.Minimum = 0;

            trackBar_startnode.Maximum = nodeCount - 1;
            trackBar_endnode.Maximum = nodeCount - 1;

            trackBar_startnode.Value = 0;
            trackBar_endnode.Value = nodeCount - 1;

            // Update the label
            UpdateRangeLabels();

            // Set the defaults
            textBox_amplitude.Text = Properties.Settings.Default.Sett_load_ampl.ToString();
            textBox_starttime.Text = Properties.Settings.Default.Sett_load_starttime.ToString();
            textBox_endtime.Text = Properties.Settings.Default.Sett_load_endtime.ToString();

            comboBox_inerpolation.SelectedIndex = Properties.Settings.Default.Sett_load_interpol;
            comboBox_pulse_option.SelectedIndex = Properties.Settings.Default.Sett_load_option;

        }




        private void EnforceNodeRange(ActiveTrackbar active)
        {
            if (active == ActiveTrackbar.Start)
            {
                int end_node_value = Math.Max(trackBar_endnode.Value, trackBar_startnode.Value + 1);

                trackBar_endnode.Value = end_node_value > trackBar_endnode.Maximum ? trackBar_endnode.Maximum : end_node_value;

                trackBar_startnode.Value =
               Math.Min(trackBar_startnode.Value, trackBar_endnode.Value - 1);

            }
            else
            {
                int start_node_value = Math.Min(trackBar_startnode.Value, trackBar_endnode.Value - 1);

                trackBar_startnode.Value = start_node_value < 0 ? 0 : start_node_value;


                trackBar_endnode.Value =
                    Math.Max(trackBar_endnode.Value, trackBar_startnode.Value + 1);

            }

        }


        private void UpdateRangeLabels()
        {
            // Update the label value
            label_startnode.Text = trackBar_startnode.Value.ToString();
            label_endnode.Text = trackBar_endnode.Value.ToString();

        }


        private void trackBar_startnode_Scroll(object sender, EventArgs e)
        {
            EnforceNodeRange(ActiveTrackbar.Start);
            UpdateRangeLabels();

        }

        private void trackBar_endnode_Scroll(object sender, EventArgs e)
        {
            EnforceNodeRange(ActiveTrackbar.End);
            UpdateRangeLabels();

        }

        private void update_dataGridView()
        {


        }


        private void button_add_Click(object sender, EventArgs e)
        {

        }

        private void button_delete_Click(object sender, EventArgs e)
        {

        }

    }
}
