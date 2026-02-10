using OpenTK;
using billiard_collisions_simulation.src.geom_objects;
using billiard_collisions_simulation.src.global_variables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace billiard_collisions_simulation.src.fe_objects
{
    public class elementfixedend_store
    {
        const float fixed_wall_width = 30.0f;

        // Fixed end drawing data
        meshdata_store fixed_drawingdata;


        public elementfixedend_store(float fixedend_width, float fixedend_height)
        {

            fixed_drawingdata = new meshdata_store(false);


            // Wall Section Bottom 
            fixed_drawingdata.add_mesh_point(0, -fixedend_width * 0.5f, -fixedend_height * 0.5f, 0.0, -5);
            fixed_drawingdata.add_mesh_point(1, -fixedend_width * 0.5f, (-fixedend_height * 0.5f) - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(2, fixedend_width * 0.5f, (-fixedend_height * 0.5f) - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(3, fixedend_width * 0.5f, -fixedend_height * 0.5f, 0.0, -5);

            fixed_drawingdata.add_mesh_tris(0, 0, 1, 2, -5);
            fixed_drawingdata.add_mesh_tris(1, 2, 3, 0, -5);


            // Wall Section Top 
            fixed_drawingdata.add_mesh_point(4, -fixedend_width * 0.5f, fixedend_height * 0.5f, 0.0, -5);
            fixed_drawingdata.add_mesh_point(5, -fixedend_width * 0.5f, (fixedend_height * 0.5f) + fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(6, fixedend_width * 0.5f, (fixedend_height * 0.5f) + fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(7, fixedend_width * 0.5f, fixedend_height * 0.5f, 0.0, -5);

            fixed_drawingdata.add_mesh_tris(2, 4, 5, 6, -5);
            fixed_drawingdata.add_mesh_tris(3, 6, 7, 4, -5);


            // Wall Section Left
            fixed_drawingdata.add_mesh_point(8, (-fixedend_width * 0.5f) , (-fixedend_height * 0.5f) - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(9, (-fixedend_width * 0.5f) - fixed_wall_width, (-fixedend_height * 0.5f) - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(10, (-fixedend_width * 0.5f) - fixed_wall_width, (fixedend_height * 0.5f) + fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(11, (-fixedend_width * 0.5f), (fixedend_height * 0.5f) + fixed_wall_width, 0.0, -5);

            fixed_drawingdata.add_mesh_tris(4, 8, 9, 10, -5);
            fixed_drawingdata.add_mesh_tris(5, 10, 11, 8, -5);


            // Wall Section Right
            fixed_drawingdata.add_mesh_point(12, (fixedend_width * 0.5f), (-fixedend_height * 0.5f) - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(13, (fixedend_width * 0.5f) + fixed_wall_width, (-fixedend_height * 0.5f) - fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(14, (fixedend_width * 0.5f) + fixed_wall_width, (fixedend_height * 0.5f) + fixed_wall_width, 0.0, -5);
            fixed_drawingdata.add_mesh_point(15, (fixedend_width * 0.5f), (fixedend_height * 0.5f) + fixed_wall_width, 0.0, -5);

            fixed_drawingdata.add_mesh_tris(6, 12, 13, 14, -5);
            fixed_drawingdata.add_mesh_tris(7, 14, 15, 12, -5);


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
