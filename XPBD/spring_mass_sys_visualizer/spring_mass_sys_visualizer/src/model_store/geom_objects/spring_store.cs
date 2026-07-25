using spring_mass_sys_visualizer.src.global_variables;
using spring_mass_sys_visualizer.src.opentk_control.opentk_buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;


namespace spring_mass_sys_visualizer.src.model_store.geom_objects
{
    public class spring_data
    {
        public int spring_id;
        public float x_start;
        public float y_start;
        public float x_end;
        public float y_end;
        
        public float spring_rest_length = -1.0f;
        public float spring_current_length = -1.0f;

        public const int turn_count = 11;
        const float spring_element_width = 3.0f;


        public spring_data(int spring_id, float x_start, float y_start, float x_end, float y_end)
        {
            this.spring_id = spring_id;
            this.x_start = x_start;
            this.y_start = y_start;
            this.x_end = x_end;
            this.y_end = y_end;

            // Set the spring rest length based on the initial positions
            this.spring_rest_length = (float)Math.Sqrt(Math.Pow(x_end - x_start, 2) + Math.Pow(y_end - y_start, 2));
            this.spring_current_length = this.spring_rest_length;
        }


        public void update_position(float new_x_start, float new_y_start, float new_x_end, float new_y_end)
        {
            this.x_start = new_x_start;
            this.y_start = new_y_start;
            this.x_end = new_x_end;
            this.y_end = new_y_end;

            // Update the current length of the spring based on the new positions
            this.spring_current_length = (float)Math.Sqrt(Math.Pow(new_x_end - new_x_start, 2) + Math.Pow(new_y_end - new_y_start, 2));
        }
        


        public List<float> spring_vertex_data()
        {
            List<float> vertexData = new List<float>();

            // Spring portion l_cosine, m_sine
            float l_cos = (x_end - x_start) / spring_current_length;
            float m_sin = (y_start  - y_end) / spring_current_length;

            // Find the scale factors 
            // The idea is to keep the flat line length rigid but only change the spring portion length

            float factor1 = 0.2f * (spring_rest_length / spring_current_length);
            float factor2 = 1.0f - (2.0f * factor1);
            float factor3 = 1.0f - factor1;


            // Spring start flat portion
            vertexData.Add(x_start);
            vertexData.Add(y_start);

            vertexData.Add((1.0f - factor1) * x_start + factor1 * x_end);
            vertexData.Add((1.0f - factor1) * y_start + factor1 * y_end);


            Vector2 origin_pt = new Vector2((1.0f - factor1) * x_start + factor1 * x_end, 
                (1.0f - factor1) * y_start + factor1 * y_end);

            // Spring zig-zag portion
            for (int i = 1; i < turn_count; i++)
            {
                float param_t = i / (float)(turn_count);

                float pt_x = (param_t * spring_current_length * factor2);
                float pt_y = spring_element_width * ((i % 2 == 0) ? 1 : -1);

                Vector2 curr_pt = new Vector2(((l_cos * pt_x) + (m_sin * pt_y)),
                    ((-1.0f * m_sin * pt_x) + (l_cos * pt_y)));

                curr_pt = curr_pt + origin_pt;

                // Add pt
                vertexData.Add(curr_pt.X);
                vertexData.Add(curr_pt.Y);

            }


            // Spring end flat portion
            vertexData.Add((1.0f - factor3) * x_start + factor3 * x_end);
            vertexData.Add((1.0f - factor3) * y_start + factor3 * y_end);

            vertexData.Add(x_end);
            vertexData.Add(y_end);

            return vertexData;
        }


        public List<int> spring_line_index_data(int vertexOffset)
        {
            List<int> indexData = new List<int>();
            // The spring has (turn_count + 3) vertices
            for (int i = 0; i < turn_count + 2; i++)
            {
                indexData.Add(vertexOffset + i);
                indexData.Add(vertexOffset + i + 1);
            }
            return indexData;
        }


    }




    public class spring_store : IDisposable
    {
        private Dictionary<int, spring_data> springs;


        // Rendering resources
        private VertexArray _springVAO;
        private VertexBuffer _springVBO;

        private IndexBuffer _springIBO;



        public spring_store()
        {
            InitializeBuffers();

            springs = new Dictionary<int, spring_data>();

        }



        private void InitializeBuffers()
        {
            // Initialize rendering resources
            _springVAO = new VertexArray();
            _springVBO = new VertexBuffer(10);
            _springIBO = new IndexBuffer(10);

            // Define the layout of the vertex buffer (2 floats for position)
            var springBufferLayout = new VertexBufferLayout();
            springBufferLayout.AddFloat(2); // Each vertex has 2 floats (x, y)

            _springVAO.Add_vertexBuffer(_springVBO, springBufferLayout);

        }



        public void AddSpring(int spring_id, float x_start, float y_start, float x_end, float y_end)
        {
            // Add the spring to the dictionary
            spring_data spring = new spring_data(spring_id, x_start, y_start, x_end, y_end);
            springs.Add(spring.spring_id, spring);
        }


        public void RemoveSpring(int spring_id)
        {
            // Remove the spring from the dictionary
            if (springs.ContainsKey(spring_id))
            {
                springs.Remove(spring_id);
            }
        }


        public void updateSpringPosition(int spring_id, float new_startx, float new_starty, float new_endx, float new_endy)
        {
            if (springs.ContainsKey(spring_id))
            {
                springs[spring_id].update_position(new_startx, new_starty, new_endx, new_endy);
            }

            // After updating the position, we need to update the vertex buffer data

        }



        public void SetBufferData()
        {
            // Update the vertex buffer data for all springs
            List<float> allVertexData = new List<float>();

            // Spring boundary indices (for drawing lines)
            List<int> boundaryindex = new List<int>();


            int vertexOffset = 0;
            foreach (var spring in springs.Values)
            {
                allVertexData.AddRange(spring.spring_vertex_data());

                // Add indices for the spring boundary (lines)
                boundaryindex.AddRange(spring.spring_line_index_data(vertexOffset));


                vertexOffset += spring_data.turn_count + 3; // Each spring has a certain number of vertices

            }

            // Update the VBO with the new vertex data
            _springVBO.AppendVertexBuffer(allVertexData.ToArray());

            _springIBO.AppendIndexBuffer(boundaryindex.ToArray());

        }



        public void UpdateVertexBuffers()
        {
            // Update the vertex buffer data for all springs
            List<float> allVertexData = new List<float>();

            foreach (var spring in springs.Values)
            {
                allVertexData.AddRange(spring.spring_vertex_data());
            }

            // Update the VBO with the new vertex data
            _springVBO.updateVertexBuffer(allVertexData.ToArray());
        }


        public void PaintSprings()
        {
            // Bind the VAO and IBO for rendering
            _springVAO.Bind();
            _springIBO.Bind();
            // Draw the springs using GL.Lines
            GL.DrawElements(PrimitiveType.Lines, _springIBO.BufferCount, DrawElementsType.UnsignedInt, 0);
            // Unbind after drawing
            _springIBO.UnBind();
            _springVAO.UnBind();
        }



        public void Dispose()
        {
            _springVAO.Dispose();
            _springVBO.Dispose();
            _springIBO.Dispose();
        }

    }
}
