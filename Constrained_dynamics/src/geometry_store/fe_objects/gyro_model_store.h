#pragma once
#include <glm/vec2.hpp>
#include <glm/vec3.hpp>
#include <unordered_map>
#include "../geom_parameters.h"
#include "springelement_store.h"
#include "rigidelement_store.h"


struct gyronode_store
{
	int gnode_id = 0; // Node ID
	glm::vec2 gnode_pt = glm::vec2(0); // Node point

};


struct gyrospring_store
{
	int gsprg_id = 0; // Spring ID
	gyronode_store* gstart_node; // Start node
	gyronode_store* gend_node; // End node

};


struct gyrorigid_store
{
	int grigd_id = 0; // Rigid ID
	gyronode_store* gstart_node; // Start node
	gyronode_store* gend_node; // End node

};

struct gyroptmass_store
{
	int gmass_id = 0; // Mass ID
	gyronode_store* gmass_node; // Mass node

};


class gyro_model_store
{
public:
	std::unordered_map<int, gyronode_store> g_nodes;
	std::vector<gyrospring_store> g_springs;
	std::vector<gyrorigid_store> g_rigids;
	std::vector<gyroptmass_store> g_ptmass;


	gyro_model_store();
	~gyro_model_store();

	void init(geom_parameters* geom_param_ptr);

	void add_gyronodes(int& node_id, double& nd_x, double& nd_y);
	void add_gyrosprings(int& sprg_id, int& startnd_id, int& endnd_id);
	void add_gyrorigids(int& rigd_id, int& startnd_id,int& endnd_id);
	void add_gyroptmass(int& mass_id,int& mass_nd_id);

	void set_buffer();
	void paint_gyro_model();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);

private:
	geom_parameters* geom_param_ptr = nullptr;

	springelement_store spring_elements; // Spring elements
	rigidelement_store rigid_elements; // Rigid elements

};
