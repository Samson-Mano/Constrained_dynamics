using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPBD_soft_body_dynamics.Resources;
using XPBD_soft_body_dynamics.src.geom_objects;

namespace XPBD_soft_body_dynamics.src.fe_objects
{

    public class nodeconstraint_store
    {
        public int constraint_id { get; set; }
        public Vector2 constraint_loc { get; set; }
        public int constraint_type { get; set; } // 1 or 2

        public double constraint_angle { get; set; }

        
    }


    public class nodeconstraint_list_store
    {

        const float geom_constraint_size = 30.0f;

        // Constraint data list
        public Dictionary<int, nodeconstraint_store> constraintMap = new Dictionary<int, nodeconstraint_store>();
        public int constraint_count = 0;
        public int color_id = 0;

        // Constraint drawing data
        texture_list_store constraint_pin_drawingdata;
        texture_list_store constraint_roller_drawingdata;


        public nodeconstraint_list_store()
        {
            // (Re)Initialize the data
            constraintMap = new Dictionary<int, nodeconstraint_store>();
            constraint_count = 0;

        }


        public void add_constraint(int constraint_id, Vector2 constraint_loc, int constraint_type, double constraint_angle)
        {

            // Add a constraint data
            nodeconstraint_store new_constraint = new nodeconstraint_store();
            new_constraint.constraint_id = constraint_id;
            new_constraint.constraint_loc = constraint_loc;
            new_constraint.constraint_type = constraint_type;
            new_constraint.constraint_angle = constraint_angle;

            // Add to the map
            constraintMap.Add(new_constraint.constraint_id, new_constraint);
            constraint_count++;

        }


        public void set_constraint_visualization(float geom_size)
        {

            // Load the Texture for the costraint
            byte[] res_pinsupport = Resource_font.pic_pin_support;
            byte[] res_rollersupport = Resource_font.pic_roller_support;


            // Initialize the constraint drawing data
            constraint_pin_drawingdata = new texture_list_store(false, res_pinsupport);
            constraint_roller_drawingdata = new texture_list_store(false, res_rollersupport);

            // Set the constraint visualization for all constraints in the map

            foreach (nodeconstraint_store constraint in constraintMap.Values)
            {

                // Set the point id
                int pt_id = constraint.constraint_id; // 1 point to form a texture
                Vector2 constraint_pt = constraint.constraint_loc;

                float constraint_size = geom_constraint_size * (geom_size * 0.002f);

                int color_id = -9;

                //_____________________________________________________________________________________________
                // Mesh objects
                
                if(constraint.constraint_type == 1)
                {
                    // Pin support
                    constraint_pin_drawingdata.add_texture(pt_id, constraint_pt, 
                        constraint_size, constraint_size, constraint.constraint_angle, color_id);

                }
                else if (constraint.constraint_type == 2)
                {
                    // Roller support
                    constraint_roller_drawingdata.add_texture(pt_id, constraint_pt,
                        constraint_size, constraint_size, constraint.constraint_angle, color_id);

                }

            }

            // Set the shader
            constraint_pin_drawingdata.set_shader();
            constraint_roller_drawingdata.set_shader();

            // Set the buffer
            constraint_pin_drawingdata.set_buffer();
            constraint_roller_drawingdata.set_buffer();

            //
        }


        public void paint_constraint()
        {
            // Paint the constraint
            constraint_pin_drawingdata.paint_static_texture();
            constraint_roller_drawingdata.paint_static_texture();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            if (constraint_count == 0)
                return;

            // Update the openTK uniforms of the drawing objects
            constraint_pin_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

            constraint_roller_drawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

        }

        //

    }
    //
}
