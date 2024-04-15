#include "rigidelement_store.h"

rigidelement_store::rigidelement_store()
{
	// Empty constructor

}

rigidelement_store::~rigidelement_store()
{
	// Empty destructor
}
void rigidelement_store::init(geom_parameters* geom_param_ptr, std::vector<gyrorigid_store*>* g_rigids)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;
	this->g_rigids = g_rigids;
	rigd_line_count = 0;

	// Create the point shader
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	rigd_shader.create_shader((shadersPath.string() + "/resources/shaders/sprg_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/sprg_frag_shader.frag").c_str());

}

void rigidelement_store::set_buffer()
{
	// Set the number of lines
	rigd_line_count = static_cast<int>((*g_rigids).size()); // 2 triangle surface for every single rigid line


	// Set the rigid lines buffer for the index
	unsigned int rigd_indices_count = 6 * rigd_line_count; // 6 indices for single rigid line (3  + 3 for 2 triangles)
	unsigned int* rigd_vertex_indices = new unsigned int[rigd_indices_count];

	unsigned int rigd_i_index = 0;


	for (auto& rigd_e : *g_rigids)
	{
		// Add the rigid element index buffer
		get_rigd_index_buffer(rigd_vertex_indices, rigd_i_index);

	}

	VertexBufferLayout rigd_layout;
	rigd_layout.AddFloat(2);  // Position
	rigd_layout.AddFloat(1);  // Defl

	// Define the Rigid surface vertices of model
	unsigned int rigd_vertex_count = 4 * 3 * rigd_line_count;  // 4 points per rigid line (2 + 1)
	unsigned int rigd_vertex_size = rigd_vertex_count * sizeof(float);

	// Allocate space for the spring vertex buffer
	rigd_buffer.CreateDynamicBuffers(rigd_vertex_size,
		rigd_vertex_indices, rigd_indices_count, rigd_layout);

	// Delete the Dynamic arrays
	delete[] rigd_vertex_indices;


	// Update the buffer for the spring lines points
	update_buffer();

}



void rigidelement_store::update_buffer()
{
	// Update the buffer
	// Update the rigid element buffer
	unsigned int rigd_vertex_count = 4 * 3 * rigd_line_count;  // 4 points per rigid line (2 + 1)
	float* rigd_vertices = new float[rigd_vertex_count];

	unsigned int rigd_v_index = 0;

	// Update the rigid element vertex buffer
	for (auto& rigd_e : *g_rigids)
	{
		glm::vec2 start_pt = rigd_e->gstart_node->gnode_pt; // get the start pt
		glm::vec2 end_pt = rigd_e->gend_node->gnode_pt; // get the end pt

		// Add the vertex buffer of rigid quad
		get_rigd_vertex_buffer(start_pt, end_pt, rigd_vertices, rigd_v_index);

	}

	unsigned int rigd_vertex_size = rigd_vertex_count * sizeof(float); // size of the rigid element vertices

	// Update the buffer
	rigd_buffer.UpdateDynamicVertexBuffer(rigd_vertices, rigd_vertex_size);

	// Delete the Dynamic arrays
	delete[] rigd_vertices;

}


void rigidelement_store::paint_rigid_geom()
{
	// Paint the rigid triangles (surfaces)
	rigd_shader.Bind();
	rigd_buffer.Bind();
	glDrawElements(GL_TRIANGLES, (6 * rigd_line_count), GL_UNSIGNED_INT, 0);
	rigd_buffer.UnBind();
	rigd_shader.UnBind();

}

void rigidelement_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	if (set_modelmatrix == true)
	{
		// set the model matrix
		rigd_shader.setUniform("geom_scale", static_cast<float>(geom_param_ptr->geom_scale));
		rigd_shader.setUniform("transparency", 1.0f);

		rigd_shader.setUniform("modelMatrix", geom_param_ptr->modelMatrix, false);
	}

	if (set_pantranslation == true)
	{
		// set the pan translation
		rigd_shader.setUniform("panTranslation", geom_param_ptr->panTranslation, false);
	}

	if (set_zoomtranslation == true)
	{
		// set the zoom translation
		rigd_shader.setUniform("zoomscale", static_cast<float>(geom_param_ptr->zoom_scale));
	}

	if (set_transparency == true)
	{
		// set the alpha transparency
		rigd_shader.setUniform("transparency", static_cast<float>(geom_param_ptr->geom_transparency));
	}

	if (set_deflscale == true)
	{
		// set the deflection scale
		rigd_shader.setUniform("normalized_deflscale", static_cast<float>(geom_param_ptr->normalized_defl_scale));
		rigd_shader.setUniform("deflscale", static_cast<float>(geom_param_ptr->defl_scale));
	}

}


