#pragma once
#include <glm/vec2.hpp>
#include <glm/vec3.hpp>
#include <glm/glm.hpp>
#include <sstream>
#include "geometry_buffers/font_atlas.h"

// Stopwatch
#include "../events_handler/Stopwatch_events.h"

struct geom_color_theme
{
	glm::vec3 background_color = glm::vec3(0);
	glm::vec3 node_color = glm::vec3(0);
	glm::vec3 selection_color = glm::vec3(0);
	glm::vec3 rigid_line_color = glm::vec3(0);
	glm::vec3 spring_line_color = glm::vec3(0);
	glm::vec3 line_length_color = glm::vec3(0);
	glm::vec3 load_color = glm::vec3(0);
	glm::vec3 constraint_color = glm::vec3(0);
	glm::vec3 ptmass_color = glm::vec3(0);
	glm::vec3 inlcond_displ_color = glm::vec3(0);
	glm::vec3 inlcond_velo_color = glm::vec3(0);
	glm::vec3 triangle_color = glm::vec3(0);
};

struct chart_setting_data
{
	// X Range
	float chart_x_max = 0.0f;
	float chart_x_min = 0.0f;

	// Y Range
	float chart_y_max = 0.0f;
	float chart_y_min = 0.0f;

	// Number of data points
	int data_pt_count = 0;
};

struct frequency_reponse_data
{
	int node_id;
	std::vector<double> frequency_values;
	// X response
	std::vector<double> displ_x;
	std::vector<double> phase_x;

	// Y response
	std::vector<double> displ_y;
	std::vector<double> phase_y;

	// Magnitude response
	std::vector<double> displ_magnitude;
	std::vector<double> phase_magnitude;

};


struct gyronode_store
{
	int gnode_id = 0; // Node ID
	glm::vec2 gnode_pt = glm::vec2(0); // Node point
	glm::vec2 gnode_velo = glm::vec2(0); // Node velocity
	glm::vec2 gnode_normal = glm::vec2(0); // Node normal vector

	bool isPtmassexist = false; // Is point mass exist in the node or not
	double gmass_value = 0.001; // Node mass value

	// Time integration values
	glm::vec2 gnode_displ_hat = glm::vec2(0); // node displacement star
	glm::vec2 gnode_velo_hat = glm::vec2(0); // node velocity star


	bool isFixed = false; // Node 
};


struct gyrospring_store
{
	int gsprg_id = 0; // Spring ID
	gyronode_store* gstart_node = nullptr; // Start node
	gyronode_store* gend_node = nullptr; // End node
	bool is_rigid = false;

	// Rest length
	double rest_length = 0.0;

	// alpha_i = 0.0 for rigid element
	// alpha_i = 1.0 / (stiff * delta_t^2) for spring element
	double alpha_i = 0.0;

	// beta_i = delta_t^2 * damp for spring element
	double beta_i = 0.0; 

	// gamma_i = (alpha_i * beta_i) / delta_t
	double gamma_i = 0.0;


	// XPBD Lagrange multipliers
	double delta_lamda_i = 0.0; // delta lamda_i
	double lamda_i = 0.0; // lamda_i 
};


struct constrainedtri_store
{
	int ctri_id = 0; // ID of the triangle element
	gyronode_store* c_nd1 = nullptr; // node 1
	gyronode_store* c_nd2 = nullptr; // node 2
	gyronode_store* c_nd3 = nullptr; // node 3
};


class geom_parameters
{
public:
	// Standard sizes
	const float font_size = static_cast<float>(12.0f * std::pow(10, -5));
	const float node_circle_radii = 0.005f;

	// Geometry size
	const float point_size = 3.0f;
	const float selected_point_size = 6.0f;
	const float line_width = 2.1f;
	const float selected_line_width = 4.2f;
	
	// element width
	const float spring_element_width = 1.2f;
	const float rigid_element_width = 1.6f;

	// Spring turns
	const double spring_turn_min = 10.0;
	const double spring_turn_max = 20.0;

	// Precision for various values
	const int length_precision = 2;
	const int coord_precision = 2;
	const int constraint_precision = 2;
	const int load_precision = 2;
	const int inlcond_precision = 3;
	const int defl_precision = 6;

	// Triangle mesh shrunk factor
	const double traingle_shrunk_factor = 0.8;

	// PI value
	const double mPI = 3.14159265358979323; 

	// File path
	std::filesystem::path resourcePath = "";

	// Window size
	int window_width = 0;
	int window_height = 0;

	glm::vec2 min_b = glm::vec2(0); // (min_x, min_y)
	glm::vec2 max_b = glm::vec2(0); // (max_x, max_y)
	glm::vec2 geom_bound = glm::vec2(0); // Bound magnitude
	glm::vec2 center = glm::vec2(0); // center of the geometry
	glm::mat4 modelMatrix = glm::mat4(0); // Geometry model matrix
	double geom_scale = 0.0; // Scale of the geometry
	double geom_transparency = 1.0; // Value to control the geometry transparency
	double normalized_defl_scale = 0.0f; // Value of deflection scale
	double defl_scale = 0.0f; // Value of deflection scale

	// Screen transformations
	glm::mat4 panTranslation = glm::mat4(0); // Pan translataion
	double zoom_scale = 0.0; // Zoom scale

	// Standard colors
	geom_color_theme geom_colors;

	font_atlas main_font;

	geom_parameters();
	~geom_parameters();
	void init();

	static bool isPointInsideRectangle(const glm::vec2& rect_cpt1, const glm::vec2& rect_cpt2, const glm::vec2& pt);

	static void copyNodenumberlistToCharArray(const std::vector<int>& vec, char* charArray, size_t bufferSize);

	static glm::vec3 get_standard_color(int color_index);

	static glm::vec2 linear_interpolation(const glm::vec2& pt1, const glm::vec2& pt2, const double& param_t);

	static	glm::vec2 findGeometricCenter(const std::vector<glm::vec2>& all_pts);

	static std::pair<glm::vec2, glm::vec2> findMinMaxXY(const std::vector<glm::vec2>& all_pts);

	static glm::vec3 getHeatMapColor(float value);

	static	double roundToSixDigits(const double& number);

	static glm::vec3 getContourColor_d(float value);

	static double get_triangle_area(const glm::vec2& pt1, const glm::vec2& pt2, const glm::vec2& pt3);

	static double get_line_length(const glm::vec2& pt1, const glm::vec2& pt2);

	static glm::vec2 calculateCatmullRomPoint(const std::vector<glm::vec2>& controlPoints, float t);

	static double get_lerp(const double& max_value, const double& min_value, const double& param_t);

	static double get_invlerp(const double& max_value, const double& min_value, const double& value);

	static double get_remap(const double& max_value, const double& min_value, const double& limit_max, 
		const double& limit_min,		const double& value);

	static bool isLeft(const glm::vec2& A, const glm::vec2& B, const glm::vec2& P);

	static bool is_triangle_clicked(const glm::vec2& mouse_loc, const glm::vec2& tri_nd1, const glm::vec2& tri_nd2, const glm::vec2& tri_nd3);

	static double calculateAngle_withOrigin(const glm::vec2& pt1, const glm::vec2& pt2);

private:
	static double HueToRGB(double v1, double v2, double vH);

};



