using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.model_store.system2_store_data;
using spring_mass_sys_visualizer.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;




namespace spring_mass_sys_visualizer.src.model_store.system3_mdof_data
{
    public class system_mdof_store
    {
        // Geometry data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store velocity_vectors;
        private vector_store acceleration_vectors;

        private mdof1d_rigidcollisionSolver mdof_springsolver;

        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = 20.0; // seconds

        int num_DOF = 10; // Number of degrees of freedom

        public system_mdof_store(double total_simulation_time)
        {

            // Initialize the multi dof_store
            this.total_simulation_time = total_simulation_time;


            // Initialize the rectangle data
            rigidboundary = new rectangle_store();

            // Add rigid boundary rectangles to the model
            rigidboundary.AddRectangle(0, 100.0f, 10.0f, 0.0f, -50.0f, 0.0f, true);


            default_ptmass_location = new List<float>();

            for (int i = 1; i < num_DOF + 1; i++)
            {
                float param_t = (float)i / (float)(num_DOF + 1);

                float location = -45.0f * (1.0f - param_t) + 45.0f * param_t;

                default_ptmass_location.Add(location); // Example: 40.0, 50.0, 60.0 for 3 DOF
            }



            // Initialize the circle (point mass) data
            pointmass = new circle_store();


            // Add the reference circle with Radius 45.0f to the model
            pointmass.AddCircle(0, 45.0f, 0.0f, 0.0f, false);

            int ptmass_id = 1;
            float ptmass_radius = 2.0f;


            foreach (float location in default_ptmass_location)
            {
                pointmass.AddCircle(ptmass_id, ptmass_radius, 0.0f, location, true);
                ptmass_id++;
            }




            // Initialize the spring data
            springs = new spring_store();
            gvariables_static.spring_element_width = 1.5f; // Set the spring element width to 2.0f

            // First spring connecting the reference circle to the first point mass
            // Final spring connecting the last point mass to the reference circle
            springs.AddSpring(0, 0.0f, -45.0f, 0.0f, default_ptmass_location[0]);


            for (int i = 1; i < default_ptmass_location.Count; i++)
            {
                float start_x = 0.0f;
                float start_y = default_ptmass_location[i - 1];
                float end_x = 0.0f;
                float end_y = default_ptmass_location[i];
                springs.AddSpring(i, start_x, start_y, end_x, end_y);
            }






            // Example model
            double mass_m = 0.001; // 1 KG
            double stiff_k = 1.5; // Stiffness k1 spring
            double dampratio_zeta = 0.0; // Damping ratio
            double gravity_g = -9806.65 * 0.1; // mm/s^2

            List<double> mass_list = new List<double>();
            List<double> stiff_list = new List<double>();


            for (int i = 0; i < num_DOF; i++)
            {
                mass_list.Add(mass_m);
                stiff_list.Add(stiff_k);
            }


            // Initialize the multi degree of freedom spring solver
            mdof_springsolver = new mdof1d_rigidcollisionSolver(num_DOF, mass_list, stiff_list, dampratio_zeta, gravity_g);


            double inl_displ = 500.0; // Initial displacement in mm
            double inl_vel = -000.0; // Initial velocity in mm/s

            // Set the initial conditions for the multi degree of freedom spring solver
            List<double> inl_dipl_list = new List<double>();
            List<double> inl_vel_list = new List<double>();

            for (int i = 0; i < num_DOF; i++)
            {
                inl_dipl_list.Add(inl_displ);
                inl_vel_list.Add(inl_vel);
            }


            mdof_springsolver.solve_multidof_rigidcollision(total_simulation_time, max_time_increment: 0.01,
                inl_dipl_list, inl_vel_list);



            // Find the maximum displacement for the vector representation
            max_displacement = double.MinValue;
            max_velocity = double.MinValue;
            max_acceleration = double.MinValue;

            int time_points = mdof_springsolver.SimulationResults.TimePoints.Count;

            for (int i = 0; i < time_points; i++)
            {

                (List<double> displacement_at_t, List<double> velocity_at_t, List<double> acceleration_at_t)
                    = mdof_springsolver.SimulationResults.GetStateListAtTimeIndex(i);

                for (int j = 0; j < num_DOF; j++)
                {
                    max_displacement = Math.Max(max_displacement, Math.Abs(displacement_at_t[j]));
                    max_velocity = Math.Max(max_velocity, Math.Abs(velocity_at_t[j]));
                    max_acceleration = Math.Max(max_acceleration, Math.Abs(acceleration_at_t[j]));
                }
            }




            // Initialize the velocity vectors
            velocity_vectors = new vector_store();

            // Initialize the acceleration vectors
            acceleration_vectors = new vector_store();



            // Step 3: Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();

        }

