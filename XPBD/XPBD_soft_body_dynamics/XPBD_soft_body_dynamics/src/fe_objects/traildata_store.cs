using OpenTK;
using XPBD_soft_body_dynamics.src.geom_objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPBD_soft_body_dynamics.src.fe_objects
{
    public class traildata_store
    {
        const int graph_pt_count = 40;

        // Graph intensity values (For color mapping)
        float[] graph_intensities = new float[graph_pt_count];

        // Graph x and y values
        float[] graph_x_values = new float[graph_pt_count];
        float[] graph_y_values = new float[graph_pt_count];

        // Phase portrait drawing data
        meshdata_store traildata_drawingdata;

        public traildata_store(float x, float y)
        {
            // Initialize trail data
            traildata_drawingdata = new meshdata_store(false);


            // Create dummy graph data
            for (int i = 0; i < graph_pt_count; i++)
            {
                // initialize the x and y values
                graph_x_values[i] = x;
                graph_y_values[i] = y;

                // dummy graph intensity values
                graph_intensities[i] = 0.0f;

                // Add point to mesh data
                traildata_drawingdata.add_mesh_point(i, x, y, 0.0, -6);

                // Add line segment
                if (i > 0)
                {
                    traildata_drawingdata.add_mesh_lines(i - 1, i - 1, i, -6);
                }
            }

            // Set the shader
            traildata_drawingdata.set_shader();

            // Set the buffer
            traildata_drawingdata.set_buffer();

        }


        public void update_trail(float x, float y)
        {

            // Shift old x,y values to the next
            for (int i = graph_pt_count - 1; i > 0; i--)
            {
                // Previous x & y
                graph_x_values[i] = graph_x_values[i - 1];
                graph_y_values[i] = graph_y_values[i - 1];


            }

            // Add new x, y value at the start
            graph_x_values[0] = x;
            graph_y_values[0] = y;


            for (int i = 0; i < graph_pt_count; i++)
            {
                float x1 = graph_x_values[i];
                float y1 = graph_y_values[i];

                // Update point in mesh data
                traildata_drawingdata.update_mesh_point(i, x1, y1, 0.0, 0.0);

            }

        }


        public void reset_trail(float x, float y)
        {
            // Reset the graph x, y values to origin
            for (int i = 0; i < graph_pt_count; i++)
            {
                // initialize the x and y values
                graph_x_values[i] = x;
                graph_y_values[i] = y;

                // dummy graph intensity values
                graph_intensities[i] = 0.0f;

                // Update point in mesh data
                traildata_drawingdata.update_mesh_point(i, x, y, 0.0, 0.0);

            }

        }


        public void paint_trail()
        {

            // Paint the trail
            traildata_drawingdata.paint_dynamic_mesh_lines();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            traildata_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

        }


    }
}
