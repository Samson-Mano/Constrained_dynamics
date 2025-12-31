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
        public elementmass_store fe_mass;
        //public elementspring_store fe_spring;
        //public elementfixedend_store fe_fixedend;

        //// Drawing vectors
        //public vector_store velo_vector;
        //public vector_store accl_vector;

        //// Drawing graphs
        //public graph_store disp_graph;
        //public graph_store velo_graph;
        //public graph_store accl_graph;

        // Drawing labels
        public text_store time_label;
        public text_store disp_label;
        public text_store velo_label;
        public text_store accl_label;

        //// Phase portrait
        //public phaseportrait_store phase_portrait;

        //// Portrait of delayed potential vectors
        //const int num_x_grid = 100; // even number only 2,4,6,8,10
        //const int num_potential_vectors = num_x_grid * num_x_grid; // square grid 1, 4, 9, 16, 25, 36 ...
        //const float potential_vector_spacing = 10.0f;

        //public vector_list_store potential_vectors = new vector_list_store();

        //// Potential vector around unit circle
        //const int num_circle_vectors = 360;
        //const float ptmass_circle_radius = 100.0f; // Circle radius
        //private float x_shift = 0.0f;
        //private float y_shift = 0.0f;

        //public vector_list_store potential_vectors_circle = new vector_list_store();

        //public vector_store resultant_potential_vector;
        //public graph_store resultant_potential_graph;

        //// Drawing object for shadow trail
        //public shadow_trail_store shadow_trail;


        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private double accumulatedTime = 0.0;
        private int time_step = 0;

        // Drawing bound data
        public Vector3 min_bounds = new Vector3(-1);
        public Vector3 max_bounds = new Vector3(1);
        public Vector3 geom_bounds = new Vector3(2);

        //// Fe Results store
        // public feresults_data_store feresults = new feresults_data_store();

        private bool isModelSet = false;

        const double displextent = 100.0; // Extent of displacement for scaling
        const double veloextent = 100.0; // Extent of velocity for scaling
        const double acclextent = 100.0; // Extent of acceleration for scaling


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

            // To control the drawing graphics
            // graphic_events_control = new drawing_events(this);

            // (Re)Initialize the data
            fe_mass = new elementmass_store(new Vector2(0.0f, 0.0f));
            //fe_spring = new elementspring_store(new Vector2(0.0f, -500.0f), new Vector2(0.0f, 0.0f));
            //fe_fixedend = new elementfixedend_store(new Vector2(0.0f, -500.0f), 90.0f);

            //// Set the Animation drawing objects
            //// Set the vectors
            //velo_vector = new vector_store(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), -9);
            //accl_vector = new vector_store(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), -10);

            //// Set the graphs
            //disp_graph = new graph_store(-8);
            //velo_graph = new graph_store(-9);
            //accl_graph = new graph_store(-10);

            // Initialize the labels 
            time_label = new text_store("Time = 0.0000000 s", new Vector2(0.0f, -450.0f), -3); // Number of character  = 18
            disp_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -8); // Number of character  = 12
            velo_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -9); // Number of character  = 12
            accl_label = new text_store("0.0000000000", new Vector2(0.0f, 0.0f), -10); // Number of character  = 12

            //// Initialize the phase portrait
            //phase_portrait = new phaseportrait_store();

            //// Initialize the potential vectors
            //for (int i = 0; i < num_x_grid; i++)
            //{
            //    for (int j = 0; j < num_x_grid; j++)
            //    {
            //        int vec_id = i * num_x_grid + j;
            //        float x_loc = (((float)i + 0.5f) - (float)(num_x_grid) / 2.0f) * potential_vector_spacing;
            //        float y_loc = (((float)j + 0.5f) - (float)(num_x_grid) / 2.0f) * potential_vector_spacing;

            //        potential_vectors.add_vector(vec_id, new Vector2(x_loc, y_loc), new Vector2(potential_vector_spacing + x_loc, y_loc), 0.0);

            //    }
            //}

            //// Set the vectors visualization
            //potential_vectors.set_vector_visualization();

            //// Initialize the potential vectors around circle
            //for (int i = 0; i < num_circle_vectors; i++)
            //{
            //    double angle = (2.0 * Math.PI * i) / (double)num_circle_vectors;
            //    float x_loc = (float)(ptmass_circle_radius * Math.Cos(angle));
            //    float y_loc = (float)(ptmass_circle_radius * Math.Sin(angle));
            //    potential_vectors_circle.add_vector(i, new Vector2(x_loc, y_loc),
            //        new Vector2(x_loc + (float)(5.0 * Math.Cos(angle)), y_loc + (float)(5.0 * Math.Sin(angle))), 0.0);
            //}

            //// Set the vectors visualization
            //potential_vectors_circle.set_vector_visualization();

            //resultant_potential_vector = new vector_store(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), -4);
            //resultant_potential_graph = new graph_store(-4);

            //// Set the shadow trail
            //shadow_trail = new shadow_trail_store();
            //shadow_trail.set_trail_visualization();


            isModelSet = true;

            update_openTK_uniforms(true, true, true);

        }




        public void paint_model()
        {
            if (!isModelSet)
                return;


            // Paint the spring mass system
            fe_mass.paint_pointmass();
            //fe_fixedend.paint_fixedend();
            //fe_spring.paint_spring();

            //if (feresults.isResultsStored && gvariables_static.animate_stop == false)
            //{
            //    gvariables_static.LineWidth = 3.0f;

            //    // Paint the vectors
            //    // Paint the velocity vector
            //    if (gvariables_static.is_show_velocity_vector == true)
            //        velo_vector.paint_vector();

            //    // Paint the acceleration vector
            //    if (gvariables_static.is_show_acceleration_vector == true)
            //        accl_vector.paint_vector();

            //    // Paint the graphs
            //    // Paint the displacement graph
            //    if (gvariables_static.is_show_displacement_graph == true)
            //        disp_graph.paint_graph();

            //    // Paint the velocity graph
            //    if (gvariables_static.is_show_velocity_graph == true)
            //        velo_graph.paint_graph();

            //    // Paint the acceleration graph
            //    if (gvariables_static.is_show_acceleration_graph == true)
            //        accl_graph.paint_graph();

            //    gvariables_static.LineWidth = 1.0f;

            //    // Paint the labels
            //    // Paint time label
            //    if (gvariables_static.is_show_time_label == true)
            //        time_label.paint_dynamic_text();

            //    // Paint displacement label
            //    if (gvariables_static.is_show_displacement_label == true)
            //        disp_label.paint_dynamic_text();

            //    // Paint velocity label
            //    if (gvariables_static.is_show_velocity_label == true)
            //        velo_label.paint_dynamic_text();

            //    // Paint acceleration label
            //    if (gvariables_static.is_show_acceleration_label == true)
            //        accl_label.paint_dynamic_text();


            //    // Paint the phase portrait
            //    if (gvariables_static.is_show_phaseportrait == true)
            //        phase_portrait.paint_graph();

            //    // Paint the potential vectors
            //    if (gvariables_static.is_show_larmour_field == true)
            //        potential_vectors.paint_vectors();

            //    // Paint the potential vectors around circle
            //    if (gvariables_static.is_show_field_around_circle == true)
            //    {
            //        potential_vectors_circle.paint_vectors();
            //        // resultant_potential_vector.paint_vector();
            //        // resultant_potential_graph.paint_graph();

            //    }


            //    // Paint the shadow trail
            //    if(gvariables_static.is_show_masstrail == true)
            //        shadow_trail.paint_shadow_trail();


            //}

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


        private void update_potential_vectors()
        {
            // Potential vectors update
            const double waveSpeedC = 300.0; // wave speed
            Vector2 refZero = new Vector2(-100000, -100000); // Reference point far away


            // First pass — compute all E-vectors and track min/max magnitude.
            Dictionary<int, (Vector2 eVec, double mag)> eData = new Dictionary<int, (Vector2 eVec, double mag)>();

            double minMag = double.PositiveInfinity;
            double maxMag = double.NegativeInfinity;

            //foreach (var vec in potential_vectors.vectorMap.Values)
            //{

            //    // Delayed index for finite propagation
            //    int delayedIndex = get_delyed_index(vec.tail_pt, time_step, feresults.dt, waveSpeedC);

            //    // Acceleration at delayed time
            //    double accVal = feresults.acceleration[delayedIndex];
            //    Vector2 accAtT = new Vector2(0, (float)accVal);

            //    // W-vector (mass displacement)
            //    Vector2 locNow = new Vector2(0, (float)feresults.displacement[time_step]);
            //    Vector2 wVec = locNow - refZero;

            //    // Compute the E-field using your Larmour function
            //    Vector2 eVec = gvariables_static.larmour_field(
            //        vec.tail_pt,      // grid point
            //        accAtT,           // acceleration
            //        waveSpeedC,       // c
            //        wVec,             // w vector
            //        refZero           // reference point
            //    );

            //    double magnitude = eVec.Length;

            //    // Store
            //    eData[vec.vector_id] = (eVec, magnitude);

            //    // Track min and max
            //    if (magnitude > maxMag) maxMag = magnitude;
            //    if (magnitude < minMag) minMag = magnitude;
            //}

            //// Second pass — normalize magnitudes and update vectors
            //foreach (var vec in potential_vectors.vectorMap.Values)
            //{
            //    var (eVec, magnitude) = eData[vec.vector_id];

            //    // Avoid div-by-zero if all magnitudes identical
            //    double normalizedMag = (maxMag != minMag)
            //        ? gvariables_static.GetRemap(maxMag, minMag, 1.0, 0.0, magnitude)
            //        : 0.0;

            //    // Unit direction
            //    Vector2 direction = eVec != Vector2.Zero
            //        ? Vector2.Normalize(eVec)
            //        : Vector2.Zero;

            //    // Tail + arrow
            //    Vector2 arrowPt = vec.tail_pt + direction * potential_vector_spacing; //* (float)normalizedMag

            //    potential_vectors.update_vector(
            //        vec.vector_id,
            //        vec.tail_pt,
            //        arrowPt,
            //        normalizedMag
            //    );
            //}

        }


        private void update_potential_vectors_circle(Vector2 new_mass_loc)
        {
            // Potential vectors update
            const double waveSpeedC = 300.0; // wave speed
            Vector2 refZero = new Vector2(-100000, -100000); // Reference point far away


            // First pass — compute all E-vectors and track min/max magnitude.
            Dictionary<int, (Vector2 eVec, double mag)> eData = new Dictionary<int, (Vector2 eVec, double mag)>();

            double minMag = double.PositiveInfinity;
            double maxMag = double.NegativeInfinity;

            //// Initialize the potential vectors around circle
            //for (int i = 0; i < num_circle_vectors; i++)
            //{
            //    double angle = (2.0 * Math.PI * i) / (double)num_circle_vectors;
            //    float x_loc = new_mass_loc.X + x_shift + (float)(ptmass_circle_radius * Math.Cos(angle));
            //    float y_loc = new_mass_loc.Y + y_shift + (float)(ptmass_circle_radius * Math.Sin(angle));

            //    Vector2 new_tail_pt = new Vector2(x_loc, y_loc);

            //    // Delayed index for finite propagation
            //    int delayedIndex = get_delyed_index(new_tail_pt, time_step, feresults.dt, waveSpeedC);

            //    // Acceleration at delayed time
            //    double accVal = feresults.acceleration[delayedIndex];
            //    Vector2 accAtT = new Vector2(0, (float)accVal);

            //    // W-vector (mass displacement)
            //    Vector2 locNow = new Vector2(0, (float)feresults.displacement[time_step]);
            //    Vector2 wVec = locNow - refZero;

            //    // Compute the E-field using your Larmour function
            //    Vector2 eVec = gvariables_static.larmour_field(
            //        new_tail_pt,      // grid point
            //        accAtT,           // acceleration
            //        waveSpeedC,       // c
            //        wVec,             // w vector
            //        refZero           // reference point
            //    );

            //    double magnitude = eVec.Length;

            //    // Store
            //    eData[i] = (eVec, magnitude);

            //    // Track min and max
            //    if (magnitude > maxMag) maxMag = magnitude;
            //    if (magnitude < minMag) minMag = magnitude;

            //}

            //Vector2 resultant = Vector2.Zero;

            //// Second pass — normalize magnitudes and update vectors
            //for (int i = 0; i < num_circle_vectors; i++)
            //{
            //    double angle = (2.0 * Math.PI * i) / (double)num_circle_vectors;
            //    float x_loc = new_mass_loc.X + x_shift + (float)(ptmass_circle_radius * Math.Cos(angle));
            //    float y_loc = new_mass_loc.Y + y_shift + (float)(ptmass_circle_radius * Math.Sin(angle));

            //    Vector2 new_tail_pt = new Vector2(x_loc, y_loc);

            //    var (eVec, magnitude) = eData[i];

            //    // Avoid div-by-zero if all magnitudes identical
            //    double normalizedMag = (maxMag != minMag)
            //        ? gvariables_static.GetRemap(maxMag, minMag, 1.0, 0.0, magnitude)
            //        : 0.0;

            //    // Unit direction
            //    Vector2 direction = eVec != Vector2.Zero
            //        ? Vector2.Normalize(eVec)
            //        : Vector2.Zero;

            //    // RESULTANT contribution
            //    Vector2 v = direction * (float)normalizedMag;
            //    resultant += v;


            //    // Tail + arrow
            //    Vector2 arrowPt = new_tail_pt + 10.0f * direction * (float)normalizedMag * potential_vector_spacing; //* (float)normalizedMag

            //    potential_vectors_circle.update_vector(
            //        i,
            //        new_tail_pt,
            //        arrowPt,
            //        normalizedMag
            //    );
            //}

            //resultant_potential_vector.update_vector(new_mass_loc, new_mass_loc + resultant);
            //resultant_potential_graph.update_graph(resultant.Length);

        }


        private int get_delyed_index(Vector2 grid_node_pt, int step_i, double time_interval, double wave_speed_c)
        {
            // Delayed time
            double location_from_origin = grid_node_pt.Length; // Location from length

            double delayed_time = (step_i * time_interval) - (location_from_origin / wave_speed_c);

            if (delayed_time < 0)
            {
                delayed_time = 0;
            }


            // Find the index of acceleration at delayed time
            return (int)(Math.Round(delayed_time / time_interval));

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
            fe_mass.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
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


            //// Update spring openTK uniforms
            //fe_spring.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);


            //// Update the animation objects openTK uniforms
            //// Update vector openTK uniforms
            //velo_vector.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);

            //accl_vector.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);


            //// Update graph openTK uniforms
            //// Update displacement graph openTK uniforms
            //disp_graph.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //     graphic_events_control.projectionMatrix,
            //     graphic_events_control.modelMatrix,
            //     graphic_events_control.viewMatrix,
            //     gvariables_static.geom_transparency);

            //// Update velocity graph openTK uniforms
            //velo_graph.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);

            //// Update acceleration graph openTK uniforms
            //accl_graph.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);


            //// Update the phase portrait openTK uniforms
            //phase_portrait.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
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


            //// update potential vectors openTK uniforms
            //potential_vectors.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control);

            //// update potential vectors around circle openTK uniforms
            //potential_vectors_circle.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control);

            //resultant_potential_vector.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);

            //resultant_potential_graph.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);

            //// Update the shadow trail 
            //shadow_trail.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control);



        }


    }
}
