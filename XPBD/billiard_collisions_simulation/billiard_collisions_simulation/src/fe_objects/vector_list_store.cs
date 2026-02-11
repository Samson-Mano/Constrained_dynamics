// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using billiard_collisions_simulation.src.geom_objects;
using billiard_collisions_simulation.src.global_variables;
using billiard_collisions_simulation.src.opentk_control.opentk_bgdraw;
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace billiard_collisions_simulation.src.fe_objects
{
    public class vector_data
    {
        public int vector_id { get; set; }
        public Vector2 tail_pt { get; set; }
        public Vector2 arrow_pt { get; set; }
        // public double intensity { get; set; }

    }

    public class vector_list_store
    {

        // Vector data list
        public Dictionary<int, vector_data> vectorMap = new Dictionary<int, vector_data>();
        public int vector_count = 0;
        public int color_id = 0;

        // Vector drawing data
        meshdata_store vector_drawingdata;

        // Length and width of arrow head
        const float L = 6.0f;   // arrow head length 10F
        const float W = 3.0f;  // arrow head width 5F

        public vector_list_store()
        {
            // (Re)Initialize the data
            vectorMap = new Dictionary<int, vector_data>();
            vector_count = 0;

            // vector_drawingdata = new meshdata_store(true);

        }


        public void add_vector(int vector_id, Vector2 tail_pt, Vector2 arrow_pt, int color_id)
        {

            // Add a vector data
            vector_data new_vector = new vector_data();
            new_vector.vector_id = vector_id;
            new_vector.tail_pt = tail_pt;
            new_vector.arrow_pt = arrow_pt;
            // new_vector.intensity = intensity;

            this.color_id = color_id;

            // Add to the map
            vectorMap.Add(new_vector.vector_id, new_vector);
            vector_count++;

        }

        public void update_vector(int vector_id, Vector2 tail_pt, Vector2 arrow_pt)
        {
            //// Check if the vector id exists
            //if (vectorMap.ContainsKey(vector_id) == false)
            //{
            //    // Vector id not found
            //    return;
            //}


            // Update the vector data
            vectorMap[vector_id].tail_pt = tail_pt;
            vectorMap[vector_id].arrow_pt = arrow_pt;
            // vectorMap[vector_id].intensity = intensity;

            // Update the point and line data in the drawing data
            int tail_pt_id = vector_id * 4;
            int arrow_pt_id = tail_pt_id + 1;
            int arrow_left_pt_id = tail_pt_id + 2;
            int arrow_right_pt_id = tail_pt_id + 3;

            // Update the vector points
            vector_drawingdata.update_mesh_point(tail_pt_id, tail_pt.X, tail_pt.Y, 0.0, color_id);
            vector_drawingdata.update_mesh_point(arrow_pt_id, arrow_pt.X, arrow_pt.Y, 0.0, color_id);
            // Add the arrow head points

            Vector2 dir_vector = Vector2.Normalize(arrow_pt - tail_pt);
            Vector2 perp_vector = new Vector2(-dir_vector.Y, dir_vector.X);

            Vector2 arrow_left = arrow_pt - (L * dir_vector) + (W * perp_vector);
            Vector2 arrow_right = arrow_pt - (L * dir_vector) - (W * perp_vector);

            vector_drawingdata.update_mesh_point(arrow_left_pt_id, arrow_left.X, arrow_left.Y, 0.0, color_id);
            vector_drawingdata.update_mesh_point(arrow_right_pt_id, arrow_right.X, arrow_right.Y, 0.0, color_id);


        }


        public void set_vector_visualization()
        {
            // Initialize the mesh data
            vector_drawingdata = new meshdata_store(false);

            // Set the vector visualization for all vectors in the map

            foreach (vector_data vector in vectorMap.Values)
            {
                // Set the point ids
                int tail_pt_id = vector.vector_id * 4;
                int arrow_pt_id = tail_pt_id + 1;
                int arrow_left_pt_id = tail_pt_id + 2;
                int arrow_right_pt_id = tail_pt_id + 3;

                // Add the vector points
                vector_drawingdata.add_mesh_point(tail_pt_id, vector.tail_pt.X, vector.tail_pt.Y, 0.0, color_id);
                vector_drawingdata.add_mesh_point(arrow_pt_id, vector.arrow_pt.X, vector.arrow_pt.Y, 0.0, color_id);
                // Add the arrow head points

                Vector2 dir_vector = Vector2.Normalize(vector.arrow_pt - vector.tail_pt);
                Vector2 perp_vector = new Vector2(-dir_vector.Y, dir_vector.X);

                Vector2 arrow_left = vector.arrow_pt - (L * dir_vector) + (W * perp_vector);
                Vector2 arrow_right = vector.arrow_pt - (L * dir_vector) - (W * perp_vector);

                vector_drawingdata.add_mesh_point(arrow_left_pt_id, arrow_left.X, arrow_left.Y, 0.0, color_id);
                vector_drawingdata.add_mesh_point(arrow_right_pt_id, arrow_right.X, arrow_right.Y, 0.0, color_id);


                // Set the line ids
                int vector_line_id = vector.vector_id * 3;
                int arrow_head_line_id1 = vector_line_id + 1;
                int arrow_head_line_id2 = vector_line_id + 2;

                // Lines
                vector_drawingdata.add_mesh_lines(vector_line_id, tail_pt_id, arrow_pt_id, color_id);

                // Arrow head lines
                vector_drawingdata.add_mesh_lines(arrow_head_line_id1, arrow_pt_id, arrow_left_pt_id, color_id);
                vector_drawingdata.add_mesh_lines(arrow_head_line_id2, arrow_pt_id, arrow_right_pt_id, color_id);

            }

            // Set the shader
            vector_drawingdata.set_shader();

            // Set the buffer
            vector_drawingdata.set_buffer();


        }

        public void paint_vectors()
        {
            // Paint the vector
            vector_drawingdata.paint_dynamic_mesh_lines();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float rslt_transparency)
        {
            if (vector_count == 0)
                return;


            vector_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                rslt_transparency);

        }

        //
    }
    //
}
