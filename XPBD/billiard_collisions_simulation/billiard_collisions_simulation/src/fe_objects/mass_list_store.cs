// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using billiard_collisions_simulation.src.geom_objects;
using billiard_collisions_simulation.src.global_variables;
using billiard_collisions_simulation.src.opentk_control.opentk_bgdraw;
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace billiard_collisions_simulation.src.fe_objects
{
    public class mass_data
    {
        public int mass_id { get; set; }
        public Vector2 mass_loc { get; set; }
        public float mass_size { get; set; }

    }



    public class mass_list_store
    {

        const int circle_pt_count = 30; // Circle segment count

        // Mass data list
        public Dictionary<int, mass_data> massMap = new Dictionary<int, mass_data>();
        public int mass_count = 0;
        public int color_id = 0;

        // Mass drawing data
        meshdata_store mass_drawingdata;


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

        public void update_mass(int mass_id, Vector2 mass_loc)
        {

            // Update the mass data
            massMap[mass_id].mass_loc = mass_loc;

            // Set the point id
            int origin_pt_id = mass_id * (circle_pt_count + 1);

            // Add the origin point
            mass_drawingdata.update_mesh_point(origin_pt_id + 0, mass_loc.X, mass_loc.Y, 0.0, -4);

            for (int i = 0; i < circle_pt_count; i++)
            {
                // Create the points for circle
                double angle = (2.0 * Math.PI * i) / (double)circle_pt_count;
                double x = mass_loc.X + (massMap[mass_id].mass_size * Math.Cos(angle));
                double y = mass_loc.Y + (massMap[mass_id].mass_size * Math.Sin(angle));

                mass_drawingdata.update_mesh_point(origin_pt_id + (i + 1), x, y, 0.0, -4);

            }

        }


        public void set_mass_visualization()
        {
            // Initialize the mass drawing data
            mass_drawingdata = new meshdata_store(false);

            // Set the mass visualization for all masses in the map

            foreach (mass_data mass in massMap.Values)
            {
                // Set the point id
                int origin_pt_id =  mass.mass_id * (circle_pt_count + 1);

 
                // Add the origin point
                mass_drawingdata.add_mesh_point(origin_pt_id + 0, mass.mass_loc.X, mass.mass_loc.Y, 0.0, -4);

                for (int i = 0; i < circle_pt_count; i++)
                {
                    // Create the points for circle
                    double angle = (2.0 * Math.PI * i) / (double)circle_pt_count;
                    double x = mass.mass_loc.X + (mass.mass_size * Math.Cos(angle));
                    double y = mass.mass_loc.Y + (mass.mass_size * Math.Sin(angle));

                    mass_drawingdata.add_mesh_point(origin_pt_id + (i + 1), x, y, 0.0, -4);

                }

                // Add Triangle and lines
                for (int i = 0; i < circle_pt_count - 1; i++)
                {
                    mass_drawingdata.add_mesh_lines(origin_pt_id + i, 
                        origin_pt_id + (i + 1), 
                        origin_pt_id + (i + 2), -4);

                    mass_drawingdata.add_mesh_tris(origin_pt_id + i, 
                        origin_pt_id + 0,
                        origin_pt_id + (i + 1),
                        origin_pt_id + (i + 2), -4);

                }

                // Final segment
                mass_drawingdata.add_mesh_lines(origin_pt_id + (circle_pt_count - 1), 
                    origin_pt_id + circle_pt_count, 
                    origin_pt_id + 1, -4);

                mass_drawingdata.add_mesh_tris(origin_pt_id + (circle_pt_count - 1), 
                    origin_pt_id + 0, 
                    origin_pt_id + circle_pt_count, 
                    origin_pt_id + 1, -4);


            }

            // Set the shader
            mass_drawingdata.set_shader();

            // Set the buffer
            mass_drawingdata.set_buffer();

        }


        public void paint_pointmass()
        {
            // Paint the mass
            mass_drawingdata.paint_dynamic_mesh();

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
                0.5f * geom_transparency);

        }

        //
    }
    //
}
