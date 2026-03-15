#include "gyro_model_store.h"

gyro_model_store::gyro_model_store()
{
	// Empty constructor

}

gyro_model_store::~gyro_model_store()
{
	// Empty destructor

}

void gyro_model_store::init(geom_parameters* geom_param_ptr)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;

	// Clear the gyro model
	g_nodes.clear();
	g_springs.clear();

	// Gyro model
	spring_elements.init(geom_param_ptr, &g_springs);
	rigid_elements.init(geom_param_ptr, &g_springs);
	mass_elements.init(geom_param_ptr, &g_nodes);
}

void gyro_model_store::add_gyronodes(int& node_id, double& nd_x, double& nd_y)
{
	// Add the node to the list
	gyronode_store* temp_node = new gyronode_store;
	temp_node->gnode_id = node_id;
	temp_node->gnode_pt = glm::vec2(nd_x, nd_y);
	temp_node->gnode_normal = glm::normalize(temp_node->gnode_pt); // Normal to the vector
	temp_node->isFixed = true; // Set all the nodes as fixed (change the fixed to free at point mass addition)

	// Add to the node point list
	g_nodepts.push_back(temp_node->gnode_pt);

	// Add to the node list
	g_nodes.insert({ node_id, temp_node });

}

void gyro_model_store::add_gyrosprings(int& sprg_id, int& startnd_id, int& endnd_id)
{
	gyrospring_store* temp_spring = new gyrospring_store;
	temp_spring->gsprg_id = sprg_id;
	temp_spring->gstart_node = g_nodes[startnd_id];
	temp_spring->gend_node = g_nodes[endnd_id];
	temp_spring->is_rigid = false;

	// Rest length
	double x1 = g_nodes[startnd_id]->gnode_pt.x;
	double y1 = g_nodes[startnd_id]->gnode_pt.y;

	double x2 = g_nodes[endnd_id]->gnode_pt.x;
	double y2 = g_nodes[endnd_id]->gnode_pt.y;

	temp_spring->rest_length = std::sqrt(std::pow(x2 - x1, 2) + std::pow(y2 - y1, 2));
	//-----------------------------------------------------------------

	temp_spring->stiffeness_K = spring_stiff;


	// Add to the spring list
	g_springs.push_back(temp_spring);

}

void gyro_model_store::add_gyrorigids(int& rigd_id, int& startnd_id, int& endnd_id)
{
	gyrospring_store* temp_rigid = new gyrospring_store;
	temp_rigid->gsprg_id = rigd_id;
	temp_rigid->gstart_node = g_nodes[startnd_id];
	temp_rigid->gend_node = g_nodes[endnd_id];
	temp_rigid->is_rigid = true;
	
	// Rest length
	double x1 = g_nodes[startnd_id]->gnode_pt.x;
	double y1 = g_nodes[startnd_id]->gnode_pt.y;

	double x2 = g_nodes[endnd_id]->gnode_pt.x;
	double y2 = g_nodes[endnd_id]->gnode_pt.y;

	temp_rigid->rest_length = std::sqrt(std::pow(x2 - x1, 2) + std::pow(y2 - y1, 2));
	//-----------------------------------------------------------------

	// Add to the rigid list
	g_springs.push_back(temp_rigid);

}

void gyro_model_store::add_gyroptmass(int& mass_id, int& mass_nd_id, double& ptmass_value)
{
	// Update the point mass value
	g_nodes[mass_nd_id]->isPtmassexist = true;
	g_nodes[mass_nd_id]->isFixed = false;
	g_nodes[mass_nd_id]->gmass_value = ptmass_value;

}


void gyro_model_store::set_matrices()
{
	// Set the matrices of the default state

	// Lagrange Dynamics Solver
	ld_solver.set_lagrangesolver_matrices(g_nodes, g_springs);

	// Penalty Dynamics Solver
	pd_solver.set_penaltysolver_matrices(g_nodes, g_springs);

}



void gyro_model_store::rotate_gyro_model(const double& rotation_angle)
{
	// Rotate the gyro ring
	for (int i = 0; i < static_cast<int>(g_nodes.size()); i++)
	{
		double x = g_nodepts[i].x;
		double y = g_nodepts[i].y;

		// Update the node point
		g_nodes[i]->gnode_pt.x = x * cos(rotation_angle) - y * sin(rotation_angle);
		g_nodes[i]->gnode_pt.y = x * sin(rotation_angle) + y * cos(rotation_angle);

		// Node normal vector towards origin
		g_nodes[i]->gnode_normal = glm::normalize(g_nodes[i]->gnode_pt);
	}

	// Update the buffer
	mass_elements.update_buffer();
	spring_elements.update_buffer();
	rigid_elements.update_buffer();

}


void gyro_model_store::rotate_gyro_model_ends(const double& rotation_angle)
{
	// Set the rotation angle
	for (int i = 0; i < static_cast<int>(g_nodes.size()); i++)
	{
		double x = g_nodepts[i].x;
		double y = g_nodepts[i].y;

		g_nodepts[i].x = x * cos(rotation_angle) - y * sin(rotation_angle);
		g_nodepts[i].y = x * sin(rotation_angle) + y * cos(rotation_angle);

	}

}

void gyro_model_store::run_simulation(double delta_t)
{



	// Update the buffer
	mass_elements.update_buffer();
	spring_elements.update_buffer();
	rigid_elements.update_buffer();

}


double gyro_model_store::get_acceleration_at_t(const double& time_t)
{
	// get the acceleration at time t
	// accl_freq = 2.0; // Acceleration frequency

	return 100.0 * std::sin(time_t * 2.0 * (geom_param_ptr->mPI) * accl_freq);
}


void gyro_model_store::set_buffer()
{
	// Create the Rigid element geometry
	rigid_elements.set_buffer();

	// Create the Spring element geometry
	spring_elements.set_buffer();

	// Create the Mass element geometry
	mass_elements.set_buffer();

}

void gyro_model_store::paint_gyro_model()
{
	// Paint the gyro model
	spring_elements.paint_spring_geom();
	rigid_elements.paint_rigid_geom();
	mass_elements.paint_ptmass_geom();

}

void gyro_model_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	spring_elements.update_geometry_matrices(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);
	rigid_elements.update_geometry_matrices(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);
	mass_elements.update_geometry_matrices(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);

}
