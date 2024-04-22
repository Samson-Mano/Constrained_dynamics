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
	cnsr_triangle_count = 0;
	c_nodepts.clear();
	c_nodes.clear();
	c_triangles.clear();

	// Create the constrained ring shader
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	cnsr_shader.create_shader((shadersPath.string() + "/resources/shaders/cnsrpoint_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/cnsrpoint_frag_shader.frag").c_str());


	cnsr_shader.setUniform("triColor", geom_param_ptr->geom_colors.triangle_color);

}

void constrained_ring_store::add_constrainednodes(int& node_id, double& nd_x, double& nd_y)
{
	// Add the node to the list
	gyronode_store temp_node;
	temp_node.gnode_id = node_id;
	temp_node.gnode_pt = glm::vec2(nd_x, nd_y);

	// Add to the node point list
	c_nodepts.push_back(temp_node.gnode_pt);

	// Add to the node list
	c_nodes.insert({ node_id, temp_node });

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
		glm::vec4 nd1_screenpos = scaledModelMatrix * glm::vec4(tri.c_nd1->gnode_pt.x, tri.c_nd1->gnode_pt.y, 0, 1.0f) * geom_param_ptr->panTranslation;
		glm::vec4 nd2_screenpos = scaledModelMatrix * glm::vec4(tri.c_nd2->gnode_pt.x, tri.c_nd2->gnode_pt.y, 0, 1.0f) * geom_param_ptr->panTranslation;
		glm::vec4 nd3_screenpos = scaledModelMatrix * glm::vec4(tri.c_nd3->gnode_pt.x, tri.c_nd3->gnode_pt.y, 0, 1.0f) * geom_param_ptr->panTranslation;

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

void constrained_ring_store::rotate_constrained_ring(const double& rotation_angle)
{
	// Rotate the constrained ring
	for (int i = 0; i < static_cast<int>(c_nodes.size()); i++)
	{
		double x = c_nodepts[i].x;
		double y = c_nodepts[i].y;

		c_nodes[i].gnode_pt.x = x * cos(rotation_angle) - y * sin(rotation_angle);
		c_nodes[i].gnode_pt.y = x * sin(rotation_angle) + y * cos(rotation_angle);
	}


	update_buffer();
}


void constrained_ring_store::rotate_constrained_ring_ends(const double& rotation_angle)
{
	// Set the rotation angle
	for (int i = 0; i < static_cast<int>(c_nodes.size()); i++)
	{
		double x = c_nodepts[i].x;
		double y = c_nodepts[i].y;

		c_nodepts[i].x = x * cos(rotation_angle) - y * sin(rotation_angle);
		c_nodepts[i].y = x * sin(rotation_angle) + y * cos(rotation_angle);

	}

}


void constrained_ring_store::set_buffer()
{
	// Set the buffers for Constrained ring Model
	// Set the number of lines
	cnsr_triangle_count = static_cast<int>(c_triangles.size()); // constrained triangle count

	// Set the spring lines buffer for the index
	unsigned int cnsr_indices_count = 3 * cnsr_triangle_count;
	unsigned int* cnsr_vertex_indices = new unsigned int[cnsr_indices_count];

	unsigned int cnsr_i_index = 0;


	for (auto& cnsr_e : c_triangles)
	{
		// Add the constrained ring surfaces (triangles) index buffer
		get_cnsr_index_buffer(cnsr_vertex_indices, cnsr_i_index);

	}

	VertexBufferLayout cnsr_layout;
	cnsr_layout.AddFloat(2);  // Position

	// Define the constrained surface vertices of model
	unsigned int cnsr_vertex_count = 2 * 3 * cnsr_triangle_count; // 3 vertices (2  float position)
	unsigned int cnsr_vertex_size = cnsr_vertex_count * sizeof(float);

	// Allocate space for the constrained ring surface buffer
	cnsr_buffer.CreateDynamicBuffers(cnsr_vertex_size,
		cnsr_vertex_indices, cnsr_indices_count, cnsr_layout);

	// Delete the Dynamic arrays
	delete[] cnsr_vertex_indices;

	// Update the buffer
	update_buffer();

}

void constrained_ring_store::update_buffer()
{
	// Update the buffer
	// Update the constrained ring buffer
	unsigned int cnsr_vertex_count = 2 * 3 * cnsr_triangle_count;  // 3 vertices (2  float position)
	float* cnsr_vertices = new float[cnsr_vertex_count];

	unsigned int cnsr_v_index = 0;

	// Update the constrained ring vertex buffer
	for (auto& cnsr_e : c_triangles)
	{
		glm::vec2 pt1 = cnsr_e.c_nd1->gnode_pt; // get the pt1
		glm::vec2 pt2 = cnsr_e.c_nd2->gnode_pt; // get the pt2
		glm::vec2 pt3 = cnsr_e.c_nd3->gnode_pt; // get the pt3

		// Add the vertex buffer of constrained surface triangles
		get_cnsr_vertex_buffer(pt1, pt2, pt3, cnsr_vertices, cnsr_v_index);

	}

	unsigned int cnsr_vertex_size = cnsr_vertex_count * sizeof(float); // size of the constrained ring vertices

	// Update the buffer
	cnsr_buffer.UpdateDynamicVertexBuffer(cnsr_vertices, cnsr_vertex_size);

	// Delete the Dynamic arrays
	delete[] cnsr_vertices;

}

void constrained_ring_store::paint_constrained_ring()
{
	// Paint the surface triangles
	cnsr_shader.Bind();
	cnsr_buffer.Bind();
	glDrawElements(GL_TRIANGLES, (3 * cnsr_triangle_count), GL_UNSIGNED_INT, 0);
	cnsr_buffer.UnBind();
	cnsr_shader.UnBind();

}

void constrained_ring_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	if (set_modelmatrix == true)
	{
		// set the model matrix
		cnsr_shader.setUniform("geom_scale", static_cast<float>(geom_param_ptr->geom_scale));
		cnsr_shader.setUniform("transparency", 1.0f);

		cnsr_shader.setUniform("modelMatrix", geom_param_ptr->modelMatrix, false);
	}

	if (set_pantranslation == true)
	{
		// set the pan translation
		cnsr_shader.setUniform("panTranslation", geom_param_ptr->panTranslation, false);
	}

	if (set_zoomtranslation == true)
	{
		// set the zoom translation
		cnsr_shader.setUniform("zoomscale", static_cast<float>(geom_param_ptr->zoom_scale));
	}

	if (set_transparency == true)
	{
		// set the alpha transparency
		cnsr_shader.setUniform("transparency", static_cast<float>(geom_param_ptr->geom_transparency));
	}

	if (set_deflscale == true)
	{
		// set the deflection scale
		cnsr_shader.setUniform("normalized_deflscale", static_cast<float>(geom_param_ptr->normalized_defl_scale));
		cnsr_shader.setUniform("deflscale", static_cast<float>(geom_param_ptr->defl_scale));
	}

}

