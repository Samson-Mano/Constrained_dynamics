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
	g_rigids.clear();
	g_ptmass.clear();

	// Gyro model
	spring_elements.init(geom_param_ptr);
	rigid_elements.init(geom_param_ptr);
}

void gyro_model_store::add_gyronodes(int& node_id, double& nd_x, double& nd_y)
{
	// Add the node to the list
	gyronode_store temp_node;
	temp_node.gnode_id = node_id;
	temp_node.gnode_pt = glm::vec2(nd_x, nd_y);

	// Add to the node list
	g_nodes.insert({ node_id, temp_node });

}

void gyro_model_store::add_gyrosprings(int& sprg_id, int& startnd_id, int& endnd_id)
{
	gyrospring_store temp_spring;
	temp_spring.gsprg_id = sprg_id;
	temp_spring.gstart_node = &g_nodes[startnd_id];
	temp_spring.gend_node = &g_nodes[endnd_id];

	// Add to the spring list
	g_springs.push_back(temp_spring);

}

void gyro_model_store::add_gyrorigids(int& rigd_id, int& startnd_id, int& endnd_id)
{
	gyrorigid_store temp_rigid;
	temp_rigid.grigd_id = rigd_id;
	temp_rigid.gstart_node = &g_nodes[startnd_id];
	temp_rigid.gend_node = &g_nodes[endnd_id];

	// Add to the rigid list
	g_rigids.push_back(temp_rigid);

}

void gyro_model_store::add_gyroptmass(int& mass_id, int& mass_nd_id)
{
}

void gyro_model_store::set_buffer()
{
	// Create the Rigid element geometry
	for (auto& relm : g_rigids)
	{
		rigid_elements.add_rigid_geom(relm.gstart_node->gnode_pt, relm.gend_node->gnode_pt);
	}
	rigid_elements.set_buffer();

	// Create the Spring element geometry
	for (auto& selm : g_springs)
	{
		spring_elements.add_spring_geom(selm.gstart_node->gnode_pt, selm.gend_node->gnode_pt);
	}
	spring_elements.set_buffer();

	// Create the Mass element geometry

}

void gyro_model_store::paint_gyro_model()
{
	// Paint the gyro model
	spring_elements.paint_spring_geom();
	rigid_elements.paint_rigid_geom();
}

void gyro_model_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	spring_elements.update_geometry_matrices(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);
	rigid_elements.update_geometry_matrices(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);

}
