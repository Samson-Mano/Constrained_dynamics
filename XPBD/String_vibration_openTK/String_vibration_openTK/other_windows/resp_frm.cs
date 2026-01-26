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


            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_inpt_frms();
            }


        }






        //__________________________________________________________________________________________________________


        private void button_animation_speed_Click(object sender, EventArgs e)
        {

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

        }

        private void button_stop_Click(object sender, EventArgs e)
        {

        }




        //__________________________________________________________________________________________________________





    }
}
