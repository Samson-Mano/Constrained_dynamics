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
	void set_spring_geom(glm::vec2 start_pt, glm::vec2 end_pt);

	void set_buffer();
	void paint_spring_geom();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;

	std::vector<gyrospring_store*>* g_springs;
	line_list_store spring_lines;

};
