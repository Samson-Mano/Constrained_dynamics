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

        private twodof_flexiblecollisionSolver multidofflexiblecollisionSolver;


        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = -1.0f; // seconds

        private float ptmass_radius = 2.0f; // Radius of the point mass circles
        private int fixedendDOF = 1; // Number of fixed end degrees of freedom (DOF) for the system
        private int freeendDOF = 1; // Number of free end degrees of freedom (DOF) for the system


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
            gvariables_static.spring_element_width = 1.5f; // Set the spring element width to 2.0f


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


            // Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();
            velocity_vectors.SetBufferData();
            acceleration_vectors.SetBufferData();


        }



        private void PerformSolve()
        {

            List<double> fixedend_mass = new List<double> { 0.002 }; // Mass of the fixed end segment
            List< double > fixedend_stiffness = new List<double> { 0.018 }; // Stiffness of the fixed end segment
            List<double> freeend_mass = new List<double> { 0.002 }; // Mass of the free end segment
            List< double > freeend_stiffness = new List<double> { 0.018 }; // Stiffness of the free end segment

            List<double> u_inl = new List<double> {0.0, 1000.0  }; 
            List< double > v_inl = new List<double> {0.0, -500.0 };


            if (fixedendDOF != fixedend_mass.Count || fixedendDOF != fixedend_stiffness.Count ||
                freeendDOF != freeend_mass.Count || freeendDOF != freeend_stiffness.Count)
            {
                throw new ArgumentException("Mismatch between DOF and mass/stiffness list lengths.");
            }

            if((fixedendDOF + freeendDOF) != u_inl.Count || (fixedendDOF + freeendDOF) != v_inl.Count)
            {
                throw new ArgumentException("Mismatch between DOF and initial condition list lengths.");
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

                for (int j = 0; j < 2; j++)
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

            modelShader.SetVector4("vertexColor", accelerationVectorColor);
            acceleration_vectors.PaintVectors();

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


        }


    }
}
