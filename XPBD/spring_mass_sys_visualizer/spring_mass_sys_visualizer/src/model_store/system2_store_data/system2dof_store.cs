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



namespace spring_mass_sys_visualizer.src.model_store.system2_store_data
{
    public class system2dof_store
    {
        // Geometry data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store vectors;

        private sdof2d_rigidcollisionSolver twodof_springsolver;


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = 20.0; // seconds


        public system2dof_store(double total_simulation_time) 
        {
            // Initialize the system2dof_store
            this.total_simulation_time = total_simulation_time;

            // Initialize the rectangle data
            rigidboundary = new rectangle_store();

            // Add rigid boundary rectangles to the model
            rigidboundary.AddRectangle(0, 100.0f, 10.0f, 0.0f, -50.0f, 0.0f, true);
            // rigidboundary.AddRectangle(1, 200.0f, 0.05f, 0.0f, 0.0f, 0.0f, false);


            // Initialize the circle data
            pointmass = new circle_store();

            // Add a simple circle to the model
            pointmass.AddCircle(0, 45.0f, 0.0f, 0.0f, false);

            pointmass.AddCircle(1, 5.0f, 0.0f, 0.0f - (40.0f / 3.0f), true); // Mass M1
            pointmass.AddCircle(2, 5.0f, 0.0f, 0.0f + (40.0f / 3.0f), true); // Mass M2


            // Initialize the spring data
            springs = new spring_store();

            // Add a simple spring to the model
            springs.AddSpring(0, 0.0f, 0.0f - (40.0f / 3.0f), 0.0f, -45.0f); // Spring between mass M1 and the rigid boundary
            springs.AddSpring(1, 0.0f, 0.0f - (40.0f / 3.0f), 0.0f, 0.0f + (40.0f / 3.0f)); // Spring between mass M1 and mass M2

            // Example model
            double mass_m1 = 0.001; // 1 KG
            double mass_m2 = 0.002; // 2 KG

            double stiff_k1 = 0.5; // Stiffness k1 spring
            double stiff_k2 = 0.2; // Stiffness k2 spring

            double dampratio_zeta = 0.015; // Damping ratio

            double gravity_g = -9806.65 * 1.0; // mm/s^2

            // Initialize the two DOF rigid collision solver
            twodof_springsolver = new sdof2d_rigidcollisionSolver(mass_m1: mass_m1,
                stiffness_k1: stiff_k1, mass_m2: mass_m2,
                stiffness_k2: stiff_k2, dampratio_zeta: dampratio_zeta,
                const_accla0: gravity_g);


            twodof_springsolver.solve_sdof2_rigidcollision(total_simulation_time: total_simulation_time,
                max_time_increment: 0.01, u1_inl: 100.0, u2_inl: 100.0, v1_inl: 0.0, v2_inl: 0.0);


            // Find the maximum displacement for the vector representation
            max_displacement = double.MinValue;
            max_velocity = double.MinValue;
            max_acceleration = double.MinValue;

            foreach (var rslt in twodof_springsolver.SimulationResults.Node1Response)
            {
                max_displacement = Math.Max(max_displacement, Math.Abs(rslt.displacement));
                max_velocity = Math.Max(max_velocity, Math.Abs(rslt.velocity));
                max_acceleration = Math.Max(max_acceleration, Math.Abs(rslt.acceleration));

            }

            foreach (var rslt in twodof_springsolver.SimulationResults.Node2Response)
            {
                max_displacement = Math.Max(max_displacement, Math.Abs(rslt.displacement));
                max_velocity = Math.Max(max_velocity, Math.Abs(rslt.velocity));
                max_acceleration = Math.Max(max_acceleration, Math.Abs(rslt.acceleration));

            }




            // Initialize the vector data
            vectors = new vector_store();

            // Add a simple vector to the model
           vectors.AddVector(0, 0.0f, 0.0f, 10.0f, 10.0f); // Velocity vector for mass M1
           vectors.AddVector(1, 10.0f, 10.0f, -40.0f, 30.0f); // Acceleration vector for mass M1

           vectors.AddVector(2, 0.0f, 0.0f, 10.0f, 10.0f); // Velocity vector for mass M2
           vectors.AddVector(3, 10.0f, 10.0f, -40.0f, 30.0f); // Acceleration vector for mass M2


            // Step 3: Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();
            vectors.SetBufferData();


        }

