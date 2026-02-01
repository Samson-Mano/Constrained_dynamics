namespace String_vibration_openTK.other_windows
{
    partial class resp_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(resp_frm));
            this.label_status = new System.Windows.Forms.Label();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_play_pause = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_animation_speed = new System.Windows.Forms.Label();
            this.label_realtimeanim_speed = new System.Windows.Forms.Label();
            this.button_animation_speed = new System.Windows.Forms.Button();
            this.label_acceleration_scale = new System.Windows.Forms.Label();
            this.trackBar_acceleration_scale = new System.Windows.Forms.TrackBar();
            this.label_velocity_scale = new System.Windows.Forms.Label();
            this.trackBar_velocity_scale = new System.Windows.Forms.TrackBar();
            this.label_deformation_scale = new System.Windows.Forms.Label();
            this.trackBar_deformation_scale = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_close = new System.Windows.Forms.Button();
            this.checkBox_rollform = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_acceleration_scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_velocity_scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_deformation_scale)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(9, 343);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(53, 16);
            this.label_status.TabIndex = 22;
            this.label_status.Text = "Playing";
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(12, 296);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(133, 33);
            this.button_stop.TabIndex = 21;
            this.button_stop.Text = "Stop Animation";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_play_pause
            // 
            this.button_play_pause.Location = new System.Drawing.Point(12, 257);
            this.button_play_pause.Name = "button_play_pause";
            this.button_play_pause.Size = new System.Drawing.Size(133, 33);
            this.button_play_pause.TabIndex = 20;
            this.button_play_pause.Text = "Play Animation";
            this.button_play_pause.UseVisualStyleBackColor = true;
            this.button_play_pause.Click += new System.EventHandler(this.button_play_pause_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_animation_speed);
            this.groupBox1.Controls.Add(this.label_realtimeanim_speed);
            this.groupBox1.Controls.Add(this.button_animation_speed);
            this.groupBox1.Location = new System.Drawing.Point(12, 380);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(485, 144);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation Speed: ";
            // 
            // label_animation_speed
            // 
            this.label_animation_speed.AutoSize = true;
            this.label_animation_speed.Location = new System.Drawing.Point(216, 30);
            this.label_animation_speed.Name = "label_animation_speed";
            this.label_animation_speed.Size = new System.Drawing.Size(28, 16);
            this.label_animation_speed.TabIndex = 2;
            this.label_animation_speed.Text = "1.0";
            // 
            // label_realtimeanim_speed
            // 
            this.label_realtimeanim_speed.AutoSize = true;
            this.label_realtimeanim_speed.Location = new System.Drawing.Point(6, 73);
            this.label_realtimeanim_speed.Name = "label_realtimeanim_speed";
            this.label_realtimeanim_speed.Size = new System.Drawing.Size(309, 16);
            this.label_realtimeanim_speed.TabIndex = 1;
            this.label_realtimeanim_speed.Text = "1 second in real time = 1 second in Animation";
            // 
            // button_animation_speed
            // 
            this.button_animation_speed.Location = new System.Drawing.Point(9, 22);
            this.button_animation_speed.Name = "button_animation_speed";
            this.button_animation_speed.Size = new System.Drawing.Size(153, 32);
            this.button_animation_speed.TabIndex = 0;
            this.button_animation_speed.Text = "Animation Speed";
            this.button_animation_speed.UseVisualStyleBackColor = true;
            this.button_animation_speed.Click += new System.EventHandler(this.button_animation_speed_Click);
            // 
            // label_acceleration_scale
            // 
            this.label_acceleration_scale.AutoSize = true;
            this.label_acceleration_scale.Location = new System.Drawing.Point(271, 135);
            this.label_acceleration_scale.Name = "label_acceleration_scale";
            this.label_acceleration_scale.Size = new System.Drawing.Size(168, 16);
            this.label_acceleration_scale.TabIndex = 5;
            this.label_acceleration_scale.Text = "Acceleration scale = 1.0";
            // 
            // trackBar_acceleration_scale
            // 
            this.trackBar_acceleration_scale.Location = new System.Drawing.Point(9, 135);
            this.trackBar_acceleration_scale.Name = "trackBar_acceleration_scale";
            this.trackBar_acceleration_scale.Size = new System.Drawing.Size(256, 45);
            this.trackBar_acceleration_scale.TabIndex = 4;
            // 
            // label_velocity_scale
            // 
            this.label_velocity_scale.AutoSize = true;
            this.label_velocity_scale.Location = new System.Drawing.Point(271, 84);
            this.label_velocity_scale.Name = "label_velocity_scale";
            this.label_velocity_scale.Size = new System.Drawing.Size(138, 16);
            this.label_velocity_scale.TabIndex = 3;
            this.label_velocity_scale.Text = "Velocity scale = 1.0";
            // 
            // trackBar_velocity_scale
            // 
            this.trackBar_velocity_scale.Location = new System.Drawing.Point(9, 84);
            this.trackBar_velocity_scale.Name = "trackBar_velocity_scale";
            this.trackBar_velocity_scale.Size = new System.Drawing.Size(256, 45);
            this.trackBar_velocity_scale.TabIndex = 2;
            // 
            // label_deformation_scale
            // 
            this.label_deformation_scale.AutoSize = true;
            this.label_deformation_scale.Location = new System.Drawing.Point(271, 33);
            this.label_deformation_scale.Name = "label_deformation_scale";
            this.label_deformation_scale.Size = new System.Drawing.Size(165, 16);
            this.label_deformation_scale.TabIndex = 1;
            this.label_deformation_scale.Text = "Deformation scale = 1.0";
            // 
            // trackBar_deformation_scale
            // 
            this.trackBar_deformation_scale.Location = new System.Drawing.Point(9, 33);
            this.trackBar_deformation_scale.Name = "trackBar_deformation_scale";
            this.trackBar_deformation_scale.Size = new System.Drawing.Size(256, 45);
            this.trackBar_deformation_scale.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label_acceleration_scale);
            this.groupBox2.Controls.Add(this.trackBar_acceleration_scale);
            this.groupBox2.Controls.Add(this.label_velocity_scale);
            this.groupBox2.Controls.Add(this.trackBar_velocity_scale);
            this.groupBox2.Controls.Add(this.label_deformation_scale);
            this.groupBox2.Controls.Add(this.trackBar_deformation_scale);
            this.groupBox2.Location = new System.Drawing.Point(12, 44);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(485, 202);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Animation Scale: ";
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(182, 553);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(125, 46);
            this.button_close.TabIndex = 24;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // checkBox_rollform
            // 
            this.checkBox_rollform.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox_rollform.AutoSize = true;
            this.checkBox_rollform.Location = new System.Drawing.Point(471, 12);
            this.checkBox_rollform.Name = "checkBox_rollform";
            this.checkBox_rollform.Size = new System.Drawing.Size(25, 26);
            this.checkBox_rollform.TabIndex = 25;
            this.checkBox_rollform.Text = "▲";
            this.checkBox_rollform.UseVisualStyleBackColor = true;
            this.checkBox_rollform.CheckedChanged += new System.EventHandler(this.checkBox_rollform_CheckedChanged);
            // 
            // resp_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 611);
            this.Controls.Add(this.checkBox_rollform);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_play_pause);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximumSize = new System.Drawing.Size(525, 650);
            this.MinimumSize = new System.Drawing.Size(525, 85);
            this.Name = "resp_frm";
            this.Opacity = 0.85D;
            this.Text = "Response Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.resp_frm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_acceleration_scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_velocity_scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_deformation_scale)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_play_pause;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_animation_speed;
        private System.Windows.Forms.Label label_realtimeanim_speed;
        private System.Windows.Forms.Button button_animation_speed;
        private System.Windows.Forms.Label label_acceleration_scale;
        private System.Windows.Forms.TrackBar trackBar_acceleration_scale;
        private System.Windows.Forms.Label label_velocity_scale;
        private System.Windows.Forms.TrackBar trackBar_velocity_scale;
        private System.Windows.Forms.Label label_deformation_scale;
        private System.Windows.Forms.TrackBar trackBar_deformation_scale;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.CheckBox checkBox_rollform;
    }
}