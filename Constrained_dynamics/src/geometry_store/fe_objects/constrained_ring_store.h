#pragma once
#include <glm/vec2.hpp>
#include <glm/vec3.hpp>
#include <unordered_map>
#include "../geom_parameters.h"
#include "../geometry_buffers/gBuffers.h"


class constrained_ring_store
{
public:
	std::vector<glm::vec2> c_nodepts;
	std::unordered_map<int, gyronode_store> c_nodes;
	std::vector<constrainedtri_store> c_triangles;
	int cnsr_triangle_count = 0; // Constrained ring surface triangle count

	constrained_ring_store();
	~constrained_ring_store();

	void init(geom_parameters* geom_param_ptr);

	void add_constrainednodes(int& node_id,double& nd_x, double& nd_y);
	void add_constrainedtris(int& tri_id, int& nd1, int& nd2, int& nd3);

	bool is_constrained_ring_clicked(glm::vec2& screen_loc);
	void rotate_constrained_ring(const double& rotation_angle);
	void rotate_constrained_ring_ends(const double& rotation_angle);

	void set_buffer();
	void update_buffer();
	void paint_constrained_ring();
	void update_geometry_matrices(bool set_modelmatrix, bool set_pantranslation, bool set_zoomtranslation, bool set_transparency, bool set_deflscale);
private:
	geom_parameters* geom_param_ptr = nullptr;
	gBuffers cnsr_buffer;
	Shader cnsr_shader;

	void get_cnsr_vertex_buffer(glm::vec2 cnsr_pt1, glm::vec2 cnsr_pt2, glm::vec2 cnsr_pt3,
		float* cnsr_vertices, unsigned int& cnsr_v_index);

	void get_cnsr_index_buffer(unsigned int* cnsr_vertex_indices, unsigned int& cnsr_i_index);


	// tri_list_store c_surfacetris;

};
