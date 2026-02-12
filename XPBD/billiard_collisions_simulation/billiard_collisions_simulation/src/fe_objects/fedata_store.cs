// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using billiard_collisions_simulation.Properties;
using billiard_collisions_simulation.src.events_handler;
using billiard_collisions_simulation.src.global_variables;
using billiard_collisions_simulation.src.opentk_control.opentk_bgdraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace billiard_collisions_simulation.src.fe_objects
{

    public class fedata_store
    {
        // public pendulum_data_store pendulum_data;
        public billiardball_data_store billiardballs;

        public elementfixedend_store fe_fixedend;


        // Drawing labels
        public text_store time_label;
        public text_store disp_label;
        public text_store velo_label;
        public text_store accl_label;


        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        // private double accumulatedTime = 0.0;
        // private int time_step = 0;

        // Drawing bound data
        public Vector3 min_bounds = new Vector3(-1);
        public Vector3 max_bounds = new Vector3(1);
        public Vector3 geom_bounds = new Vector3(2);


        private bool isModelSet = false;


        public fedata_store()
        {

            // Set a default geometry bounds
            min_bounds = new Vector3(-1);
            max_bounds = new Vector3(1);
            geom_bounds = new Vector3(2);


            // To control the drawing graphics
            graphic_events_control = new drawing_events(this);

        }


        public void set_model()
        {
            // Create the boundary
            List<Vector3> nodePtsList = new List<Vector3>();

            nodePtsList.Add(new Vector3(-600.0f, -500.0f, 0.0f));
            nodePtsList.Add(new Vector3(-600.0f, 500.0f, 0.0f));
            nodePtsList.Add(new Vector3(600.0f, 500.0f, 0.0f));
            nodePtsList.Add(new Vector3(600.0f, -500.0f, 0.0f));


            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            gvariables_static.geom_size = this.geom_bounds.Length;

            // (Re)Initialize the data
            fe_fixedend = new elementfixedend_store(1200.0f, 800.0f);

            // Settings.Default.Reset();

            // Set the pendulum model
            billiardballs = new billiardball_data_store();


            // Initialize the labels 
            time_label = new text_store("Time = 0.0000000 s", new Vector2(-465.0f, 475.0f), -3); // Number of character  = 18
            disp_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -8); // Number of character  = 12
            velo_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -9); // Number of character  = 12
            accl_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -10); // Number of character  = 12

            isModelSet = true;

            update_openTK_uniforms(true, true, true);

            stop_animation();

            gvariables_static.animate_play = true;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = false;

            start_animation();

        }

        public void set_billiardball_model(int number_of_balls, double min_radius, double max_radius)
        {
            // Update the Billiard ball data
            billiardballs.update_billiardball_datas(number_of_balls, min_radius, max_radius);

            update_openTK_uniforms(true, true, true);

            stop_animation();

            gvariables_static.animate_play = true;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = false; 

            start_animation();
        }


        public void paint_model()
        {
            if (!isModelSet)
                return;


            // Paint the three pendulum system
            fe_fixedend.paint_fixedend();

            // Paint the Billiard balls
            billiardballs.paint_billiardballs();

            // Paint the animation time
            time_label.paint_dynamic_text();

        }


        public void start_animation()
        {
            // Restart the animation stopwatch
            stopwatch.Start();

        }


        public void pause_animation()
        {
            // Pause the animation
            stopwatch.Stop();

        }

        public void stop_animation()
        {
            if (!isModelSet)
                return;

            // Reset the animation stopwatch and time step
            stopwatch.Reset();
            stopwatch.Stop();

            billiardballs.reset_simulation();
          
        }


        public void UpdateAnimationStep()
        {
            if (!isModelSet)
                return;

            // Results are stored, update the mass position
            double elapsedRealTime = stopwatch.Elapsed.TotalSeconds;


            if(gvariables_static.animate_play == true)
            {
                time_label.update_text($"Time = {convert_value_to_label(elapsedRealTime, 9)} s", new Vector2(-465.0f, 475.0f)); 

                billiardballs.simulate(elapsedRealTime);

            }
            
        }


        private string convert_value_to_label(double value, int num_char)
        {
            // Convert to the shortest string first
            string shortStr = value.ToString("G10");

            // If it fits, pad it
            if (shortStr.Length <= num_char)
                return shortStr.PadRight(num_char);

            // Else create a formatted value that fits
            // Determine how many decimals can be shown
            int maxDecimals = Math.Max(0, num_char - (int)value.ToString("F0").Length - 1);

            string format = "F" + maxDecimals;
            string fixedStr = value.ToString(format);

            // If STILL too long, hard truncate (last resort)
            if (fixedStr.Length > num_char)
                fixedStr = fixedStr.Substring(0, num_char);

            return fixedStr.PadRight(num_char);
        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency)
        {
            if (!isModelSet)
                return;



            // Update fixed end openTK uniforms
            fe_fixedend.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            // Update billiard balls openTK uniforms
            billiardballs.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);


            // Update text label openTK uniforms
            time_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

        }


    }
}
