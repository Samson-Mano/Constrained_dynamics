using String_vibration_openTK.src.fe_objects;
using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace String_vibration_openTK.other_windows
{
    public partial class resp_frm : Form
    {
        private fedata_store fe_data;


        private Panel panelPopup;
        private TextBox textPopup;
        private Button buttonPopupOK;
        private Button buttonPopupCancel;



        public resp_frm(ref fedata_store fe_data)
        {
            InitializeComponent();


            this.fe_data = fe_data;

            // === Popup Panel ===
            panelPopup = new Panel();
            panelPopup.Size = new Size(220, 70);
            panelPopup.BackColor = Form.DefaultBackColor;
            panelPopup.BorderStyle = BorderStyle.FixedSingle;
            panelPopup.Visible = false;   // Hidden by default
            panelPopup.BringToFront();
            groupBox1.Controls.Add(panelPopup);

            // === TextBox ===
            textPopup = new TextBox();
            textPopup.Size = new Size(200, 25);
            textPopup.Location = new Point(10, 10);
            panelPopup.Controls.Add(textPopup);

            // === OK Button ===
            buttonPopupOK = new Button();
            buttonPopupOK.Text = "OK";
            buttonPopupOK.Size = new Size(80, 25);
            buttonPopupOK.Location = new Point(20, 40);
            buttonPopupOK.Click += ButtonPopupOK_Click;
            panelPopup.Controls.Add(buttonPopupOK);

            // === Cancel Button ===
            buttonPopupCancel = new Button();
            buttonPopupCancel.Text = "Cancel";
            buttonPopupCancel.Size = new Size(80, 25);
            buttonPopupCancel.Location = new Point(120, 40);
            buttonPopupCancel.Click += ButtonPopupCancel_Click;
            panelPopup.Controls.Add(buttonPopupCancel);


        }


        public void initialize_response_form()
        {
            // Initialie the Pulse Form data

            // Update the scale labels based on current global variable values
            trackBar_deformation_scale.Value = ComputeTrackBarFromScale(gvariables_static.displacement_scale);

            UpdateScale(
              trackBar_deformation_scale,
              label_deformation_scale,
              "Deformation scale",
              v => v = gvariables_static.displacement_scale
          );

            trackBar_velocity_scale.Value = ComputeTrackBarFromScale(gvariables_static.velocity_scale);

            UpdateScale(
             trackBar_velocity_scale,
             label_velocity_scale,
             "Velocity scale",
             v => gvariables_static.velocity_scale = v
         );

            trackBar_acceleration_scale.Value = ComputeTrackBarFromScale(gvariables_static.acceleration_scale);

            UpdateScale(
               trackBar_acceleration_scale,
               label_acceleration_scale,
               "Acceleration scale",
               v => gvariables_static.acceleration_scale = v
           );



            //____________________________________________________________________________________________________

            if (gvariables_static.animate_play)
            {

                // Set the status label Playing
                label_status.Text = "Playing";

            }
            else if (gvariables_static.animate_pause)
            {
                // Set the status label Paused
                label_status.Text = "Paused";
            }
            else
            {
                // Set the status label Stopped
                label_status.Text = "Stopped";

            }

            // Set the global variable
            double value = gvariables_static.resp_animation_speed;

            // Set label
            label_animation_speed.Text = value.ToString(CultureInfo.InvariantCulture);
            label_realtimeanim_speed.Text = $"1 second in real time = {value.ToString(CultureInfo.InvariantCulture)} second in Animation";

            if (this.fe_data.stringintension_data.load_data.Count > 0 ||
                this.fe_data.stringintension_data.inlcond_data.Count > 0)
            {

                // Create the response analysis - initialize the load matrices
                this.fe_data.create_response_analysis_load_matrices();

                // Set the modal form is open
                this.fe_data.isResponseAnalysisPaint = true;
                this.fe_data.update_model_transparency();

                //___________________________________________________________________________________________________
                // Animation control
                this.fe_data.stop_animation();

                gvariables_static.animate_play = true;
                gvariables_static.animate_pause = false;
                gvariables_static.animate_stop = false;

                this.fe_data.start_animation();


                // Call to main form
                if (this.Owner is main_frm mainForm)
                {
                    mainForm.CallFrom_inpt_frms();
                }
            }
            else
            {

                this.fe_data.isResponseAnalysisPaint = false;

                // Disable animation controls
                button_animation_speed.Enabled = false;
                button_play_pause.Enabled = false;
                button_stop.Enabled = false;
                label_status.Text = "No load or initial condition loaded.";
            }


        }






        //__________________________________________________________________________________________________________


        private void UpdateScale(TrackBar bar, Label label, string prefix, Action<double> setValue)
        {
            double value = ComputeScaleFromTrackBar(bar.Value);

            label.Text = $"{prefix} = {value:F1}";
            setValue(value);
        }


        private double ComputeScaleFromTrackBar(int value)
        {
            const int PIVOT_INTEGER = 10;

            if (value <= PIVOT_INTEGER)
            {
                // --- Fine Control (0.7, 0.8, 0.9, 1.0) --- 
                // The step is 0.1. 
                // When I=3, difference is 0. 1.0 + 0 = 1.0 
                // When I=2, difference is -1. 1.0 + (-1 * 0.1) = 0.9

                int difference = value - PIVOT_INTEGER;
                return 1.0 + (difference * 0.1);   // fine step
            }
            else
            {
                // --- Coarse Control (1.0, 2.0, 3.0, ... 8.0) --- 
                // The step is 1.0. 
                // When I=4, difference is 1. 1.0 + (1 * 1.0) = 2.0 
                // When I=10 (Max), difference is 7. 1.0 + (7 * 1.0) = 8.0

                int difference = value - PIVOT_INTEGER;
                return 1.0 + (difference * 1.0);   // coarse step
            }

        }

        private int ComputeTrackBarFromScale(double scale)
        {
            const int PIVOT_INTEGER = 10;

            // Fine range: 0.7 → 1.0   (step 0.1)
            if (scale <= 1.0)
            {
                // scale = 1.0 + (difference * 0.1)
                // difference = (scale - 1.0) / 0.1
                int difference = (int)Math.Round((scale - 1.0) / 0.1);
                return PIVOT_INTEGER + difference;
            }
            else
            {
                // Coarse range: 1.0 → 8.0   (step 1.0)
                // scale = 1.0 + (difference * 1.0)
                // difference = scale - 1.0
                int difference = (int)Math.Round(scale - 1.0);
                return PIVOT_INTEGER + difference;
            }
        }



        private void button_animation_speed_Click(object sender, EventArgs e)
        {
            // Position near the button
            panelPopup.Location = new Point(button_animation_speed.Left, button_animation_speed.Bottom);

            textPopup.Text = "";         // Clear previous input
            panelPopup.Visible = true;   // Show popup
            label_realtimeanim_speed.Visible = false;
            textPopup.Focus();           // Focus for typing

        }


        private void ButtonPopupOK_Click(object sender, EventArgs e)
        {
            string input = textPopup.Text;

            // Test whether the input is a valid number (positive integer or float)
            // Try to parse the input as a floating-point number
            bool isNumeric = double.TryParse(
                input,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value
            );

            // Validate: numeric AND positive
            if (!isNumeric || value <= 0)
            {
                panelPopup.Visible = false;
                label_realtimeanim_speed.Visible = true;

                return; // Do not continue
            }

            // Set the modal animation global variable
            gvariables_static.resp_animation_speed = value;

            // Set label
            label_animation_speed.Text = value.ToString(CultureInfo.InvariantCulture);
            label_realtimeanim_speed.Text = $"1 second in real time = {value.ToString(CultureInfo.InvariantCulture)} second in Animation";

            panelPopup.Visible = false;
            label_realtimeanim_speed.Visible = true;
        }

        private void ButtonPopupCancel_Click(object sender, EventArgs e)
        {
            panelPopup.Visible = false;
            label_realtimeanim_speed.Visible = true;
        }


        private void button_play_pause_Click(object sender, EventArgs e)
        {

            if (gvariables_static.animate_play)
            {
                // Currently playing, so pause
                gvariables_static.animate_play = false;
                gvariables_static.animate_pause = true;

                fe_data.pause_animation();

                // Set the status label
                label_status.Text = "Paused";

            }
            else
            {
                // Currently paused/stopped, so play
                gvariables_static.animate_play = true;
                gvariables_static.animate_pause = false;

                // Set the status label
                label_status.Text = "Playing";

            }

            if (gvariables_static.animate_stop == true)
            {
                // Retart the animation from the beginning
                gvariables_static.animate_stop = false;

                // Restart the animation
                fe_data.start_animation();

            }


        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            // Stop the animation
            gvariables_static.animate_play = false;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = true;

            // Reset the animation to the beginning
            fe_data.stop_animation();

            label_status.Text = "Stopped";

        }

        private void button_close_Click(object sender, EventArgs e)
        {
            // Exit
            this.Close();

        }




        //__________________________________________________________________________________________________________





    }
}
