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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Plexdata.Utilities.XPixMapper.Internals.Models
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal class XPixMap
    {
        #region Construction

        private XPixMap() { }

        #endregion

        #region Properties

        public Boolean IsValid { get { return this.Header != null && this.Colors != null && this.Pixels != null; } }

        public XpmHeader Header { get; private set; } = null;

        public IDictionary<String, XpmColor> Colors { get; private set; } = null;

        public XpmPixels Pixels { get; private set; } = null;

        public XpmExtensions Extensions { get; private set; } = null;

        #endregion

        #region Methods

        public static XPixMap Parse(String[] source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), $"Parameter '{nameof(source)}' must not be null.");
            }

            if (source.Length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(source), $"Parameter '{nameof(source)}' must consist of at least one line.");
            }

            if (source.Any(x => x is null))
            {
                throw new ArgumentOutOfRangeException(nameof(source), $"Parameter '{nameof(source)}' contains at least one null line.");
            }

            Int32 skip = 0;
            Int32 take = 1;

            if (source[0].TrimStart().StartsWith("!"))
            {
                skip = 1; // Skip file header if one.
            }

            if (source.Length < (skip + 1))
            {
                throw new ArgumentOutOfRangeException(nameof(source), $"Parameter '{nameof(source)}' must consist of at least the header.");
            }

            XpmHeader header = XpmHeader.Parse(source.Skip(skip).Take(take).First());

            skip += take;
            take = header.NumberOfColors;
            IDictionary<String, XpmColor> colors = XpmColor.Parse(header, source.Skip(skip).Take(take).ToArray());

            skip += take;
            take = header.Height;
            XpmPixels pixels = XpmPixels.Parse(header, source.Skip(skip).Take(take).ToArray());

            skip += take;
            XpmExtensions extensions = XpmExtensions.Parse(header, source.Skip(skip).ToArray());

            return new XPixMap()
            {
                Header = header,
                Colors = colors,
                Pixels = pixels,
                Extensions = extensions
            };
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder(512);

            if (this.Header != null)
            {
                builder.AppendLine(this.Header.ToString());
            }

            if (this.Colors != null)
            {
                foreach (KeyValuePair<String, XpmColor> current in this.Colors)
                {
                    builder.AppendLine(current.Value.ToString());
                }
            }

            if (this.Pixels != null)
            {
                builder.AppendLine(this.Pixels.ToString());
            }

            if (this.Extensions != null)
            {
                builder.AppendLine(this.Extensions.ToString());
            }

            String result = builder.ToString();

            if (!result.EndsWith($"{Environment.NewLine}{Environment.NewLine}"))
            {
                return result;
            }

            return result.Substring(0, result.Length - Environment.NewLine.Length);
        }

        [ExcludeFromCodeCoverage]
        private String GetDebuggerDisplay()
        {
            return $"{nameof(this.Header)}={this.Header?.ToString() ?? "null"}, {nameof(this.IsValid)}={this.IsValid}";
        }

        #endregion
    }
}
