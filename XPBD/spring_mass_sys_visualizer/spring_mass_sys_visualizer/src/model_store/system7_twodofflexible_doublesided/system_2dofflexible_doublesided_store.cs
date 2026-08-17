using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.model_store.system6_sdofflexible_doublesided;
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




namespace spring_mass_sys_visualizer.src.model_store.system7_twodofflexible_doublesided
{
    public class system_2dofflexible_doublesided_store
    {

        // Geometry data
        private rectangle_store rigidboundary;
        private circle_store pointmass;
        private spring_store springs;
        private vector_store velocity_vectors;
        private vector_store acceleration_vectors;

       //  private sdof_doublesided_flexiblecollisionSolver sdofdoublesided_flexiblecollisionSolver;

        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = -1.0f; // seconds


        const float ptmass_radius = 1.0f; // Radius of the point mass circles
        const float spring_element_wd = 0.5f; // Width of the spring elements
        const float total_width = 150.0f; // Total width of the system

        const float relaxed_spring_length = 10.0f; // Relaxed length of the springs

        public system_2dofflexible_doublesided_store(double total_simulation_time)
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

            // Free floating mass 1
            location = 0.0f - total_width * (1.0f / 6.0f);
            default_ptmass_location.Add(location);

            // Free floating mass 2
            location = 0.0f + total_width * (1.0f / 6.0f);
            default_ptmass_location.Add(location);

            // Right flexible boundary at 50.0f
            location = (total_width * 0.5f) - relaxed_spring_length;
            default_ptmass_location.Add(location);



            // Initialize the circle (point mass) data
            pointmass = new circle_store();

            // // Add the reference circle with Radius 45.0f to the model
            // pointmass.AddCircle(0, 45.0f, 0.0f, 0.0f, false); // Reference circle

            // First mass m1 (attached to the flexible boundary)
            pointmass.AddCircle(0, ptmass_radius, default_ptmass_location[0], 0.0f, true); // First mass

            // Second mass m2 (free floating mass)
            pointmass.AddCircle(1, ptmass_radius, default_ptmass_location[1], 0.0f, true); // Second mass

            // Third mass m3 (free floating mass)
            pointmass.AddCircle(2, ptmass_radius, default_ptmass_location[2], 0.0f, true); // Third mass

            // Fourth mass m4 (attached to the flexible boundary)
            pointmass.AddCircle(3, ptmass_radius, default_ptmass_location[3], 0.0f, true); // Fourth mass


            // Add a reference circle with Radius relaxed spring length to the model
            pointmass.AddCircle(4, relaxed_spring_length, default_ptmass_location[1], 0.0f, false); // Reference circle 1 

            pointmass.AddCircle(5, relaxed_spring_length, default_ptmass_location[2], 0.0f, false); // Reference circle 2




            // Initialize the spring data
            springs = new spring_store();
            gvariables_static.spring_element_width = spring_element_wd; // Set the spring element width to 2.0f

            // First spring (attached to the flexible boundary and first mass)
            springs.AddSpring(0, -(total_width * 0.5f) + ptmass_radius, 0.0f, default_ptmass_location[0], 0.0f); // First spring

            // Free floating mass 1 (attached to the first spring and second spring)
            // Second spring (free floating mass - left free spring)
            springs.AddSpring(1, default_ptmass_location[1] - relaxed_spring_length, 0.0f, default_ptmass_location[1], 0.0f); // Second spring

            // Third spring (free floating mass - right free spring)
            springs.AddSpring(2, default_ptmass_location[1], 0.0f, default_ptmass_location[1] + relaxed_spring_length, 0.0f); // Third spring


            // Free floating mass 2 (attached to the third spring and fourth spring)
            // Second spring (free floating mass - left free spring)
            springs.AddSpring(3, default_ptmass_location[2] - relaxed_spring_length, 0.0f, default_ptmass_location[2], 0.0f); // Second spring

            // Third spring (free floating mass - right free spring)
            springs.AddSpring(4, default_ptmass_location[2], 0.0f, default_ptmass_location[2] + relaxed_spring_length, 0.0f); // Third spring



            // Fourth spring (attached to the flexible boundary and fourth mass)
            springs.AddSpring(5, default_ptmass_location[3], 0.0f, (total_width * 0.5f) - ptmass_radius, 0.0f); // Fourth spring


            PerformSolve();


            // Initialize the vector data
            velocity_vectors = new vector_store();
            acceleration_vectors = new vector_store();

            // Add a simple vector to the model
            velocity_vectors.AddVector(0, default_ptmass_location[0], 10.0f, 1.0f, 0.0f); // mass attached to left flexible boundary
            velocity_vectors.AddVector(1, default_ptmass_location[1], 10.0f, 1.0f, 0.0f); // Free floating mass 1 
            velocity_vectors.AddVector(2, default_ptmass_location[2], 10.0f, 1.0f, 0.0f); // Free floating mass 2 
            velocity_vectors.AddVector(3, default_ptmass_location[3], 10.0f, 1.0f, 0.0f); // mass attached to right flexible boundary


            acceleration_vectors.AddVector(0, default_ptmass_location[0], -10.0f, 1.0f, 0.0f); // mass attached to left flexible boundary
            acceleration_vectors.AddVector(1, default_ptmass_location[1], -10.0f, 1.0f, 0.0f); // Free floating mass 1
            acceleration_vectors.AddVector(2, default_ptmass_location[2], -10.0f, 1.0f, 0.0f); // Free floating mass 2
            acceleration_vectors.AddVector(3, default_ptmass_location[3], -10.0f, 1.0f, 0.0f); // mass attached to right flexible boundary


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




        public void paint_twodof_doublesided_flexibleboundary(ref Shader modelShader)
        {

            // Implement the painting logic for twodof_doublesided_flexibleboundary

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



        public void update_twodof_doublesided_flexibleboundary_collision(double elapsedRealTime)
        {
            float scale_value = 40.0f; // Scale for visualization   



        }













    }
}