        public void paint_system2(ref Shader modelShader)
        {
            // Implement the painting logic for system2

            Vector4 rectColor = new Vector4(gvariables_static.ColorUtils.get_RectangleColor(),
gvariables_static.geom_transparency * 0.8f);

            Vector4 springColor = new Vector4(gvariables_static.ColorUtils.get_SpringColor(),
gvariables_static.geom_transparency * 0.8f);

            Vector4 circleColor = new Vector4(gvariables_static.ColorUtils.get_CircleColor(),
                gvariables_static.geom_transparency * 0.8f);

            Vector4 vectorColor = new Vector4(gvariables_static.ColorUtils.get_VectorColor(),
                gvariables_static.geom_transparency * 0.8f);


            modelShader.SetVector4("vertexColor", rectColor);
            rigidboundary.PaintRectangles();

            modelShader.SetVector4("vertexColor", circleColor);
            pointmass.PaintCircles();

            modelShader.SetVector4("vertexColor", springColor);
            GL.LineWidth(3.0f);
            springs.PaintSprings();


            modelShader.SetVector4("vertexColor", vectorColor);
            vectors.PaintVectors();
            GL.LineWidth(1.0f);
        }

        public void update_system2(double elapsedRealTime)
        {
            // Get the response for the current time from the simulation results
            List<sdof2d_rigidcollisionResponse> respList = twodof_springsolver.getResult_at_timet(elapsedRealTime);

            float scale_value = 30.0f; // Scale for visualization   

            // node 1 Response
            sdof2d_rigidcollisionResponse node1Resp = respList[0];

            double node1_displ_at_t = node1Resp.displacement;

            // Map to [-1, 1] range for OpenGL coordinates
            // double mapped_displacement = 2.0 * ((displ_at_t - min_displacement) / (max_displacement - min_displacement)) - 1.0;
            double node1_mapped_displacement = node1_displ_at_t / Math.Abs(max_displacement); // Scale down for visualization


            pointmass.updateCirclePosition(1, 0.0f, -(40.0f / 3.0f) + (float)node1_mapped_displacement * scale_value); // Scale for visualization
            // pointmass.updateCirclePosition(1, 0.0f, (float)node1_mapped_displacement * scale_value); // Scale for visualization

            //if (response_at_t.contact_force > 0.0f)
            //{
            //    // No contact
            //    springs.updateSpringPosition(0, 0.0f, (float)mapped_displacement * scale_value, 0.0f,
            //    ((float)mapped_displacement * scale_value) - 45.0f);

            //}
            //else
            //{
            //    // Contact with the rigid boundary
            //    // You can implement any additional logic here if needed

            //    springs.updateSpringPosition(0, 0.0f, (float)mapped_displacement * scale_value, 0.0f, -45.0f);

            //}

            //// Update the vector position based on velocity and acceleration
            //double mapped_velocity = response_at_t.velocity / Math.Abs(max_velocity);
            //double mapped_acceleration = response_at_t.acceleration / Math.Abs(max_acceleration);

            //vectors.updateVectorPosition(0, 10.0f, (float)mapped_displacement * scale_value,
            //    0.0f, scale_value * (float)mapped_velocity);

            //vectors.updateVectorPosition(1, 20.0f, (float)mapped_displacement * scale_value,
            //    0.0f, scale_value * (float)mapped_acceleration);






            // node 2 Response
            sdof2d_rigidcollisionResponse node2Resp = respList[1];

            double node2_displ_at_t = node2Resp.displacement;

            // Map to [-1, 1] range for OpenGL coordinates
            // double mapped_displacement = 2.0 * ((displ_at_t - min_displacement) / (max_displacement - min_displacement)) - 1.0;
            double node2_mapped_displacement = node2_displ_at_t / Math.Abs(max_displacement); // Scale down for visualization


            pointmass.updateCirclePosition(2, 0.0f, (40.0f / 3.0f) + (float)node2_mapped_displacement * scale_value); // Scale for visualization



            pointmass.UpdateVertexBuffers();


        }



    }
}
