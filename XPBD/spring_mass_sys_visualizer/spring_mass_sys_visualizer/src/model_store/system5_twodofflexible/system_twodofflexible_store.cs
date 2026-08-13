using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.model_store.system4_sdofflexible;
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




namespace spring_mass_sys_visualizer.src.model_store.system5_twodofflexible
{
    public class system_twodofflexible_store
    {
        // Geometry data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store velocity_vectors;
        private vector_store acceleration_vectors;

        private vector_store group_velocity_vector;
        private vector_store group_acceleration_vector;

        private twodof_flexiblecollisionSolver multidofflexiblecollisionSolver;


        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = -1.0f; // seconds

        const float ptmass_radius = 1.0f; // Radius of the point mass circles
        const float spring_element_wd = 0.5f; // Width of the spring elements

        private int fixedendDOF = 4; // Number of fixed end degrees of freedom (DOF) for the system
        private int freeendDOF = 4; // Number of free end degrees of freedom (DOF) for the system

        private float average_fixedend_location = 0.0f; // Average location of the fixed end masses
        private float average_freeend_location = 0.0f; // Average location of the free end masses

        private List<double> fixedend_mass_data; // Mass of the fixed end segment
        private List<double> freeend_mass_data; // Mass of the free end segment

        private double total_fixedend_mass = 0.0f; // Total mass of the fixed end segment
        private double total_freeend_mass = 0.0f; // Total mass of the free end segment

        public system_twodofflexible_store(double total_simulation_time)
        {

            // Initialize the multi dof_store
            this.total_simulation_time = total_simulation_time;

            // Initialize the rectangle data
            rigidboundary = new rectangle_store();


            // Add rigid boundary rectangles to the model
            rigidboundary.AddRectangle(0, 2.0f * ptmass_radius, 100.0f, -50.0f, 0.0f, 0.0f, true);

            int numDOF = fixedendDOF + freeendDOF; // Total number of degrees of freedom (DOF) for the system
            default_ptmass_location = new List<float>();

            for (int i = 1; i < numDOF + 1; i++)
            {
                float param_t = (float)i / (float)(numDOF + 1);

                float location = -50.0f * (1.0f - param_t) + 50.0f * param_t;

                default_ptmass_location.Add(location); // Example: 40.0, 50.0, 60.0 for 3 DOF
            }


            // Initialize the circle (point mass) data
            pointmass = new circle_store();

            // // Add the reference circle with Radius 45.0f to the model
            // pointmass.AddCircle(0, 45.0f, 0.0f, 0.0f, false); // Reference circle

            for (int i = 0; i < numDOF; i++)
            {
                float location = default_ptmass_location[i];
                pointmass.AddCircle(i, ptmass_radius, location, 0.0f, true); // Point mass circles
            }



            // Initialize the spring data
            springs = new spring_store();
            gvariables_static.spring_element_width = spring_element_wd; // Set the spring element width to 1.5f


            for (int i = 0; i < numDOF; i++)
            {
                if (i == 0)
                {
                    // First spring (attached to the flexible boundary and first mass)
                    springs.AddSpring(i, -50.0f + ptmass_radius, 0.0f, default_ptmass_location[i], 0.0f); // First spring

                    continue; // Skip the first spring as it is attached to the fixed boundary
                }

                // Subsequent spring (attached to the i-1 mass and i mass)
                springs.AddSpring(i, default_ptmass_location[i - 1] + ptmass_radius, 0.0f, default_ptmass_location[i], 0.0f); // Second spring

            }


            PerformSolve();


            // Initialize the vector data
            velocity_vectors = new vector_store();
            acceleration_vectors = new vector_store();

            // Add a simple vector to the model
            for (int i = 0; i < numDOF; i++)
            {
                float location = default_ptmass_location[i];

                velocity_vectors.AddVector(i, location, 10.0f, 1.0f, 0.0f); // Velocity vector for mass M_i
                acceleration_vectors.AddVector(i, location, -10.0f, 1.0f, 0.0f); // Acceleration vector for mass M_i

            }


            // Average location for the group velocity vector
            float average_location = 0.0f;

            for (int i = 0; i < fixedendDOF; i++)
            {
                average_location += default_ptmass_location[i];
            }

            average_location /= fixedendDOF;
            this.average_fixedend_location = average_location;

            group_velocity_vector = new vector_store();
            group_velocity_vector.AddVector(0, average_location, 20.0f, 1.0f, 0.0f); // Group velocity vector

            group_acceleration_vector = new vector_store();
            group_acceleration_vector.AddVector(0, average_location, -20.0f, 1.0f, 0.0f); // Group acceleration vector


            average_location = 0.0f;
            for (int i = 0;i < freeendDOF; i++ )
            {
                average_location += default_ptmass_location[fixedendDOF + i];
            }

            average_location /= freeendDOF;
            this.average_freeend_location = average_location;

            group_velocity_vector.AddVector(1, average_location, 20.0f, 1.0f, 0.0f); // Group velocity vector
            group_acceleration_vector.AddVector(1, average_location, -20.0f, 1.0f, 0.0f); // Group acceleration vector

            // Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();
            velocity_vectors.SetBufferData();
            acceleration_vectors.SetBufferData();
            group_velocity_vector.SetBufferData();
            group_acceleration_vector.SetBufferData();

        }



