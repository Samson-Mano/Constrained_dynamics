using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.system1_store_data
{
    public struct sdof1d_rigidcollisionResponse
    {
        public double time;
        public double displacement;
        public double velocity;
        public double acceleration;
        // public double contact_force;

    }

    public class sdof1d_rigidcollisionSolver
    {

        private double mass_m;
        private double stiffness_k;
        private double dampratio_zeta;
        private double const_accla0;

        private double damping_c;
        private double omega_n;
        private double omega_D;

        public List<sdof1d_rigidcollisionResponse> responseList = new List<sdof1d_rigidcollisionResponse>();
        private double total_time;

        public sdof1d_rigidcollisionSolver(double mass_m, double stiffness_k, double dampratio_zeta, double const_accla0)
        {
            this.mass_m = mass_m;
            this.stiffness_k = stiffness_k;
            this.dampratio_zeta = dampratio_zeta;
            this.const_accla0 = const_accla0;

            // Calculate the damping coefficient based on the mass, stiffness, and damping ratio
            this.damping_c = 2.0 * dampratio_zeta * Math.Sqrt(stiffness_k * mass_m);

            // Find the natural frequency of the system
            this.omega_n = Math.Sqrt(stiffness_k / mass_m);
            this.omega_D = omega_n * Math.Sqrt(1.0 - (dampratio_zeta * dampratio_zeta));

            // Response list initialization
            responseList.Clear();

        }

        private sdof1d_rigidcollisionResponse get_flight_solution(double time_t, double u_inl, double v_inl)
        {
            /// <summary>
            /// Computes the flight solution for a given time increment t.
            /// </summary>

            double a0 = this.const_accla0;

            sdof1d_rigidcollisionResponse response = new sdof1d_rigidcollisionResponse();

            response.time = -1.0; // Time is not defined in this context, so we set it to -1.0 to indicate that it's not applicable.
            response.acceleration = a0;
            response.velocity = v_inl + (a0 * time_t);
            response.displacement = u_inl + (v_inl * time_t) + (0.5 * a0 * time_t * time_t);

            return response;
        }


        private sdof1d_rigidcollisionResponse get_contact_solution(double time_t, double u_inl, double v_inl)
        {
            /// <summary>
            /// Computes the contact solution for a given time increment t.
            /// </summary>

            sdof1d_rigidcollisionResponse response = new sdof1d_rigidcollisionResponse();

            double exp_term = Math.Exp(-dampratio_zeta * omega_n * time_t);
            double cos_term = Math.Cos(omega_D * time_t);
            double sin_term = Math.Sin(omega_D * time_t);

            // Particular (Forced response) solution 
            double u_static = const_accla0 / (omega_n * omega_n);

            double A1 = -u_static;
            double A2 = -u_static * (dampratio_zeta / Math.Sqrt(1 - (dampratio_zeta * dampratio_zeta)));

            double u_particular = exp_term * ((A1 * cos_term) + (A2 * sin_term));
            double v_particular = exp_term * (const_accla0 / omega_D) * sin_term;

            double A3 = dampratio_zeta * (omega_n / omega_D);

            double a_particular = const_accla0 * exp_term * (cos_term - A3 * sin_term);


            // Homogeneous (Free response) complementary solution
            double C1 = u_inl;
            double C2 = v_inl + ((dampratio_zeta * omega_n * u_inl) / omega_D);

            double u_homogeneous = exp_term * (C1 * cos_term + C2 * sin_term);


            double C3 = ((u_inl * (omega_n * omega_n)) + (dampratio_zeta * omega_n * v_inl)) / omega_D;

            double v_homogeneous = -exp_term * (C3 * sin_term - v_inl * cos_term);

            double C4 = (2.0 * dampratio_zeta * omega_n * v_inl) + ((omega_n * omega_n) * u_inl);
            double C5_1 = (dampratio_zeta * omega_n * omega_n * u_inl);
            double C5_2 = ((2.0 * dampratio_zeta * dampratio_zeta) - 1.0) * omega_n * v_inl;
            double C5 = (C5_1 + C5_2) / Math.Sqrt(1.0 - (dampratio_zeta * dampratio_zeta));

            double a_homogeneous = -exp_term * (C4 * cos_term - C5 * sin_term);

            // Total response is the sum of the particular and homogeneous solutions
            response.time = -1.0; // Time is not defined in this context, so we set it to -1.0 to indicate that it's not applicable.
            response.displacement = u_particular + u_homogeneous;
            response.velocity = v_particular + v_homogeneous;
            response.acceleration = a_particular + a_homogeneous;
            return response;

        }


        private sdof1d_rigidcollisionResponse detect_contact_to_flight_response(double time_width, double u_inl, double v_inl)
        {
            // Combined bisection and Newton-Raphson method to find the time of transition from contact to flight
            // Start with bisection method to bracket the root

            double t_lower = 0.0;
            double t_upper = time_width;

            // Bisection method to find the time of transition from contact to flight
            for (int i = 0; i < 5; i++)
            {
                double t_mid = 0.5 * (t_lower + t_upper);
                sdof1d_rigidcollisionResponse response_at_mid = get_contact_solution(t_mid, u_inl, v_inl);
                sdof1d_rigidcollisionResponse response_at_lower = get_contact_solution(t_lower, u_inl, v_inl);

                double contact_force_mid = (stiffness_k * response_at_mid.displacement) + (damping_c * response_at_mid.velocity);
                double contact_force_lower = (stiffness_k * response_at_lower.displacement) + (damping_c * response_at_lower.velocity);

                if ((contact_force_mid * contact_force_lower) < 0.0)
                {
                    t_upper = t_mid;
                }
                else
                {
                    t_lower = t_mid;
                }
            }

            // Switch to Newton-Raphson method for faster convergence
           double t_val = 0.5 * (t_lower + t_upper);

            for (int i = 0; i < 20; i++)
            {
                sdof1d_rigidcollisionResponse response_at_tval = get_contact_solution(t_val, u_inl, v_inl);

                // Calculate the contact force and its derivative at the current time value
                double conact_force_tval = (stiffness_k * response_at_tval.displacement) + (damping_c * response_at_tval.velocity);
                double conact_force_derivative_tval = (stiffness_k * response_at_tval.velocity) + (damping_c * response_at_tval.acceleration);

                if(Math.Abs(conact_force_tval) < 1e-6)
                {
                    break;
                }

                // Update the time value using Newton-Raphson method
                t_val = t_val - (conact_force_tval / conact_force_derivative_tval);

                // Keep the time value within the bounds of the time width
                t_val = Math.Max(t_lower, Math.Min(t_upper, t_val));

            }

            sdof1d_rigidcollisionResponse response_at_collision;
            response_at_collision = get_contact_solution(t_val, u_inl, v_inl);

            response_at_collision.time = t_val;

            return response_at_collision;
        }


        private sdof1d_rigidcollisionResponse detect_flight_to_contact_response(double time_width, double u_inl, double v_inl)
        {
            // Combined bisection and Newton-Raphson method to find the time of transition from flight to contact
            // Start with bisection method to bracket the root

            double t_lower = 0.0;
            double t_upper = time_width;

            // Bisection method to find the time of transition from flight to contact
            for (int i = 0; i < 5; i++)
            {
                double t_mid = 0.5 * (t_lower + t_upper);
                sdof1d_rigidcollisionResponse response_at_mid = get_flight_solution(t_mid, u_inl, v_inl);
                sdof1d_rigidcollisionResponse response_at_lower = get_flight_solution(t_lower, u_inl, v_inl);

                double contact_force_mid = (stiffness_k * response_at_mid.displacement) + (damping_c * response_at_mid.velocity);
                double contact_force_lower = (stiffness_k * response_at_lower.displacement) + (damping_c * response_at_lower.velocity);

                if ((contact_force_mid * contact_force_lower) < 0.0)
                {
                    t_upper = t_mid;
                }
                else
                {
                    t_lower = t_mid;
                }
            }

            // Switch to Newton-Raphson method for faster convergence
            double t_val = 0.5 * (t_lower + t_upper);

            for (int i = 0; i < 20; i++)
            {
                sdof1d_rigidcollisionResponse response_at_tval = get_flight_solution(t_val, u_inl, v_inl);

                // Calculate the contact force and its derivative at the current time value
                double conact_force_tval = (stiffness_k * response_at_tval.displacement) + (damping_c * response_at_tval.velocity);
                double conact_force_derivative_tval = (stiffness_k * response_at_tval.velocity) + (damping_c * response_at_tval.acceleration);

                if (Math.Abs(conact_force_tval) < 1e-6)
                {
                    break;
                }

                // Update the time value using Newton-Raphson method
                t_val = t_val - (conact_force_tval / conact_force_derivative_tval);

                // Keep the time value within the bounds of the time width
                t_val = Math.Max(t_lower, Math.Min(t_upper, t_val));

            }

            sdof1d_rigidcollisionResponse response_at_collision;
            response_at_collision = get_flight_solution(t_val, u_inl, v_inl);

            response_at_collision.time = t_val;

            return response_at_collision;
        }



        public void solve_sdof1d_rigidcollision(double total_time, double max_time_increment, double initial_displacement, double initial_velocity)
        {
            /// <summary>
            /// Solves the SDOF system with rigid collision over the specified time range.
            /// </summary>
            // Clear the response list before starting the simulation
            responseList.Clear();
            sdof1d_rigidcollisionResponse response_at_t;

            // Store the total time
            this.total_time = total_time;


            double time_t = 0.0;
            double t_event = 0.0;
            double t_tau = 0.0;

            // Store the initial conditions at the event time
            double displ_at_event = initial_displacement;
            double velo_at_event = initial_velocity;

            response_at_t.time = time_t;
            response_at_t.displacement = initial_displacement;
            response_at_t.velocity = initial_velocity;
            response_at_t.acceleration = const_accla0;

            // Track the contact force
            double contact_force = (stiffness_k * initial_displacement) + (damping_c * initial_velocity);

            // Initialize the event tracker
            bool IsContact = false;
            
            if(contact_force < 0.0)
            {
                IsContact = true;
                response_at_t.acceleration = (contact_force / mass_m) + const_accla0;
            }

            // Add the initial response to the list
            responseList.Add(response_at_t);


            while (time_t < total_time)
            {
                // Time increment for the next iteration
                time_t += max_time_increment;

                if(time_t > total_time)
                {
                    time_t = total_time;
                }

                // Event span
                t_tau = time_t - t_event;

                if (!IsContact)
                {
                    // Flight phase
                    response_at_t = get_flight_solution(t_tau, displ_at_event, velo_at_event);

                }
                else
                {
                    // Contact phase
                    response_at_t = get_contact_solution(t_tau, displ_at_event, velo_at_event);
                }

                // store the time
                response_at_t.time = time_t;


                // Transition check: Determine if the system transitions from flight to contact or vice versa
                contact_force = (stiffness_k * response_at_t.displacement) + (damping_c * response_at_t.velocity);

                if(contact_force > 0.0 && IsContact == true)
                {
                    IsContact = false;

                    double displ_prev = responseList[responseList.Count - 1].displacement;
                    double vel_prev = responseList[responseList.Count - 1].velocity;


                    // Detect the exact transition time from contact to flight
                    sdof1d_rigidcollisionResponse response_at_event = 
                        detect_contact_to_flight_response(max_time_increment, displ_prev, vel_prev);

                    // Update the event time and store the response at the transition
                    time_t = (time_t - max_time_increment) + response_at_event.time;
                    t_event = time_t;

                    displ_at_event = response_at_event.displacement;
                    velo_at_event = response_at_event.velocity;

                    // Update the response at the transition time
                    response_at_t.time = time_t;
                    response_at_t.displacement = displ_at_event;
                    response_at_t.velocity = velo_at_event;
                    response_at_t.acceleration = response_at_event.acceleration;

                }
                else if(contact_force <= 0.0 && IsContact == false)
                {
                    IsContact = true;

                    double displ_prev = responseList[responseList.Count - 1].displacement;
                    double vel_prev = responseList[responseList.Count - 1].velocity;


                    // Detect the exact transition time from flight to contact
                    sdof1d_rigidcollisionResponse response_at_event =
                        detect_flight_to_contact_response(max_time_increment, displ_prev, vel_prev);

                    // Update the event time and store the response at the transition
                    time_t = (time_t - max_time_increment) + response_at_event.time;
                    t_event = time_t;

                    displ_at_event = response_at_event.displacement;
                    velo_at_event = response_at_event.velocity;

                    // Update the response at the transition time
                    response_at_t.time = time_t;
                    response_at_t.displacement = displ_at_event;
                    response_at_t.velocity = velo_at_event;
                    response_at_t.acceleration = response_at_event.acceleration;
                }


                // Add the computed response to the list
                responseList.Add(response_at_t);

            }

        }


        public sdof1d_rigidcollisionResponse getResult_at_timet(double time_t)
        {
            /// <summary>
            /// Retrieves the response at a specific time from the response list.
            /// </summary>

            if(time_t > total_time || time_t < 0.0)
            {
                // Reset the time to 0.0 if it exceeds the total simulation time
                time_t = 0.0;
            }


            // Find the two points to interpolate between
            int lowerIndex = 0;
            int upperIndex = responseList.Count - 1;
            int midIndex;

            // Binary search to find the interval containing time_t
            while (upperIndex - lowerIndex > 1)
            {
                midIndex = (lowerIndex + upperIndex) / 2;

                if (responseList[midIndex].time <= time_t)
                    lowerIndex = midIndex;
                else
                    upperIndex = midIndex;
            }

            // Get the two bounding points
            var lowerPoint = responseList[lowerIndex];
            var upperPoint = responseList[upperIndex];

            // Calculate interpolation factor (0.0 to 1.0)
            double dt = upperPoint.time - lowerPoint.time;

            // Handle case where time difference is zero (shouldn't happen with proper data)
            if (dt < 1e-12)
                return lowerPoint;


            // Clamp interpolation factor to [0,1] for safety
            double param_t = (time_t - lowerPoint.time) / dt;
            param_t = Math.Max(0.0, Math.Min(1.0, param_t));

            // Linear interpolation
            sdof1d_rigidcollisionResponse interpolated = new sdof1d_rigidcollisionResponse
            {
                time = time_t,
                displacement = lowerPoint.displacement + (upperPoint.displacement - lowerPoint.displacement) * param_t,
                velocity = lowerPoint.velocity + (upperPoint.velocity - lowerPoint.velocity) * param_t,
                acceleration = lowerPoint.acceleration + (upperPoint.acceleration - lowerPoint.acceleration) * param_t
            };

            return interpolated;
        }



    }
}
