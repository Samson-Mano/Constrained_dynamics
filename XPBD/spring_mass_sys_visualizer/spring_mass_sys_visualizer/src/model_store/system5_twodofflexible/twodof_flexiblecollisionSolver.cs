using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using spring_mass_sys_visualizer.src.model_store.system2_store_data;
using spring_mass_sys_visualizer.src.model_store.system3_mdof_data;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system5_twodofflexible
{

    public class twodof_flexiblecollisionSolver
    {

        private class SystemMatrixData
        {

            private int nDOF;

            // Mass, Stiffness, and Damping matrices for the system
            private Matrix<double> MassMatrix { get; set; }
            private Matrix<double> StiffnessMatrix { get; set; }
            private Matrix<double> DampingMatrix { get; set; }
            private Vector<double> ForceVector { get; set; }

            // Mode shapes for the system
            private Vector<double> AngularNaturalFrequencies { get; set; }  // ω_n

            // Public properties to access the mode shape matrix and its inverse
            public Matrix<double> ModeShapeMatrix { get; set; }
            public Matrix<double> ModeShapeMatrixInverse { get; set; }

            // Public modal transformed data of the system
            public Vector<double> ModalMass { get; set; }
            public Vector<double> ModalStiffness { get; set; }
            public Vector<double> ModalZeta { get; set; }
            public Vector<double> ModalForce { get; set; }


            private Vector<double> ModeShape(int modeIndex)
            {
                // if (modeIndex < 0 || modeIndex >= ModeShapeMatrix.ColumnCount)
                // {
                //     throw new ArgumentOutOfRangeException(nameof(modeIndex), "Mode index is out of range.");
                // }
                return ModeShapeMatrix.Column(modeIndex);
            }


            private Vector<double> ModeShapeInverse(int modeIndex)
            {
                // if (modeIndex < 0 || modeIndex >= ModeShapeMatrixInverse.ColumnCount)
                // {
                //     throw new ArgumentOutOfRangeException(nameof(modeIndex), "Mode index is out of range.");
                // }
                return ModeShapeMatrixInverse.Column(modeIndex);
            }

            public Matrix<double> GetDampingMatrix() => this.DampingMatrix;
            


            public SystemMatrixData(Matrix<double> M_matrix, Matrix<double> K_matrix, double damping_ratio, double const_accl, bool IsFreeSystem)
            {
                this.MassMatrix = M_matrix;
                this.StiffnessMatrix = K_matrix;

                // Set the number of degrees of freedom based on the mass matrix
                this.nDOF = M_matrix.RowCount;

                // Calculate the damping matrix based on the damping ratio and stiffness matrix
                var (angularNaturalFrequencies, modeShapeMatrix) = GetModeShapeMatrix(M_matrix, K_matrix, nDOF);

                // Angular Natural Frequencies (Omega), Mode Shape Matrix and Mode Shape matrix inverse
                this.AngularNaturalFrequencies = angularNaturalFrequencies;
                this.ModeShapeMatrix = modeShapeMatrix;
                this.ModeShapeMatrixInverse = this.ModeShapeMatrix.Transpose() * this.MassMatrix;   // ModeShapeMatrix.Inverse();

                if(IsFreeSystem)
                {
                    this.AngularNaturalFrequencies[0] = 0.0; // For free system, the first natural frequency is zero (rigid body mode)
                }

                double[] modalDampingRatios = new double[nDOF];

                Vector<double> ForceVector = Vector<double>.Build.Dense(nDOF);

                for (int i = 0; i < nDOF; i++)
                {
                    modalDampingRatios[i] = damping_ratio;
                    ForceVector[i] = M_matrix[i,i] * const_accl; // F = ma
                }

                // Damping matrix C
                this.DampingMatrix = CalculateDamping(M_matrix, K_matrix, 
                    angularNaturalFrequencies, nDOF, modalDampingRatios);


                // Modal Damping ratios
                this.ModalZeta = CalculateModalDampingRatios(this.DampingMatrix, M_matrix, 
                    this.AngularNaturalFrequencies, nDOF);


                // Modal mass vector
                this.ModalMass = (modeShapeMatrix.Transpose() * M_matrix * modeShapeMatrix).Diagonal();

                // Modal stiffness vector
                this.ModalStiffness = (modeShapeMatrix.Transpose() * K_matrix * modeShapeMatrix).Diagonal();

                // Modal force vector
                this.ModalForce = modeShapeMatrix.Transpose() * ForceVector;

            }

            private (Vector<double> AngularNaturalFrequencies, Matrix<double> ModeShapeMatrix) 
                GetModeShapeMatrix(Matrix<double> M, Matrix<double> K, int dof)
            {

                if(dof == 1)
                {                     
                    // For 1 DOF system, the natural frequency is sqrt(k/m)
                    double omega = Math.Sqrt(K[0, 0] / M[0, 0]);
                    Vector<double> angularNaturalFrequencies = Vector<double>.Build.DenseOfArray(new double[] { omega });
                    Matrix<double> modeShapeMatrix = Matrix<double>.Build.Dense(1, 1, 1.0); // Mode shape is just [1]
                    return (angularNaturalFrequencies, modeShapeMatrix);
                }


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

                    Vector<double> AngularNaturalFrequencies = Vector<double>.Build.DenseOfArray(sortedOmega);
                    Matrix<double> modeShapeMatrix = massNormalizedVectors;

                    // Angular Natural Frequencies (Omega) and Mode Shape Matrix 
                    return (AngularNaturalFrequencies, modeShapeMatrix);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error solving eigenproblem: {ex.Message}");
                    throw;
                }
            }


            // =====================================================================
            // DAMPING CALCULATION
            // =====================================================================
            private Matrix<double> CalculateDamping(Matrix<double> M,
                                         Matrix<double> K,
                                         Vector<double> AngularNaturalFrequencies,
                                         int dof,
                                         double[] modalDampingRatios)
            {


                double[] dampingRatios = new double[dof];
                Matrix<double> dampingMatrix = Matrix<double>.Build.Dense(dof, dof);

                // For each mode, set the damping ratio individually
                for (int i = 0; i < dof; i++)
                {
                    dampingRatios[i] = (i < modalDampingRatios.Length) ? modalDampingRatios[i] : 0.05;
                }

                // Rayleigh damping: C = α*M + β*K
                // We can fit this to match the desired modal damping ratios
                if (dof >= 2)
                {
                    double ω1 = AngularNaturalFrequencies[0];
                    double ω2 = AngularNaturalFrequencies[1];
                    double ζ1 = dampingRatios[0];
                    double ζ2 = dampingRatios[1];

                    // Solve for α and β from the desired modal damping ratios
                    // ζ_i = (α / (2ω_i)) + (βω_i / 2)
                    if (ω2 > ω1 && ω1 > 1e-12)
                    {
                        double alpha = 2.0 * (ζ1 * ω1 * ω2 * ω2 - ζ2 * ω1 * ω1 * ω2) / (ω2 * ω2 - ω1 * ω1);
                        double beta = 2.0 * ((ζ2 * ω2) - (ζ1 * ω1)) / ((ω2 * ω2) - (ω1 * ω1));

                        dampingMatrix = alpha * M + beta * K;

                    }
                    else
                    {
                        // If we can't fit Rayleigh damping, use mass-proportional
                        dampingMatrix = 2.0 * dampingRatios[0] * M;
                    }
                }
                else if (dof == 1)
                {
                   dampingMatrix[0, 0] = 0.0;

                   // 1 DOF system
                   double ω = AngularNaturalFrequencies[0];
                    if (ω > 1e-12)
                    {
                        dampingMatrix[0, 0] = 2 * dampingRatios[0] * ω * M[0, 0];
                    }
                }

                return dampingMatrix;
            }



            private Vector<double> CalculateModalDampingRatios(Matrix<double> C, 
                Matrix<double> M, Vector<double> AngularNaturalFrequencies, int dof)
            {

                Vector<double> modalDampingRatios = Vector<double>.Build.Dense(dof);
                for (int i = 0; i < dof; i++)
                {
                    double ω_n = AngularNaturalFrequencies[i];
                    if (ω_n > 1e-12)
                    {
                        // ζ_i = (φ_i^T * C * φ_i) / (2 * ω_n * (φ_i^T * M * φ_i))
                        var phi_i = ModeShape(i);
                        double modalMass = phi_i.DotProduct(M * phi_i);
                        double modalDamping = phi_i.DotProduct(C * phi_i);
                        modalDampingRatios[i] = modalDamping / (2.0 * ω_n * modalMass);
                    }
                    else
                    {
                        modalDampingRatios[i] = 0.0;
                    }
                }
                return modalDampingRatios;
            }


        }



        private int fixedend_dof = 0;
        private int freeend_dof = 0;
        private int total_dof = 0;


        private List<double> fixedend_mass = new List<double>();
        private List<double> fixedend_stiffness = new List<double>();

        private List<double> freeend_mass = new List<double>();
        private List<double> freeend_stiffness = new List<double>();

        private double dampratio_zeta = 0.0;
        private double const_accla0 = 0.0;


        private SystemMatrixData fixedend_system;
        private SystemMatrixData freeend_system;
        private SystemMatrixData contact_system;


        // Contact stiffness and damping parameters
        private double contact_stiffness = 0.0;
        private double contact_damping = 0.0;

        public multidof1d_rigidcollisionSolverResult SimulationResults { get; private set; }
        private double total_time;



        public twodof_flexiblecollisionSolver(List<double> fixedend_mass, List<double> fixedend_stiffness, 
            List<double> freeend_mass, List<double> freeend_stiffness, 
            double dampratio_zeta, double const_accla0)
        {
            // Initialize the solver with the given parameters
            // Fixedend Mass [m1, m2, ..., mn], Fixedend Stiffness [k1, k2, ..., kn]


            // Set the number of degrees of freedom for fixed and free ends
            this.fixedend_dof = fixedend_mass.Count;
            this.freeend_dof = freeend_mass.Count;
            this.total_dof = this.fixedend_dof + this.freeend_dof;

            // Set the fixed end and free end mass and stiffness values
            this.fixedend_mass = fixedend_mass;
            this.fixedend_stiffness = fixedend_stiffness;
            this.freeend_mass = freeend_mass;
            this.freeend_stiffness = freeend_stiffness;

            this.dampratio_zeta = dampratio_zeta;
            this.const_accla0 = const_accla0;

            // Build the mass and stiffness matrices for the fixed system : Flight mode
            Matrix<double> M_fixed = Matrix<double>.Build.Dense(this.fixedend_dof, this.fixedend_dof);
            Matrix<double> K_fixed = Matrix<double>.Build.Dense(this.fixedend_dof, this.fixedend_dof);

            for (int i = 0; i < this.fixedend_dof; i++)
            {
                M_fixed[i, i] = fixedend_mass[i];
            }


            K_fixed[0, 0] = fixedend_stiffness[0];

            for (int i = 1; i < this.fixedend_dof; i++)
            {
                K_fixed[i, i] = K_fixed[i - 1, i - 1] + fixedend_stiffness[i];
                K_fixed[i, i - 1] = -fixedend_stiffness[i];
                K_fixed[i - 1, i] = -fixedend_stiffness[i];
            }

            fixedend_system = new SystemMatrixData(M_fixed, K_fixed, dampratio_zeta, const_accla0, false);

            // Build the mass and stiffness matrices for the free system : Flight mode
            Matrix<double> M_free = Matrix<double>.Build.Dense(this.freeend_dof, this.freeend_dof);
            Matrix<double> K_free = Matrix<double>.Build.Dense(this.freeend_dof, this.freeend_dof);

            for (int i = 0; i < this.freeend_dof; i++)
            {
                M_free[i, i] = freeend_mass[i];
            }

            K_free[0, 0] = 0.0; // The first stiffness is zero for the free end system (no spring connected to the fixed end last mass)

            for (int i = 1; i < this.freeend_dof; i++)
            {
                K_free[i, i] = K_free[i - 1, i - 1] + freeend_stiffness[i];
                K_free[i, i - 1] = -freeend_stiffness[i];
                K_free[i - 1, i] = -freeend_stiffness[i];
            }

            freeend_system = new SystemMatrixData(M_free, K_free, dampratio_zeta, const_accla0, true);


            //_____________________________________________________________________________________________________________________________
            // Build the contact system matrices (mass, stiffness) for the system
            Matrix<double> M_contact = Matrix<double>.Build.Dense(this.total_dof, this.total_dof);
            Matrix<double> K_contact = Matrix<double>.Build.Dense(this.total_dof, this.total_dof);

            for(int i = 0; i < this.fixedend_dof; i++)
            {
                M_contact[i, i] = fixedend_mass[i];
            }
            
            for(int i = 0; i < this.freeend_dof; i++)
            {
                M_contact[i + this.fixedend_dof, i + this.fixedend_dof] = freeend_mass[i];
            }


            K_contact[0, 0] = fixedend_stiffness[0];

            for (int i = 1; i < this.fixedend_dof; i++)
            {
                K_contact[i, i] = K_contact[i - 1, i - 1] + fixedend_stiffness[i];
                K_contact[i, i - 1] = -fixedend_stiffness[i];
                K_contact[i - 1, i] = -fixedend_stiffness[i];
            }

            for (int i = 0; i < this.freeend_dof; i++)
            {
                int offset = i + this.fixedend_dof;

                K_contact[offset, offset] = K_contact[offset - 1, offset - 1] + freeend_stiffness[i];
                K_contact[offset, offset - 1] = -freeend_stiffness[i];
                K_contact[offset - 1, offset] = -freeend_stiffness[i];
            }

            contact_system = new SystemMatrixData(M_contact, K_contact, dampratio_zeta, const_accla0, false);

            // First spring of free end system is the contact spring, so we can set the contact stiffness
            this.contact_stiffness = freeend_stiffness[0];
            this.contact_damping = contact_system.GetDampingMatrix()[this.fixedend_dof, this.fixedend_dof + 1]; // beta * k_contact

            //_____________________________________________________________________________________________________________________________

        }


        private double GetContactForce(Vector<double> displacement, Vector<double> velocity)
        {

            int last_fixed_dof_index = this.fixedend_dof - 1;
            int first_free_dof_index = this.fixedend_dof;

            // Relative displacement between the last fixed end mass and the first free end mass
            double relative_displacement = displacement[first_free_dof_index] - displacement[last_fixed_dof_index];  // un+1 - un

            // Relative velocity between the last fixed end mass and the first free end mass
            double relative_velocity = velocity[first_free_dof_index] - velocity[last_fixed_dof_index]; // vn+1 - vn

            // Get the contact force using the contact stiffness and damping
            double contact_force = this.contact_stiffness * relative_displacement + this.contact_damping * relative_velocity;

            return contact_force;
        }


        private double GetDerivativeContactForce(Vector<double> velocity, Vector<double> acceleration)
        {
         
            int last_fixed_dof_index = this.fixedend_dof - 1;
            int first_free_dof_index = this.fixedend_dof;

            // Relative velocity between the last fixed end mass and the first free end mass
            double relative_velocity = velocity[first_free_dof_index] - velocity[last_fixed_dof_index]; // vn+1 - vn

            // Relative acceleration between the last fixed end mass and the first free end mass
            double relative_acceleration = acceleration[first_free_dof_index] - acceleration[last_fixed_dof_index]; // an+1 - an

            // Get the derivative of contact force using the contact stiffness and damping
            double derivative_contact_force = this.contact_stiffness * relative_velocity + this.contact_damping * relative_acceleration;

            return derivative_contact_force;
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



        private (Vector<double> displacement, Vector<double> velocity, Vector<double> acceleration)
            FlightModeResponse(double time_t, Vector<double> u_inl, Vector<double> v_inl)
        {

            // Split the initial conditions
            Vector<double> u_fixedend = u_inl.SubVector(0, this.fixedend_dof);
            Vector<double> v_fixedend = v_inl.SubVector(0, this.fixedend_dof);

            Vector<double> u_freeend = u_inl.SubVector(this.fixedend_dof, this.freeend_dof);
            Vector<double> v_freeend = v_inl.SubVector(this.fixedend_dof, this.freeend_dof);


            //______________________________________________________________________________________________________
            // Fixed End Response (attached to wall) 
            // Transform to modal coordinates  q = Φ⁻¹ * u

            Vector<double> modal_u_fixedend_0 = fixedend_system.ModeShapeMatrixInverse * u_fixedend;
            Vector<double> modal_v_fixedend_0 = fixedend_system.ModeShapeMatrixInverse * v_fixedend;
            
            // Compute modal responses
            Vector<double> modal_u_fixedend_response = Vector<double>.Build.Dense(this.fixedend_dof);
            Vector<double> modal_v_fixedend_response = Vector<double>.Build.Dense(this.fixedend_dof);
            Vector<double> modal_a_fixedend_response = Vector<double>.Build.Dense(this.fixedend_dof);


            for(int i = 0; i < this.fixedend_dof; i++)
            {
                double mass_m = fixedend_system.ModalMass[i];
                double stiff_k = fixedend_system.ModalStiffness[i];
                double zeta = fixedend_system.ModalZeta[i];
                double const_a0 = fixedend_system.ModalForce[i];


                (double u, double v, double a) = GetSDOFResponse(time_t, mass_m, stiff_k, 
                    zeta, modal_u_fixedend_0[i], modal_v_fixedend_0[i], const_a0);


                modal_u_fixedend_response[i] = u;
                modal_v_fixedend_response[i] = v;
                modal_a_fixedend_response[i] = a;
            }

            // Convert back to physical coordinates
            Vector<double> u_fixedend_response = Vector<double>.Build.Dense(this.fixedend_dof);
            Vector<double> v_fixedend_response = Vector<double>.Build.Dense(this.fixedend_dof);
            Vector<double> a_fixedend_response = Vector<double>.Build.Dense(this.fixedend_dof);

            u_fixedend_response = fixedend_system.ModeShapeMatrix * modal_u_fixedend_response;
            v_fixedend_response = fixedend_system.ModeShapeMatrix * modal_v_fixedend_response;
            a_fixedend_response = fixedend_system.ModeShapeMatrix * modal_a_fixedend_response;


            //______________________________________________________________________________________________________
            // Free End Response (flight free end) 
            // Transform to modal coordinates  q = Φ⁻¹ * u

            Vector<double> u_freeend_response = Vector<double>.Build.Dense(this.freeend_dof);
            Vector<double> v_freeend_response = Vector<double>.Build.Dense(this.freeend_dof);
            Vector<double> a_freeend_response = Vector<double>.Build.Dense(this.freeend_dof);


            if (freeend_dof > 2)
            {
                // Transform to modal coordinates
                Vector<double> modal_u_freeend_0 = freeend_system.ModeShapeMatrixInverse * u_freeend;
                Vector<double> modal_v_freeend_0 = freeend_system.ModeShapeMatrixInverse * v_freeend;

                // Compute modal responses
                Vector<double> modal_u_freeend_response = Vector<double>.Build.Dense(this.freeend_dof);
                Vector<double> modal_v_freeend_response = Vector<double>.Build.Dense(this.freeend_dof);
                Vector<double> modal_a_freeend_response = Vector<double>.Build.Dense(this.freeend_dof);

                for (int i = 0; i < this.freeend_dof; i++)
                {
                    double mass_m = freeend_system.ModalMass[i];
                    double stiff_k = freeend_system.ModalStiffness[i];
                    double zeta = freeend_system.ModalZeta[i];
                    double const_a0 = freeend_system.ModalForce[i];


                    (double u, double v, double a) = GetSDOFResponse(time_t, mass_m, stiff_k,
                        zeta, modal_u_freeend_0[i], modal_v_freeend_0[i], const_a0);


                    modal_u_freeend_response[i] = u;
                    modal_v_freeend_response[i] = v;
                    modal_a_freeend_response[i] = a;
                }

                u_freeend_response = freeend_system.ModeShapeMatrix * modal_u_freeend_response;
                v_freeend_response = freeend_system.ModeShapeMatrix * modal_v_freeend_response;
                a_freeend_response = freeend_system.ModeShapeMatrix * modal_a_freeend_response;

            }
            else
            {
                // Single DOF free end - pure kinematic integration
                // (No spring, only constant acceleration)

                u_freeend_response[0] = u_freeend[0] + (v_freeend[0] * time_t) + (0.5 * const_accla0 * time_t * time_t);
                v_freeend_response[0] = v_freeend[0] + (const_accla0 * time_t);
                a_freeend_response[0] = const_accla0;
            }



            // Total response in physical coordinates
            Vector<double> displacement = Vector<double>.Build.Dense(
                    u_fixedend_response.ToArray().Concat(u_freeend_response.ToArray()).ToArray());
            Vector<double> velocity = Vector<double>.Build.Dense(
                    v_fixedend_response.ToArray().Concat(v_freeend_response.ToArray()).ToArray());
            Vector<double> acceleration = Vector<double>.Build.Dense(
                    a_fixedend_response.ToArray().Concat(a_freeend_response.ToArray()).ToArray());


            return (displacement, velocity, acceleration);


        }



        private (Vector<double> displacement, Vector<double> velocity, Vector<double> acceleration)
            ContactModeResponse(double time_t, Vector<double> u_inl, Vector<double> v_inl)
        {
            // Contact response is more complex due to the interaction between the fixed and free ends.
            // We will use the contact system matrices to compute the response.
            // Transform to modal coordinates

            Vector<double> modal_u_contact_0 = contact_system.ModeShapeMatrixInverse * u_inl;
            Vector<double> modal_v_contact_0 = contact_system.ModeShapeMatrixInverse * v_inl;


            // Compute modal responses
            Vector<double> modal_u_contact_response = Vector<double>.Build.Dense(this.total_dof);
            Vector<double> modal_v_contact_response = Vector<double>.Build.Dense(this.total_dof);
            Vector<double> modal_a_contact_response = Vector<double>.Build.Dense(this.total_dof);


            for (int i = 0; i < this.total_dof; i++)
            {
                double mass_m = contact_system.ModalMass[i];
                double stiff_k = contact_system.ModalStiffness[i];
                double zeta = contact_system.ModalZeta[i];
                double const_a0 = contact_system.ModalForce[i];


                (double u, double v, double a) = GetSDOFResponse(time_t, mass_m, stiff_k,
                    zeta, modal_u_contact_0[i], modal_v_contact_0[i], const_a0);


                modal_u_contact_response[i] = u;
                modal_v_contact_response[i] = v;
                modal_a_contact_response[i] = a;
            }

            // Convert back to physical coordinates
            Vector<double> u_contact_response = Vector<double>.Build.Dense(this.total_dof);
            Vector<double> v_contact_response = Vector<double>.Build.Dense(this.total_dof);
            Vector<double> a_contact_response = Vector<double>.Build.Dense(this.total_dof);


            u_contact_response = contact_system.ModeShapeMatrix * modal_u_contact_response;
            v_contact_response = contact_system.ModeShapeMatrix * modal_v_contact_response;
            a_contact_response = contact_system.ModeShapeMatrix * modal_a_contact_response;


            return (u_contact_response, v_contact_response, a_contact_response);

        }



        private (double t_exact, Vector<double> u, Vector<double> v, Vector<double> a)
          DetectFlightToCollisionPhaseTransition(double t_end, Vector<double> u_start, Vector<double> v_start)
        {

            double tau_low = 0.0;
            double tau_high = t_end;

            // Start with bisection to bracket the root
            for (int i = 0; i < 5; ++i)
            {

                double tau_mid = 0.5 * (tau_low + tau_high);

                (Vector<double> u_mid, Vector<double> v_mid, Vector<double> a_mid) = FlightModeResponse(tau_mid, u_start, v_start);
                (Vector<double> u_low, Vector<double> v_low, Vector<double> a_low) = FlightModeResponse(tau_low, u_start, v_start);

                double contact_force_mid = GetContactForce(u_mid, v_mid);
                double contact_force_low = GetContactForce(u_low, v_low);

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
                (u_tau, v_tau, a_tau) = FlightModeResponse(tau, u_start, v_start);
                double contact_force_tau = GetContactForce(u_tau, v_tau);
                double derivative_contact_force_tau = GetDerivativeContactForce(v_tau, a_tau);

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



        private (double t_exact, Vector<double> u, Vector<double> v, Vector<double> a)
            DetectCollisionToFlightPhaseTransition(double t_end, Vector<double> u_start, Vector<double> v_start)
        {

            double tau_low = 0.0;
            double tau_high = t_end;

            // Start with bisection to bracket the root
            for (int i = 0; i < 5; ++i)
            {

                double tau_mid = 0.5 * (tau_low + tau_high);

                (Vector<double> u_mid, Vector<double> v_mid, Vector<double> a_mid) = ContactModeResponse(tau_mid, u_start, v_start);
                (Vector<double> u_low, Vector<double> v_low, Vector<double> a_low) = ContactModeResponse(tau_low, u_start, v_start);

                double contact_force_mid = GetContactForce(u_mid, v_mid);
                double contact_force_low = GetContactForce(u_low, v_low);

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
                (u_tau, v_tau, a_tau) = ContactModeResponse(tau, u_start, v_start);
                double contact_force_tau = GetContactForce(u_tau, v_tau);
                double derivative_contact_force_tau = GetDerivativeContactForce(v_tau, a_tau);

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



        public void solve_multidof_collision_with_flexible_boundary(double total_simulation_time, 
            double max_time_increment,
            List<double> u_inl, List<double> v_inl)
        {

            this.total_time = total_simulation_time;


            // Clear previous results
            SimulationResults.ClearData();


            // Physical initial conditions
            Vector<double> u_at_event = Vector<double>.Build.Dense(u_inl.ToArray());
            Vector<double> v_at_event = Vector<double>.Build.Dense(v_inl.ToArray());

            Vector<double> u_at_t = u_at_event.Clone();
            Vector<double> v_at_t = v_at_event.Clone();
            Vector<double> a_at_t = Vector<double>.Build.Dense(2);

            double time_t = 0.0;
            double t_event = 0.0;

            // Calculate the contact force at time step 0
            double contact_force_at_t = GetContactForce(u_at_event, v_at_event);

            // Determine initial phase
            bool IsContact = contact_force_at_t <= 0.0;



            if (!IsContact)
            {

                // Calculate the acceleration at time step 0 for flight phase
                (_, _, a_at_t) = FlightModeResponse(time_t, u_at_event, v_at_event);
            }
            else
            {
                // Calculate the acceleration at time step 0 for contact phase
                (_, _, a_at_t) = ContactModeResponse(time_t, u_at_event, v_at_event);

                SimulationResults.TimeContactBand.Add(time_t);
            }

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


                if (!IsContact)
                {
                    // Flight phase
                    (u_at_t, v_at_t, a_at_t) = FlightModeResponse(t_tau, u_at_event, v_at_event);
                }
                else
                {
                    // Contact phase
                    (u_at_t, v_at_t, a_at_t) = ContactModeResponse(t_tau, u_at_event, v_at_event);
                }

                // Calculate the contact force at the current time step
                contact_force_at_t = GetContactForce(u_at_t, v_at_t);


                // Transition check: Determine if the system transitions from flight to contact or vice versa
                if (contact_force_at_t > 0.0 && IsContact == true)
                {
                    IsContact = false;

                    // Get previous state
                    (Vector<double> u_prev, Vector<double> v_prev, _) =
                        SimulationResults.GetStateAtTimeIndex(SimulationResults.TimePoints.Count - 1);


                    double tau_exact = 0.0;

                    (tau_exact, u_at_t, v_at_t, a_at_t) = DetectCollisionToFlightPhaseTransition(max_time_increment,
                        u_prev, v_prev);

                    // Recalculate the contact force at the exact transition time
                    contact_force_at_t = GetContactForce(u_at_t, v_at_t);


                    // Adjust the time and event time based on the exact transition time
                    time_t = (time_t - max_time_increment) + tau_exact;

                    // Update state to transition point
                    t_event = time_t;
                    u_at_event = u_at_t.Clone();
                    v_at_event = v_at_t.Clone();

                    SimulationResults.TimeContactBand.Add(time_t);

                }
                else if (contact_force_at_t <= 0.0 && IsContact == false)
                {
                    IsContact = true;

                    // Get previous state
                    (Vector<double> u_prev, Vector<double> v_prev, _) =
                             SimulationResults.GetStateAtTimeIndex(SimulationResults.TimePoints.Count - 1);


                    double tau_exact = 0.0;

                    (tau_exact, u_at_t, v_at_t, a_at_t) = DetectFlightToCollisionPhaseTransition(max_time_increment,
                        u_prev, v_prev);

                    // Recalculate the contact force at the exact transition time
                    contact_force_at_t = GetContactForce(u_at_t, v_at_t);

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



