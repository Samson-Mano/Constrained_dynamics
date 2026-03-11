using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XPBD_soft_body_dynamics.src.fe_objects
{
    public class xpbd_particle
    {
        public Vec2Data position;
        public Vec2Data prev_position;
        public Vec2Data velocity;

        public double mass;
        public double inv_mass;

    }

    public class xpbd_distance_constraint
    {
        int i; // particle A
        int j; // particle B
        double rest_length;


        double compliance; // 0 = rigid
        double lambda; // Accumulated Lagrane multiplier

    }


    public class xpbd_solver
    {
        public Dictionary<int, xpbd_particle> particles = new Dictionary<int, xpbd_particle>();

        public Dictionary<int, xpbd_distance_constraint> springs = new Dictionary<int, xpbd_distance_constraint>();


        public xpbd_solver() { }    


        public xpbd_solver(softbody_data_container softbody_data)
        {
            // Initialize the particles
            particles = new Dictionary<int, xpbd_particle>();

            foreach (grid_data_store grid in softbody_data.grid_data)
            {
                // Create the particles
                xpbd_particle new_particle = new xpbd_particle();
                new_particle.position = grid.coord_pt;
                new_particle.prev_position = grid.coord_pt;
                new_particle.velocity = new Vec2Data(0, 0);

                new_particle.mass = 0.0; // Set mass as zero first
                new_particle.inv_mass = 0.0;     // Set inv mass as zero first

                particles.Add(grid.grid_id, new_particle);
            }

            // Apply point mass to the data
            foreach (mass_data_store mass in softbody_data.mass_data)
            {

                // Get the mass location
                particles[mass.grid_id].mass += mass.mass_value;

            }

            // Apply mass of Attached CROD to the node
            foreach (crod_data_store crod in softbody_data.crod_data)
            {
                // Get the PROD Id
               int prop_id = crod.property_id;

                // Get the cross section area of the ROD element
                double crod_cs_area = softbody_data.prop_data[prop_id].cs_area;

                // Get the density of the material
                int mat_id = softbody_data.prop_data[prop_id].mat_id;

                double mat_density = softbody_data.mat_data[mat_id].mat_density_rho;

                // Get the length of the element
                Vec2Data start_pt = softbody_data.grid_data[crod.start_grid_id].coord_pt;
                Vec2Data end_pt = softbody_data.grid_data[crod.end_grid_id].coord_pt;

                double crod_length = Vector2.Distance(start_pt.GetVector(), end_pt.GetVector());    

                // Total mass of the rod
                double crod_mass = crod_cs_area * crod_length * mat_density;

                // Apply the mass to the nodes attached to the crod
                particles[crod.start_grid_id].mass += crod_mass * 0.5;
                particles[crod.end_grid_id].mass += crod_mass * 0.5;

            }






            // Apply mass to the particles





        }



        public void simulate(Vec2Data gravity, double dt)
        {
            integrate_particles(gravity, dt);

            int solver_iterations = 10;

            for (int i = 0; i < solver_iterations; i++)
            {
                solve_distance_constraints(dt);
                solve_boundary_constraints();
                solve_collisions();
            }

            update_velocities(dt);
        }


        void integrate_particles(Vec2Data gravity, double dt)
        {
            foreach (var g in grid_data)
            {
                if (g.inv_mass == 0) continue;

                g.velocity += gravity * dt;
                g.prev_position = g.position;
                g.position += g.velocity * dt;
            }
        }

        void solve_distance_constraints(double dt)
        {
            foreach (var rod in crod_data)
            {
                var p1 = grid_data[rod.node1];
                var p2 = grid_data[rod.node2];

                Vec2Data d = p1.position - p2.position;
                double len = d.Length();

                if (len == 0) continue;

                Vec2Data n = d / len;

                double C = len - rod.rest_length;

                double alpha = rod.compliance / (dt * dt);

                double w = p1.inv_mass + p2.inv_mass + alpha;

                double dlambda = (-C - alpha * rod.lambda) / w;

                rod.lambda += dlambda;

                Vec2Data correction = dlambda * n;

                p1.position += correction * p1.inv_mass;
                p2.position -= correction * p2.inv_mass;
            }
        }


        void update_velocities(double dt)
        {
            foreach (var p in grid_data)
            {
                p.velocity = (p.position - p.prev_position) / dt;
            }
        }






    }
}
