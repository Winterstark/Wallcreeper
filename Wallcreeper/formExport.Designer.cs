namespace Wallcreeper
{
    partial class formExport
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
            this.label1 = new System.Windows.Forms.Label();
            this.chklistThemes = new System.Windows.Forms.CheckedListBox();
            this.buttSelAll = new System.Windows.Forms.Button();
            this.buttSelNone = new System.Windows.Forms.Button();
            this.buttSelInvert = new System.Windows.Forms.Button();
            this.lblNumber = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.saveDiag = new System.Windows.Forms.SaveFileDialog();
            this.openDiag = new System.Windows.Forms.OpenFileDialog();
            this.textArchiver = new System.Windows.Forms.TextBox();
            this.buttBrowse = new System.Windows.Forms.Button();
            this.buttExport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Export the following wallpaper themes:";
            // 
            // chklistThemes
            // 
            this.chklistThemes.FormattingEnabled = true;
            this.chklistThemes.Location = new System.Drawing.Point(33, 25);
            this.chklistThemes.Name = "chklistThemes";
            this.chklistThemes.Size = new System.Drawing.Size(239, 229);
            this.chklistThemes.TabIndex = 1;
            this.chklistThemes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklistThemes_ItemCheck);
            // 
            // buttSelAll
            // 
            this.buttSelAll.Location = new System.Drawing.Point(33, 260);
            this.buttSelAll.Name = "buttSelAll";
            this.buttSelAll.Size = new System.Drawing.Size(75, 23);
            this.buttSelAll.TabIndex = 2;
            this.buttSelAll.Text = "Select all";
            this.buttSelAll.UseVisualStyleBackColor = true;
            this.buttSelAll.Click += new System.EventHandler(this.buttSelAll_Click);
            // 
            // buttSelNone
            // 
            this.buttSelNone.Location = new System.Drawing.Point(197, 260);
            this.buttSelNone.Name = "buttSelNone";
            this.buttSelNone.Size = new System.Drawing.Size(75, 23);
            this.buttSelNone.TabIndex = 3;
            this.buttSelNone.Text = "Select none";
            this.buttSelNone.UseVisualStyleBackColor = true;
            this.buttSelNone.Click += new System.EventHandler(this.buttSelNone_Click);
            // 
            // buttSelInvert
            // 
            this.buttSelInvert.Location = new System.Drawing.Point(115, 260);
            this.buttSelInvert.Name = "buttSelInvert";
            this.buttSelInvert.Size = new System.Drawing.Size(75, 23);
            this.buttSelInvert.TabIndex = 4;
            this.buttSelInvert.Text = "Invert Sel.";
            this.buttSelInvert.UseVisualStyleBackColor = true;
            this.buttSelInvert.Click += new System.EventHandler(this.buttSelInvert_Click);
            // 
            // lblNumber
            // 
            this.lblNumber.AutoSize = true;
            this.lblNumber.Location = new System.Drawing.Point(294, 25);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(199, 13);
            this.lblNumber.TabIndex = 5;
            this.lblNumber.Text = "Wallpaper pack will contain 0 wallpapers";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(294, 51);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(89, 13);
            this.lblSize.TabIndex = 6;
            this.lblSize.Text = "Total size = 0 MB";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(294, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "File archiver (e.g. 7-Zip) path:";
            // 
            // openDiag
            // 
            this.openDiag.FileName = "7zG.exe";
            this.openDiag.Filter = "Applications|*.exe";
            // 
            // textArchiver
            // 
            this.textArchiver.Location = new System.Drawing.Point(297, 138);
            this.textArchiver.Name = "textArchiver";
            this.textArchiver.Size = new System.Drawing.Size(196, 20);
            this.textArchiver.TabIndex = 8;
            // 
            // buttBrowse
            // 
            this.buttBrowse.Location = new System.Drawing.Point(499, 136);
            this.buttBrowse.Name = "buttBrowse";
            this.buttBrowse.Size = new System.Drawing.Size(39, 23);
            this.buttBrowse.TabIndex = 9;
            this.buttBrowse.Text = "...";
            this.buttBrowse.UseVisualStyleBackColor = true;
            this.buttBrowse.Click += new System.EventHandler(this.buttBrowse_Click);
            // 
            // buttExport
            // 
            this.buttExport.Location = new System.Drawing.Point(297, 217);
            this.buttExport.Name = "buttExport";
            this.buttExport.Size = new System.Drawing.Size(196, 37);
            this.buttExport.TabIndex = 10;
            this.buttExport.Text = "Export";
            this.buttExport.UseVisualStyleBackColor = true;
            this.buttExport.Click += new System.EventHandler(this.buttExport_Click);
            // 
            // formExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 301);
            this.Controls.Add(this.buttExport);
            this.Controls.Add(this.buttBrowse);
            this.Controls.Add(this.textArchiver);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.lblNumber);
            this.Controls.Add(this.buttSelInvert);
            this.Controls.Add(this.buttSelNone);
            this.Controls.Add(this.buttSelAll);
            this.Controls.Add(this.chklistThemes);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "formExport";
            this.Text = "Export Wallpaper Pack";
            this.Load += new System.EventHandler(this.formExport_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chklistThemes;
        private System.Windows.Forms.Button buttSelAll;
        private System.Windows.Forms.Button buttSelNone;
        private System.Windows.Forms.Button buttSelInvert;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SaveFileDialog saveDiag;
        private System.Windows.Forms.OpenFileDialog openDiag;
        private System.Windows.Forms.TextBox textArchiver;
        private System.Windows.Forms.Button buttBrowse;
        private System.Windows.Forms.Button buttExport;
    }
}