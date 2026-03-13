#include "lagrange_dynamics_solver.h"

lagrange_dynamics_solver::lagrange_dynamics_solver()
{
	// Empty constructor

}



void lagrange_dynamics_solver::set_matrices(std::unordered_map<int, gyronode_store*> g_nodes,
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



	//____________________________________________________________________________________________________________________
	// Global Mass Matrix
	globalMassMatrix.resize(numDOF, numDOF);
	globalMassMatrix.setZero();





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

		// Get the Node ID 
		int sn_id = nodeid_map[spring_element->gstart_node->gnode_id]; // get the ordered map of the start node ID
		int en_id = nodeid_map[spring_element->gend_node->gnode_id]; // get the ordered map of the end node ID

		globalStiffnessMatrix.block<2, 2>(sn_id * 2, sn_id * 2) += elementStiffnessMatrix.block<2, 2>(0, 0);
		globalStiffnessMatrix.block<2, 2>(sn_id * 2, en_id * 2) += elementStiffnessMatrix.block<2, 2>(0, 2);
		globalStiffnessMatrix.block<2, 2>(en_id * 2, sn_id * 2) += elementStiffnessMatrix.block<2, 2>(2, 0);
		globalStiffnessMatrix.block<2, 2>(en_id * 2, en_id * 2) += elementStiffnessMatrix.block<2, 2>(2, 2);

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

