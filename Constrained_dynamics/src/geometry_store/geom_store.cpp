#include "geom_store.h"

geom_store::geom_store()
{
	// Empty Constructor
}

geom_store::~geom_store()
{
	// Empty Destructor
}

void geom_store::init(modal_analysis_window* modal_solver_window,
	pulse_analysis_window* pulse_solver_window,
	options_window* op_window,
	node_load_window* nd_load_window,
	inlcondition_window* nd_inlcond_window,
	new_model_window* md_window)
{
	// Initialize
	// Initialize the geometry parameters
	geom_param.init();

	// Intialize the selection rectangle
	selection_rectangle.init(&geom_param);

	is_geometry_set = false;


	// Add the window pointers
	this->md_window = md_window;
	this->op_window = op_window; // Option window
	this->nd_load_window = nd_load_window; // Node Load window
	this->nd_inlcond_window = nd_inlcond_window; // Node initial condition window

	// Add the solver window pointers
	this->modal_solver_window = modal_solver_window; // Modal Analysis Solver window
	this->pulse_solver_window = pulse_solver_window; // Pulse Analysis Solver window
}

void geom_store::fini()
{
	// Deinitialize
	is_geometry_set = false;
}

void geom_store::load_constrained_ring(std::ifstream& cring_input_data, std::ifstream& gyro_input_data)
{
	// Read the Raw Data - Constrained Ring
	// Read the entire file into a string
	std::string cringfile_contents((std::istreambuf_iterator<char>(cring_input_data)),
		std::istreambuf_iterator<char>());

	// Split the string into lines
	std::istringstream iss1(cringfile_contents);
	std::string line;
	std::vector<std::string> cdata_lines;
	while (std::getline(iss1, line))
	{
		cdata_lines.push_back(line);
	}
	//________________________________________________________________________
	//________________________________________________________________________

	int j = 0;

	// Initialize the constrained ring
	this->constrained_ring.init(&geom_param);
	this->gyro_model.init(&geom_param);

	//Node Point list
	std::vector<glm::vec2> node_pts_list;

	// Process the lines
	while (j < cdata_lines.size())
	{
		std::istringstream iss(cdata_lines[j]);

		std::string inpt_type;
		char comma;
		iss >> inpt_type;

		if (inpt_type == "*NODE")
		{
			// Nodes
			while (j < cdata_lines.size())
			{
				std::istringstream nodeIss(cdata_lines[j + 1]);

				// Vector to store the split values
				std::vector<std::string> splitValues;

				// Split the string by comma
				std::string token;
				while (std::getline(nodeIss, token, ','))
				{
					splitValues.push_back(token);
				}

				if (static_cast<int>(splitValues.size()) <= 3)
				{
					break;
				}

				int node_id = std::stoi(splitValues[0]); // node ID
				double x = geom_parameters::roundToSixDigits(std::stod(splitValues[1])); // Node coordinate x
				double y = geom_parameters::roundToSixDigits(std::stod(splitValues[2])); // Node coordinate y

				glm::vec2 node_pt = glm::vec2(x, y);
				node_pts_list.push_back(node_pt);

				// Add the nodes
				this->constrained_ring.add_constrainednodes(node_id, x,y);
				j++;
			}

		}

		if (inpt_type == "*ELEMENT,TYPE=CTRIA")
		{
			// Triangle Element
			while (j < cdata_lines.size())
			{
				std::istringstream elementIss(cdata_lines[j + 1]);

				// Vector to store the split values
				std::vector<std::string> splitValues;

				// Split the string by comma
				std::string token;
				while (std::getline(elementIss, token, ','))
				{
					splitValues.push_back(token);
				}

				if (static_cast<int>(splitValues.size()) != 4)
				{
					break;
				}

				int tri_id = std::stoi(splitValues[0]); // triangle ID
				int nd1 = std::stoi(splitValues[1]); // Node id 1
				int nd2 = std::stoi(splitValues[2]); // Node id 2
				int nd3 = std::stoi(splitValues[3]); // Node id 3

				// Add the Triangle Elements
				this->constrained_ring.add_constrainedtris(tri_id, nd1, nd2,nd3);
				j++;
			}
		}

		// Iterate the line
		j++;
	}

	//________________________________________________________________________
	//________________________________________________________________________
	// Read the Raw Data - Gyro Model
	// Read the entire file into a string
	std::string gyrofile_contents((std::istreambuf_iterator<char>(gyro_input_data)),
		std::istreambuf_iterator<char>());

	// Split the string into lines
	std::istringstream iss2(gyrofile_contents);
	std::vector<std::string> gyro_lines;

	while (std::getline(iss2, line))
	{
		gyro_lines.push_back(line);
	}

	j = 0;

	// Process the lines
	while (j < gyro_lines.size())
	{
		line = gyro_lines[j];
		std::string type = line.substr(0, 4);  // Extract the first 4 characters of the line

		// Split the line into comma-separated fields
		std::istringstream iss3(line);
		std::string field1;
		std::vector<std::string> fields;
		while (std::getline(iss3, field1, ','))
		{
			fields.push_back(field1);
		}

		if (type == "node")
		{
			// Read the nodes
			int node_id = std::stoi(fields[1]); // node ID
			double x = std::stod(fields[2]); // Node coordinate x
			double y = std::stod(fields[3]); // Node coordinate y

			// Add to node Map
			this->gyro_model.add_gyronodes(node_id, x,y);
		}
		else if (type == "sprg")
		{
			int line_id = std::stoi(fields[1]); // line ID
			int start_node_id = std::stoi(fields[2]); // line id start node
			int end_node_id = std::stoi(fields[3]); // line id end node
			int material_id = std::stoi(fields[4]); // materail ID of the line

			// Add to spring element map
			this->gyro_model.add_gyrosprings(line_id, start_node_id, end_node_id);
		}
		else if (type == "rigd")
		{
			int line_id = std::stoi(fields[1]); // line ID
			int start_node_id = std::stoi(fields[2]); // line id start node
			int end_node_id = std::stoi(fields[3]); // line id end node
			int material_id = std::stoi(fields[4]); // materail ID of the line

			// Add to rigid element map
			this->gyro_model.add_gyrorigids(line_id, start_node_id,end_node_id);
		}
		else if (type == "ptms")
		{
			int ptmass_id = std::stoi(fields[1]); // mass ID
			int node_id = std::stoi(fields[2]); // node ID
			double ptmass_val = std::stod(fields[3]); // point mass value

			// Add to the point mass map
			this->gyro_model.add_gyroptmass(ptmass_id, node_id);
		}

		// Iterate line
		j++;
	}


	//________________________________________________________________________
	//________________________________________________________________________

	// Geometry is loaded
	is_geometry_set = true;

	// Set the boundary of the geometry
	std::pair<glm::vec2, glm::vec2> result = geom_parameters::findMinMaxXY(node_pts_list);
	this->geom_param.min_b = result.first;
	this->geom_param.max_b = result.second;
	this->geom_param.geom_bound = geom_param.max_b - geom_param.min_b;

	// Set the center of the geometry
	this->geom_param.center = geom_parameters::findGeometricCenter(node_pts_list);

	// Set the geometry
	update_model_matrix();
	update_model_zoomfit();

	// Set the buffer
	this->constrained_ring.set_buffer();
	this->gyro_model.set_buffer();

	//________________________________________________________________________________________________________________________
	//________________________________________________________________________________________________________________________


}


