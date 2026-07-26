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
        public vector_store vectors;



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


        public void update_system1(ref double elapsedRealTime)
        {
            // Implement the update logic for system1
        }


    }
}
