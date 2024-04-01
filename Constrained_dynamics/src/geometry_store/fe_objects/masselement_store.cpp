#include "masselement_store.h"

masselement_store::masselement_store()
{
	// Empty constructor

}

masselement_store::~masselement_store()
{
	// Empty destructor

}

void masselement_store::init(geom_parameters* geom_param_ptr)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;

	// Create the shader and Texture for the drawing the constraints
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	ptmass_shader.create_shader((shadersPath.string() + "/resources/shaders/ptmass_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/ptmass_frag_shader.frag").c_str());

	// Set texture uniform
	ptmass_shader.setUniform("u_Texture", 0);

	// Load the texture
	ptmass_texture.LoadTexture((shadersPath.string() + "/resources/images/pic_3D_circle.png").c_str());

	// Clear the Point mass locations
	pt_mass_locations.clear();
	ptmass_count = 0;

}

void masselement_store::add_ptmass_geom(glm::vec2 ptmass_pt)
{
	// Add to the point mass locations
	pt_mass_locations.push_back(ptmass_pt);
	ptmass_count++;

}

void masselement_store::set_buffer()
{
	// Set the point mass buffer
	unsigned int ptmass_vertex_count = 4 * 12 * ptmass_count;
	float* ptmass_vertices = new float[ptmass_vertex_count];

	unsigned int ptmass_indices_count = 6 * ptmass_count;
	unsigned int* ptmass_indices = new unsigned int[ptmass_indices_count];

	unsigned int ptmass_v_index = 0;
	unsigned int ptmass_i_index = 0;

	for (auto& ptm_loc : pt_mass_locations)
	{
		// Add the texture buffer
		get_constraint_buffer(ptm_loc, ptmass_vertices, ptmass_v_index, ptmass_indices, ptmass_i_index);
	}

	VertexBufferLayout ptmass_layout;
	ptmass_layout.AddFloat(2);  // Position
	ptmass_layout.AddFloat(2);  // Center
	ptmass_layout.AddFloat(2);  // Defl
	ptmass_layout.AddFloat(3);  // Color
	ptmass_layout.AddFloat(2);  // Texture co-ordinate
	ptmass_layout.AddFloat(1);  // Is Deflection

	unsigned int ptmass_vertex_size = ptmass_vertex_count * sizeof(float);

	// Create the Constraint buffers
	ptmass_buffer.CreateBuffers(ptmass_vertices, ptmass_vertex_size,
		ptmass_indices, ptmass_indices_count, ptmass_layout);

	// Delete the Dynamic arrays
	delete[] ptmass_vertices;
	delete[] ptmass_indices;



}

void masselement_store::paint_ptmass_geom()
{
	// Paint the point mass
	ptmass_texture.Bind();
	ptmass_shader.Bind();
	ptmass_buffer.Bind();
	glDrawElements(GL_TRIANGLES, 6 * ptmass_count, GL_UNSIGNED_INT, 0);
	ptmass_buffer.UnBind();
	ptmass_shader.UnBind();
	ptmass_texture.UnBind();

}

void masselement_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{

	if (set_modelmatrix == true)
	{
		// set the model matrix
		ptmass_shader.setUniform("geom_scale", static_cast<float>(geom_param_ptr->geom_scale));
		ptmass_shader.setUniform("transparency", 1.0f);

		ptmass_shader.setUniform("modelMatrix", geom_param_ptr->modelMatrix, false);
	}

	if (set_pantranslation == true)
	{
		// set the pan translation
		ptmass_shader.setUniform("panTranslation", geom_param_ptr->panTranslation, false);
	}

	if (set_zoomtranslation == true)
	{
		// set the zoom translation
		ptmass_shader.setUniform("zoomscale", static_cast<float>(geom_param_ptr->zoom_scale));
	}

	if (set_transparency == true)
	{
		// set the alpha transparency
		ptmass_shader.setUniform("transparency", static_cast<float>(geom_param_ptr->geom_transparency));
	}

	if (set_deflscale == true)
	{
		// set the deflection scale
		// ptmass_shader.setUniform("deflscale", static_cast<float>(geom_param_ptr->defl_scale));
	}

}

