/*
 * MIT License
 * 
 * Copyright (c) 2024 plexdata.de
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Plexdata.XPixMapper.GUI.Dialogs
{
    partial class AboutDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
            this.picLogo = new PictureBox();
            this.lblProduct = new Label();
            this.lblVersion = new Label();
            this.lblCopyright = new Label();
            this.txtDescription = new TextBox();
            this.btnClose = new Button();
            this.ttpHelper = new ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)this.picLogo).BeginInit();
            this.SuspendLayout();
            // 
            // picLogo
            // 
            this.picLogo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.picLogo.Cursor = Cursors.Hand;
            this.picLogo.Image = (Image)resources.GetObject("picLogo.Image");
            this.picLogo.Location = new Point(208, 14);
            this.picLogo.Margin = new Padding(4, 3, 4, 3);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new Size(182, 76);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 12;
            this.picLogo.TabStop = false;
            this.ttpHelper.SetToolTip(this.picLogo, "Click here to open the homepage!");
            this.picLogo.Click += this.OnLogoClicked;
            // 
            // lblProduct
            // 
            this.lblProduct.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblProduct.Location = new Point(14, 14);
            this.lblProduct.Margin = new Padding(4, 3, 4, 3);
            this.lblProduct.Name = "lblProduct";
            this.lblProduct.Size = new Size(187, 20);
            this.lblProduct.TabIndex = 0;
            this.lblProduct.Text = "Product";
            this.lblProduct.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblVersion.Location = new Point(14, 40);
            this.lblVersion.Margin = new Padding(4, 3, 4, 3);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new Size(187, 20);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Version";
            this.lblVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCopyright
            // 
            this.lblCopyright.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblCopyright.Location = new Point(14, 67);
            this.lblCopyright.Margin = new Padding(4, 3, 4, 3);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new Size(187, 20);
            this.lblCopyright.TabIndex = 2;
            this.lblCopyright.Text = "Copyright";
            this.lblCopyright.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.txtDescription.BackColor = Color.White;
            this.txtDescription.Location = new Point(14, 97);
            this.txtDescription.Margin = new Padding(4, 3, 4, 3);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = ScrollBars.Both;
            this.txtDescription.Size = new Size(375, 114);
            this.txtDescription.TabIndex = 3;
            this.txtDescription.TabStop = false;
            this.txtDescription.Text = "Description";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Location = new Point(302, 218);
            this.btnClose.Margin = new Padding(4, 3, 4, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(88, 29);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "&Close";
            this.btnClose.Click += this.OnCloseButtonClicked;
            // 
            // ttpHelper
            // 
            this.ttpHelper.ShowAlways = true;
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new Size(404, 261);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblProduct);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.txtDescription);
            this.Margin = new Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(420, 300);
            this.Name = "AboutDialog";
            this.Padding = new Padding(10);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Show;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About ";
            ((System.ComponentModel.ISupportInitialize)this.picLogo).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private PictureBox picLogo;
        private Label lblProduct;
        private Label lblVersion;
        private Label lblCopyright;
        private TextBox txtDescription;
        private Button btnClose;
        private ToolTip ttpHelper;
    }
}
