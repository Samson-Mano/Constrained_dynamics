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




class penalty_dynamics_solver
{
public:
	penalty_dynamics_solver();
	~penalty_dynamics_solver() = default;

	void set_penaltysolver_matrices(std::unordered_map<int, gyronode_store*> g_nodes,
		std::vector<gyrospring_store*> g_springs);

private:
	const double M_PI = 3.14159265358979323;

	// Penalty stiffness
	double max_stiffness = 0.0;
	const double penalty_factor = 1E+6;


	std::unordered_map<int, int> nodeid_map;

	// Global stiffness matrix
	Eigen::MatrixXd globalStiffnessMatrix;

	// Global mass matrix
	Eigen::MatrixXd globalMassMatrix;

	// Inverse global mass matrix
	Eigen::MatrixXd inverse_globalMassMatrix;


	// Global Penalty SPC & MPC matrices
	Eigen::MatrixXd globalPenalty_SPC_StiffnessMatrix;
	Eigen::MatrixXd globalPenalty_MPC_StiffnessMatrix;


	// Penalty Augmentation for Stiffness Matrix 
	Eigen::MatrixXd globalPenaltyAugmentedStiffnessMatrix;

	// Initial displacement vector
	Eigen::VectorXd initial_displVector;

	// Force Vector
	Eigen::VectorXd forceVector;




	void get_global_stiffness_matrix(Eigen::MatrixXd& globalStiffnessMatrix, std::vector<gyrospring_store*> g_springs);


	void get_element_stiffness_matrix(Eigen::Matrix4d& elementStiffnessMatrix, gyrospring_store* spring_element);


	void get_global_mass_matrix(Eigen::MatrixXd& globalMassMatrix, Eigen::MatrixXd& inverse_globalMassMatrix,
		std::unordered_map<int, gyronode_store*> g_nodes);


	void get_boundary_condition_penalty_matrix(Eigen::MatrixXd& globalPenalty_SPC_StiffnessMatrix,
		Eigen::MatrixXd& globalPenalty_MPC_StiffnessMatrix, int numDOF,
		std::unordered_map<int, gyronode_store*> g_nodes, std::vector<gyrospring_store*> g_springs);


	void get_initial_displ_vector(Eigen::VectorXd& initial_displVector, std::unordered_map<int, gyronode_store*> g_nodes);


};





