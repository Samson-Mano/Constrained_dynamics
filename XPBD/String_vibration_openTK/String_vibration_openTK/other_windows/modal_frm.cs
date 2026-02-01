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
    public partial class modal_frm : Form
    {

        private fedata_store fe_data;

        private const int max_number_of_modes = 100;


        private Panel panelPopup;
        private TextBox textPopup;
        private Button buttonPopupOK;
        private Button buttonPopupCancel;



        Timer rollTimer = new Timer();
        int targetHeight;
        int step = 20;


        public modal_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

            // Check box to control the form roll
            checkBox_rollform.Checked = false;
            rollTimer.Interval = 15;
            rollTimer.Tick += RollTimer_Tick;


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


        public void initialize_modal_form()
        {
            // Initialie the Modal Form data

            // Get the model data
            int number_of_nodes = this.fe_data.stringintension_data.no_of_nodes;
            double stringline_tension = this.fe_data.stringintension_data.string_tension;
            double stringline_density = this.fe_data.stringintension_data.string_density;
            double stringline_length = this.fe_data.stringintension_data.string_length; 

            // Create the c paramter for wave speed
            double c_param = Math.Sqrt(stringline_tension / stringline_density);

            // Set the wave speed
            label_wavespeed.Text = string.Format($"Wave Speed, c = {c_param:F4} unit/s");


            // Clear the combobox existing items
            comboBox_modedata.Items.Clear();


            for (int i = 0; i <number_of_nodes; i++)
            {

                // Mode number
                int mode_number = i + 1;    

                if(i< max_number_of_modes)
                {

                    // Angular frequency
                    double angular_frequency = (mode_number * Math.PI * c_param) / stringline_length;

                    // Natural frequency
                    double natural_frequency = angular_frequency / (2 * Math.PI);

                    // Add to the combobox
                    string modal_data_str = string.Format($"Mode {mode_number}:  Natural Frequency = {natural_frequency:F4} Hz");

                    comboBox_modedata.Items.Add(modal_data_str);

                }

            }

            // Set the selected index to the first mode
            int selected_index = Properties.Settings.Default.Sett_modal_selected_index;

            if(selected_index >=0 && selected_index < comboBox_modedata.Items.Count)
            {
                comboBox_modedata.SelectedIndex = selected_index;
            }
            else
            {
                comboBox_modedata.SelectedIndex = 0;
            }


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

            gvariables_static.modal_animation_speed = Properties.Settings.Default.Sett_modal_animation_speed;

            // Set the global variable
            double value = gvariables_static.modal_animation_speed;

            // Set label
            label_animation_speed.Text = value.ToString(CultureInfo.InvariantCulture);
            label_realtimeanim_speed.Text = $"1 second in real time = {value.ToString(CultureInfo.InvariantCulture)} second in Animation";


            // Set the modal form is open
            this.fe_data.isModalAnalysisPaint = true;
            this.fe_data.update_model_transparency();

            //___________________________________________________________________________________________________
            // Animation control
            this.fe_data.stop_animation();

            gvariables_static.animate_play = true;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = false;

            this.fe_data.start_animation();



            this.fe_data.selected_mode_shape = comboBox_modedata.SelectedIndex;

            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }

        }


        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void modal_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Update the settings
            Properties.Settings.Default.Sett_modal_selected_index = comboBox_modedata.SelectedIndex;
            Properties.Settings.Default.Sett_modal_animation_speed = gvariables_static.modal_animation_speed;

            Properties.Settings.Default.Save();

            // Stop painting modal analysis
            this.fe_data.isModalAnalysisPaint = false;
            this.fe_data.update_model_transparency();

            //___________________________________________________________________________________________________
            // Animation control
            this.fe_data.stop_animation();

            gvariables_static.animate_play = false;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = false;

            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }

        }


        private void comboBox_modedata_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }

            this.fe_data.selected_mode_shape = comboBox_modedata.SelectedIndex;

        }



        //__________________________________________________________________________________________________________

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
            gvariables_static.modal_animation_speed = value;

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

        private void checkBox_rollform_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_rollform.Checked)
            {
                // Collapse
                checkBox_rollform.Text = "▼";
                targetHeight = 85;
            }
            else
            {
                // Expand
                checkBox_rollform.Text = "▲";
                targetHeight = 650;
            }


            rollTimer.Start();

        }



        private void RollTimer_Tick(object sender, EventArgs e)
        {
            if (this.Height < targetHeight) // Height is at 85 and will be extended to Target height of 650
            {
                this.Height += step;
                if (this.Height >= targetHeight)
                {
                    this.Height = targetHeight;
                    rollTimer.Stop();
                }
            }
            else if (this.Height > targetHeight) // Height is at 650 and will be reduced to Target height of 85
            {
                this.Height -= step;
                if (this.Height <= targetHeight)
                {
                    this.Height = targetHeight;
                    rollTimer.Stop();
                }
            }
            else
            {
                rollTimer.Stop();
            }
        }




        //__________________________________________________________________________________________________________

    }
}
