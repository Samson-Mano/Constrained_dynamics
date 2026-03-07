using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPBD_soft_body_dynamics.Resources;
using XPBD_soft_body_dynamics.src.geom_objects;

namespace XPBD_soft_body_dynamics.src.fe_objects
{
    public class node_store
    {
        public int node_id = 0;

        public Vector2 nodept = new Vector2(0);


        public node_store(int node_id, Vector2 nodept)
        {
            this.node_id = node_id;
            this.nodept = nodept;
        }
        //
    }


    public class node_list_store
    {
        const float geom_node_size = 6.0f;

        public Dictionary<int, node_store> nodeMap = new Dictionary<int, node_store>();
        public int node_count = 0;


        // Node drawing data
        texture_list_store node_drawingdata;



        public node_list_store()
        {
            // (Re)Initialize the data
            nodeMap = new Dictionary<int, node_store>();
            node_count = 0;

            //
        }


        public void add_node(int node_id, Vector2 node_pt)
        {

            // Add a node data
            node_store new_node = new node_store(node_id,
                node_pt);


            // Add to the map
            nodeMap.Add(new_node.node_id, new_node);
            node_count++;

            //
        }


        public void update_node(int node_id, Vector2 node_pt, double norm_defl_scale)
        {
            // Update the point data

            node_drawingdata.update_texturecenter(node_id, node_pt, norm_defl_scale);

            //
        }


        public void set_node_visualization(float geom_size)
        {
            // Load the texture for node
            byte[] res_3dcirclepic = Resource_font.pic_node_texture2;


            // Initialize the node texture drawing data
            node_drawingdata = new texture_list_store(true, res_3dcirclepic);

            // Set the element link visualization for all links in the map

            foreach (node_store nd in nodeMap.Values)
            {
                // Set the point id
                int pt_id = nd.node_id; // 1 point to form a texture
                Vector2 node_pt = nd.nodept;

                float node_pt_dia = geom_node_size * (geom_size * 0.002f);

                int color_id = -3;

                //_______________________________________________________________________
                //_____________________________________________________________________________________________
                // Mesh objects

                node_drawingdata.add_texture(pt_id, node_pt, node_pt_dia, node_pt_dia, color_id);

                //
            }

            // Set the shader
            node_drawingdata.set_shader();

            // Set the buffer
            node_drawingdata.set_buffer();
            //
        }


        public void paint_node()
        {

            // Paint the node
            node_drawingdata.paint_dynamic_texture();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            if (node_count == 0)
                return;

            // Update the openTK uniforms of the drawing objects
            node_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
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
