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

        int num_DOF = 4; // Number of degrees of freedom

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

            foreach (float location in default_ptmass_location)
            {
                pointmass.AddCircle(ptmass_id, 5.0f, 0.0f, location, true);
                ptmass_id++;
            }




            // Initialize the spring data
            springs = new spring_store();




            // Initialize the velocity vectors
            velocity_vectors = new vector_store();

            // Initialize the acceleration vectors
            acceleration_vectors = new vector_store();



            // Step 3: Set the buffer data for the geometry data
            rigidboundary.SetBufferData();
            pointmass.SetBufferData();

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

            //modelShader.SetVector4("vertexColor", springColor);
            //GL.LineWidth(3.0f);
            //springs.PaintSprings();


            //modelShader.SetVector4("vertexColor", velocityVectorColor);
            //velocity_vectors.PaintVectors();

            //modelShader.SetVector4("vertexColor", accelerationVectorColor);
            //acceleration_vectors.PaintVectors();

            //GL.LineWidth(1.0f);
        }


        public void update_system3(double elapsedRealTime)
        {

        }


    }
}
