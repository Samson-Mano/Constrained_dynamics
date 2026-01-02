using OpenTK;
using PBD_pendulum_simulation.src.global_variables;
using PBD_pendulum_simulation.src.opentk_control.opentk_bgdraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBD_pendulum_simulation.src.fe_objects
{
    public class pendulum_data_store
    {

        // Stores data of the Three pendulum
        const int number_of_mass = 3;

        public double mass1_value = 0.0;
        public double mass2_value = 0.0;
        public double mass3_value = 0.0; 

        public double length1_value = 0.0;
        public double length2_value = 0.0;
        public double length3_value = 0.0;

        public double initial_angle_mass1 = 0.0;
        public double initial_angle_mass2 = 0.0;
        public double initial_angle_mass3 = 0.0;


        public elementmass_store fe_mass1;
        public elementmass_store fe_mass2;
        public elementmass_store fe_mass3;
        public elementlink_store fe_rigidlink1;
        public elementlink_store fe_rigidlink2;
        public elementlink_store fe_rigidlink3;


        public pendulum_data_store(double mass1, double mass2, double mass3,
            double length1, double length2, double length3,
            double initial_angle1, double initial_angle2, double initial_angle3)
        {
                        
            mass1_value = mass1;
            mass2_value = mass2;
            mass3_value = mass3;

            length1_value = length1;
            length2_value = length2;
            length3_value = length3;

            initial_angle_mass1 = initial_angle1;
            initial_angle_mass2 = initial_angle2;
            initial_angle_mass3 = initial_angle3;



            // -------------------------------
            // Constants
            // -------------------------------
            const float TOTAL_SCREEN_LENGTH = 400f;
            const float MAX_MASS_SIZE = 50.0f;
            const float MIN_MASS_SIZE = 0.01f;

            // -------------------------------
            // Mass scaling
            // -------------------------------
            double maxMass = Math.Max(mass1, Math.Max(mass2, mass3));


            float screen_mass1_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE, mass1);
            float screen_mass2_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE, mass2);
            float screen_mass3_size = (float)gvariables_static.GetRemap(maxMass, 0.0, MAX_MASS_SIZE, MIN_MASS_SIZE, mass3);


            // -------------------------------
            // Length scaling
            // -------------------------------
            double total_length = length1 + length2 + length3;

            double length_ratio1 = (length1 / total_length) * TOTAL_SCREEN_LENGTH;
            double length_ratio2 = (length2 / total_length) * TOTAL_SCREEN_LENGTH;
            double length_ratio3 = (length3 / total_length) * TOTAL_SCREEN_LENGTH;

            // -------------------------------
            // Position helper
            // -------------------------------
            double UserAngleToPendulumRad(double userDeg)
            {
                return Math.PI / 2.0 - userDeg * Math.PI / 180.0;
            }


            Vector2 NextPoint(Vector2 origin, double angle, double length)
            {
                return origin + new Vector2(
                    (float)(length * Math.Sin(angle)),
                     -(float)(length * Math.Cos(angle))
                );
            }

            // -------------------------------
            // Initial positions
            // -------------------------------
            Vector2 p0 = Vector2.Zero;
            Vector2 p1 = NextPoint(p0, UserAngleToPendulumRad(initial_angle1), length_ratio1);
            Vector2 p2 = NextPoint(p1, UserAngleToPendulumRad(initial_angle2), length_ratio2);
            Vector2 p3 = NextPoint(p2, UserAngleToPendulumRad(initial_angle3), length_ratio3);


            // Set the drawing data
            // Masses
            fe_mass1 = new elementmass_store(p1, screen_mass1_size);
            fe_mass2 = new elementmass_store(p2, screen_mass2_size);
            fe_mass3 = new elementmass_store(p3, screen_mass3_size);

            // Rigid link
            fe_rigidlink1 = new elementlink_store(p0, p1);
            fe_rigidlink2 = new elementlink_store(p1, p2);
            fe_rigidlink3 = new elementlink_store(p2, p3);

        }


        public void paint_pendulum()
        {

            // Paint the masses
            fe_mass1.paint_pointmass();
            fe_mass2.paint_pointmass();
            fe_mass3.paint_pointmass();

            // Paint the rigid link
            fe_rigidlink1.paint_rigidlink();
            fe_rigidlink2.paint_rigidlink();
            fe_rigidlink3.paint_rigidlink();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
                                  drawing_events graphic_events_control)
        {


            // Update mass openTK uniforms
            fe_mass1.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_mass2.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_mass3.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);



            // Update the rigid link
            fe_rigidlink1.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_rigidlink2.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            fe_rigidlink3.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);



        }



    }
}
