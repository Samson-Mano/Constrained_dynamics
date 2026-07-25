using spring_mass_sys_visualizer.src.events_handler;
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


namespace spring_mass_sys_visualizer.src.model_store
{
    public class modeldata_store
    {

        // Drawing bound data
        public Vector3 min_bounds = new Vector3(-1);
        public Vector3 max_bounds = new Vector3(1);
        public Vector3 geom_bounds = new Vector3(2);


        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        // Model object visualization
        private Shader modelShader;

        // rectangle data
        private rectangle_store rectangles;
        private circle_store circles;
        private spring_store springs;
        public vector_store vectors;


        bool isModelGeomInitialized = false;

        public modeldata_store()
        {
            // To control the drawing graphics events
            graphic_events_control = new drawing_events(this);

        }


        public void InitializeModelGeom()
        {
            // Initialize the Shader 
            modelShader = new Shader(
                ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.DrawingShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.DrawingShader)
                );


            // Step 1: Set the geometry bounds for the model
            min_bounds = new Vector3(-50);
            max_bounds = new Vector3(50);
            geom_bounds = new Vector3(100);

            graphic_events_control.update_drawing_area_size(graphic_events_control.window_width , graphic_events_control.window_height);


            // Step 2: Create the geometry data for the model
            // Initialize the rectangle data
            rectangles = new rectangle_store();

            // Add a simple rectangle to the model
           // rectangles.AddRectangle(0, 100.0f, 100.0f, 0.0f, 0.0f, 0.0f, false);

          //  rectangles.AddRectangle(1, 80.0f, 60.0f, 0.0f, 0.0f, 0.0f, false);

           // rectangles.AddRectangle(2, 30.0f, 40.0f, 0.0f, 0.0f, 0.0f, false);

            // Initialize the circle data
            circles = new circle_store();

            // Add a simple circle to the model
            circles.AddCircle(0, 50.0f, 0.0f, 0.0f, false);

            circles.AddCircle(1, 8.0f, 15.0f, 25.0f, true);

            circles.AddCircle(2, 10.0f, -10.0f, -25.0f, true);


            // Initialize the spring data
            springs = new spring_store();

            // Add a simple spring to the model
            springs.AddSpring(0, -10.0f, -20.0f, 20.0f, 20.0f);

            springs.AddSpring(1, -30.0f, 10.0f, 30.0f, -10.0f);


            // Initialize the vector data
            vectors = new vector_store();

            // Add a simple vector to the model
            vectors.AddVector(0, 0.0f, 0.0f, 10.0f, 10.0f);

            vectors.AddVector(1, 10.0f, 10.0f, -40.0f, 30.0f);


            // Step 3: Set the buffer data for the geometry data
            rectangles.SetBufferData();
            circles.SetBufferData();
            springs.SetBufferData();
            vectors.SetBufferData();

            isModelGeomInitialized = true;
            update_openTK_uniforms();
        }

        public void PaintModel()
        {
            if (!isModelGeomInitialized)
                return;


            modelShader.Bind();

            Vector4 rectColor = new Vector4(gvariables_static.ColorUtils.get_RectangleColor(),
gvariables_static.geom_transparency * 0.8f);

            Vector4 springColor = new Vector4(gvariables_static.ColorUtils.get_SpringColor(),
gvariables_static.geom_transparency * 0.8f);

            Vector4 circleColor = new Vector4(gvariables_static.ColorUtils.get_CircleColor(),
                gvariables_static.geom_transparency * 0.8f);

            Vector4 vectorColor = new Vector4(gvariables_static.ColorUtils.get_VectorColor(),
                gvariables_static.geom_transparency * 0.8f);


            modelShader.SetVector4("vertexColor", rectColor);
            rectangles.PaintRectangles();
            
            modelShader.SetVector4("vertexColor", circleColor);
            circles.PaintCircles();

            modelShader.SetVector4("vertexColor", springColor);
            GL.LineWidth(3.0f);
            springs.PaintSprings();


            modelShader.SetVector4("vertexColor", vectorColor);
            vectors.PaintVectors();
            GL.LineWidth(1.0f);


            modelShader.UnBind();
        }



        public void update_openTK_uniforms()
        {
            if (!isModelGeomInitialized)
                return;


            Matrix4 uMVP = graphic_events_control.projectionMatrix *
                         graphic_events_control.viewMatrix *
                         graphic_events_control.modelMatrix;

            float zoomscale = (float)graphic_events_control.zoom_val;

            modelShader.SetMatrix4("uMVP", uMVP);

        }
    }
}
