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

    }



    public class billiardball_data_store
    {
        List<billiardball_data> billiardballs = new List<billiardball_data>();

        const double SIMULATION_WIDTH = 1200.0;
        const double SIMULATION_HEIGHT = 800.0;

        const string FILE_NAME = "billiardballs.json";

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
            }
            else
            {
                update_billiardball_datas(5, 10, 30);
                SaveToJson();
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

                double pos_x = radius + (SIMULATION_WIDTH - 2.0 * radius) * rnd.NextDouble();
                double pos_y = radius + (SIMULATION_HEIGHT - 2.0 * radius) * rnd.NextDouble();

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
        }


        //__________________________________________________________________________
        //__________________________________________________________________________

        public void paint_billiardballs()
        {

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


        }






    }
}
