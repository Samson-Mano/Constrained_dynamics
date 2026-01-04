namespace PBD_pendulum_simulation.other_windows
{
    partial class animation_frm
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
            this.button_play_pause = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.label_status = new System.Windows.Forms.Label();
            this.checkBox_showtrail = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_play_pause
            // 
            this.button_play_pause.Location = new System.Drawing.Point(70, 51);
            this.button_play_pause.Name = "button_play_pause";
            this.button_play_pause.Size = new System.Drawing.Size(143, 44);
            this.button_play_pause.TabIndex = 0;
            this.button_play_pause.Text = "Play/ Pause Animation";
            this.button_play_pause.UseVisualStyleBackColor = true;
            this.button_play_pause.Click += new System.EventHandler(this.button_play_pause_Click);
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(70, 118);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(143, 44);
            this.button_stop.TabIndex = 1;
            this.button_stop.Text = "Stop Animation";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(67, 202);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(63, 20);
            this.label_status.TabIndex = 2;
            this.label_status.Text = "Playing";
            // 
            // checkBox_showtrail
            // 
            this.checkBox_showtrail.AutoSize = true;
            this.checkBox_showtrail.Location = new System.Drawing.Point(165, 200);
            this.checkBox_showtrail.Name = "checkBox_showtrail";
            this.checkBox_showtrail.Size = new System.Drawing.Size(108, 24);
            this.checkBox_showtrail.TabIndex = 3;
            this.checkBox_showtrail.Text = "Show Trail";
            this.checkBox_showtrail.UseVisualStyleBackColor = true;
            this.checkBox_showtrail.CheckedChanged += new System.EventHandler(this.checkBox_showtrail_CheckedChanged);
            // 
            // animation_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 262);
            this.Controls.Add(this.checkBox_showtrail);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_play_pause);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "animation_frm";
            this.Text = "Animation Control";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_play_pause;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.CheckBox checkBox_showtrail;
    }
}