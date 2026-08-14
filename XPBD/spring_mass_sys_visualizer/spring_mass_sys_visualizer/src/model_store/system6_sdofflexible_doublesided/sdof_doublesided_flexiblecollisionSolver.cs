using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using spring_mass_sys_visualizer.src.model_store.system2_store_data;
using spring_mass_sys_visualizer.src.model_store.system3_mdof_data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system6_sdofflexible_doublesided
{
    public class sdof_doublesided_flexiblecollisionSolver
    {

        private double leftmass_m1;
        private double rightmass_m3;
        private double strikemass_m2;

        private double leftstiffness_k1;
        private double rightstiffness_k3;
        private double strikestiffness_k2;

        private double dampratio_zeta;
        

        private ModalProperties _leftcontactModalProperties;  // 2 DOF - The system is fully connected with both springs k2 and k1 active

        private ModalProperties _rightcontactModalProperties;  // 2 DOF - The system is fully connected with both springs k2 and k3 active


        public multidof1d_rigidcollisionSolverResult SimulationResults { get; private set; }
        private double total_time;

        private double leftcontact_damping; // Damping coefficient for contact phase (Rayleigh damping)
        private double rightcontact_damping; // Damping coefficient for contact phase (Rayleigh damping)

        private double leftcontact_stiffness; // Effective stiffness for contact phase (Rayleigh damping)
        private double rightcontact_stiffness; // Effective stiffness for contact phase (Rayleigh damping)

        public sdof_doublesided_flexiblecollisionSolver(double _leftmass_m1, double _strikemass_m2, 
            double _rightmass_m3, double _leftstiffness_k1, double _strikestiffness_k2, double _rightstiffness_k3, 
            double _zeta)
        {

            // Mass data
            leftmass_m1 = _leftmass_m1;
            strikemass_m2 = _strikemass_m2;
            rightmass_m3 = _rightmass_m3;

            // Stiffness data
            leftstiffness_k1 = _leftstiffness_k1;
            strikestiffness_k2 = _strikestiffness_k2;
            rightstiffness_k3 = _rightstiffness_k3;

            // Global damping ratio for the system (used for Rayleigh damping)
            dampratio_zeta = _zeta;


            this.SimulationResults = new multidof1d_rigidcollisionSolverResult(3); // 3 nodes: m1, m2, m3


            _leftcontactModalProperties = SolveLeftContactPhase();  // Solve the contact phase eigenproblem upon initialization
            _rightcontactModalProperties = SolveRightContactPhase();  // Solve the contact phase eigenproblem upon initialization


        }


        // =====================================================================
        // CONTACT PHASE: 2 DOF System (mass 1 contact at wall through spring k1)
        // Both masses move and acts as simple 2 DOF system with both masses
        // connected through spring k2, spring k1
        // =====================================================================
        private ModalProperties SolveLeftContactPhase()
        {
            // The system is attached to rigid wall using spring k1
            // Acts as simple 2 DOF system with both masses connected through spring k2, spring k1


            // Mass matrix (2x2)
            Matrix<double> M = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { leftmass_m1, 0.0 },
                { 0.0, strikemass_m2 }
            });

            // Stiffness matrix (2x2) - both springs active
            Matrix<double> K = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { leftstiffness_k1 + strikestiffness_k2, -strikestiffness_k2 },
                { -strikestiffness_k2, strikestiffness_k2 }
            });

            // Solve eigenproblem (2 DOF)
            ModalProperties modalProps = SolveGeneralizedEigenproblem(M, K, "Contact", 2);

            // Calculate damping
            this.leftcontact_damping =   CalculateDamping(modalProps, M, K, new double[] { dampratio_zeta, dampratio_zeta });
            this.leftcontact_stiffness = strikestiffness_k2; // Effective stiffness for contact phase (Rayleigh damping)

            return modalProps;
        }


        private ModalProperties SolveRightContactPhase()
        {
            // The system is attached to rigid wall using spring k3
            // Acts as simple 2 DOF system with both masses connected through spring k2, spring k3


            // Mass matrix (2x2)
            Matrix<double> M = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { strikemass_m2, 0.0 },
                { 0.0, rightmass_m3 }
            });

            // Stiffness matrix (2x2) - both springs active
            Matrix<double> K = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { strikestiffness_k2, -strikestiffness_k2 },
                { -strikestiffness_k2, strikestiffness_k2 + rightstiffness_k3 }
            });

            // Solve eigenproblem (2 DOF)
            ModalProperties modalProps = SolveGeneralizedEigenproblem(M, K, "Contact", 2);

            // Calculate damping
            this.rightcontact_damping = CalculateDamping(modalProps, M, K, new double[] { dampratio_zeta, dampratio_zeta });
            this.rightcontact_stiffness = strikestiffness_k2; // Effective stiffness for contact phase (Rayleigh damping)

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
        private double CalculateDamping(ModalProperties modalProps,
                                     Matrix<double> M,
                                     Matrix<double> K,
                                     double[] modalDampingRatios)
        {
            int n = modalProps.AngularNaturalFrequencies.Length;
            double[] dampingRatios = new double[n];
            Matrix<double> dampingMatrix = Matrix<double>.Build.Dense(n, n);

            double contactDamping = 0.0;

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
                    contactDamping = beta * strikestiffness_k2; // Damping coefficient for mass 2 in contact phase
                }
                else
                {
                    // If we can't fit Rayleigh damping, use mass-proportional
                    dampingMatrix = 2 * dampingRatios[0] * M;

                    contactDamping = 2 * dampingRatios[0] * Math.Sqrt(strikestiffness_k2 * strikemass_m2); // Damping coefficient for mass 2 in contact phase
                }
            }
            else if (n == 1)
            {
                // 1 DOF system
                double ω = modalProps.AngularNaturalFrequencies[0];
                if (ω > 1e-12)
                {
                    dampingMatrix[0, 0] = 2 * dampingRatios[0] * ω * M[0, 0];

                    contactDamping = 2.0 * dampingRatios[0] * Math.Sqrt(strikestiffness_k2 * strikemass_m2); // Damping coefficient for mass 2 in contact phase
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

            return contactDamping;
        }




        private (double u, double v, double a) GetSDOFResponse(double time_t, double mass_m, double stiff_k,
            double zeta, double u_inl, double v_inl, double const_a0)
        {

            if (stiff_k < 1E-8)
            {
                // Rigid body mode (no stiffness)
                double u = u_inl + (v_inl * time_t) + (0.5 * const_a0 * time_t * time_t);
                double v = v_inl + (const_a0 * time_t);
                double a = const_a0;

                return (u, v, a);
            }


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



        private (Vector<double> u, Vector<double> v, Vector<double> a)
            GetFlightResponse(double t, Vector<double> u_at_event, Vector<double> v_at_event)
        {

            double const_accla0 = 0.0; // No acceleration in flight phase

            // Left mass m1 : SDOF system (m2 and k2 are in flight)
            (double u1, double v1, double a1) = GetSDOFResponse(t, leftmass_m1, leftstiffness_k1, dampratio_zeta,
                u_at_event[0], v_at_event[0], const_accla0);

            // strike mass m2 : Kinetic response (free flight, no spring)
            double u2 = u_at_event[1] + (v_at_event[1] * t) + (0.5 * const_accla0 * t * t);
            double v2 = v_at_event[1] + (const_accla0 * t);
            double a2 = const_accla0;


            // Right mass m3 : SDOF system (m2 and k2 are in flight)
            (double u3, double v3, double a3) = GetSDOFResponse(t, rightmass_m3, rightstiffness_k3, dampratio_zeta,
                u_at_event[2], v_at_event[2], const_accla0);


            return (Vector<double>.Build.Dense(new double[] { u1, u2, u3 }),
                    Vector<double>.Build.Dense(new double[] { v1, v2, v3 }),
                    Vector<double>.Build.Dense(new double[] { a1, a2, a3 }));

        }



        private (Vector<double> u, Vector<double> v, Vector<double> a)
            GetLeftContactResponse(double t, Vector<double> u_at_event, Vector<double> v_at_event)
        {

            // Split the displacement and velocity vectors into the left contact phase (2 DOF) and right mass (1 DOF)
            Vector<double> u_left = Vector<double>.Build.Dense(new double[] { u_at_event[0], u_at_event[1] });
            Vector<double> v_left = Vector<double>.Build.Dense(new double[] { v_at_event[0], v_at_event[1] });

            double u_right = u_at_event[2];
            double v_right = v_at_event[2];


            ModalProperties modalProps = _leftcontactModalProperties;

            // Transform physical to modal coordinates using Φᵀ * M * u
            // For mass-normalized eigenvectors: q = Φ⁻¹ * u
            Vector<double> q0 = modalProps.ModeShapeInverseMatrix * u_left;
            Vector<double> q0_dot = modalProps.ModeShapeInverseMatrix * v_left;

            double const_accla0 = 0.0; // No acceleration 

            // External force in modal coordinates
            Vector<double> Fext = Vector<double>.Build.Dense(new double[] {
                leftmass_m1 * const_accla0,
                strikemass_m2 * const_accla0
            });


            // Transform force to modal coordinates: P_modal = Φᵀ * F_ext
            Vector<double> P_modal = modalProps.ModeShapeMatrix.Transpose() * Fext;


            int n = q0.Count;
            Vector<double> q = Vector<double>.Build.Dense(n);
            Vector<double> q_dot = Vector<double>.Build.Dense(n);
            Vector<double> q_ddot = Vector<double>.Build.Dense(n);


            for (int i = 0; i < n; i++)
            {
                double ω = modalProps.AngularNaturalFrequencies[i];
                double ζ = modalProps.ModalDampingRatios[i];
                double Mm = modalProps.ModalMass[i, i]; // Should be 1.0 for mass-normalized
                double Km = modalProps.ModalStiffness[i, i]; // Should be ω²

                // Handle rigid body mode (ω = 0)
                if (ω < 1e-8)
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

            // Transform back to physical coordinates
            Vector<double> uleft_at_t = modalProps.ModeShapeMatrix * q;
            Vector<double> vleft_at_t = modalProps.ModeShapeMatrix * q_dot;
            Vector<double> aleft_at_t = modalProps.ModeShapeMatrix * q_ddot;


            // Find the response of the right mass (1 DOF) in flight phase
            (double uright_at_t, double vright_at_t, double aright_at_t) = GetSDOFResponse(t, rightmass_m3, rightstiffness_k3, dampratio_zeta,
                u_right, v_right, const_accla0);


            // Concatenate the left and right responses to form the full system response
            Vector<double> u_at_t = Vector<double>.Build.Dense(new double[] { uleft_at_t[0], uleft_at_t[1], uright_at_t });
            Vector<double> v_at_t = Vector<double>.Build.Dense(new double[] { vleft_at_t[0], vleft_at_t[1], vright_at_t });
            Vector<double> a_at_t = Vector<double>.Build.Dense(new double[] { aleft_at_t[0], aleft_at_t[1], aright_at_t });


            return (u_at_t, v_at_t, a_at_t);
        }




        private (Vector<double> u, Vector<double> v, Vector<double> a)
            GetRightContactResponse(double t, Vector<double> u_at_event, Vector<double> v_at_event)
        {

            // Split the displacement and velocity vectors into the left mass (1 DOF) and right contact phase (2 DOF)
            double u_left = u_at_event[0];
            double v_left = v_at_event[0];

            Vector<double> u_right = Vector<double>.Build.Dense(new double[] { u_at_event[1], u_at_event[2] });
            Vector<double> v_right = Vector<double>.Build.Dense(new double[] { v_at_event[1], v_at_event[2] });


            ModalProperties modalProps = _rightcontactModalProperties;

            // Transform physical to modal coordinates using Φᵀ * M * u
            // For mass-normalized eigenvectors: q = Φ⁻¹ * u
            Vector<double> q0 = modalProps.ModeShapeInverseMatrix * u_right;
            Vector<double> q0_dot = modalProps.ModeShapeInverseMatrix * v_right;

            double const_accla0 = 0.0; // No acceleration 

            // External force in modal coordinates
            Vector<double> Fext = Vector<double>.Build.Dense(new double[] {
                strikemass_m2 * const_accla0,
                rightmass_m3 * const_accla0
            });


            // Transform force to modal coordinates: P_modal = Φᵀ * F_ext
            Vector<double> P_modal = modalProps.ModeShapeMatrix.Transpose() * Fext;


            int n = q0.Count;
            Vector<double> q = Vector<double>.Build.Dense(n);
            Vector<double> q_dot = Vector<double>.Build.Dense(n);
            Vector<double> q_ddot = Vector<double>.Build.Dense(n);


            for (int i = 0; i < n; i++)
            {
                double ω = modalProps.AngularNaturalFrequencies[i];
                double ζ = modalProps.ModalDampingRatios[i];
                double Mm = modalProps.ModalMass[i, i]; // Should be 1.0 for mass-normalized
                double Km = modalProps.ModalStiffness[i, i]; // Should be ω²

                // Handle rigid body mode (ω = 0)
                if (ω < 1e-8)
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

            // Transform back to physical coordinates
            Vector<double> uright_at_t = modalProps.ModeShapeMatrix * q;
            Vector<double> vright_at_t = modalProps.ModeShapeMatrix * q_dot;
            Vector<double> aright_at_t = modalProps.ModeShapeMatrix * q_ddot;


            // Find the response of the left mass (1 DOF) in flight phase
            (double uleft_at_t, double vleft_at_t, double aleft_at_t) = GetSDOFResponse(t, leftmass_m1, leftstiffness_k1, dampratio_zeta,
                u_left, v_left, const_accla0);

            // Concatenate the left and right responses to form the full system response
            Vector<double> u_at_t = Vector<double>.Build.Dense(new double[] { uleft_at_t, uright_at_t[0], uright_at_t[1] });
            Vector<double> v_at_t = Vector<double>.Build.Dense(new double[] { vleft_at_t, vright_at_t[0], vright_at_t[1] });
            Vector<double> a_at_t = Vector<double>.Build.Dense(new double[] { aleft_at_t, aright_at_t[0], aright_at_t[1] });


            return (u_at_t, v_at_t, a_at_t);
        }



        private (double contact_force, double derivative_contact_force)
            GetLeftContactForce(Vector<double> u_at_t, Vector<double> v_at_t, Vector<double> a_at_t)
        {
            // Contact force at the left contact (between mass 2 and mass 1 attached to the wall)
            double delta_u = u_at_t[1] - u_at_t[0]; // Relative displacement between mass 2 and mass 1
            double delta_v = v_at_t[1] - v_at_t[0]; // Relative velocity between mass 2 and mass 1
            double delta_a = a_at_t[1] - a_at_t[0]; // Relative acceleration between mass 2 and mass 1

            // Contact force with respect to time
            double contact_force = (leftstiffness_k1 * delta_u) + (leftcontact_damping * delta_v);

            // Derivative of contact force with respect to time
            double derivative_contact_force = (leftstiffness_k1 * delta_v) + (leftcontact_damping * delta_a);

            return (contact_force, derivative_contact_force);
        }


        private (double contact_force, double derivative_contact_force)
            GetRightContactForce(Vector<double> u_at_t, Vector<double> v_at_t, Vector<double> a_at_t)
        {
            // Contact force at the right contact (between mass 2 and mass 3 attached to the wall)
            double delta_u = u_at_t[1] - u_at_t[2]; // Relative displacement between mass 2 and mass 3
            double delta_v = v_at_t[1] - v_at_t[2]; // Relative velocity between mass 2 and mass 3
            double delta_a = a_at_t[1] - a_at_t[2]; // Relative acceleration between mass 2 and mass 3

            // Contact force with respect to time
            double contact_force = (rightstiffness_k3 * delta_u) + (rightcontact_damping * delta_v);

            // Derivative of contact force with respect to time
            double derivative_contact_force = (rightstiffness_k3 * delta_v) + (rightcontact_damping * delta_a);

            return (contact_force, derivative_contact_force);
        }





        // Define the response function type
        private delegate (Vector<double> u, Vector<double> v, Vector<double> a)
            ResponseFunction(double tau, Vector<double> u_start, Vector<double> v_start);


        // Define the contact force function type
        private delegate (double contact_force, double derivative_contact_force)
            ContactForceFunction(Vector<double> u_tau, Vector<double> v_tau, Vector<double> a_tau);



        private (double t_exact, Vector<double> u, Vector<double> v, Vector<double> a)
          DetectPhaseTransition(double t_end, Vector<double> u_start, Vector<double> v_start,
            ResponseFunction getResponse,
            ContactForceFunction getContactForce)
        {

            double tau_low = 0.0;
            double tau_high = t_end;

            // Start with bisection to bracket the root
            for (int i = 0; i < 5; ++i)
            {

                double tau_mid = 0.5 * (tau_low + tau_high);

                (Vector<double> u_mid, Vector<double> v_mid, Vector<double> a_mid) = getResponse(tau_mid, u_start, v_start);
                (Vector<double> u_low, Vector<double> v_low, Vector<double> a_low) = getResponse(tau_low, u_start, v_start);

                (double contact_force_mid, _) = getContactForce(u_mid, v_mid, a_mid);
                (double contact_force_low, _) = getContactForce(u_low, v_low, a_low);

                if ((contact_force_mid * contact_force_low) < 0.0)
                {
                    tau_high = tau_mid;
                }
                else
                {
                    tau_low = tau_mid;
                }
            }


            // Switch to Newton-Raphson for refinement
            double tau = 0.5 * (tau_low + tau_high);
            Vector<double> u_tau = null, v_tau = null, a_tau = null;

            for (int i = 0; i < 20; ++i)
            {
                (u_tau, v_tau, a_tau) = getResponse(tau, u_start, v_start);
                (double contact_force_tau, double derivative_contact_force_tau) = getContactForce(u_tau, v_tau, a_tau);

                if (Math.Abs(contact_force_tau) < 1e-10)
                {
                    return (tau, u_tau, v_tau, a_tau);
                }

                tau = tau - (contact_force_tau / derivative_contact_force_tau);

                // Keep within bracket
                tau = Math.Max(tau_low, Math.Min(tau_high, tau));
            }

            return (tau, u_tau, v_tau, a_tau);

        }




        public void solve_sdof_collision_with_flexible_boundary(double total_simulation_time, double max_time_increment,
    double strikemass_initial_velocity, double total_width)
        {
            this.total_time = total_simulation_time;

            double left_width = -0.5 * total_width;
            double right_width = 0.5 * total_width;

            // Clear previous results
            SimulationResults.ClearData();


            // Physical initial conditions 
            // Left end --- strike mass --- Right end
            Vector<double> u_at_event = Vector<double>.Build.Dense(new double[] { left_width, 0.0, right_width });
            Vector<double> v_at_event = Vector<double>.Build.Dense(new double[] { 0.0,  strikemass_initial_velocity, 0.0 });

            Vector<double> u_at_t = u_at_event.Clone();
            Vector<double> v_at_t = v_at_event.Clone();
            Vector<double> a_at_t = Vector<double>.Build.Dense(3);

            double time_t = 0.0;
            double t_event = 0.0;


            // Set initial phase
            // No contact at the start, so we are in flight phase
            bool LeftContact = false;
            bool RightContact = false;

            (double left_contact_force, _) = GetLeftContactForce(u_at_t, v_at_t, a_at_t);
            (double right_contact_force, _) = GetRightContactForce(u_at_t, v_at_t, a_at_t);

            double contact_force_at_t = Math.Min(left_contact_force, right_contact_force);

            // Add the first increment to the Response lists 
            SimulationResults.AddResponse(time_t, u_at_t, v_at_t, a_at_t, contact_force_at_t);


            // Main simulation loop
            while (time_t < total_simulation_time)
            {
                // Time increment for the next iteration
                time_t += max_time_increment;

                if (time_t > total_simulation_time)
                {
                    time_t = total_simulation_time;
                }

                // Event span
                double t_tau = time_t - t_event;

                if(LeftContact)
                {
                    // Left Contact phase
                    (u_at_t, v_at_t, a_at_t) = GetLeftContactResponse(t_tau, u_at_event, v_at_event);
                }
                else if(RightContact)
                {
                    // Right Contact phase
                    (u_at_t, v_at_t, a_at_t) = GetRightContactResponse(t_tau, u_at_event, v_at_event);
                }
                else
                {
                    // Flight phase
                    (u_at_t, v_at_t, a_at_t) = GetFlightResponse(t_tau, u_at_event, v_at_event);
                }


                // Calculate the contact force at the current time step
                (left_contact_force, _) = GetLeftContactForce(u_at_t, v_at_t, a_at_t);
                (right_contact_force, _) = GetRightContactForce(u_at_t, v_at_t, a_at_t);

                bool isTransition = false;
                double tau_exact = 0.0;

                if (LeftContact)
                {
                    // Transition check: Determine if the system transitions from left contact to flight phase
                    if(left_contact_force > 0.0)
                    {
                        // Contact is broken, transition to flight phase
                        LeftContact = false;

                        // Get previous state
                        (Vector<double> u_prev, Vector<double> v_prev, _) =
                            SimulationResults.GetStateAtTimeIndex(SimulationResults.TimePoints.Count - 1);


                        (tau_exact, u_at_t, v_at_t, a_at_t) = DetectPhaseTransition(max_time_increment,
                            u_prev, v_prev,
                            GetLeftContactResponse,
                            GetLeftContactForce);

                        isTransition = true;

                    }

                }
                else if(RightContact)
                {
                    // Transition check: Determine if the system transitions from right contact to flight phase
                    if (right_contact_force > 0.0)
                    {
                        // Contact is broken, transition to flight phase
                        RightContact = false;

                        // Get previous state
                        (Vector<double> u_prev, Vector<double> v_prev, _) =
                            SimulationResults.GetStateAtTimeIndex(SimulationResults.TimePoints.Count - 1);


                        (tau_exact, u_at_t, v_at_t, a_at_t) = DetectPhaseTransition(max_time_increment,
                            u_prev, v_prev,
                            GetRightContactResponse,
                            GetRightContactForce);

                        isTransition = true;

                    }

                }
                else
                {
                    if( left_contact_force < 0.0 )
                    {
                        // Contact is made with the left boundary, transition to left contact phase
                        LeftContact = true;

                        // Get previous state
                        (Vector<double> u_prev, Vector<double> v_prev, _) =
                            SimulationResults.GetStateAtTimeIndex(SimulationResults.TimePoints.Count - 1);


                        (tau_exact, u_at_t, v_at_t, a_at_t) = DetectPhaseTransition(max_time_increment,
                            u_prev, v_prev,
                            GetFlightResponse,
                            GetLeftContactForce);

                        isTransition = true;

                    }
                    else if (right_contact_force < 0.0)
                    {
                        // Contact is made with the right boundary, transition to right contact phase
                        RightContact = true;


                        // Get previous state
                        (Vector<double> u_prev, Vector<double> v_prev, _) =
                            SimulationResults.GetStateAtTimeIndex(SimulationResults.TimePoints.Count - 1);


                        (tau_exact, u_at_t, v_at_t, a_at_t) = DetectPhaseTransition(max_time_increment,
                            u_prev, v_prev,
                            GetFlightResponse,
                            GetRightContactForce);

                        isTransition = true;

                    }
                }


                // If a transition occurred, adjust the time and state accordingly
                if (isTransition == true)
                {

                    // Adjust the time and event time based on the exact transition time
                    time_t = (time_t - max_time_increment) + tau_exact;

                    // Update state to transition point
                    t_event = time_t;
                    u_at_event = u_at_t.Clone();
                    v_at_event = v_at_t.Clone();

                    SimulationResults.TimeContactBand.Add(time_t);

                }


                // Add the computed response to the list
                SimulationResults.AddResponse(time_t, u_at_t, v_at_t, a_at_t, contact_force_at_t);

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







    }
}
