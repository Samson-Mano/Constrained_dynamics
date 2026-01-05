// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using String_vibration_openTK.other_windows;
using String_vibration_openTK.src.fe_objects;
// Local resource
using String_vibration_openTK.src.global_variables;


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace String_vibration_openTK
{
    public partial class main_frm : Form
    {

        // Zoom To Fit 
        private Timer zoomToFitTimer;

        // Refreh and FPS Tracking variables
        private Timer refreshStatusResetTimer;
        private Stopwatch fpsStopwatch = new Stopwatch();

        // main spring mass data store
        public fedata_store fedata;

        //// Forms
        //private setmodel_frm model_Form;
        //private animation_frm animation_Form;
        //private option_frm option_Form;

        private int frameCount = 0;



        public main_frm()
        {
            InitializeComponent();

            // Initialize the spring mass system
            // Create the fe_data object
            fedata = new fedata_store();

            // Initialize the timer
            zoomToFitTimer = new Timer();
            zoomToFitTimer.Interval = 10; // ~60 FPS refresh (16 ms)
            zoomToFitTimer.Tick += ZoomToFitTimer_Tick;


            refreshStatusResetTimer = new Timer();
            refreshStatusResetTimer.Interval = 500; // milliseconds before resetting status
            refreshStatusResetTimer.Tick += RefreshStatusResetTimer_Tick;

            // Render timer
            Application.Idle += OnApplicationIdle;

        }

        private void main_frm_Load(object sender, EventArgs e)
        {
            // Initialize the GLControl in the Load event
            // Fill the gcontrol panel
            glControl_main_panel.Dock = DockStyle.Fill;

        }



        #region "glControl Main Panel Events"
        private void glControl_main_panel_Load(object sender, EventArgs e)
        {

        }

        private void glControl_main_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void glControl_main_panel_SizeChanged(object sender, EventArgs e)
        {

        }
        private void glControl_main_panel_MouseEnter(object sender, EventArgs e)
        {

        }

        private void glControl_main_panel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }


        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {


        }

        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void glControl_main_panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }
       private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {

        }


        private void ZoomToFitTimer_Tick(object sender, EventArgs e)
        {
            glControl_ZoomToFitOperation();

        }



        private void glControl_ZoomToFitOperation()
        {
            // Refresh the glControl_main_panel as the zoom to fit operation in progress
            glControl_main_panel.Invalidate();

            if (fedata.graphic_events_control.isZoomToFitInProgress == false)
            {
                // End the zoom to fit operation
                // Stop zoom-to-fit operation once done
                zoomToFitTimer.Stop();

            }

        }


        private void RefreshStatusResetTimer_Tick(object sender, EventArgs e)
        {
            refreshStatusResetTimer.Stop();
            SetRefreshStatus(false);

        }


        // Utility function for status updates
        private void SetRefreshStatus(bool isRefreshing)
        {

            if (isRefreshing)
            {
                toolStripStatusLabel_IsRefresh.Text = "REFRESH";
                toolStripStatusLabel_IsRefresh.ForeColor = Color.Green;
                toolStripStatusLabel_IsRefresh.Invalidate();

                // Start timer to reset status
                refreshStatusResetTimer.Stop(); // restart if already running
                refreshStatusResetTimer.Start();

            }
            else
            {
                toolStripStatusLabel_IsRefresh.Text = "";
                toolStripStatusLabel_IsRefresh.ForeColor = SystemColors.Control;
                toolStripStatusLabel_IsRefresh.Invalidate();

            }

        }
        #endregion



        private bool IsApplicationIdle()
        {
            Message msg;
            return !gvariables_static.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                fedata.UpdateAnimationStep();   // Update animation
                glControl_main_panel.Invalidate(); // Redraw
            }
        }





    }
}
