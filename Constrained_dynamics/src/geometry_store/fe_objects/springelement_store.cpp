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

	// Create the point shader
	std::filesystem::path shadersPath = geom_param_ptr->resourcePath;

	sprg_shader.create_shader((shadersPath.string() + "/resources/shaders/sprg_vert_shader.vert").c_str(),
		(shadersPath.string() + "/resources/shaders/sprg_frag_shader.frag").c_str());


	// Clear the spring lines
	spring_lines.init(geom_param_ptr);
	spring_lines.clear_lines();

}

void springelement_store::set_buffer()
{
	// Create the spring lines
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
		// Flat ends of the spring
		int line_id = spring_lines.line_count;
		glm::vec3 temp_color = geom_param_ptr->geom_colors.spring_line_color;
		glm::vec2 curr_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.25f);

		// Flat end 1
		spring_lines.add_line(line_id, start_pt, curr_pt,
			glm::vec2(0), glm::vec2(0), temp_color, temp_color, false);

		curr_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.75f);

		// Flat end 2
		line_id = spring_lines.line_count;
		spring_lines.add_line(line_id, curr_pt, end_pt,
			glm::vec2(0), glm::vec2(0), temp_color, temp_color, false);


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

			line_id = spring_lines.line_count;

			spring_lines.add_line(line_id, prev_pt, curr_pt,
				glm::vec2(0), glm::vec2(0), temp_color, temp_color, false);

			// set the previous pt
			prev_pt = curr_pt;
		}

		// Last point
		curr_pt = geom_parameters::linear_interpolation(start_pt, end_pt, 0.75f);

		line_id = spring_lines.line_count;

		spring_lines.add_line(line_id, prev_pt, curr_pt,
			glm::vec2(0), glm::vec2(0), temp_color, temp_color, false);

	}



	// Set the buffer for the spring lines
	spring_lines.set_buffer();

}

void springelement_store::paint_spring_geom()
{
	// Paint the spring lines
	spring_lines.paint_lines();

}

void springelement_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	spring_lines.update_opengl_uniforms(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);

}
