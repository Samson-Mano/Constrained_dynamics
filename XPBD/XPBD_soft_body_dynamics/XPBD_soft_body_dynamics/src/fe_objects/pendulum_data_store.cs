using OpenTK;
using XPBD_soft_body_dynamics.src.global_variables;
using XPBD_soft_body_dynamics.src.opentk_control.opentk_bgdraw;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XPBD_soft_body_dynamics.src.fe_objects
{

    public class pendulum_values_store
    {
        public double mass_value = 0.0;
        public double length_value = 0.0;
        public double initial_angle_rad = 0.0;

        // Position 
        public double Pos_X = 0.0;
        public double Pos_Y = 0.0;

        // Screen mass and screen length
        public float screen_mass_size = 0.0f;
        public float screen_length = 0.0f;


        // Velocity
        public double Velo_X = 0.0;
        public double Velo_Y = 0.0;

        // Previous position
        public double prevPos_X = 0.0;
        public double prevPos_Y = 0.0;

        public elementmass_store fe_mass;
        public elementlink_store fe_rigidlink;
        public traildata_store traildata;

    }


    public class pendulum_data_store
    {

        // Stores data of the Three pendulum
        public List<pendulum_values_store> p_mass = new List<pendulum_values_store>();


        // -------------------------------
        // Constants
        // -------------------------------
        const float TOTAL_SCREEN_LENGTH = 400f;
        const float MAX_MASS_SIZE = 50.0f;
        const float MIN_MASS_SIZE = 0.01f;

        const double GRAVITY = -9.8065;

        // Simulation constants
        const double FIXED_DT = 1.0 / 120.0; 
        const int CONSTRAINT_ITERATIONS = 8;
        const double VELOCITY_DAMPING = 0.999; // very mild



        double maxMass = 0.0;
        double total_length = 0.0;

        double prev_time = 0.0;
        double accumulator = 0.0;

        public pendulum_data_store(List<double> masses, List<double> lengths, List<double> initial_angles_deg)
        {

            maxMass = 0.0;
            total_length = 0.0;

            // Add three masses
            p_mass = new List<pendulum_values_store>();

            for (int i = 0; i < masses.Count; i++)
            {
                pendulum_values_store temp_pendulum = new pendulum_values_store();

                temp_pendulum.mass_value = masses[i];
                temp_pendulum.length_value = lengths[i];
                temp_pendulum.initial_angle_rad = UserAngleToPendulumRad(initial_angles_deg[i]);

                // Find the maximum mass
                maxMass = Math.Max(maxMass, masses[i]);

                // Find the total length
                total_length = total_length + lengths[i];

                // Add to the list
                p_mass.Add(temp_pendulum);

            }


            // -------------------------------
            // Initial positions
            // -------------------------------
            double p0_x = 0.0;
            double p0_y = 0.0;

            Vector2 p0 = Vector2.Zero;

            for (int i = 0; i < p_mass.Count; i++)
            {
       
                Tuple<double, double> p1_pair = OrientPoint(p0_x, p0_y, p_mass[i].initial_angle_rad, p_mass[i].length_value);

                double p1_x = p1_pair.Item1;
                double p1_y = p1_pair.Item2;

                // Set the Pendulum values
                p_mass[i].Pos_X = p1_x;
                p_mass[i].Pos_Y = p1_y;

                // Velocity
                p_mass[i].Velo_X = 0.0;
                p_mass[i].Velo_Y = 0.0;

                // Previous position
                p_mass[i].prevPos_X = 0.0;
                p_mass[i].prevPos_Y = 0.0;
                
                p0_x = p1_x;
                p0_y = p1_y;


                //____________________________________________________________________________________________________________
                // Set the drawing data

                // -------------------------------
                // Mass scaling
                // -------------------------------
                p_mass[i].screen_mass_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE,
                    p_mass[i].mass_value);

                // -------------------------------
                // Length scaling
                // -------------------------------
                float length_ratio = (float)(p_mass[i].length_value / total_length) * TOTAL_SCREEN_LENGTH;

                // Add screen length
                p_mass[i].screen_length = length_ratio;

                Vector2 dir = new Vector2((float)(p_mass[i].Pos_X - p0_x), (float)(p_mass[i].Pos_Y - p0_y));
                dir = Vector2.Normalize(dir);

                Vector2 p1 = p0 + (dir * p_mass[i].screen_length);   // NextPoint(p0, p_mass[i].initial_angle_rad, length_ratio);

                // Masses
                p_mass[i].fe_mass = new elementmass_store(p1, p_mass[i].screen_mass_size);

                // Initialize the trail
                p_mass[i].traildata = new traildata_store(p1.X, p1.Y);

                // Rigid link
                p_mass[i].fe_rigidlink = new elementlink_store(p0, p1);

                // Reset the first point
                p0 = p1;

            }

            prev_time = 0.0;

        }


        // -------------------------------
        // Position helper
        // -------------------------------
        private double UserAngleToPendulumRad(double userDeg)
        {
            return Math.PI / 2.0 - userDeg * Math.PI / 180.0;
        }

        private Tuple<double, double> OrientPoint(double p0_x, double p0_y, double angle, double length)
        {

            double p1_x = p0_x + (length * Math.Sin(angle));
            double p1_y = p0_y - (length * Math.Cos(angle));

            return new Tuple<double, double>(p1_x, p1_y);

        }



        public void paint_pendulum()
        {

            for (int i = 0; i < p_mass.Count; i++)
            {
                // Paint the mass
                p_mass[i].fe_mass.paint_pointmass();

                // Paint the rigid link
                p_mass[i].fe_rigidlink.paint_rigidlink();

                if(gvariables_static.is_show_masstrail == true)
                {
                    // Paint the trail data
                    p_mass[i].traildata.paint_trail();

                }

            }

        }

        public void reset_simulation()
        {
            // -------------------------------
            // Initial positions
            // -------------------------------
            double p0_x = 0.0;
            double p0_y = 0.0;

            Vector2 p0 = Vector2.Zero;

            for (int i = 0; i < p_mass.Count; i++)
            {

                Tuple<double, double> p1_pair = OrientPoint(p0_x, p0_y, p_mass[i].initial_angle_rad, p_mass[i].length_value);

                double p1_x = p1_pair.Item1;
                double p1_y = p1_pair.Item2;

                // Reset the Pendulum values
                p_mass[i].Pos_X = p1_x;
                p_mass[i].Pos_Y = p1_y;

                // Velocity
                p_mass[i].Velo_X = 0.0;
                p_mass[i].Velo_Y = 0.0;

                // Previous position
                p_mass[i].prevPos_X = 0.0;
                p_mass[i].prevPos_Y = 0.0;

             
                //____________________________________________________________________________________________________________
                // Reset the drawing data

                Vector2 dir = new Vector2((float)(p_mass[i].Pos_X - p0_x), (float)(p_mass[i].Pos_Y - p0_y));
                dir = Vector2.Normalize(dir);

                Vector2 p1 = p0 + (dir * p_mass[i].screen_length);   //NextPoint(p0, p_mass[i].initial_angle_rad, length_ratio);

                // Masses
                p_mass[i].fe_mass.update_pointmass(p1, p_mass[i].screen_mass_size);

                p_mass[i].traildata.reset_trail(p1.X, p1.Y);

                // Rigid link
                p_mass[i].fe_rigidlink.update_rigidlink(p0, p1);

                // Reset the first point
                p0_x = p1_x;
                p0_y = p1_y;

                p0 = p1;

            }

            prev_time = 0.0;
            accumulator = 0.0;

        }



        public void simulate(double time_t)
        {
            double frameTime = time_t - prev_time;
            frameTime = Math.Min(frameTime, 0.05); // avoid spiral of death

            accumulator += frameTime;

            while (accumulator >= FIXED_DT)
            {
                StepPhysics(FIXED_DT);
                accumulator -= FIXED_DT;
            }

            prev_time = time_t;

        }


        private void StepPhysics(double dt)
        {
            // 1. Update the position
            // Integrate the velocities to position
            for (int i = 0; i < p_mass.Count; i++)
            {
                // Calculate y velocity which is due to downward gravity
                p_mass[i].Velo_Y = p_mass[i].Velo_Y + (dt * GRAVITY);

                // Set the previous position
                p_mass[i].prevPos_X = p_mass[i].Pos_X;
                p_mass[i].prevPos_Y = p_mass[i].Pos_Y;

                // Calculate the current position
                p_mass[i].Pos_X = p_mass[i].Pos_X + (p_mass[i].Velo_X * dt);
                p_mass[i].Pos_Y = p_mass[i].Pos_Y + (p_mass[i].Velo_Y * dt);

            }

            // 2. Adjust the position for position error by applying length constraint (Rigid length)
            // Solve constraints through multiple iteration
            double p0_x = 0.0;
            double p0_y = 0.0;

            for (int iter = 0; iter < CONSTRAINT_ITERATIONS; iter++)
            {
                p0_x = 0.0;
                p0_y = 0.0;
                double p0_mass = 0.0;

                for (int i = 0; i < p_mass.Count; i++)
                {
                    // Calculate the lengths
                    double dx = p_mass[i].Pos_X - p0_x;
                    double dy = p_mass[i].Pos_Y - p0_y;

                    double d = Math.Sqrt((dx * dx) + (dy * dy));

                    // Find the mass ratio
                    double w0 = p0_mass > 0.0 ? (1.0 / p0_mass) : 0.0;
                    double w1 = p_mass[i].mass_value > 0.0 ? (1.0 / p_mass[i].mass_value) : 0.0;

                    // Correction value
                    double C = d - p_mass[i].length_value;
                    double corr = C / (w0 + w1);


                    if (i != 0)
                    {
                        p_mass[i - 1].Pos_X = p_mass[i - 1].Pos_X  + ((w0 * corr * dx) / d);
                        p_mass[i - 1].Pos_Y = p_mass[i - 1].Pos_Y  + ((w0 * corr * dy) / d);
                    }

                    p_mass[i].Pos_X = p_mass[i].Pos_X - ((w1 * corr * dx) / d);
                    p_mass[i].Pos_Y = p_mass[i].Pos_Y - ((w1 * corr * dy) / d);

                    p0_x = p_mass[i].Pos_X;
                    p0_y = p_mass[i].Pos_Y;
                    p0_mass = p_mass[i].mass_value;
                }

            }

            // 3. Recompute velocities
            // 3A. Calculate the Kinetic energy before projection
            double KE_before = 0.0;
            for (int i = 0; i < p_mass.Count; i++)
            {
                KE_before += 0.5 * p_mass[i].mass_value *
                    (p_mass[i].Velo_X * p_mass[i].Velo_X +
                     p_mass[i].Velo_Y * p_mass[i].Velo_Y);
            }

            // 3B. Project the new position to calculate the velocity
            for (int i = 0; i < p_mass.Count; i++)
            {
                p_mass[i].Velo_X = (p_mass[i].Pos_X - p_mass[i].prevPos_X) / dt;
                p_mass[i].Velo_Y = (p_mass[i].Pos_Y - p_mass[i].prevPos_Y) / dt;

                // p_mass[i].Velo_X = p_mass[i].Velo_X * VELOCITY_DAMPING;
               //  p_mass[i].Velo_Y = p_mass[i].Velo_Y * VELOCITY_DAMPING;
            }

            // 3C. After constraints + velocity rebuild, rescale
            double KE_after = 0.0;
            for (int i = 0; i < p_mass.Count; i++)
            {
                KE_after += 0.5 * p_mass[i].mass_value *
                    (p_mass[i].Velo_X * p_mass[i].Velo_X +
                     p_mass[i].Velo_Y * p_mass[i].Velo_Y);
            }

            if (KE_after > 1e-8)
            {
                double scale = Math.Sqrt(KE_before / KE_after) * 0.999;
                for (int i = 0; i < p_mass.Count; i++)
                {
                    p_mass[i].Velo_X *= scale;
                    p_mass[i].Velo_Y *= scale;
                }
            }


            // Update the rendering drawing data 
            p0_x = 0.0;
            p0_y = 0.0;
            Vector2 p0 = Vector2.Zero;

            for (int i = 0; i < p_mass.Count; i++)
            {

                Vector2 dir = new Vector2((float)(p_mass[i].Pos_X - p0_x), (float)(p_mass[i].Pos_Y - p0_y));
                dir = Vector2.Normalize(dir);

                Vector2 p1 = p0 + dir * (float)p_mass[i].screen_length;

                // Masses
                p_mass[i].fe_mass.update_pointmass(p1, p_mass[i].screen_mass_size);

                p_mass[i].traildata.update_trail(p1.X, p1.Y);

                // Rigid link
                p_mass[i].fe_rigidlink.update_rigidlink(p0, p1);

                // Reset the first point
                p0_x = p_mass[i].Pos_X;
                p0_y = p_mass[i].Pos_Y;

                p0 = p1;
            }

        }




        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
                                  drawing_events graphic_events_control)
        {


            for (int i = 0; i < p_mass.Count; i++)
            {
                // Update mass openTK uniforms
                p_mass[i].fe_mass.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                    graphic_events_control.projectionMatrix,
                    graphic_events_control.modelMatrix,
                    graphic_events_control.viewMatrix,
                    gvariables_static.geom_transparency);


                p_mass[i].fe_rigidlink.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                    graphic_events_control.projectionMatrix,
                    graphic_events_control.modelMatrix,
                    graphic_events_control.viewMatrix,
                    gvariables_static.geom_transparency);

                p_mass[i].traildata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                    graphic_events_control.projectionMatrix,
                    graphic_events_control.modelMatrix,
                    graphic_events_control.viewMatrix,
                    gvariables_static.geom_transparency);

            }


        }



    }
}
