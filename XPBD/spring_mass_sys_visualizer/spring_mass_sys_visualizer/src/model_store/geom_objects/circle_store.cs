using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spring_mass_sys_visualizer.src.model_store.geom_objects
{
    public class circle_data
    {
        public int circle_id;
        public float radius;
        public float x_position;
        public float y_position;
        public bool isFilled = false; // Default to false, can be set later

        const int circle_vertex_count = 36; // Number of vertices to approximate the circle

        public circle_data(int circle_id, float radius, float x_position, float y_position, bool isFilled)
        {
            this.circle_id = circle_id;
            this.radius = radius;
            this.x_position = x_position;
            this.y_position = y_position;
            this.isFilled = isFilled;
        }


        public List<float> circle_vertex_data()
        {
            List<float> vertexData = new List<float>();

            for (int i = 0; i < circle_vertex_count; i++)
            {
                float angle = 2 * (float)Math.PI * i / circle_vertex_count;
                float x = x_position + radius * (float)Math.Cos(angle);
                float y = y_position + radius * (float)Math.Sin(angle);
                vertexData.Add(x);
                vertexData.Add(y);
            }

            return vertexData;
        }


    }




    public class circle_store
    {



    }
}
