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


class lagrange_dynamics_solver
{
public:
	lagrange_dynamics_solver();
	~lagrange_dynamics_solver() = default;

	void set_matrices(std::unordered_map<int, gyronode_store*> g_nodes,
	std::vector<gyrospring_store*> g_springs);

	// Mass matrix

	// Stiffness matrix

	// Damping matrix

	// External force matrix (Column matrix)

private:
	std::unordered_map<int, int> nodeid_map;

	// Global stiffness matrix
	Eigen::MatrixXd globalStiffnessMatrix;

	// Global mass matrix
	Eigen::MatrixXd globalMassMatrix;


	void get_global_stiffness_matrix(Eigen::MatrixXd& globalStiffnessMatrix, std::vector<gyrospring_store*> g_springs);


	void get_element_stiffness_matrix(Eigen::Matrix4d& elementStiffnessMatrix, gyrospring_store* spring_element);





};
