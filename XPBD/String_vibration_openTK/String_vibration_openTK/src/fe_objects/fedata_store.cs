// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using String_vibration_openTK.Properties;
using String_vibration_openTK.src.events_handler;
using String_vibration_openTK.src.global_variables;
using String_vibration_openTK.src.opentk_control.opentk_bgdraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace String_vibration_openTK.src.fe_objects
{

    public class fedata_store
    {

        public stringdata_store stringintension_data;

        public stringlinedrawingdata_store elementstringline_data;

        public inlconddrawingdata_store inldispldrawing_data;
        public inlconddrawingdata_store inlvelodrawing_data;


        //// Drawing labels
        //public text_store time_label;
        //public text_store disp_label;
        //public text_store velo_label;
        //public text_store accl_label;


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

            nodePtsList.Add(new Vector3(-1250.0f, -400.0f, 0.0f));
            nodePtsList.Add(new Vector3(-1250.0f, 400.0f, 0.0f));
            nodePtsList.Add(new Vector3(1250.0f, 400.0f, 0.0f));
            nodePtsList.Add(new Vector3(1250.0f, -400.0f, 0.0f));


            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            gvariables_static.geom_size = this.geom_bounds.Length;


            // Load the string in tension data
            stringintension_data = stringdata_store.Load();


            update_string_in_tension_model();
            // update_initial_condition();
            // update_load();

            isModelSet = true;  

            //// Settings.Default.Reset();
            
            //// Set the pendulum model
            //set_triple_pendulum_model(Settings.Default.sett_mass1,
            //    Settings.Default.sett_mass2, 
            //    Settings.Default.sett_mass3, 
            //    Settings.Default.sett_length1, 
            //    Settings.Default.sett_length2, 
            //    Settings.Default.sett_length3, 
            //    Settings.Default.sett_initialangle1, 
            //    Settings.Default.sett_initialangle2, 
            //    Settings.Default.sett_initialangle3);


            //// Initialize the labels 
            //time_label = new text_store("Time = 0.0000000 s", new Vector2(0.0f, 0.0f), -3); // Number of character  = 18
            //disp_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -8); // Number of character  = 12
            //velo_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -9); // Number of character  = 12
            //accl_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -10); // Number of character  = 12

            update_openTK_uniforms(true, true, true);

        }


        public void update_string_in_tension_model()
        {
            isModelSet = false;

            // (Re)Create string in tension visualization data
            Vector2 start_loc = new Vector2(-1000.0f, 0.0f);
            Vector2 end_loc = new Vector2(1000.0f, 0.0f);
            int segment_count = stringintension_data.no_of_nodes - 1;

            elementstringline_data = new stringlinedrawingdata_store(start_loc, end_loc, segment_count);

            update_initial_condition();

            isModelSet = true;

            update_openTK_uniforms(true, true, true);

        }


        public void update_initial_condition()
        {
            // Find the absolute maximum displacement and absolute maximum velocity
            double abs_max_displ_val = 0.0;
            double abs_max_velo_val = 0.0;  

            foreach( initialconditiondata_store inlcond in stringintension_data.inlcond_data)
            {
                if(inlcond.inlcond_type == 0)
                {
                    // Displacement
                    abs_max_displ_val = Math.Max(abs_max_displ_val, inlcond.abs_max_value);

                }

                if(inlcond.inlcond_type == 1)
                {
                    // Velocity
                    abs_max_velo_val = Math.Max(abs_max_velo_val, inlcond.abs_max_value);

                }

            }

            Vector2 start_loc = new Vector2(-1000.0f, 0.0f);
            Vector2 end_loc = new Vector2(1000.0f, 0.0f);


            inldispldrawing_data = new inlconddrawingdata_store(0, start_loc, end_loc, stringintension_data.no_of_nodes - 1);
            inlvelodrawing_data = new inlconddrawingdata_store(1, start_loc, end_loc, stringintension_data.no_of_nodes - 1);

            // Reset the drawing data
            foreach (initialconditiondata_store inlcond in stringintension_data.inlcond_data)
            {
                if (inlcond.inlcond_type == 0)
                {
                    // Displacement
                    inldispldrawing_data.add_initialcondition(inlcond.inlcond_id, 
                        inlcond.inlcond_nodes, inlcond.inlcond_values, abs_max_displ_val);

                }

                if (inlcond.inlcond_type == 1)
                {
                    // Velocity
                   inlvelodrawing_data.add_initialcondition(inlcond.inlcond_id,
                        inlcond.inlcond_nodes, inlcond.inlcond_values, abs_max_velo_val);

                }

            }

            update_openTK_uniforms(true, true, true);

        }


        public void update_load()
        {
            // Find the absolute maximum load
            double abs_max_load_val = 0.0;

            foreach ( loaddata_store ld in stringintension_data.load_data)
            {
                // Load value
                abs_max_load_val = Math.Max(abs_max_load_val, ld.abs_max_value);

            }




        }

        //public void set_triple_pendulum_model(double mass1, double mass2, double mass3,
        //    double length1, double length2, double length3,
        //    double initial_angle1, double initial_angle2, double initial_angle3)
        //{
        //    // Set the pendulum data
        //    List<double> masses = new List<double>() { mass1, mass2, mass3 };
        //    List<double> lengths = new List<double>() { length1, length2, length3 };
        //    List<double> initial_angles_deg = new List<double>() { initial_angle1, initial_angle2, initial_angle3 };


        //    pendulum_data = new pendulum_data_store(masses, lengths, initial_angles_deg);

        //    update_openTK_uniforms(true, true, true);

        //    stop_animation();

        //    gvariables_static.animate_play = true;
        //    gvariables_static.animate_pause = false;
        //    gvariables_static.animate_stop = false; 

        //    start_animation();
        //}


        public void paint_model()
        {
            if (!isModelSet) return;

            // Paint the string in tension
            elementstringline_data.paint_elementstringline();
            
            inldispldrawing_data.paint_inlconddrawing();
            inlvelodrawing_data.paint_inlconddrawing();

            // Paint the animation time
            // time_label.paint_dynamic_text();

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

            // Reset the animation stopwatch and time step
            stopwatch.Reset();
            stopwatch.Stop();

            // pendulum_data.reset_simulation();
          
        }


        public void UpdateAnimationStep()
        {

            // Results are stored, update the mass position
            double elapsedRealTime = stopwatch.Elapsed.TotalSeconds;


            if(gvariables_static.animate_play == true)
            {
                // time_label.update_text($"Time = {convert_value_to_label(elapsedRealTime, 9)} s", new Vector2(0.0f, 65.0f)); 

                // pendulum_data.simulate(elapsedRealTime);

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
            if(!isModelSet) return;

            // Update the string in tension openTK uniforms
            elementstringline_data.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            inldispldrawing_data.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            inlvelodrawing_data.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);




            //// Update fixed end openTK uniforms
            //fe_fixedend.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);


            //// Update pendulum openTK uniforms
            //pendulum_data.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control);


            //// Update text label openTK uniforms
            //time_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control);

        }


    }
}
