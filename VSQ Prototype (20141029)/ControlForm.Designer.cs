namespace Prototype
{
    partial class ControlForm
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
            this.labelUserName = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.buttonDoExperiment = new System.Windows.Forms.Button();
            this.groupBoxButtonSize = new System.Windows.Forms.GroupBox();
            this.radioButtonButtonSize77 = new System.Windows.Forms.RadioButton();
            this.radioButtonButtonSize66 = new System.Windows.Forms.RadioButton();
            this.radioButtonButtonSize55 = new System.Windows.Forms.RadioButton();
            this.radioButtonButtonSize44 = new System.Windows.Forms.RadioButton();
            this.radioButtonButtonSize33 = new System.Windows.Forms.RadioButton();
            this.radioButtonButtonSize22 = new System.Windows.Forms.RadioButton();
            this.groupBoxCDGain = new System.Windows.Forms.GroupBox();
            this.radioButtonCDGain1x = new System.Windows.Forms.RadioButton();
            this.radioButtonCDGain2x = new System.Windows.Forms.RadioButton();
            this.radioButtonCDGain4x = new System.Windows.Forms.RadioButton();
            this.radioButtonCDGain3x = new System.Windows.Forms.RadioButton();
            this.buttonExitExperiment = new System.Windows.Forms.Button();
            this.groupBoxButtonSize.SuspendLayout();
            this.groupBoxCDGain.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Location = new System.Drawing.Point(19, 9);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(137, 12);
            this.labelUserName.TabIndex = 0;
            this.labelUserName.Text = "이름을 입력하여 주세요.";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(171, 6);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(258, 21);
            this.textBoxUserName.TabIndex = 1;
            // 
            // buttonDoExperiment
            // 
            this.buttonDoExperiment.Location = new System.Drawing.Point(17, 149);
            this.buttonDoExperiment.Name = "buttonDoExperiment";
            this.buttonDoExperiment.Size = new System.Drawing.Size(187, 24);
            this.buttonDoExperiment.TabIndex = 2;
            this.buttonDoExperiment.Text = "실험을 시작합니다.";
            this.buttonDoExperiment.UseVisualStyleBackColor = true;
            this.buttonDoExperiment.Click += new System.EventHandler(this.buttonDoExperiment_Click);
            // 
            // groupBoxButtonSize
            // 
            this.groupBoxButtonSize.Controls.Add(this.radioButtonButtonSize77);
            this.groupBoxButtonSize.Controls.Add(this.radioButtonButtonSize66);
            this.groupBoxButtonSize.Controls.Add(this.radioButtonButtonSize55);
            this.groupBoxButtonSize.Controls.Add(this.radioButtonButtonSize44);
            this.groupBoxButtonSize.Controls.Add(this.radioButtonButtonSize33);
            this.groupBoxButtonSize.Controls.Add(this.radioButtonButtonSize22);
            this.groupBoxButtonSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxButtonSize.Location = new System.Drawing.Point(14, 35);
            this.groupBoxButtonSize.Name = "groupBoxButtonSize";
            this.groupBoxButtonSize.Size = new System.Drawing.Size(415, 44);
            this.groupBoxButtonSize.TabIndex = 3;
            this.groupBoxButtonSize.TabStop = false;
            this.groupBoxButtonSize.Text = "Button Size";
            // 
            // radioButtonButtonSize77
            // 
            this.radioButtonButtonSize77.AutoSize = true;
            this.radioButtonButtonSize77.Location = new System.Drawing.Point(348, 21);
            this.radioButtonButtonSize77.Name = "radioButtonButtonSize77";
            this.radioButtonButtonSize77.Size = new System.Drawing.Size(64, 16);
            this.radioButtonButtonSize77.TabIndex = 0;
            this.radioButtonButtonSize77.TabStop = true;
            this.radioButtonButtonSize77.Text = "7x7mm";
            this.radioButtonButtonSize77.UseVisualStyleBackColor = true;
            // 
            // radioButtonButtonSize66
            // 
            this.radioButtonButtonSize66.AutoSize = true;
            this.radioButtonButtonSize66.Location = new System.Drawing.Point(279, 21);
            this.radioButtonButtonSize66.Name = "radioButtonButtonSize66";
            this.radioButtonButtonSize66.Size = new System.Drawing.Size(64, 16);
            this.radioButtonButtonSize66.TabIndex = 0;
            this.radioButtonButtonSize66.TabStop = true;
            this.radioButtonButtonSize66.Text = "6x6mm";
            this.radioButtonButtonSize66.UseVisualStyleBackColor = true;
            // 
            // radioButtonButtonSize55
            // 
            this.radioButtonButtonSize55.AutoSize = true;
            this.radioButtonButtonSize55.Location = new System.Drawing.Point(210, 21);
            this.radioButtonButtonSize55.Name = "radioButtonButtonSize55";
            this.radioButtonButtonSize55.Size = new System.Drawing.Size(64, 16);
            this.radioButtonButtonSize55.TabIndex = 0;
            this.radioButtonButtonSize55.TabStop = true;
            this.radioButtonButtonSize55.Text = "5x5mm";
            this.radioButtonButtonSize55.UseVisualStyleBackColor = true;
            // 
            // radioButtonButtonSize44
            // 
            this.radioButtonButtonSize44.AutoSize = true;
            this.radioButtonButtonSize44.Location = new System.Drawing.Point(141, 21);
            this.radioButtonButtonSize44.Name = "radioButtonButtonSize44";
            this.radioButtonButtonSize44.Size = new System.Drawing.Size(64, 16);
            this.radioButtonButtonSize44.TabIndex = 0;
            this.radioButtonButtonSize44.TabStop = true;
            this.radioButtonButtonSize44.Text = "4x4mm";
            this.radioButtonButtonSize44.UseVisualStyleBackColor = true;
            // 
            // radioButtonButtonSize33
            // 
            this.radioButtonButtonSize33.AutoSize = true;
            this.radioButtonButtonSize33.Location = new System.Drawing.Point(72, 21);
            this.radioButtonButtonSize33.Name = "radioButtonButtonSize33";
            this.radioButtonButtonSize33.Size = new System.Drawing.Size(64, 16);
            this.radioButtonButtonSize33.TabIndex = 0;
            this.radioButtonButtonSize33.TabStop = true;
            this.radioButtonButtonSize33.Text = "3x3mm";
            this.radioButtonButtonSize33.UseVisualStyleBackColor = true;
            // 
            // radioButtonButtonSize22
            // 
            this.radioButtonButtonSize22.AutoSize = true;
            this.radioButtonButtonSize22.Enabled = false;
            this.radioButtonButtonSize22.Location = new System.Drawing.Point(3, 21);
            this.radioButtonButtonSize22.Name = "radioButtonButtonSize22";
            this.radioButtonButtonSize22.Size = new System.Drawing.Size(64, 16);
            this.radioButtonButtonSize22.TabIndex = 0;
            this.radioButtonButtonSize22.Text = "2x2mm";
            this.radioButtonButtonSize22.UseVisualStyleBackColor = true;
            // 
            // groupBoxCDGain
            // 
            this.groupBoxCDGain.Controls.Add(this.radioButtonCDGain1x);
            this.groupBoxCDGain.Controls.Add(this.radioButtonCDGain2x);
            this.groupBoxCDGain.Controls.Add(this.radioButtonCDGain4x);
            this.groupBoxCDGain.Controls.Add(this.radioButtonCDGain3x);
            this.groupBoxCDGain.Location = new System.Drawing.Point(15, 89);
            this.groupBoxCDGain.Name = "groupBoxCDGain";
            this.groupBoxCDGain.Size = new System.Drawing.Size(415, 46);
            this.groupBoxCDGain.TabIndex = 3;
            this.groupBoxCDGain.TabStop = false;
            this.groupBoxCDGain.Text = "CD Gain";
            // 
            // radioButtonCDGain1x
            // 
            this.radioButtonCDGain1x.AutoSize = true;
            this.radioButtonCDGain1x.Location = new System.Drawing.Point(71, 20);
            this.radioButtonCDGain1x.Name = "radioButtonCDGain1x";
            this.radioButtonCDGain1x.Size = new System.Drawing.Size(36, 16);
            this.radioButtonCDGain1x.TabIndex = 0;
            this.radioButtonCDGain1x.TabStop = true;
            this.radioButtonCDGain1x.Text = "1x";
            this.radioButtonCDGain1x.UseVisualStyleBackColor = true;
            // 
            // radioButtonCDGain2x
            // 
            this.radioButtonCDGain2x.AutoSize = true;
            this.radioButtonCDGain2x.Location = new System.Drawing.Point(140, 20);
            this.radioButtonCDGain2x.Name = "radioButtonCDGain2x";
            this.radioButtonCDGain2x.Size = new System.Drawing.Size(36, 16);
            this.radioButtonCDGain2x.TabIndex = 0;
            this.radioButtonCDGain2x.TabStop = true;
            this.radioButtonCDGain2x.Text = "2x";
            this.radioButtonCDGain2x.UseVisualStyleBackColor = true;
            // 
            // radioButtonCDGain4x
            // 
            this.radioButtonCDGain4x.AutoSize = true;
            this.radioButtonCDGain4x.Location = new System.Drawing.Point(278, 20);
            this.radioButtonCDGain4x.Name = "radioButtonCDGain4x";
            this.radioButtonCDGain4x.Size = new System.Drawing.Size(36, 16);
            this.radioButtonCDGain4x.TabIndex = 0;
            this.radioButtonCDGain4x.TabStop = true;
            this.radioButtonCDGain4x.Text = "4x";
            this.radioButtonCDGain4x.UseVisualStyleBackColor = true;
            // 
            // radioButtonCDGain3x
            // 
            this.radioButtonCDGain3x.AutoSize = true;
            this.radioButtonCDGain3x.Location = new System.Drawing.Point(209, 20);
            this.radioButtonCDGain3x.Name = "radioButtonCDGain3x";
            this.radioButtonCDGain3x.Size = new System.Drawing.Size(36, 16);
            this.radioButtonCDGain3x.TabIndex = 0;
            this.radioButtonCDGain3x.TabStop = true;
            this.radioButtonCDGain3x.Text = "3x";
            this.radioButtonCDGain3x.UseVisualStyleBackColor = true;
            // 
            // buttonExitExperiment
            // 
            this.buttonExitExperiment.Location = new System.Drawing.Point(243, 149);
            this.buttonExitExperiment.Name = "buttonExitExperiment";
            this.buttonExitExperiment.Size = new System.Drawing.Size(187, 24);
            this.buttonExitExperiment.TabIndex = 2;
            this.buttonExitExperiment.Text = "실험을 종료합니다.";
            this.buttonExitExperiment.UseVisualStyleBackColor = true;
            this.buttonExitExperiment.Click += new System.EventHandler(this.buttonExitExperiment_Click);
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 185);
            this.Controls.Add(this.groupBoxCDGain);
            this.Controls.Add(this.groupBoxButtonSize);
            this.Controls.Add(this.buttonExitExperiment);
            this.Controls.Add(this.buttonDoExperiment);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.labelUserName);
            this.Name = "ControlForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ControlForm";
            this.Load += new System.EventHandler(this.ControlForm_Load);
            this.groupBoxButtonSize.ResumeLayout(false);
            this.groupBoxButtonSize.PerformLayout();
            this.groupBoxCDGain.ResumeLayout(false);
            this.groupBoxCDGain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Button buttonDoExperiment;
        private System.Windows.Forms.GroupBox groupBoxButtonSize;
        private System.Windows.Forms.GroupBox groupBoxCDGain;
        private System.Windows.Forms.RadioButton radioButtonButtonSize77;
        private System.Windows.Forms.RadioButton radioButtonButtonSize66;
        private System.Windows.Forms.RadioButton radioButtonButtonSize55;
        private System.Windows.Forms.RadioButton radioButtonButtonSize44;
        private System.Windows.Forms.RadioButton radioButtonButtonSize33;
        private System.Windows.Forms.RadioButton radioButtonButtonSize22;
        private System.Windows.Forms.RadioButton radioButtonCDGain1x;
        private System.Windows.Forms.RadioButton radioButtonCDGain2x;
        private System.Windows.Forms.RadioButton radioButtonCDGain4x;
        private System.Windows.Forms.RadioButton radioButtonCDGain3x;
        private System.Windows.Forms.Button buttonExitExperiment;

    }
}