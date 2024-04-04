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
	temp_tri.c_nd1 = &c_nodes[nd1];
	temp_tri.c_nd2 = &c_nodes[nd2];
	temp_tri.c_nd3 = &c_nodes[nd3];

	// Add to the triangle list
	c_triangles.push_back(temp_tri);

	//__________________________ Add the Triangle
	glm::vec2 node_pt1 = c_nodes[nd1].cnode_pt;
	glm::vec2 node_pt2 = c_nodes[nd2].cnode_pt;
	glm::vec2 node_pt3 = c_nodes[nd3].cnode_pt;

	// Main triangle
	c_surfacetris.add_tri(tri_id, node_pt1, node_pt2, node_pt3);

}

bool constrained_ring_store::is_constrained_ring_clicked(glm::vec2& screen_loc)
{
	// Check whether the constrained is clicked or not
	// Convert the nodal location to screen location
	glm::mat4 scaling_matrix = glm::mat4(1.0) * static_cast<float>(geom_param_ptr->zoom_scale);
	scaling_matrix[3][3] = 1.0f;

	glm::mat4 scaledModelMatrix = scaling_matrix * geom_param_ptr->modelMatrix;

	for (auto& tri : c_triangles)
	{
		// convert all the 3 points to screen position
		glm::vec4 nd1_screenpos = scaledModelMatrix * glm::vec4(tri.c_nd1->cnode_pt.x, tri.c_nd1->cnode_pt.y, 0, 1.0f) * geom_param_ptr->panTranslation;
		glm::vec4 nd2_screenpos = scaledModelMatrix * glm::vec4(tri.c_nd2->cnode_pt.x, tri.c_nd2->cnode_pt.y, 0, 1.0f) * geom_param_ptr->panTranslation;
		glm::vec4 nd3_screenpos = scaledModelMatrix * glm::vec4(tri.c_nd3->cnode_pt.x, tri.c_nd3->cnode_pt.y, 0, 1.0f) * geom_param_ptr->panTranslation;

		// Convert glm::vec4 to glm::vec2 by simply taking the x and y components
		glm::vec2 nd1_screenpos2D = glm::vec2(nd1_screenpos.x, nd1_screenpos.y);
		glm::vec2 nd2_screenpos2D = glm::vec2(nd2_screenpos.x, nd2_screenpos.y);
		glm::vec2 nd3_screenpos2D = glm::vec2(nd3_screenpos.x, nd3_screenpos.y);

		bool is_triangle_clicked = geom_parameters::is_triangle_clicked(screen_loc, nd1_screenpos2D, nd2_screenpos2D, nd3_screenpos2D);

		if (is_triangle_clicked == true)
		{
			// Return true if triangle is clicked
			return true;
		}
	}


	return false;
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