void masselement_store::get_constraint_buffer(glm::vec2& ptm_loc, float* ptmass_vertices, unsigned int& ptmass_v_index, unsigned int* ptmass_indices, unsigned int& ptmass_i_index)
{
	// Constraint color
	glm::vec3 ptmass_color = geom_param_ptr->geom_colors.ptmass_color;
	float corner_size = static_cast<float>(-2.4 * (geom_param_ptr->node_circle_radii / geom_param_ptr->geom_scale));

	// Set the Point mass vertices Corner 1 Top Left
	ptmass_vertices[ptmass_v_index + 0] = ptm_loc.x - corner_size;
	ptmass_vertices[ptmass_v_index + 1] = ptm_loc.y + corner_size;

	// Set the node center
	ptmass_vertices[ptmass_v_index + 2] = ptm_loc.x;
	ptmass_vertices[ptmass_v_index + 3] = ptm_loc.y;

	// Defl value
	ptmass_vertices[ptmass_v_index + 4] = 0.0;
	ptmass_vertices[ptmass_v_index + 5] = 0.0;

	// Set the node color
	ptmass_vertices[ptmass_v_index + 6] = ptmass_color.x;
	ptmass_vertices[ptmass_v_index + 7] = ptmass_color.y;
	ptmass_vertices[ptmass_v_index + 8] = ptmass_color.z;

	// Set the Texture co-ordinates
	ptmass_vertices[ptmass_v_index + 9] = 0.0;
	ptmass_vertices[ptmass_v_index + 10] = 0.0;

	// Set the defl_value
	ptmass_vertices[ptmass_v_index + 11] = 0.0;

	// Increment
	ptmass_v_index = ptmass_v_index + 12;


	// Set the Point mass vertices Corner 2 Top Right
	ptmass_vertices[ptmass_v_index + 0] = ptm_loc.x + corner_size;
	ptmass_vertices[ptmass_v_index + 1] = ptm_loc.y + corner_size;

	// Set the node center
	ptmass_vertices[ptmass_v_index + 2] = ptm_loc.x;
	ptmass_vertices[ptmass_v_index + 3] = ptm_loc.y;

	// Defl value
	ptmass_vertices[ptmass_v_index + 4] = 0.0;
	ptmass_vertices[ptmass_v_index + 5] = 0.0;

	// Set the node color
	ptmass_vertices[ptmass_v_index + 6] = ptmass_color.x;
	ptmass_vertices[ptmass_v_index + 7] = ptmass_color.y;
	ptmass_vertices[ptmass_v_index + 8] = ptmass_color.z;

	// Set the Texture co-ordinates
	ptmass_vertices[ptmass_v_index + 9] = 1.0;
	ptmass_vertices[ptmass_v_index + 10] = 0.0;

	// Set the defl_value
	ptmass_vertices[ptmass_v_index + 11] = 0.0;

	// Increment
	ptmass_v_index = ptmass_v_index + 12;


	// Set the Point Mass vertices Corner 3 Bot Right
	ptmass_vertices[ptmass_v_index + 0] = ptm_loc.x + corner_size;
	ptmass_vertices[ptmass_v_index + 1] = ptm_loc.y - corner_size;

	// Set the node center
	ptmass_vertices[ptmass_v_index + 2] = ptm_loc.x;
	ptmass_vertices[ptmass_v_index + 3] = ptm_loc.y;

	// Defl value
	ptmass_vertices[ptmass_v_index + 4] = 0.0;
	ptmass_vertices[ptmass_v_index + 5] = 0.0;

	// Set the node color
	ptmass_vertices[ptmass_v_index + 6] = ptmass_color.x;
	ptmass_vertices[ptmass_v_index + 7] = ptmass_color.y;
	ptmass_vertices[ptmass_v_index + 8] = ptmass_color.z;

	// Set the Texture co-ordinates
	ptmass_vertices[ptmass_v_index + 9] = 1.0;
	ptmass_vertices[ptmass_v_index + 10] = 1.0;

	// Set the defl_value
	ptmass_vertices[ptmass_v_index + 11] = 0.0;

	// Increment
	ptmass_v_index = ptmass_v_index + 12;


	// Set the Constraint vertices Corner 4 Bot Left
	ptmass_vertices[ptmass_v_index + 0] = ptm_loc.x - corner_size;
	ptmass_vertices[ptmass_v_index + 1] = ptm_loc.y - corner_size;

	// Set the node center
	ptmass_vertices[ptmass_v_index + 2] = ptm_loc.x;
	ptmass_vertices[ptmass_v_index + 3] = ptm_loc.y;

	// Defl value
	ptmass_vertices[ptmass_v_index + 4] = 0.0;
	ptmass_vertices[ptmass_v_index + 5] = 0.0;

	// Set the node color
	ptmass_vertices[ptmass_v_index + 6] = ptmass_color.x;
	ptmass_vertices[ptmass_v_index + 7] = ptmass_color.y;
	ptmass_vertices[ptmass_v_index + 8] = ptmass_color.z;

	// Set the Texture co-ordinates
	ptmass_vertices[ptmass_v_index + 9] = 0.0;
	ptmass_vertices[ptmass_v_index + 10] = 1.0;

	// Set the defl_value
	ptmass_vertices[ptmass_v_index + 11] = 0.0;

	// Increment
	ptmass_v_index = ptmass_v_index + 12;


	//______________________________________________________________________

	// Set the Quad indices
	unsigned int t_id = ((ptmass_i_index / 6) * 4);

	// Triangle 0,1,2
	ptmass_indices[ptmass_i_index + 0] = t_id + 0;
	ptmass_indices[ptmass_i_index + 1] = t_id + 1;
	ptmass_indices[ptmass_i_index + 2] = t_id + 2;

	// Triangle 2,3,0
	ptmass_indices[ptmass_i_index + 3] = t_id + 2;
	ptmass_indices[ptmass_i_index + 4] = t_id + 3;
	ptmass_indices[ptmass_i_index + 5] = t_id + 0;

	// Increment
	ptmass_i_index = ptmass_i_index + 6;





}