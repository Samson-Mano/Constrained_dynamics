// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using PBD_pendulum_simulation.src.events_handler;
using PBD_pendulum_simulation.src.global_variables;
using PBD_pendulum_simulation.src.opentk_control.opentk_bgdraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PBD_pendulum_simulation.src.fe_objects
{

    public class fedata_store
    {
        public pendulum_data_store pendulum_data;

        
        public elementfixedend_store fe_fixedend;
        public elementmass_store fe_mass1;
        public elementmass_store fe_mass2;
        public elementmass_store fe_mass3;
        public elementlink_store fe_rigidlink1;
        public elementlink_store fe_rigidlink2;
        public elementlink_store fe_rigidlink3;


        //public elementspring_store fe_spring;

        // Drawing labels
        public text_store time_label;
        public text_store disp_label;
        public text_store velo_label;
        public text_store accl_label;


        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private double accumulatedTime = 0.0;
        private int time_step = 0;

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

            nodePtsList.Add(new Vector3(-500.0f, -500.0f, 0.0f));
            nodePtsList.Add(new Vector3(-500.0f, 500.0f, 0.0f));
            nodePtsList.Add(new Vector3(500.0f, 500.0f, 0.0f));
            nodePtsList.Add(new Vector3(500.0f, -500.0f, 0.0f));


            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            gvariables_static.geom_size = this.geom_bounds.Length;

            // (Re)Initialize the data
            fe_fixedend = new elementfixedend_store(new Vector2(0.0f, 0.0f), 270.0f);

            // Set the pendulum model
            double inital_angle1 = (90.0-45.0 / 180.0) * Math.PI;
            double inital_angle2 = (90.0+0.0 / 180.0) * Math.PI;
            double inital_angle3 = (90.0-45.0 / 180.0) * Math.PI;

            set_triple_pendulum_model(10,12,15,100,120,130,inital_angle1, inital_angle2, inital_angle3);

            // Initialize the labels 
            time_label = new text_store("Time = 0.0000000 s", new Vector2(0.0f, -450.0f), -3); // Number of character  = 18
            disp_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -8); // Number of character  = 12
            velo_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -9); // Number of character  = 12
            accl_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -10); // Number of character  = 12

            isModelSet = true;

            update_openTK_uniforms(true, true, true);

        }

        public void set_triple_pendulum_model(double mass1, double mass2, double mass3,
            double length1, double length2, double length3,
            double initial_angle1, double initial_angle2, double initial_angle3)
        {
            // Set the pendulum data
            pendulum_data = new pendulum_data_store(mass1, mass2, mass3,
                length1, length2, length3, initial_angle1, initial_angle2, initial_angle3);


            // -------------------------------
            // Constants
            // -------------------------------
            const float TOTAL_SCREEN_LENGTH = 400f;
            const float MAX_MASS_SIZE = 50.0f;
            const float MIN_MASS_SIZE = 0.01f;

            // -------------------------------
            // Mass scaling
            // -------------------------------
            double maxMass = Math.Max(mass1, Math.Max(mass2, mass3));


            float screen_mass1_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE, mass1);
            float screen_mass2_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE, mass2);
            float screen_mass3_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE, mass3);


            // -------------------------------
            // Length scaling
            // -------------------------------
            double total_length = length1 + length2 + length3;
            
            double length_ratio1 = (length1 / total_length) * TOTAL_SCREEN_LENGTH;
            double length_ratio2 = (length2 / total_length) * TOTAL_SCREEN_LENGTH;
            double length_ratio3 = (length3 / total_length) * TOTAL_SCREEN_LENGTH;

            // -------------------------------
            // Position helper
            // -------------------------------
            Vector2 NextPoint(Vector2 origin, double angle, double length)
            {
                return origin + new Vector2(
                    -(float)(length * Math.Sin(angle)),
                     (float)(length * Math.Cos(angle))
                );
            }

            // -------------------------------
            // Initial positions
            // -------------------------------
            Vector2 p0 = Vector2.Zero;
            Vector2 p1 = NextPoint(p0, initial_angle1, length_ratio1);
            Vector2 p2 = NextPoint(p1, initial_angle2, length_ratio2);
            Vector2 p3 = NextPoint(p2, initial_angle3, length_ratio3);


            // Set the drawing data
            // Masses
            fe_mass1 = new elementmass_store(p1, screen_mass1_size);
            fe_mass2 = new elementmass_store(p2, screen_mass2_size);
            fe_mass3 = new elementmass_store(p3, screen_mass3_size);

            // Rigid link
            fe_rigidlink1 = new elementlink_store(p0, p1);
            fe_rigidlink2 = new elementlink_store(p1, p2);
            fe_rigidlink3 = new elementlink_store(p2, p3);


        }


        public void paint_model()
        {
            if (!isModelSet)
                return;


            // Paint the three pendulum system
            fe_fixedend.paint_fixedend();

            // Paint the masses
            fe_mass1.paint_pointmass();
            fe_mass2.paint_pointmass();
            fe_mass3.paint_pointmass();

            // Paint the rigid link
           gvariables_static.LineWidth = 4.0f;
            fe_rigidlink1.paint_rigidlink();
            fe_rigidlink2.paint_rigidlink();
            fe_rigidlink3.paint_rigidlink();

            gvariables_static.LineWidth = 1.0f;

        }


        public void start_animation()
        {
            // Restart the animation stopwatch
            time_step = 0;
            stopwatch.Restart();

        }

        public void pause_animation()
        {
            // Pause the animation
            stopwatch.Stop();

        }


        public void UpdateAnimationStep()
        {
            //if (!isModelSet || !feresults.isResultsStored || !gvariables_static.animate_play)
            //    return;

            //// Results are stored, update the mass position
            //double elapsedRealTime = stopwatch.Elapsed.TotalSeconds;

            //// How much simulated time should pass this frame?
            //double desiredSimTime = elapsedRealTime * gvariables_static.animation_speed;

            //// How many FE time steps must we advance?
            //int stepsToAdvance = (int)(desiredSimTime / feresults.dt);

            //if (stepsToAdvance <= 0) return;

            //// Advance the time step
            //stopwatch.Restart();
            //time_step = time_step + stepsToAdvance;


            //if (time_step >= feresults.time_step_count)
            //{
            //    time_step = 0;

            //    // Reset the graphs
            //    disp_graph.reset_graph();
            //    velo_graph.reset_graph();
            //    accl_graph.reset_graph();

            //    phase_portrait.reset_graph();

            //    resultant_potential_graph.reset_graph();

            //}


            //// Get the results at time step
            //double disp = gvariables_static.GetRemap(feresults.max_displacement, feresults.min_displacement,
            //    1.0, -1.0, feresults.displacement[time_step]);
            //double velo = gvariables_static.GetRemap(feresults.max_velocity, feresults.min_velocity,
            //    1.0, -1.0, feresults.velocity[time_step]);
            //double accl = gvariables_static.GetRemap(feresults.max_acceleration, feresults.min_acceleration,
            //    1.0, -1.0, feresults.acceleration[time_step]);

            //// Create the screen points
            //float displ_screenpt = (float)(disp * (displextent * gvariables_static.displacement_scale));
            //float velo_screenpt = (float)(velo * (veloextent * gvariables_static.velocity_scale));
            //float accl_screenpt = (float)(accl * (acclextent * gvariables_static.acceleration_scale));

            //// Update the mass position
            //Vector2 new_mass_loc = new Vector2(0.0f, displ_screenpt);
            //fe_mass.update_pointmass(new_mass_loc, 1.0f);

            //// update the spring 
            //fe_spring.update_spring(new_mass_loc, new Vector2(0.0f, -500.0f));

            //// Update the animation vectors
            //velo_vector.update_vector(new_mass_loc, new_mass_loc + new Vector2(0.0f, 2.0f * velo_screenpt));
            //accl_vector.update_vector(new_mass_loc, new_mass_loc + new Vector2(0.0f, 2.0f * accl_screenpt));

            //// Update the graphs
            //disp_graph.update_graph(displ_screenpt);
            //velo_graph.update_graph(velo_screenpt);
            //accl_graph.update_graph(accl_screenpt);

            //// Update the phase portrait
            //float intensity = (float)Math.Abs(velo);
            //phase_portrait.update_graph(2.0f * velo_screenpt, 2.0f * displ_screenpt, intensity);

            //// Update the labels
            //time_label.update_text($"Time = {convert_value_to_label(time_step * feresults.dt, 9)} s", new Vector2(0.0f, -450.0f)); ;
            //disp_label.update_text($"{convert_value_to_label(feresults.displacement[time_step], 12)}", new Vector2(0.0f, displ_screenpt));
            //velo_label.update_text($"{convert_value_to_label(feresults.velocity[time_step], 12)}", new_mass_loc + new Vector2(0.0f, 2.0f * velo_screenpt));
            //accl_label.update_text($"{convert_value_to_label(feresults.acceleration[time_step], 12)}", new_mass_loc + new Vector2(0.0f, 2.0f * accl_screenpt));

            //update_potential_vectors();

            //update_potential_vectors_circle(new_mass_loc);

            //// Update the shadow trails
            //shadow_trail.update_trail_data(new_mass_loc, feresults.velocity[time_step], feresults.acceleration[time_step]);



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


        public void stop_animation()
        {
            if (!isModelSet)
                return;
            // Reset the animation stopwatch and time step
            stopwatch.Reset();
            stopwatch.Stop();

            time_step = 0;

            //// Reset the graphs
            //disp_graph.reset_graph();
            //velo_graph.reset_graph();
            //accl_graph.reset_graph();

            //phase_portrait.reset_graph();

            //// Get the results at time step
            //double disp = feresults.displacement[time_step] / feresults.displacement_range;
            //double velo = feresults.velocity[time_step] / feresults.velocity_range;
            //double accl = feresults.acceleration[time_step] / feresults.acceleration_range;

            //// Create the screen points
            //float displ_screenpt = (float)(disp * (displextent * gvariables_static.displacement_scale));
            //float velo_screenpt = (float)(velo * (veloextent * gvariables_static.velocity_scale));
            //float accl_screenpt = (float)(accl * (acclextent * gvariables_static.acceleration_scale));

            //// Update the mass position
            //Vector2 new_mass_loc = new Vector2(0.0f, displ_screenpt);
            //fe_mass.update_pointmass(new_mass_loc, 1.0f);

            //// update the spring 
            //fe_spring.update_spring(new_mass_loc, new Vector2(0.0f, -500.0f));

            //// Update the animation vectors
            //velo_vector.update_vector(new_mass_loc, new Vector2(0.0f, velo_screenpt));
            //accl_vector.update_vector(new_mass_loc, new Vector2(0.0f, accl_screenpt));

            //// Update the phase portrait
            //phase_portrait.update_graph(velo_screenpt, displ_screenpt, 0.0f);

        }


        public void importBINfile(string bin_filepath)
        {

            if (!isModelSet)
                return;

            //// Import the binary file data to the spring mass system
            //file_events.import_binary_results(bin_filepath, ref feresults);

            //if (feresults.isResultsStored)
            //{
            //    // Results are stored, start the stopwatch
            //    start_animation();
            //}
            //else
            //{
            //    stop_animation();
            //}


        }


        public void importTXTfile(string txt_filepath)
        {
            if (!isModelSet)
                return;

            //// Import the text file data to the spring mass system
            //file_events.import_text_results(txt_filepath, ref feresults);

            //if (feresults.isResultsStored)
            //{
            //    // Results are stored, start the stopwatch
            //    start_animation();
            //}
            //else
            //{
            //    stop_animation();
            //}

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency)
        {
            if (!isModelSet)
                return;

            // Update the openTK uniforms of spring mass objects

            // Update mass openTK uniforms
            fe_mass1.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_mass2.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_mass3.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            // Update fixed end openTK uniforms
            fe_fixedend.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            // Update the rigid link
            fe_rigidlink1.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_rigidlink2.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_rigidlink3.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            //// Update spring openTK uniforms
            //fe_spring.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);


            // Update text label openTK uniforms
            time_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

            disp_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

            velo_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

            accl_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

        }


    }
}
