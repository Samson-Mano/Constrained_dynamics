// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPBD_soft_body_dynamics.Resources;
using XPBD_soft_body_dynamics.src.geom_objects;
using XPBD_soft_body_dynamics.src.global_variables;
using XPBD_soft_body_dynamics.src.opentk_control.opentk_bgdraw;

namespace XPBD_soft_body_dynamics.src.fe_objects
{
    public class mass_data
    {
        public int mass_id { get; set; }
        public Vector2 mass_loc { get; set; }
        public float mass_size { get; set; } // Varies between 0.0 to 1.0

    }



    public class mass_list_store
    {

        const float geom_mass_size = 30.0f;

        // Mass data list
        public Dictionary<int, mass_data> massMap = new Dictionary<int, mass_data>();
        public int mass_count = 0;
        public int color_id = 0;

        // Mass drawing data
        texture_list_store mass_drawingdata;



        public mass_list_store()
        {
            // (Re)Initialize the data
            massMap = new Dictionary<int, mass_data>();
            mass_count = 0;

        }


        public void add_mass(int mass_id, Vector2 mass_loc, float mass_size, int color_id)
        {

            // Add a mass data
            mass_data new_mass = new mass_data();
            new_mass.mass_id = mass_id;
            new_mass.mass_loc = mass_loc;
            new_mass.mass_size = mass_size;

            this.color_id = color_id;

            // Add to the map
            massMap.Add(new_mass.mass_id, new_mass);
            mass_count++;

        }

        public void update_mass(int mass_id, Vector2 mass_loc, double norm_defl_scale)
        {
            // Update the point data

            mass_drawingdata.update_texturecenter(mass_id, mass_loc, norm_defl_scale);

            //
        }


        public void set_mass_visualization(float geom_size)
        {

            // Load the Texture for mass
            byte[] res_3dcirclepic = Resource_font.pic_3D_circle_paint;


            // Initialize the mass drawing data
            mass_drawingdata = new texture_list_store(false, res_3dcirclepic);

            // Set the mass visualization for all masses in the map

            foreach (mass_data mass in massMap.Values)
            {

                // Set the point id
                int pt_id = mass.mass_id; // 1 point to form a texture
                Vector2 mass_pt = mass.mass_loc;

                float mass_dia = geom_mass_size * mass.mass_size * (geom_size * 0.002f);

                int color_id = -3;

                //_______________________________________________________________________
                //_____________________________________________________________________________________________
                // Mesh objects

                mass_drawingdata.add_texture(pt_id, mass_pt, mass_dia, mass_dia, color_id);

                //

            }

            // Set the shader
            mass_drawingdata.set_shader();

            // Set the buffer
            mass_drawingdata.set_buffer();

        }


        public void paint_pointmass()
        {
            // Paint the mass
            mass_drawingdata.paint_dynamic_texture();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            if (mass_count == 0)
                return;

            // Update the openTK uniforms of the drawing objects
            mass_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

        }
        
        //
    }
    //
}
