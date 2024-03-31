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

using Plexdata.Utilities.XPixMapper;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

namespace Plexdata.XPixMapper.GUI.Controls
{
    public class RadioMenuItem : ToolStripMenuItem
    {
        #region Events

        public event EventHandler<EventArgs> MenuItemTypeChanged;

        #endregion

        #region Fields

        private XPixMapType type = XPixMapType.Default;
        private Boolean mouseDown = false;
        private Boolean mouseHover = false;

        #endregion

        #region Construction

        public RadioMenuItem(String text, XPixMapType type)
            : base(text)
        {
            this.Initialize(type);
        }

        #endregion

        #region Properties

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image Image
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw new NotSupportedException();
                }
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XPixMapType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                if (this.type != value)
                {
                    this.type = value;

                    this.MenuItemTypeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Messages

        protected override Boolean ProcessCmdKey(ref Message message, Keys keys)
        {
            if (base.Enabled && keys == Keys.Space)
            {
                base.PerformClick();
                return true;
            }

            return base.ProcessCmdKey(ref message, keys);
        }

        protected override void OnCheckedChanged(EventArgs args)
        {
            base.OnCheckedChanged(args);

            if (!base.Checked || base.Parent is null)
            {
                return;
            }

            foreach (ToolStripItem item in base.Parent.Items)
            {
                if (item is RadioMenuItem radio && radio != this && radio.Checked)
                {
                    radio.Checked = false;
                    return;
                }
            }
        }

        protected override void OnClick(EventArgs args)
        {
            if (base.Checked)
            {
                return;
            }

            base.OnClick(args);
        }

        protected override void OnPaint(PaintEventArgs args)
        {
            this.DoDrawParent(args);

            RadioButtonState buttonState;

            if (this.Enabled)
            {
                if (this.mouseDown)
                {
                    buttonState = base.Checked ? RadioButtonState.CheckedPressed : RadioButtonState.UncheckedPressed;
                }
                else if (this.mouseHover)
                {
                    buttonState = base.Checked ? RadioButtonState.CheckedHot : RadioButtonState.UncheckedHot;
                }
                else
                {
                    buttonState = base.Checked ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal;
                }
            }
            else
            {
                buttonState = base.Checked ? RadioButtonState.CheckedDisabled : RadioButtonState.UncheckedDisabled;
            }

            Size glyph = RadioButtonRenderer.GetGlyphSize(args.Graphics, buttonState);
            Rectangle content = base.ContentRectangle;
            Int32 offset = (content.Height - glyph.Height) / 2;
            Point location = new Point(content.Location.X + 4, content.Location.Y + offset);

            RadioButtonRenderer.DrawRadioButton(args.Graphics, location, buttonState);
        }

        protected override void OnMouseEnter(EventArgs args)
        {
            this.mouseHover = true;
            base.Invalidate();
            base.OnMouseEnter(args);
        }

        protected override void OnMouseLeave(EventArgs args)
        {
            this.mouseHover = false;
            base.OnMouseLeave(args);
        }

        protected override void OnMouseDown(MouseEventArgs args)
        {
            this.mouseDown = true;
            base.Invalidate();
            base.OnMouseDown(args);
        }

        protected override void OnMouseUp(MouseEventArgs args)
        {
            this.mouseDown = false;
            base.OnMouseUp(args);
        }

        #endregion

        #region Helpers

        private void Initialize(XPixMapType type)
        {
            base.CheckOnClick = true;
            this.Type = type;
        }

        private void DoDrawParent(PaintEventArgs args)
        {
            // This is actually ugly! But it would be
            // too much effort to do this manually.

            CheckState currentState = this.CheckState;

            try
            {
                base.CheckState = CheckState.Unchecked;
                base.OnPaint(args);
            }
            finally
            {
                base.CheckState = currentState;
            }
        }

        #endregion
    }
}

#nullable restore