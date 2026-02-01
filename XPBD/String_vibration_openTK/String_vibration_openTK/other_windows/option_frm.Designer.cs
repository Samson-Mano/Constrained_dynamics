namespace String_vibration_openTK.other_windows
{
    partial class option_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(option_frm));
            this.button_ok = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_show_time_label = new System.Windows.Forms.CheckBox();
            this.checkBox_show_acceleration_vector = new System.Windows.Forms.CheckBox();
            this.checkBox_show_velocity_vector = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(134, 483);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(109, 37);
            this.button_ok.TabIndex = 3;
            this.button_ok.Text = "Ok";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_show_time_label);
            this.groupBox1.Controls.Add(this.checkBox_show_acceleration_vector);
            this.groupBox1.Controls.Add(this.checkBox_show_velocity_vector);
            this.groupBox1.Location = new System.Drawing.Point(22, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(344, 465);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "View Options";
            // 
            // checkBox_show_time_label
            // 
            this.checkBox_show_time_label.AutoSize = true;
            this.checkBox_show_time_label.Location = new System.Drawing.Point(30, 103);
            this.checkBox_show_time_label.Name = "checkBox_show_time_label";
            this.checkBox_show_time_label.Size = new System.Drawing.Size(137, 20);
            this.checkBox_show_time_label.TabIndex = 5;
            this.checkBox_show_time_label.Text = "Show Time Label";
            this.checkBox_show_time_label.UseVisualStyleBackColor = true;
            // 
            // checkBox_show_acceleration_vector
            // 
            this.checkBox_show_acceleration_vector.AutoSize = true;
            this.checkBox_show_acceleration_vector.Location = new System.Drawing.Point(30, 60);
            this.checkBox_show_acceleration_vector.Name = "checkBox_show_acceleration_vector";
            this.checkBox_show_acceleration_vector.Size = new System.Drawing.Size(197, 20);
            this.checkBox_show_acceleration_vector.TabIndex = 1;
            this.checkBox_show_acceleration_vector.Text = "Show Acceleration Vector";
            this.checkBox_show_acceleration_vector.UseVisualStyleBackColor = true;
            // 
            // checkBox_show_velocity_vector
            // 
            this.checkBox_show_velocity_vector.AutoSize = true;
            this.checkBox_show_velocity_vector.Location = new System.Drawing.Point(30, 35);
            this.checkBox_show_velocity_vector.Name = "checkBox_show_velocity_vector";
            this.checkBox_show_velocity_vector.Size = new System.Drawing.Size(167, 20);
            this.checkBox_show_velocity_vector.TabIndex = 0;
            this.checkBox_show_velocity_vector.Text = "Show Velocity Vector";
            this.checkBox_show_velocity_vector.UseVisualStyleBackColor = true;
            // 
            // option_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 535);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "option_frm";
            this.Opacity = 0.85D;
            this.Text = "option_frm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_show_time_label;
        private System.Windows.Forms.CheckBox checkBox_show_acceleration_vector;
        private System.Windows.Forms.CheckBox checkBox_show_velocity_vector;
    }
}