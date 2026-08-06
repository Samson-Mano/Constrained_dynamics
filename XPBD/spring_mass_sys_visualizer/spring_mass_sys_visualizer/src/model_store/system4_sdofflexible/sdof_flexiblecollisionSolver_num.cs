using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using spring_mass_sys_visualizer.src.model_store.system2_store_data;
using spring_mass_sys_visualizer.src.model_store.system3_mdof_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system4_sdofflexible
{
    public class sdof_flexiblecollisionSolver_num
    {


        private double mass_m1;
        private double mass_m2;

        private double stiffness_k1;
        private double stiffness_k2;

        private double dampratio_zeta;
        private double const_accla0;


        public multidof1d_rigidcollisionSolverResult SimulationResults { get; private set; }
        private double total_time;

        private double Cc; // Damping coefficient for contact phase (Rayleigh damping)


        private Matrix<double> MassMatrixM;
        private Matrix<double> invMassMatrix;
        private Matrix<double> StiffMatrixK;
        private Matrix<double> dampingMatrixC;
        private Vector<double> Fext;

        public sdof_flexiblecollisionSolver_num(double m1, double m2, double k1, double k2, double zeta, double accla0)
        {
            mass_m1 = m1;
            mass_m2 = m2;
            stiffness_k1 = k1;
            stiffness_k2 = k2;
            dampratio_zeta = zeta;
            const_accla0 = accla0;


            this.SimulationResults = new multidof1d_rigidcollisionSolverResult(2);


           SolveContactPhase();  // Solve the contact phase eigenproblem upon initialization


        }


        // =====================================================================
        // CONTACT PHASE: 2 DOF System (mass 1 contact at wall through spring k1)
        // Both masses move and acts as simple 2 DOF system with both masses
        // connected through spring k2, spring k1
        // =====================================================================
        private void SolveContactPhase()
        {
            // The system is attached to rigid wall using spring k1
            // Acts as simple 2 DOF system with both masses connected through spring k2, spring k1


            // Mass matrix (2x2)
            MassMatrixM = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { mass_m1, 0.0 },
                { 0.0, mass_m2 }
            });

            // Inverse mass matrix (2x2)
            invMassMatrix = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { 1.0 / mass_m1, 0.0 },
                { 0.0, 1.0 / mass_m2 }
            });


            // Stiffness matrix (2x2) - both springs active
            StiffMatrixK = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { stiffness_k1 + stiffness_k2, -stiffness_k2 },
                { -stiffness_k2, stiffness_k2 }
            });



            // External force vector (2x1)
            Fext = Vector<double>.Build.Dense(2, 0.0);

            Fext = MassMatrixM * Vector<double>.Build.DenseOfArray(new double[] { const_accla0, const_accla0 });



            // Solve eigenproblem (2 DOF)
            ModalProperties modalProps = SolveGeneralizedEigenproblem(MassMatrixM, StiffMatrixK, "Contact", 2);

            // Calculate damping
            CalculateDamping(modalProps, MassMatrixM, StiffMatrixK, new double[] { dampratio_zeta, dampratio_zeta });


            // Copy the damping matrix to the class-level variable for use in the RHS function
            dampingMatrixC = modalProps.DampingMatrix;

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
                    ModeShapeInverseMatrix = modeShapeMatrix.Transpose() * M, // Φ⁻¹
                    ModalMass = modalMassMatrix,
                    ModalStiffness = modalStiffnessMatrix,
                    ModalDampingRatios = new double[n] // Placeholder, will be calculated later
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
                    double beta = 2 * ((ζ2 * ω2) - (ζ1 * ω1)) / ((ω2 * ω2) - (ω1 * ω1));

                    dampingMatrix = alpha * M + beta * K;

                    // Contact damping coefficient for the contact phase (Rayleigh damping)
                    this.Cc = beta * stiffness_k2; // Damping coefficient for mass 2 in contact phase
                }
                else
                {
                    // If we can't fit Rayleigh damping, use mass-proportional
                    dampingMatrix = 2 * dampingRatios[0] * M;

                    this.Cc = 2 * dampingRatios[0] * Math.Sqrt(stiffness_k2 * mass_m2); // Damping coefficient for mass 2 in contact phase
                }
            }
            else if (n == 1)
            {
                // 1 DOF system
                double ω = modalProps.AngularNaturalFrequencies[0];
                if (ω > 1e-12)
                {
                    dampingMatrix[0, 0] = 2 * dampingRatios[0] * ω * M[0, 0];

                    this.Cc = 2.0 * dampingRatios[0] * Math.Sqrt(stiffness_k2 * mass_m2); // Damping coefficient for mass 2 in contact phase
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



        // -----------------------------------------------------------------
        // Right hand side for the *flight* phase (no coupling)
        // -----------------------------------------------------------------
        private Vector<double> RHS_Flight(double t, Vector<double> y)
        {
            // y = [x1, v1, x2, v2]
            double x1 = y[0];
            double v1 = y[1];
            double x2 = y[2];
            double v2 = y[3];

            // --- boundary mass (m1) ---
            double c1 = 2.0 * dampratio_zeta * Math.Sqrt(stiffness_k1 * mass_m1);
            double a1 = ((mass_m1 * const_accla0) - (c1 * v1) - (stiffness_k1 * x1)) / mass_m1;

            // --- free mass (m2) ---
            double a2 = const_accla0;

            return Vector<double>.Build.Dense(new double[] { v1, a1, v2, a2 });
        }


        // -----------------------------------------------------------------
        // Right hand side for the *contact* phase (both masses coupled)
        // -----------------------------------------------------------------
        private Vector<double> RHS_Contact(double t, Vector<double> y)
        {
            // y = [x1, v1, x2, v2]
            Vector<double> x = Vector<double>.Build.Dense(new double[] { y[0], y[2] });  // [x1, x2]
            Vector<double> v = Vector<double>.Build.Dense(new double[] { y[1], y[3] });  // [v1, v2]

            // Compute C*v
            Vector<double> Cv = dampingMatrixC * v;

            // Compute K*x
            Vector<double> Kx = StiffMatrixK * x;

            // rhs = Fext - C*v - K*x
            Vector<double> rhs = Fext - Cv - Kx;

            // Solve M * a = rhs  =>  a = M^(-1) * rhs
            Vector<double> a = invMassMatrix * rhs;

            return Vector<double>.Build.Dense(new double[] { v[0], a[0], v[1], a[1] });
        }


        // -----------------------------------------------------------------
        // RK4 Integration Step
        // -----------------------------------------------------------------
        private Vector<double> RK4Step(Func<double, Vector<double>, Vector<double>> rhs, double t, Vector<double> y, double h)
        {
            // k1 = rhs(t, y)
            Vector<double> k1 = rhs(t, y);

            // k2 = rhs(t + h/2, y + h*k1/2)
            Vector<double> y2 = y + k1 * (h / 2.0);
            Vector<double> k2 = rhs(t + h / 2.0, y2);

            // k3 = rhs(t + h/2, y + h*k2/2)
            Vector<double> y3 = y + k2 * (h / 2.0);
            Vector<double> k3 = rhs(t + h / 2.0, y3);

            // k4 = rhs(t + h, y + h*k3)
            Vector<double> y4 = y + k3 * h;
            Vector<double> k4 = rhs(t + h, y4);

            // y_new = y + (h/6) * (k1 + 2*k2 + 2*k3 + k4)
            Vector<double> result = y + (h / 6.0) * (k1 + 2.0 * k2 + 2.0 * k3 + k4);

            return result;
        }


        // -----------------------------------------------------------------
        // Get contact force
        // -----------------------------------------------------------------
        private double GetContactForce(Vector<double> y)
        {
            double x1 = y[0];
            double v1 = y[1];
            double x2 = y[2];
            double v2 = y[3];

            // Contact force: F_contact = k2 * (x2 - x1) + c_contact * (v2 - v1)
            // Use damping coefficient from contact phase
            double c_contact = this.Cc; // Beta * stiffness_k2; // Damping coefficient for contact phase

            return stiffness_k2 * (x2 - x1) + c_contact * (v2 - v1);
        }


        // -----------------------------------------------------------------
        // Detect phase transition
        // -----------------------------------------------------------------
        private bool DetectPhaseTransition(Vector<double> y, bool inContact)
        {
            double contactForce = GetContactForce(y);

            if (inContact)
            {
                // Transition to flight if contact force becomes tensile (positive)
                return contactForce > 1e-8;
            }
            else
            {
                // Transition to contact if force is compressive
                return contactForce < -1e-8;
            }
        }


        private (double, Vector<double>) BisectTransitionEvent(double t_left, Vector<double> y_left,
            double t_right, Vector<double> y_right, double tol = 1e-6, int maxIter = 50)
        {

            for (int iter = 0; iter < maxIter; iter++)
            {
                double t_mid = 0.5 * (t_left + t_right);

                // linear interpolation of the state (good enough for a tiny interval)
                Vector<double> y_mid = 0.5 * (y_left + y_right);

                // If the interval is already smaller than ``tol`` we stop.
                if ((t_right - t_left) < tol)
                {
                    return (t_mid, y_mid);
                }


                // Evaluate the event function at the midpoint
                double phi_mid = GetContactForce(y_mid);
                double phi_left = GetContactForce(y_left);

                // Decide which half interval contains the root
                if (phi_mid * phi_left <= 0.0)
                {
                    t_right = t_mid;
                    y_right = y_mid;
                }
                else
                {
                    t_left = t_mid;
                    y_left = y_mid;
                }

            }

            return (0.5 * (t_left + t_right), 0.5 * (y_left + y_right)); // Return midpoint if max iterations reached

        }



        public void solve_sdof_collision_with_flexible_boundary(double total_simulation_time, double max_time_increment,
double u1_inl, double u2_inl, double v1_inl, double v2_inl)
        {
            this.total_time = total_simulation_time;


            // Clear previous results
            SimulationResults.ClearData();



            double t_old = 0.0;
            double t_new = 0.0;
            double dt = max_time_increment;


            Vector<double> y_old = Vector<double>.Build.Dense(new double[] { u1_inl, v1_inl, u2_inl, v2_inl });

            // Initialize the phase: flight or contact
            double contact_force_at_t = GetContactForce(y_old);

            // Determine initial phase
            bool InContact = contact_force_at_t <= 0.0;


            if (InContact)
            {
                SimulationResults.TimeContactBand.Add(t_old);
            }


            // Main simulation loop
            while (t_old < total_simulation_time)
            {
                Func<double, Vector<double>, Vector<double>> rhs;

                if (InContact)
                {
                    // Contact phase
                    // Coupled response of the two SDOF systems
                    rhs = RHS_Contact;

                }
                else
                {
                    // Flight phase
                    // Uncoupled response of the two SDOF systems
                   rhs = RHS_Flight;

                }

                // Get the acceleration at the current state
                Vector<double> a_value = rhs(t_old, y_old);

                // Integrate *until the next event* or until t_final
                Vector<double> y_new = RK4Step(rhs, t_old, y_old, dt);


                // Get the contact force at the new state
                contact_force_at_t = GetContactForce(y_new);

                t_new = t_old + dt;


                // Check for phase transition
                if( DetectPhaseTransition(y_new, InContact))
                {
                    // Transition detected, perform bisection to find the exact transition time
                    InContact = !InContact;


                    // Bisection to find the transition time
                    (t_new, y_new) = BisectTransitionEvent(t_old, y_old, t_new, y_new);

                    // Get the contact force at the new state after bisection
                    contact_force_at_t = GetContactForce(y_new);

                    SimulationResults.TimeContactBand.Add(t_new);
                }

                // Update the state for the next iteration
                t_old = t_new;
                y_old = y_new;



                // Add the computed response to the list
                Vector<double> u_new = Vector<double>.Build.Dense(new double[] { y_new[0], y_new[2] });
                Vector<double> v_new = Vector<double>.Build.Dense(new double[] { y_new[1], y_new[3] });
                Vector<double> a_new = Vector<double>.Build.Dense(new double[] { a_value[1], a_value[3] });
                SimulationResults.AddResponse(t_new, u_new, v_new, a_new, contact_force_at_t);

            }

            // Ensure the last contact band time is recorded if the simulation ends in contact
            if ((SimulationResults.TimeContactBand.Count % 2) != 0)
            {
                SimulationResults.TimeContactBand.Add(total_simulation_time);
            }

        }




        public (List<double> Displacement, List<double> Velocity, List<double> Acceleration, double contact_force)
            getResult_at_timet(double time_t)
        {
            /// <summary>
            /// Retrieves the response at a specific time from the response list.
            /// </summary>

            if (time_t > total_time || time_t < 0.0)
            {
                // Reset the time to 0.0 if it exceeds the total simulation time
                time_t = 0.0;
            }


            // Find the two points to interpolate between
            int lowerIndex = 0;
            int upperIndex = SimulationResults.TimePoints.Count - 1;
            int midIndex;

            // Binary search to find the interval containing time_t
            while (upperIndex - lowerIndex > 1)
            {
                midIndex = (lowerIndex + upperIndex) / 2;

                if (SimulationResults.TimePoints[midIndex] <= time_t)
                    lowerIndex = midIndex;
                else
                    upperIndex = midIndex;
            }



            // Get the two bounding points
            var lowerTimePoint = SimulationResults.TimePoints[lowerIndex];
            var upperTimePoint = SimulationResults.TimePoints[upperIndex];

            // Calculate interpolation factor (0.0 to 1.0)
            double dt = upperTimePoint - lowerTimePoint;

            (Vector<double> lowerDisplacement, Vector<double> lowerVelocity, Vector<double> lowerAcceleration) =
                 SimulationResults.GetStateAtTimeIndex(lowerIndex);

            List<double> respDisplacement = new List<double>();
            List<double> respVelocity = new List<double>();
            List<double> respAcceleration = new List<double>();

            // Handle case where time difference is zero (shouldn't happen with proper data)
            if (dt < 1e-12)
            {

                respDisplacement = new List<double>(lowerDisplacement.ToArray());
                respVelocity = new List<double>(lowerVelocity.ToArray());
                respAcceleration = new List<double>(lowerAcceleration.ToArray());

                return (respDisplacement, respVelocity, respAcceleration, SimulationResults.ContactForce[lowerIndex]);
            }


            // Clamp interpolation factor to [0,1] for safety
            double param_t = (time_t - lowerTimePoint) / dt;
            param_t = Math.Max(0.0, Math.Min(1.0, param_t));

            (Vector<double> upperDisplacement, Vector<double> upperVelocity, Vector<double> upperAcceleration) =
                SimulationResults.GetStateAtTimeIndex(upperIndex);


            for (int i = 0; i < 2; i++)
            {
                // Linear interpolation for displacement, velocity, and acceleration
                double interpolatedDisplacement = lowerDisplacement[i] + (upperDisplacement[i] - lowerDisplacement[i]) * param_t;
                double interpolatedVelocity = lowerVelocity[i] + (upperVelocity[i] - lowerVelocity[i]) * param_t;
                double interpolatedAcceleration = lowerAcceleration[i] + (upperAcceleration[i] - lowerAcceleration[i]) * param_t;
                // Store the interpolated values in the response lists
                respDisplacement.Add(interpolatedDisplacement);
                respVelocity.Add(interpolatedVelocity);
                respAcceleration.Add(interpolatedAcceleration);
            }



            // Contact force interpolation
            double lowerContactForce = SimulationResults.ContactForce[lowerIndex];
            double upperContactForce = SimulationResults.ContactForce[upperIndex];
            double interpolatedContactForce = lowerContactForce + (upperContactForce - lowerContactForce) * param_t;


            return (respDisplacement, respVelocity, respAcceleration, interpolatedContactForce);
        }





        //

    }
}
