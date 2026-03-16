#pragma once
#include <iostream>
#include <fstream>

// Stop watch
#include "../events_handler/Stopwatch_events.h"

#include "../geometry_store/geom_parameters.h"

#pragma warning(push)
#pragma warning (disable : 26451)
#pragma warning (disable : 26495)
#pragma warning (disable : 6255)
#pragma warning (disable : 6294)
#pragma warning (disable : 26813)
#pragma warning (disable : 26454)

// Optimization for Eigen Library
// 1) OpenMP (Yes (/openmp)
//	 Solution Explorer->Configuration Properties -> C/C++ -> Language -> Open MP Support
// 2) For -march=native, choose "AVX2" or the latest supported instruction set.
//   Solution Explorer->Configuration Properties -> C/C++ -> Code Generation -> Enable Enhanced Instruction Set 

#include <Eigen/Dense>
#include <Eigen/Sparse>
#include <Eigen/SparseLU>
#include <Eigen/Eigenvalues>
// Define the sparse matrix type for the reduced global stiffness matrix
typedef Eigen::SparseMatrix<double> SparseMatrix;
#pragma warning(pop)



class hht_alpha_newmark_method
{
public:
	hht_alpha_newmark_method();
	~hht_alpha_newmark_method() = default;

	void initialize_hhtsolver(const Eigen::MatrixXd& massMatrix,
		const Eigen::MatrixXd& inversemassMatrix,
		const Eigen::MatrixXd& stiffnessMatrix,
		const Eigen::MatrixXd& dampingMatrix,
		const Eigen::VectorXd& initialDispl,
		const Eigen::VectorXd& initialVelo,
		const Eigen::VectorXd& initialForce,
		int solvertype);

	void hht_alpha_newmark_solve(double dt);

	void update_force_vector(Eigen::VectorXd force_vector_at_timet);

	const Eigen::VectorXd& get_displacement_at_t() const;

	const Eigen::VectorXd& get_velocity_at_t() const;

	const Eigen::VectorXd& get_acceleration_at_t() const;

private:
	// Alpha values
	//   0	    Standard Newmark(no numerical damping)
	// -0.05	Very light high - frequency damping
	//	-0.1	Moderate damping
	//	-0.2	Strong damping
	//	-1 / 3 = -0.333	Maximum stable dissipation

	//             Abaqus	-0.05
	//	ANSYS	around -0.05 to -0.1
	//	many research codes	-0.1

	// Hilber - Hughes - Taylor alpha method
	const double alpha = -0.05; // Varies between -1/3 to 0
	double gamma = 0.0; // 0.5 (1.0 - 2.0 * alpha)
	double beta = 0.0; // 0.25 (1.0 - alpha)^2

	// Average Acceleration method (Alpha = 0.0)
	// gamma = 0.5, beta = 1/4
	
	// Linear Acceleration metho
	// gamma = 0.5, beta = 1/6

	int solvertype = 0; // 0 = HHT, 1 = Newmark

	// Set the mass, stiffness and damping matrix
	Eigen::SparseMatrix<double> massMatrix;
	Eigen::SparseMatrix<double> stiffnessMatrix;
	Eigen::SparseMatrix<double> dampingMatrix;


	Eigen::VectorXd displ_at_t;
	Eigen::VectorXd velo_at_t;
	Eigen::VectorXd accl_at_t;

	Eigen::VectorXd force_vector_at_t;
	Eigen::VectorXd force_vector_prev;

	// Solver Matrices
	Eigen::SparseMatrix<double> a1_matrix;
	Eigen::SparseMatrix<double> a2_matrix;
	Eigen::SparseMatrix<double> a3_matrix;
	Eigen::SparseMatrix<double> stiffness_hat_matrix;

	Eigen::SimplicialLDLT<Eigen::SparseMatrix<double>, Eigen::Lower, Eigen::NaturalOrdering<int>> solver;


	void hht_solve(double dt);

	void newmark_solve(double dt);

};