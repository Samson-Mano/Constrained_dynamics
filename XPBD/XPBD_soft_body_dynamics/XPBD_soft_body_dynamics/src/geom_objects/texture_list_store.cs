// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPBD_soft_body_dynamics.src.global_variables;
using XPBD_soft_body_dynamics.src.opentk_control.opentk_bgdraw;
using XPBD_soft_body_dynamics.src.opentk_control.opentk_buffer;
using XPBD_soft_body_dynamics.src.opentk_control.shader_compiler;

namespace XPBD_soft_body_dynamics.src.geom_objects
{
    public class texture_store
    {
        public int texture_id { get; set; }

        public Vector2 textrure_center { get; set; }

        public double texture_width { get; set; }

        public double texture_height { get; set; }

        public double rotation_angle { get; set; }

        public double normalized_defl_scale { get; set; }
        public int color_id { get; set; }
        public Vector3 point_color { get; set; }


    }


    public class texture_list_store
    {

        public Dictionary<int, texture_store> textureMap { get; } = new Dictionary<int, texture_store>();
        public int texture_count = 0;
        private bool is_DynamicDraw = false;


        private graphicBuffers texture_buffer;
        public Shader texture_shader;
        private TextureBuffer texture_data;



        public texture_list_store(bool is_DynamicDraw, byte[] res_pic)
        {
            // (Re)Initialize the data
            textureMap = new Dictionary<int, texture_store>();
            texture_count = 0;
            this.is_DynamicDraw = is_DynamicDraw;


            texture_data = new TextureBuffer(res_pic);
        }



        public void add_texture(int texture_id, Vector2 textrure_center, 
            double texture_width, double texture_height, double rotation_angle, int color_id)
        {
            // Add the Texture to the list
            texture_store temp_texture = new texture_store
            {
                texture_id = texture_id,
                textrure_center = textrure_center,
                texture_width = texture_width,
                texture_height = texture_height,
                rotation_angle = rotation_angle,

                color_id = color_id,
                point_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id)

            };

            textureMap[texture_id] = temp_texture;
            texture_count++;

        }


        public void delete_texture(int texture_id)
        {
            // Delete the texture
            textureMap.Remove(texture_id);
            texture_count--;

        }


        public void update_texturecenter(int texture_id, Vector2 textrure_center, double normalized_defl_scale)
        {
            // Update the point co-ordinates
            textureMap[texture_id].textrure_center = textrure_center;
            textureMap[texture_id].normalized_defl_scale = normalized_defl_scale;

        }


