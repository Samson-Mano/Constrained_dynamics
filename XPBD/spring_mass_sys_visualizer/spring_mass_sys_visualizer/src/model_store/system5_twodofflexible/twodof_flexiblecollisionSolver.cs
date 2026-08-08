using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using spring_mass_sys_visualizer.src.model_store.system2_store_data;
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
            public Matrix<double> MassMatrix { get; set; }
            public Matrix<double> StiffnessMatrix { get; set; }
            public Matrix<double> DampingMatrix { get; set; }
            public Vector<double> ForceVector { get; set; }

            // Mode shapes for the system
            public Vector<double> AngularNaturalFrequencies { get; set; }  // ω_n
            public Matrix<double> ModeShapeMatrix { get; set; }
            public Matrix<double> ModeShapeMatrixInverse { get; set; }

            // Modal analysis results for the system
            public Vector<double> ModalMass { get; set; }
            public Vector<double> ModalStiffness { get; set; }
            public Vector<double> ModalZeta { get; set; }
            public Vector<double> ModalForce { get; set; }


            public Vector<double> ModeShape(int modeIndex)
            {
                // if (modeIndex < 0 || modeIndex >= ModeShapeMatrix.ColumnCount)
                // {
                //     throw new ArgumentOutOfRangeException(nameof(modeIndex), "Mode index is out of range.");
                // }
                return ModeShapeMatrix.Column(modeIndex);
            }


            public Vector<double> ModeShapeInverse(int modeIndex)
            {
                // if (modeIndex < 0 || modeIndex >= ModeShapeMatrixInverse.ColumnCount)
                // {
                //     throw new ArgumentOutOfRangeException(nameof(modeIndex), "Mode index is out of range.");
                // }
                return ModeShapeMatrixInverse.Column(modeIndex);
            }


            public SystemMatrixData(Matrix<double> M_matrix, Matrix<double> K_matrix, double damping_ratio, double const_accl)
            {
                this.MassMatrix = M_matrix;
                this.StiffnessMatrix = K_matrix;

                // Set the number of degrees of freedom based on the mass matrix
                this.nDOF = M_matrix.RowCount;

                // Calculate the damping matrix based on the damping ratio and stiffness matrix
                var (angularNaturalFrequencies, modeShapeMatrix) = GetModeShapeMatrix(M_matrix, K_matrix, nDOF);

                this.AngularNaturalFrequencies = angularNaturalFrequencies;
                this.ModeShapeMatrix = modeShapeMatrix;
                this.ModeShapeMatrixInverse = this.ModeShapeMatrix.Transpose() * this.MassMatrix;   // ModeShapeMatrix.Inverse();

                double[] modalDampingRatios = new double[nDOF];
                for (int i = 0; i < nDOF; i++)
                {
                    modalDampingRatios[i] = damping_ratio;
                }


                this.DampingMatrix = CalculateDamping(M_matrix, K_matrix, 
                    angularNaturalFrequencies, nDOF, modalDampingRatios);




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

                    //// Calculate modal properties
                    //Matrix<double> modalMassMatrix = modeShapeMatrix.Transpose() * M * modeShapeMatrix;
                    //Matrix<double> modalStiffnessMatrix = modeShapeMatrix.Transpose() * K * modeShapeMatrix;
                    
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
                Matrix<double> M, Matrix<double> K, Vector<double> AngularNaturalFrequencies, int dof)
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


        public twodof_flexiblecollisionSolver(List<double> fixedend_mass, List<double> fixedend_stiffness, 
            List<double> freeend_mass, List<double> freeend_stiffness, 
            double dampratio_zeta, double const_accla0)
        {

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

            // Build the mass and stiffness matrices for the system

        }


        private void _BuildFlightMatrices()
        {
            // Build the mass and stiffness matrices for the system
            // This method will create the mass and stiffness matrices based on the fixed and free end properties




        }


        private void _BuildContactMatrices()
        {
            // Build the contact matrices for the system
            // This method will create the contact matrices based on the fixed and free end properties

        }








        }
    }



