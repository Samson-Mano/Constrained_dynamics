// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using spring_mass_sys_visualizer.src.events_handler;
using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.model_store.system1_store_data;
using spring_mass_sys_visualizer.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace spring_mass_sys_visualizer.src.model_store
{
    public enum AnimationState
    {
        Stopped,
        Running,
        Paused
    }

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

        private system_wrapper_store systemWrapper;

        bool isModelGeomInitialized = false;


        // Animation control data
        private AnimationState _state = AnimationState.Stopped;
        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        double elapsedRealTime = 0.0;

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

            graphic_events_control.update_drawing_area_size(graphic_events_control.window_width, graphic_events_control.window_height);

            systemWrapper = new system_wrapper_store(system_wrapper_store.SystemType.System2Dof);


            gvariables_static.animate_play = true;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = false;
            _state = AnimationState.Running;
            _stopwatch.Start();

            isModelGeomInitialized = true;
            update_openTK_uniforms();
        }

        public void PaintModel()
        {
            if (!isModelGeomInitialized)
                return;


            modelShader.Bind();

            systemWrapper.paintSystem(ref modelShader);


            modelShader.UnBind();
        }


        public void update_model_animation()
        {
            if (_state == AnimationState.Running)
            {
                elapsedRealTime = (_stopwatch.Elapsed.TotalSeconds * gvariables_static.animation_speed);

                if(elapsedRealTime > systemWrapper.total_simulation_time)
                {
                    // Reset the animation stopwatch and time step
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    elapsedRealTime = 0;
                }


                if (!isModelGeomInitialized)
                    return;

                systemWrapper.update_system(elapsedRealTime);
            }
        }


        public void start_animation()
        {
            // Start the animation
            if (_state != AnimationState.Running)
            {
                _stopwatch.Start();
                _state = AnimationState.Running;
            }

        }


        public void pause_animation()
        {
            // Pause the animation
            if (_state == AnimationState.Running)
            {
                _stopwatch.Stop(); // Stop = Pause
                _state = AnimationState.Paused;

            }
        }


        public void stop_animation()
        {
            // Reset the animation stopwatch and time step
            _stopwatch.Reset();
            elapsedRealTime = 0.0;
            _state = AnimationState.Stopped;

            systemWrapper.update_system(elapsedRealTime);

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
