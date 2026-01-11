using OpenTK;
using SharpFont;
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
    public partial class load_frm : Form
    {

        private fedata_store fe_data;

        enum ActiveTrackbar
        {
            Start,
            End
        }

        enum InterpolationType
        {
            Triangular = 0,
            CubicBezier = 1,
            Sine = 2,
            Rectangular = 3,
            SingleNode = 4
        }


        public load_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

            // Set the data Grid View
            dataGridView_load.ReadOnly = true;
            dataGridView_load.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_load.MultiSelect = false;

            dataGridView_load.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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

            // Update the datagrid view
            update_dataGridView();


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
            dataGridView_load.Rows.Clear();

            // Add the following data to dataGridView for each update
            foreach (loaddata_store load in fe_data.stringintension_data.load_data)
            {
                string load_typeStr = "";

                // Half Sine Pulse
                // Rectangular Pulse
                // Triangular Pulse
                // Step Force with Finite Rise
                // Harmonic Excitation

                switch (load.load_type)
                {
                    case 0:
                        {
                            load_typeStr = "Half Sine Pulse";
                            break;
                        }
                    case 1:
                        {
                            load_typeStr = "Rectangular Pulse";
                            break;
                        }
                    case 2:
                        {
                            load_typeStr = "Triangular Pulse";
                            break;
                        }
                    case 3:
                        {
                            load_typeStr = "Step Force with Finite Rise";
                            break;
                        }
                    case 4:
                        {
                            load_typeStr = "Harmonic excitation";
                            break;
                        }
                }


                int load_id = load.load_id;


                int start_node = load.load_nodes[0];
                int end_node = load.load_nodes[load.load_nodes.Count - 1];

                string maxValStr = load.abs_max_value.ToString("F4"); ;


                dataGridView_load.Rows.Add(
                               load_id,
                               load_typeStr,
                               start_node,
                               end_node,
                               load.abs_max_value,
                               load.load_start_time,
                               load.load_end_time);

            }


        }


        private void button_add_Click(object sender, EventArgs e)
        {
            // Check whether the inputs are valid
            // -------------------------------
            // Validate inputs
            // -------------------------------
            if (!gvariables_static.TryGetPositiveDouble(textBox_amplitude, "Load amplitude", out double ld_ampl)) return;
            if (!gvariables_static.TryGetPositiveDouble(textBox_starttime, "Load start time", out double ld_starttime)) return;
            if (!gvariables_static.TryGetPositiveDouble(textBox_endtime, "Load end time", out double ld_endtime)) return;

            int node_start = trackBar_startnode.Value;
            int node_end = trackBar_endnode.Value;


            // int interpolation_type = comboBox_interpolation.SelectedIndex;

            // 0 - Triangular Interpolation
            // 1 - Cubic bezier Interpolation
            // 2 - Sine Interpolation
            // 3 - Rectangular Interpolation
            // 4 - Single Node

            List<int> load_nodes = new List<int>();
            List<double> load_values = new List<double>();

            InterpolationType interpolationType = (InterpolationType)comboBox_inerpolation.SelectedIndex;


            switch (interpolationType)
            {
                case InterpolationType.Triangular:
                    {
                        // 0 - Triangular Interpolation
                        TriangularInterpolationValues(node_start, node_end, ld_ampl, out load_nodes, out load_values);
                        break;
                    }
                case InterpolationType.CubicBezier:
                    {
                        // 1 - Cubic bezier Interpolation
                        CubicBezierInterpolationValues(node_start, node_end, ld_ampl, out load_nodes, out load_values);
                        break;
                    }
                case InterpolationType.Sine:
                    {
                        // 2 - Sine Interpolation
                        SineInterpolationValues(node_start, node_end, ld_ampl, out load_nodes, out load_values);
                        break;
                    }
                case InterpolationType.Rectangular:
                    {
                        // 3 - Rectangular Interpolation
                        RectangularInterpolationValues(node_start, node_end, ld_ampl, out load_nodes, out load_values);
                        break;
                    }
                case InterpolationType.SingleNode:
                    {
                        // 4 - Single Node
                        load_nodes = new List<int>();
                        load_values = new List<double>();

                        load_nodes.Add(node_start);
                        load_values.Add(ld_ampl);
                        break;
                    }

            }


            // Add the load to the model
            fe_data.stringintension_data.add_load(comboBox_pulse_option.SelectedIndex,
                ld_starttime, ld_endtime, load_nodes, load_values);

            fe_data.update_load();

            // Save the defaults
            Properties.Settings.Default.Sett_load_ampl = ld_ampl;
            Properties.Settings.Default.Sett_load_starttime = ld_starttime;
            Properties.Settings.Default.Sett_load_endtime = ld_endtime;

            Properties.Settings.Default.Sett_load_option = comboBox_pulse_option.SelectedIndex;
            Properties.Settings.Default.Sett_load_interpol = comboBox_inerpolation.SelectedIndex;

            Properties.Settings.Default.Save();

            // Update the datagrid view
            update_dataGridView();

            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }

        }



        private void button_delete_Click(object sender, EventArgs e)
        {
            if (dataGridView_load.SelectedRows.Count == 0)
                return;

            DataGridViewRow row = dataGridView_load.SelectedRows[0];

            int load_id = Convert.ToInt32(row.Cells[0].Value);

            // Remove from the model
            fe_data.stringintension_data.delete_load(load_id);
            fe_data.update_load();

            // Update the datagrid view
            update_dataGridView();

            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }


        }



        private void TriangularInterpolationValues(
                int node_start,
                int node_end,
                double amplitude,
                out List<int> inlcond_nodes,
                out List<double> inlcond_values)
        {

            inlcond_nodes = new List<int>();
            inlcond_values = new List<double>();

            // Find the mid node
            int mid_node = node_start + (node_end - node_start) / 2;
            double halfWidth = Math.Max(mid_node - node_start, node_end - mid_node);


            for (int i = node_start; i <= node_end; i++)
            {
                double dist = Math.Abs(i - mid_node);
                double t = 1.0 - (dist / halfWidth);  // t E [0,1]


                t = Math.Max(0.0, t);                 // safety clamp
                double value = amplitude * t;

                inlcond_nodes.Add(i);
                inlcond_values.Add(value);

            }

        }


        private void CubicBezierInterpolationValues(
                int node_start,
                int node_end,
                double amplitude,
                out List<int> inlcond_nodes,
                out List<double> inlcond_values)
        {

            inlcond_nodes = new List<int>();
            inlcond_values = new List<double>();

            int mid_node = node_start + (node_end - node_start) / 2;

            double spreadLength = node_end - node_start;

            // -------------------------
            // Positive slope (start → mid)
            // -------------------------
            Vector2 p0 = new Vector2(0f, 0f);
            Vector2 p1 = new Vector2((float)(spreadLength * 0.25), 0f);
            Vector2 p2 = new Vector2((float)(spreadLength * 0.25), (float)amplitude);
            Vector2 p3 = new Vector2((float)(spreadLength * 0.5), (float)amplitude);

            for (int i = node_start; i < mid_node; i++)
            {
                double t = (i - node_start) / (double)(mid_node - node_start);
                Vector2 pt = CubicBezier(p0, p1, p2, p3, t);

                inlcond_nodes.Add(i);
                inlcond_values.Add(pt.Y);
            }

            // -------------------------
            // Negative slope (mid → end)
            // -------------------------
            p0 = new Vector2((float)(spreadLength * 0.5), (float)amplitude);
            p1 = new Vector2((float)(spreadLength * 0.75), (float)amplitude);
            p2 = new Vector2((float)(spreadLength * 0.75), 0f);
            p3 = new Vector2((float)spreadLength, 0f);

            for (int i = mid_node; i <= node_end; i++)
            {
                double t = (i - mid_node) / (double)(node_end - mid_node);
                Vector2 pt = CubicBezier(p0, p1, p2, p3, t);

                inlcond_nodes.Add(i);
                inlcond_values.Add(pt.Y);
            }

        }


        private static Vector2 CubicBezier(Vector2 p0, Vector2 p1,
                                    Vector2 p2, Vector2 p3,
                                    double t)
        {
            double u = 1.0 - t;
            double tt = t * t;
            double uu = u * u;

            float x =
                (float)(
                    uu * u * p0.X +
                    3 * uu * t * p1.X +
                    3 * u * tt * p2.X +
                    tt * t * p3.X);

            float y =
                (float)(
                    uu * u * p0.Y +
                    3 * uu * t * p1.Y +
                    3 * u * tt * p2.Y +
                    tt * t * p3.Y);

            return new Vector2(x, y);
        }


        private void SineInterpolationValues(
                int node_start,
                int node_end,
                double amplitude,
                out List<int> inlcond_nodes,
                out List<double> inlcond_values)
        {

            inlcond_nodes = new List<int>();
            inlcond_values = new List<double>();

            int span = node_end - node_start;

            for (int i = node_start; i <= node_end; i++)
            {
                double t = (i - node_start) / (double)span;   // t E [0,1]
                double y = amplitude * Math.Sin(Math.PI * t);

                inlcond_nodes.Add(i);
                inlcond_values.Add(y);
            }

        }



        private void RectangularInterpolationValues(
            int node_start,
            int node_end,
            double amplitude,
            out List<int> inlcond_nodes,
            out List<double> inlcond_values)
        {

            inlcond_nodes = new List<int>();
            inlcond_values = new List<double>();


            for (int i = node_start; i <= node_end; i++)
            {
                inlcond_nodes.Add(i);
                inlcond_values.Add(amplitude);
            }

        }





    }
}
