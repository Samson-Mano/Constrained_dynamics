using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
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





namespace spring_mass_sys_visualizer.src.model_store.system4_sdofflexible
{
    public class system_flexible_store
    {
        // Geometry data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store velocity_vectors;
        private vector_store acceleration_vectors;

        private twodof_flexiblecollisionSolver twodofflexiblecollisionSolver;

        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = -1.0f; // seconds

        public system_flexible_store(double total_simulation_time)
        {

            // Initialize the multi dof_store
            this.total_simulation_time = total_simulation_time;

            // Initialize the rectangle data
            rigidboundary = new rectangle_store();

            // Add rigid boundary rectangles to the model
            rigidboundary.AddRectangle(0, 100.0f, 10.0f, 0.0f, -50.0f, 0.0f, true);


            default_ptmass_location = new List<float>();
            int numDOF = 2; // Number of degrees of freedom (DOF) for the system

            for (int i = 1; i < numDOF + 1; i++)
            {
                float param_t = (float)i / (float)(numDOF + 1);

                float location = -45.0f * (1.0f - param_t) + 45.0f * param_t;

                default_ptmass_location.Add(location); // Example: 40.0, 50.0, 60.0 for 3 DOF
            }




            // Initialize the circle (point mass) data
            pointmass = new circle_store();

            // // Add the reference circle with Radius 45.0f to the model
            // pointmass.AddCircle(0, 45.0f, 0.0f, 0.0f, false); // Reference circle

            // First mass m1 (attached to the flexible boundary)
            pointmass.AddCircle(0, 5.0f, 0.0f, default_ptmass_location[0], true); // First mass

            // Second mass m2 (free floating mass)
            pointmass.AddCircle(1, 5.0f, 0.0f, default_ptmass_location[1], true); // Second mass



            // Initialize the spring data
            springs = new spring_store();
            gvariables_static.spring_element_width = 1.5f; // Set the spring element width to 2.0f

            // First spring (attached to the flexible boundary and first mass)
            springs.AddSpring(0, 0, -45.0f, 0.0f, default_ptmass_location[0]); // First spring

            // Second spring (attached to the first mass and second mass)
            springs.AddSpring(1, 0, default_ptmass_location[0], 0.0f, default_ptmass_location[1]); // Second spring


            PerformSolve();


            // Initialize the vector data
            velocity_vectors = new vector_store();
            acceleration_vectors = new vector_store();

            // Add a simple vector to the model
            velocity_vectors.AddVector(0, 10.0f, 0.0f, 0.0f, 10.0f); // Velocity vector for mass M1
            acceleration_vectors.AddVector(0, 20.0f, 0.0f, 0.0f, 30.0f); // Acceleration vector for mass M1

            velocity_vectors.AddVector(1, 10.0f, 0.0f, 0.0f, 10.0f); // Velocity vector for mass M2
            acceleration_vectors.AddVector(1, 20.0f, 0.0f, 0.0f, 30.0f); // Acceleration vector for mass M2



            // Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();
            velocity_vectors.SetBufferData();
            acceleration_vectors.SetBufferData();

        }


        private void PerformSolve()
        {
            // Example model
            double mass_m1 = 0.02; // 1 KG
            double mass_m2 = 0.02; // 2 KG

            double stiff_k1 = 0.018; // Stiffness k1 spring
            double stiff_k2 = 0.18; // 0.045; // Stiffness k2 spring

            // Intersting case
            // m1 = m2 = 0.02, k1 = 0.018, k2 = 0.18


            double dampratio_zeta = 0.0; // Damping ratio

            double gravity_g = -9806.65 * 0.0; // mm/s^2


            twodofflexiblecollisionSolver = new twodof_flexiblecollisionSolver(mass_m1, mass_m2, stiff_k1, stiff_k2, dampratio_zeta, gravity_g);
            
            double u1_static = (mass_m1 * gravity_g) / stiff_k1;

            double u1_inl = u1_static; // Initial displacement for mass m1
            double u2_inl = 1000.0;

            double v1_inl = 0.0; // Initial velocity for mass m1
            double v2_inl = -400.0;


            twodofflexiblecollisionSolver.solve_sdof_collision_with_flexible_boundary(total_simulation_time,
               max_time_increment: 0.001, u1_inl, u2_inl, v1_inl, v2_inl);

            // Find the maximum displacement for the vector representation
            max_displacement = double.MinValue;
            max_velocity = double.MinValue;
            max_acceleration = double.MinValue;

            int time_points = twodofflexiblecollisionSolver.SimulationResults.TimePoints.Count;

            for (int i = 0; i < time_points; i++)
            {

                (List<double> displacement_at_t, List<double> velocity_at_t, List<double> acceleration_at_t)
                    = twodofflexiblecollisionSolver.SimulationResults.GetStateListAtTimeIndex(i);

                for (int j = 0; j < 2; j++)
                {
                    max_displacement = Math.Max(max_displacement, Math.Abs(displacement_at_t[j]));
                    max_velocity = Math.Max(max_velocity, Math.Abs(velocity_at_t[j]));
                    max_acceleration = Math.Max(max_acceleration, Math.Abs(acceleration_at_t[j]));
                }
            }

            max_displacement = u2_inl; // Use the initial displacement of mass m1 as the maximum displacement for scaling


        }




