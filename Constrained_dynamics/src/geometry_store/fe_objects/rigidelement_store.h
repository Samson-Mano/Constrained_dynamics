#pragma once
#include "../geometry_objects/tri_list_store.h"
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"

class rigidelement_store
{
public:
	int rigd_surf_count = 0; // Rigid line count

	rigidelement_store();
	~rigidelement_store();


	void init(geom_parameters* geom_param_ptr, std::vector<gyrorigid_store*>* g_rigids);

	void set_buffer();
	void update_buffer();
	void paint_rigid_geom();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;
	gBuffers rigd_buffer;
	Shader rigd_shader;

	std::vector<gyrorigid_store*>* g_rigids;

	void get_rigd_vertex_buffer(glm::vec2 rigd_startpt, glm::vec2 rigd_endpt,
		float* rigd_vertices, unsigned int& rigd_v_index);

	void get_rigd_index_buffer(unsigned int* rigd_vertex_indices, unsigned int& rigd_i_index);

};
