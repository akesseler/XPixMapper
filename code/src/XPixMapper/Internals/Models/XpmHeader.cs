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

using Plexdata.Utilities.XPixMapper.Internals.Defines;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Plexdata.Utilities.XPixMapper.Internals.Models
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal class XpmHeader
    {
        #region Construction

        private XpmHeader() { }

        #endregion

        #region Properties

        public Int32 Width { get; private set; } = 0;

        public Int32 Height { get; private set; } = 0;

        public Int32 NumberOfColors { get; private set; } = 0;

        public Int32 CharactersPerPixel { get; private set; } = 0;

        public Point? Hotspot { get; private set; } = null;

        public Boolean HasExtensions { get; private set; } = false;

        #endregion

        #region Methods

        public static XpmHeader Parse(String source)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentOutOfRangeException(nameof(source), $"Parameter '{nameof(source)}' must not be null, empty or white space.");
            }

            String[] values = source
                .Split()
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToArray();

            if (values.Length < 4)
            {
                throw new FormatException("Header must consist of at least four values.");
            }

            if (!Int32.TryParse(values[0], out Int32 width))
            {
                throw new FormatException($"Unable to convert value of '{nameof(XpmHeader.Width)}' into an integer.");
            }

            if (!Int32.TryParse(values[1], out Int32 height))
            {
                throw new FormatException($"Unable to convert value of '{nameof(XpmHeader.Height)}' into an integer.");
            }

            if (!Int32.TryParse(values[2], out Int32 numberOfColors))
            {
                throw new FormatException($"Unable to convert value of '{nameof(XpmHeader.NumberOfColors)}' into an integer.");
            }

            if (!Int32.TryParse(values[3], out Int32 charactersPerPixel))
            {
                throw new FormatException($"Unable to convert value of '{nameof(XpmHeader.CharactersPerPixel)}' into an integer.");
            }

            Point? hotspot = null;
            Boolean extensions = false;

            if (values.Length > 4)
            {
                extensions = String.Equals(values[4], XpmConstants.XpmExtTag, StringComparison.InvariantCultureIgnoreCase);
            }

            if (!extensions && values.Length > 5)
            {
                Int32.TryParse(values[4], out Int32 x);
                Int32.TryParse(values[5], out Int32 y);

                hotspot = new Point(x, y);
            }

            if (!extensions && values.Length > 6)
            {
                extensions = String.Equals(values[6], XpmConstants.XpmExtTag, StringComparison.InvariantCultureIgnoreCase);
            }

            return new XpmHeader()
            {
                Width = width,
                Height = height,
                NumberOfColors = numberOfColors,
                CharactersPerPixel = charactersPerPixel,
                Hotspot = hotspot,
                HasExtensions = extensions
            };
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder(128);

            builder.Append($"{this.Width} {this.Height} {this.NumberOfColors} {this.CharactersPerPixel} ");

            if (this.Hotspot != null)
            {
                builder.Append($"{this.Hotspot.Value.X} {this.Hotspot.Value.Y} ");
            }

            if (this.HasExtensions)
            {
                builder.Append($"{XpmConstants.XpmExtTag} ");
            }

            builder.Length--; // Cut off last space...

            return builder.ToString().Trim();
        }

        [ExcludeFromCodeCoverage]
        private String GetDebuggerDisplay()
        {
            return $"{nameof(this.Width)}={this.Width}, " +
                   $"{nameof(this.Height)}={this.Height}, " +
                   $"{nameof(this.NumberOfColors)}={this.NumberOfColors}, " +
                   $"{nameof(this.CharactersPerPixel)}={this.CharactersPerPixel}";
        }

        #endregion
    }
}
