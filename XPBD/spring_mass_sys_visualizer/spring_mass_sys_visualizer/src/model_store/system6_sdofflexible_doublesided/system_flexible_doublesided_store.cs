// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.model_store.system4_sdofflexible;
using spring_mass_sys_visualizer.src.model_store.system5_twodofflexible;
using spring_mass_sys_visualizer.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace spring_mass_sys_visualizer.src.model_store.system6_sdofflexible_doublesided
{
    public class system_flexible_doublesided_store
    {

        // Geometry data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store velocity_vectors;
        private vector_store acceleration_vectors;

        private sdof_doublesided_flexiblecollisionSolver sdofdoublesided_flexiblecollisionSolver;

        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = -1.0f; // seconds


        const float ptmass_radius = 1.0f; // Radius of the point mass circles
        const float spring_element_wd = 0.5f; // Width of the spring elements
        const float total_width = 100.0f; // Total width of the system

        const float relaxed_spring_length = 10.0f; // Relaxed length of the springs

        public system_flexible_doublesided_store(double total_simulation_time)
        {

            // Initialize the multi dof_store
            this.total_simulation_time = total_simulation_time;

            // Initialize the rectangle data
            rigidboundary = new rectangle_store();

            // Add rigid boundary rectangles to the model
            // Left end of the rigid boundary
            rigidboundary.AddRectangle(0, 2.0f * ptmass_radius, 100.0f, -total_width * 0.5f, 0.0f, 0.0f, true);
            rigidboundary.AddRectangle(1, 2.0f * ptmass_radius, 100.0f, total_width * 0.5f, 0.0f, 0.0f, true);


            default_ptmass_location = new List<float>();

            // Left flexible boundary at -50.0f, right flexible boundary at 50.0f, and point masses in between
            float location = -(total_width * 0.5f) + relaxed_spring_length;
            default_ptmass_location.Add(location);

            // Free floating mass at the center of the system
            location = 0.0f;
            default_ptmass_location.Add(location);

            // Right flexible boundary at 50.0f
            location = (total_width * 0.5f) - relaxed_spring_length;
            default_ptmass_location.Add(location);



            // Initialize the circle (point mass) data
            pointmass = new circle_store();

            // First mass m1 (attached to the flexible boundary)
            pointmass.AddCircle(0, ptmass_radius, default_ptmass_location[0], 0.0f, true); // First mass

            // Second mass m2 (free floating mass)
            pointmass.AddCircle(1, ptmass_radius, default_ptmass_location[1], 0.0f, true); // Second mass

            // Third mass m3 (attached to the flexible boundary)
            pointmass.AddCircle(2, ptmass_radius, default_ptmass_location[2], 0.0f, true); // Third mass

            // Add a reference circle with Radius relaxed spring length to the model
            pointmass.AddCircle(3, relaxed_spring_length, default_ptmass_location[1], 0.0f, false); // Reference circle


            // Initialize the spring data
            springs = new spring_store();
            gvariables_static.spring_element_width = spring_element_wd; // Set the spring element width to 2.0f

            // First spring (attached to the flexible boundary and first mass)
            springs.AddSpring(0, -(total_width * 0.5f) + ptmass_radius, 0.0f, default_ptmass_location[0], 0.0f); // First spring

            // Second spring (free floating mass - left free spring)
            springs.AddSpring(1, default_ptmass_location[1] - relaxed_spring_length, 0.0f, default_ptmass_location[1], 0.0f); // Second spring

            // Third spring (free floating mass - right free spring)
            springs.AddSpring(2, default_ptmass_location[1], 0.0f, default_ptmass_location[1] + relaxed_spring_length, 0.0f); // Third spring

            // Fourth spring (attached to the flexible boundary and third mass)
            springs.AddSpring(3, default_ptmass_location[2], 0.0f, (total_width * 0.5f) - ptmass_radius, 0.0f); // Fourth spring


            PerformSolve();


            // Initialize the vector data
            velocity_vectors = new vector_store();
            acceleration_vectors = new vector_store();

            // Add a simple vector to the model
            velocity_vectors.AddVector(0, default_ptmass_location[0], 10.0f, 1.0f, 0.0f); // mass attached to left flexible boundary
            velocity_vectors.AddVector(1, default_ptmass_location[1], 10.0f, 1.0f, 0.0f); // Free floating mass
            velocity_vectors.AddVector(2, default_ptmass_location[2], 10.0f, 1.0f, 0.0f); // mass attached to right flexible boundary


            acceleration_vectors.AddVector(0, default_ptmass_location[0], -10.0f, 1.0f, 0.0f); // mass attached to left flexible boundary
            acceleration_vectors.AddVector(1, default_ptmass_location[1], -10.0f, 1.0f, 0.0f); // Free floating mass
            acceleration_vectors.AddVector(2, default_ptmass_location[2], -10.0f, 1.0f, 0.0f); // mass attached to right flexible boundary


            // Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();
            velocity_vectors.SetBufferData();
            acceleration_vectors.SetBufferData();

        }



        private void PerformSolve()
        {

            // mass and stiffness parameters for the left, strike, and right masses and springs
            double leftmass_m1 = 0.002f;
            double strikemass_m2 = 0.002f;
            double rightmass_m3 = 0.002f;
            
            double leftstiffness_k1 = 0.018f;
            double strikestiffness_k2 = 0.018f;
            double rightstiffness_k3 = 0.018f;

            double dampratio_zeta = 0.0; // Damping ratio

      double total_width = 1000.0; // Total width of the system

            double total_simulation_time = this.total_simulation_time; // seconds


            // Initialize the multi DOF flexible collision solver
            sdofdoublesided_flexiblecollisionSolver = new sdof_doublesided_flexiblecollisionSolver(
                leftmass_m1, strikemass_m2, rightmass_m3, leftstiffness_k1, strikestiffness_k2, rightstiffness_k3,
                 dampratio_zeta, total_width);


            double strikemass_initial_velocity = -1000.0; // Initial velocity of the strike mass
      


            // Solve the system for the given initial conditions and total simulation time
            sdofdoublesided_flexiblecollisionSolver.solve_sdof_collision_with_doublesided_flexible_boundary(total_simulation_time, 
                max_time_increment: 0.001,
                strikemass_initial_velocity);


            // Find the maximum displacement for the vector representation
            max_displacement = double.MinValue;
            max_velocity = double.MinValue;
            max_acceleration = double.MinValue;



            int time_points = sdofdoublesided_flexiblecollisionSolver.SimulationResults.TimePoints.Count;

            for (int i = 0; i < time_points; i++)
            {

                (List<double> displacement_at_t, List<double> velocity_at_t, List<double> acceleration_at_t)
                    = sdofdoublesided_flexiblecollisionSolver.SimulationResults.GetStateListAtTimeIndex(i);

                for (int j = 0; j < 3; j++)
                {
                    max_displacement = Math.Max(max_displacement, Math.Abs(displacement_at_t[j]));
                    max_velocity = Math.Max(max_velocity, Math.Abs(velocity_at_t[j]));
                    max_acceleration = Math.Max(max_acceleration, Math.Abs(acceleration_at_t[j]));
                }
            }


        }




        public void paint_sdof_doublesided_flexibleboundary(ref Shader modelShader)
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



        public void update_sdof_doublesided_flexibleboundary_collision(double elapsedRealTime)
        {
            float scale_value = 40.0f; // Scale for visualization   


            (List<double> Displacement, List<double> Velocity, List<double> Acceleration, double contact_force)
                = sdofdoublesided_flexiblecollisionSolver.getResult_at_timet(elapsedRealTime);


            int numDOF = 3; // Total number of degrees of freedom (DOF) for the system

            List<float> mapped_displacement_list = new List<float>();

            // Update the point mass locations based on the Displacement values
            for (int i = 0; i < numDOF; i++)
            {
                float location = default_ptmass_location[i];
                float displacement_scaled = ((float)Displacement[i] / Math.Abs((float)max_displacement)) * scale_value;

                float mapped_displacement = location + displacement_scaled;
                mapped_displacement_list.Add(mapped_displacement);


                pointmass.updateCirclePosition(i, mapped_displacement, 0.0f); // Point mass circles
            }

            pointmass.UpdateVertexBuffers();


            ////_______________________________________________________________________________________________________________________________
            //// Update the spring locations based on the Displacement values
            //for (int i = 0; i < fixedendDOF; i++)
            //{
            //    if (i == 0)
            //    {
            //        springs.updateSpringPosition(i, -50.0f + ptmass_radius, 0.0f, mapped_displacement_list[i], 0.0f); // First spring
            //        continue; // Skip the first spring as it is attached to the fixed boundary
            //    }

            //    springs.updateSpringPosition(i, mapped_displacement_list[i - 1] + ptmass_radius, 0.0f,
            //        mapped_displacement_list[i], 0.0f); // Subsequent spring

            //}

            //for (int i = 0; i < freeendDOF; i++)
            //{
            //    int offset = fixedendDOF + i;

            //    if (i == 0)
            //    {
            //        // First spring of free flight mass segment (either in contact or not in contact)
            //        if (contact_force > 0.0f)
            //        {
            //            // No contact
            //            float undeformedspringlength = default_ptmass_location[1] - default_ptmass_location[0];

            //            springs.updateSpringPosition(offset, mapped_displacement_list[offset] - undeformedspringlength + ptmass_radius, 0.0f,
            //                mapped_displacement_list[offset], 0.0f); // Subsequent spring
            //        }
            //        else
            //        {
            //            // Contact with the last mass of fixed end segment
            //            springs.updateSpringPosition(offset, mapped_displacement_list[offset - 1] + ptmass_radius, 0.0f,
            //                mapped_displacement_list[offset], 0.0f); // Subsequent spring
            //        }

            //        continue;
            //    }


            //    springs.updateSpringPosition(offset, mapped_displacement_list[offset - 1] + ptmass_radius, 0.0f,
            //        mapped_displacement_list[offset], 0.0f); // Subsequent spring
            //}

            //springs.UpdateVertexBuffers();

            //_______________________________________________________________________________________________________________________________

            float vector_scale_value = 20.0f; // Scale for visualization   

            for (int i = 0; i < numDOF; i++)
            {

                float velocity_scaled = ((float)Velocity[i] / Math.Abs((float)max_velocity)) * vector_scale_value;
                float acceleration_scaled = ((float)Acceleration[i] / Math.Abs((float)max_acceleration)) * vector_scale_value;

                velocity_vectors.updateVectorPosition(i, mapped_displacement_list[i], 10.0f, velocity_scaled, 0.0f); // Velocity vector for mass M_i
                acceleration_vectors.updateVectorPosition(i, mapped_displacement_list[i], -10.0f, acceleration_scaled, 0.0f); // Acceleration vector for mass M_i

            }

            velocity_vectors.UpdateVertexBuffers();
            acceleration_vectors.UpdateVertexBuffers();





        }







    }
}
