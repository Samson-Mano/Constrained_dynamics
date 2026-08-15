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


        public system_flexible_doublesided_store(double total_simulation_time)
        {

            // Initialize the multi dof_store
            this.total_simulation_time = total_simulation_time;

            // Initialize the rectangle data
            rigidboundary = new rectangle_store();

            // Add rigid boundary rectangles to the model
            rigidboundary.AddRectangle(0, 10.0f, 100.0f, -50.0f, 0.0f, 0.0f, true);
            rigidboundary.AddRectangle(1, 10.0f, 100.0f, 50.0f, 0.0f, 0.0f, true);


            default_ptmass_location = new List<float>();
            int numDOF = 2; // Number of degrees of freedom (DOF) for the system

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

            // First mass m1 (attached to the flexible boundary)
            pointmass.AddCircle(0, 5.0f, default_ptmass_location[0], 0.0f, true); // First mass

            // Second mass m2 (free floating mass)
            pointmass.AddCircle(1, 5.0f, default_ptmass_location[1], 0.0f, true); // Second mass



            // Initialize the spring data
            springs = new spring_store();
            gvariables_static.spring_element_width = 1.5f; // Set the spring element width to 2.0f

            // First spring (attached to the flexible boundary and first mass)
            springs.AddSpring(0, default_ptmass_location[0], 0.0f, -45.0f, 0.0f); // First spring

            // Second spring (attached to the first mass and second mass)
            springs.AddSpring(1, default_ptmass_location[1], 0.0f, default_ptmass_location[0], 0.0f); // Second spring


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

          

        }







    }
}
