using OpenTK;
using String_vibration_openTK.src.geom_objects;
using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace String_vibration_openTK.src.fe_objects
{
    public class stringlinemodalresults_store
    {

        // Line in tension and nodes modal results data
        meshdata_store stringline_resultsdata;

        // Store the initialization data
        Vector2 start_loc = new Vector2(0.0f, 0.0f);
        Vector2 end_loc = new Vector2(0.0f, 0.0f);
        int segmentCount = 0;

        public stringlinemodalresults_store(Vector2 start_loc, Vector2 end_loc, int segmentCount)
        {
            // Store the initialization data
            this.start_loc = start_loc;
            this.end_loc = end_loc;
            this.segmentCount = segmentCount;   


            //____________________________________________
            const int node_color = -3;
            const int linesegment_color = -4;

            stringline_resultsdata = new meshdata_store(true);

            stringline_resultsdata.add_mesh_point(0, start_loc.X, start_loc.Y, 0.0f, node_color);

            float invSegments = 1.0f / (float)segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                // Find the next point by interpolation
                float t = (i + 1) * invSegments;
                Vector2 p = Vector2.Lerp(start_loc, end_loc, t);

                // Add the point
                stringline_resultsdata.add_mesh_point(i + 1, p.X, p.Y, 0.0f, node_color);

                // Add the line segment
                stringline_resultsdata.add_mesh_lines(i, i, i + 1, linesegment_color);

            }

            // Set the shader
            stringline_resultsdata.set_shader();

            // Set the buffer
            stringline_resultsdata.set_buffer();

        }


        public void update_modalresults_time_step(double elapsedRealTime, int selected_mode_shape, double displ_scale)
        {
            int mode_number = selected_mode_shape + 1;

            // Update the modal results displacement of the string line nodes
            stringline_resultsdata.update_mesh_point(0, start_loc.X, start_loc.Y, 0.0f, 0.0f);

            float invSegments = 1.0f / (float)segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                // Find the next point by interpolation
                float t = (i + 1) * invSegments;
                Vector2 p = Vector2.Lerp(start_loc, end_loc, t);

                // Update the P - y point
                double eigen_vec = Math.Sin(mode_number * Math.PI * t);

                double color_scale = Math.Abs(eigen_vec);

                double animscale = displ_scale * eigen_vec * Math.Cos(Math.PI * elapsedRealTime * gvariables_static.modal_animation_speed);

                // Update the point
                stringline_resultsdata.update_mesh_point(i + 1, p.X, p.Y + animscale, 0.0f, color_scale);
         
            }

        }


        public void paint_modalanalysisresults()
        {

            // Paint the string in tension (Line and nodes)
            gvariables_static.LineWidth = 2.0f;
            gvariables_static.PointSize = 6.0f;

            stringline_resultsdata.paint_dynamic_mesh_lines();
            stringline_resultsdata.paint_dynamic_mesh_points();

            gvariables_static.LineWidth = 1.0f;
            gvariables_static.PointSize = 1.0f;

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            stringline_resultsdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);


        }



    }
}
