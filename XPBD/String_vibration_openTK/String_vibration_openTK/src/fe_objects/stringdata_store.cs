using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTK.Graphics.ES11;

namespace String_vibration_openTK.src.fe_objects
{

    public static class AppPaths
    {
        public static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "StringVibrationOpenTK");

        public static readonly string StringDataFile =
            Path.Combine(AppFolder, "stringdata.json");
    }

    public class initialconditiondata_store
    {
        public int inlcond_id { get; set; }
        public int inlcond_type { get; set; } // 0 = Displacement, 1 = Velocity

        public List<int> inlcond_nodes { get; set; } = new List<int>(); // Initial condition nodes
        public List<double> inlcond_values { get; set; } = new List<double>(); // Initial condition values

    }


    public class loaddata_store
    {
        public int load_id { get; set; }
        public int load_type { get; set; } // 0 = half sine pulse, 1 = Rectangular Pulse, 
                                                    // 2 = Triangular Pulse, 3 = Step Force with Finite Rise
                                                    // 4 = Harmonic Excitation

        public double load_start_time { get; set; } // Load start time
        public double load_end_time { get; set; } // Load end time

        public List<int> load_nodes { get; set; } = new List<int>(); // Load nodes
        public List<double> load_values { get; set; } = new List<double>(); // Load values

    }


    public class stringdata_store
    {
        public int model_version { get; set; } = 1;

        public int no_of_nodes { get; set; }
        public double string_tension { get; set; }
        public double string_length { get; set; }
        public double string_density { get; set; }

        public List<initialconditiondata_store> inlcond_data { get; set; } = new List<initialconditiondata_store>();
        public List<loaddata_store> load_data { get; set; } = new List<loaddata_store>();

        // -------------------------
        // UPDATE MODEL
        // -------------------------
        public void update_model(int no_of_nodes, double string_tension, double string_length, double string_density)
        {
            this.no_of_nodes = no_of_nodes;
            this.string_tension = string_tension;
            this.string_length = string_length;
            this.string_density = string_density;

            // Clear the load and initial condition data
            inlcond_data.Clear();
            load_data.Clear();

        }


        // -------------------------
        // ADD/ DELETE INITIAL CONDITION
        // -------------------------
        public void add_initial_condition(int inlcond_id, int inlcond_type, List<int> inlcond_nodes, List<double> inlcond_values)
        {
            inlcond_data.Add(new initialconditiondata_store()
            {
                inlcond_id = inlcond_id,
                inlcond_type = inlcond_type,
                inlcond_nodes = new List<int>(inlcond_nodes),
                inlcond_values = new List<double>(inlcond_values)
            });
        }


        public void delete_initial_condition(int inlcond_id)
        {

            inlcond_data.Remove( inlcond_data.Find(e=>e.inlcond_id == inlcond_id) );

        }



        // -------------------------
        // ADD/ DELETE LOAD
        // -------------------------
        public void add_load(int load_id, int load_type, double load_start_time,
            double load_end_time, List<int> load_nodes, List<double> load_values)
        {
            load_data.Add(new loaddata_store()
            {
                load_id = load_id,
                load_type = load_type,
                load_start_time = load_start_time,
                load_end_time = load_end_time,
                load_nodes = new List<int>(load_nodes),
                load_values = new List<double>(load_values)
            });
        }


        public void delete_load(int load_id)
        {

            load_data.Remove(load_data.Find(e => e.load_id == load_id));

        }


        // -------------------------
        // DEFAULT INITIALIZATION
        // -------------------------
        public static stringdata_store CreateDefault()
        {
            return new stringdata_store
            {
                no_of_nodes = 50,
                string_tension = 100.0,
                string_length = 10.0,
                string_density = 0.01
            };
        }

        // -------------------------
        // LOAD FROM FILE
        // -------------------------
        public static stringdata_store Load()
        {
            try
            {
                if (!Directory.Exists(AppPaths.AppFolder))
                    Directory.CreateDirectory(AppPaths.AppFolder);

                if (!File.Exists(AppPaths.StringDataFile))
                {
                    var def = CreateDefault();
                    def.Save();
                    return def;
                }

                string json = File.ReadAllText(AppPaths.StringDataFile);
                return JsonConvert.DeserializeObject<stringdata_store>(json)
                       ?? CreateDefault();
            }
            catch
            {
                // Failsafe
                return CreateDefault();
            }
        }

        // -------------------------
        // SAVE TO FILE
        // -------------------------
        public void Save()
        {
            if (!Directory.Exists(AppPaths.AppFolder))
                Directory.CreateDirectory(AppPaths.AppFolder);

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(AppPaths.StringDataFile, json);
        }

    }


}
