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


namespace spring_mass_sys_visualizer.src.model_store.system1_store_data
{
    public class system1_store
    {

        // rectangle data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store vectors;

        private sdof1d_rigidcollisionSolver springsolver;

        private double max_displacement;
        private double min_displacement;

        public double total_simulation_time = 10.0; // seconds


        public system1_store()
        {
            // Initialize the system1_store

            // Initialize the rectangle data
            rigidboundary = new rectangle_store();

            // Add rigid boundary rectangles to the model
            rigidboundary.AddRectangle(0, 100.0f, 10.0f, 0.0f, -50.0f, 0.0f, true);


            // Initialize the circle data
            pointmass = new circle_store();

            // Add a simple circle to the model
            pointmass.AddCircle(0, 50.0f, 0.0f, 40.0f, false);
            pointmass.AddCircle(1, 5.0f, 0.0f, 40.0f, true);


            // Initialize the spring data
            springs = new spring_store();

            // Add a simple spring to the model
            springs.AddSpring(0, 0.0f, 40.0f, 0.0f, -10.0f);

            // Example model
            double mass_m0 = 0.001; // 1 KG
            double nat_freq = 4.0; // Hz
            double stiff_k = mass_m0 * (nat_freq * 2.0 * Math.PI) * (nat_freq * 2.0 * Math.PI);
            double gravity_g = -9806.65; // m/s^2

            // Initialize the rigid collision solver
            springsolver = new sdof1d_rigidcollisionSolver(mass_m: mass_m0, 
                stiffness_k: stiff_k, dampratio_zeta: 0.05, const_accla0: gravity_g);

            springsolver.solve_sdof1d_rigidcollision(total_time: total_simulation_time, max_time_increment: 0.01,
                initial_displacement: 100.0, initial_velocity: 0.0);

            // Find the maximum displacement for the vector representation
            max_displacement = double.MinValue;
            min_displacement = double.MaxValue;

            foreach(var rslt in springsolver.responseList)
            {
                if (rslt.displacement > max_displacement)
                {
                    max_displacement = rslt.displacement;
                }
                if (rslt.displacement < min_displacement)
                {
                    min_displacement = rslt.displacement;
                }
            }



            // Initialize the vector data
            vectors = new vector_store();

            // Add a simple vector to the model
            // vectors.AddVector(0, 0.0f, 0.0f, 10.0f, 10.0f);

            // vectors.AddVector(1, 10.0f, 10.0f, -40.0f, 30.0f);


            // Step 3: Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();
            springs.SetBufferData();
            vectors.SetBufferData();


        }

        public void paint_system1(ref Shader modelShader)
        {
            // Implement the painting logic for system1

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


        public void update_system1(double elapsedRealTime)
        {
            // Implement the update logic for system1
            sdof1d_rigidcollisionResponse response_at_t = springsolver.getResult_at_timet(elapsedRealTime);

            double displ_at_t = response_at_t.displacement;

            // Map to [-1, 1] range for OpenGL coordinates
            double mapped_displacement = 2.0 * ((displ_at_t - min_displacement) / (max_displacement - min_displacement)) - 1.0;

            pointmass.updateCirclePosition(0, 0.0f, (float)mapped_displacement * 50.0f); // Scale for visualization
            pointmass.updateCirclePosition(1, 0.0f, (float)mapped_displacement * 50.0f); // Scale for visualization

            pointmass.UpdateVertexBuffers();

        }


    }
}