        public void paint_sdof_flexibleboundary(ref Shader modelShader)
        {

            // Implement the painting logic for sdof_flexibleboundary

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


            modelShader.SetVector4("vertexColor", velocityVectorColor);
            velocity_vectors.PaintVectors();

            modelShader.SetVector4("vertexColor", accelerationVectorColor);
            acceleration_vectors.PaintVectors();

            GL.LineWidth(1.0f);

        }



        public void update_sdof_flexibleboundary_collision(double elapsedRealTime)
        {
            float scale_value = 40.0f; // Scale for visualization   

            (List<double> Displacement, List<double> Velocity, List<double> Acceleration, double contact_force)
                = twodofflexiblecollisionSolver.getResult_at_timet(elapsedRealTime);

            double node1_displ_at_t = Displacement[0];
            double node2_displ_at_t = Displacement[1];


            double node1_mapped_displacement = node1_displ_at_t / Math.Abs(max_displacement); // Scale down for visualization
            double node2_mapped_displacement = node2_displ_at_t / Math.Abs(max_displacement); // Scale down for visualization

            pointmass.updateCirclePosition(0, 0.0f, (float)(default_ptmass_location[0] + node1_mapped_displacement * scale_value));
            pointmass.updateCirclePosition(1, 0.0f, (float)(default_ptmass_location[1] + 5.0f + node2_mapped_displacement * scale_value));

            pointmass.UpdateVertexBuffers();


            springs.updateSpringPosition(0, 0.0f, -45.0f, 0.0f, (float)(default_ptmass_location[0] + node1_mapped_displacement * scale_value));


            if(contact_force > 0)
            {
                // No Contact
                springs.updateSpringPosition(1, 
                    0.0f, (float)( -default_ptmass_location[1] + 5.0f + node2_mapped_displacement * scale_value),
                    0.0f, (float)(default_ptmass_location[1] + 5.0f + node2_mapped_displacement * scale_value));

            }
            else
            {
                // Contact with the flexible boundary
                // Mass m2 is in contact with the flexible boundary, so we need to update the spring position accordingly
                springs.updateSpringPosition(1, 
                    0.0f, (float)(default_ptmass_location[0] + 5.0f + node1_mapped_displacement * scale_value),
                    0.0f, (float)(default_ptmass_location[1] + 5.0f + node2_mapped_displacement * scale_value));

            }

            springs.UpdateVertexBuffers();

            //_______________________________________________________________________________________________________________________________

            float vector_scale_value = 20.0f; // Scale for visualization   

            double node1_velo_at_t = Velocity[0];
            double node2_velo_at_t = Velocity[1];

            double node1_accel_at_t = Acceleration[0];
            double node2_accel_at_t = Acceleration[1];


            // Update the vector position based on velocity and acceleration
            double node1_mapped_velocity = node1_velo_at_t / Math.Abs(max_velocity);
            double node1_mapped_acceleration = node1_accel_at_t / Math.Abs(max_acceleration);

            velocity_vectors.updateVectorPosition(0, 10.0f, (float)(default_ptmass_location[0] + node1_mapped_displacement * scale_value),
                    0.0f, vector_scale_value * (float)node1_mapped_velocity);

            acceleration_vectors.updateVectorPosition(0, 20.0f, (float)(default_ptmass_location[0] + node1_mapped_displacement * scale_value),
                0.0f, vector_scale_value * (float)node1_mapped_acceleration);



            double node2_mapped_velocity = node2_velo_at_t / Math.Abs(max_velocity);
            double node2_mapped_acceleration = node2_accel_at_t / Math.Abs(max_acceleration);

            velocity_vectors.updateVectorPosition(1, 10.0f, (float)(default_ptmass_location[1] + 5.0f + node2_mapped_displacement * scale_value),
                    0.0f, vector_scale_value * (float)node2_mapped_velocity);

            acceleration_vectors.updateVectorPosition(1, 20.0f, (float)(default_ptmass_location[1] + 5.0f + node2_mapped_displacement * scale_value),
                0.0f, vector_scale_value * (float)node2_mapped_acceleration);


            velocity_vectors.UpdateVertexBuffers();
            acceleration_vectors.UpdateVertexBuffers();



        }



    }
}
