#include "lagrange_dynamics_solver.h"

lagrange_dynamics_solver::lagrange_dynamics_solver()
{
	// Empty constructor

}



void lagrange_dynamics_solver::set_lagrangesolver_matrices(std::unordered_map<int, gyronode_store*> g_nodes,
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
	// Global Constraint A matrix 
	// SPC Matrix contains (Single point constraints - Pin support) and MPC matrix containt (Multi point constraints - Rigid elements)
	// Constraint matrix is resized inside the function

	set_global_constraint_A_matrix(globalConstraint_SPC_AMatrix, globalConstraint_MPC_AMatrix, numDOF,
		g_nodes, g_springs);


	// Create the global constraint A matrix
	int row_spc = globalConstraint_SPC_AMatrix.rows();
	int row_mpc = globalConstraint_MPC_AMatrix.rows();

	globalConstraint_AMatrix.resize(row_spc + row_mpc, numDOF); // where the row size is SPC A matrix + MPC A matrix


	if (row_spc > 0)
	{
		globalConstraint_AMatrix.topRows(row_spc) = globalConstraint_SPC_AMatrix;
	}

	if (row_mpc > 0)
	{
		globalConstraint_AMatrix.bottomRows(row_mpc) = globalConstraint_MPC_AMatrix;
	}


	//____________________________________________________________________________________________________________________
	// Intial displacement vector 
	initial_displVector.resize(numDOF);
	initial_displVector.setZero();

	get_initial_displ_vector(initial_displVector, g_nodes);


	//____________________________________________________________________________________________________________________
	// Intial velocity vector 
	initial_veloVector.resize(numDOF);
	initial_veloVector.setZero();

	

	//____________________________________________________________________________________________________________________
	// Intialize the Force vector (Zero at time = 0.0) 
	forceVector.resize(numDOF);
	forceVector.setZero();


	//____________________________________________________________________________________________________________________
	// Perform Lagrange Augmentation

	//____________________________________________________________________________________________________________________
	//____________________________________________________________________________________________________________________
	// Lagrange Augmentation of global stiffness matrix with global constratint A matrix
	int SPCconstraintEqnSize = globalConstraint_SPC_AMatrix.rows();
	int MPCconstraintEqnSize = globalConstraint_MPC_AMatrix.rows();

	int constraintEqnSize = SPCconstraintEqnSize + MPCconstraintEqnSize;

	int LagrageAugmentedMatrixSize = numDOF + constraintEqnSize;

	globalLagrangeAugmentedStiffnessMatrix.resize(LagrageAugmentedMatrixSize, LagrageAugmentedMatrixSize);
	globalLagrangeAugmentedStiffnessMatrix.setZero();

	// Top-left block: K
	globalLagrangeAugmentedStiffnessMatrix.topLeftCorner(numDOF, numDOF) = globalStiffnessMatrix;

	// Top-right block: A^T
	globalLagrangeAugmentedStiffnessMatrix.topRightCorner(numDOF, constraintEqnSize) = globalConstraint_AMatrix.transpose();

	// Bottom-left block: A
	globalLagrangeAugmentedStiffnessMatrix.bottomLeftCorner(constraintEqnSize, numDOF) = globalConstraint_AMatrix;

	// Bottom-right block: zero matrix (already zero-initialized)



	//____________________________________________________________________________________________________________________
	// Lagrange Augmentation of global Mass matrix with global constratint A matrix
	
	globalLagrangeAugmentedMassMatrix.resize(LagrageAugmentedMatrixSize,LagrageAugmentedMatrixSize);
	globalLagrangeAugmentedMassMatrix.setZero();

	// Top-left block: M
	globalLagrangeAugmentedMassMatrix.topLeftCorner(numDOF, numDOF) = globalMassMatrix;

	// Remaining blocks stay zero


	globalLagrangeAugmentedinverseMassMatrix.resize(LagrageAugmentedMatrixSize, LagrageAugmentedMatrixSize);
	globalLagrangeAugmentedinverseMassMatrix.setZero();

	// Top-left block: M
	globalLagrangeAugmentedinverseMassMatrix.topLeftCorner(numDOF, numDOF) = inverse_globalMassMatrix;

	// Remaining blocks stay zero





	//____________________________________________________________________________________________________________________
	// Lagrange Augmentation of global Damping matrix with global constratint A matrix

	globalLagrangeAugmentedDampingMatrix.resize(LagrageAugmentedMatrixSize, LagrageAugmentedMatrixSize);
	globalLagrangeAugmentedDampingMatrix.setZero();

	// Top-left block: M
	// globalLagrangeAugmentedDampingMatrix.topLeftCorner(numDOF, numDOF) = dampingCMatrix;

	// Remaining blocks stay zero




	//____________________________________________________________________________________________________________________
	// Lagrange Augmentation of initial displacement vector with global constratint A matrix

	globalAugmentedInitialDisplacement.resize(LagrageAugmentedMatrixSize);
	globalAugmentedInitialDisplacement.setZero();

	// top part = physical displacement
	globalAugmentedInitialDisplacement.head(numDOF) = initial_displVector;

	// bottom part = lagrange multipliers
	// already zero


		//____________________________________________________________________________________________________________________
	// Lagrange Augmentation of initial velocity vector with global constratint A matrix

	globalAugmentedInitialVelocity.resize(LagrageAugmentedMatrixSize);
	globalAugmentedInitialVelocity.setZero();

	// top part = physical velocity
	globalAugmentedInitialVelocity.head(numDOF) = initial_veloVector;

	// bottom part = lagrange multipliers
	// already zero




	//____________________________________________________________________________________________________________________
	// Lagrange Augmentation of force vector with global constratint A matrix

	globalAugmentedForceVector.resize(LagrageAugmentedMatrixSize);
	globalAugmentedForceVector.setZero();

	// top part = Force
	globalAugmentedForceVector.head(numDOF) = initial_displVector;

	// bottom part = lagrange multipliers
	// already zero




	//____________________________________________________________________________________________________________________
	// Set the solver
	n_solver.initialize_hhtsolver(globalLagrangeAugmentedMassMatrix, 
		globalLagrangeAugmentedinverseMassMatrix,
		globalLagrangeAugmentedStiffnessMatrix, 
		globalLagrangeAugmentedDampingMatrix,
		globalAugmentedInitialDisplacement,
		globalAugmentedInitialVelocity, 
		globalAugmentedForceVector);



}


