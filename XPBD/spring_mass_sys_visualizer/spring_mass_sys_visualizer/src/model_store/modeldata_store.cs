using OpenTK;
using spring_mass_sys_visualizer.src.events_handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store
{
    public class modeldata_store
    {

        // Drawing bound data
        public Vector3 min_bounds = new Vector3(-1);
        public Vector3 max_bounds = new Vector3(1);
        public Vector3 geom_bounds = new Vector3(2);


        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }


        public modeldata_store()
        {
            // To control the drawing graphics events
            graphic_events_control = new drawing_events(this);

        }


        public void InitializeModelGeom()
        {

        }

        public void PaintModel()
        {

        }



        public void update_openTK_uniforms()
        {


        }
    }
}