        public void paint_system3(ref Shader modelShader)
        {

            // Implement the painting logic for system2

            Vector4 rectColor = new Vector4(gvariables_static.ColorUtils.get_RectangleColor(),
gvariables_static.geom_transparency * 0.8f);

            Vector4 springColor = new Vector4(gvariables_static.ColorUtils.get_SpringColor(),
gvariables_static.geom_transparency * 0.8f);

            Vector4 circleColor = new Vector4(gvariables_static.ColorUtils.get_CircleColor(),
                gvariables_static.geom_transparency * 0.8f);

            Vector4 velocityVectorColor = new Vector4(gvariables_static.ColorUtils.get_VelocityVectorColor(),
                gvariables_static.geom_transparency * 0.8f);

            Vector4 accelerationVectorColor = new Vector4(gvariables_static.ColorUtils.get_AccelerationVectorColor(),
                gvariables_static.geom_transparency * 0.8f);


            modelShader.SetVector4("vertexColor", rectColor);
            rigidboundary.PaintRectangles();

            modelShader.SetVector4("vertexColor", circleColor);
            pointmass.PaintCircles();

            modelShader.SetVector4("vertexColor", springColor);
            GL.LineWidth(3.0f);
            springs.PaintSprings();


            //modelShader.SetVector4("vertexColor", velocityVectorColor);
            //velocity_vectors.PaintVectors();

            //modelShader.SetVector4("vertexColor", accelerationVectorColor);
            //acceleration_vectors.PaintVectors();

            //GL.LineWidth(1.0f);
        }


        public void update_system3(double elapsedRealTime)
        {
            float scale_value = 80.0f; // Scale for visualization   

            (List<double> Displacement, List<double> Velocity, List<double> Acceleration, double contact_force)
                = mdof_springsolver.getResult_at_timet(elapsedRealTime);


            int ptmass_id = 1;

            foreach (float location in default_ptmass_location)
            {

                double node_mapped_displacement = Displacement[ptmass_id - 1] / Math.Abs(max_displacement); // Scale down for visualization
                float scaled_displacement = (float)(node_mapped_displacement * scale_value);

                pointmass.updateCirclePosition(ptmass_id, 0.0f, location + scaled_displacement);
                ptmass_id++;
            }

            // update the reference circle with Radius 45.0f to the model
            pointmass.updateCirclePosition(0, 0.0f, 0.0f + (float)(Displacement[0] / Math.Abs(max_displacement)) * scale_value);


            pointmass.UpdateVertexBuffers();


            if (contact_force > 0.0f)
            {
                // No contact
                double startnode_mapped_displacement = Displacement[0] / Math.Abs(max_displacement); // Scale down for visualization
                float scaled_startnodedisplacement = (float)(startnode_mapped_displacement * scale_value);

                springs.updateSpringPosition(0, 0.0f,
                             -45.0f + scaled_startnodedisplacement, 0.0f, default_ptmass_location[0] + scaled_startnodedisplacement);

            }
            else
            {
                // Contact with the rigid boundary
                // You can implement any additional logic here if needed
                double startnode_mapped_displacement = Displacement[0] / Math.Abs(max_displacement); // Scale down for visualization
                float scaled_startnodedisplacement = (float)(startnode_mapped_displacement * scale_value);


                springs.updateSpringPosition(0, 0.0f, -45.0f, 0.0f, default_ptmass_location[0] + scaled_startnodedisplacement);

            }



            for (int i = 1; i < default_ptmass_location.Count; i++)
            {
                double startnode_mapped_displacement = Displacement[i - 1] / Math.Abs(max_displacement); // Scale down for visualization
                float scaled_startnodedisplacement = (float)(startnode_mapped_displacement * scale_value);

                float start_x = 0.0f;
                float start_y = default_ptmass_location[i - 1] + scaled_startnodedisplacement;


                double endnode_mapped_displacement = Displacement[i] / Math.Abs(max_displacement); // Scale down for visualization
                float scaled_endnodedisplacement = (float)(endnode_mapped_displacement * scale_value);

                float end_x = 0.0f;
                float end_y = default_ptmass_location[i] + scaled_endnodedisplacement;

                springs.updateSpringPosition(i, start_x, start_y, end_x, end_y);
            }

            springs.UpdateVertexBuffers();





        }


    }
}
