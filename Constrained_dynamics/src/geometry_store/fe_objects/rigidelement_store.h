#pragma once
#include "../geometry_objects/tri_list_store.h"
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"

class rigidelement_store
{
public:
	rigidelement_store();
	~rigidelement_store();

	void init(geom_parameters* geom_param_ptr);
	void add_rigid_geom(glm::vec2 start_pt, glm::vec2 end_pt);

	void set_buffer();
	void paint_rigid_geom();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;

	tri_list_store rigid_element_surfaces;

};
