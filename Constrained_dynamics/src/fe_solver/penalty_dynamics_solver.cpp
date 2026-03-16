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

	globalPenalty_SPC_StiffnessMatrix.resize(numDOF, numDOF);
	globalPenalty_SPC_StiffnessMatrix.setZero();

	globalPenalty_MPC_StiffnessMatrix.resize(numDOF, numDOF);
	globalPenalty_MPC_StiffnessMatrix.setZero();


	get_boundary_condition_penalty_matrix(globalPenalty_SPC_StiffnessMatrix, globalPenalty_MPC_StiffnessMatrix, numDOF,
		g_nodes, g_springs);


	//____________________________________________________________________________________________________________________
	// Penalty Augmentation of global stiffness matrix
	globalPenaltyAugmentedStiffnessMatrix.resize(numDOF, numDOF);
	globalPenaltyAugmentedStiffnessMatrix.setZero();

	globalPenaltyAugmentedStiffnessMatrix = globalStiffnessMatrix + 
		(globalPenalty_SPC_StiffnessMatrix + globalPenalty_MPC_StiffnessMatrix);

	//____________________________________________________________________________________________________________________
	// Damping matrix
	dampingCMatrix.resize(numDOF, numDOF);
	dampingCMatrix.setZero();

	//____________________________________________________________________________________________________________________
	// Intial displacement vector 
	initial_displVector.resize(numDOF);
	initial_displVector.setZero();

	get_initial_displ_vector(initial_displVector, g_nodes);


	initial_veloVector.resize(numDOF);
	initial_veloVector.setZero();


	//____________________________________________________________________________________________________________________
	// Intialize the Force vector (Zero at time = 0.0) 
	forceVector.resize(numDOF);
	forceVector.setZero();


	//___________________ Print the matrices for Debugging_______________________
	// print_matrices();



	//____________________________________________________________________________________________________________________
	// Set the solver
	int solvertype = 0; // 0 = HHT, 1 = Newmark

	n_solver.initialize_hhtsolver(globalMassMatrix,
		inverse_globalMassMatrix,
		globalPenaltyAugmentedStiffnessMatrix,
		dampingCMatrix,
		initial_displVector,
		initial_veloVector,
		forceVector,
		solvertype);


	isMatricesSet = true;
	accumulated_time = 0.0;
	//
}


void penalty_dynamics_solver::perform_penalty_solve(double dt,
	std::unordered_map<int, gyronode_store*> g_nodes)
{

	// Main solver call
	n_solver.hht_alpha_newmark_solve(dt);

	// get the results
	Eigen::VectorXd globalAugmentedDisplacement_at_t = n_solver.get_displacement_at_t();
	Eigen::VectorXd globalAugmentedVelocity_at_t = n_solver.get_velocity_at_t();

	// Map the results to the node
	// Create the global mass matrix
	for (auto& node_m : g_nodes)
	{
		int nd_id = node_m.first;
		gyronode_store* node = node_m.second;

		// Get the Node ID map
		int nd_map = nodeid_map[node->gnode_id]; // get the ordered map of the start node ID

		g_nodes[nd_id]->gnode_displ = glm::vec2(globalAugmentedDisplacement_at_t(nd_map * 2),
			globalAugmentedDisplacement_at_t((nd_map * 2) + 1));


		g_nodes[nd_id]->gnode_velo = glm::vec2(globalAugmentedVelocity_at_t(nd_map * 2),
			globalAugmentedVelocity_at_t((nd_map * 2) + 1));

	}


	// Accumulate the time
	accumulated_time = accumulated_time + dt;

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





void penalty_dynamics_solver::get_initial_displ_vector(Eigen::VectorXd& initial_displVector,
	std::unordered_map<int, gyronode_store*> g_nodes)
{
	// Create initial dispalcement vector based on mode shapes number
	const int mode_number = 3;

	// Find the angle of free ring nodes

	for (auto& node_m : g_nodes)
	{
		gyronode_store* node = node_m.second;

		// Get the Node ID 
		int nd_map = nodeid_map[node->gnode_id]; // get the ordered map of the start node ID


		// Get the node point
		if (node->isFixed == false)
		{
			// Get the node point
			glm::vec2 node_pt = node->gnode_pt;

			// Free circular nodes
			// 1. Find the angle of the node
			double nd_angle = std::atan2(node_pt.y, node_pt.x);

			// convert [0 to pi, -pi to 0] --> [0, 2pi]
			if (nd_angle < 0.0)
			{
				nd_angle = 2.0 * M_PI + nd_angle;
			}


			// 2. Find the amplitude magnitude based on mode shape
			double ampl_mag = std::sin(mode_number * nd_angle);

			// 3. Find the vector of node
			// glm::vec2 norm_node_pt = static_cast<float>(ampl_mag) * glm::normalize(node_pt);
			glm::vec2 norm_node_pt = glm::vec2(0.0, 1.0);



			// Add to the displacement vector
			initial_displVector(nd_map * 2) = norm_node_pt.x;
			initial_displVector((nd_map * 2) + 1) = norm_node_pt.y;

		}
		else
		{
			// Fixed nodes 
			initial_displVector(nd_map * 2) = 0.0;
			initial_displVector((nd_map * 2) + 1) = 0.0;
		}
		//
	}
	//
}




void penalty_dynamics_solver::print_matrices()
{
	std::ofstream output_file;
	output_file.open("fe_matrices_penalty.txt");

	// Print the Global Mass matrix
	output_file << "Global Mass Matrix" << std::endl;
	output_file << globalMassMatrix << std::endl;
	output_file << std::endl;

	// Print the Global Inverse Mass matrix
	output_file << "Global Inverse Mass Matrix" << std::endl;
	output_file << inverse_globalMassMatrix << std::endl;
	output_file << std::endl;

	// Print the Global Penalty Augmented Stiffness matrix
	output_file << "Global Penalty Augmented Stiffness Matrix" << std::endl;
	output_file << globalPenaltyAugmentedStiffnessMatrix << std::endl;
	output_file << std::endl;

	//// Print the Damping C matrix
	//output_file << "Global Damping C Matrix" << std::endl;
	//output_file << dampingCMatrix << std::endl;
	//output_file << std::endl;


	// Print the Initial Displacement matrix
	output_file << "Global Inital Displacement Vector" << std::endl;
	output_file << initial_displVector << std::endl;
	output_file << std::endl;



	//dampingCMatrix,
	//	initial_displVector,
	//	initial_veloVector,
	//	forceVector

	output_file.close();

}