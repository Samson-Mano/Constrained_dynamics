#pragma once
#include "geom_parameters.h"

// File system
#include <fstream>
#include <sstream>
#include <iomanip>

// Window includes
#include "../tool_window/new_model_window.h"
#include "../tool_window/node_load_window.h"
#include "../tool_window/inlcondition_window.h"
#include "../tool_window/options_window.h"
#include "../tool_window/modal_analysis_window.h"
#include "../tool_window/pulse_analysis_window.h"

// FE Objects
#include "fe_objects/constrained_ring_store.h"
#include "fe_objects/gyro_model_store.h"

// Geometry Objects
#include "geometry_objects/dynamic_selrectangle_store.h"

// FE Result Objects



class geom_store
{
public: 
	const double m_pi = 3.14159265358979323846;
	bool is_geometry_set = false;

	// Main Variable to strore the geometry parameters
	geom_parameters geom_param;

	geom_store();
	~geom_store();

	void init(modal_analysis_window* modal_solver_window,
		pulse_analysis_window* pulse_solver_window,
		options_window* op_window,
		node_load_window* nd_load_window, 
		inlcondition_window* nd_inlcond_window,
		new_model_window* md_window);
	void fini();

	// Load the geometry
	void load_constrained_ring(std::ifstream& cring_input_data, std::ifstream& gyro_input_data);

	bool is_constrained_clicked(glm::vec2& mouse_loc);
	void rotate_constraint(glm::vec2& click_pt, glm::vec2& curr_pt);
	void rotate_constraint_ends(glm::vec2& click_pt, glm::vec2& curr_pt);

	// Functions to control the drawing area
	void update_WindowDimension(const int& window_width, const int& window_height);
	void update_model_matrix();
	void update_model_zoomfit();
	void update_model_pan(glm::vec2& transl);
	void update_model_zoom(double& z_scale);
	void update_model_transperency(bool is_transparent);

	// Function to paint the selection rectangle
	void update_selection_rectangle(const glm::vec2& o_pt, const glm::vec2& c_pt,
		const bool& is_paint, const bool& is_select, const bool& is_rightbutton);

	// Functions to paint the geometry and results
	void paint_geometry();
private:
	dynamic_selrectangle_store selection_rectangle;

	// Geometry objects
	constrained_ring_store constrained_ring;
	gyro_model_store gyro_model;

	// Window pointers
	new_model_window* md_window = nullptr;
	options_window* op_window = nullptr;
	node_load_window* nd_load_window = nullptr;
	inlcondition_window* nd_inlcond_window = nullptr;

	// Analysis window
	modal_analysis_window* modal_solver_window = nullptr;
	pulse_analysis_window* pulse_solver_window = nullptr;

	void paint_model(); // Paint the model
	void run_simulation(); // Run the dynamic simulation

};

