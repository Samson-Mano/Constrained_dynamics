using billiard_collisions_simulation.src.fe_objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace billiard_collisions_simulation.other_windows
{
    public partial class setmodel_frm : Form
    {
        private fedata_store fe_data;


        public setmodel_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }


        public void initialize_model_form()
        {

        }



    }
}
