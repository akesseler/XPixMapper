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

using Plexdata.Utilities.XPixMapper;
using System.ComponentModel;

namespace Plexdata.XPixMapper.GUI.Controls
{
    public class RadioMenuStrip : ContextMenuStrip
    {
        #region Methods

        public static RadioMenuStrip Create()
        {
            RadioMenuStrip result = new RadioMenuStrip();

            result.Items.AddRange(new RadioMenuItem[]
            {
                new RadioMenuItem("Colored",    XPixMapType.Colored)    { Checked = true  },
                new RadioMenuItem("Grayscale",  XPixMapType.Grayscale)  { Checked = false },
                new RadioMenuItem("Monochrome", XPixMapType.Monochrome) { Checked = false }
            });

            return result;
        }

        #endregion

        #region Construction

        private RadioMenuStrip() : base() { }

        #endregion

        #region Properties

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XPixMapType Selected
        {
            get
            {
                XPixMapType result = XPixMapType.Default;

                foreach (ToolStripItem item in base.Items)
                {
                    if (item is RadioMenuItem radio && radio.Checked)
                    {
                        result |= radio.Type;
                    }
                }

                return result;
            }
        }

        #endregion
    }
}
