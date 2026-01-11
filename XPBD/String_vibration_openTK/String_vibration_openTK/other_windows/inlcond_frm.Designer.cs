namespace String_vibration_openTK.other_windows
{
    partial class inlcond_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(inlcond_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_endnode = new System.Windows.Forms.Label();
            this.label_startnode = new System.Windows.Forms.Label();
            this.button_add = new System.Windows.Forms.Button();
            this.trackBar_endnode = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_interpolation = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_amplitude = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBar_startnode = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_inlcond_type = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView_inlcond = new System.Windows.Forms.DataGridView();
            this.button_delete = new System.Windows.Forms.Button();
            this.Column_inlcond_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_inlcond_type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_inlcond_interpolation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_nodestart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_endNode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_maxvalue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_endnode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_startnode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_inlcond)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_endnode);
            this.groupBox1.Controls.Add(this.label_startnode);
            this.groupBox1.Controls.Add(this.button_add);
            this.groupBox1.Controls.Add(this.trackBar_endnode);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboBox_interpolation);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_amplitude);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.trackBar_startnode);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBox_inlcond_type);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(531, 363);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Initial Condition Data: ";
            // 
            // label_endnode
            // 
            this.label_endnode.AutoSize = true;
            this.label_endnode.Location = new System.Drawing.Point(493, 233);
            this.label_endnode.Name = "label_endnode";
            this.label_endnode.Size = new System.Drawing.Size(17, 17);
            this.label_endnode.TabIndex = 20;
            this.label_endnode.Text = "0";
            // 
            // label_startnode
            // 
            this.label_startnode.AutoSize = true;
            this.label_startnode.Location = new System.Drawing.Point(493, 180);
            this.label_startnode.Name = "label_startnode";
            this.label_startnode.Size = new System.Drawing.Size(17, 17);
            this.label_startnode.TabIndex = 19;
            this.label_startnode.Text = "0";
            // 
            // button_add
            // 
            this.button_add.Location = new System.Drawing.Point(163, 293);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(182, 46);
            this.button_add.TabIndex = 10;
            this.button_add.Text = "Add Initial Condition";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.button_add_Click);
            // 
            // trackBar_endnode
            // 
            this.trackBar_endnode.Location = new System.Drawing.Point(108, 231);
            this.trackBar_endnode.Name = "trackBar_endnode";
            this.trackBar_endnode.Size = new System.Drawing.Size(379, 45);
            this.trackBar_endnode.TabIndex = 9;
            this.trackBar_endnode.Scroll += new System.EventHandler(this.trackBar_endnode_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 233);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "End Node: ";
            // 
            // comboBox_interpolation
            // 
            this.comboBox_interpolation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_interpolation.FormattingEnabled = true;
            this.comboBox_interpolation.Items.AddRange(new object[] {
            "Triangular Interpolation",
            "Cubic bezier Interpolation",
            "Sine Interpolation",
            "Rectangular Interpolation",
            "Single Node"});
            this.comboBox_interpolation.Location = new System.Drawing.Point(190, 127);
            this.comboBox_interpolation.Name = "comboBox_interpolation";
            this.comboBox_interpolation.Size = new System.Drawing.Size(231, 25);
            this.comboBox_interpolation.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(75, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Interpolation: ";
            // 
            // textBox_amplitude
            // 
            this.textBox_amplitude.Location = new System.Drawing.Point(190, 84);
            this.textBox_amplitude.Name = "textBox_amplitude";
            this.textBox_amplitude.Size = new System.Drawing.Size(100, 24);
            this.textBox_amplitude.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Amplitude: ";
            // 
            // trackBar_startnode
            // 
            this.trackBar_startnode.Location = new System.Drawing.Point(108, 180);
            this.trackBar_startnode.Name = "trackBar_startnode";
            this.trackBar_startnode.Size = new System.Drawing.Size(379, 45);
            this.trackBar_startnode.TabIndex = 3;
            this.trackBar_startnode.Scroll += new System.EventHandler(this.trackBar_startnode_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 180);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Start Node: ";
            // 
            // comboBox_inlcond_type
            // 
            this.comboBox_inlcond_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_inlcond_type.FormattingEnabled = true;
            this.comboBox_inlcond_type.Items.AddRange(new object[] {
            "Displacement",
            "Velocity"});
            this.comboBox_inlcond_type.Location = new System.Drawing.Point(190, 42);
            this.comboBox_inlcond_type.Name = "comboBox_inlcond_type";
            this.comboBox_inlcond_type.Size = new System.Drawing.Size(195, 25);
            this.comboBox_inlcond_type.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Initial condition type: ";
            // 
            // dataGridView_inlcond
            // 
            this.dataGridView_inlcond.AllowUserToAddRows = false;
            this.dataGridView_inlcond.AllowUserToDeleteRows = false;
            this.dataGridView_inlcond.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_inlcond.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_inlcond_id,
            this.Column_inlcond_type,
            this.Column_inlcond_interpolation,
            this.Column_nodestart,
            this.Column_endNode,
            this.Column_maxvalue});
            this.dataGridView_inlcond.Location = new System.Drawing.Point(549, 24);
            this.dataGridView_inlcond.Name = "dataGridView_inlcond";
            this.dataGridView_inlcond.ReadOnly = true;
            this.dataGridView_inlcond.RowHeadersWidth = 51;
            this.dataGridView_inlcond.Size = new System.Drawing.Size(530, 265);
            this.dataGridView_inlcond.TabIndex = 1;
            // 
            // button_delete
            // 
            this.button_delete.Location = new System.Drawing.Point(701, 305);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(162, 46);
            this.button_delete.TabIndex = 2;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // Column_inlcond_id
            // 
            this.Column_inlcond_id.FillWeight = 50F;
            this.Column_inlcond_id.HeaderText = "Id";
            this.Column_inlcond_id.Name = "Column_inlcond_id";
            this.Column_inlcond_id.ReadOnly = true;
            this.Column_inlcond_id.Width = 50;
            // 
            // Column_inlcond_type
            // 
            this.Column_inlcond_type.HeaderText = "Type";
            this.Column_inlcond_type.Name = "Column_inlcond_type";
            this.Column_inlcond_type.ReadOnly = true;
            // 
            // Column_inlcond_interpolation
            // 
            this.Column_inlcond_interpolation.HeaderText = "Interpolation";
            this.Column_inlcond_interpolation.Name = "Column_inlcond_interpolation";
            this.Column_inlcond_interpolation.ReadOnly = true;
            // 
            // Column_nodestart
            // 
            this.Column_nodestart.FillWeight = 50F;
            this.Column_nodestart.HeaderText = "Start Node";
            this.Column_nodestart.Name = "Column_nodestart";
            this.Column_nodestart.ReadOnly = true;
            this.Column_nodestart.Width = 50;
            // 
            // Column_endNode
            // 
            this.Column_endNode.FillWeight = 50F;
            this.Column_endNode.HeaderText = "End Node";
            this.Column_endNode.Name = "Column_endNode";
            this.Column_endNode.ReadOnly = true;
            this.Column_endNode.Width = 50;
            // 
            // Column_maxvalue
            // 
            this.Column_maxvalue.HeaderText = "Max Value";
            this.Column_maxvalue.Name = "Column_maxvalue";
            this.Column_maxvalue.ReadOnly = true;
            // 
            // inlcond_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1091, 396);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.dataGridView_inlcond);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1032, 435);
            this.Name = "inlcond_frm";
            this.Opacity = 0.85D;
            this.Text = "Initial Condition";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_endnode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_startnode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_inlcond)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox_inlcond_type;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBar_startnode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_interpolation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_amplitude;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.TrackBar trackBar_endnode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dataGridView_inlcond;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Label label_endnode;
        private System.Windows.Forms.Label label_startnode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_inlcond_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_inlcond_type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_inlcond_interpolation;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_nodestart;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_endNode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_maxvalue;
    }
}