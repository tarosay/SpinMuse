
namespace SpinMuse
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.pbxEdit = new System.Windows.Forms.PictureBox();
            this.btnCreateGifWithRedLineAxis = new System.Windows.Forms.Button();
            this.pbxAnimation = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxAnimation)).BeginInit();
            this.SuspendLayout();
            // 
            // pbxEdit
            // 
            this.pbxEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxEdit.Location = new System.Drawing.Point(0, 23);
            this.pbxEdit.Name = "pbxEdit";
            this.pbxEdit.Size = new System.Drawing.Size(236, 260);
            this.pbxEdit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxEdit.TabIndex = 0;
            this.pbxEdit.TabStop = false;
            this.pbxEdit.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxEdit_Paint);
            this.pbxEdit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbxEdit_MouseUp);
            // 
            // btnCreateGifWithRedLineAxis
            // 
            this.btnCreateGifWithRedLineAxis.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCreateGifWithRedLineAxis.Enabled = false;
            this.btnCreateGifWithRedLineAxis.Location = new System.Drawing.Point(0, 0);
            this.btnCreateGifWithRedLineAxis.Name = "btnCreateGifWithRedLineAxis";
            this.btnCreateGifWithRedLineAxis.Size = new System.Drawing.Size(504, 23);
            this.btnCreateGifWithRedLineAxis.TabIndex = 1;
            this.btnCreateGifWithRedLineAxis.Text = "Create Animated GIF with Red Line Rotation";
            this.btnCreateGifWithRedLineAxis.UseVisualStyleBackColor = true;
            this.btnCreateGifWithRedLineAxis.Click += new System.EventHandler(this.btnCreateGifWithRedLineAxis_Click);
            // 
            // pbxAnimation
            // 
            this.pbxAnimation.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbxAnimation.Location = new System.Drawing.Point(236, 23);
            this.pbxAnimation.Name = "pbxAnimation";
            this.pbxAnimation.Size = new System.Drawing.Size(268, 260);
            this.pbxAnimation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxAnimation.TabIndex = 2;
            this.pbxAnimation.TabStop = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 283);
            this.Controls.Add(this.pbxEdit);
            this.Controls.Add(this.pbxAnimation);
            this.Controls.Add(this.btnCreateGifWithRedLineAxis);
            this.Name = "MainForm";
            this.Text = "SpinMuse";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.pbxEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxAnimation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxEdit;
        private System.Windows.Forms.Button btnCreateGifWithRedLineAxis;
        private System.Windows.Forms.PictureBox pbxAnimation;
    }
}

