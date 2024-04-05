#pragma once
#include <glm/vec2.hpp>
#include <glm/vec3.hpp>
#include <unordered_map>
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"
#include "../geometry_objects/tri_list_store.h"
#include "../geometry_objects/point_list_store.h"


struct constrainednode_store
{
	int cnode_id = 0; // Node ID
	glm::vec2 cnode_pt = glm::vec2(0); // Node point
	
};


struct constrainedtri_store
{
	int ctri_id = 0; // ID of the triangle element
	constrainednode_store* c_nd1; // node 1
	constrainednode_store* c_nd2; // node 2
	constrainednode_store* c_nd3; // node 3
};



class constrained_ring_store
{
public:
	std::unordered_map<int, constrainednode_store> c_nodes;
	std::vector<constrainedtri_store> c_triangles;

	constrained_ring_store();
	~constrained_ring_store();

	void init(geom_parameters* geom_param_ptr);

	void add_constrainednodes(int& node_id,double& nd_x, double& nd_y);
	void add_constrainedtris(int& tri_id, int& nd1, int& nd2, int& nd3);

	bool is_constrained_ring_clicked(glm::vec2& screen_loc);
	void rotate_constrained_ring(const double& rotation_angle);

	void set_buffer();
	void paint_constrained_ring();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;

	tri_list_store c_surfacetris;

};
