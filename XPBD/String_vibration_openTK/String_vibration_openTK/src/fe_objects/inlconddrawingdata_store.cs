using OpenTK;
using OpenTK.Graphics.ES11;
using String_vibration_openTK.src.geom_objects;
using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace String_vibration_openTK.src.fe_objects
{

    public class inlconddrawingdata_store
    {
        // initial condition drawing data
        meshdata_store inlconddrawingdata;
        label_list_store inlcondlabeldata;

        // Drawing data
        Vector2 stringline_start_loc;
        Vector2 stringline_end_loc;
        int segmentCount;

        bool isInlConditionExists = false;

        const float y_drawing_scale = 300.0f;
        int colorID = -1;

        const int DISPLACEMENT_COLOR = -6;
        const int VELOCITY_COLOR = -7;

        int pt_id = 0;

        public inlconddrawingdata_store(int type, Vector2 start_loc, Vector2 end_loc, int segmentCount)
        {
            // Initialize/ reset drawing data
            inlconddrawingdata = new meshdata_store(false);
            inlcondlabeldata = new label_list_store(6.0f);
  
            if(type == 0)
            {
                // Initial condition is displacement
                colorID = DISPLACEMENT_COLOR;
            }
            else if(type == 1)
            {
                // Initial condition is displacement
                colorID = VELOCITY_COLOR;

            }

            isInlConditionExists = false;

            pt_id = 0;

            // Store the drawing data
            stringline_start_loc = start_loc;
            stringline_end_loc = end_loc;
            this.segmentCount = segmentCount;

        }


        public void add_initialcondition(int inlcond_id, List<int> inlcond_nodes, List<double> inlcond_values, 
            double total_abs_max_value)
        {
            // Create the displacement profile
            int inlcond_Count = inlcond_nodes.Count;
            
            int labelIndex = -1;
            double maxAbsValue = double.MinValue;

            // Find the label index of the maximum value
            for (int i = 0; i < inlcond_Count; i++)
            {
                double absVal = Math.Abs(inlcond_values[i]);
                if (absVal > maxAbsValue)
                {
                    maxAbsValue = absVal;
                    labelIndex = i;
                }
            }


            float invSegments = 1.0f / (float)segmentCount;


            for (int i = 0; i < inlcond_Count; i++)
            {
                // get the node id and initial condition value at that node
                int nd_id = inlcond_nodes[i];
                double value = inlcond_values[i];

                // calculate the x location
                float t = nd_id * invSegments;
                Vector2 p = Vector2.Lerp(stringline_start_loc, stringline_end_loc, t);

                // Displ pt location
                Vector2 displ_pt = new Vector2(p.X, p.Y + (float)(value / total_abs_max_value) * y_drawing_scale);

                inlconddrawingdata.add_mesh_point(pt_id, displ_pt.X, displ_pt.Y , 0.0, colorID);

                if (i != 0)
                {
                    inlconddrawingdata.add_mesh_lines(pt_id - 1, pt_id - 1, pt_id, colorID);
                }

                pt_id++;

    

                // Create the label for showing the initial condition
                if (i == labelIndex)
                {
                    string label = "";

                    if (colorID == DISPLACEMENT_COLOR)
                    {
                        label = $"id = {inlcond_id}, idispl = {value.ToString("F4")}";
                    }
                    else if(colorID == VELOCITY_COLOR)
                    {
                        label = $"id = {inlcond_id}, ivelo = {value.ToString("F4")}";
                    }

                    inlcondlabeldata.add_label(inlcond_id, label, displ_pt, colorID);

                }
                
            }


            // Set the shader
            inlconddrawingdata.set_shader();
            inlcondlabeldata.set_shader();

            // Set the buffer
            inlconddrawingdata.set_buffer();
            inlcondlabeldata.set_buffer();


            isInlConditionExists = true;

        }



        public void paint_inlconddrawing()
        {
            if (!isInlConditionExists) return;

            // Paint the initial condition drawing data
            gvariables_static.PointSize = 6.0f;
            inlconddrawingdata.paint_static_mesh_points();
            gvariables_static.PointSize = 1.0f;


            inlconddrawingdata.paint_static_mesh_lines();

            inlcondlabeldata.paint_static_labels();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            if (!isInlConditionExists) return;

            // Update the openTK uniforms of the drawing objects
            inlconddrawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

            inlcondlabeldata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency, projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);


        }








    }
}
