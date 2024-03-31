#include "rigidelement_store.h"

rigidelement_store::rigidelement_store()
{
	// Empty constructor

}

rigidelement_store::~rigidelement_store()
{
	// Empty destructor
}

void rigidelement_store::init(geom_parameters* geom_param_ptr)
{
	// Set the geometry parameters
	this->geom_param_ptr = geom_param_ptr;

	// Clear the rigid element surface
	rigid_element_surfaces.init(geom_param_ptr);
	rigid_element_surfaces.clear_triangles();

}

void rigidelement_store::add_rigid_geom(glm::vec2 start_pt, glm::vec2 end_pt)
{
	// Add the Rigid line
	// Line length
	double element_length = geom_parameters::get_line_length(start_pt, end_pt);

	// Direction cosines
	double l_cos = (end_pt.x - start_pt.x) / element_length; // l cosine
	double m_sin = (start_pt.y - end_pt.y) / element_length; // m sine

	double rigid_width_amplitude = geom_param_ptr->rigid_element_width *
		(geom_param_ptr->node_circle_radii / geom_param_ptr->geom_scale);

	// Half-width vector
	glm::vec2 half_width_vector = glm::vec2(m_sin, l_cos) * static_cast<float>(rigid_width_amplitude/2.0f);

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

void rigidelement_store::set_buffer()
{
	// Set the buffer for the rigid element triangles
	rigid_element_surfaces.set_buffer();

}

void rigidelement_store::paint_rigid_geom()
{
	// Paint the rigid triangles
	rigid_element_surfaces.paint_triangles();

}

void rigidelement_store::update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale)
{
	// Update model openGL uniforms
	rigid_element_surfaces.update_opengl_uniforms(set_modelmatrix, set_pantranslation, set_zoomtranslation, set_transparency, set_deflscale);

}
