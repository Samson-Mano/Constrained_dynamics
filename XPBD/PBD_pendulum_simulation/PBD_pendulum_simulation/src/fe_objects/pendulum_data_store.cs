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

    }


}
}
