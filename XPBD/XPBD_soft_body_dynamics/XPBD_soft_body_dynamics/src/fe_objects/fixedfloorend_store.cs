using OpenTK;
using XPBD_soft_body_dynamics.src.geom_objects;
using XPBD_soft_body_dynamics.src.global_variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPBD_soft_body_dynamics.src.fe_objects
{
    public class fixedfloorend_store
    {
        const float fixed_wall_width = 30.0f;

        // Fixed end drawing data
        meshdata_store fixed_drawingdata;


        public fixedfloorend_store(Vector2 floor_center)
        {

            fixed_drawingdata = new meshdata_store(false);


            // Wall Section Bottom 
            fixed_drawingdata.add_mesh_point(0, -1e+6, floor_center.Y, 0.0, -5);
            fixed_drawingdata.add_mesh_point(1, -1e+6, floor_center.Y - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(2, 1e+6, floor_center.Y - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(3, 1e+6, floor_center.Y, 0.0, -5);

            fixed_drawingdata.add_mesh_tris(0, 0, 1, 2, -5);
            fixed_drawingdata.add_mesh_tris(1, 2, 3, 0, -5);



            // Set the shader
            fixed_drawingdata.set_shader();

            // Set the buffer
            fixed_drawingdata.set_buffer();
//
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