bool geom_store::is_constrained_clicked(glm::vec2& mouse_loc)
{
	// Convert the mouse location to transformed screen locations
	int max_dim = geom_param.window_width > geom_param.window_height ? geom_param.window_width : geom_param.window_height;

	// Transform the mouse location to openGL screen coordinates
	glm::vec2 screen_loc = glm::vec2(2.0f * ((mouse_loc.x - (geom_param.window_width * 0.5f)) / max_dim),
									 2.0f * (((geom_param.window_height * 0.5f) - mouse_loc.y) / max_dim));


	// Is constrained click
	return constrained_ring.is_constrained_ring_clicked(screen_loc);

}

void geom_store::rotate_constraint(glm::vec2& click_pt, glm::vec2& curr_pt)
{
	// Rotate constraint ring and the gyro model
	


	constrained_ring.rotate_constrained_ring(rotation_angle);
	gyro_model.rotate_gyro_model(rotation_angle);

}

void geom_store::rotate_constraint_ends(glm::vec2& click_pt, glm::vec2& curr_pt)
{
	// Rotate ends for constraint ring & gyro model
	constrained_ring.rotate_constrained_ring_ends(rotation_angle);
	gyro_model.rotate_gyro_model_ends(rotation_angle);

}

void geom_store::update_WindowDimension(const int& window_width, const int& window_height)
{
	// Update the window dimension
	this->geom_param.window_width = window_width;
	this->geom_param.window_height = window_height;

	if (is_geometry_set == true)
	{
		// Update the model matrix
		update_model_matrix();
		// !! Zoom to fit operation during window resize is handled in mouse event class !!
	}
}


