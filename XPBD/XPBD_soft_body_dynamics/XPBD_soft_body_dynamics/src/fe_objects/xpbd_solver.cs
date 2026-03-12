using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPBD_soft_body_dynamics.src.global_variables;


namespace XPBD_soft_body_dynamics.src.fe_objects
{
    public class xpbd_particle
    {
        public int particle_id;

        public Vector2 position;
        public Vector2 prev_position;
        public Vector2 predicted_position;
        public Vector2 velocity;

        public double mass;
        public double inv_mass;


        // SPC Matrix
        public Matrix2 spc_matrix;

    }

    public class xpbd_distance_constraint
    {
        public int element_id;

        public int i; // particle A
        public int j; // particle B
        public double rest_length;


        public double compliance; // 0 = rigid
        public double lambda; // Accumulated Lagrane multiplier

    }


    public class xpbd_solver
    {
        public Dictionary<int, xpbd_particle> particles = new Dictionary<int, xpbd_particle>();

        public Dictionary<int, xpbd_distance_constraint> springs = new Dictionary<int, xpbd_distance_constraint>();

        bool isModelSet = false;

        // Floor data
        Vector2 fixedend_floor_center = new Vector2();
        float particle_radius = 0.01f;


        public xpbd_solver() { }    


        public xpbd_solver(softbody_data_container softbody_data)
        {
            List<Vector3> nodePtsList = new List<Vector3>();

            // Initialize the particles
            particles = new Dictionary<int, xpbd_particle>();

            foreach (grid_data_store grid in softbody_data.grid_data)
            {
                // Create the particles
                xpbd_particle new_particle = new xpbd_particle();

                new_particle.particle_id = grid.grid_id;
                new_particle.position = grid.coord_pt.GetVector();
                new_particle.prev_position = grid.coord_pt.GetVector();
                new_particle.predicted_position = grid.coord_pt.GetVector();    
                new_particle.velocity = Vector2.Zero;

                new_particle.mass = 0.0; // Set mass as zero first
                new_particle.inv_mass = 0.0;     // Set inv mass as zero first

                new_particle.spc_matrix = Matrix2.Identity;

                particles.Add(grid.grid_id, new_particle);

                // add to the points list
                nodePtsList.Add( new Vector3(new_particle.position.X, new_particle.position.Y, 0.0f));

            }

            // Apply point mass to the data
            foreach (mass_data_store mass in softbody_data.mass_data)
            {

                // Get the mass location
                particles[mass.grid_id].mass += mass.mass_value;

            }

            //_________________________________________________________________________________________

            // Create the distance constraint
            springs = new Dictionary<int, xpbd_distance_constraint>();

            // and Apply mass of Attached CROD to the node
            foreach (crod_data_store crod in softbody_data.crod_data)
            {
                int start_grid_id = crod.start_grid_id;
                int end_grid_id = crod.end_grid_id;

                // Get the PROD Id
                int prop_id = crod.property_id;

                // Get the cross section area of the ROD element
                double crod_cs_area = softbody_data.prop_data[prop_id].cs_area;

                // Get the density of the material
                int mat_id = softbody_data.prop_data[prop_id].mat_id;

                double mat_density = softbody_data.mat_data[mat_id].mat_density_rho;

                // Get the length of the element
                Vec2Data start_pt = softbody_data.grid_data[start_grid_id].coord_pt;
                Vec2Data end_pt = softbody_data.grid_data[end_grid_id].coord_pt;

                double crod_length = Vector2.Distance(start_pt.GetVector(), end_pt.GetVector());    

                // Total mass of the rod
                double crod_mass = crod_cs_area * crod_length * mat_density;

                // Apply the mass to the nodes attached to the crod
                particles[crod.start_grid_id].mass += crod_mass * 0.5;
                particles[crod.end_grid_id].mass += crod_mass * 0.5;


                //____________________________________________________________________________________________________
                double youngs_modulus = softbody_data.mat_data[mat_id].youngs_mod;

                // Create the constraint
                xpbd_distance_constraint new_spring = new xpbd_distance_constraint();

                new_spring.element_id = crod.element_id;

                new_spring.i = start_grid_id;
                new_spring.j = end_grid_id;

                new_spring.rest_length = crod_length;

                if (Double.IsInfinity(youngs_modulus) == false)
                {
                    // Find the axial stiffeness
                    double axial_stiffness = (youngs_modulus * crod_cs_area) / crod_length; // EA/L

                    new_spring.compliance = 1.0 / axial_stiffness;
                }
                else
                {
                    new_spring.compliance = 0.0;
                }

                springs.Add(crod.element_id, new_spring);
                //
            }


            // Apply SPC Boundary condition matrix
            foreach (spc_data_store spc1 in softbody_data.spc_data)
            {
                // Get the coord
                int spc_grid_id = spc1.grid_id;

                if (spc1.spc_type == 12)
                {
                    // Pin support
                    particles[spc_grid_id].spc_matrix = Matrix2.Zero;

                }
                else if (spc1.spc_type == 1)
                {
                    // Roller support
                   double constraint_radian =  spc1.spc_angle * (Math.PI / 180.0);

                    float c_cos = (float)Math.Cos(constraint_radian);
                    float s_sin = (float)Math.Sin(constraint_radian);

                    particles[spc_grid_id].spc_matrix = new Matrix2(c_cos * c_cos, c_cos * s_sin, 
                                                                    s_sin * c_cos, s_sin * s_sin);

                }
                //
            }

            // Invert the mass
            foreach (xpbd_particle particle in particles.Values)
            {
                if(particle.mass >0)
                {
                    particle.inv_mass = 1.0 / particle.mass;
                }
                else
                {
                    particle.mass = 0.0;
                    particle.inv_mass = 0.0;
                }
                
            }

            // Create the floor
            // Find the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            Vector3 min_bounds = geom_extremes.Item1; // Minimum bound
            Vector3 max_bounds = geom_extremes.Item2; // Maximum bound

            // Find the y bound of the model
            double y_bound = max_bounds.Y - min_bounds.Y;

            Vector3 floor = new Vector3(geometry_center.X, geometry_center.Y - (float)(y_bound * 1.5f), 0.0f);

            // Set the fixed end floor
           this.fixedend_floor_center = new Vector2(floor.X, floor.Y);

            // Add the floor point to the node list
            nodePtsList.Add(floor);

            // Recalculate the boundary
            geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);

