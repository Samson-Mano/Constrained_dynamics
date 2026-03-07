using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPBD_soft_body_dynamics.src.geom_objects;

namespace XPBD_soft_body_dynamics.src.fe_objects
{

    public class elementlink_store
    {
        public int element_id = 0;

        public int prop_id = 0;

        public Vector2 startpt = new Vector2(0);
        public Vector2 endpt = new Vector2(0);

        public float elementlength = 0.0f;



        public elementlink_store(int element_id, Vector2 start_pt, Vector2 end_pt, int prop_id)
        {
            // Element ID
            this.element_id = element_id;

            // Element property ID
            this.prop_id = prop_id;

            // element length
            float element_length = Vector2.Distance(start_pt, end_pt);

            // Store the data
            this.startpt = start_pt;
            this.endpt = end_pt;

            this.elementlength = element_length;

        }
        //
    }


    public class elementlink_list_store
    {

        const float geom_line_width = 6.0f;

        public Dictionary<int, elementlink_store> elementlinkMap = new Dictionary<int, elementlink_store>();
        public int elementlink_count = 0;


        // Spring Link drawing data
        meshdata_store springlink_drawingdata;



        public elementlink_list_store()
        {
            // (Re)Initialize the data
            elementlinkMap = new Dictionary<int, elementlink_store>();
            elementlink_count = 0;

            //
        }


        public void add_elementlink(int element_id, Vector2 start_pt, Vector2 end_pt, int prop_id)
        {

            // Add a element link data
            elementlink_store new_elementlink = new elementlink_store(element_id,
                start_pt,
                end_pt, 
                prop_id);
            
            
            // Add to the map
            elementlinkMap.Add(new_elementlink.element_id, new_elementlink);
            elementlink_count++;

            //
        }


        public void update_elementlink(int element_id, Vector2 start_pt, Vector2 end_pt)
        {
            int element_pt_id = element_id * 4; // 4 points to form a rigid link

            int color_id = elementlinkMap[element_pt_id].prop_id;

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

            springlink_drawingdata.update_mesh_point(element_pt_id + 0, p1.X, p1.Y, 0.0, color_id);
            springlink_drawingdata.update_mesh_point(element_pt_id + 1, p2.X, p2.Y, 0.0, color_id);
            springlink_drawingdata.update_mesh_point(element_pt_id + 2, p3.X, p3.Y, 0.0, color_id);
            springlink_drawingdata.update_mesh_point(element_pt_id + 3, p4.X, p4.Y, 0.0, color_id);

            //
        }


        public void set_elementlink_visualization(float geom_size)
        {
            // Initialize the element link drawing data
            springlink_drawingdata = new meshdata_store(true);

            // Set the element link visualization for all links in the map

            foreach (elementlink_store elementlink in elementlinkMap.Values)
            {
                // Set the point id
                int element_pt_id = elementlink.element_id * 4; // 4 points to form a rigid link
                int element_line_id = elementlink.element_id * 4; // 4 lines to form the rigid link
                int element_tri_id = elementlink.element_id * 2; // 2 triangles to form the rigid link


                int color_id = elementlink.prop_id;

                //_______________________________________________________________________
                //_____________________________________________________________________________________________
                // Mesh objects
                Vector2 startpt = elementlink.startpt;
                Vector2 endpt = elementlink.endpt;


                float halfWidth = geom_line_width * (geom_size * 0.001f) * 0.5f;

                Vector2 dir = endpt - startpt;
                float length = dir.Length;

                // Prevent division by zero
                if (length < 1e-6f)
                    return;

                dir /= length; // normalize

                // Perpendicular normal
                Vector2 normal = new Vector2(-dir.Y, dir.X);

                // Rectangle vertices
                Vector2 p1 = startpt + (normal * halfWidth);
                Vector2 p2 = endpt + (normal * halfWidth);
                Vector2 p3 = endpt - (normal * halfWidth);
                Vector2 p4 = startpt - (normal * halfWidth);


                // Add to the point
                springlink_drawingdata.add_mesh_point(element_pt_id + 0, p1.X, p1.Y, 0.0, color_id);
                springlink_drawingdata.add_mesh_point(element_pt_id + 1, p2.X, p2.Y, 0.0, color_id);
                springlink_drawingdata.add_mesh_point(element_pt_id + 2, p3.X, p3.Y, 0.0, color_id);
                springlink_drawingdata.add_mesh_point(element_pt_id + 3, p4.X, p4.Y, 0.0, color_id);

                //_____________________________________________________________________________________________________
                springlink_drawingdata.add_mesh_lines(element_line_id + 0, element_pt_id + 0, element_pt_id + 1, color_id);
                springlink_drawingdata.add_mesh_lines(element_line_id + 1, element_pt_id + 1, element_pt_id + 2, color_id);
                springlink_drawingdata.add_mesh_lines(element_line_id + 2, element_pt_id + 2, element_pt_id + 3, color_id);
                springlink_drawingdata.add_mesh_lines(element_line_id + 3, element_pt_id + 3, element_pt_id + 0, color_id);

                //_____________________________________________________________________________________________________
                springlink_drawingdata.add_mesh_tris(element_tri_id + 0, element_pt_id + 0, element_pt_id + 1, element_pt_id + 2, color_id);
                springlink_drawingdata.add_mesh_tris(element_tri_id + 1, element_pt_id + 2, element_pt_id + 3, element_pt_id + 0, color_id);

            }

            // Set the shader
            springlink_drawingdata.set_shader();

            // Set the buffer
            springlink_drawingdata.set_buffer();
            //
        }


        public void paint_elementlink()
        {
            // Paint the mass
            springlink_drawingdata.paint_dynamic_mesh();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            if (elementlink_count == 0)
                return;

            // Update the openTK uniforms of the drawing objects
            springlink_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

            //
        }

        //

    }
    //
}
