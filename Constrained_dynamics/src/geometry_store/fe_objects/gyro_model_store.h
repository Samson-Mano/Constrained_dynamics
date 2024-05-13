#pragma once
#include <glm/vec2.hpp>
#include <glm/vec3.hpp>
#include <unordered_map>
#include "../geom_parameters.h"
#include "springelement_store.h"
#include "rigidelement_store.h"
#include "masselement_store.h"

class gyro_model_store
{
public:
	std::vector<glm::vec2> g_nodepts;
	std::unordered_map<int, gyronode_store> g_nodes;
	std::vector<gyrospring_store*> g_springs;
	std::vector<gyrorigid_store*> g_rigids;
	std::vector<gyroptmass_store*> g_ptmass;


	gyro_model_store();
	~gyro_model_store();

	void init(geom_parameters* geom_param_ptr);

	void add_gyronodes(int& node_id, double& nd_x, double& nd_y);
	void add_gyrosprings(int& sprg_id, int& startnd_id, int& endnd_id);
	void add_gyrorigids(int& rigd_id, int& startnd_id,int& endnd_id);
	void add_gyroptmass(int& mass_id,int& mass_nd_id);

	void rotate_gyro_model(const double& rotation_angle);
	void rotate_gyro_model_ends(const double& rotation_angle);

	// Simulate the run
	void run_simulation(double time_t);
	double get_acceleration_at_t(const double& time_t);

	void set_buffer();
	void paint_gyro_model();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);

private:
	const double delta_t = 0.001; // delta time
	double time_at = 0.0; // time t
	double accl_freq = 1.0; // Acceleration frequency
	double spring_stiff = 1000; // Spring stiffness

	geom_parameters* geom_param_ptr = nullptr;

	springelement_store spring_elements; // Spring elements
	rigidelement_store rigid_elements; // Rigid elements
	masselement_store mass_elements; // Mass elements

};