            min_bounds = geom_extremes.Item1; // Minimum bound
            max_bounds = geom_extremes.Item2; // Maximum bound

            Vector3 geom_bounds = max_bounds - min_bounds;  

            float geom_size = geom_bounds.Length;


            const float geom_node_size = 6.0f;


            // Set the particle node radius
            this.particle_radius = (geom_node_size * (geom_size * 0.002f)) * 0.5f;

            // Flag changes to model set
            isModelSet = true;

            //
        }



        public void simulate(Vector2 gravity, double dt)
        {
            if (!isModelSet) return;


            integrate_particles(gravity, dt);

            // RESET LAMBDA EACH TIME STEP
            foreach (xpbd_distance_constraint spring in springs.Values)
                spring.lambda = 0.0;


            int solver_iterations = 10;

            for (int i = 0; i < solver_iterations; i++)
            {
                solve_distance_constraints(dt);
               // solve_boundary_constraints();
                solve_collisions();
            }

            update_velocities(dt);
        }


        void integrate_particles(Vector2 gravity, double dt)
        {
            // Calculate the dv
            Vector2 gdt = gravity * (float)dt;

            foreach (xpbd_particle particle in particles.Values)
            {
                if (particle.inv_mass == 0) continue;

                // Velocity increment
                particle.velocity += gdt;

                // Apply the SPC Matrix
                particle.velocity = mult(particle.spc_matrix, particle.velocity);

                particle.prev_position = particle.position;

                particle.position += particle.velocity * (float)dt;
            }
        }

        public Vector2 mult(Matrix2 m, Vector2 v)
        {
            return new Vector2(
                m.M11 * v.X + m.M12 * v.Y,
                m.M21 * v.X + m.M22 * v.Y
            );
        }

        void solve_distance_constraints(double dt)
        {
            
            double dt2 = dt * dt;


            foreach (xpbd_distance_constraint spring in springs.Values)
            {
                // Get the i, j of particle
                xpbd_particle p1 = particles[spring.i]; 
                xpbd_particle p2 = particles[spring.j];

                if (p1.inv_mass + p2.inv_mass == 0)
                    continue;

                // Find the length of the particles
                Vector2 d = p1.position - p2.position;
                double len = d.Length;

                if (len == 0) continue;

                Vector2 n = d / (float)len;

                double C = len - spring.rest_length;

                double alpha = spring.compliance / dt2;

                double w = p1.inv_mass + p2.inv_mass + alpha;

                double dlambda = (-C - alpha * spring.lambda) / w;

                spring.lambda += dlambda;

                Vector2 correction = (float)dlambda * n;

                // Apply the SPC 
                Vector2 corr1 = mult(p1.spc_matrix, (correction * (float)p1.inv_mass));
                Vector2 corr2 = mult(p2.spc_matrix, (-correction * (float)p2.inv_mass));

                p1.position += corr1;
                p2.position += corr2;
            }
        }


        void update_velocities(double dt)
        {

            float inv_dt = (float)(1.0 / dt);

            foreach (xpbd_particle particle in particles.Values)
            {
                if (particle.inv_mass == 0)
                {
                    particle.velocity = Vector2.Zero;
                    continue;
                }

                particle.velocity =
                    (particle.position - particle.prev_position) * inv_dt;

                // Apply SPC projection
                particle.velocity = mult( particle.spc_matrix, particle.velocity);
            }
            //
        }


        void solve_collisions()
        {
            // Floor is a y = a line passing through the point  (fixedend_floor_center)
            float floor_y = fixedend_floor_center.Y;


            foreach (xpbd_particle p in particles.Values)
            {
                // Find the y distance to the floor
                float y_distance = p.position.Y - floor_y;

                if (y_distance < particle_radius)
                {
                    float penetration = particle_radius - y_distance;

                    Vector2 normal = new Vector2(0.0f, 1.0f);

                    Vector2 correction = penetration * normal;

                    // Respect SPC constraints
                    correction = mult(p.spc_matrix, correction);

                    p.position += correction;
                }
            }

        }



    }
}
