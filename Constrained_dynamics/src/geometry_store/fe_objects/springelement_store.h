#pragma once
#include "../geometry_objects/line_list_store.h"
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"

class springelement_store
{
public:
	const int spring_turn_count = 12;
	springelement_store();
	~springelement_store();

	void init(geom_parameters* geom_param_ptr, std::vector<gyrospring_store*>* g_springs);

	void set_buffer();
	void paint_spring_geom();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;
	gBuffers sprg_buffer;
	Shader sprg_shader;


	std::vector<gyrospring_store*>* g_springs;
	line_list_store spring_lines;

	void get_line_vertex_buffer(dynamic_line_store& ln, const int& dyn_index,
		float* dyn_line_vertices, unsigned int& dyn_line_v_index);

	void get_line_index_buffer(unsigned int* dyn_line_vertex_indices, unsigned int& dyn_line_i_index);
};
