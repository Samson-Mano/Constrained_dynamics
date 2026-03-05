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

        List<coord_sys_data_store> coord_sys_data = new List<coord_sys_data_store>();

        List<grid_data_store> grid_data = new List<grid_data_store>();

        List<crod_data_store> crod_data = new List<crod_data_store>();

        List<spc_data_store> spc_data = new List<spc_data_store>();

        List<mass_data_store> mass_data = new List<mass_data_store>();

        List<property_data_store> prop_data = new List<property_data_store>();

        List<mat1_data_store> mat_data = new List<mat1_data_store>();


        public void simulate(Vec2Data gravity, double delta_t)
        {


        }

    }



    public class softbody_data_store
    {

        // Stores the entire softbody data
        softbody_data_container softbody_data = new softbody_data_container();


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



        }


        public void set_drawing_data()
        {


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








    }
    //
}
