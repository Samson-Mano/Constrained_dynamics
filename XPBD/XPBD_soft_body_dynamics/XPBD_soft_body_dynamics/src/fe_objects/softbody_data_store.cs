using XPBD_soft_body_dynamics.src.global_variables;
using XPBD_soft_body_dynamics.src.opentk_control.opentk_bgdraw;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XPBD_soft_body_dynamics.src.fe_objects
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


    public class coord_sys_data_store
    {
        // CORD2R data store
        // CORD2R, CID, RID, ORG_X, ORG_Y, , SPT_X, SPT_Y, , TPT_X, TPT_Y, 
        public int coord_id { get; set; }

        public Vec2Data origin_pt { get; set; }

        public Vec2Data haxis_pt { get; set; }

        public Vec2Data vaxis_pt { get; set; }

    }


    public class grid_data_store
    {
        // GRID data store
        // GRID, ID, CID, X1, X2

        public int grid_id { get; set; }   

        public int coord_id { get; set; }   

        public Vec2Data coord_pt { get; set; }

    }


    public class crod_data_store
    {
        // CROD data store
        // CROD, EID, PID, GRID1, GRID2
        public int element_id { get; set; }

        public int property_id { get; set; }    

        public int start_grid_id { get; set; }

        public int end_grid_id { get; set; }

    }


    public class spc_data_store
    {
        // SPC1 data store
        // SPC1, SID, C, GRID
        public int spc_id { get; set; }

        public int spc_type { get; set; }

        public int grid_id { get; set; }

    }


    public class mass_data_store
    {
        // MASS data store
        // CONM2, EID, GRID, CID, MASS VALUE
        public int mass_id { get; set; }

        public int grid_id { get; set; }

        public int coord_id { get; set; }

        public double mass_value { get; set; }  

    }


    public class property_data_store
    {
        // PROD data store
        // PROD, PID, MID, AREA, , , NSM
        // NMS nonstructural mass per unit length

        public int prop_id { get; set; }

        public int mat_id { get; set; }

        public double cs_area { get; set; }

        public double non_structural_mass { get; set; }

    }


    public class mat1_data_store
    {
        // MAT1 data store
        // MAT1, MID, E, G, NU, RHO
        public int mat_id { get; set; }

        public double youngs_mod { get; set; }

        public double shear_mod { get; set; }

        public double poissons_ratio { get; set; }

        public double mat_density_rho { get; set; } 

    }


    public class softbody_data_container
    {

        public Dictionary<int, Vec2Data> grid_vertexMap { get; set; } = new Dictionary<int, Vec2Data>();

        public Dictionary<int, int> grid_coordMap { get; set; } = new Dictionary<int, int>();

        public List<coord_sys_data_store> coord_sys_data { get; set; } = new List<coord_sys_data_store>();

        public List<grid_data_store> grid_data { get; set; } = new List<grid_data_store>();

        public List<crod_data_store> crod_data { get; set; } = new List<crod_data_store>();

        public List<spc_data_store> spc_data { get; set; } = new List<spc_data_store>();

        public List<mass_data_store> mass_data { get; set; } = new List<mass_data_store>();

        public List<property_data_store> prop_data { get; set; } = new List<property_data_store>();

        public List<mat1_data_store> mat_data { get; set; } = new List<mat1_data_store>();

        public double max_mass { get; set; } = 0.0;


        public void simulate(Vec2Data gravity, double delta_t)
        {


        }

    }



    public class softbody_data_store
    {
        // Stores the entire softbody data
        public softbody_data_container softbody_data = new softbody_data_container();

        // Drawing Elements
        node_list_store node_list = new node_list_store(); // Node store
        elementlink_list_store elementlink_list = new elementlink_list_store(); // Element link store

        mass_list_store mass_list = new mass_list_store(); // Mass store


        bool isModelSet = false;
    
        const string FILE_NAME = "softbodydata.json";


        // Gravity
        Vec2Data GRAVITY = new Vec2Data(0.0f, 0.0f);


        // Simulation constants
        const double FIXED_DT = 1.0 / 120.0;
        const int CONSTRAINT_ITERATIONS = 8;
        double prev_time = 0.0;
        double accumulator = 0.0;



        public softbody_data_store()
        {
            // Constructor (Read the JSON and setup default)

            LoadFromJsonOrCreateDefault();

        }



        void LoadFromJsonOrCreateDefault()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                softbody_data = JsonConvert.DeserializeObject<softbody_data_container>(json);

                //____________________________________________________________________________________________________________
                // Set the drawing data
                set_drawing_data();
            }
            else
            {
                update_softbody_data(default_model_data());
                // SaveToJson();
            }
        }

        public void SaveToJson()
        {
            string json = JsonConvert.SerializeObject(softbody_data, Formatting.Indented);
            File.WriteAllText(FILE_NAME, json);

        }


        public void update_softbody_data(string model_data)
        {
            isModelSet = false;

            // Initialize the soft body data
            softbody_data = new softbody_data_container();

            var lines = model_data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw_line in lines)
            {
                string line = raw_line.Trim();

                // Ignore comments
                if (line.StartsWith("$"))
                    continue;

                var tokens = line.Split(',')
                                 .Select(t => t.Trim())
                                 .ToArray();

                if (tokens.Length == 0)
                    continue;

                string card = tokens[0];

                switch (card)
                {
                    case "CORD2R":
                        parse_CORD2R(tokens);
                        break;

                    case "GRID":
                        parse_GRID(tokens);
                        break;

                    case "CROD":
                        parse_CROD(tokens);
                        break;

                    case "SPC1":
                        parse_SPC1(tokens);
                        break;

                    case "CONM2":
                        parse_CONM2(tokens);
                        break;

                    case "PROD":
                        parse_PROD(tokens);
                        break;

                    case "MAT1":
                    case "MAT2":
                        parse_MAT(tokens);
                        break;
                }
            }

            // Set the drawing data
            set_drawing_data();

            isModelSet = true;

            //
        }

        #region "Parse model string data"
        void parse_CORD2R(string[] t)
        {
            // Parse the CORD2R data
            coord_sys_data_store coord = new coord_sys_data_store();

            coord.coord_id = int.Parse(t[1]); // Coord ID

            coord.origin_pt = new Vec2Data(
                float.Parse(t[3]), // Origin pt x
                float.Parse(t[4]) // Origin pt y
            );

            coord.haxis_pt = new Vec2Data(
                float.Parse(t[6]), // Horizontal pt x
                float.Parse(t[7]) // Horizontal pt y
            );

            coord.vaxis_pt = new Vec2Data(
                float.Parse(t[9]), // Vertical pt x
                float.Parse(t[10]) // Vertical pt y
            );

            softbody_data.coord_sys_data.Add(coord);
        }

        void parse_GRID(string[] t)
        {
            // Parse GRID data
            grid_data_store grid = new grid_data_store();

            grid.grid_id = int.Parse(t[1]); // GRID Id
            grid.coord_id = int.Parse(t[2]); // Coord Id

            grid.coord_pt = new Vec2Data(
                float.Parse(t[3]), // x pt
                float.Parse(t[4]) // y pt
            );

            // Add to the map
            softbody_data.grid_vertexMap.Add(grid.grid_id, grid.coord_pt);
            softbody_data.grid_coordMap.Add(grid.grid_id, grid.coord_id);


            softbody_data.grid_data.Add(grid);
        }


        void parse_CROD(string[] t)
        {
            // Parse CROD data
            crod_data_store rod = new crod_data_store();

            rod.element_id = int.Parse(t[1]); // Element ID
            rod.property_id = int.Parse(t[2]); // Property ID
            rod.start_grid_id = int.Parse(t[3]); // Start GRID Id
            rod.end_grid_id = int.Parse(t[4]); // End GRID Id

            softbody_data.crod_data.Add(rod);
        }


        void parse_SPC1(string[] t)
        {
            // Parse SPC1 data
            spc_data_store spc = new spc_data_store();

            spc.spc_id = int.Parse(t[1]); // SPC Id
            spc.spc_type = int.Parse(t[2]); // SPC type
            spc.grid_id = int.Parse(t[3]); // GRID Id

            softbody_data.spc_data.Add(spc);
        }


        void parse_CONM2(string[] t)
        {
            // Parse MASS data
            mass_data_store mass = new mass_data_store();

            mass.mass_id = int.Parse(t[1]); // Mass Id
            mass.grid_id = int.Parse(t[2]); // Grid Id
            mass.coord_id = int.Parse(t[3]); // Coord Id
            mass.mass_value = double.Parse(t[4]); // Mass value

            // Set the max mass value
            softbody_data.max_mass = Math.Max(softbody_data.max_mass, Math.Abs(mass.mass_value));

            softbody_data.mass_data.Add(mass);
        }


        void parse_PROD(string[] t)
        {
            // Parse PRDO data
            property_data_store prop = new property_data_store();

            prop.prop_id = int.Parse(t[1]); // PROD Id
            prop.mat_id = int.Parse(t[2]); // MAT Id
            prop.cs_area = double.Parse(t[3]); // CS Area

            if (t.Length > 6 && !string.IsNullOrWhiteSpace(t[6]))
                prop.non_structural_mass = double.Parse(t[6]); // Non Structural Mass

            softbody_data.prop_data.Add(prop);
        }


        void parse_MAT(string[] t)
        {
            // Parse MAT data
            mat1_data_store mat = new mat1_data_store();

            mat.mat_id = int.Parse(t[1]); // MAT Id
            mat.youngs_mod = double.Parse(t[2]); // Youngs Modulus
            mat.shear_mod = double.Parse(t[3]); // Shear Modulus
            mat.poissons_ratio = double.Parse(t[4]); // Poissons Ratio
            mat.mat_density_rho = double.Parse(t[5]); // Material density

            softbody_data.mat_data.Add(mat);
        }



        // Default model data
        private string default_model_data()
        {
            return @"
$ CORD2R, CID, RID, ORG_X, ORG_Y, , SPT_X, SPT_Y, , TPT_X, TPT_Y, 
$ Origin pt, second pt, third pt
CORD2R, 0, , 0.0, 0.0, , 1.0, 0.0, , 0.0, 1.0, 
CORD2R, 1, , 0.0, 0.0, , 1.0, 0.0, , 0.0, 1.0, 
$ GRID, ID, CID, X1, X2
GRID, 0, 0, -5400, -1559
GRID, 1, 0, -1800, -1559
GRID, 2, 0, 1800, -1559
GRID, 3, 0, 5400, -1559
GRID, 5, 0, -3600, 1559
GRID, 6, 0, -0.000207583, 1559
GRID, 7, 0, 3600, 1559
$ CROD, EID, PID, GRID1, GRID2
CROD, 0, 0, 0, 1
CROD, 1, 0, 1, 2
CROD, 2, 0, 2, 3
CROD, 5, 0, 0, 5
CROD, 6, 0, 5, 1
CROD, 7, 0, 1, 6
CROD, 8, 0, 6, 2
CROD, 9, 0, 2, 7
CROD, 10, 0 , 7, 3
CROD, 11, 0 , 5, 6
CROD, 12, 0, 6, 7
$ SPC1, SID, C, GRID
$ C = 123456
SPC1, 0, 12, 0
SPC1, 1, 1, 3
$ CONM2, EID, GRID, CID, MASS VALUE
CONM2, 0, 1, 0, 100.0
CONM2, 1, 2, 0, 90.0
$ PROD, PID, MID, AREA, , , NSM
$ NMS nonstructural mass per unit length
PROD, 0, 1, 10.2, , , 1.4
$ MAT1, MID, E, G, NU, RHO
MAT1, 0, 207000, 80000, 0.3, 7.83e-09
MAT2, 1, 69000, 25000, 0.33, 2.73e-09
                    ";

        }


        #endregion


        public void set_drawing_data()
        {
            // Create the drawing data

            // Create the node drawing data
            node_list = new node_list_store();

            foreach (grid_data_store grid in softbody_data.grid_data)
            {
                // Get the grid node point
                Vec2Data gridpt = grid.coord_pt;

                node_list.add_node(grid.grid_id, gridpt.GetVector());  
            }


            // Create the element link drawing data
            elementlink_list = new elementlink_list_store();

            foreach( crod_data_store crod in softbody_data.crod_data)
            {
                // Get the start pt and end pt
                Vec2Data startpt = softbody_data.grid_vertexMap[crod.start_grid_id];
                Vec2Data endpt = softbody_data.grid_vertexMap[crod.end_grid_id];

                elementlink_list.add_elementlink(crod.element_id, startpt.GetVector(), endpt.GetVector(), crod.property_id);
            }

            // Create the mass drawing data
            mass_list = new mass_list_store();

            foreach (mass_data_store mass in softbody_data.mass_data)
            {

                // Get the mass location
                Vec2Data mass_loc = softbody_data.grid_vertexMap[mass.grid_id];

                float mass_size = (float)(Math.Abs(mass.mass_value) / softbody_data.max_mass);

                mass_list.add_mass(mass.mass_id, mass_loc.GetVector(), mass_size, -3);
            }


            //_______________________________________________________________________________________
            // Find the boundary
            List<Vector3> nodePtsList = new List<Vector3>();

            // Get the model boundary
            foreach (Vec2Data vec2d in softbody_data.grid_vertexMap.Values)
            {
                Vector2 vec = vec2d.GetVector();

                nodePtsList.Add(new Vector3(vec.X, vec.Y, 0.0f));

            }


            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            Vector3  min_bounds = geom_extremes.Item1; // Minimum bound
            Vector3  max_bounds = geom_extremes.Item2; // Maximum bound

            Vector3  geom_bounds = max_bounds - min_bounds;

            float geom_size = geom_bounds.Length;
            //_______________________________________________________________________________________




            // Finalize the visualization
            node_list.set_node_visualization(geom_size);
            elementlink_list.set_elementlink_visualization(geom_size);
            mass_list.set_mass_visualization(geom_size);    

        }


        public void paint_drawing_data()
        {
            if (!isModelSet) return;


            elementlink_list.paint_elementlink();

            node_list.paint_node();

            mass_list.paint_pointmass();    
        }




        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
                          drawing_events graphic_events_control)
        {

            if (!isModelSet) return;

            //// Update mass openTK uniforms
            //mass_List.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
            //    graphic_events_control.projectionMatrix,
            //    graphic_events_control.modelMatrix,
            //    graphic_events_control.viewMatrix,
            //    gvariables_static.geom_transparency);

            elementlink_list.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

            node_list.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            mass_list.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);

        }






    }
    //
}
