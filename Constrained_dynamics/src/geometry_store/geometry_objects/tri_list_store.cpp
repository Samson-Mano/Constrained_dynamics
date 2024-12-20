#include "tri_list_store.h"

tri_list_store::tri_list_store()
{
	// Empty constructor
}

tri_list_store::~tri_list_store()
{
	// Empty destructor
}

void tri_list_store::init(geom_parameters* geom_param_ptr)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;

	// Create the point shader
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	tri_shader.create_shader((shadersPath.string() + "/resources/shaders/cnsrpoint_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/cnsrpoint_frag_shader.frag").c_str());

	tri_shader.setUniform("triColor", geom_param_ptr->geom_colors.triangle_color);

	// Delete all the triangles
	tri_count = 0;
	triMap.clear();
}

void tri_list_store::add_tri(int& tri_id, const glm::vec2& tript1_loc, const glm::vec2& tript2_loc, const glm::vec2& tript3_loc)
{
	// Create a temporary points
	tri_store temp_tri;
	temp_tri.tri_id = tri_id;

	// Boundary Node points
	temp_tri.tript1_loc = tript1_loc;
	temp_tri.tript2_loc = tript2_loc;
	temp_tri.tript3_loc = tript3_loc;

	//___________________________________________________________________
	// Add to the list
	triMap.push_back(temp_tri);

	// Iterate the point count
	tri_count++;
}


void tri_list_store::set_buffer()
{
	// Define the tri vertices of the model for a node (2 position) 
	const unsigned int tri_vertex_count = 2 * 3 * tri_count;
	float* tri_vertices = new float[tri_vertex_count];

	unsigned int tri_indices_count = 3 * tri_count; // 3 indices to form a triangle
	unsigned int* tri_vertex_indices = new unsigned int[tri_indices_count];

	unsigned int tri_v_index = 0;
	unsigned int tri_i_index = 0;

	// Set the tri vertices
	for (auto& tri : triMap)
	{
		// Add triangle buffers
		get_line_buffer(tri, tri_vertices, tri_v_index, tri_vertex_indices, tri_i_index);
	}

	VertexBufferLayout tri_pt_layout;
	tri_pt_layout.AddFloat(2);  // Node center

	unsigned int tri_vertex_size = tri_vertex_count * sizeof(float); // Size of the node_vertex

	// Create the triangle buffers
	tri_buffer.CreateBuffers(tri_vertices, tri_vertex_size, tri_vertex_indices, tri_indices_count, tri_pt_layout);

	// Delete the dynamic array
	delete[] tri_vertices;
	delete[] tri_vertex_indices;
}

void tri_list_store::paint_triangles()
{
	// Paint all the triangles
	tri_shader.Bind();
	tri_buffer.Bind();
	glDrawElements(GL_TRIANGLES, (3 * tri_count), GL_UNSIGNED_INT, 0);
	tri_buffer.UnBind();
	tri_shader.UnBind();
}

void tri_list_store::clear_triangles()
{
	// Delete all the triangles
	tri_count = 0;
	triMap.clear();
}

void tri_list_store::update_opengl_uniforms(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	if (set_modelmatrix == true)
	{
		// set the model matrix
		tri_shader.setUniform("geom_scale", static_cast<float>(geom_param_ptr->geom_scale));
		tri_shader.setUniform("transparency", 0.8f);

		tri_shader.setUniform("modelMatrix", geom_param_ptr->modelMatrix, false);
	}

	if (set_pantranslation == true)
	{
		// set the pan translation
		tri_shader.setUniform("panTranslation", geom_param_ptr->panTranslation, false);
	}

	if (set_zoomtranslation == true)
	{
		// set the zoom translation
		tri_shader.setUniform("zoomscale", static_cast<float>(geom_param_ptr->zoom_scale));
	}

	if (set_transparency == true)
	{
		// set the alpha transparency
		tri_shader.setUniform("transparency", static_cast<float>(geom_param_ptr->geom_transparency));
	}

	if (set_deflscale == true)
	{
		// set the deflection scale
		tri_shader.setUniform("normalized_deflscale", static_cast<float>(geom_param_ptr->normalized_defl_scale));
		tri_shader.setUniform("deflscale", static_cast<float>(geom_param_ptr->defl_scale));
	}
}

void tri_list_store::get_line_buffer(tri_store& tri, float* tri_vertices, unsigned int& tri_v_index, unsigned int* tri_vertex_indices, unsigned int& tri_i_index)
{
	// Get the three node buffer for the shader
	// Point 1
	// Point location
	tri_vertices[tri_v_index + 0] = tri.tript1_loc.x;
	tri_vertices[tri_v_index + 1] = tri.tript1_loc.y;

	// Iterate
	tri_v_index = tri_v_index + 2;

	// Point 2
	// Point location
	tri_vertices[tri_v_index + 0] = tri.tript2_loc.x;
	tri_vertices[tri_v_index + 1] = tri.tript2_loc.y;

	// Iterate
	tri_v_index = tri_v_index + 2;

	// Point 3
	// Point location
	tri_vertices[tri_v_index + 0] = tri.tript3_loc.x;
	tri_vertices[tri_v_index + 1] = tri.tript3_loc.y;

	// Iterate
	tri_v_index = tri_v_index + 2;

	//__________________________________________________________________________
	// Add the indices
	// Index 1
	tri_vertex_indices[tri_i_index] = tri_i_index;

	tri_i_index = tri_i_index + 1;

	// Index 2
	tri_vertex_indices[tri_i_index] = tri_i_index;

	tri_i_index = tri_i_index + 1;

	// Index 3
	tri_vertex_indices[tri_i_index] = tri_i_index;

	tri_i_index = tri_i_index + 1;
}