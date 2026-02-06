using OpenTK;
using OpenTK.Graphics.OpenGL;
using billiard_collisions_simulation.src.geom_objects;
using billiard_collisions_simulation.src.global_variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace billiard_collisions_simulation.src.fe_objects
{
    public class elementmass_store
    {
        float ptmass_size = 20.0f;
      
        const int circle_pt_count = 30; // Circle segment count

        // Mass mesh data
        meshdata_store mass_drawingdata;


        public elementmass_store(Vector2 mass_loc, float ptmass_size)
        {
            // Point mass size
            this.ptmass_size = ptmass_size;    

            //_____________________________________________________________________________________________
            // Mesh objects
            mass_drawingdata = new meshdata_store(false);

            // Add the origin point
            mass_drawingdata.add_mesh_point(0, mass_loc.X, mass_loc.Y, 0.0, -4);

            for (int i = 0; i < circle_pt_count; i++)
            {
                // Create the points for circle
                double angle = (2.0 * Math.PI * i) / (double)circle_pt_count;
                double x = mass_loc.X + (ptmass_size * Math.Cos(angle));
                double y = mass_loc.Y + (ptmass_size * Math.Sin(angle));

                mass_drawingdata.add_mesh_point(i + 1, x, y, 0.0, -4);

            }

            // Add Triangle and lines
            for (int i = 0; i < circle_pt_count - 1;i++)
            {
                mass_drawingdata.add_mesh_lines(i, i + 1, i + 2, -4);
                mass_drawingdata.add_mesh_tris(i, 0, i + 1, i + 2, -4);

            }

            // Final segment
            mass_drawingdata.add_mesh_lines(circle_pt_count - 1, circle_pt_count, 1, -4);
            mass_drawingdata.add_mesh_tris(circle_pt_count - 1, 0, circle_pt_count, 1, -4);

            // Set the shader
            mass_drawingdata.set_shader();

            // Set the buffer
            mass_drawingdata.set_buffer();

        }


        public void update_pointmass(Vector2 mass_loc, float scaled_mass_size)
        {
            // Update the point mass displacement, velocity and acceleration

            float updated_mass_size = scaled_mass_size;


            // Update the origin point
            mass_drawingdata.update_mesh_point(0, mass_loc.X, mass_loc.Y, 0.0, 1.0);

            for (int i = 0; i < circle_pt_count; i++)
            {
                // Update the points for circle
                double angle = (2.0 * Math.PI * i) / (double)circle_pt_count;
                double x = mass_loc.X + (updated_mass_size * Math.Cos(angle));
                double y = mass_loc.Y + (updated_mass_size * Math.Sin(angle));

                mass_drawingdata.update_mesh_point(i + 1, x, y, 0.0, 1.0);

            }

        }


        public void paint_pointmass()
        {
            // Paint the mass element
            mass_drawingdata.paint_dynamic_mesh();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
             float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            mass_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                0.5f * geom_transparency);

        }


    }
}