void rigidelement_store::get_rigd_vertex_buffer(glm::vec2 rigd_startpt, glm::vec2 rigd_endpt,
	float* rigd_vertices, unsigned int& rigd_v_index)
{

	// Add the Rigid line as surface with 2 triangles
	// Line length
	double element_length = geom_parameters::get_line_length(rigd_startpt, rigd_endpt);

	// Direction cosines
	double l_cos = (rigd_endpt.x - rigd_startpt.x) / element_length; // l cosine
	double m_sin = (rigd_startpt.y - rigd_endpt.y) / element_length; // m sine

	double rigid_width_amplitude = geom_param_ptr->rigid_element_width *
		(geom_param_ptr->node_circle_radii / geom_param_ptr->geom_scale);

	// Half-width vector
	glm::vec2 half_width_vector = glm::vec2(m_sin, l_cos) * static_cast<float>(rigid_width_amplitude / 2.0f);

	// Four points to form the rigid element rectangle
	glm::vec2 point1 = rigd_startpt + half_width_vector; // Upper left 
	glm::vec2 point2 = rigd_startpt - half_width_vector; // Lower left
	glm::vec2 point3 = rigd_endpt + half_width_vector;   // Upper right
	glm::vec2 point4 = rigd_endpt - half_width_vector;   // Lower right


	// Get the vertex buffer for the shader
	// Point 1
	// Point location
	rigd_vertices[rigd_v_index + 0] = point1.x;
	rigd_vertices[rigd_v_index + 1] = point1.y;

	// Point color
	rigd_vertices[rigd_v_index + 2] = 0;

	// Iterate
	rigd_v_index = rigd_v_index + 3;

	// Point 2
	// Point location
	rigd_vertices[rigd_v_index + 0] = point2.x;
	rigd_vertices[rigd_v_index + 1] = point2.y;

	// Point color
	rigd_vertices[rigd_v_index + 2] = 0.0;

	// Iterate
	rigd_v_index = rigd_v_index + 3;

	// Point 3
	// Point location
	rigd_vertices[rigd_v_index + 0] = point3.x;
	rigd_vertices[rigd_v_index + 1] = point3.y;

	// Point color
	rigd_vertices[rigd_v_index + 2] = 0.0;

	// Iterate
	rigd_v_index = rigd_v_index + 3;

	// Point 4
	// Point location
	rigd_vertices[rigd_v_index + 0] = point4.x;
	rigd_vertices[rigd_v_index + 1] = point4.y;

	// Point color
	rigd_vertices[rigd_v_index + 2] = 0.0;

	// Iterate
	rigd_v_index = rigd_v_index + 3;

}

void rigidelement_store::get_rigd_index_buffer(unsigned int* rigd_vertex_indices, unsigned int& rigd_i_index)
{
	//__________________________________________________________________________
	// Add the indices
	// Index 0 1 2
	rigd_vertex_indices[rigd_i_index + 0] = static_cast<int>((rigd_i_index / 6.0) * 4.0) + 0;
	rigd_vertex_indices[rigd_i_index + 1] = static_cast<int>((rigd_i_index / 6.0) * 4.0) + 1;
	rigd_vertex_indices[rigd_i_index + 2] = static_cast<int>((rigd_i_index / 6.0) * 4.0) + 2;

	// Index 2 3 0
	rigd_vertex_indices[rigd_i_index + 3] = static_cast<int>((rigd_i_index / 6.0) * 4.0) + 2;
	rigd_vertex_indices[rigd_i_index + 4] = static_cast<int>((rigd_i_index / 6.0) * 4.0) + 3;
	rigd_vertex_indices[rigd_i_index + 5] = static_cast<int>((rigd_i_index / 6.0) * 4.0) + 0;


	rigd_i_index = rigd_i_index + 6;

}