        public void set_shader()
        {

            // Create Shader
            texture_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.TextureShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.TextureShader));

   
        }


        public void paint_static_texture()
        {
            // Paint the static text
            texture_shader.Bind();
            texture_buffer.Bind();

            // Activate texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture to the active texture unit
            GL.BindTexture(TextureTarget.Texture2D, texture_data.TextureID);

            // Draw the elements
            GL.DrawElements(PrimitiveType.Triangles, 6 * texture_count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);

            texture_shader.UnBind();
            texture_buffer.UnBind();

        }


        public void paint_dynamic_texture()
        {
            // Paint the dynamic text
            texture_shader.Bind();
            texture_buffer.Bind();

            // Update the text buffer data for dynamic drawing
            update_buffer();

            // Activate texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture to the active texture unit
            GL.BindTexture(TextureTarget.Texture2D,  texture_data.TextureID);

            // Draw the elements
            GL.DrawElements(PrimitiveType.Triangles, 6 * texture_count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);

            texture_shader.UnBind();
            texture_buffer.UnBind();

        }




        public void set_buffer()
        {

            // Set the buffer for index (6 indices to form a two triangle, quadrilateral )
            int texture_indices_count = 6 * texture_count; // 6 index per character
            int[] texture_vertex_indices = new int[texture_indices_count];

            int texture_i_index = 0;

            // Set the texture index buffers
            foreach (var tx in textureMap)
            {
                // Set the texture index buffers
                get_texture_index_buffer(ref texture_vertex_indices, ref texture_i_index);

            }

            // Define the vertex layout
            VertexBufferLayout textureLayout = new VertexBufferLayout();
            textureLayout.AddFloat(2);  // quad point 
            textureLayout.AddFloat(2);  // quad center
            textureLayout.AddFloat(2);  // Texture coordinate
            textureLayout.AddFloat(3);  // Texture color
            textureLayout.AddFloat(1);  // Is Dynamic data
            textureLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point ( 2 quad position, 2 quad center, 2 Texture coordinate, 3 Texture color, 1 IsDynamic, 1 normalized defl scale)
            int texture_vertex_count = 4 * 11 * texture_count;
            int texture_vertex_size = texture_vertex_count * sizeof(float);

            // Create the texture dynamic buffers
            texture_buffer = new graphicBuffers(null, texture_vertex_size, texture_vertex_indices,
                texture_indices_count, textureLayout, true);

            // Update the buffer
            update_buffer();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
           Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix, float geom_transparency)
        {

            // Update the openGl uniform matrices
            if (set_modelmatrix == true)
            {
                // Set the model matrix
                texture_shader.SetMatrix4("modelMatrix", modelMatrix);

                // Set the projection matrix
                texture_shader.SetMatrix4("projectionMatrix", projectionMatrix);

            }

            if (set_viewmatrix == true)
            {
                // Set the view matrix
                texture_shader.SetMatrix4("viewMatrix", viewMatrix);

            }

            if (set_transparency == true)
            {
                // Set the transparency float
                texture_shader.SetFloat("vertexTransparency", geom_transparency);

            }

        }


        public void update_buffer()
        {
            // Define the vertex buffer size for a texture
            // (2 quad position, 2 quad center, 2 Texture coordinate, 3 Texture color, 1 IsDynamic, 1 normalized defl scale)
            int texture_vertex_count = 4 * 11 * texture_count;
            float[] texture_vertices = new float[texture_vertex_count];

            int texture_v_index = 0;

            // Set the texture vertex buffers
            foreach (var tx in textureMap)
            {
                // Add vertex buffers
                get_texture_vertex_buffer(tx.Value, ref texture_vertices, ref texture_v_index);
            }

            int texture_vertex_size = texture_vertex_count * sizeof(float); // Size of the texture vertex buffer

            // Update the buffer
            texture_buffer.UpdateDynamicVertexBuffer(texture_vertices, texture_vertex_size);

        }




        private void get_texture_vertex_buffer(texture_store tx, ref float[] texture_vertices, ref int texture_v_index)
        {
            // Get the center of texture
            Vector2 center_pt = tx.textrure_center;

            float norm_defl_scale = (float)tx.normalized_defl_scale;

            float hw = (float)tx.texture_width * 0.5f;
            float hh = (float)tx.texture_height * 0.5f;

            float angleRad = (float)((tx.rotation_angle + 180.0) * Math.PI / 180.0);

            float cos = (float)Math.Cos(angleRad);
            float sin = (float)Math.Sin(angleRad);

            // Create the four corner point of the quad
            // rotate offsets
            Vector2 topleft = new Vector2((-hw * cos) + (hh * sin), (hw * sin) + (hh * cos)) + center_pt;
            Vector2 topright = new Vector2((hw * cos) + (hh * sin), -(hw * sin) + (hh * cos)) + center_pt;
            Vector2 botright = new Vector2((hw * cos) - (hh * sin), -(hw * sin) - (hh * cos)) + center_pt;
            Vector2 botleft = new Vector2((-hw * cos) - (hh * sin), (hw * sin) - (hh * cos)) + center_pt;


            // Get the node buffer for the shader
            // Set the Point mass vertices Corner 1 Top Left
            // Top left location
            texture_vertices[texture_v_index + 0] = topleft.X;
            texture_vertices[texture_v_index + 1] = topleft.Y;

            // Center location
            texture_vertices[texture_v_index + 2] = center_pt.X;
            texture_vertices[texture_v_index + 3] = center_pt.Y;

            // Texture coordinate
            texture_vertices[texture_v_index + 4] = 0.0f;
            texture_vertices[texture_v_index + 5] = 0.0f;

            // Texture color
            texture_vertices[texture_v_index + 6] = tx.point_color.X;
            texture_vertices[texture_v_index + 7] = tx.point_color.Y;
            texture_vertices[texture_v_index + 8] = tx.point_color.Z;

            // Is Dynamic Draw
            texture_vertices[texture_v_index + 9] = is_DynamicDraw ? 1.0f : 0.0f;

            // Normalized deflection scale
            texture_vertices[texture_v_index + 10] = norm_defl_scale;

            // Iterate
            texture_v_index = texture_v_index + 11;


            // Set the Point mass vertices Corner 2 Top Right
            // Top Right location
            texture_vertices[texture_v_index + 0] = topright.X;
            texture_vertices[texture_v_index + 1] = topright.Y;

            // Center location
            texture_vertices[texture_v_index + 2] = center_pt.X;
            texture_vertices[texture_v_index + 3] = center_pt.Y;

            // Texture coordinate
            texture_vertices[texture_v_index + 4] = 1.0f;
            texture_vertices[texture_v_index + 5] = 0.0f;

            // Texture color
            texture_vertices[texture_v_index + 6] = tx.point_color.X;
            texture_vertices[texture_v_index + 7] = tx.point_color.Y;
            texture_vertices[texture_v_index + 8] = tx.point_color.Z;

            // Is Dynamic Draw
            texture_vertices[texture_v_index + 9] = is_DynamicDraw ? 1.0f : 0.0f;

            // Normalized deflection scale
            texture_vertices[texture_v_index + 10] = norm_defl_scale;

            // Iterate
            texture_v_index = texture_v_index + 11;



            // Set the Point Mass vertices Corner 3 Bot Right
            // Bot Right location
            texture_vertices[texture_v_index + 0] = botright.X;
            texture_vertices[texture_v_index + 1] = botright.Y;

            // Center location
            texture_vertices[texture_v_index + 2] = center_pt.X;
            texture_vertices[texture_v_index + 3] = center_pt.Y;

            // Texture coordinate
            texture_vertices[texture_v_index + 4] = 1.0f;
            texture_vertices[texture_v_index + 5] = 1.0f;

            // Texture color
            texture_vertices[texture_v_index + 6] = tx.point_color.X;
            texture_vertices[texture_v_index + 7] = tx.point_color.Y;
            texture_vertices[texture_v_index + 8] = tx.point_color.Z;

            // Is Dynamic Draw
            texture_vertices[texture_v_index + 9] = is_DynamicDraw ? 1.0f : 0.0f;

            // Normalized deflection scale
            texture_vertices[texture_v_index + 10] = norm_defl_scale;

            // Iterate
            texture_v_index = texture_v_index + 11;


            // Set the Constraint vertices Corner 4 Bot Left
            // Bot Left location
            texture_vertices[texture_v_index + 0] = botleft.X;
            texture_vertices[texture_v_index + 1] = botleft.Y;

            // Center location
            texture_vertices[texture_v_index + 2] = center_pt.X;
            texture_vertices[texture_v_index + 3] = center_pt.Y;

            // Texture coordinate
            texture_vertices[texture_v_index + 4] = 0.0f;
            texture_vertices[texture_v_index + 5] = 1.0f;

            // Texture color
            texture_vertices[texture_v_index + 6] = tx.point_color.X;
            texture_vertices[texture_v_index + 7] = tx.point_color.Y;
            texture_vertices[texture_v_index + 8] = tx.point_color.Z;

            // Is Dynamic Draw
            texture_vertices[texture_v_index + 9] = is_DynamicDraw ? 1.0f : 0.0f;

            // Normalized deflection scale
            texture_vertices[texture_v_index + 10] = norm_defl_scale;

            // Iterate
            texture_v_index = texture_v_index + 11;


        }





        private void get_texture_index_buffer(ref int[] texture_vertex_indices, ref int texture_i_index)
        {
            int t_id = (int)((texture_i_index / 6.0) * 4.0);

            // Add the indices
            // Index 0 1 2 
            texture_vertex_indices[texture_i_index + 0] = t_id + 0;

            texture_vertex_indices[texture_i_index + 1] = t_id + 1;

            texture_vertex_indices[texture_i_index + 2] = t_id + 2;

            // Index 2 3 0 
            texture_vertex_indices[texture_i_index + 3] = t_id + 2;

            texture_vertex_indices[texture_i_index + 4] = t_id + 3;

            texture_vertex_indices[texture_i_index + 5] = t_id + 0;

            // Iterate
            texture_i_index = texture_i_index + 6;

        }
        //
    }
    //
}
