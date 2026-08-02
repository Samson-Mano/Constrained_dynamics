using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system4_sdofflexible
{
    public class twodof_flexiblecollisionSolver
    {

        private double mass_m1;
        private double mass_m2;

        private double stiffness_k1;
        private double stiffness_k2;

        private double dampratio_zeta;
        private double const_accla0;


        public twodof_flexiblecollisionSolver(double m1, double m2, double k1, double k2, double zeta, double accla0)
        {
            mass_m1 = m1;
            mass_m2 = m2;
            stiffness_k1 = k1;
            stiffness_k2 = k2;
            dampratio_zeta = zeta;
            const_accla0 = accla0;
        }





        private (double u, double v, double a) GetSDOFResponse(double time_t, double mass_m, double stiff_k,
            double zeta, double u_inl, double v_inl, double const_a0)
        {

            double omega_n = Math.Sqrt(stiff_k / mass_m); // ωn = √(k/m)
            double omega_D = omega_n * Math.Sqrt(1 - zeta * zeta); // ωD = ωn √(1 - ζ²)


            double exp_term = Math.Exp(-zeta * omega_n * time_t); // e^{‑ζ ωn τ}
            double cos_term = Math.Cos(omega_D * time_t); // cos(ωD t)
            double sin_term = Math.Sin(omega_D * time_t); // sin(ωD t)

            double zeta_squared = zeta * zeta; // ζ²
            double damp_term = (zeta / Math.Sqrt(1 - zeta_squared)); // ζ / √(1 - ζ²)

            double omega_n_squared = omega_n * omega_n; // ωn²


            //_____________________________________________________________________________________________________
            // Particular (Forced response) solution 
            double u_static = const_a0 / omega_n_squared; // u_static = a0 / ωn²

            double A1 = -u_static;
            double A2 = -u_static * damp_term;

            double u_particular = exp_term * ((A1 * cos_term) + (A2 * sin_term)) + u_static;
            double v_particular = exp_term * (const_a0 / omega_D) * sin_term;


            double a_particular = const_a0 * exp_term * (cos_term - (damp_term * sin_term));


            //_____________________________________________________________________________________________________
            // Homogeneous (Free response) complementary solution
            double C1 = u_inl;
            double C2 = (v_inl / omega_D) + (damp_term * u_inl);

            double u_homogeneous = exp_term * ((C1 * cos_term) + (C2 * sin_term));


            double C3 = ((u_inl * omega_n_squared) / omega_D) + (damp_term * v_inl);

            double v_homogeneous = -exp_term * ((C3 * sin_term) - (v_inl * cos_term));

            double C4 = (2.0 * zeta * omega_n * v_inl) + (omega_n_squared * u_inl);
            double C5_1 = (zeta * omega_n_squared * u_inl);
            double C5_2 = ((2.0 * zeta_squared) - 1.0) * omega_n * v_inl;
            double C5 = (C5_1 + C5_2) / Math.Sqrt(1.0 - zeta_squared);

            double a_homogeneous = -exp_term * (C4 * cos_term - C5 * sin_term);

            return (u_particular + u_homogeneous, v_particular + v_homogeneous, a_particular + a_homogeneous);

        }







    }
}
