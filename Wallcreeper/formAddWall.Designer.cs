namespace Wallcreeper
{
    partial class formAddWall
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
            this.components = new System.ComponentModel.Container();
            this.picThumbnail = new System.Windows.Forms.PictureBox();
            this.lblFilename = new System.Windows.Forms.Label();
            this.listThemes = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textFilter = new System.Windows.Forms.TextBox();
            this.buttSave = new System.Windows.Forms.Button();
            this.picFrame = new System.Windows.Forms.PictureBox();
            this.picTooltip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFrame)).BeginInit();
            this.SuspendLayout();
            // 
            // picThumbnail
            // 
            this.picThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picThumbnail.Location = new System.Drawing.Point(12, 59);
            this.picThumbnail.Name = "picThumbnail";
            this.picThumbnail.Size = new System.Drawing.Size(205, 128);
            this.picThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picThumbnail.TabIndex = 0;
            this.picThumbnail.TabStop = false;
            this.picThumbnail.Click += new System.EventHandler(this.picThumbnail_Click);
            this.picThumbnail.MouseEnter += new System.EventHandler(this.picThumbnail_MouseEnter);
            this.picThumbnail.MouseLeave += new System.EventHandler(this.picThumbnail_MouseLeave);
            // 
            // lblFilename
            // 
            this.lblFilename.AutoEllipsis = true;
            this.lblFilename.Location = new System.Drawing.Point(12, 9);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(205, 43);
            this.lblFilename.TabIndex = 1;
            this.lblFilename.Text = "lblFilename";
            this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listThemes
            // 
            this.listThemes.FormattingEnabled = true;
            this.listThemes.Location = new System.Drawing.Point(15, 219);
            this.listThemes.Name = "listThemes";
            this.listThemes.Size = new System.Drawing.Size(202, 147);
            this.listThemes.TabIndex = 2;
            this.listThemes.SelectedIndexChanged += new System.EventHandler(this.listThemes_SelectedIndexChanged);
            this.listThemes.DoubleClick += new System.EventHandler(this.listThemes_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 196);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter themes:";
            // 
            // textFilter
            // 
            this.textFilter.Location = new System.Drawing.Point(84, 193);
            this.textFilter.Name = "textFilter";
            this.textFilter.Size = new System.Drawing.Size(133, 20);
            this.textFilter.TabIndex = 4;
            this.textFilter.TextChanged += new System.EventHandler(this.textFilter_TextChanged);
            this.textFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textFilter_KeyDown);
            // 
            // buttSave
            // 
            this.buttSave.Enabled = false;
            this.buttSave.Location = new System.Drawing.Point(15, 372);
            this.buttSave.Name = "buttSave";
            this.buttSave.Size = new System.Drawing.Size(202, 23);
            this.buttSave.TabIndex = 5;
            this.buttSave.Text = "Save";
            this.buttSave.UseVisualStyleBackColor = true;
            this.buttSave.Click += new System.EventHandler(this.buttSave_Click);
            // 
            // picFrame
            // 
            this.picFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.picFrame.Location = new System.Drawing.Point(8, 55);
            this.picFrame.Name = "picFrame";
            this.picFrame.Size = new System.Drawing.Size(213, 136);
            this.picFrame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picFrame.TabIndex = 6;
            this.picFrame.TabStop = false;
            this.picFrame.Visible = false;
            // 
            // picTooltip
            // 
            this.picTooltip.ShowAlways = true;
            // 
            // formAddWall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(233, 406);
            this.Controls.Add(this.buttSave);
            this.Controls.Add(this.textFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listThemes);
            this.Controls.Add(this.lblFilename);
            this.Controls.Add(this.picThumbnail);
            this.Controls.Add(this.picFrame);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "formAddWall";
            this.Text = "Add wallpaper";
            this.Load += new System.EventHandler(this.formAddWall_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFrame)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picThumbnail;
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.ListBox listThemes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textFilter;
        private System.Windows.Forms.Button buttSave;
        private System.Windows.Forms.PictureBox picFrame;
        private System.Windows.Forms.ToolTip picTooltip;
    }
}