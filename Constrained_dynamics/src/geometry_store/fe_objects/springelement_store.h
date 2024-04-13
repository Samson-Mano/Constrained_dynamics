#pragma once
#include "../geometry_objects/line_list_store.h"
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"

class springelement_store
{
public:
	const int spring_turn_count = 12; // spring turn count
	int sprg_line_count = 0; // Spring line count

	springelement_store();
	~springelement_store();

	void init(geom_parameters* geom_param_ptr, std::vector<gyrospring_store*>* g_springs);

	void set_buffer();
	void update_buffer();
	void paint_spring_geom();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;
	gBuffers sprg_buffer;
	Shader sprg_shader;

	std::vector<gyrospring_store*>* g_springs;

	void get_sprg_vertex_buffer(glm::vec2 sprg_startpt, glm::vec2 sprg_endpt,
		float* sprg_vertices, unsigned int& sprg_v_index);

	void get_sprg_index_buffer(unsigned int* sprg_vertex_indices, unsigned int& sprg_i_index);
};
