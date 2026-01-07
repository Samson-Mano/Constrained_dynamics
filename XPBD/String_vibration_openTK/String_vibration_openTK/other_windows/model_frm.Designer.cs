namespace String_vibration_openTK.other_windows
{
    partial class model_frm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button_update_model = new System.Windows.Forms.Button();
            this.textBox_no_of_nodes = new System.Windows.Forms.TextBox();
            this.textBox_tension = new System.Windows.Forms.TextBox();
            this.textBox_length = new System.Windows.Forms.TextBox();
            this.textBox_density = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_density);
            this.groupBox1.Controls.Add(this.textBox_length);
            this.groupBox1.Controls.Add(this.textBox_tension);
            this.groupBox1.Controls.Add(this.textBox_no_of_nodes);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(362, 175);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "String in Tension (Fixed both ends)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(54, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Number of nodes: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(115, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Tension: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(120, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Length: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(120, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Density: ";
            // 
            // button_update_model
            // 
            this.button_update_model.Location = new System.Drawing.Point(130, 206);
            this.button_update_model.Name = "button_update_model";
            this.button_update_model.Size = new System.Drawing.Size(127, 37);
            this.button_update_model.TabIndex = 1;
            this.button_update_model.Text = "Update Model";
            this.button_update_model.UseVisualStyleBackColor = true;
            // 
            // textBox_no_of_nodes
            // 
            this.textBox_no_of_nodes.Location = new System.Drawing.Point(189, 39);
            this.textBox_no_of_nodes.Name = "textBox_no_of_nodes";
            this.textBox_no_of_nodes.Size = new System.Drawing.Size(100, 23);
            this.textBox_no_of_nodes.TabIndex = 4;
            // 
            // textBox_tension
            // 
            this.textBox_tension.Location = new System.Drawing.Point(189, 68);
            this.textBox_tension.Name = "textBox_tension";
            this.textBox_tension.Size = new System.Drawing.Size(100, 23);
            this.textBox_tension.TabIndex = 5;
            // 
            // textBox_length
            // 
            this.textBox_length.Location = new System.Drawing.Point(189, 97);
            this.textBox_length.Name = "textBox_length";
            this.textBox_length.Size = new System.Drawing.Size(100, 23);
            this.textBox_length.TabIndex = 6;
            // 
            // textBox_density
            // 
            this.textBox_density.Location = new System.Drawing.Point(189, 126);
            this.textBox_density.Name = "textBox_density";
            this.textBox_density.Size = new System.Drawing.Size(100, 23);
            this.textBox_density.TabIndex = 7;
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(130, 249);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(127, 37);
            this.button_cancel.TabIndex = 2;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // model_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 311);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_update_model);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(409, 350);
            this.Name = "model_frm";
            this.Opacity = 0.85D;
            this.Text = "Update Model";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_density;
        private System.Windows.Forms.TextBox textBox_length;
        private System.Windows.Forms.TextBox textBox_tension;
        private System.Windows.Forms.TextBox textBox_no_of_nodes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_update_model;
        private System.Windows.Forms.Button button_cancel;
    }
}