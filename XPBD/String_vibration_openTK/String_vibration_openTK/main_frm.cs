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

        // Forms
        private setmodel_frm setmodel_Form;
        private inlcond_frm inlcond_Form;
        private load_frm load_Form;
        private modal_frm modal_Form;
        private resp_frm resp_Form;

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
            // Paint the background
            Color clr_bg = gvariables_static.glcontrol_background_color;
            GL.ClearColor(((float)clr_bg.R / 255.0f),
                ((float)clr_bg.G / 255.0f),
                ((float)clr_bg.B / 255.0f),
                ((float)clr_bg.A / 255.0f));

            // Update the size of the drawing area
            fedata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height);

            // Create the main font atlas
            gvariables_static.main_font.CreateAtlas();

            // Create the model once the Model panel width and height are set
            fedata.set_model();

            fpsStopwatch.Start();

            // Refresh the controller (doesnt do much.. nothing to draw)
            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_Paint(object sender, PaintEventArgs e)
        {
            // Paint the drawing area (glControl_main)
            // Tell OpenGL to use MyGLControl
            glControl_main_panel.MakeCurrent();

            // GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(0, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Clear the background
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            fedata.paint_model();

            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            glControl_main_panel.SwapBuffers();

            // Update the zoom value
            double zm_val = fedata.graphic_events_control.zoom_val;
            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(zm_val * 100))).ToString() + "%";
            toolStripStatusLabel_IsRefresh.Invalidate();

            // FPS
            frameCount++;

            // Update FPS every second
            if (fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                toolStripStatusFPSLabel.Text = $"FPS: {frameCount}";
                frameCount = 0;

                fpsStopwatch.Restart();

                SetRefreshStatus(true); // Update status bar
            }

        }

        private void glControl_main_panel_SizeChanged(object sender, EventArgs e)
        {
            // Note: SizeChanged can fire before the OpenGL context exists (e.g., during form initialization, Load etc).
            if (glControl_main_panel == null || fedata == null)
                return;

            // Update the size of the drawing area
            fedata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height);

            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(1.0f * 100))).ToString() + "%";

            // Refresh the painting area
            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseEnter(object sender, EventArgs e)
        {
            // set the focus to enable zoom/ pan & zoom to fit
            glControl_main_panel.Focus();

        }

        private void glControl_main_panel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                // Left button down
                fedata.graphic_events_control.handleMouseLeftButtonClick(true, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button down
                fedata.graphic_events_control.handleMouseRightButtonClick(true, e.X, e.Y);

            }

            glControl_main_panel.Invalidate();

        }


        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse wheel
            fedata.graphic_events_control.handleMouseScroll(e.Delta, e.X, e.Y);

            glControl_main_panel.Invalidate();

        }


        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse move 
            fedata.graphic_events_control.handleMouseMove(e.X, e.Y);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                // Left button up
                fedata.graphic_events_control.handleMouseLeftButtonClick(false, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button up
                fedata.graphic_events_control.handleMouseRightButtonClick(false, e.X, e.Y);

            }

            glControl_main_panel.Invalidate();

        }


        private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard Key Down
            fedata.graphic_events_control.handleKeyboardAction(true, e.KeyValue);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard Key Up
            fedata.graphic_events_control.handleKeyboardAction(false, e.KeyValue);

            glControl_main_panel.Invalidate();

            // If zoom-to-fit started, start the timer
            if (fedata.graphic_events_control.isZoomToFitInProgress == true)
            {
                // Start the zoomToFit timer
                if (!zoomToFitTimer.Enabled)
                    zoomToFitTimer.Start();

            }

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

        #region "Menu Items"
        private void newModelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Check if model_Form is null or disposed
            if (setmodel_Form == null || setmodel_Form.IsDisposed)
            {
                setmodel_Form = new setmodel_frm(ref this.fedata);

                // Make it behave like a tool window
                setmodel_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                setmodel_Form.ShowInTaskbar = false;
                setmodel_Form.TopLevel = true;
                setmodel_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - setmodel_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - setmodel_Form.Height) / 2;
                setmodel_Form.StartPosition = FormStartPosition.Manual;
                setmodel_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Show the form
            setmodel_Form.initialize_model_form();

            setmodel_Form.Show(this);
            setmodel_Form.BringToFront();

            glControl_main_panel.Invalidate();


        }


        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();

        }


        private void initialConditionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Check if inlcond_Form is null or disposed
            if (inlcond_Form == null || inlcond_Form.IsDisposed)
            {
                inlcond_Form = new inlcond_frm(ref this.fedata);

                // Make it behave like a tool window
                inlcond_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                inlcond_Form.ShowInTaskbar = false;
                inlcond_Form.TopLevel = true;
                inlcond_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - inlcond_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - inlcond_Form.Height) / 2;
                inlcond_Form.StartPosition = FormStartPosition.Manual;
                inlcond_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Show the form
            inlcond_Form.initialize_initialcondition_form();

            inlcond_Form.Show(this);
            inlcond_Form.BringToFront();

            inlcond_Form.Invalidate();


        }


        private void nodalLoadsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Check if load_Form is null or disposed
            if (load_Form == null || load_Form.IsDisposed)
            {
                load_Form = new load_frm(ref this.fedata);

                // Make it behave like a tool window
                load_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                load_Form.ShowInTaskbar = false;
                load_Form.TopLevel = true;
                load_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - load_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - load_Form.Height) / 2;
                load_Form.StartPosition = FormStartPosition.Manual;
                load_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Show the form
            load_Form.initialize_load_form();

            load_Form.Show(this);
            load_Form.BringToFront();

            load_Form.Invalidate();


        }


        private void modalAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if modal_Form is null or disposed
            if (modal_Form == null || modal_Form.IsDisposed)
            {
                modal_Form = new modal_frm(ref this.fedata);

                // Make it behave like a tool window
                modal_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                modal_Form.ShowInTaskbar = false;
                modal_Form.TopLevel = true;
                modal_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - modal_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - modal_Form.Height) / 2;
                modal_Form.StartPosition = FormStartPosition.Manual;
                modal_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Show the form
            modal_Form.initialize_modal_form();

            modal_Form.Show(this);
            modal_Form.BringToFront();

            modal_Form.Invalidate();


        }


        private void responseAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Check if resp_Form is null or disposed
            if (resp_Form == null || resp_Form.IsDisposed)
            {
                resp_Form = new resp_frm(ref this.fedata);

                // Make it behave like a tool window
                resp_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                resp_Form.ShowInTaskbar = false;
                resp_Form.TopLevel = true;
                resp_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - resp_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - resp_Form.Height) / 2;
                resp_Form.StartPosition = FormStartPosition.Manual;
                resp_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Show the form
             resp_Form.initialize_response_form();

            resp_Form.Show(this);
            resp_Form.BringToFront();

            resp_Form.Invalidate();

        }


        public void CallFrom_inpt_frms()
        {

            // Refresh 
            glControl_main_panel.Invalidate();

        }


        #endregion

    }
}
