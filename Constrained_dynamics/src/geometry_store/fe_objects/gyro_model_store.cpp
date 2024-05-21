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

	temp_spring->alpha_i = (1.0 / (delta_t * delta_t * spring_stiff));
	temp_spring->beta_i = (delta_t * delta_t * spring_damp);
	temp_spring->gamma_i = (temp_spring->alpha_i * temp_spring->beta_i) / delta_t;

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
	
	temp_rigid->alpha_i = 0.0;
	temp_rigid->beta_i = 0.0;
	temp_rigid->gamma_i = 0.0;

	// Add to the rigid list
	g_springs.push_back(temp_rigid);

}

void gyro_model_store::add_gyroptmass(int& mass_id, int& mass_nd_id, double& ptmass_value)
{
	// Update the point mass value
	g_nodes[mass_nd_id]->isPtmassexist = true;
	g_nodes[mass_nd_id]->gmass_value = ptmass_value;

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

void gyro_model_store::run_simulation(double time_t)
{
	// Run the simulation
	// Step 1: Time integration (Velocity and Displacement)
	for (int i = 0; i < static_cast<int>(g_nodes.size()); i++)
	{
		// Get the node
		gyronode_store* nd = g_nodes[i];

		// Acceleration vector
		glm::vec2 accl_vec = glm::vec2(0);
		
		if (nd->gnode_id == 38)
		{
			// Force is applied to only node at 
			accl_vec = static_cast<float>(get_acceleration_at_t(time_t)) * nd->gnode_normal;
		}
		

		// Velocity update v^ = v(t) + delta_t * accl
		g_nodes[i]->gnode_velo_hat = g_nodes[i]->gnode_velo + (static_cast<float>(delta_t) * accl_vec);

		// Update position x^ = x(t) + delta_t * v^
		g_nodes[i]->gnode_displ_hat = g_nodes[i]->gnode_pt + (static_cast<float>(delta_t) * g_nodes[i]->gnode_velo_hat);
	}


	// Step 2: Solver loop
	for (int j = 0; j < 100; j++)
	{
		// Cycle through all the Spring element
		for (int i = 0; i < static_cast<int>(g_springs.size()); i++)
		{
			// Transform the Global co-ordinate to local co-ordinate
			// Node 1 x,y coordinate
			double x1 = g_springs[i]->gstart_node->gnode_pt.x;
			double y1 = g_springs[i]->gstart_node->gnode_pt.y;

			// Node 2 x,y coordinate
			double x2 = g_springs[i]->gend_node->gnode_pt.x;
			double y2 = g_springs[i]->gend_node->gnode_pt.y;

			// Length of the element
			double l_element = std::sqrt(std::pow(x2 - x1, 2) + std::pow(y2 - y1, 2));

			// Direction cosines
			double l_cos = (x2 - x1) / l_element;
			double m_sin = (y1 - y2) / l_element;

			// Local co-ordinate Node 1 & 2
			double l1 = (l_cos * x1) + (m_sin * y1);
			double l2 = (l_cos * x2) + (m_sin * y2);

			// 2.1 Compute Lagrange Multipliers
			double cnstraint_x = l1 + l2;



			// 2.2 Constraint Gradients


		}
	}


}

double gyro_model_store::get_acceleration_at_t(const double& time_t)
{
	// get the acceleration at time t
	// accl_freq = 2.0; // Acceleration frequency

	return 1.0 * std::sin(time_t * 2.0 * (geom_param_ptr->mPI) * accl_freq);
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
