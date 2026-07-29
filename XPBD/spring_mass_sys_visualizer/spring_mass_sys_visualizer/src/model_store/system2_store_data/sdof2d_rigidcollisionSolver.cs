// using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using spring_mass_sys_visualizer.src.model_store.system1_store_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace spring_mass_sys_visualizer.src.model_store.system2_store_data
{

    public struct sdof2d_rigidcollisionResponse
    {
        public double displacement;
        public double velocity;
        public double acceleration;

    }

    public class ModalProperties
    {
        public Matrix<double> MassMatrix { get; set; }
        public Matrix<double> StiffnessMatrix { get; set; }
        public Matrix<double> DampingMatrix { get; set; }

        public double[] AngularNaturalFrequencies { get; set; }      // rad/s
        public double[] NaturalFrequenciesHz { get; set; }          // Hz
        public double[] Periods { get; set; }                       // seconds
        public Matrix<double> ModeShapeMatrix { get; set; }         // Mass-normalized
        public Matrix<double> ModeShapeTransformationMatrix { get; set; } // ΦᵀM
        public Matrix<double> ModalMass { get; set; }
        public Matrix<double> ModalStiffness { get; set; }
        public Matrix<double> ModalDamping { get; set; }
        public double[] ModalDampingRatios { get; set; }

        public string PhaseName { get; set; }
        public int DegreesOfFreedom { get; set; }

        public override string ToString()
        {
            string result = $"=== {PhaseName} Modal Properties (DOF={DegreesOfFreedom}) ===\n";
            result += $"Natural Frequencies:\n";

            for (int i = 0; i < AngularNaturalFrequencies.Length; i++)
            {
                result += $"  Mode {i + 1}: {NaturalFrequenciesHz[i]:F2} Hz " +
                         $"(ω = {AngularNaturalFrequencies[i]:F2} rad/s, " +
                         $"T = {Periods[i]:F4} s, ζ = {ModalDampingRatios[i]:F4})\n";
            }

            result += $"\nMode Shapes (columns):\n";
            for (int i = 0; i < ModeShapeMatrix.RowCount; i++)
            {
                for (int j = 0; j < ModeShapeMatrix.ColumnCount; j++)
                {
                    result += $"{ModeShapeMatrix[i, j]:F4} ";
                }
                result += "\n";
            }

            return result;
        }
    }


    // The system is:

    //                            _________              _________ 
    //                     k1    |         |     k2     |         |
    //               --/\/\/\/\--|    m1   |--/\/\/\/\--|    m2   |    <---- a0
    //                           |         |            |         |  
    //                            ---------              --------- 
    //                                 <- v1                   <- v2
    //  ------> +ive direction       


    public class sdof2d_rigidcollisionSolver
    {


        private double mass_m1;
        private double stiffness_k1;


        private double mass_m2;
        private double stiffness_k2;

        private double dampratio_zeta;
        private double const_accla0;


        // Modal Properties for each phase
        private ModalProperties _flightModalProperties;   // 2 DOF - both masses attached to each other through spring k2 but spring k1 is free (deactivated)
        private ModalProperties _contactModalProperties;  // 2 DOF - The system is fully connected with both springs k1 and k2 active


        // Results storage for time history
        public List<double> TimeHistory { get; private set; } = new List<double>();
        public List<double> ContactForce { get; private set; } = new List<double>();

        public List<sdof2d_rigidcollisionResponse> node1Response { get; private set; } = new List<sdof2d_rigidcollisionResponse>();

        public List<sdof2d_rigidcollisionResponse> node2Response { get; private set; } = new List<sdof2d_rigidcollisionResponse>();



        public sdof2d_rigidcollisionSolver(double mass_m1, double stiffness_k1,
            double mass_m2, double stiffness_k2,
            double dampratio_zeta, double const_accla0)
        {
            this.mass_m1 = mass_m1;
            this.stiffness_k1 = stiffness_k1;

            this.mass_m2 = mass_m2;
            this.stiffness_k2 = stiffness_k2;

            this.dampratio_zeta = dampratio_zeta;
            this.const_accla0 = const_accla0;

            // Build matrices for each phase
            _flightModalProperties = SolveFlightPhase();
            _contactModalProperties = SolveContactPhase();

        }


        // =====================================================================
        // FLIGHT PHASE: 2 DOF System (the system is in flight)
        // Both masses are connected through spring k2, but spring k1 is inactive
        // =====================================================================
        private ModalProperties SolveFlightPhase()
        {
            // Mass matrix (2x2)
            Matrix<double> M = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { mass_m1, 0.0 },
                { 0.0, mass_m2 }
            });

            // Stiffness matrix (2x2) - Spring K1 is INACTIVE
            // Only spring K2 connects the two masses
            Matrix<double> K = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {  stiffness_k2, -stiffness_k2 },
                { -stiffness_k2, stiffness_k2 }
            });

            // Solve eigenproblem
            ModalProperties modalProps = SolveGeneralizedEigenproblem(M, K, "Flight", 2);


            // Flight phase has a rigid body mode (ω = 0) because there's no spring to ground
            // Set the first mode frequency to 0 (rigid body mode)
            if (modalProps.AngularNaturalFrequencies.Length > 0)
            {
                modalProps.AngularNaturalFrequencies[0] = 0.0;
                modalProps.NaturalFrequenciesHz[0] = 0.0;
                modalProps.Periods[0] = double.PositiveInfinity;
                modalProps.ModalDampingRatios[0] = 0.0;
            }


            // Calculate damping
            CalculateDamping(modalProps, M, K, new double[] { 0.0, dampratio_zeta });

            return modalProps;
        }

        // =====================================================================
        // CONTACT PHASE: 2 DOF System (mass 1 contact at wall through spring k1)
        // Both masses move and acts as simple 2 DOF system with both masses
        // connected through spring k2, spring k1
        // =====================================================================
        private ModalProperties SolveContactPhase()
        {
            // The system is attached to rigid wall using spring k1
            // Acts as simple 2 DOF system with both masses connected through spring k2, spring k1


            // Mass matrix (2x2)
            Matrix<double> M = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { mass_m1, 0.0 },
                { 0.0, mass_m2 }
            });

            // Stiffness matrix (2x2) - both springs active
            Matrix<double> K = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { stiffness_k1 + stiffness_k2, -stiffness_k2 },
                { -stiffness_k2, stiffness_k2 }
            });

            // Solve eigenproblem (1 DOF)
            ModalProperties modalProps = SolveGeneralizedEigenproblem(M, K, "Contact", 1);

            // Calculate damping
            CalculateDamping(modalProps, M, K, new double[] { dampratio_zeta, dampratio_zeta });

            return modalProps;
        }


        // =====================================================================
        // EIGENPROBLEM SOLVER
        // =====================================================================
        private ModalProperties SolveGeneralizedEigenproblem(Matrix<double> M,
                                                            Matrix<double> K,
                                                            string phaseName,
                                                            int dof)
        {
            try
            {
                // Solve generalized eigenvalue problem: K * φ = ω² * M * φ
                // Eigenvalues and eigenvectors
                Matrix<double> Z_matrix = M.Inverse() * K;
                Evd<double> eigen = Z_matrix.Evd();

                var eigenvalues = eigen.EigenValues;
                var eigenvectors = eigen.EigenVectors;

                int n = eigenvalues.Count;

                // Extract real eigenvalues
                double[] omegaSquared = new double[n];
                double[] omega = new double[n];

                for (int i = 0; i < n; i++)
                {
                    omegaSquared[i] = eigenvalues[i].Real;
                    omega[i] = Math.Sqrt(Math.Max(omegaSquared[i], 0));
                }

                // Sort by frequency (ascending)
                var sortedIndices = Enumerable.Range(0, n)
                    .OrderBy(i => omega[i])
                    .ToArray();

                // Reorder
                double[] sortedOmega = new double[n];
                double[] sortedOmegaSquared = new double[n];
                Matrix<double> sortedEigenvectors = Matrix<double>.Build.Dense(n, n);

                for (int i = 0; i < n; i++)
                {
                    int idx = sortedIndices[i];
                    sortedOmega[i] = omega[idx];
                    sortedOmegaSquared[i] = omegaSquared[idx];

                    for (int j = 0; j < n; j++)
                    {
                        sortedEigenvectors[j, i] = eigenvectors[j, idx];
                    }
                }

                // Mass-normalize eigenvectors
                Matrix<double> massNormalizedVectors = Matrix<double>.Build.Dense(n, n);
                double[] modalMasses = new double[n];

                for (int i = 0; i < n; i++)
                {
                    var phi = sortedEigenvectors.Column(i);
                    double modalMass = phi.DotProduct(M * phi);
                    modalMasses[i] = modalMass;

                    double normFactor = Math.Sqrt(Math.Max(modalMass, 1e-12));
                    for (int j = 0; j < n; j++)
                    {
                        massNormalizedVectors[j, i] = sortedEigenvectors[j, i] / normFactor;
                    }
                }

                Matrix<double> modeShapeMatrix = massNormalizedVectors;

                // Calculate modal properties
                Matrix<double> modalMassMatrix = modeShapeMatrix.Transpose() * M * modeShapeMatrix;
                Matrix<double> modalStiffnessMatrix = modeShapeMatrix.Transpose() * K * modeShapeMatrix;

                ModalProperties modalProps = new ModalProperties
                {
                    PhaseName = phaseName,
                    DegreesOfFreedom = dof,
                    MassMatrix = M,
                    StiffnessMatrix = K,
                    AngularNaturalFrequencies = sortedOmega,
                    NaturalFrequenciesHz = sortedOmega.Select(w => w / (2 * Math.PI)).ToArray(),
                    Periods = sortedOmega.Select(w => w > 1e-12 ? 2 * Math.PI / w : double.PositiveInfinity).ToArray(),
                    ModeShapeMatrix = massNormalizedVectors,
                    ModeShapeTransformationMatrix = modeShapeMatrix.Transpose() * M,
                    ModalMass = modalMassMatrix,
                    ModalStiffness = modalStiffnessMatrix
                };

                return modalProps;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error solving eigenproblem for {phaseName}: {ex.Message}");
                throw;
            }
        }

        // =====================================================================
        // DAMPING CALCULATION
        // =====================================================================
        private void CalculateDamping(ModalProperties modalProps,
                                     Matrix<double> M,
                                     Matrix<double> K,
                                     double[] modalDampingRatios)
        {
            int n = modalProps.AngularNaturalFrequencies.Length;
            double[] dampingRatios = new double[n];
            Matrix<double> dampingMatrix = Matrix<double>.Build.Dense(n, n);

            // For each mode, set the damping ratio individually
            for (int i = 0; i < n; i++)
            {
                dampingRatios[i] = (i < modalDampingRatios.Length) ? modalDampingRatios[i] : 0.05;
            }

            // Rayleigh damping: C = α*M + β*K
            // We can fit this to match the desired modal damping ratios
            if (n >= 2)
            {
                double ω1 = modalProps.AngularNaturalFrequencies[0];
                double ω2 = modalProps.AngularNaturalFrequencies[1];
                double ζ1 = dampingRatios[0];
                double ζ2 = dampingRatios[1];

                // Solve for α and β from the desired modal damping ratios
                // ζ_i = (α / (2ω_i)) + (βω_i / 2)
                if (ω2 > ω1 && ω1 > 1e-12)
                {
                    double alpha = 2 * (ζ1 * ω1 * ω2 * ω2 - ζ2 * ω1 * ω1 * ω2) / (ω2 * ω2 - ω1 * ω1);
                    double beta = 2 * (ζ2 * ω2 - ζ1 * ω1) / (ω2 * ω2 - ω1 * ω1);

                    dampingMatrix = alpha * M + beta * K;
                }
                else
                {
                    // If we can't fit Rayleigh damping, use mass-proportional
                    dampingMatrix = 2 * dampingRatios[0] * M;
                }
            }
            else if (n == 1)
            {
                // 1 DOF system
                double ω = modalProps.AngularNaturalFrequencies[0];
                if (ω > 1e-12)
                {
                    dampingMatrix[0, 0] = 2 * dampingRatios[0] * ω * M[0, 0];
                }
            }

            // Calculate actual modal damping from the damping matrix
            var modalDampingMatrix = modalProps.ModeShapeMatrix.Transpose() * dampingMatrix * modalProps.ModeShapeMatrix;

            for (int i = 0; i < n; i++)
            {
                double ω_i = modalProps.AngularNaturalFrequencies[i];
                if (ω_i > 1e-12)
                {
                    modalDampingMatrix[i, i] = Math.Min(modalDampingMatrix[i, i], ω_i); // Clamp for stability
                }
            }

            modalProps.DampingMatrix = dampingMatrix;
            modalProps.ModalDamping = modalDampingMatrix;
            modalProps.ModalDampingRatios = dampingRatios;

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



        // =====================================================================
        // RESPONSE CALCULATION
        // =====================================================================

        /// <summary>
        /// Get modal response at time t
        /// </summary>
        public (Vector<double> q, Vector<double> q_dot, Vector<double> q_ddot)
            GetModalResponse(double t, Vector<double> q0, Vector<double> q0_dot,
                            ModalProperties modalProps)
        {
            int n = q0.Count;
            var q = Vector<double>.Build.Dense(n);
            var q_dot = Vector<double>.Build.Dense(n);
            var q_ddot = Vector<double>.Build.Dense(n);

            // External force in modal coordinates
            var Fext = Vector<double>.Build.Dense(new double[] {
                mass_m1 * const_accla0,
                mass_m2 * const_accla0
            });

            // Transform force to modal coordinates: P_modal = Φᵀ * F_ext
            var P_modal = modalProps.ModeShapeMatrix.Transpose() * Fext;

            for (int i = 0; i < n; i++)
            {
                double ω = modalProps.AngularNaturalFrequencies[i];
                double ζ = modalProps.ModalDampingRatios[i];
                double Mm = modalProps.ModalMass[i, i];
                double Km = modalProps.ModalStiffness[i, i];

                // Handle rigid body mode (ω = 0)
                if (ω < 1e-12)
                {
                    // For rigid body mode, the effective acceleration is the modal force / modal mass
                    double acc_m = P_modal[i] / Mm;

                    q[i] = q0[i] + (q0_dot[i] * t) + (0.5 * acc_m * t * t);
                    q_dot[i] = q0_dot[i] + (acc_m * t);
                    q_ddot[i] = acc_m;
                    continue;
                }

                // For elastic modes, use the SDOF solver with the effective modal acceleration
                // The SDOF solver expects acceleration as input, so we convert the modal force
                double effective_acceleration = P_modal[i] / Mm;


                (q[i], q_dot[i], q_ddot[i]) = GetSDOFResponse(
                                    t,           // time
                                    Mm,          // modal mass
                                    Km,          // modal stiffness
                                    ζ,           // damping ratio
                                    q0[i],       // initial displacement in modal coordinates
                                    q0_dot[i],   // initial velocity in modal coordinates
                                    effective_acceleration  // EFFECTIVE modal acceleration!
                                );


            }

            return (q, q_dot, q_ddot);
        }



        public void solve_sdof2_rigidcollision(double total_simulation_time, double max_time_increment,
            double u1_inl, double u2_inl, double v1_inl, double v2_inl)
        {

            // Physical initial conditions
            Vector<double> physical_u0 = Vector<double>.Build.Dense(new double[] { u1_inl, u2_inl });
            Vector<double> physical_v0 = Vector<double>.Build.Dense(new double[] { v1_inl, v2_inl });

            double time_t = 0.0;
            double t_event = 0.0;
            double t_tau = 0.0;

            // Initialize the event tracker
            bool IsContact = false;
            double contact_force = 0.0;



            while (time_t < total_simulation_time)
            {
                // Time increment for the next iteration
                time_t += max_time_increment;

                if (time_t > total_simulation_time)
                {
                    time_t = total_simulation_time;
                }

                // Event span
                t_tau = time_t - t_event;


                if (!IsContact)
                {
                    // Flight phase


                }
                else
                {
                    // Contact phase
 
                }

                // store the time



                // Transition check: Determine if the system transitions from flight to contact or vice versa


                if (contact_force > 0.0 && IsContact == true)
                {
                    IsContact = false;

               

                }
                else if (contact_force <= 0.0 && IsContact == false)
                {
                    IsContact = true;

                   
                }


                // Add the computed response to the list


            }


        }



        //

    }
}