        private void PerformSolve()
        {

            List<double> fixedend_mass = new List<double> { 0.002, 0.002, 0.002, 0.002 }; // Mass of the fixed end segment
            List<double> fixedend_stiffness = new List<double> { 0.018, 0.018, 0.018, 0.018 }; // Stiffness of the fixed end segment
            List<double> freeend_mass = new List<double> { 0.002, 0.002, 0.002, 0.002 }; // Mass of the free end segment
            List<double> freeend_stiffness = new List<double> { 0.018, 0.018, 0.018, 0.018 }; // Stiffness of the free end segment

            List<double> u_inl = new List<double> {0.0, 0.0, 0.0, 0.0, 1000.0, 1000.0, 1000.0, 1000.0  }; 
            List<double> v_inl = new List<double> {0.0, 0.0,  0.0, 0.0, -400.0, -400.0, -400.0, -400.0 };


            if (fixedendDOF != fixedend_mass.Count || fixedendDOF != fixedend_stiffness.Count ||
                freeendDOF != freeend_mass.Count || freeendDOF != freeend_stiffness.Count)
            {
                throw new ArgumentException("Mismatch between DOF and mass/stiffness list lengths.");
            }

            if((fixedendDOF + freeendDOF) != u_inl.Count || (fixedendDOF + freeendDOF) != v_inl.Count)
            {
                throw new ArgumentException("Mismatch between DOF and initial condition list lengths.");
            }


            this.total_fixedend_mass = 0.0f;
            this.fixedend_mass_data = new List<double>();
            foreach (double mass in fixedend_mass)
            {
                this.total_fixedend_mass += mass;
                this.fixedend_mass_data.Add(mass);
            }

            this.total_freeend_mass = 0.0f;
            this.freeend_mass_data = new List<double>();
            foreach (double mass in freeend_mass)
            {
                this.total_freeend_mass += mass;
                this.freeend_mass_data.Add(mass);
            }


            double dampratio_zeta = 0.0; // Damping ratio
            double const_accla0 = 9806.65 * 0.0; // Constant acceleration (e.g., gravity)

            double total_simulation_time = this.total_simulation_time; // seconds

            // Initialize the multi DOF flexible collision solver
            multidofflexiblecollisionSolver = new twodof_flexiblecollisionSolver(
                fixedend_mass, fixedend_stiffness, freeend_mass, freeend_stiffness,
                 dampratio_zeta, const_accla0);


            // Solve the system for the given initial conditions and total simulation time
            multidofflexiblecollisionSolver.solve_multidof_collision_with_flexible_boundary(total_simulation_time, max_time_increment: 0.001,
                u_inl, v_inl);


            // Find the maximum displacement for the vector representation
            max_displacement = double.MinValue;
            max_velocity = double.MinValue;
            max_acceleration = double.MinValue;

            int time_points = multidofflexiblecollisionSolver.SimulationResults.TimePoints.Count;

            for (int i = 0; i < time_points; i++)
            {

                (List<double> displacement_at_t, List<double> velocity_at_t, List<double> acceleration_at_t)
                    = multidofflexiblecollisionSolver.SimulationResults.GetStateListAtTimeIndex(i);

                for (int j = 0; j < (fixedendDOF + freeendDOF); j++)
                {
                    max_displacement = Math.Max(max_displacement, Math.Abs(displacement_at_t[j]));
                    max_velocity = Math.Max(max_velocity, Math.Abs(velocity_at_t[j]));
                    max_acceleration = Math.Max(max_acceleration, Math.Abs(acceleration_at_t[j]));
                }
            }


            // reset the maximum displacement for initial condition
            max_displacement = double.MinValue;

            for (int i = 0; i < (fixedendDOF + freeendDOF); i++)
            {
                max_displacement = Math.Max(max_displacement, Math.Abs(u_inl[i]));
            }

        }