void lagrange_dynamics_solver::perform_lagrange_solve(double dt)
{



}




void lagrange_dynamics_solver::get_global_stiffness_matrix(Eigen::MatrixXd& globalStiffnessMatrix, 
	std::vector<gyrospring_store*> g_springs)
{

	for (auto& spring_m : g_springs)
	{
		gyrospring_store* spring_element = spring_m;

		// Create a matrix for element striffness matrix
		Eigen::Matrix4d elementStiffnessMatrix = Eigen::Matrix4d::Zero();;

		get_element_stiffness_matrix(elementStiffnessMatrix, spring_element);

		// Get the Node ID map
		int sn_ndmap = nodeid_map[spring_element->gstart_node->gnode_id]; // get the ordered map of the start node ID
		int en_ndmap = nodeid_map[spring_element->gend_node->gnode_id]; // get the ordered map of the end node ID

		globalStiffnessMatrix.block<2, 2>(sn_ndmap * 2, sn_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(0, 0);
		globalStiffnessMatrix.block<2, 2>(sn_ndmap * 2, en_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(0, 2);
		globalStiffnessMatrix.block<2, 2>(en_ndmap * 2, sn_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(2, 0);
		globalStiffnessMatrix.block<2, 2>(en_ndmap * 2, en_ndmap * 2) += elementStiffnessMatrix.block<2, 2>(2, 2);

	}
	//
}


void lagrange_dynamics_solver::get_element_stiffness_matrix(Eigen::Matrix4d& elementStiffnessMatrix, 
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



void lagrange_dynamics_solver::get_global_mass_matrix(Eigen::MatrixXd& globalMassMatrix, 
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



void lagrange_dynamics_solver::set_global_constraint_A_matrix(Eigen::MatrixXd& globalConstraint_SPC_AMatrix,
	Eigen::MatrixXd& globalConstraint_MPC_AMatrix, int numDOF,
	std::unordered_map<int, gyronode_store*> g_nodes, std::vector<gyrospring_store*> g_springs)
{

	// Apply boundary condition using Lagrange method
	// Single point constraint (only Pinned boundary condition)
	globalConstraint_SPC_AMatrix.resize(0, numDOF); // Start with zero rows

	int currentRows = 0;


	for (auto& node_m : g_nodes)
	{
		gyronode_store* node = node_m.second;

		// Get the Node ID 
		int nd_map = nodeid_map[node->gnode_id]; // get the ordered map of the start node ID

		// Node is pinned or not
		if (node->isFixed == true)
		{
			// Single point constraint A vector
			Eigen::VectorXd SPC_AVector(numDOF);
			SPC_AVector.setZero();

			// Pinned support (Degree of Freedom 1)
			SPC_AVector[(nd_map * 2) + 0] = 1.0;
			SPC_AVector[(nd_map * 2) + 1] = 0.0;

			// **Expand A_matrix by adding a new row**
			currentRows = globalConstraint_SPC_AMatrix.rows();
			globalConstraint_SPC_AMatrix.conservativeResize(currentRows + 1, numDOF); // Add one row
			globalConstraint_SPC_AMatrix.row(currentRows) = SPC_AVector;       // Insert the new vector (X fixed)

			// Pinned support (Degree of Freedom 2)
			SPC_AVector[(nd_map * 2) + 0] = 0.0;
			SPC_AVector[(nd_map * 2) + 1] = 1.0;

			// **Expand A_matrix by adding a new row**
			currentRows = globalConstraint_SPC_AMatrix.rows();
			globalConstraint_SPC_AMatrix.conservativeResize(currentRows + 1, numDOF); // Add one row
			globalConstraint_SPC_AMatrix.row(currentRows) = SPC_AVector;       // Insert the new vector (Y fixed)

		}
		//
	}


	// Multi point constraint (Rigid link)
	globalConstraint_MPC_AMatrix.resize(0, numDOF); // Start with zero rows


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
			Eigen::VectorXd MPC_AVector(numDOF);
			MPC_AVector.setZero();

			// start node transformation
			MPC_AVector[(sn_ndmap * 2) + 0] = Lcos;
			MPC_AVector[(sn_ndmap * 2) + 1] = Msin;

			// end node transformation
			MPC_AVector[(en_ndmap * 2) + 0] = -Lcos;
			MPC_AVector[(en_ndmap * 2) + 1] = -Msin;

			// **Expand A_matrix by adding a new row**
			currentRows = globalConstraint_MPC_AMatrix.rows();
			globalConstraint_MPC_AMatrix.conservativeResize(currentRows + 1, numDOF); // Add one row
			globalConstraint_MPC_AMatrix.row(currentRows) = MPC_AVector;       // Insert the new vector

		}
		//
	}
	//
}


void lagrange_dynamics_solver::get_initial_displ_vector(Eigen::VectorXd& initial_displVector, 
	std::unordered_map<int, gyronode_store*> g_nodes)
{
	// Create initial dispalcement vector based on mode shapes number
	const int mode_number = 1;

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
			glm::vec2 norm_node_pt = static_cast<float>(ampl_mag) *  glm::normalize(node_pt);


			// Add to the displacement vector
			initial_displVector(nd_map * 2) = norm_node_pt.x;
			initial_displVector((nd_map * 2) + 1) = norm_node_pt.y;

		}
		else
		{
			// Fixed nodes 
			initial_displVector(nd_map * 2) = 0.0;
			initial_displVector((nd_map * 2) + 1)= 0.0;
		}
		//
	}
	//
}

