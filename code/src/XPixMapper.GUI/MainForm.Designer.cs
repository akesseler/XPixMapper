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

using Plexdata.XPixMapper.GUI.Controls;

namespace Plexdata.XPixMapper.GUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbImageView = new ImageView();
            this.txLinesEdit = new TextBox();
            this.tlLayouter = new TableLayoutPanel();
            this.btLeftToRight = new MenuSplitButton();
            this.btRightToLeft = new MenuSplitButton();
            this.tlLayouter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbImageView
            // 
            this.pbImageView.AutoScrollMinSize = new Size(20, 20);
            this.pbImageView.BackColor = Color.White;
            this.pbImageView.Dock = DockStyle.Fill;
            this.pbImageView.ErrorText = "";
            this.pbImageView.Interpolation = false;
            this.pbImageView.Location = new Point(410, 0);
            this.pbImageView.Margin = new Padding(0);
            this.pbImageView.Name = "pbImageView";
            this.tlLayouter.SetRowSpan(this.pbImageView, 4);
            this.pbImageView.Size = new Size(350, 437);
            this.pbImageView.SuppressScrollbars = false;
            this.pbImageView.TabIndex = 3;
            // 
            // txLinesEdit
            // 
            this.txLinesEdit.Dock = DockStyle.Fill;
            this.txLinesEdit.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.txLinesEdit.Location = new Point(0, 0);
            this.txLinesEdit.Margin = new Padding(0);
            this.txLinesEdit.MaxLength = Int32.MaxValue;
            this.txLinesEdit.Multiline = true;
            this.txLinesEdit.Name = "txLinesEdit";
            this.tlLayouter.SetRowSpan(this.txLinesEdit, 4);
            this.txLinesEdit.ScrollBars = ScrollBars.Both;
            this.txLinesEdit.Size = new Size(350, 437);
            this.txLinesEdit.TabIndex = 0;
            this.txLinesEdit.WordWrap = false;
            // 
            // tlLayouter
            // 
            this.tlLayouter.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.tlLayouter.ColumnCount = 3;
            this.tlLayouter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tlLayouter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            this.tlLayouter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tlLayouter.Controls.Add(this.pbImageView, 2, 0);
            this.tlLayouter.Controls.Add(this.btLeftToRight, 1, 1);
            this.tlLayouter.Controls.Add(this.btRightToLeft, 1, 2);
            this.tlLayouter.Controls.Add(this.txLinesEdit, 0, 0);
            this.tlLayouter.Location = new Point(12, 12);
            this.tlLayouter.Name = "tlLayouter";
            this.tlLayouter.RowCount = 4;
            this.tlLayouter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.tlLayouter.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            this.tlLayouter.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            this.tlLayouter.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.tlLayouter.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            this.tlLayouter.Size = new Size(760, 437);
            this.tlLayouter.TabIndex = 0;
            // 
            // btLeftToRight
            // 
            this.btLeftToRight.Dock = DockStyle.Fill;
            this.btLeftToRight.Location = new Point(355, 188);
            this.btLeftToRight.Margin = new Padding(5);
            this.btLeftToRight.Name = "btLeftToRight";
            this.btLeftToRight.Size = new Size(50, 25);
            this.btLeftToRight.TabIndex = 1;
            this.btLeftToRight.Text = ">>";
            this.btLeftToRight.UseVisualStyleBackColor = true;
            this.btLeftToRight.Click += this.OnButtonLeftToRightClick;
            // 
            // btRightToLeft
            // 
            this.btRightToLeft.Dock = DockStyle.Fill;
            this.btRightToLeft.Location = new Point(355, 223);
            this.btRightToLeft.Margin = new Padding(5);
            this.btRightToLeft.Name = "btRightToLeft";
            this.btRightToLeft.Size = new Size(50, 25);
            this.btRightToLeft.TabIndex = 2;
            this.btRightToLeft.Text = "<<";
            this.btRightToLeft.UseVisualStyleBackColor = true;
            this.btRightToLeft.Click += this.OnButtonRightToLeftClick;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 461);
            this.Controls.Add(this.tlLayouter);
            this.Name = "MainForm";
            this.SizeGripStyle = SizeGripStyle.Show;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "XPixMapper";
            this.tlLayouter.ResumeLayout(false);
            this.tlLayouter.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private ImageView pbImageView;
        private TextBox txLinesEdit;
        private TableLayoutPanel tlLayouter;
        private MenuSplitButton btLeftToRight;
        private MenuSplitButton btRightToLeft;
    }
}
