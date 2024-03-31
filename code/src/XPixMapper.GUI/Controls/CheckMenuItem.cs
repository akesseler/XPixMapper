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

namespace Plexdata.XPixMapper.GUI.Controls
{
    public class CheckMenuItem : ToolStripMenuItem
    {
        #region Events

        public event EventHandler<EventArgs> MenuItemTypeChanged;

        #endregion

        #region Fields

        private XPixMapType type = XPixMapType.Default;

        #endregion

        #region Construction

        public CheckMenuItem(String text, XPixMapType type)
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

        #endregion

        #region Helpers

        private void Initialize(XPixMapType type)
        {
            base.CheckOnClick = true;
            this.Type = type;
        }

        #endregion
    }
}

#nullable restore