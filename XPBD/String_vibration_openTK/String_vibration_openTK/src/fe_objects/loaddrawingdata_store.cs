using OpenTK;
using String_vibration_openTK.src.geom_objects;
using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace String_vibration_openTK.src.fe_objects
{
    public  class loaddrawingdata_store
    {
        // load drawing data
        meshdata_store loaddrawingdata;
        label_list_store loadlabeldata;

        // Drawing data
        Vector2 stringline_start_loc;
        Vector2 stringline_end_loc;
        int segmentCount;

        bool isLoadExists = false;

        const float y_drawing_scale = 200.0f;
        const float y_arrow_size = 30.0f;

        const int LOAD_COLOR = -8;

        int pt_id = 0;


        public loaddrawingdata_store(Vector2 start_loc, Vector2 end_loc, int segmentCount)
        {
            // Initialize/ reset drawing data
            loaddrawingdata = new meshdata_store(false);
            loadlabeldata = new label_list_store(6.0f);

            // Flag to track drawing
            isLoadExists = false;

            pt_id = 0;

            // Store the drawing data
            stringline_start_loc = start_loc;
            stringline_end_loc = end_loc;
            this.segmentCount = segmentCount;

        }



        public void add_load(int load_id, List<int> load_nodes, List<double> load_values,
            double total_abs_max_value)
        {
            // Create the load profile
            int load_Count = load_nodes.Count;

            int labelIndex = -1;
            double maxAbsValue = double.MinValue;

            // Find the label index of the maximum value
            for (int i = 0; i < load_Count; i++)
            {
                double absVal = Math.Abs(load_values[i]);
                if (absVal > maxAbsValue)
                {
                    maxAbsValue = absVal;
                    labelIndex = i;
                }
            }


            float invSegments = 1.0f / (float)segmentCount;


            for (int i = 0; i < load_Count; i++)
            {
                // get the node id and load value at that node
                int nd_id = load_nodes[i];
                double value = load_values[i];
                int load_sign = Math.Sign(value);

                // calculate the x location
                float t = nd_id * invSegments;
                Vector2 nd_pt = Vector2.Lerp(stringline_start_loc, stringline_end_loc, t);

                // Load pt location
                Vector2 load_pt = new Vector2(nd_pt.X, nd_pt.Y + (float)(value / total_abs_max_value) * y_drawing_scale);

                // Create the load arrow point
                Vector2 arrow_pt1 = new Vector2(0.0f, 0.0f - y_arrow_size * load_sign);
                Vector2 arrow_pt2 = new Vector2(0.0f, 0.0f - y_arrow_size * load_sign);

                // Roate the point
                double radian1 = ((90.0 + 90.0 + 20.0) * Math.PI) / 180.0d;

                arrow_pt1 = new Vector2((arrow_pt1.X * (float)Math.Cos(radian1)) + (arrow_pt1.Y * (float)Math.Sin(radian1)),
                            -(arrow_pt1.X * (float)Math.Sin(radian1)) + (arrow_pt1.Y * (float)Math.Cos(radian1))); // 1

                arrow_pt1 = nd_pt + arrow_pt1;

                radian1 = ((90.0 + 90.0 - 20.0) * Math.PI) / 180.0d;

                arrow_pt2 = new Vector2((arrow_pt2.X * (float)Math.Cos(radian1)) + (arrow_pt2.Y * (float)Math.Sin(radian1)),
                            -(arrow_pt2.X * (float)Math.Sin(radian1)) + (arrow_pt2.Y * (float)Math.Cos(radian1))); // 1

                arrow_pt2 = nd_pt + arrow_pt2;

                // Create the drawing data
                loaddrawingdata.add_mesh_point(pt_id, load_pt.X, load_pt.Y, 0.0, LOAD_COLOR);
                loaddrawingdata.add_mesh_point(pt_id + 1, nd_pt.X, nd_pt.Y, 0.0, LOAD_COLOR);
                loaddrawingdata.add_mesh_point(pt_id + 2, arrow_pt1.X, arrow_pt1.Y, 0.0, LOAD_COLOR);
                loaddrawingdata.add_mesh_point(pt_id + 3, arrow_pt2.X, arrow_pt2.Y, 0.0, LOAD_COLOR);


                loaddrawingdata.add_mesh_lines(pt_id, pt_id, pt_id + 1, LOAD_COLOR);
                loaddrawingdata.add_mesh_lines(pt_id + 1, pt_id + 1, pt_id + 2, LOAD_COLOR);
                loaddrawingdata.add_mesh_lines(pt_id + 2, pt_id + 1, pt_id + 3, LOAD_COLOR);

                pt_id = pt_id + 4;


                // Create the label for showing the initial condition
                if (i == labelIndex)
                {
                    string label = $"id = {load_id}, load = {value.ToString("F4")}";
           
                    loadlabeldata.add_label(load_id, label, load_pt, LOAD_COLOR);

                }

            }


            // Set the shader
            loaddrawingdata.set_shader();
            loadlabeldata.set_shader();

            // Set the buffer
            loaddrawingdata.set_buffer();
            loadlabeldata.set_buffer();


            isLoadExists = true;

        }



        public void paint_loaddrawing()
        {
            if (!isLoadExists) return;

            // Paint the load drawing data
            //gvariables_static.PointSize = 6.0f;
            //loaddrawingdata.paint_static_mesh_points();
            //gvariables_static.PointSize = 1.0f;


            loaddrawingdata.paint_static_mesh_lines();

            loadlabeldata.paint_static_labels();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            if (!isLoadExists) return;

            // Update the openTK uniforms of the drawing objects
            loaddrawingdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);

            loadlabeldata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency, projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);


        }






    }
}
