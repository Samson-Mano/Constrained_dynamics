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
    public class stringlinedrawingdata_store
    {
        fixedenddrawing_store fixedend_left;
        fixedenddrawing_store fixedend_right;

        // Line in tension and nodes drawing data
        meshdata_store stringline_drawingdata;

        public stringlinedrawingdata_store(Vector2 start_loc, Vector2 end_loc, int segmentCount) 
        {
            const int node_color = -3;
            const int linesegment_color = -4;

            stringline_drawingdata = new meshdata_store(false);

            stringline_drawingdata.add_mesh_point(0, start_loc.X, start_loc.Y, 0.0f, node_color);

            float invSegments = 1.0f / (float)segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                // Find the next point by interpolation
                float t = (i + 1) * invSegments;
                Vector2 p = Vector2.Lerp(start_loc, end_loc, t);

                // Add the point
                stringline_drawingdata.add_mesh_point(i+1, p.X, p.Y, 0.0f, node_color);

                // Add the line segment
                stringline_drawingdata.add_mesh_lines(i, i, i + 1, linesegment_color);

            }

            // Set the fixed ends at the end of the string in tension
            fixedend_left = new fixedenddrawing_store(start_loc, 0.0f);
            fixedend_right = new fixedenddrawing_store(end_loc, 0.0f);


            // Set the shader
            stringline_drawingdata.set_shader();

            // Set the buffer
            stringline_drawingdata.set_buffer();

        }



        public void paint_elementstringline()
        {
            // Paint the fixed ends at left and right
            fixedend_left.paint_fixedend();
            fixedend_right.paint_fixedend();

            // Paint the string in tension (Line and nodes)
            gvariables_static.LineWidth = 2.0f;
            gvariables_static.PointSize = 6.0f;

            stringline_drawingdata.paint_static_mesh_lines();
            stringline_drawingdata.paint_static_mesh_points();

            gvariables_static.LineWidth = 1.0f;
            gvariables_static.PointSize = 1.0f;

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            stringline_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);


            fixedend_left.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

            fixedend_right.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
               projectionMatrix,
               modelMatrix,
               viewMatrix,
               geom_transparency);




        }


    }
}
