using spring_mass_sys_visualizer.src.model_store.geom_objects;
using spring_mass_sys_visualizer.src.model_store.system4_sdofflexible;
using spring_mass_sys_visualizer.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // private sdof_flexiblecollisionSolver twodofflexiblecollisionSolver;
        // private sdof_flexiblecollisionSolver_num twodofNumericalflexiblecollisionSolver;

        List<float> default_ptmass_location = new List<float>();


        private double max_displacement;
        private double max_velocity;
        private double max_acceleration;


        private double total_simulation_time = -1.0f; // seconds



        public system_twodofflexible_store(double total_simulation_time)
        {



        }



        private void PerformSolve()
        {

        }



        public void paint_twodof_flexibleboundary(ref Shader modelShader)
        {

        }


        public void update_twodof_flexibleboundary_collision(double elapsedRealTime)
        {


        }


        }
    }
