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

	// Create the point shader
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	rigd_shader.create_shader((shadersPath.string() + "/resources/shaders/sprg_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/sprg_frag_shader.frag").c_str());

}

void rigidelement_store::set_buffer()
{
	// Set the number of lines
	rigd_surf_count = static_cast<int>((*g_rigids).size()) * 2; // 2 triangle surface for every single rigid line


	// Set the rigid lines buffer for the index
	unsigned int rigd_indices_count = 4 * rigd_surf_count;
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
	unsigned int rigd_vertex_count = 2 * 3 * sprg_line_count;
	unsigned int rigd_vertex_size = rigd_vertex_count * sizeof(float);

	// Allocate space for the spring vertex buffer
	rigd_buffer.CreateDynamicBuffers(rigd_vertex_size,
		rigd_vertex_indices, rigd_indices_count, rigd_layout);

	// Delete the Dynamic arrays
	delete[] rigd_vertex_indices;


	// Update the buffer for the spring lines points
	update_buffer();



	// Set the buffer for the rigid element triangles
	for (auto& rigd_e : *g_rigids)
	{
		glm::vec2 start_pt = rigd_e->gstart_node->gnode_pt; // get the start pt
		glm::vec2 end_pt = rigd_e->gend_node->gnode_pt; // get the end pt

		// Add the Rigid line
		// Line length
		double element_length = geom_parameters::get_line_length(start_pt, end_pt);

		// Direction cosines
		double l_cos = (end_pt.x - start_pt.x) / element_length; // l cosine
		double m_sin = (start_pt.y - end_pt.y) / element_length; // m sine

		double rigid_width_amplitude = geom_param_ptr->rigid_element_width *
			(geom_param_ptr->node_circle_radii / geom_param_ptr->geom_scale);

		// Half-width vector
		glm::vec2 half_width_vector = glm::vec2(m_sin, l_cos) * static_cast<float>(rigid_width_amplitude / 2.0f);

		// Four points to form the rigid element rectangle
		glm::vec2 point1 = start_pt + half_width_vector; // Upper left
		glm::vec2 point2 = start_pt - half_width_vector; // Lower left
		glm::vec2 point3 = end_pt + half_width_vector;   // Upper right
		glm::vec2 point4 = end_pt - half_width_vector;   // Lower right


		// Rigid element triangle 1
		int tri_id = rigid_element_surfaces.tri_count;
		rigid_element_surfaces.add_tri(tri_id, point2, point1, point3);

		// Rigid element triangle 2
		tri_id = rigid_element_surfaces.tri_count;
		rigid_element_surfaces.add_tri(tri_id, point3, point4, point2);

	}



	rigid_element_surfaces.set_buffer();

}



void rigidelement_store::update_buffer()
{

}


void rigidelement_store::paint_rigid_geom()
{
	// Paint the rigid triangles (surfaces)
	rigd_shader.Bind();
	rigd_buffer.Bind();
	glDrawElements(GL_TRIANGLES, (3 * tri_count), GL_UNSIGNED_INT, 0);
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


void get_rigd_vertex_buffer(glm::vec2 rigd_startpt, glm::vec2 rigd_endpt,
	float* rigd_vertices, unsigned int& rigd_v_index)
{


}

void get_rigd_index_buffer(unsigned int* rigd_vertex_indices, unsigned int& rigd_i_index)
{


}

