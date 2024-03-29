#include "constrained_ring_store.h"

constrained_ring_store::constrained_ring_store()
{
	// Empty constructor
}

constrained_ring_store::~constrained_ring_store()
{
	// Empty destructor
}

void constrained_ring_store::init(geom_parameters* geom_param_ptr)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;

	// Clear the surface
	c_nodes.clear();
	c_triangles.clear();

	// Constrained surface triangles
	c_surfacetris.init(geom_param_ptr);
	c_surfacetris.clear_triangles();

}

void constrained_ring_store::add_constrainednodes(int& node_id, double& nd_x, double& nd_y)
{
	// Add the node to the list
	constrainednode_store temp_node;
	temp_node.cnode_id = node_id;
	temp_node.cnode_pt = glm::vec2(nd_x,nd_y);

	// Add to the node list
	c_nodes.insert({ node_id, temp_node});

}

void constrained_ring_store::add_constrainedtris(int& tri_id, int& nd1, int& nd2, int& nd3)
{
	constrainedtri_store temp_tri;
	temp_tri.ctri_id = tri_id;
	temp_tri.c_nd1 = c_nodes[nd1];
	temp_tri.c_nd2 = c_nodes[nd2];
	temp_tri.c_nd3 = c_nodes[nd3];

	// Add to the triangle list
	c_triangles.push_back(temp_tri);

	//__________________________ Add the Triangle
	glm::vec2 node_pt1 = c_nodes[nd1].cnode_pt;
	glm::vec2 node_pt2 = c_nodes[nd2].cnode_pt;
	glm::vec2 node_pt3 = c_nodes[nd3].cnode_pt;

	// Main triangle
	c_surfacetris.add_tri(tri_id, node_pt1, node_pt2, node_pt3);

}

void constrained_ring_store::set_buffer()
{
	// Set the buffers for the Model
	c_surfacetris.set_buffer();

}

void constrained_ring_store::paint_constrained_ring()
{
	// Paint the surface triangles
	c_surfacetris.paint_triangles();

}

void constrained_ring_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	c_surfacetris.update_opengl_uniforms(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);

}
