#pragma once
#include <iostream>
#include <fstream>

// Stop watch
#include "../events_handler/Stopwatch_events.h"

#include "../geometry_store/geom_parameters.h"
#include "hht_alpha_newmark_method.h"


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


class lagrange_dynamics_solver
{
public:
	lagrange_dynamics_solver();
	~lagrange_dynamics_solver() = default;

	void set_lagrangesolver_matrices(std::unordered_map<int, gyronode_store*> g_nodes,
	std::vector<gyrospring_store*> g_springs);


	void perform_lagrange_solve(double dt, std::unordered_map<int, gyronode_store*> g_nodes);


private:
	const double M_PI = 3.14159265358979323;

	double accumulated_time = 0.0;

	bool isMatricesSet = false;
	std::unordered_map<int, int> nodeid_map;

	// Global stiffness matrix
	Eigen::MatrixXd globalStiffnessMatrix;

	// Global mass matrix
	Eigen::MatrixXd globalMassMatrix;

	// Inverse global mass matrix
	Eigen::MatrixXd inverse_globalMassMatrix;


	// Global constraint matrix
	Eigen::MatrixXd globalConstraint_SPC_AMatrix; 
	Eigen::MatrixXd globalConstraint_MPC_AMatrix; 

	Eigen::MatrixXd globalConstraint_AMatrix;
	
	// Damping C matrix
	Eigen::MatrixXd dampingCMatrix;


	// Initial displacement vector
	Eigen::VectorXd initial_displVector;

	// Initial velocity vector
	Eigen::VectorXd initial_veloVector;


	// Force Vector
	Eigen::VectorXd forceVector;

	// Lagrange Augmentation of Matrix
	Eigen::MatrixXd globalLagrangeAugmentedStiffnessMatrix;
	Eigen::MatrixXd globalLagrangeAugmentedMassMatrix;
	Eigen::MatrixXd globalLagrangeAugmentedinverseMassMatrix;
	Eigen::MatrixXd globalLagrangeAugmentedDampingMatrix;
	Eigen::VectorXd globalAugmentedInitialDisplacement;
	Eigen::VectorXd globalAugmentedInitialVelocity;
	Eigen::VectorXd globalAugmentedForceVector;

	// HHT solver
	hht_alpha_newmark_method n_solver;



	void get_global_stiffness_matrix(Eigen::MatrixXd& globalStiffnessMatrix, std::vector<gyrospring_store*> g_springs);


	void get_element_stiffness_matrix(Eigen::Matrix4d& elementStiffnessMatrix, gyrospring_store* spring_element);


	void get_global_mass_matrix(Eigen::MatrixXd& globalMassMatrix, Eigen::MatrixXd& inverse_globalMassMatrix, 
		std::unordered_map<int, gyronode_store*> g_nodes);


	void set_global_constraint_A_matrix(Eigen::MatrixXd& globalConstraint_SPC_AMatrix,
		Eigen::MatrixXd& globalConstraint_MPC_AMatrix, int numDOF,
		std::unordered_map<int, gyronode_store*> g_nodes, std::vector<gyrospring_store*> g_springs);


	void get_initial_displ_vector(Eigen::VectorXd& initial_displVector, std::unordered_map<int, gyronode_store*> g_nodes);


	void print_matrices();

};
