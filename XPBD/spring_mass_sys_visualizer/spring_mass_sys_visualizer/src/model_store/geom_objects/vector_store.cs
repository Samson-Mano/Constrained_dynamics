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
    public class vector_data
    {
        public int vector_id;
        public float x_start;
        public float y_start;
        public float dir_x;
        public float dir_y;
        public float vector_amplitude = -1.0f;

        const float arrow_size = 0.4f; // Length of the arrowhead


        public vector_data(int vector_id, float x_start, float y_start, float dir_x, float dir_y)
        {
            this.vector_id = vector_id;
            this.x_start = x_start;
            this.y_start = y_start;
            this.dir_x = dir_x;
            this.dir_y = dir_y;
            this.vector_amplitude = (float)Math.Sqrt(dir_x * dir_x + dir_y * dir_y);
        }

        public void update_position(float new_x_start, float new_y_start, float new_dir_x, float new_dir_y)
        {
            this.x_start = new_x_start;
            this.y_start = new_y_start;
            this.dir_x = new_dir_x;
            this.dir_y = new_dir_y;
            this.vector_amplitude = (float)Math.Sqrt(new_dir_x * new_dir_x + new_dir_y * new_dir_y);
        }


        public List<float> vector_vertex_data()
        {
            List<float> vertexData = new List<float>();
            // Calculate the end point of the vector based on its direction and amplitude
            float x_end = x_start + dir_x;
            float y_end = y_start + dir_y;
            // Add the start and end points to the vertex data
            vertexData.Add(x_start);
            vertexData.Add(y_start);
            vertexData.Add(x_end);
            vertexData.Add(y_end);

            float arrow_head_length = 5.0f * arrow_size; // Length of the arrowhead
            float arrow_head_width = 2.0f * arrow_size; // Width of the arrowhead

            // Calculate the arrowhead points
            float arrow_x = x_end - arrow_head_length * (dir_x / vector_amplitude);
            float arrow_y = y_end - arrow_head_length * (dir_y / vector_amplitude);

            float arrow_left_x = arrow_x + arrow_head_width * (dir_y / vector_amplitude);
            float arrow_left_y = arrow_y - arrow_head_width * (dir_x / vector_amplitude);

            float arrow_right_x = arrow_x - arrow_head_width * (dir_y / vector_amplitude);
            float arrow_right_y = arrow_y + arrow_head_width * (dir_x / vector_amplitude);

            // Add the arrowhead points to the vertex data
            vertexData.Add(arrow_left_x);
            vertexData.Add(arrow_left_y);
            vertexData.Add(arrow_right_x);
            vertexData.Add(arrow_right_y);

            return vertexData;
        }

        public List<int> vector_index_data(int vertexOffset)
        {
            List<int> indexData = new List<int>();
            // The first two indices are for the line segment
            indexData.Add(vertexOffset + 0);
            indexData.Add(vertexOffset + 1);
            // The next two indices are for the arrowhead lines
            indexData.Add(vertexOffset + 1);
            indexData.Add(vertexOffset + 2);
            indexData.Add(vertexOffset + 1);
            indexData.Add(vertexOffset + 3);
            return indexData;
        }

    }


    public class vector_store : IDisposable
    {
        private Dictionary<int, vector_data> vectors;

        // Rendering resources
        private VertexArray _vectorVAO;
        private VertexBuffer _vectorVBO;

        private IndexBuffer _vectorIBO;

        public vector_store()
        {
            InitializeBuffers();

            vectors = new Dictionary<int, vector_data>();
        }

        private void InitializeBuffers()
        {
            // Initialize rendering resources
            _vectorVAO = new VertexArray();
            _vectorVBO = new VertexBuffer(10);
            _vectorIBO = new IndexBuffer(10);

            // Define the layout of the vertex buffer (2 floats for position)
            var vectorBufferLayout = new VertexBufferLayout();
            vectorBufferLayout.AddFloat(2); // Each vertex has 2 floats (x, y)

            _vectorVAO.Add_vertexBuffer(_vectorVBO, vectorBufferLayout);

        }


        public void AddVector(int vector_id, float x_start, float y_start, float dir_x, float dir_y)
        {
            // Add the vector to the dictionary
            vector_data vector = new vector_data(vector_id, x_start, y_start, dir_x, dir_y);
            vectors.Add(vector.vector_id, vector);
        }


        public void RemoveVector(int vector_id)
        {
            // Remove the vector from the dictionary
            if (vectors.ContainsKey(vector_id))
            {
                vectors.Remove(vector_id);
            }
        }


        public void updateVectorPosition(int vector_id, float new_startx, float new_starty, float new_dirx, float new_diry)
        {
            if (vectors.ContainsKey(vector_id))
            {
                vectors[vector_id].update_position(new_startx, new_starty, new_dirx, new_diry);
            }

            // After updating the position, we need to update the vertex buffer data

        }



        public void SetBufferData()
        {
            // Update the vertex buffer data for all vectors
            List<float> allVertexData = new List<float>();

            // Vector boundary indices (for drawing lines)
            List<int> boundaryindex = new List<int>();


            int vertexOffset = 0;
            foreach (var vector in vectors.Values)
            {
                allVertexData.AddRange(vector.vector_vertex_data());

                // Add indices for the vector boundary (lines)
                boundaryindex.AddRange(vector.vector_index_data(vertexOffset));


                vertexOffset += 4; // Each vector has a 4 vertices

            }

            // Update the VBO with the new vertex data
            _vectorVBO.AppendVertexBuffer(allVertexData.ToArray());

            _vectorIBO.AppendIndexBuffer(boundaryindex.ToArray());

        }



        public void UpdateVertexBuffers()
        {
            // Update the vertex buffer data for all vectors
            List<float> allVertexData = new List<float>();

            foreach (var vector in vectors.Values)
            {
                allVertexData.AddRange(vector.vector_vertex_data());
            }

            // Update the VBO with the new vertex data
            _vectorVBO.updateVertexBuffer(allVertexData.ToArray());
        }


        public void PaintVectors()
        {
            // Bind the VAO and IBO for rendering
            _vectorVAO.Bind();
            _vectorIBO.Bind();
            // Draw the vectors using GL.Lines
            GL.DrawElements(PrimitiveType.Lines, _vectorIBO.BufferCount, DrawElementsType.UnsignedInt, 0);
            // Unbind after drawing
            _vectorIBO.UnBind();
            _vectorVAO.UnBind();
        }



        public void Dispose()
        {
            _vectorVAO.Dispose();
            _vectorVBO.Dispose();
            _vectorIBO.Dispose();
        }


    }
}
