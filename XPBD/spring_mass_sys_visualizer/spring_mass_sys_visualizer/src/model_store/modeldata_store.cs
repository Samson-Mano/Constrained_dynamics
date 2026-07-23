using OpenTK;
using spring_mass_sys_visualizer.src.events_handler;
using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Initialize the rectangle data
            rectangles = new rectangle_store();

            // Add a simple rectangle to the model
            rectangles.AddRectangle(0, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, false);

            rectangles.UpdateVertexBuffers();






            isModelGeomInitialized = true;
            update_openTK_uniforms();
        }

        public void PaintModel()
        {
            if (!isModelGeomInitialized)
                return;


            modelShader.Bind();

            Vector4 rectColor = new Vector4(gvariables_static.ColorUtils.get_PtColor(),
gvariables_static.geom_transparency * 0.8f);

            modelShader.SetVector4("vertexColor", rectColor);

            rectangles.PaintRectangles();


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
