using OpenTK;
using String_vibration_openTK.src.geom_objects;
using String_vibration_openTK.src.global_variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MathNet.Numerics;


namespace String_vibration_openTK.src.fe_objects
{
    public class stringlinerespresults_store
    {

        // Line in tension and nodes modal results data
        meshdata_store stringline_resultsdata;

        // Store the initialization data
        Vector2 start_loc = new Vector2(0.0f, 0.0f);
        Vector2 end_loc = new Vector2(0.0f, 0.0f);
        int segmentCount = 0;

        //_________________________________________________________________________________________
        int number_of_nodes = 0;
        MathNet.Numerics.LinearAlgebra.Matrix<double> eigen_vectors;
        MathNet.Numerics.LinearAlgebra.Matrix<double> eigen_vectors_inverse;

        // Modal transformed Initial displacements & velocities matrices
        MathNet.Numerics.LinearAlgebra.Vector<double> modal_InitialDisplacementVector;
        MathNet.Numerics.LinearAlgebra.Vector<double> modal_InitialVelocityVector;

        List<transformed_load_data> transformed_Loads = new List<transformed_load_data>();   

         struct transformed_load_data
        {
            public int load_id;
            public double load_start_time;
            public double load_end_time;

            // 0 = half sine pulse, 1 = Rectangular Pulse, 
            // 2 = Triangular Pulse, 3 = Step Force with Finite Rise
            // 4 = Harmonic Excitation
            public int load_type;

            public MathNet.Numerics.LinearAlgebra.Vector<double> modal_LoadAmplitudeVector;
        }



        public stringlinerespresults_store(Vector2 start_loc, Vector2 end_loc, int segmentCount)
        {
            // Store the initialization data
            this.start_loc = start_loc;
            this.end_loc = end_loc;
            this.segmentCount = segmentCount;


            //____________________________________________
            const int node_color = -3;
            const int linesegment_color = -4;

            stringline_resultsdata = new meshdata_store(true);

            stringline_resultsdata.add_mesh_point(0, start_loc.X, start_loc.Y, 0.0f, node_color);

            float invSegments = 1.0f / (float)segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                // Find the next point by interpolation
                float t = (i + 1) * invSegments;
                Vector2 p = Vector2.Lerp(start_loc, end_loc, t);

                // Add the point
                stringline_resultsdata.add_mesh_point(i + 1, p.X, p.Y, 0.0f, node_color);

                // Add the line segment
                stringline_resultsdata.add_mesh_lines(i, i, i + 1, linesegment_color);

            }

            // Set the shader
            stringline_resultsdata.set_shader();

            // Set the buffer
            stringline_resultsdata.set_buffer();


        }


        public void initialize_response_matrices(fedata_store fe_data)
        {
            //___________________________________________________________________________________
            // Create the modal analysis results
            this.number_of_nodes = segmentCount + 1;

            int matrix_size = this.number_of_nodes - 2;

            eigen_vectors = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(matrix_size, matrix_size);

            for (int i = 0; i < matrix_size; i++)
            {
                // Mode number
                int mode_number = i + 1;

                for (int j = 0; j < matrix_size; j++)
                {
                    int node_number = j + 1;  // interior node index

                    // Length ratio
                    double length_ratio = node_number / (double)(matrix_size + 1);
                    double eigen_vec = Math.Sin(mode_number * Math.PI * length_ratio);

                    eigen_vectors[j, i] = eigen_vec;
                }
            }

            // Create the eigen vectors inverse matrix
            double inv_factor = 2.0 / (double)(matrix_size + 1.0);

            eigen_vectors_inverse = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(matrix_size, matrix_size);

            eigen_vectors_inverse = inv_factor * eigen_vectors.Transpose();


            // 1. Create the modal Initial displacement matrices
            modal_InitialDisplacementVector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(matrix_size);
            modal_InitialVelocityVector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(matrix_size);


            create_initial_conditions_vectors(
                ref modal_InitialDisplacementVector,
                ref modal_InitialVelocityVector,
                fe_data.stringintension_data.inlcond_data,
                matrix_size,
                eigen_vectors_inverse);


            // 2. Create the modal transformed load vectors
            this.transformed_Loads.Clear();

            foreach (loaddata_store load in fe_data.stringintension_data.load_data)
            {
                // Create the global load amplitude vector
                MathNet.Numerics.LinearAlgebra.Vector<double> global_LoadAmplitudeVector
                    = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(matrix_size);

                int i = 0;
                foreach (int nd in load.load_nodes)
                {
                    global_LoadAmplitudeVector[nd] += load.load_values[i];
                    i++;
                }
                // Apply the modal transformation
                MathNet.Numerics.LinearAlgebra.Vector<double> modal_LoadAmplitudeVector
                    = eigen_vectors * global_LoadAmplitudeVector;


                // Store the transformed load data
                transformed_load_data t_load = new transformed_load_data();
                t_load.load_id = load.load_id;
                t_load.load_type = load.load_type;
                t_load.load_start_time = load.load_start_time;
                t_load.load_end_time = load.load_end_time;
                t_load.modal_LoadAmplitudeVector = modal_LoadAmplitudeVector;
                this.transformed_Loads.Add(t_load);
            }




        }


        private void create_initial_conditions_vectors(
            ref MathNet.Numerics.LinearAlgebra.Vector<double> modal_InitialDisplacementVector,
            ref MathNet.Numerics.LinearAlgebra.Vector<double> modal_InitialVelocityVector,
            List<initialconditiondata_store> inlcond_data,
            int matrix_size,
            MathNet.Numerics.LinearAlgebra.Matrix<double> eigen_vectors_inverse)
        {

            MathNet.Numerics.LinearAlgebra.Vector<double> global_InitialDisplacementVector
                = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(matrix_size);

            MathNet.Numerics.LinearAlgebra.Vector<double> global_InitialVelocityVector
                = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(matrix_size);

            foreach (initialconditiondata_store inlcond in inlcond_data)
            {

                // Create the global initial condition vectors
                if (inlcond.inlcond_type == 0) // Initial Displacement
                {

                    int i = 0;
                    foreach (int nd in inlcond.inlcond_nodes)
                    {
                        global_InitialDisplacementVector[nd] += inlcond.inlcond_values[i];
                        i++;

                    }

                }
                else if (inlcond.inlcond_type == 1) // Initial Velocity
                {

                    int i = 0;
                    foreach (int nd in inlcond.inlcond_nodes)
                    {
                        global_InitialVelocityVector[nd] += inlcond.inlcond_values[i];
                        i++;

                    }

                }
            }

            // Apply the modal transformation
            modal_InitialDisplacementVector = eigen_vectors_inverse * global_InitialDisplacementVector;
            modal_InitialVelocityVector = eigen_vectors_inverse * global_InitialVelocityVector;

        }




        public void update_respresults_time_step(double elapsedRealTime, double displ_scale,
            double velo_scale, double accl_scale)
        {





        }


        public void paint_responseanalysisresults()
        {

            // Paint the string in tension (Line and nodes)
            gvariables_static.LineWidth = 2.0f;
            gvariables_static.PointSize = 6.0f;

            stringline_resultsdata.paint_dynamic_mesh_lines();
            stringline_resultsdata.paint_dynamic_mesh_points();

            gvariables_static.LineWidth = 1.0f;
            gvariables_static.PointSize = 1.0f;

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix,
            float geom_transparency)
        {
            // Update the openTK uniforms of the drawing objects
            stringline_resultsdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                projectionMatrix,
                modelMatrix,
                viewMatrix,
                geom_transparency);


        }








    }



}
