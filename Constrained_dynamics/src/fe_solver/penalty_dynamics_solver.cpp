#include "penalty_dynamics_solver.h"


penalty_dynamics_solver::penalty_dynamics_solver()
{
	// Empty constructor

}




void penalty_dynamics_solver::set_penaltysolver_matrices(std::unordered_map<int, gyronode_store*> g_nodes,
	std::vector<gyrospring_store*> g_springs)
{
	// Create a node ID map (assuming all the nodes are ordered and numbered from 0,1,2...n)
	int i = 0;
	nodeid_map.clear();
	for (auto& nd_m : g_nodes)
	{
		gyronode_store* nd = nd_m.second;

		// Node id -> i
		nodeid_map[nd->gnode_id] = i;
		i++;
	}

	//____________________________________________________________________________________________________________________
	int numDOF = static_cast<int>(nodeid_map.size()) * 2; // Number of degrees of freedom (2 DOFs per node)


	//____________________________________________________________________________________________________________________
	// Global Stiffness Matrix
	globalStiffnessMatrix.resize(numDOF, numDOF);
	globalStiffnessMatrix.setZero();

	get_global_stiffness_matrix(globalStiffnessMatrix, g_springs);

	//____________________________________________________________________________________________________________________
	// Global Mass Matrix
	globalMassMatrix.resize(numDOF, numDOF);
	globalMassMatrix.setZero();

	//____________________________________________________________________________________________________________________
	// Inverse Global Mass Matrix
	inverse_globalMassMatrix.resize(numDOF, numDOF);
	inverse_globalMassMatrix.setZero();

	get_global_mass_matrix(globalMassMatrix, inverse_globalMassMatrix, g_nodes);


	//____________________________________________________________________________________________________________________
	// Global Penalty for Stiffness 
	// SPC Matrix contains (Single point constraints - Pin support) and MPC matrix containt (Multi point constraints - Rigid elements)

	globalPenalty_SPC_StiffnessMatrix(numDOF, numDOF);
	globalPenalty_SPC_StiffnessMatrix.setZero();

	globalPenalty_MPC_StiffnessMatrix(numDOF, numDOF);
	globalPenalty_MPC_StiffnessMatrix.setZero();


	get_boundary_condition_penalty_matrix(globalPenalty_SPC_StiffnessMatrix, globalPenalty_MPC_StiffnessMatrix, numDOF,
		g_nodes, g_springs);


	//____________________________________________________________________________________________________________________
	// Penalty Augmentation of global stiffness matrix
	globalPenaltyAugmentedStiffnessMatrix(numDOF, numDOF);
	globalPenaltyAugmentedStiffnessMatrix.setZero();

	globalPenaltyAugmentedStiffnessMatrix = globalStiffnessMatrix + 
		(globalPenalty_SPC_StiffnessMatrix + globalPenalty_MPC_StiffnessMatrix);



	//____________________________________________________________________________________________________________________
	// Intial displacement vector 
	initial_displVector.resize(numDOF);
	initial_displVector.setZero();

	get_initial_displ_vector(initial_displVector, g_nodes);


	//____________________________________________________________________________________________________________________
	// Intialize the Force vector (Zero at time = 0.0) 
	forceVector.resize(numDOF);
	forceVector.setZero();



}







