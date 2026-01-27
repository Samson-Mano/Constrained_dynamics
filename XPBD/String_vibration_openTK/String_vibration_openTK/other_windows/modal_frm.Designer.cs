namespace String_vibration_openTK.other_windows
{
    partial class modal_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(modal_frm));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_modedata = new System.Windows.Forms.ComboBox();
            this.label_wavespeed = new System.Windows.Forms.Label();
            this.button_close = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_animation_speed = new System.Windows.Forms.Label();
            this.label_realtimeanim_speed = new System.Windows.Forms.Label();
            this.button_animation_speed = new System.Windows.Forms.Button();
            this.label_status = new System.Windows.Forms.Label();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_play_pause = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Natural Frequency: ";
            // 
            // comboBox_modedata
            // 
            this.comboBox_modedata.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_modedata.FormattingEnabled = true;
            this.comboBox_modedata.Items.AddRange(new object[] {
            "Linear Interpolation",
            "Cubic bezier Interpolation",
            "Sine Interpolation",
            "Rectangular Interpolation",
            "Single Node"});
            this.comboBox_modedata.Location = new System.Drawing.Point(188, 55);
            this.comboBox_modedata.Name = "comboBox_modedata";
            this.comboBox_modedata.Size = new System.Drawing.Size(379, 24);
            this.comboBox_modedata.TabIndex = 8;
            this.comboBox_modedata.SelectedIndexChanged += new System.EventHandler(this.comboBox_modedata_SelectedIndexChanged);
            // 
            // label_wavespeed
            // 
            this.label_wavespeed.AutoSize = true;
            this.label_wavespeed.Location = new System.Drawing.Point(43, 113);
            this.label_wavespeed.Name = "label_wavespeed";
            this.label_wavespeed.Size = new System.Drawing.Size(106, 16);
            this.label_wavespeed.TabIndex = 9;
            this.label_wavespeed.Text = "Wave speed = ";
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(236, 448);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(125, 46);
            this.button_close.TabIndex = 10;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_animation_speed);
            this.groupBox1.Controls.Add(this.label_realtimeanim_speed);
            this.groupBox1.Controls.Add(this.button_animation_speed);
            this.groupBox1.Location = new System.Drawing.Point(46, 286);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(521, 144);
            this.groupBox1.TabIndex = 11;
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
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(43, 249);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(53, 16);
            this.label_status.TabIndex = 14;
            this.label_status.Text = "Playing";
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(46, 202);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(133, 33);
            this.button_stop.TabIndex = 13;
            this.button_stop.Text = "Stop Animation";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_play_pause
            // 
            this.button_play_pause.Location = new System.Drawing.Point(46, 163);
            this.button_play_pause.Name = "button_play_pause";
            this.button_play_pause.Size = new System.Drawing.Size(133, 33);
            this.button_play_pause.TabIndex = 12;
            this.button_play_pause.Text = "Play Animation";
            this.button_play_pause.UseVisualStyleBackColor = true;
            this.button_play_pause.Click += new System.EventHandler(this.button_play_pause_Click);
            // 
            // modal_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 511);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_play_pause);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.label_wavespeed);
            this.Controls.Add(this.comboBox_modedata);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "modal_frm";
            this.Opacity = 0.85D;
            this.Text = "Modal Analysis Results";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.modal_frm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_modedata;
        private System.Windows.Forms.Label label_wavespeed;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_animation_speed;
        private System.Windows.Forms.Label label_realtimeanim_speed;
        private System.Windows.Forms.Button button_animation_speed;
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_play_pause;
    }
}