namespace billiard_collisions_simulation.other_windows
{
    partial class setmodel_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(setmodel_frm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_no_of_balls = new System.Windows.Forms.TextBox();
            this.textBox_min_radius = new System.Windows.Forms.TextBox();
            this.textBox_max_radius = new System.Windows.Forms.TextBox();
            this.button_update_model = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(231, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Number of billiard balls : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(77, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(181, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Min., radius value : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(73, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(185, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Max., radius value : ";
            // 
            // textBox_no_of_balls
            // 
            this.textBox_no_of_balls.Location = new System.Drawing.Point(221, 33);
            this.textBox_no_of_balls.Name = "textBox_no_of_balls";
            this.textBox_no_of_balls.Size = new System.Drawing.Size(100, 27);
            this.textBox_no_of_balls.TabIndex = 3;
            // 
            // textBox_min_radius
            // 
            this.textBox_min_radius.Location = new System.Drawing.Point(221, 72);
            this.textBox_min_radius.Name = "textBox_min_radius";
            this.textBox_min_radius.Size = new System.Drawing.Size(100, 27);
            this.textBox_min_radius.TabIndex = 4;
            // 
            // textBox_max_radius
            // 
            this.textBox_max_radius.Location = new System.Drawing.Point(221, 115);
            this.textBox_max_radius.Name = "textBox_max_radius";
            this.textBox_max_radius.Size = new System.Drawing.Size(100, 27);
            this.textBox_max_radius.TabIndex = 5;
            // 
            // button_update_model
            // 
            this.button_update_model.Location = new System.Drawing.Point(130, 203);
            this.button_update_model.Name = "button_update_model";
            this.button_update_model.Size = new System.Drawing.Size(119, 42);
            this.button_update_model.TabIndex = 6;
            this.button_update_model.Text = "Update Model";
            this.button_update_model.UseVisualStyleBackColor = true;
            this.button_update_model.Click += new System.EventHandler(this.button_update_model_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(45, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(220, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Screen size: 1000 x 800";
            // 
            // setmodel_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 267);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_update_model);
            this.Controls.Add(this.textBox_max_radius);
            this.Controls.Add(this.textBox_min_radius);
            this.Controls.Add(this.textBox_no_of_balls);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "setmodel_frm";
            this.Opacity = 0.85D;
            this.Text = "Update Model";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_no_of_balls;
        private System.Windows.Forms.TextBox textBox_min_radius;
        private System.Windows.Forms.TextBox textBox_max_radius;
        private System.Windows.Forms.Button button_update_model;
        private System.Windows.Forms.Label label4;
    }
}