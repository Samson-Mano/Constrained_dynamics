#pragma once
#include "../geometry_objects/tri_list_store.h"
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"

class masselement_store
{
public:
	std::vector<glm::vec2> pt_mass_locations; // point mass location
	int ptmass_count = 0; // point mass count

	masselement_store();
	~masselement_store();

	void init(geom_parameters* geom_param_ptr);
	void add_ptmass_geom(glm::vec2 ptmass_pt);

	void set_buffer();
	void paint_ptmass_geom();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);

private:
	geom_parameters* geom_param_ptr = nullptr;
	gBuffers ptmass_buffer;
	Shader ptmass_shader;
	Texture ptmass_texture;


	void get_constraint_buffer(glm::vec2& ptm_loc, float* ptmass_vertices, unsigned int& ptmass_v_index,
		unsigned int* ptmass_indices, unsigned int& ptmass_i_index);
};