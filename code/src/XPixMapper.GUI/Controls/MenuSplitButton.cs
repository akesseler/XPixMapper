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

#nullable disable

using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace Plexdata.XPixMapper.GUI.Controls
{
    public class MenuSplitButton : Button
    {
        #region Fields

        private static readonly Int32 borderSize = SystemInformation.Border3DSize.Width;
        private static readonly Int32 splitWidth = 13;

        private PushButtonState pushState = PushButtonState.Normal;
        private Boolean showSplit = false;
        private Boolean lockShowMenu = false;
        private Rectangle splitBounds = Rectangle.Empty;
        private Rectangle valueBounds = Rectangle.Empty;
        private Boolean isMenuVisible = false;
        private ContextMenuStrip menuStrip = null;

        #endregion

        #region Construction

        public MenuSplitButton() : base() { }

        #endregion

        #region Properties

        [DefaultValue(false)]
        public Boolean ShowSplit
        {
            get
            {
                return this.showSplit;
            }
            set
            {
                if (this.showSplit != value)
                {
                    this.showSplit = value;

                    base.Invalidate();

                    if (base.Parent != null)
                    {
                        base.Parent.PerformLayout();
                    }
                }
            }
        }

        [DefaultValue(null)]
        public override ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this.menuStrip;
            }
            set
            {
                if (this.menuStrip != null)
                {
                    this.menuStrip.Closing -= this.OnDropMenuClosing;
                    this.menuStrip.Opening -= this.OnDropMenuOpening;
                }

                this.menuStrip = null;

                if (value != null)
                {
                    this.menuStrip = value;
                    this.menuStrip.Closing += this.OnDropMenuClosing;
                    this.menuStrip.Opening += this.OnDropMenuOpening;
                }

                this.ShowSplit = this.menuStrip != null;

                base.OnContextMenuStripChanged(EventArgs.Empty);
            }
        }

        protected override Padding DefaultPadding
        {
            get
            {
                Padding result = base.DefaultPadding;
                result.Left += 3;
                return result;
            }
        }

        protected override Boolean ShowFocusCues
        {
            get
            {
                return false;
            }
        }

        private PushButtonState PushState
        {
            get
            {
                return this.pushState;
            }
            set
            {
                if (this.pushState != value)
                {
                    this.pushState = value;
                    base.Invalidate();
                }
            }
        }

        #endregion

        #region Events

        protected override void OnSizeChanged(EventArgs args)
        {
            Rectangle client = base.ClientRectangle;

            this.splitBounds = new Rectangle(client.Right - MenuSplitButton.splitWidth, client.Top, MenuSplitButton.splitWidth, client.Height);
            this.valueBounds = new Rectangle(client.Left, client.Top, client.Width - MenuSplitButton.splitWidth, client.Height);

            base.OnSizeChanged(args);
        }

        protected override void OnEnabledChanged(EventArgs args)
        {
            this.PushState = base.Enabled ? PushButtonState.Normal : PushButtonState.Disabled;

            base.OnEnabledChanged(args);
        }

        protected override void OnGotFocus(EventArgs args)
        {
            if (!this.ShowSplit)
            {
                base.OnGotFocus(args);
                return;
            }

            if (this.PushState != PushButtonState.Pressed && this.PushState != PushButtonState.Disabled)
            {
                this.PushState = PushButtonState.Hot;
            }
        }

        protected override void OnLostFocus(EventArgs args)
        {
            if (!this.ShowSplit)
            {
                base.OnLostFocus(args);
                return;
            }

            if (this.PushState != PushButtonState.Pressed && PushState != PushButtonState.Disabled)
            {
                this.PushState = PushButtonState.Normal;
            }
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            if (this.ShowSplit)
            {
                if (args.Modifiers == Keys.None && args.KeyCode == Keys.Space)
                {
                    this.PushState = PushButtonState.Pressed;
                }

                if (!this.isMenuVisible && args.KeyCode == Keys.Down)
                {
                    this.ShowContextMenuStrip();
                }
            }

            base.OnKeyDown(args);
        }

        protected override void OnKeyUp(KeyEventArgs args)
        {
            if (Control.MouseButtons == MouseButtons.None)
            {
                if (args.KeyCode == Keys.Space)
                {
                    this.PushState = base.Focused ? PushButtonState.Hot : PushButtonState.Normal;
                }

                if (!this.isMenuVisible && args.KeyCode == Keys.Apps)
                {
                    this.ShowContextMenuStrip();
                }
            }

            base.OnKeyUp(args);
        }

        protected override void OnMouseEnter(EventArgs args)
        {
            if (!this.ShowSplit)
            {
                base.OnMouseEnter(args);
                return;
            }

            if (this.PushState != PushButtonState.Pressed && this.PushState != PushButtonState.Disabled)
            {
                this.PushState = PushButtonState.Hot;
            }
        }

        protected override void OnMouseLeave(EventArgs args)
        {
            if (!this.ShowSplit)
            {
                base.OnMouseLeave(args);
                return;
            }

            if (this.PushState != PushButtonState.Pressed && PushState != PushButtonState.Disabled)
            {
                this.PushState = base.Focused ? PushButtonState.Hot : PushButtonState.Normal;
            }
        }

        protected override void OnMouseDown(MouseEventArgs args)
        {
            if (!this.ShowSplit)
            {
                base.OnMouseDown(args);
                return;
            }

            if (!this.isMenuVisible && args.Button == MouseButtons.Left && this.splitBounds.Contains(args.Location))
            {
                this.ShowContextMenuStrip();
            }
            else
            {
                this.PushState = PushButtonState.Pressed;
            }
        }

        protected override void OnMouseUp(MouseEventArgs args)
        {
            if (!this.ShowSplit)
            {
                base.OnMouseUp(args);
                return;
            }

            if (!this.isMenuVisible && args.Button == MouseButtons.Right && base.ClientRectangle.Contains(args.Location))
            {
                this.ShowContextMenuStrip();
                return;
            }

            if (!this.isMenuVisible || this.ContextMenuStrip is null)
            {
                this.SetButtonDrawState();

                if (!this.splitBounds.Contains(args.Location) && base.ClientRectangle.Contains(args.Location))
                {
                    base.OnClick(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs args)
        {
            base.OnPaint(args);

            if (!this.ShowSplit)
            {
                return;
            }

            this.DoDrawButton(args.Graphics);
            this.DoDrawSplit(args.Graphics);
            this.DoDrawText(args.Graphics);
            this.DoDrawFocus(args.Graphics);
        }

        private void DoDrawButton(Graphics graphics)
        {
            ButtonRenderer.DrawButton(graphics, base.ClientRectangle, this.PushState);
        }

        private void DoDrawSplit(Graphics graphics)
        {
            GraphicsState state = graphics.Save();

            try
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                if (this.PushState == PushButtonState.Hot || this.PushState == PushButtonState.Pressed)
                {
                    Int32 x1 = this.splitBounds.Left;
                    Int32 y1 = this.splitBounds.Top + MenuSplitButton.borderSize;
                    Int32 x2 = this.splitBounds.Left;
                    Int32 y2 = this.splitBounds.Bottom - MenuSplitButton.borderSize - 1;

                    graphics.DrawLine(SystemPens.ButtonShadow, x1, y1, x2, y2);
                }

                Single x = this.splitBounds.Left + (this.splitBounds.Width - MenuSplitButton.borderSize) / 2F;
                Single y = this.splitBounds.Top + (this.splitBounds.Height - MenuSplitButton.borderSize) / 2F;

                PointF[] points = new PointF[]
                {
                    new PointF(x - 3, y - 1),
                    new PointF(x + 3, y - 1),
                    new PointF(x,     y + 2),
                };

                Brush brush = base.Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow;

                graphics.FillPolygon(brush, points);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private void DoDrawText(Graphics graphics)
        {
            if (String.IsNullOrWhiteSpace(base.Text))
            {
                return;
            }

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;

            if (!base.UseMnemonic)
            {
                flags |= TextFormatFlags.NoPrefix;
            }

            if (!base.ShowKeyboardCues)
            {
                flags |= TextFormatFlags.HidePrefix;
            }

            Int32 x = this.valueBounds.Left + MenuSplitButton.borderSize + base.Padding.Left;
            Int32 y = this.valueBounds.Top + MenuSplitButton.borderSize + base.Padding.Top;
            Int32 w = this.valueBounds.Width - (MenuSplitButton.borderSize + base.Padding.Horizontal) - 1;
            Int32 h = this.valueBounds.Height - (2 * MenuSplitButton.borderSize + base.Padding.Vertical) - 1;

            Rectangle bounds = new Rectangle(x, y, w, h);

            if (base.Enabled)
            {
                TextRenderer.DrawText(graphics, base.Text, base.Font, bounds, base.ForeColor, flags);
            }
            else
            {
                ControlPaint.DrawStringDisabled(graphics, base.Text, base.Font, SystemColors.GrayText, bounds, flags);
            }
        }

        private void DoDrawFocus(Graphics graphics)
        {
            if (this.PushState != PushButtonState.Pressed && base.Focused && this.ShowFocusCues)
            {
                Rectangle bounds = base.ClientRectangle;
                bounds.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(graphics, bounds);
            }
        }

        #endregion

        #region Menu

        private void OnDropMenuOpening(Object sender, CancelEventArgs args)
        {
            this.isMenuVisible = true;
        }

        private void OnDropMenuClosing(Object sender, ToolStripDropDownClosingEventArgs args)
        {
            this.isMenuVisible = false;

            this.SetButtonDrawState();

            if (args.CloseReason == ToolStripDropDownCloseReason.AppClicked)
            {
                this.lockShowMenu = Control.MouseButtons == MouseButtons.Left && this.splitBounds.Contains(base.PointToClient(Cursor.Position));
            }
        }

        private void ShowContextMenuStrip()
        {
            if (this.lockShowMenu)
            {
                this.lockShowMenu = false;
                return;
            }

            this.PushState = PushButtonState.Pressed;

            this.ContextMenuStrip?.Show(this, new Point(0, base.Height), ToolStripDropDownDirection.BelowRight);
        }

        #endregion

        #region Helpers

        private void SetButtonDrawState()
        {
            if (base.Bounds.Contains(base.Parent.PointToClient(Cursor.Position)))
            {
                this.PushState = PushButtonState.Hot;
            }
            else if (base.Focused)
            {
                this.PushState = PushButtonState.Hot;
            }
            else if (!base.Enabled)
            {
                this.PushState = PushButtonState.Disabled;
            }
            else
            {
                this.PushState = PushButtonState.Normal;
            }
        }

        #endregion
    }
}

#nullable restore