        public void paint_twodof_flexibleboundary(ref Shader modelShader)
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
            group_velocity_vector.PaintVectors();

            modelShader.SetVector4("vertexColor", accelerationVectorColor);
            acceleration_vectors.PaintVectors();
            group_acceleration_vector.PaintVectors();

            GL.LineWidth(1.0f);

        }


        public void update_twodof_flexibleboundary_collision(double elapsedRealTime)
        {
            float scale_value = 40.0f; // Scale for visualization   

            (List<double> Displacement, List<double> Velocity, List<double> Acceleration, double contact_force)
                = multidofflexiblecollisionSolver.getResult_at_timet(elapsedRealTime);


            int numDOF = fixedendDOF + freeendDOF; // Total number of degrees of freedom (DOF) for the system

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


            //_______________________________________________________________________________________________________________________________
            // Update the spring locations based on the Displacement values
            for (int i = 0; i < fixedendDOF; i++)
            {
                if (i == 0)
                {
                    springs.updateSpringPosition(i, -50.0f + ptmass_radius, 0.0f, mapped_displacement_list[i], 0.0f); // First spring
                    continue; // Skip the first spring as it is attached to the fixed boundary
                }

                springs.updateSpringPosition(i, mapped_displacement_list[i - 1] + ptmass_radius, 0.0f, 
                    mapped_displacement_list[i], 0.0f); // Subsequent spring

            }

            for (int i = 0; i < freeendDOF; i++)
            {
                int offset = fixedendDOF + i;

                if (i == 0)
                {
                    // First spring of free flight mass segment (either in contact or not in contact)
                    if (contact_force > 0.0f)
                    {
                        // No contact
                        float undeformedspringlength = default_ptmass_location[1] - default_ptmass_location[0];

                        springs.updateSpringPosition(offset, mapped_displacement_list[offset] - undeformedspringlength + ptmass_radius, 0.0f,
                            mapped_displacement_list[offset], 0.0f); // Subsequent spring
                    }
                    else
                    {
                        // Contact with the last mass of fixed end segment
                        springs.updateSpringPosition(offset, mapped_displacement_list[offset - 1] + ptmass_radius, 0.0f,
                            mapped_displacement_list[offset], 0.0f); // Subsequent spring
                    }

                    continue;
                }


                springs.updateSpringPosition(offset, mapped_displacement_list[offset - 1] + ptmass_radius, 0.0f,
                    mapped_displacement_list[offset], 0.0f); // Subsequent spring
            }

            springs.UpdateVertexBuffers();

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


            //_______________________________________________________________________________________________________________________________
            // Update the group velocity vector based on the average location of the fixed end masses
            updateGroupVelocityVector(mapped_displacement_list, Displacement, Velocity, vector_scale_value);
            updateGroupAccelerationVector(mapped_displacement_list, Displacement, Acceleration, vector_scale_value);

        }


