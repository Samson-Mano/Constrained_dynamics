using OpenTK;
using OpenTK.Graphics.ES11;
using SharpFont.Cache;
using String_vibration_openTK.src.fe_objects;
using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace String_vibration_openTK.other_windows
{
    public partial class inlcond_frm : Form
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


        public inlcond_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

            // Set the data Grid View
            dataGridView_inlcond.ReadOnly = true;
            dataGridView_inlcond.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView_inlcond.MultiSelect = false;

            dataGridView_inlcond.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        public void initialize_initialcondition_form()
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
            textBox_amplitude.Text = Properties.Settings.Default.Sett_inlcond_ampl.ToString();
            comboBox_interpolation.SelectedIndex = Properties.Settings.Default.Sett_inlcond_interpol;
            comboBox_inlcond_type.SelectedIndex = Properties.Settings.Default.Sett_inlcond_type;

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


        private void button_add_Click(object sender, EventArgs e)
        {
            // Check whether the inputs are valid
            // -------------------------------
            // Validate inputs
            // -------------------------------
            if (!gvariables_static.TryGetPositiveDouble(textBox_amplitude, "Initial condition amplitude", out double inl_ampl)) return;

            int node_start = trackBar_startnode.Value;
            int node_end = trackBar_endnode.Value;


            // int interpolation_type = comboBox_interpolation.SelectedIndex;

            // 0 - Triangular Interpolation
            // 1 - Cubic bezier Interpolation
            // 2 - Sine Interpolation
            // 3 - Rectangular Interpolation
            // 4 - Single Node

            List<int> inlcond_nodes = new List<int>();
            List<double> inlcond_values = new List<double>();

            InterpolationType interpolationType = (InterpolationType)comboBox_interpolation.SelectedIndex;


            switch (interpolationType)
            {
                case InterpolationType.Triangular:
                    {
                        // 0 - Triangular Interpolation
                        TriangularInterpolationValues(node_start, node_end, inl_ampl, out inlcond_nodes, out inlcond_values);
                        break;
                    }
                case InterpolationType.CubicBezier:
                    {
                        // 1 - Cubic bezier Interpolation
                        CubicBezierInterpolationValues(node_start, node_end, inl_ampl, out inlcond_nodes, out inlcond_values);
                        break;
                    }
                case InterpolationType.Sine:
                    {
                        // 2 - Sine Interpolation
                        SineInterpolationValues(node_start, node_end, inl_ampl, out inlcond_nodes, out inlcond_values);
                        break;
                    }
                case InterpolationType.Rectangular:
                    {
                        // 3 - Rectangular Interpolation
                        RectangularInterpolationValues(node_start, node_end, inl_ampl, out inlcond_nodes, out inlcond_values);
                        break;
                    }
                case InterpolationType.SingleNode:
                    {
                        // 4 - Single Node
                        inlcond_nodes = new List<int>();
                        inlcond_values = new List<double>();

                        inlcond_nodes.Add(node_start);
                        inlcond_values.Add(inl_ampl);
                        break;
                    }

            }


            // Add the initial condition to the model
            fe_data.stringintension_data.add_initial_condition(comboBox_inlcond_type.SelectedIndex,
                comboBox_interpolation.SelectedIndex,
                inlcond_nodes, inlcond_values);

            fe_data.update_initial_condition();

            // Save the defaults
            Properties.Settings.Default.Sett_inlcond_ampl = inl_ampl;
            Properties.Settings.Default.Sett_inlcond_interpol = comboBox_interpolation.SelectedIndex;
            Properties.Settings.Default.Sett_inlcond_type = comboBox_inlcond_type.SelectedIndex;

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
            if (dataGridView_inlcond.SelectedRows.Count == 0)
                return;

            DataGridViewRow row = dataGridView_inlcond.SelectedRows[0];

            int inlcond_id = Convert.ToInt32(row.Cells[0].Value);

            // Remove from the model
            fe_data.stringintension_data.delete_initial_condition(inlcond_id);
            fe_data.update_initial_condition();

            // Update the datagrid view
            update_dataGridView();

            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }
        }


        private void update_dataGridView()
        {
            dataGridView_inlcond.Rows.Clear();

            // Add the following data to dataGridView for each update
            foreach(initialconditiondata_store inlcond in fe_data.stringintension_data.inlcond_data)
            {
                int inlcond_id = inlcond.inlcond_id;
                string inlcond_typeStr = inlcond.inlcond_type == 0 ? "Displacement" : "Velocity";

                InterpolationType interpolationType = (InterpolationType)inlcond.inlcond_interpolation;

                string interpolationTypeStr = interpolationType.ToString(); 

                int start_node = inlcond.inlcond_nodes[0];
                int end_node = inlcond.inlcond_nodes[inlcond.inlcond_nodes.Count - 1];

                string maxValStr = inlcond.abs_max_value.ToString("F4"); ;


                dataGridView_inlcond.Rows.Add(
                               inlcond_id,
                               inlcond_typeStr,
                               interpolationTypeStr,
                               start_node,
                               end_node,
                               maxValStr);

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