void penalty_dynamics_solver::get_global_stiffness_matrix(Eigen::MatrixXd& globalStiffnessMatrix,
	std::vector<gyrospring_store*> g_springs)
{
	this->max_stiffness = 0.0;


	for (auto& spring_m : g_springs)
	{
		gyrospring_store* spring_element = spring_m;

		// Create a matrix for element striffness matrix
		Eigen::Matrix4d elementStiffnessMatrix = Eigen::Matrix4d::Zero();;

		get_element_stiffness_matrix(elementStiffnessMatrix, spring_element);

		// Set the maximum stiffness
		this->max_stiffness = std::max(this->max_stiffness, spring_element->stiffeness_K);

		// Get the Node ID map
		int sn_ndmap = nodeid_map[spring_element->gstart_node->gnode_id]; // get the ordered map of the start node ID
		int en_ndmap = nodeid_map[spring_element->gend_node->gnode_id]; // get the ordered map of the end node ID

		globalStiffnessMatrix.block<2, 2>(sn_ndmap * 2, sn_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(0, 0);
		globalStiffnessMatrix.block<2, 2>(sn_ndmap * 2, en_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(0, 2);
		globalStiffnessMatrix.block<2, 2>(en_ndmap * 2, sn_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(2, 0);
		globalStiffnessMatrix.block<2, 2>(en_ndmap * 2, en_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(2, 2);

	}

	if (this->max_stiffness == 0)
	{
		// All the elements are rigid elements
		this->max_stiffness = 100.0;
	}


	//
}


void penalty_dynamics_solver::get_element_stiffness_matrix(Eigen::Matrix4d& elementStiffnessMatrix,
	gyrospring_store* spring_element)
{

	// Create a element stiffness matrix
	// Compute the differences in x and y coordinates
	double dx = spring_element->gend_node->gnode_pt.x - spring_element->gstart_node->gnode_pt.x;
	double dy = -1.0 * (spring_element->gend_node->gnode_pt.y - spring_element->gstart_node->gnode_pt.y);

	// Compute the length of the truss element
	double eLength = std::sqrt((dx * dx) + (dy * dy));

	// Compute the direction cosines
	double Lcos = (dx / eLength);
	double Msin = (dy / eLength);

	// Compute the stiffness factor
	double k1 = 0.0;

	if (spring_element->is_rigid == true)
	{
		// Rigid element
		k1 = 0;

	}
	else
	{
		// Spring element
		k1 = spring_element->stiffeness_K;

	}

	//Stiffness matrix components
	double v1 = k1 * std::pow(Lcos, 2);
	double v2 = k1 * std::pow(Msin, 2);
	double v3 = k1 * (Lcos * Msin);

	// Create the Element stiffness matrix
	elementStiffnessMatrix.row(0) = Eigen::RowVector4d(v1, v3, -v1, -v3);
	elementStiffnessMatrix.row(1) = Eigen::RowVector4d(v3, v2, -v3, -v2);
	elementStiffnessMatrix.row(2) = Eigen::RowVector4d(-v1, -v3, v1, v3);
	elementStiffnessMatrix.row(3) = Eigen::RowVector4d(-v3, -v2, v3, v2);
	//
}



void penalty_dynamics_solver::get_global_mass_matrix(Eigen::MatrixXd& globalMassMatrix,
	Eigen::MatrixXd& inverse_globalMassMatrix,
	std::unordered_map<int, gyronode_store*> g_nodes)
{
	// Create the global mass matrix
	for (auto& node_m : g_nodes)
	{
		gyronode_store* node = node_m.second;


		// Get the Node ID map
		int nd_map = nodeid_map[node->gnode_id]; // get the ordered map of the start node ID


		// Mass matrix is set on diagonal
		double mass_m = node->gmass_value;

		globalMassMatrix(nd_map * 2, nd_map * 2) = mass_m;
		globalMassMatrix((nd_map * 2) + 1, (nd_map * 2) + 1) = mass_m;

		if (mass_m != 0)
		{
			inverse_globalMassMatrix(nd_map * 2, nd_map * 2) = (1.0 / mass_m);
			inverse_globalMassMatrix((nd_map * 2) + 1, (nd_map * 2) + 1) = (1.0 / mass_m);
		}

	}
	//
}




void penalty_dynamics_solver::get_boundary_condition_penalty_matrix(Eigen::MatrixXd& globalPenalty_SPC_StiffnessMatrix,
	Eigen::MatrixXd& globalPenalty_MPC_StiffnessMatrix, int numDOF,
	std::unordered_map<int, gyronode_store*> g_nodes, std::vector<gyrospring_store*> g_springs)
{

	// Apply boundary condition using Penalty method
	// Single point constraint (only Pinned boundary condition)
	Eigen::MatrixXd  global_penalty_SPC_AMatrix(numDOF, 0); // Start with zero columns

	int currentCols = 0;


	for (auto& node_m : g_nodes)
	{
		gyronode_store* node = node_m.second;

		// Get the Node ID 
		int nd_map = nodeid_map[node->gnode_id]; // get the ordered map of the start node ID

		// Node is pinned or not
		if (node->isFixed == true)
		{
			// Single point constraint A vector
			Eigen::VectorXd penalty_SPC_AVector(numDOF);
			penalty_SPC_AVector.setZero();

			// Pinned support (Degree of Freedom 1)
			penalty_SPC_AVector[(nd_map * 2) + 0] = 1.0;
			penalty_SPC_AVector[(nd_map * 2) + 1] = 0.0;

			// **Expand A_matrix by adding a new column**
			currentCols = global_penalty_SPC_AMatrix.cols();
			global_penalty_SPC_AMatrix.conservativeResize(numDOF, currentCols + 1); // Add one column
			global_penalty_SPC_AMatrix.col(currentCols) = penalty_SPC_AVector;       // Insert the new vector (X fixed)

			// Pinned support (Degree of Freedom 2)
			penalty_SPC_AVector[(nd_map * 2) + 0] = 0.0;
			penalty_SPC_AVector[(nd_map * 2) + 1] = 1.0;

			// **Expand A_matrix by adding a new column**
			currentCols = global_penalty_SPC_AMatrix.cols();
			global_penalty_SPC_AMatrix.conservativeResize(numDOF, currentCols + 1); // Add one column
			global_penalty_SPC_AMatrix.col(currentCols) = penalty_SPC_AVector;       // Insert the new vector (Y fixed)

		}
		//
	}


	// Multi point constraint (Rigid link)
	Eigen::MatrixXd global_penalty_MPC_AMatrix(numDOF, 0); // Start with zero columns


	for (auto& spring_m : g_springs)
	{
		gyrospring_store* spring_element = spring_m;

		if (spring_element->is_rigid == true)
		{
			// Rigid element
			// Get the Node ID map
			int sn_ndmap = nodeid_map[spring_element->gstart_node->gnode_id]; // get the ordered map of the start node ID
			int en_ndmap = nodeid_map[spring_element->gend_node->gnode_id]; // get the ordered map of the end node ID


			// Get the element properties
			// Compute the differences in x and y coordinates
			double dx = spring_element->gend_node->gnode_pt.x - spring_element->gstart_node->gnode_pt.x;
			double dy = -1.0 * (spring_element->gend_node->gnode_pt.y - spring_element->gstart_node->gnode_pt.y);

			// Compute the length of the truss element
			double eLength = std::sqrt((dx * dx) + (dy * dy));

			// Compute the direction cosines
			double Lcos = (dx / eLength);
			double Msin = (dy / eLength);


			// Multi point constraint A vector
			Eigen::VectorXd penalty_MPC_AVector(numDOF);
			penalty_MPC_AVector.setZero();

			// start node transformation
			penalty_MPC_AVector[(sn_ndmap * 2) + 0] = Lcos;
			penalty_MPC_AVector[(sn_ndmap * 2) + 1] = Msin;

			// end node transformation
			penalty_MPC_AVector[(en_ndmap * 2) + 0] = -Lcos;
			penalty_MPC_AVector[(en_ndmap * 2) + 1] = -Msin;

			// **Expand A_matrix by adding a new column**
			currentCols = global_penalty_MPC_AMatrix.cols();
			global_penalty_MPC_AMatrix.conservativeResize(numDOF, currentCols + 1); // Add one column
			global_penalty_MPC_AMatrix.col(currentCols) = penalty_MPC_AVector;       // Insert the new vector

		}
		//
	}


	// Create the Penalty Stiffness Matrix
	double penalty = this->max_stiffness * this->penalty_factor;

	globalPenalty_SPC_StiffnessMatrix = penalty * (global_penalty_SPC_AMatrix * global_penalty_SPC_AMatrix.transpose());

	if (global_penalty_MPC_AMatrix.cols() > 0)
	{
		globalPenalty_MPC_StiffnessMatrix = penalty * (global_penalty_MPC_AMatrix * global_penalty_MPC_AMatrix.transpose());
	}



	//
}