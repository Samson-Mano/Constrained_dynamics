using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system3_mdof_data
{

    public struct multidof1d_rigidcollisionResponse
    {
        public double displacement;
        public double velocity;
        public double acceleration;

    }


    public class mdof1d_rigidcollisionSolver
    {
        public List<double> TimePoints { get; set; }
        public List<double> ContactForce { get; set; }
        public List<double> TimeContactBand { get; set; }






    }
}
