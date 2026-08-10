using spring_mass_sys_visualizer.src.model_store.system1_store_data;
using spring_mass_sys_visualizer.src.model_store.system2_store_data;
using spring_mass_sys_visualizer.src.model_store.system3_mdof_data;
using spring_mass_sys_visualizer.src.model_store.system4_sdofflexible;
using spring_mass_sys_visualizer.src.model_store.system5_twodofflexible;
using spring_mass_sys_visualizer.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store
{
    public class system_wrapper_store
    {

        public enum SystemType
        {
            System1Dof,
            System2Dof,
            SystemMDOF,
            SystemFlexible1Dof,
            SystemFlexible1Dof_Rotated,
            SystemFlexible2Dof,
        }


        // System control
        private system1dof_store system1;
        private system2dof_store system2;
        private system_mdof_store systemMDOF;
        private system_flexible_store systemFlexible1Dof;
        private system_flexible_store_rotated systemFlexible1Dof_Rotated;
        private system_twodofflexible_store system2DofFlexible;

        private SystemType currentSystemType = SystemType.System1Dof;

        public double total_simulation_time = 8.0; // seconds

        public system_wrapper_store(SystemType systemType)
        {
            // Initialize the system_wrapper_store
            currentSystemType = systemType;

            if (currentSystemType == SystemType.System1Dof)
            {
                // system 1dof
                system1 = new system1dof_store(total_simulation_time);
            }
            else if (currentSystemType == SystemType.System2Dof)
            {
                // system 2dof
                system2 = new system2dof_store(total_simulation_time);
            }
            else if (currentSystemType == SystemType.SystemMDOF)
            {
                // system MDOF
                systemMDOF = new system_mdof_store(total_simulation_time);
            }
            else if (currentSystemType == SystemType.SystemFlexible1Dof)
            {
                // system Flexible 1dof
                systemFlexible1Dof = new system_flexible_store(total_simulation_time);
            }
            else if (currentSystemType == SystemType.SystemFlexible1Dof_Rotated)
            {
                // system Flexible 1dof rotated
                systemFlexible1Dof_Rotated = new system_flexible_store_rotated(total_simulation_time);
            }
            else if (currentSystemType == SystemType.SystemFlexible2Dof)
            {
                // system Flexible 2dof
                system2DofFlexible = new system_twodofflexible_store(total_simulation_time);
            }
        }

        public void paintSystem(ref Shader modelShader)
        {
            if (currentSystemType == SystemType.System1Dof)
            {
                // Paint system 1dof
                // Implement the painting logic for system 1dof here
                system1.paint_system1(ref modelShader);
            }
            else if (currentSystemType == SystemType.System2Dof)
            {
                // Paint system 2dof
                // Implement the painting logic for system 2dof here
                system2.paint_system2(ref modelShader);
            }
            else if (currentSystemType == SystemType.SystemMDOF)
            {
                // Paint system MDOF
                // Implement the painting logic for system MDOF here
                systemMDOF.paint_system3(ref modelShader);
            }
            else if (currentSystemType == SystemType.SystemFlexible1Dof)
            {
                // Paint system Flexible 1dof
                // Implement the painting logic for system Flexible 1dof here
                systemFlexible1Dof.paint_sdof_flexibleboundary(ref modelShader);
            }
            else if (currentSystemType == SystemType.SystemFlexible1Dof_Rotated)
            {
                // Paint system Flexible 1dof rotated
                // Implement the painting logic for system Flexible 1dof rotated here
                systemFlexible1Dof_Rotated.paint_sdof_flexibleboundary_rotated(ref modelShader);
            }
            else if (currentSystemType == SystemType.SystemFlexible2Dof)
            {
                // Paint system Flexible 2dof
                // Implement the painting logic for system Flexible 2dof here
                system2DofFlexible.paint_twodof_flexibleboundary(ref modelShader);
            }
        }



        public void update_system(double elapsedRealTime)
        {
            if (currentSystemType == SystemType.System1Dof)
            {
                system1.update_system1(elapsedRealTime);
            }
            else if (currentSystemType == SystemType.System2Dof)
            {
                system2.update_system2(elapsedRealTime);
            }
            else if (currentSystemType == SystemType.SystemMDOF)
            {
                systemMDOF.update_system3(elapsedRealTime);
            }
            else if (currentSystemType == SystemType.SystemFlexible1Dof)
            {
                systemFlexible1Dof.update_sdof_flexibleboundary_collision(elapsedRealTime);
            }
            else if (currentSystemType == SystemType.SystemFlexible1Dof_Rotated)
            {
                systemFlexible1Dof_Rotated.update_sdof_flexibleboundary_collision_rotated(elapsedRealTime);
            }
            else if (currentSystemType == SystemType.SystemFlexible2Dof)
            {
                system2DofFlexible.update_twodof_flexibleboundary_collision(elapsedRealTime);
            }
        }



    }
}
