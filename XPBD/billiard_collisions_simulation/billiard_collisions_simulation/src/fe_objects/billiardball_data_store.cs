using billiard_collisions_simulation.src.global_variables;
using billiard_collisions_simulation.src.opentk_control.opentk_bgdraw;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace billiard_collisions_simulation.src.fe_objects
{

    public class Vec2Data
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vec2Data() { }

        public Vec2Data(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class billiardball_data
    {
        public int billiardball_id { get; set; }
        public double billiardball_mass { get; set; }
        public double billiardball_radius { get; set; }


        public Vec2Data billiardball_position { get; set; }
        public Vec2Data billiardball_velocity { get; set; }


        // will NOT be written to or read from JSON
        // [JsonIgnore]
        // public elementmass_store fe_mass;



    }



    public class billiardball_data_store
    {
        List<billiardball_data> billiardballs = new List<billiardball_data>();

        const double SIMULATION_WIDTH = 1200.0;
        const double SIMULATION_HEIGHT = 800.0;
        const double VELOCITY_VECTOR_SIZE = 100.0f;

        const string FILE_NAME = "billiardballs.json";

        // Drawing data
        mass_list_store mass_List = new mass_list_store();
        vector_list_store vector_List = new vector_list_store();


        public billiardball_data_store()
        {
            // Constructor (Read the JSON and setup default)

            LoadFromJsonOrCreateDefault();

        }

        void LoadFromJsonOrCreateDefault()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                billiardballs = JsonConvert.DeserializeObject<List<billiardball_data>>(json);

                //____________________________________________________________________________________________________________
                // Set the drawing data
                set_drawing_data();
            }
            else
            {
                update_billiardball_datas(5, 10, 30);
                // SaveToJson();
            }
        }

        public void SaveToJson()
        {
            string json = JsonConvert.SerializeObject(billiardballs, Formatting.Indented);
            File.WriteAllText(FILE_NAME, json);

        }

        public void update_billiardball_datas(int number_of_balls, double min_radius, double max_radius)
        {
            // Check whether max radius or min radius is within the bound
            if (2.0 * max_radius > SIMULATION_HEIGHT)
            {
                max_radius = SIMULATION_HEIGHT * 0.9;
                //
            }


            if (min_radius >= max_radius)
            {
                min_radius = max_radius;
                //
            }



            billiardballs.Clear();
            Random rnd = new Random();

            for (int i = 0; i < number_of_balls; i++)
            {
                double rand = rnd.NextDouble();

                double radius = min_radius * (1.0 - rand) + max_radius * rand;
                double mass = Math.PI * radius * radius;

                double pos_x = (radius + (SIMULATION_WIDTH - 2.0 * radius) * rnd.NextDouble()) - SIMULATION_WIDTH * 0.5;
                double pos_y = (radius + (SIMULATION_HEIGHT - 2.0 * radius) * rnd.NextDouble()) - SIMULATION_HEIGHT * 0.5;

                Vec2Data pos = new Vec2Data((float)pos_x, (float)pos_y);
                Vec2Data vel = new Vec2Data(
                    -1.0f + 2.0f * (float)rnd.NextDouble(),
                    -1.0f + 2.0f * (float)rnd.NextDouble()
                );

                billiardballs.Add(new billiardball_data()
                {
                    billiardball_id = i,
                    billiardball_mass = mass,
                    billiardball_radius = radius,
                    billiardball_position = pos,
                    billiardball_velocity = vel
                });
            }

            SaveToJson();

            //____________________________________________________________________________________________________________
            // Set the drawing data
            set_drawing_data();
        }


        //__________________________________________________________________________
        //__________________________________________________________________________

        private void set_drawing_data()
        {
            mass_List = new mass_list_store();
            vector_List = new vector_list_store();

            // Find the maximum velocity
            double abs_max_velocity = 0.0;

            foreach (billiardball_data billiardball in billiardballs)
            {
                // Add the mass 
                Vector2 pos = new Vector2(billiardball.billiardball_position.X,
                    billiardball.billiardball_position.Y);


                mass_List.add_mass(billiardball.billiardball_id, pos,
                    (float)billiardball.billiardball_radius, -4);


                // Find the maximum velocity
                Vector2 velo = new Vector2(billiardball.billiardball_velocity.X,
                    billiardball.billiardball_velocity.Y);
                double speed = velo.Length;

                abs_max_velocity = Math.Max(abs_max_velocity, speed);

            }

            // To avoid division by zero
            if (abs_max_velocity < 1e-12)
                abs_max_velocity = 1.0;


            foreach (billiardball_data billiardball in billiardballs)
            {
                // Add the vectors
                Vector2 pos = new Vector2(billiardball.billiardball_position.X,
                    billiardball.billiardball_position.Y);

                Vector2 velo = new Vector2(billiardball.billiardball_velocity.X,
                    billiardball.billiardball_velocity.Y);

                double speed = velo.Length;
                double velocity_scale = VELOCITY_VECTOR_SIZE * (speed / abs_max_velocity);
                Vector2 dir = Vector2.Normalize(velo);



                vector_List.add_vector(billiardball.billiardball_id, pos,
                    pos + (float)velocity_scale * dir, -9);


            }

            // Finalize the visualization
            mass_List.set_mass_visualization();

            vector_List.set_vector_visualization();



        }



        public void paint_billiardballs()
        {
            // Paint the billiard ball mass
            mass_List.paint_pointmass();

            vector_List.paint_vectors();

        }



        public void reset_simulation()
        {


        }

        public void simulate(double time_t)
        {


        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
                          drawing_events graphic_events_control)
        {

            // Update mass openTK uniforms
            mass_List.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            vector_List.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


        }






    }
}
