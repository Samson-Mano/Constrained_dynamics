#include "springelement_store.h"

springelement_store::springelement_store()
{
	// Empty constructor

}

springelement_store::~springelement_store()
{
	// Empty destructor

}

void springelement_store::init(geom_parameters* geom_param_ptr, std::vector<gyrospring_store*>* g_springs)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;
	this->g_springs = g_springs;
	sprg_line_count = 0;

	// Create the point shader
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	sprg_shader.create_shader((shadersPath.string() + "/resources/shaders/sprg_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/sprg_frag_shader.frag").c_str());

}

void springelement_store::set_buffer()
{
	// Set the number of lines
	sprg_line_count = static_cast<int>((*g_springs).size()) * (2 + spring_turn_count); // 2 flat ends + spring turn count 


	// Set the spring lines buffer for the index
	unsigned int sprg_indices_count = 2 * sprg_line_count;
	unsigned int* sprg_vertex_indices = new unsigned int[sprg_indices_count];

	unsigned int sprg_i_index = 0;


	for (auto& sprg_e : *g_springs)
	{
		// Add the spring element index buffer
		// Flat end 1
		get_sprg_index_buffer(sprg_vertex_indices, sprg_i_index);

		// Flat end 2
		get_sprg_index_buffer(sprg_vertex_indices, sprg_i_index);

		// Spring portion
		for (int i = 0; i < spring_turn_count; i++)
		{
			get_sprg_index_buffer(sprg_vertex_indices, sprg_i_index);
		}
	}

	VertexBufferLayout sprg_layout;
	sprg_layout.AddFloat(2);  // Position
	sprg_layout.AddFloat(1);  // Defl

	// Define the spring line vertices of model
	unsigned int sprg_vertex_count = 2 * 3 * sprg_line_count;
	unsigned int sprg_vertex_size = sprg_vertex_count * sizeof(float);

	// Allocate space for the spring vertex buffer
	sprg_buffer.CreateDynamicBuffers(sprg_vertex_size,
		sprg_vertex_indices, sprg_indices_count, sprg_layout);

	// Delete the Dynamic arrays
	delete[] sprg_vertex_indices;


	// Update the buffer for the spring lines points
	update_buffer();

}


void springelement_store::update_buffer()
{
	// Update the buffer
	// Update the spring vertex buffer
	unsigned int sprg_vertex_count = 2 * 3 * sprg_line_count;
	float* sprg_vertices = new float[sprg_vertex_count];

	unsigned int sprg_v_index = 0;

	// Update the spring vertex buffer
	for (auto& sprg_e : *g_springs)
	{
		glm::vec2 start_pt = sprg_e->gstart_node->gnode_pt; // get the start pt
		glm::vec2 end_pt = sprg_e->gend_node->gnode_pt; // get the end pt

		// Line length
		double element_length = geom_parameters::get_line_length(start_pt, end_pt);

		// Direction cosines
		double l_cos = (end_pt.x - start_pt.x) / element_length; // l cosine
		double m_sin = (start_pt.y - end_pt.y) / element_length; // m sine

		// Create the springs
		// Flat end 1 of the spring
		glm::vec2 curr_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.25f);

		// Add the vertex buffer
		// Flat end 1
		get_sprg_vertex_buffer(start_pt, curr_pt, sprg_vertices, sprg_v_index);

		// Flat end 2 of the spring
		curr_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.75f);

		// Add the vertex buffer
		// Flat end 2
		get_sprg_vertex_buffer(curr_pt, end_pt, sprg_vertices, sprg_v_index);


		// Spring portion
		glm::vec2 origin_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.25f); // origin point
		glm::vec2 prev_pt = origin_pt;
		curr_pt = glm::vec2(0);

		double spring_width_amplitude = geom_param_ptr->spring_element_width *
			(geom_param_ptr->node_circle_radii / geom_param_ptr->geom_scale);

		// Points of springs
		for (int i = 1; i < spring_turn_count; i++)
		{
			double param_t = i / static_cast<double>(spring_turn_count);

			double pt_x = (param_t * element_length * 0.5f);
			double pt_y = spring_width_amplitude * ((i % 2 == 0) ? 1 : -1);

			curr_pt = glm::vec2(((l_cos * pt_x) + (m_sin * pt_y)), ((-1.0 * m_sin * pt_x) + (l_cos * pt_y)));
			curr_pt = curr_pt + origin_pt;

			// Add the vertex buffer
			// Spring coil line (in the loop)
			get_sprg_vertex_buffer(prev_pt, curr_pt, sprg_vertices, sprg_v_index);

			// set the previous pt
			prev_pt = curr_pt;
		}

		// Last point
		curr_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.75f);

		// Add the vertex buffer
		// Spring coil line (last)
		get_sprg_vertex_buffer(prev_pt, curr_pt, sprg_vertices, sprg_v_index);

	}

	unsigned int sprg_vertex_size = sprg_vertex_count * sizeof(float); // size of the spring vertices

	// Update the buffer
	sprg_buffer.UpdateDynamicVertexBuffer(sprg_vertices, sprg_vertex_size);

	// Delete the Dynamic arrays
	delete[] sprg_vertices;
}



