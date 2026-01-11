using String_vibration_openTK.src.geom_objects;
using String_vibration_openTK.src.global_variables;
using String_vibration_openTK.src.opentk_control.opentk_bgdraw;
using String_vibration_openTK.src.opentk_control.opentk_buffer;
using String_vibration_openTK.src.opentk_control.shader_compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace String_vibration_openTK.src.fe_objects
{
    public class text_store
    {
        public string text_value = "";
        public Vector2 text_loc = new Vector2(0);
        public Vector3 text_color = new Vector3(0);
        public int text_char_count = 0;

        
        const float text_angle = 0.0f;
        const float text_font_size = 12.0f;

        private graphicBuffers text_buffer;
        private Shader text_shader;

        public text_store(string text_value, Vector2 text_loc, int color_id)
        {
            // Initialize the text store
            this.text_value = text_value;
            this.text_loc = text_loc;
            this.text_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id);

            // Character count (Set value will not change)
            text_char_count = text_value.Length;


            // Create the text buffer and shader
            // Create Shader
            text_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.TextShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.TextShader));

            // Create the text buffer
            create_buffer();

        }


        public void update_text(string text_value, Vector2 text_loc)
        {
            // Update the label information
            this.text_value = text_value;
            this.text_loc = text_loc;   
            
        }


        public void paint_static_text()
        {
            // Paint the static text
            text_shader.Bind();
            text_buffer.Bind();

            // Activate texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture to the active texture unit
            GL.BindTexture(TextureTarget.Texture2D, gvariables_static.main_font.TextureID);

            // Draw the elements
            GL.DrawElements(PrimitiveType.Triangles, 6 * text_char_count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);

            text_shader.UnBind();
            text_buffer.UnBind();

        }


        public void paint_dynamic_text()
        {
            // Paint the dynamic text
            text_shader.Bind();
            text_buffer.Bind();

            // Update the text buffer data for dynamic drawing
            update_buffer();

            // Activate texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);

            // Bind the texture to the active texture unit
            GL.BindTexture(TextureTarget.Texture2D, gvariables_static.main_font.TextureID);

            // Draw the elements
            GL.DrawElements(PrimitiveType.Triangles, 6 * text_char_count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);

            text_shader.UnBind();
            text_buffer.UnBind();

        }


        public void create_buffer()
        {

            // Set the buffer for index (6 indices to form a two triangle, quadrilateral )
            int text_indices_count = 6 * text_char_count; // 6 index per character
            int[] text_indices = new int[text_indices_count];

            int text_i_index = 0;

            // Set the text index buffers
            get_text_index_buffer(ref text_indices, ref text_i_index);
         

            // Define the vertex layout
            VertexBufferLayout textLayout = new VertexBufferLayout();
            textLayout.AddFloat(2);  // Character Position
            textLayout.AddFloat(2);  // Text location
            textLayout.AddFloat(2);  // Texture coordinate
            textLayout.AddFloat(3);  // Text color

            // Define the vertex buffer size for a character
            // 4 vertex to form a quadrilateral ( 2 char position, 2 text location, 2 texture coord, 3 color)
            int text_vertex_count = 4 * 9 * text_char_count;
            int text_vertex_size = text_vertex_count * sizeof(float);

            // Create the text dynamic buffers
            text_buffer = new graphicBuffers(null, text_vertex_size, text_indices,
                text_indices_count, textLayout, true);

            // Update the buffer
            update_buffer();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
                                    drawing_events graphic_events_control)
        {

            // Update the openGl uniform matrices
            if (set_modelmatrix == true)
            {
                // Set the model matrix
                text_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);

                // Set the projection matrix
                text_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);

            }

            if (set_viewmatrix == true)
            {
                // Set the view matrix
                text_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);

            }

            if (set_transparency == true)
            {
                // Set the transparency float
                text_shader.SetFloat("vertexTransparency", gvariables_static.geom_transparency);

            }

        }


        public void update_buffer()
        {
            // Define the vertex buffer size for a character
            // 4 vertex to form a quadrilateral ( 2 char position, 2 text location, 2 texture coord, 3 color)
            int label_vertex_count = 4 * 9 * text_char_count;
            float[] label_vertices = new float[label_vertex_count];

            int label_v_index = 0;

            // Set the text vertex buffers

            // Add vertex buffers
            get_text_vertex_buffer(ref label_vertices, ref label_v_index);
            

            int label_vertex_size = label_vertex_count * sizeof(float); // Size of the label vertex buffer

            // Update the buffer
            text_buffer.UpdateDynamicVertexBuffer(label_vertices, label_vertex_size);

        }


        private void get_text_vertex_buffer(ref float[] label_vertices, ref int label_v_index)
        {

            float font_scale = gvariables_static.get_font_scale(text_font_size);

            // Find the text total width and total height of the text
            float total_label_width = 0.0f;
            float total_label_height = 0.0f;

            // lb.label[i] != '\0'

            for (int i = 0; i < text_char_count; ++i)
            {
                // get the atlas information
                char ch = text_value[i];
                Character ch_data = gvariables_static.main_font.Glyphs[ch];

                total_label_width += ch_data.Advance * font_scale;
                total_label_height = Math.Max(total_label_height, ch_data.Size.Y * font_scale);
            }


            // Get the x,y location
            Vector2 loc = text_loc;
            float x = loc.X - (total_label_width * 0.5f);

            // Whether paint above the location or not
            float y = 0.0f;
            //if (lb.label_above_loc == true)
            //{
            //    y = loc.Y + (total_label_height * 0.5f);
            //}
            //else
            //{
                y = loc.Y - (total_label_height + (total_label_height * 0.5f));
            //}


            Vector2 rotated_pt = new Vector2(0, 0);

            for (int i = 0; i < text_char_count; ++i)
            {
                // get the atlas information
                char ch = text_value[i];

                Character ch_data = gvariables_static.main_font.Glyphs[ch];

                float xpos = x + (ch_data.Bearing.X * font_scale);
                float ypos = y - (ch_data.Size.Y - ch_data.Bearing.Y) * font_scale;

                float w = ch_data.Size.X * font_scale;
                float h = ch_data.Size.Y * font_scale;

                float margin = 0.00022f; // This value prevents the minor overlap with the next char when rendering

                // Point 1
                // Vertices [0,0] // 0th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos, ypos + h), text_angle);

                // Character location
                label_vertices[label_v_index + 0] = rotated_pt.X;
                label_vertices[label_v_index + 1] = rotated_pt.Y;

                // character origin
                label_vertices[label_v_index + 2] = loc.X;
                label_vertices[label_v_index + 3] = loc.Y;

                // Texture Glyph coordinate
                label_vertices[label_v_index + 4] = ch_data.TopLeft.X + margin;
                label_vertices[label_v_index + 5] = ch_data.TopLeft.Y;

                // Text color
                label_vertices[label_v_index + 6] = text_color.X;
                label_vertices[label_v_index + 7] = text_color.Y;
                label_vertices[label_v_index + 8] = text_color.Z;

                // Iterate
                label_v_index = label_v_index + 9;

                //__________________________________________________________________________________________

                // Point 2
                // Vertices [0,1] // 1th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos, ypos), text_angle);

                // Character location
                label_vertices[label_v_index + 0] = rotated_pt.X;
                label_vertices[label_v_index + 1] = rotated_pt.Y;

                // character origin
                label_vertices[label_v_index + 2] = loc.X;
                label_vertices[label_v_index + 3] = loc.Y;

                // Texture Glyph coordinate
                label_vertices[label_v_index + 4] = ch_data.TopLeft.X + margin;
                label_vertices[label_v_index + 5] = ch_data.BottomRight.Y;

                // Text color
                label_vertices[label_v_index + 6] = text_color.X;
                label_vertices[label_v_index + 7] = text_color.Y;
                label_vertices[label_v_index + 8] = text_color.Z;

                // Iterate
                label_v_index = label_v_index + 9;

                //__________________________________________________________________________________________

                // Point 3
                // Vertices [1,1] // 2th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos + w, ypos), text_angle);

                // Character location
                label_vertices[label_v_index + 0] = rotated_pt.X;
                label_vertices[label_v_index + 1] = rotated_pt.Y;

                // character origin
                label_vertices[label_v_index + 2] = loc.X;
                label_vertices[label_v_index + 3] = loc.Y;

                // Texture Glyph coordinate
                label_vertices[label_v_index + 4] = ch_data.BottomRight.X - margin;
                label_vertices[label_v_index + 5] = ch_data.BottomRight.Y;

                // Text color
                label_vertices[label_v_index + 6] = text_color.X;
                label_vertices[label_v_index + 7] = text_color.Y;
                label_vertices[label_v_index + 8] = text_color.Z;

                // Iterate
                label_v_index = label_v_index + 9;

                //__________________________________________________________________________________________

                // Point 4
                // Vertices [1,0] // 3th point
                rotated_pt = gvariables_static.RotatePoint(loc, new Vector2(xpos + w, ypos + h), text_angle);

                // Character location
                label_vertices[label_v_index + 0] = rotated_pt.X;
                label_vertices[label_v_index + 1] = rotated_pt.Y;

                // character origin
                label_vertices[label_v_index + 2] = loc.X;
                label_vertices[label_v_index + 3] = loc.Y;

                // Texture Glyph coordinate
                label_vertices[label_v_index + 4] = ch_data.BottomRight.X - margin;
                label_vertices[label_v_index + 5] = ch_data.TopLeft.Y;

                // Text color
                label_vertices[label_v_index + 6] = text_color.X;
                label_vertices[label_v_index + 7] = text_color.Y;
                label_vertices[label_v_index + 8] = text_color.Z;

                // Iterate
                label_v_index = label_v_index + 9;

                //__________________________________________________________________________________________
                x += ch_data.Advance * font_scale;

            }

        }


        private void get_text_index_buffer(ref int[] label_indices, ref int label_i_index)
        {

            for (int i = 0; i < text_char_count; ++i)
            {
                // Set the index buffers
                int t_id = ((label_i_index / 6) * 4);

                // Triangle 0,1,2
                label_indices[label_i_index + 0] = t_id + 0;
                label_indices[label_i_index + 1] = t_id + 1;
                label_indices[label_i_index + 2] = t_id + 2;

                // Triangle 2,3,0
                label_indices[label_i_index + 3] = t_id + 2;
                label_indices[label_i_index + 4] = t_id + 3;
                label_indices[label_i_index + 5] = t_id + 0;

                // Increment
                label_i_index = label_i_index + 6;
            }

        }



    }

}
