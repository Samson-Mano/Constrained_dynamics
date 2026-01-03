using PBD_pendulum_simulation.src.fe_objects;
using PBD_pendulum_simulation.src.global_variables;
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

namespace PBD_pendulum_simulation.other_windows
{
    public partial class animation_frm : Form
    {
        private fedata_store fe_data;


        public animation_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }


        public void initialize_animation_form()
        {

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

        }


        private void button_play_pause_Click(object sender, EventArgs e)
        {
            if (gvariables_static.animate_play)
            {
                // Currently playing, so pause
                gvariables_static.animate_play = false;
                gvariables_static.animate_pause = true;

                fe_data.pause_animation();

                // button_play_pause.Text = "Play Animation";

                // Set the status label
                label_status.Text = "Paused";

            }
            else
            {
                // Currently paused/stopped, so play
                gvariables_static.animate_play = true;
                gvariables_static.animate_pause = false;

                fe_data.start_animation();

                // button_play_pause.Text = "Pause Animation";

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
    }
}
