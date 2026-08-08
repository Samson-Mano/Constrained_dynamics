using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system5_twodofflexible
{
    public class twodof_flexiblecollisionSolver
    {

        private List<double> fixedend_mass = new List<double>();
        private List<double> fixedend_stiffness = new List<double>();

        private List<double> freeend_mass = new List<double>();
        private List<double> freeend_stiffness = new List<double>();

        private double dampratio_zeta = 0.0;
        private double const_accla0 = 0.0;




    }
}
