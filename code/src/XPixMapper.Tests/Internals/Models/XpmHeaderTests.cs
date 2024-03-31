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
    public class XpmHeaderTests
    {
        #region Parse

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Parse_SourceIsInvalid_ThrowsArgumentOutOfRangeException(String source)
        {
            Assert.That(() => XpmHeader.Parse(source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' must not be null, empty or white space."));
        }

        [TestCase("xxx")]
        [TestCase(" xxx\t")]
        [TestCase(" xxx \t yyy \t ")]
        [TestCase(" xxx \t yyy \t zzz ")]
        public void Parse_SourceContainsTooFewItems_ThrowsFormatException(String source)
        {
            Assert.That(() => XpmHeader.Parse(source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo("Header must consist of at least four values."));
        }

        [TestCase("xxx 111 222 333", "Unable to convert value of 'Width' into an integer.")]
        [TestCase("111 xxx 222 333", "Unable to convert value of 'Height' into an integer.")]
        [TestCase("111 222 xxx 333", "Unable to convert value of 'NumberOfColors' into an integer.")]
        [TestCase("111 222 333 xxx", "Unable to convert value of 'CharactersPerPixel' into an integer.")]
        public void Parse_SourceContainsInvalidNumbers_ThrowsFormatException(String source, String expected)
        {
            Assert.That(() => XpmHeader.Parse(source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo(expected));
        }

        [Test]
        public void Parse_SourceIsValidWithFourItems_ResultAsExpected()
        {
            String source = "  111\t222 333\t444";

            XpmHeader actual = XpmHeader.Parse(source);

            Assert.That(actual.Width, Is.EqualTo(111));
            Assert.That(actual.Height, Is.EqualTo(222));
            Assert.That(actual.NumberOfColors, Is.EqualTo(333));
            Assert.That(actual.CharactersPerPixel, Is.EqualTo(444));
            Assert.That(actual.Hotspot, Is.Null);
            Assert.That(actual.HasExtensions, Is.False);
        }

        [Test]
        public void Parse_SourceIsValidWithMissingHotspotY_ResultAsExpected()
        {
            String source = "  111\t222 333\t444 555";

            XpmHeader actual = XpmHeader.Parse(source);

            Assert.That(actual.Width, Is.EqualTo(111));
            Assert.That(actual.Height, Is.EqualTo(222));
            Assert.That(actual.NumberOfColors, Is.EqualTo(333));
            Assert.That(actual.CharactersPerPixel, Is.EqualTo(444));
            Assert.That(actual.Hotspot, Is.Null);
            Assert.That(actual.HasExtensions, Is.False);
        }

        [Test]
        public void Parse_SourceIsValidWithValidHotspot_ResultAsExpected()
        {
            String source = "  111\t222 333\t444 555\t  666";

            XpmHeader actual = XpmHeader.Parse(source);

            Assert.That(actual.Width, Is.EqualTo(111));
            Assert.That(actual.Height, Is.EqualTo(222));
            Assert.That(actual.NumberOfColors, Is.EqualTo(333));
            Assert.That(actual.CharactersPerPixel, Is.EqualTo(444));
            Assert.That(actual.Hotspot, Is.Not.Null);
            Assert.That(actual.Hotspot.Value.X, Is.EqualTo(555));
            Assert.That(actual.Hotspot.Value.Y, Is.EqualTo(666));
            Assert.That(actual.HasExtensions, Is.False);
        }

        [TestCase("xpmext", true)]
        [TestCase("XPMEXT", true)]
        [TestCase("XpmExt", true)]
        [TestCase("wrong", false)]
        public void Parse_SourceIsValidWithExtension_ResultAsExpected(String extension, Boolean expected)
        {
            String source = $"  111\t222 333\t444 555\t  666 \t {extension}";

            XpmHeader actual = XpmHeader.Parse(source);

            Assert.That(actual.Width, Is.EqualTo(111));
            Assert.That(actual.Height, Is.EqualTo(222));
            Assert.That(actual.NumberOfColors, Is.EqualTo(333));
            Assert.That(actual.CharactersPerPixel, Is.EqualTo(444));
            Assert.That(actual.Hotspot, Is.Not.Null);
            Assert.That(actual.Hotspot.Value.X, Is.EqualTo(555));
            Assert.That(actual.Hotspot.Value.Y, Is.EqualTo(666));
            Assert.That(actual.HasExtensions, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_SourceIsValidNoHotspotButExtensions_ResultAsExpected()
        {
            String source = $"  111\t222 333\t444 \t  \t XPMEXT more items";

            XpmHeader actual = XpmHeader.Parse(source);

            Assert.That(actual.Width, Is.EqualTo(111));
            Assert.That(actual.Height, Is.EqualTo(222));
            Assert.That(actual.NumberOfColors, Is.EqualTo(333));
            Assert.That(actual.CharactersPerPixel, Is.EqualTo(444));
            Assert.That(actual.Hotspot, Is.Null);
            Assert.That(actual.HasExtensions, Is.True);
        }

        [Test]
        public void Parse_SourceIsFullyValidWithMoreItems_ResultAsExpected()
        {
            String source = $"  111\t222 333\t444 555\t  666 \t XPMEXT more items";

            XpmHeader actual = XpmHeader.Parse(source);

            Assert.That(actual.Width, Is.EqualTo(111));
            Assert.That(actual.Height, Is.EqualTo(222));
            Assert.That(actual.NumberOfColors, Is.EqualTo(333));
            Assert.That(actual.CharactersPerPixel, Is.EqualTo(444));
            Assert.That(actual.Hotspot, Is.Not.Null);
            Assert.That(actual.Hotspot.Value.X, Is.EqualTo(555));
            Assert.That(actual.Hotspot.Value.Y, Is.EqualTo(666));
            Assert.That(actual.HasExtensions, Is.True);
        }

        #endregion

        #region ToString

        [Test]
        public void ToString_Minimal_ResultAsExpected()
        {
            var actual = this.CreateHeader();

            Assert.That(actual.ToString(), Is.EqualTo("10 8 3 1"));
        }

        [Test]
        public void ToString_OnlyHotspot_ResultAsExpected()
        {
            var actual = this.CreateHeader(hot: new Point(6, 7));

            Assert.That(actual.ToString(), Is.EqualTo("10 8 3 1 6 7"));
        }

        [Test]
        public void ToString_OnlyHasExtensions_ResultAsExpected()
        {
            var actual = this.CreateHeader(ext: true);

            Assert.That(actual.ToString(), Is.EqualTo("10 8 3 1 XPMEXT"));
        }

        [Test]
        public void ToString_AllValues_ResultAsExpected()
        {
            var actual = this.CreateHeader(hot: new Point(6, 7), ext: true);

            Assert.That(actual.ToString(), Is.EqualTo("10 8 3 1 6 7 XPMEXT"));
        }

        #endregion

        #region Helpers

        private XpmHeader CreateHeader(Int32 w = 10, Int32 h = 8, Int32 noc = 3, Int32 cpp = 1, Point? hot = null, Boolean ext = false)
        {
            XpmHeader header = (XpmHeader)Activator.CreateInstance(typeof(XpmHeader), true);

            PropertyInfo property;

            PropertyInfo[] properties = header.GetType().GetProperties();

            property = properties.First(x => x.Name == nameof(XpmHeader.Width));
            property.SetValue(header, w);

            property = properties.First(x => x.Name == nameof(XpmHeader.Height));
            property.SetValue(header, h);

            property = properties.First(x => x.Name == nameof(XpmHeader.NumberOfColors));
            property.SetValue(header, noc);

            property = properties.First(x => x.Name == nameof(XpmHeader.CharactersPerPixel));
            property.SetValue(header, cpp);

            property = properties.First(x => x.Name == nameof(XpmHeader.Hotspot));
            property.SetValue(header, hot);

            property = properties.First(x => x.Name == nameof(XpmHeader.HasExtensions));
            property.SetValue(header, ext);

            return header;
        }

        #endregion
    }
}
