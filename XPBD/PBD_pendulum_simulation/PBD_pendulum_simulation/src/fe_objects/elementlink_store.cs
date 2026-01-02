using OpenTK;
using PBD_pendulum_simulation.src.geom_objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBD_pendulum_simulation.src.fe_objects
{
    public class elementlink_store
    {
        Vector2 rest_startpt = new Vector2(0);
        Vector2 rest_endpt = new Vector2(0);

        float rest_elementlength = 0.0f;


        const float geom_line_width = 6.0f;

        // Rigid Link drawing data
        meshdata_store rigidlink_drawingdata;

        public elementlink_store(Vector2 start_pt, Vector2 end_pt)
        {
            // element length
            float element_length = Vector2.Distance(start_pt, end_pt);

            // Store the data
            rest_startpt = start_pt;
            rest_endpt = end_pt;

            rest_elementlength = element_length;


            //_______________________________________________________________________
            //_____________________________________________________________________________________________
            // Mesh objects

            float halfWidth = geom_line_width * 0.5f;

            Vector2 dir = end_pt - start_pt;
            float length = dir.Length;

            // Prevent division by zero
            if (length < 1e-6f)
                return;

            dir /= length; // normalize

            // Perpendicular normal
            Vector2 normal = new Vector2(-dir.Y, dir.X);

            // Rectangle vertices
            Vector2 p1 = start_pt + (normal * halfWidth);
            Vector2 p2 = end_pt + (normal * halfWidth);
            Vector2 p3 = end_pt - (normal * halfWidth);
            Vector2 p4 = start_pt - (normal * halfWidth);



            rigidlink_drawingdata = new meshdata_store(false);


            rigidlink_drawingdata.add_mesh_point(0, p1.X, p1.Y, 0.0, -3);
            rigidlink_drawingdata.add_mesh_point(1, p2.X, p2.Y, 0.0, -3);
            rigidlink_drawingdata.add_mesh_point(2, p3.X, p3.Y, 0.0, -3);
            rigidlink_drawingdata.add_mesh_point(3, p4.X, p4.Y, 0.0, -3);

            //_____________________________________________________________________________________________________
            rigidlink_drawingdata.add_mesh_lines(0, 0, 1, -3);
            rigidlink_drawingdata.add_mesh_lines(1, 1, 2, -3);
            rigidlink_drawingdata.add_mesh_lines(2, 2, 3, -3);
            rigidlink_drawingdata.add_mesh_lines(3, 3, 0, -3);

            //_____________________________________________________________________________________________________
            rigidlink_drawingdata.add_mesh_tris(0, 0, 1, 2, -3);
            rigidlink_drawingdata.add_mesh_tris(1, 2, 3, 0, -3);


            // Set the shader
            rigidlink_drawingdata.set_shader();

            // Set th buffer
            rigidlink_drawingdata.set_buffer();


        }


        public void update_rigidlink(Vector2 start_pt, Vector2 end_pt)
        {
            // Update the mesh point
            float halfWidth = geom_line_width * 0.5f;

            Vector2 dir = end_pt - start_pt;
            float length = dir.Length;

            // Prevent division by zero
            if (length < 1e-6f)
                return;

            dir /= length; // normalize

            // Perpendicular normal
            Vector2 normal = new Vector2(-dir.Y, dir.X);

            // Rectangle vertices
            Vector2 p1 = start_pt + (normal * halfWidth);
            Vector2 p2 = end_pt + (normal * halfWidth);
            Vector2 p3 = end_pt - (normal * halfWidth);
            Vector2 p4 = start_pt - (normal * halfWidth);



            rigidlink_drawingdata = new meshdata_store(false);


            rigidlink_drawingdata.update_mesh_point(0, p1.X, p1.Y, 0.0, -3);
            rigidlink_drawingdata.update_mesh_point(1, p2.X, p2.Y, 0.0, -3);
            rigidlink_drawingdata.update_mesh_point(2, p3.X, p3.Y, 0.0, -3);
            rigidlink_drawingdata.update_mesh_point(3, p4.X, p4.Y, 0.0, -3);

        }


        public void paint_rigidlink()
        {
            // Paint the spring
            // rigidlink_drawingdata.paint_dynamic_mesh_lines();

            rigidlink_drawingdata.paint_dynamic_mesh();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            rigidlink_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

        }




    }
}
