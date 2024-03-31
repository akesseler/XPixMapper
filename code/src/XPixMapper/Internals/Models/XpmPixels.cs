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
    internal class XpmPixels
    {
        #region Construction

        private XpmPixels() { }

        #endregion

        #region Properties

        public Int32 Width { get { return this.Values.GetLength(0); } }

        public Int32 Height { get { return this.Values.GetLength(1); } }

        public String[,] Values { get; private set; } = new String[0, 0];

        #endregion

        #region Methods

        public static XpmPixels Parse(XpmHeader header, String[] source)
        {
            if (header is null)
            {
                throw new ArgumentNullException(nameof(header),
                    $"Parameter '{nameof(header)}' must not be null.");
            }

            if (header.Width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(header),
                    "The width must not be less that one.");
            }

            if (header.Height < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(header),
                    "The height must not be less that one.");
            }

            if (header.CharactersPerPixel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(header),
                    "The characters per pixel must not be less that one.");
            }

            if (!source?.Any() ?? true)
            {
                throw new ArgumentNullException(nameof(source),
                    $"Parameter '{nameof(source)}' must not be null or empty.");
            }

            if (header.Height > source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(source),
                    $"Parameter '{nameof(source)}' must have at least a number of '{header.Height}' lines.");
            }

            Int32 cpp = header.CharactersPerPixel;
            Int32 cols = header.Width;
            Int32 rows = header.Height;

            String[,] values = new String[cols, rows];

            for (Int32 row = 0; row < rows; row++)
            {
                String[] line = XpmPixels.Parse(cpp, row, cols, source[row] ?? String.Empty);

                for (Int32 col = 0; col < cols; col++)
                {
                    values[col, row] = line[col];
                }
            }

            return new XpmPixels()
            {
                Values = values
            };
        }

        public override String ToString()
        {
            List<String> result = new List<String>();

            Int32 cols = this.Width;
            Int32 rows = this.Height;

            StringBuilder builder = new StringBuilder(256);

            for (Int32 row = 0; row < rows; row++)
            {
                builder.Length = 0;

                for (Int32 col = 0; col < cols; col++)
                {
                    builder.Append(this.Values[col, row]);
                }

                result.Add(builder.ToString());
            }

            return String.Join(Environment.NewLine, result);
        }

        private static String[] Parse(Int32 cpp, Int32 row, Int32 cols, String source)
        {
            if (source.Length < (cpp * cols))
            {
                throw new FormatException($"Pixel line at '{row + 1}' must have at least a length of '{cpp * cols}'.");
            }

            return Enumerable
                .Range(0, cols)
                .Select(idx => source.Substring(idx * cpp, cpp))
                .ToArray();
        }

        [ExcludeFromCodeCoverage]
        private String GetDebuggerDisplay()
        {
            return $"{nameof(this.Values)}={this.Values.Length}";
        }

        #endregion
    }
}
