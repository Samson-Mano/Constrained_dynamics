using OpenTK;
using PBD_pendulum_simulation.src.geom_objects;
using PBD_pendulum_simulation.src.global_variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBD_pendulum_simulation.src.fe_objects
{
    public class elementfixedend_store
    {
        const float fixed_end_width = 30.0f;
        const float fixed_end_height = 300.0f;

        // Fixed end drawing data
        meshdata_store fixed_drawingdata;


        public elementfixedend_store(Vector2 fixedend_loc, float fixedend_angle)
        {
            // Convert angle to radians
            float angleRad = MathHelper.DegreesToRadians(fixedend_angle);

            // Precompute cosine and sine of the angle
            float cosAngle = (float)Math.Cos(angleRad);
            float sinAngle = (float)Math.Sin(angleRad);

            // Local rectangle corners relative to the top-middle point
            Vector2 topLeft = new Vector2(-fixed_end_width, fixed_end_height / 2.0f);
            Vector2 topRight = new Vector2(0.0f, fixed_end_height / 2.0f);
            Vector2 bottomLeft = new Vector2(-fixed_end_width, -fixed_end_height / 2.0f);
            Vector2 bottomRight = new Vector2(0.0f, -fixed_end_height / 2.0f);

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
            Vector2 finalTopLeft = rotatedTopLeft + fixedend_loc;
            Vector2 finalTopRight = rotatedTopRight + fixedend_loc;
            Vector2 finalBottomLeft = rotatedBottomLeft + fixedend_loc;
            Vector2 finalBottomRight = rotatedBottomRight + fixedend_loc;


            fixed_drawingdata = new meshdata_store(false);

            // Create the geometry
            // Fixed end point
            fixed_drawingdata.add_mesh_point(0, finalTopLeft.X, finalTopLeft.Y, 0.0, -5);
            fixed_drawingdata.add_mesh_point(1, finalTopRight.X, finalTopRight.Y, 0.0, -5);
            fixed_drawingdata.add_mesh_point(2, finalBottomRight.X, finalBottomRight.Y, 0.0, -5);
            fixed_drawingdata.add_mesh_point(3, finalBottomLeft.X, finalBottomLeft.Y, 0.0, -5);

            // Fixed end 
            fixed_drawingdata.add_mesh_tris(0, 0, 1, 2, -5);
            fixed_drawingdata.add_mesh_tris(1, 2, 3, 0, -5);

            // Set the shader
            fixed_drawingdata.set_shader();

            // Set the buffer
            fixed_drawingdata.set_buffer();


        }


        public void paint_fixedend()
        {
            // Paint the fixed end
            fixed_drawingdata.paint_static_mesh();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            fixed_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

        }





    }
}
