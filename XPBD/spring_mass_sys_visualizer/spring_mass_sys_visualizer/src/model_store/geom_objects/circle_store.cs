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
    public class circle_data
    {
        public int circle_id;
        public float radius;
        public float x_position;
        public float y_position;
        public bool isFilled = false; // Default to false, can be set later

        public const int circle_vertex_count = 36; // Number of vertices to approximate the circle

        public circle_data(int circle_id, float radius, float x_position, float y_position, bool isFilled)
        {
            this.circle_id = circle_id;
            this.radius = radius;
            this.x_position = x_position;
            this.y_position = y_position;
            this.isFilled = isFilled;
        }

        public void update_position(float new_x, float new_y)
        {
            this.x_position = new_x;
            this.y_position = new_y;
        }


        public List<float> circle_vertex_data()
        {
            List<float> vertexData = new List<float>();

            // Add the origin point for the filled circle (center of the circle)
            vertexData.Add(x_position);
            vertexData.Add(y_position);

            for (int i = 0; i < circle_vertex_count; i++)
            {
                float angle = 2 * (float)Math.PI * i / circle_vertex_count;
                float x = x_position + radius * (float)Math.Cos(angle);
                float y = y_position + radius * (float)Math.Sin(angle);
                vertexData.Add(x);
                vertexData.Add(y);
            }

            return vertexData;
        }



        public List<int> circle_boundary_index_data(int vertexOffset)
        {
            List<int> indexData = new List<int>();
            for (int i = 1; i < circle_vertex_count; i++)
            {
                indexData.Add(vertexOffset + i);
                indexData.Add(vertexOffset + i + 1);
            }

            indexData.Add(vertexOffset + circle_vertex_count); // LineStrip with degenerate vertices
            indexData.Add(vertexOffset + 1); // Close the loop by connecting the last vertex to the first


            return indexData;
        }

        public List<int> circle_fill_index_data(int vertexOffset)
        {
            List<int> indexData = new List<int>();
            for (int i = 1; i < circle_vertex_count; i++)
            {
                indexData.Add(vertexOffset);
                indexData.Add(vertexOffset + i);
                indexData.Add(vertexOffset + i + 1);
            }

            // Final triangle to close the fan
            indexData.Add(vertexOffset);
            indexData.Add(vertexOffset + circle_vertex_count);
            indexData.Add(vertexOffset + 1);

            return indexData;
        }

    }




    public class circle_store : IDisposable
    {
        private Dictionary<int, circle_data> circles;


        // Rendering resources
        private VertexArray _circleVAO;
        private VertexBuffer _circleVBO;

        private IndexBuffer _circleBoundaryIBO;
        private IndexBuffer _circleFillIBO;

        public circle_store()
        {
            InitializeBuffers();

            circles = new Dictionary<int, circle_data>();
        }



        private void InitializeBuffers()
        {
            // Initialize the vertex array, vertex buffer, and index buffer for circles
            _circleVAO = new VertexArray();
            _circleVBO = new VertexBuffer(10);
            _circleBoundaryIBO = new IndexBuffer(10);
            _circleFillIBO = new IndexBuffer(10);

            var circleBufferLayout = new VertexBufferLayout();
            circleBufferLayout.AddFloat(2); // Each vertex has 2 floats (x, y)

            _circleVAO.Add_vertexBuffer(_circleVBO, circleBufferLayout);

        }



        public void AddCircle(int circle_id, float radius, float x_position, float y_position, bool isFilled)
        {
            // Add the circle to the dictionary
            circle_data circle = new circle_data(circle_id, radius, x_position, y_position, isFilled);
            circles.Add(circle.circle_id, circle);
        }


        public void RemoveCircle(int circle_id)
        {
            // Remove the circle from the dictionary
            if (circles.ContainsKey(circle_id))
            {
                circles.Remove(circle_id);
            }
        }


        public void updateCirclePosition(int circle_id, float new_x, float new_y)
        {
            if (circles.ContainsKey(circle_id))
            {
                circles[circle_id].update_position(new_x, new_y);
            }

            // After updating the position, we need to update the vertex buffer data

        }


        public void SetBufferData()
        {
            // Update the vertex buffer data for all circles
            List<float> allVertexData = new List<float>();

            // Circle boundary indices (for drawing lines)
            List<int> boundaryindex = new List<int>();

            // Circle fill indices (for drawing triangles)
            List<int> fillIndex = new List<int>();

            int vertexOffset = 0;
            foreach (var circle in circles.Values)
            {
                allVertexData.AddRange(circle.circle_vertex_data());

                // Add indices for the circle boundary (lines)
                boundaryindex.AddRange(circle.circle_boundary_index_data(vertexOffset));

                if (circle.isFilled)
                {
                    // Add the triangle indices for the circle fill if it is filled
                    fillIndex.AddRange(circle.circle_fill_index_data(vertexOffset));
                }

                vertexOffset += (circle_data.circle_vertex_count  + 1); // Each circle has a certain number of vertices

            }

            // Update the VBO with the new vertex data
            _circleVBO.AppendVertexBuffer(allVertexData.ToArray());

            _circleBoundaryIBO.AppendIndexBuffer(boundaryindex.ToArray());

            _circleFillIBO.AppendIndexBuffer(fillIndex.ToArray());

        }



        public void UpdateVertexBuffers()
        {
            // Update the vertex buffer data for all circles
            List<float> allVertexData = new List<float>();

            foreach (var circle in circles.Values)
            {
                allVertexData.AddRange(circle.circle_vertex_data());
            }

            // Update the VBO with the new vertex data
            _circleVBO.updateVertexBuffer(allVertexData.ToArray());
        }



        public void PaintCircles()
        {
            // Bind the VAO and draw the circles
            _circleVAO.Bind();

            // Draw filled circles first
            if (_circleFillIBO.BufferCount > 0)
            {
                _circleFillIBO.Bind();
                GL.DrawElements(PrimitiveType.Triangles, _circleFillIBO.BufferCount, DrawElementsType.UnsignedInt, 0);
                _circleFillIBO.UnBind();
            }

            // Draw circle boundaries (lines)
            if (_circleBoundaryIBO.BufferCount > 0)
            {
                _circleBoundaryIBO.Bind();
                GL.DrawElements(PrimitiveType.Lines, _circleBoundaryIBO.BufferCount, DrawElementsType.UnsignedInt, 0);
                _circleBoundaryIBO.UnBind();
            }

            _circleVAO.UnBind();

        }



        public void Dispose()
        {
            // Dispose of OpenGL resources
            _circleVAO.Dispose();
            _circleVBO.Dispose();
            _circleBoundaryIBO.Dispose();
            _circleFillIBO.Dispose();
        }

    }
}
