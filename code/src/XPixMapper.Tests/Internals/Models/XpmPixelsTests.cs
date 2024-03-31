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

using NUnit.Framework;
using Plexdata.Utilities.Testing;
using Plexdata.Utilities.XPixMapper.Internals.Models;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;

namespace Plexdata.Utilities.XPixMapper.Tests.Internals.Models
{
    [ExcludeFromCodeCoverage]
    [Category(TestType.UnitTest)]
    public class XpmPixelsTests
    {
        #region Parse

        [Test]
        public void Parse_XmpHeaderIsNull_ThrowsArgumentNullException()
        {
            XpmHeader header = null;
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'header' must not be null."));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void Parse_XmpHeaderWidthIsInvalid_ThrowsArgumentOutOfRangeException(Int32 width)
        {
            XpmHeader header = this.CreateHeader(width);
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("The width must not be less that one."));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void Parse_XmpHeaderHeightIsInvalid_ThrowsArgumentOutOfRangeException(Int32 height)
        {
            XpmHeader header = this.CreateHeader(10, height);
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("The height must not be less that one."));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void Parse_XmpHeaderCharactersPerPixelIsInvalid_ThrowsArgumentOutOfRangeException(Int32 cpp)
        {
            XpmHeader header = this.CreateHeader(10, 10, cpp);
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("The characters per pixel must not be less that one."));
        }

        [Test]
        public void Parse_SourceIsNull_ThrowsArgumentNullException()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = null;

            Assert.That(() => XpmPixels.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null or empty."));
        }

        [Test]
        public void Parse_SourceIsEmpty_ThrowsArgumentNullException()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = Array.Empty<String>();

            Assert.That(() => XpmPixels.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null or empty."));
        }

        [Test]
        public void Parse_SourceLengthIsLessThanHeight_ThrowsArgumentOutOfRangeException()
        {
            XpmHeader header = this.CreateHeader(2, 3);
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' must have at least a number of '3' lines."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("       ")]
        public void Parse_SourceWithInvalidLine_ThrowsFormatException(String line)
        {
            XpmHeader header = this.CreateHeader(8, 3);
            String[] source = new String[] { "xxxxxxxx", line, "xxxxxxxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo("Pixel line at '2' must have at least a length of '8'."));
        }

        [TestCase("0")]
        [TestCase("00")]
        [TestCase("000")]
        [TestCase("0000")]
        [TestCase("00000")]
        [TestCase("000000")]
        [TestCase("0000000")]
        public void Parse_SourceWithLesserLine_ThrowsFormatException(String line)
        {
            XpmHeader header = this.CreateHeader(4, 3, 2);
            String[] source = new String[] { "xxxxxxxx", line, "xxxxxxxx" };

            Assert.That(() => XpmPixels.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo("Pixel line at '2' must have at least a length of '8'."));
        }

        [Test]
        public void Parse_ParametersValid_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader(4, 2, 3);
            String[] source = new String[] { "00 01 02 03 ", "10 11 12 13 " };

            String[,] actual = XpmPixels.Parse(header, source).Values;

            for (Int32 row = 0; row < 2; row++)
            {
                for (Int32 col = 0; col < 4; col++)
                {
                    Assert.That(actual[col, row], Is.EqualTo($"{row}{col} "));
                }
            }
        }

        #endregion

        #region ToString

        [Test]
        public void ToString_ParametersValid_ResultAsExpected()
        {
            String eol = Environment.NewLine;
            XpmHeader header = this.CreateHeader(4, 3, 2);
            String[] source = new String[] { "00010203", "10111213", "20212223" };

            String actual = XpmPixels.Parse(header, source).ToString();

            Assert.That(actual, Is.EqualTo($"00010203{eol}10111213{eol}20212223"));
        }

        #endregion

        #region Helpers

        private XpmHeader CreateHeader(Int32 w = 8, Int32 h = 8, Int32 cpp = 1)
        {
            XpmHeader header = (XpmHeader)Activator.CreateInstance(typeof(XpmHeader), true);

            PropertyInfo property;

            PropertyInfo[] properties = header.GetType().GetProperties();

            property = properties.First(x => x.Name == nameof(XpmHeader.Width));
            property.SetValue(header, w);

            property = properties.First(x => x.Name == nameof(XpmHeader.Height));
            property.SetValue(header, h);

            property = properties.First(x => x.Name == nameof(XpmHeader.NumberOfColors));
            property.SetValue(header, 3);

            property = properties.First(x => x.Name == nameof(XpmHeader.CharactersPerPixel));
            property.SetValue(header, cpp);

            property = properties.First(x => x.Name == nameof(XpmHeader.Hotspot));
            property.SetValue(header, new Point(1, 1));

            property = properties.First(x => x.Name == nameof(XpmHeader.HasExtensions));
            property.SetValue(header, true);

            return header;
        }

        #endregion
    }
}