void constrained_ring_store::get_cnsr_vertex_buffer(glm::vec2 cnsr_pt1, glm::vec2 cnsr_pt2, glm::vec2 cnsr_pt3,
	float* cnsr_vertices, unsigned int& cnsr_v_index)
{
	// Get the vertex buffer for the shader
	// Start Point
	// Point 1
	cnsr_vertices[cnsr_v_index + 0] = cnsr_pt1.x;
	cnsr_vertices[cnsr_v_index + 1] = cnsr_pt1.y;

	// Iterate
	cnsr_v_index = cnsr_v_index + 2;

	// Point 2
	// Point location
	cnsr_vertices[cnsr_v_index + 0] = cnsr_pt2.x;
	cnsr_vertices[cnsr_v_index + 1] = cnsr_pt2.y;

	// Iterate
	cnsr_v_index = cnsr_v_index + 2;

	// Point 3
	// Point location
	cnsr_vertices[cnsr_v_index + 0] = cnsr_pt3.x;
	cnsr_vertices[cnsr_v_index + 1] = cnsr_pt3.y;

	// Iterate
	cnsr_v_index = cnsr_v_index + 2;

}

void constrained_ring_store::get_cnsr_index_buffer(unsigned int* cnsr_vertex_indices, unsigned int& cnsr_i_index)
{
	//__________________________________________________________________________
	// Add the indices
	// Index 0 1 2
	cnsr_vertex_indices[cnsr_i_index + 0] = cnsr_i_index + 0;
	cnsr_vertex_indices[cnsr_i_index + 1] = cnsr_i_index + 1;
	cnsr_vertex_indices[cnsr_i_index + 2] = cnsr_i_index + 2;

	cnsr_i_index = cnsr_i_index + 3;

}