void springelement_store::paint_spring_geom()
{
	// Paint the spring lines
	sprg_shader.Bind();
	sprg_buffer.Bind();
	glDrawElements(GL_LINES, (2 * sprg_line_count), GL_UNSIGNED_INT, 0);
	sprg_buffer.UnBind();
	sprg_shader.UnBind();

}

void springelement_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	if (set_modelmatrix == true)
	{
		// set the model matrix
		sprg_shader.setUniform("geom_scale", static_cast<float>(geom_param_ptr->geom_scale));
		sprg_shader.setUniform("transparency", 1.0f);

		sprg_shader.setUniform("modelMatrix", geom_param_ptr->modelMatrix, false);
	}

	if (set_pantranslation == true)
	{
		// set the pan translation
		sprg_shader.setUniform("panTranslation", geom_param_ptr->panTranslation, false);
	}

	if (set_zoomtranslation == true)
	{
		// set the zoom translation
		sprg_shader.setUniform("zoomscale", static_cast<float>(geom_param_ptr->zoom_scale));
	}

	if (set_transparency == true)
	{
		// set the alpha transparency
		sprg_shader.setUniform("transparency", static_cast<float>(geom_param_ptr->geom_transparency));
	}

	if (set_deflscale == true)
	{
		// set the deflection scale
		sprg_shader.setUniform("normalized_deflscale", static_cast<float>(geom_param_ptr->normalized_defl_scale));
		sprg_shader.setUniform("deflscale", static_cast<float>(geom_param_ptr->defl_scale));
	}

}


void springelement_store::get_sprg_vertex_buffer(glm::vec2 sprg_startpt, glm::vec2 sprg_endpt,
	float* sprg_vertices, unsigned int& sprg_v_index)
{
	// Get the vertex buffer for the shader
	// Start Point
	// Point location
	sprg_vertices[sprg_v_index + 0] = sprg_startpt.x;
	sprg_vertices[sprg_v_index + 1] = sprg_startpt.y;

	// Point color
	sprg_vertices[sprg_v_index + 2] = 0.0;

	// Iterate
	sprg_v_index = sprg_v_index + 3;

	// End Point
	// Point location
	sprg_vertices[sprg_v_index + 0] = sprg_endpt.x;
	sprg_vertices[sprg_v_index + 1] = sprg_endpt.y;

	// Point color
	sprg_vertices[sprg_v_index + 2] = 0.0;

	// Iterate
	sprg_v_index = sprg_v_index + 3;

}


void springelement_store::get_sprg_index_buffer(unsigned int* sprg_vertex_indices, unsigned int& sprg_i_index)
{
	//__________________________________________________________________________
	// Add the indices
	// Index 1 (Start point)
	sprg_vertex_indices[sprg_i_index] = sprg_i_index;

	sprg_i_index = sprg_i_index + 1;

	// Index 2 (End point)
	sprg_vertex_indices[sprg_i_index] = sprg_i_index;

	sprg_i_index = sprg_i_index + 1;

}