        private void updateGroupVelocityVector(List<float> mapped_displacement_list , List<double> Displacement, List<double> Velocity, float vector_scale_value)
        {
            // Update the group velocity vector based on the average location of the fixed end masses
            double fixedend_average_displ = 0;
            double fixedend_group_velocity = 0.0f;
            int idx = 0;
            foreach (double fixedend_mass in fixedend_mass_data)
            {
                fixedend_average_displ += mapped_displacement_list[idx];
                fixedend_group_velocity += fixedend_mass * Velocity[idx];
                idx++;
            }

            fixedend_average_displ /= fixedendDOF;
            fixedend_group_velocity /= total_fixedend_mass;

            double freeend_average_displ = 0;
            double freeend_group_velocity = 0.0f;
            foreach (double freeend_mass in freeend_mass_data)
            {
                freeend_average_displ += mapped_displacement_list[idx];
                freeend_group_velocity += freeend_mass * Velocity[idx];
                idx++;
            }

            freeend_average_displ /= freeendDOF;
            freeend_group_velocity /= total_freeend_mass;



            float groupvelocity_fixed_scaled = ((float)fixedend_group_velocity / Math.Abs((float)max_velocity)) * vector_scale_value;

            group_velocity_vector.updateVectorPosition(0, (float)fixedend_average_displ, 20.0f, groupvelocity_fixed_scaled, 0.0f); // Group velocity vector for fixed end


            float groupvelocity_free_scaled = ((float)freeend_group_velocity / Math.Abs((float)max_velocity)) * vector_scale_value;

            group_velocity_vector.updateVectorPosition(1, (float)freeend_average_displ, 20.0f, groupvelocity_free_scaled, 0.0f); // Group velocity vector for free end


            group_velocity_vector.UpdateVertexBuffers();

        }


        private void updateGroupAccelerationVector(List<float> mapped_displacement_list, List<double> Displacement, List<double> Acceleration, float vector_scale_value)
        {
            // Update the group acceleration vector based on the average location of the fixed end masses
            double fixedend_average_displ = 0;
            double fixedend_group_acceleration = 0.0f;
            int idx = 0;
            foreach (double fixedend_mass in fixedend_mass_data)
            {
                fixedend_average_displ += mapped_displacement_list[idx];
                fixedend_group_acceleration += fixedend_mass * Acceleration[idx];
                idx++;
            }

            fixedend_average_displ /= fixedendDOF;
            fixedend_group_acceleration /= total_fixedend_mass;

            double freeend_average_displ = 0;
            double freeend_group_acceleration = 0.0f;
            foreach (double freeend_mass in freeend_mass_data)
            {
                freeend_average_displ += mapped_displacement_list[idx];
                freeend_group_acceleration += freeend_mass * Acceleration[idx];
                idx++;
            }

            freeend_average_displ /= freeendDOF;
            freeend_group_acceleration /= total_freeend_mass;



            float groupacceleration_fixed_scaled = ((float)fixedend_group_acceleration / Math.Abs((float)max_acceleration)) * vector_scale_value;

            group_acceleration_vector.updateVectorPosition(0, (float)fixedend_average_displ, -20.0f, groupacceleration_fixed_scaled, 0.0f); // Group acceleration vector for fixed end


            float groupacceleration_free_scaled = ((float)freeend_group_acceleration / Math.Abs((float)max_acceleration)) * vector_scale_value;

            group_acceleration_vector.updateVectorPosition(1, (float)freeend_average_displ, -20.0f, groupacceleration_free_scaled, 0.0f); // Group acceleration vector for free end


            group_acceleration_vector.UpdateVertexBuffers();

        }


    }
}
