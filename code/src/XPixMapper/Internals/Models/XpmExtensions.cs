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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Plexdata.Utilities.XPixMapper.Internals.Models
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal class XpmExtensions
    {
        #region Construction

        public XpmExtensions() { }

        #endregion

        #region Properties

        public String[] Lines { get; private set; } = new String[0];

        #endregion

        #region Methods

        public static XpmExtensions Parse(XpmHeader header, String[] source)
        {
            if (header is null)
            {
                throw new ArgumentNullException(nameof(header),
                    $"Parameter '{nameof(header)}' must not be null.");
            }

            if (!header.HasExtensions)
            {
                return new XpmExtensions();
            }

            if (source is null)
            {
                throw new ArgumentNullException(nameof(source),
                    $"Parameter '{nameof(source)}' must not be null.");
            }

            // According to the documents, an extension is created from a name
            // and associated data. But how often is an extension actually used?
            // Therefore, all lines are simply collected here. 

            List<String> lines = new List<String>();

            for (Int32 index = 0; index < source.Length; index++)
            {
                String line = source[index];

                if (line.StartsWith(XpmConstants.XpmExtEnd, StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }

                if (line.StartsWith(XpmConstants.XpmExtTag, StringComparison.InvariantCultureIgnoreCase))
                {
                    line = line.Remove(0, XpmConstants.XpmExtTag.Length).TrimStart();
                }

                if (!String.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }

            return new XpmExtensions()
            {
                Lines = lines.ToArray()
            };
        }

        public override String ToString()
        {
            if (this.Lines.Length < 1)
            {
                return String.Empty;
            }

            List<String> result = new List<String>();

            for (Int32 line = 0; line < this.Lines.Length; line++)
            {
                result.Add($"{XpmConstants.XpmExtTag} {this.Lines[line]}");
            }

            result.Add($"{XpmConstants.XpmExtEnd}");

            return String.Join(Environment.NewLine, result);
        }

        [ExcludeFromCodeCoverage]
        private String GetDebuggerDisplay()
        {
            return $"{nameof(this.Lines)}={this.Lines.Length}";
        }

        #endregion
    }
}
