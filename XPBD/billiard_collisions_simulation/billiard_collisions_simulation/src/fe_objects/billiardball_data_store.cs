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
using System.Windows.Forms;


namespace billiard_collisions_simulation.src.fe_objects
{

    public class Vec2Data
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vec2Data() { }

        public Vec2Data(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void add(Vec2Data v, double s = 1.0f)
        {
            this.X += v.X * s;
            this.Y += v.Y * s;
 
        }

        public void addVectors(Vec2Data a, Vec2Data b)
        {
            this.X = a.X + b.X;
            this.Y = a.Y + b.Y;

        }

        public void subtract(Vec2Data v, double s = 1.0f)
        {
            this.X -= v.X * s;
            this.Y -= v.Y * s;

        }

        public void subtractVectors(Vec2Data a, Vec2Data b)
        {
            this.X = a.X - b.X;
            this.Y = a.Y - b.Y;

        }

        public double length()
        {
            return Math.Sqrt((this.X * this.X) + (this.Y * this.Y));
        }

        public void scale(double s)
        {
            this.X *= s;
            this.Y *= s;
        }

        public double dot(Vec2Data v)
        {
            return (this.X * v.X) + (this.Y * v.Y);
        }

        public Vector2 GetVector()
        {
            return new Vector2((float)this.X, (float)this.Y);   
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


        // [JsonIgnore]
        public void simulate(Vec2Data gravity, double delta_t)
        {
            this.billiardball_velocity.add(gravity, delta_t);
            this.billiardball_position.add(this.billiardball_velocity, delta_t);

        }


    }



    public class billiardball_data_store
    {
        List<billiardball_data> billiardballs = new List<billiardball_data>();
        int number_of_balls = 0;

        const double SIMULATION_WIDTH = 1200.0;
        const double SIMULATION_HEIGHT = 800.0;
        const double VELOCITY_VECTOR_SIZE = 100.0f;

        const string FILE_NAME = "billiardballs.json";

        // Drawing data
        mass_list_store mass_List = new mass_list_store();
        vector_list_store vector_List = new vector_list_store();

        // Gravity
        Vec2Data GRAVITY = new Vec2Data(0.0f, 0.0f);
        const double RESTITUTION = 1.0;


        // Simulation constants
        const double FIXED_DT = 1.0 / 120.0;
        const int CONSTRAINT_ITERATIONS = 8;
        double prev_time = 0.0;
        double accumulator = 0.0;


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

                this.number_of_balls = billiardballs.Count;
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

            this.number_of_balls = number_of_balls;    

            billiardballs.Clear();
            Random rnd = new Random();

            for (int i = 0; i < number_of_balls; i++)
            {
                double rand = rnd.NextDouble();

                double radius = min_radius * (1.0 - rand) + max_radius * rand;
                double mass = Math.PI * radius * radius;

                double pos_x = (radius + (SIMULATION_WIDTH - 2.0 * radius) * rnd.NextDouble()) - SIMULATION_WIDTH * 0.5;
                double pos_y = (radius + (SIMULATION_HEIGHT - 2.0 * radius) * rnd.NextDouble()) - SIMULATION_HEIGHT * 0.5;

                Vec2Data pos = new Vec2Data(pos_x, pos_y);
                Vec2Data vel = new Vec2Data(
                    -1.0 + 2.0 * rnd.NextDouble(),
                    -1.0 + 2.0 * rnd.NextDouble()
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
                Vector2 pos = billiardball.billiardball_position.GetVector();


                mass_List.add_mass(billiardball.billiardball_id, pos,
                    (float)billiardball.billiardball_radius, -4);


                // Find the maximum velocity
                Vector2 velo = billiardball.billiardball_velocity.GetVector();
                double speed = velo.Length;

                abs_max_velocity = Math.Max(abs_max_velocity, speed);

            }

            // To avoid division by zero
            if (abs_max_velocity < 1e-12)
                abs_max_velocity = 1.0;


            foreach (billiardball_data billiardball in billiardballs)
            {
                // Add the vectors
                Vector2 pos = billiardball.billiardball_position.GetVector();

                Vector2 velo = billiardball.billiardball_velocity.GetVector();

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


            // Reset settings
            prev_time = 0.0;
            accumulator = 0.0;


        }

        public void simulate(double time_t)
        {

            double frameTime = time_t - prev_time;
            frameTime = Math.Min(frameTime, 0.05); // avoid spiral of death

            accumulator += frameTime;

            while (accumulator >= FIXED_DT)
            {
                StepPhysics(FIXED_DT);
                accumulator -= FIXED_DT;
            }

            prev_time = time_t;
//
        }

        private void StepPhysics(double dt)
        {

            for (int i = 0; i < this.number_of_balls; i++)
            {
                // Get the ball1
                // billiardball_data ball1 = billiardballs[i];
                billiardballs[i].simulate(GRAVITY, dt);

                for (int j = i+1; j < this.number_of_balls; j++)
                {
                    // Get the ball2
                    //billiardball_data ball2 = billiardballs[j];

                    handleBallCollision(billiardballs[i], billiardballs[j]);

                }

                handleWallCollision(billiardballs[i]);
            }

            // Update the drawing
            updateDrawing();
            //

        }


        private void updateDrawing()
        {

            // Find the maximum velocity
            double abs_max_velocity = 0.0;

            foreach (billiardball_data billiardball in billiardballs)
            {
                // Add the mass 
                Vector2 pos = billiardball.billiardball_position.GetVector();


                mass_List.update_mass(billiardball.billiardball_id, pos);


                // Find the maximum velocity
                Vector2 velo = billiardball.billiardball_velocity.GetVector();
                double speed = velo.Length;

                abs_max_velocity = Math.Max(abs_max_velocity, speed);

            }

            // To avoid division by zero
            if (abs_max_velocity < 1e-12)
                abs_max_velocity = 1.0;


            foreach (billiardball_data billiardball in billiardballs)
            {
                // Add the vectors
                Vector2 pos = billiardball.billiardball_position.GetVector();

                Vector2 velo = billiardball.billiardball_velocity.GetVector();

                double speed = velo.Length;
                double velocity_scale = VELOCITY_VECTOR_SIZE * (speed / abs_max_velocity);
                Vector2 dir = Vector2.Normalize(velo);

                vector_List.update_vector(billiardball.billiardball_id, pos,
                    pos + (float)velocity_scale * dir);


            }

            //
        }


        // Collision handling
        private void handleBallCollision(billiardball_data ball1, billiardball_data ball2)
        {
            Vec2Data dir = new Vec2Data();
            dir.subtractVectors(ball1.billiardball_position, ball2.billiardball_position);
            double d = dir.length();
            if (d == 0.0 || d > (ball1.billiardball_radius + ball2.billiardball_radius))
                return;

            dir.scale(1.0 / d);

            double corr = ((ball1.billiardball_radius + ball2.billiardball_radius - d) / 2.0);
            ball1.billiardball_position.add(dir, -corr);
            ball2.billiardball_position.add(dir, corr);

            double v1 = ball1.billiardball_velocity.dot(dir);
            double v2 = ball2.billiardball_velocity.dot(dir);

            double m1 = ball1.billiardball_mass;
            double m2 = ball2.billiardball_mass;

            double newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * RESTITUTION) / (m1 + m2);
            double newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * RESTITUTION) / (m1 + m2);

            ball1.billiardball_velocity.add(dir, newV1 - v1);
            ball2.billiardball_velocity.add(dir, newV2 - v2);


        }


        private void handleWallCollision(billiardball_data ball1)
        {
            // Check collision with left wall
            if(ball1.billiardball_position.X - ball1.billiardball_radius < -SIMULATION_WIDTH * 0.5)
            {
                ball1.billiardball_position.X = (-SIMULATION_WIDTH * 0.5) + ball1.billiardball_radius;
                ball1.billiardball_velocity.X = -ball1.billiardball_velocity.X;
            }


            // Check collision with right wall
            if (ball1.billiardball_position.X + ball1.billiardball_radius > SIMULATION_WIDTH * 0.5)
            {
                ball1.billiardball_position.X = (SIMULATION_WIDTH * 0.5) - ball1.billiardball_radius;
                ball1.billiardball_velocity.X = -ball1.billiardball_velocity.X;
            }


            // Check collision with top wall
            if (ball1.billiardball_position.Y - ball1.billiardball_radius < -SIMULATION_HEIGHT * 0.5)
            {
                ball1.billiardball_position.Y = (-SIMULATION_HEIGHT * 0.5) + ball1.billiardball_radius;
                ball1.billiardball_velocity.Y = -ball1.billiardball_velocity.Y;
            }

            // Check collision with bottom wall
            if (ball1.billiardball_position.Y + ball1.billiardball_radius > SIMULATION_HEIGHT * 0.5)
            {
                ball1.billiardball_position.Y = (SIMULATION_HEIGHT * 0.5) - ball1.billiardball_radius;
                ball1.billiardball_velocity.Y = -ball1.billiardball_velocity.Y;
            }

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
