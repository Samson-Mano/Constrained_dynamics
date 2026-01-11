namespace String_vibration_openTK.other_windows
{
    partial class load_frm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(load_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox_pulse_option = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_endtime = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_starttime = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_add = new System.Windows.Forms.Button();
            this.trackBar_endnode = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_inerpolation = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_amplitude = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBar_startnode = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.button_delete = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.label_startnode = new System.Windows.Forms.Label();
            this.label_endnode = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_endnode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_startnode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_endnode);
            this.groupBox1.Controls.Add(this.label_startnode);
            this.groupBox1.Controls.Add(this.comboBox_pulse_option);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.textBox_endtime);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_starttime);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button_add);
            this.groupBox1.Controls.Add(this.trackBar_endnode);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboBox_inerpolation);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_amplitude);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.trackBar_startnode);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 17);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(531, 386);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Loads Data: ";
            // 
            // comboBox_pulse_option
            // 
            this.comboBox_pulse_option.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_pulse_option.FormattingEnabled = true;
            this.comboBox_pulse_option.Items.AddRange(new object[] {
            "Half Sine Pulse",
            "Rectangular Pulse",
            "Triangular Pulse",
            "Step Force with Finite Rise",
            "Harmonic Excitation"});
            this.comboBox_pulse_option.Location = new System.Drawing.Point(191, 156);
            this.comboBox_pulse_option.Name = "comboBox_pulse_option";
            this.comboBox_pulse_option.Size = new System.Drawing.Size(231, 24);
            this.comboBox_pulse_option.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(85, 159);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 16);
            this.label7.TabIndex = 15;
            this.label7.Text = "Pulse Option: ";
            // 
            // textBox_endtime
            // 
            this.textBox_endtime.Location = new System.Drawing.Point(191, 80);
            this.textBox_endtime.Name = "textBox_endtime";
            this.textBox_endtime.Size = new System.Drawing.Size(100, 23);
            this.textBox_endtime.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(109, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 16);
            this.label6.TabIndex = 13;
            this.label6.Text = "End Time: ";
            // 
            // textBox_starttime
            // 
            this.textBox_starttime.Location = new System.Drawing.Point(191, 51);
            this.textBox_starttime.Name = "textBox_starttime";
            this.textBox_starttime.Size = new System.Drawing.Size(100, 23);
            this.textBox_starttime.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(99, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "Start Time: ";
            // 
            // button_add
            // 
            this.button_add.Location = new System.Drawing.Point(162, 311);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(182, 46);
            this.button_add.TabIndex = 10;
            this.button_add.Text = "Add Load";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.button_add_Click);
            // 
            // trackBar_endnode
            // 
            this.trackBar_endnode.Location = new System.Drawing.Point(102, 260);
            this.trackBar_endnode.Name = "trackBar_endnode";
            this.trackBar_endnode.Size = new System.Drawing.Size(379, 45);
            this.trackBar_endnode.TabIndex = 9;
            this.trackBar_endnode.Scroll += new System.EventHandler(this.trackBar_endnode_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 262);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "End Node: ";
            // 
            // comboBox_inerpolation
            // 
            this.comboBox_inerpolation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_inerpolation.FormattingEnabled = true;
            this.comboBox_inerpolation.Items.AddRange(new object[] {
            "Linear Interpolation",
            "Cubic bezier Interpolation",
            "Sine Interpolation",
            "Rectangular Interpolation",
            "Single Node"});
            this.comboBox_inerpolation.Location = new System.Drawing.Point(193, 117);
            this.comboBox_inerpolation.Name = "comboBox_inerpolation";
            this.comboBox_inerpolation.Size = new System.Drawing.Size(231, 24);
            this.comboBox_inerpolation.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(85, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Interpolation: ";
            // 
            // textBox_amplitude
            // 
            this.textBox_amplitude.Location = new System.Drawing.Point(191, 22);
            this.textBox_amplitude.Name = "textBox_amplitude";
            this.textBox_amplitude.Size = new System.Drawing.Size(100, 23);
            this.textBox_amplitude.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(105, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Amplitude: ";
            // 
            // trackBar_startnode
            // 
            this.trackBar_startnode.Location = new System.Drawing.Point(102, 209);
            this.trackBar_startnode.Name = "trackBar_startnode";
            this.trackBar_startnode.Size = new System.Drawing.Size(379, 45);
            this.trackBar_startnode.TabIndex = 3;
            this.trackBar_startnode.Scroll += new System.EventHandler(this.trackBar_startnode_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Start Node: ";
            // 
            // button_delete
            // 
            this.button_delete.Location = new System.Drawing.Point(703, 328);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(162, 46);
            this.button_delete.TabIndex = 5;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(549, 29);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(455, 265);
            this.dataGridView1.TabIndex = 4;
            // 
            // label_startnode
            // 
            this.label_startnode.AutoSize = true;
            this.label_startnode.Location = new System.Drawing.Point(487, 209);
            this.label_startnode.Name = "label_startnode";
            this.label_startnode.Size = new System.Drawing.Size(15, 16);
            this.label_startnode.TabIndex = 17;
            this.label_startnode.Text = "0";
            // 
            // label_endnode
            // 
            this.label_endnode.AutoSize = true;
            this.label_endnode.Location = new System.Drawing.Point(486, 262);
            this.label_endnode.Name = "label_endnode";
            this.label_endnode.Size = new System.Drawing.Size(15, 16);
            this.label_endnode.TabIndex = 18;
            this.label_endnode.Text = "0";
            // 
            // load_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 415);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.dataGridView1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1032, 454);
            this.Name = "load_frm";
            this.Opacity = 0.85D;
            this.Text = "Loads";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_endnode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_startnode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_endtime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_starttime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.TrackBar trackBar_endnode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_inerpolation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_amplitude;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBar_startnode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox comboBox_pulse_option;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_endnode;
        private System.Windows.Forms.Label label_startnode;
    }
}