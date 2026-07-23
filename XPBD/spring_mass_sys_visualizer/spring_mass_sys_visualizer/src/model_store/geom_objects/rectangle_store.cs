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
    public class rectangle_data
    {
        public int rectangle_id;
        public float width;
        public float height;
        public float x_position;
        public float y_position;
        public float rotation_angle;
        public bool isFilled = false; // Default to false, can be set later

        public rectangle_data(int rectangle_id, float width, float height, 
            float x_position, float y_position, float rotation_angle, bool isFilled)
        {
            this.rectangle_id = rectangle_id;
            this.width = width;
            this.height = height;
            this.x_position = x_position;
            this.y_position = y_position;
            this.rotation_angle = rotation_angle;
            this.isFilled = isFilled;
        }


        public List<float> rectangle_vertex_data()
        {
            List<float> vertexData = new List<float>();

            // Convert angle to radians
            float angleRad = MathHelper.DegreesToRadians(rotation_angle);

            // Precompute cosine and sine of the angle
            float cosAngle = (float)Math.Cos(angleRad);
            float sinAngle = (float)Math.Sin(angleRad);

            // Local rectangle corners relative to the top-middle point
            Vector2 topLeft = new Vector2(-width / 2.0f, height / 2.0f);
            Vector2 topRight = new Vector2(width / 2.0f, height / 2.0f);
            Vector2 bottomLeft = new Vector2(-width / 2.0f, -height / 2.0f);
            Vector2 bottomRight = new Vector2(width / 2.0f, -height / 2.0f);

            // Function to apply rotation to a point
            Vector2 RotatePoint(Vector2 point)
            {
                float xNew = point.X * cosAngle - point.Y * sinAngle;
                float yNew = point.X * sinAngle + point.Y * cosAngle;
                return new Vector2(xNew, yNew);
            }

            // Rotate each corner
            Vector2 rotatedTopLeft = RotatePoint(topLeft);
            Vector2 rotatedTopRight = RotatePoint(topRight);
            Vector2 rotatedBottomLeft = RotatePoint(bottomLeft);
            Vector2 rotatedBottomRight = RotatePoint(bottomRight);

            // Translate rotated points by the top-middle point (fixedend_loc)
            Vector2 finalTopLeft = rotatedTopLeft + new Vector2(x_position, y_position);
            Vector2 finalTopRight = rotatedTopRight + new Vector2(x_position, y_position);
            Vector2 finalBottomLeft = rotatedBottomLeft + new Vector2(x_position, y_position);
            Vector2 finalBottomRight = rotatedBottomRight + new Vector2(x_position, y_position);

            // Add the final vertex positions to the list
            vertexData.Add(finalTopLeft.X);
            vertexData.Add(finalTopLeft.Y);
            vertexData.Add(finalTopRight.X);
            vertexData.Add(finalTopRight.Y);
            vertexData.Add(finalBottomRight.X);
            vertexData.Add(finalBottomRight.Y);
            vertexData.Add(finalBottomLeft.X);
            vertexData.Add(finalBottomLeft.Y);

            return vertexData;

        }




    }


    public class rectangle_store
    {
        private Dictionary<int, rectangle_data> rectangles;


        // Rendering resources
        private VertexArray _rectangleVAO;
        private VertexBuffer _rectangleVBO;

        private IndexBuffer _rectangleBoundaryIBO;
        private IndexBuffer _rectangleFillIBO;

        public rectangle_store()
        {
            InitializeBuffers();

            rectangles = new Dictionary<int, rectangle_data>();
        }


        private void InitializeBuffers()
        {
            // Initialize the vertex array, vertex buffer, and index buffer for rectangles
            _rectangleVAO = new VertexArray();
            _rectangleVBO = new VertexBuffer(10); 
            _rectangleBoundaryIBO = new IndexBuffer(10); 
            _rectangleFillIBO = new IndexBuffer(10);

            var rectangleLayout = new VertexBufferLayout();
            rectangleLayout.AddFloat(2); // Each vertex has 2 floats (x, y)
            
            _rectangleVAO.Add_vertexBuffer(_rectangleVBO, rectangleLayout);
                        
        }



        public void AddRectangle(int rectangle_id, float width, float height, 
            float x_position, float y_position, float rotation_angle, bool isFilled)
        {
            // Add the rectangle to the dictionary
            rectangle_data rectangle = new rectangle_data(rectangle_id, width, height, x_position, y_position, rotation_angle, isFilled);
            rectangles.Add(rectangle.rectangle_id, rectangle);
        }

        
        public void RemoveRectangle(int rectangle_id)
        {
            // Remove the rectangle from the dictionary
            if (rectangles.ContainsKey(rectangle_id))
            {
                rectangles.Remove(rectangle_id);
            }
        }


        public void UpdateVertexBuffers()
        {
            // Update the vertex buffer data for all rectangles
            List<float> allVertexData = new List<float>();

            // Rectangle boundary indices (for drawing lines)
            List<int> boundaryindex = new List<int>();

            // Rectangle fill indices (for drawing triangles)
            List<int> fillIndex = new List<int>();

            int vertexOffset = 0;
            foreach (var rectangle in rectangles.Values)
            {
                allVertexData.AddRange(rectangle.rectangle_vertex_data());

                // Add indices for the rectangle boundary (lines)
                int[] b_ids = new int[] 
                { vertexOffset, 
                    vertexOffset + 1,  
                    vertexOffset + 2, 
                    vertexOffset + 3};

                boundaryindex.AddRange(b_ids);

                // Add the triangle indices for the rectangle fill if it is filled
                if (rectangle.isFilled)
                {

                    // Add indices for the rectangle fill (two triangles)
                    int[] f_ids = new int[]
                { vertexOffset,
                    vertexOffset + 1,
                    vertexOffset + 2,
                    vertexOffset + 2,
                    vertexOffset + 3,
                    vertexOffset};

                    fillIndex.AddRange(f_ids);
                }


                vertexOffset += 4; // Each rectangle has 4 vertices

            }

            // Update the VBO with the new vertex data
            _rectangleVBO.updateVertexBuffer(allVertexData.ToArray());

            _rectangleBoundaryIBO.AppendIndexBuffer(boundaryindex.ToArray());                    
            
            _rectangleFillIBO.AppendIndexBuffer(fillIndex.ToArray());

        }


        public void PaintRectangles()
        {
            // Bind the VAO and draw the rectangles
            _rectangleVAO.Bind();
            // Draw filled rectangles first
            if (_rectangleFillIBO.BufferCount > 0)
            {
                _rectangleFillIBO.Bind();
                GL.DrawElements(PrimitiveType.Triangles, _rectangleFillIBO.BufferCount, DrawElementsType.UnsignedInt, 0);
                _rectangleFillIBO.UnBind();
            }
            // Draw rectangle boundaries (lines)
            if (_rectangleBoundaryIBO.BufferCount > 0)
            {
                _rectangleBoundaryIBO.Bind();
                GL.DrawElements(PrimitiveType.LineLoop, _rectangleBoundaryIBO.BufferCount, DrawElementsType.UnsignedInt, 0);
                _rectangleBoundaryIBO.UnBind();
            }

            _rectangleVAO.UnBind();

        }




        }
}