void geom_store::update_model_matrix()
{
	// Set the model matrix for the model shader
	// Find the scale of the model (with 0.9 being the maximum used)
	int max_dim = geom_param.window_width > geom_param.window_height ? geom_param.window_width : geom_param.window_height;

	double normalized_screen_width = 1.8f * (static_cast<double>(geom_param.window_width) / static_cast<double>(max_dim));
	double normalized_screen_height = 1.8f * (static_cast<double>(geom_param.window_height) / static_cast<double>(max_dim));


	geom_param.geom_scale = std::min(normalized_screen_width / geom_param.geom_bound.x,
		normalized_screen_height / geom_param.geom_bound.y);

	// Translation
	glm::vec3 geom_translation = glm::vec3(-1.0f * (geom_param.max_b.x + geom_param.min_b.x) * 0.5f * geom_param.geom_scale,
		-1.0f * (geom_param.max_b.y + geom_param.min_b.y) * 0.5f * geom_param.geom_scale,
		0.0f);

	glm::mat4 g_transl = glm::translate(glm::mat4(1.0f), geom_translation);

	geom_param.modelMatrix = g_transl * glm::scale(glm::mat4(1.0f), glm::vec3(static_cast<float>(geom_param.geom_scale)));

	// Update the model matrix
	constrained_ring.update_geometry_matrices(true, false, false, true, false);
	gyro_model.update_geometry_matrices(true, false, false, true, false);


}

void geom_store::update_model_zoomfit()
{
	if (is_geometry_set == false)
		return;

	// Set the pan translation matrix
	geom_param.panTranslation = glm::mat4(1.0f);

	// Set the zoom scale
	geom_param.zoom_scale = 1.0f;

	// Update the zoom scale and pan translation
	constrained_ring.update_geometry_matrices(false, true, true, false, false);
	gyro_model.update_geometry_matrices(false, true, true, false, false);

}

void geom_store::update_model_pan(glm::vec2& transl)
{
	if (is_geometry_set == false)
		return;

	// Pan the geometry
	geom_param.panTranslation = glm::mat4(1.0f);

	geom_param.panTranslation[0][3] = -1.0f * transl.x;
	geom_param.panTranslation[1][3] = transl.y;

	// Update the pan translation
	constrained_ring.update_geometry_matrices(false, true, false, false, false);
	gyro_model.update_geometry_matrices(false, true, false, false, false);


}

void geom_store::update_model_zoom(double& z_scale)
{
	if (is_geometry_set == false)
		return;

	// Zoom the geometry
	geom_param.zoom_scale = z_scale;

	// Update the Zoom
	constrained_ring.update_geometry_matrices(false, false, true, false, false);
	gyro_model.update_geometry_matrices(false, false, true, false, false);


}

void geom_store::update_model_transperency(bool is_transparent)
{
	if (is_geometry_set == false)
		return;

	if (is_transparent == true)
	{
		// Set the transparency value
		geom_param.geom_transparency = 0.2f;
	}
	else
	{
		// remove transparency
		geom_param.geom_transparency = 1.0f;
	}

	// Update the model transparency
	constrained_ring.update_geometry_matrices(false, false, false, true, false);
	gyro_model.update_geometry_matrices(false, false, false, true, false);

	// Donot update result elements transparency

}

void geom_store::update_selection_rectangle(const glm::vec2& o_pt, const glm::vec2& c_pt,
	const bool& is_paint, const bool& is_select, const bool& is_rightbutton)
{
	// Draw the selection rectangle
	selection_rectangle.update_selection_rectangle(o_pt, c_pt, is_paint);

	// Selection commence (mouse button release)
	if (is_paint == false && is_select == true)
	{

	}
}


void geom_store::paint_geometry()
{

	if (is_geometry_set == false)
		return;

	// Clean the back buffer and assign the new color to it
	glClear(GL_COLOR_BUFFER_BIT);

	// Paint the model
	paint_model();

	// Paint the results
	paint_model_results();

}

void geom_store::paint_model()
{
	//______________________________________________
	// Paint the model

	constrained_ring.paint_constrained_ring();
	gyro_model.paint_gyro_model();

}

void geom_store::paint_model_results()
{
	// Paint the results
	
}



