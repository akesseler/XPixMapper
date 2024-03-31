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
    public class XpmColorTests
    {
        #region Parse

        [Test]
        public void Parse_XmpHeaderIsNull_ThrowsArgumentNullException()
        {
            XpmHeader header = null;
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'header' must not be null."));
        }

        [Test]
        public void Parse_NumberOfCharactersPerPixelIsInvalid_ThrowsArgumentOutOfRangeException()
        {
            XpmHeader header = this.CreateHeader(3, 0);
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'header' does not contain expected number of characters per pixel."));
        }

        [Test]
        public void Parse_ArrayIsNull_ThrowsArgumentNullException()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = null;

            Assert.That(() => XpmColor.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null or empty."));
        }

        [Test]
        public void Parse_ArrayIsEmpty_ThrowsArgumentNullException()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = Array.Empty<String>();

            Assert.That(() => XpmColor.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null or empty."));
        }

        [Test]
        public void Parse_NumberOfColorsLessThanExpected_ThrowsArgumentOutOfRangeException()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = new String[] { "xxx" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' does not contain expected number of color lines. Expected: '3', Actual: '1"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Parse_SecondColorDefinitionLineIsInvalid_ThrowsArgumentOutOfRangeException(String lineTwo)
        {
            XpmHeader header = this.CreateHeader();
            String[] source = new String[] { "x c #FFFFFF", lineTwo, "y c #000000" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Color definition line at '2' must not be null, empty or white space."));
        }

        [Test]
        public void Parse_SecondColorDefinitionLineIsLessThanRequired_ThrowsFormatException()
        {
            XpmHeader header = this.CreateHeader(3, 2);
            String[] source = new String[] { "x c #FFFFFF", "y", "z c #000000" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo("Color definition 'y' at '2' is less than minimal required."));
        }

        [TestCase("x ", null)]
        [TestCase(" x", "")]
        [TestCase("x\t", "   ")]
        [TestCase("\tx", "\t")]
        public void Parse_OneLineWithKeyButWithoutAnyValidColorDefinition_ThrowsFormatException(String key, String value)
        {
            XpmHeader header = this.CreateHeader(1, key.Length);
            String[] source = new String[] { $"{key}{value}" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo($"Color definition '{source[0]}' at '1' does not contain any valid definition."));
        }

        [TestCase("x ", "c  ")]
        [TestCase(" x", "  c xxx\tkkk ")]
        [TestCase("x\t", "c xxx\ts symb  \t m ")]
        [TestCase("\tx", "c xxx\ts symb  \t m yyy    g")]
        public void Parse_OneLineWithKeyButWithoutAnyProcessableColorDefinition_ThrowsFormatException(String key, String value)
        {
            XpmHeader header = this.CreateHeader(1, key.Length);
            String[] source = new String[] { $"{key}{value}" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo($"Color definition '{source[0]}' at '1' does not contain a processable color definition."));
        }

        [TestCase("s")]
        [TestCase("S")]
        public void Parse_OneColorDefinitionLineWithSymbolical_ResultAsExpected(String key)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} symbolic_name" };

            KeyValuePair<String, XpmColor> actual = XpmColor.Parse(header, source).First();

            Assert.That(actual.Key, Is.EqualTo("x"));
            Assert.That(actual.Value.PixelKey, Is.EqualTo("x"));
            Assert.That(actual.Value.Symbolical, Is.EqualTo("symbolic_name"));
            Assert.That(actual.Value.Coloration, Is.Null);
            Assert.That(actual.Value.Monochrome, Is.Null);
            Assert.That(actual.Value.Grayscaled, Is.Null);
        }

        [Test]
        public void Parse_OneColorDefinitionLineWithColorationForTransparentColor_ResultAsExpected(
            [Values("c", "C")] String key, [Values("none", "NONE", "None")] String transparent)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} {transparent}" };

            KeyValuePair<String, XpmColor> actual = XpmColor.Parse(header, source).First();

            Assert.That(actual.Key, Is.EqualTo("x"));
            Assert.That(actual.Value.PixelKey, Is.EqualTo("x"));
            Assert.That(actual.Value.Symbolical, Is.Null);
            Assert.That(actual.Value.Coloration, Is.EqualTo(Color.Transparent));
            Assert.That(actual.Value.Monochrome, Is.Null);
            Assert.That(actual.Value.Grayscaled, Is.Null);
        }

        [Test]
        public void Parse_OneColorDefinitionLineWithMonochromeForTransparentColor_ResultAsExpected(
            [Values("m", "M")] String key, [Values("none", "NONE", "None")] String transparent)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} {transparent}" };

            KeyValuePair<String, XpmColor> actual = XpmColor.Parse(header, source).First();

            Assert.That(actual.Key, Is.EqualTo("x"));
            Assert.That(actual.Value.PixelKey, Is.EqualTo("x"));
            Assert.That(actual.Value.Symbolical, Is.Null);
            Assert.That(actual.Value.Coloration, Is.Null);
            Assert.That(actual.Value.Monochrome, Is.EqualTo(Color.Transparent));
            Assert.That(actual.Value.Grayscaled, Is.Null);
        }

        [Test]
        public void Parse_OneColorDefinitionLineWithGrayscaleForTransparentColor_ResultAsExpected(
            [Values("g", "G", "g4", "G4")] String key, [Values("none", "NONE", "None")] String transparent)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} {transparent}" };

            KeyValuePair<String, XpmColor> actual = XpmColor.Parse(header, source).First();

            Assert.That(actual.Key, Is.EqualTo("x"));
            Assert.That(actual.Value.PixelKey, Is.EqualTo("x"));
            Assert.That(actual.Value.Symbolical, Is.Null);
            Assert.That(actual.Value.Coloration, Is.Null);
            Assert.That(actual.Value.Monochrome, Is.Null);
            Assert.That(actual.Value.Grayscaled, Is.EqualTo(Color.Transparent));
        }

        [TestCase("g", "F0F0F0", "g4", "C0C0C0", "fff0f0f0")]
        [TestCase("g4", "C0C0C0", "g", "F0F0F0", "ffc0c0c0")]
        public void Parse_OneColorDefinitionLineWithTwoGrayscalesForDifferentColors_ResultAsExpected(String g1, String c1, String g2, String c2, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {g1} #{c1} {g2} #{c2}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Grayscaled.Value.Name, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_OneColorDefinitionLineWithUnsupportedKey_ThrowsNotSupportedException()
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { "x K value" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<NotSupportedException>()
                .And.Message.EqualTo("Color key 'K' with color value 'value' is not supported."));
        }

        [Test]
        public void Parse_TwoColorDefinitionLineWithDuplicatePixelKeys_ThrowsFormatException()
        {
            XpmHeader header = this.CreateHeader(2, 1);
            String[] source = new String[] { "x s symbolic_name", "x c #FFFFFF" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo("Color definition line at '2' is present twice."));
        }

        [TestCase("abc", "ffaabbcc")]
        [TestCase("ABC", "ffaabbcc")]
        public void Parse_OneColorDefinitionLineForColorationWithThreeDigitRgbValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x c #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Coloration.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("abc", "ffaabbcc")]
        [TestCase("ABC", "ffaabbcc")]
        public void Parse_OneColorDefinitionLineForMonochromeWithThreeDigitRgbValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x m #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Monochrome.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("g", "abc", "ffaabbcc")]
        [TestCase("g4", "ABC", "ffaabbcc")]
        public void Parse_OneColorDefinitionLineForGrayscaledWithThreeDigitRgbValue_ResultAsExpected(String grayscale, String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {grayscale} #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Grayscaled.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("1278ef", "ff1278ef")]
        [TestCase("1278EF", "ff1278ef")]
        public void Parse_OneColorDefinitionLineForColorationWithSixDigitRgbValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x c #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Coloration.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("1278ef", "ff1278ef")]
        [TestCase("1278EF", "ff1278ef")]
        public void Parse_OneColorDefinitionLineForMonochromeWithSixDigitRgbValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x m #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Monochrome.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("g", "1278ef", "ff1278ef")]
        [TestCase("g4", "1278EF", "ff1278ef")]
        public void Parse_OneColorDefinitionLineForGrayscaledWithSixDigitRgbValue_ResultAsExpected(String grayscale, String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {grayscale} #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Grayscaled.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("f01278ef", "f01278ef")]
        [TestCase("f01278EF", "f01278ef")]
        public void Parse_OneColorDefinitionLineForColorationWithEightDigitRgbValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x c #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Coloration.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("f01278ef", "f01278ef")]
        [TestCase("f01278EF", "f01278ef")]
        public void Parse_OneColorDefinitionLineForMonochromeWithEightDigitRgbValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x m #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Monochrome.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("g", "f01278ef", "f01278ef")]
        [TestCase("g4", "f01278EF", "f01278ef")]
        public void Parse_OneColorDefinitionLineForGrayscaledWithEightDigitRgbValue_ResultAsExpected(String grayscale, String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {grayscale} #{color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Grayscaled.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("c", "1")]
        [TestCase("c", "12")]
        [TestCase("c", "1234")]
        [TestCase("c", "12345")]
        [TestCase("c", "1234567")]
        [TestCase("c", "123456789")]
        [TestCase("m", "1")]
        [TestCase("m", "12")]
        [TestCase("m", "1234")]
        [TestCase("m", "12345")]
        [TestCase("m", "1234567")]
        [TestCase("m", "123456789")]
        [TestCase("g", "1")]
        [TestCase("g", "12")]
        [TestCase("g", "1234")]
        [TestCase("g", "12345")]
        [TestCase("g", "1234567")]
        [TestCase("g", "123456789")]
        [TestCase("g4", "1")]
        [TestCase("g4", "12")]
        [TestCase("g4", "1234")]
        [TestCase("g4", "12345")]
        [TestCase("g4", "1234567")]
        [TestCase("g4", "123456789")]
        public void Parse_OneColorDefinitionLineWithInvalidDigitRgbValueLength_ThrowsFormatException(String key, String color)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} #{color}" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo($"RGB color value '#{color}' at '1' is invalid."));
        }

        [TestCase("c", "xyz")]
        [TestCase("m", "xyz")]
        [TestCase("g", "xyz")]
        [TestCase("g4", "xyz")]
        public void Parse_OneColorDefinitionLineWithInvalidDigitRgbValue_ThrowsFormatException(String key, String color)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} #{color}" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<FormatException>()
                .And.Message.EqualTo($"Unable to convert RGB value '#{color}' at '1' into a valid color."));
        }

        [TestCase("c")]
        [TestCase("m")]
        [TestCase("g")]
        [TestCase("g4")]
        public void Parse_OneColorDefinitionLineWithHsvValue_ThrowsNotSupportedException(String key)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} %hsv" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<NotSupportedException>()
                .And.Message.EqualTo("The HSV color space is not supported."));
        }

        [TestCase("c")]
        [TestCase("m")]
        [TestCase("g")]
        [TestCase("g4")]
        public void Parse_OneColorDefinitionLineWithInvalidNamedColorValue_ThrowsNotSupportedException(String key)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {key} invalid-Color_NAME" };

            Assert.That(() => XpmColor.Parse(header, source), Throws.InstanceOf<NotSupportedException>()
                .And.Message.EqualTo("The color name 'invalid-Color_NAME' is not supported."));
        }

        [TestCase("AliceBlue", "fff0f8ff")]
        [TestCase("AliceBlue", "fff0f8ff")]
        public void Parse_OneColorDefinitionLineForColorationWithValidNamedColorValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x c {color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Coloration.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("AliceBlue", "fff0f8ff")]
        [TestCase("AliceBlue", "fff0f8ff")]
        public void Parse_OneColorDefinitionLineForMonochromeWithValidNamedColorValue_ResultAsExpected(String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x m {color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Monochrome.Value.Name, Is.EqualTo(expected));
        }

        [TestCase("g", "AliceBlue", "fff0f8ff")]
        [TestCase("g4", "AliceBlue", "fff0f8ff")]
        public void Parse_OneColorDefinitionLineForGrayscaledWithValidNamedColorValue_ResultAsExpected(String grayscale, String color, String expected)
        {
            XpmHeader header = this.CreateHeader(1, 1);
            String[] source = new String[] { $"x {grayscale} {color}" };

            XpmColor actual = XpmColor.Parse(header, source).First().Value;

            Assert.That(actual.Grayscaled.Value.Name, Is.EqualTo(expected));
        }

        #endregion

        #region ToString

        [Test]
        public void ToString_NoColors_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor();

            Assert.That(actual.ToString(), Is.EqualTo("XXX"));
        }

        [Test]
        public void ToString_OnlyColoration_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(c: Color.FromArgb(0x12345678));

            Assert.That(actual.ToString(), Is.EqualTo("XXX c #12345678"));
        }

        [Test]
        public void ToString_OnlyMonochrome_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(m: Color.FromArgb(0x23456789));

            Assert.That(actual.ToString(), Is.EqualTo("XXX m #23456789"));
        }

        [Test]
        public void ToString_OnlyGrayscaled_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(g: Color.FromArgb(0x34567890));

            Assert.That(actual.ToString(), Is.EqualTo("XXX g #34567890"));
        }

        [Test]
        public void ToString_OnlySymbolical_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(s: "some-symbolic-name");

            Assert.That(actual.ToString(), Is.EqualTo("XXX s some-symbolic-name"));
        }

        [Test]
        public void ToString_ColorationPlusGrayscaled_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(c: Color.FromArgb(0x12345678), g: Color.FromArgb(0x34567890));

            Assert.That(actual.ToString(), Is.EqualTo("XXX c #12345678 g #34567890"));
        }

        [Test]
        public void ToString_MonochromePlusGrayscaled_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(m: Color.FromArgb(0x23456789), g: Color.FromArgb(0x34567890));

            Assert.That(actual.ToString(), Is.EqualTo("XXX m #23456789 g #34567890"));
        }

        [Test]
        public void ToString_ColorationPlusSymbolical_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(c: Color.FromArgb(0x12345678), s: "some-symbolic-name");

            Assert.That(actual.ToString(), Is.EqualTo("XXX c #12345678 s some-symbolic-name"));
        }

        [Test]
        public void ToString_AllValues_ResultAsExpected()
        {
            XpmColor actual = this.CreateColor(c: Color.FromArgb(0x12345678), m: Color.FromArgb(0x23456789), g: Color.FromArgb(0x34567890), s: "some-symbolic-name");

            Assert.That(actual.ToString(), Is.EqualTo("XXX c #12345678 m #23456789 g #34567890 s some-symbolic-name"));
        }

        #endregion

        #region Helpers

        private XpmHeader CreateHeader(Int32 noc = 3, Int32 cpp = 1)
        {
            XpmHeader result = (XpmHeader)Activator.CreateInstance(typeof(XpmHeader), true);

            PropertyInfo property;

            PropertyInfo[] properties = result.GetType().GetProperties();

            property = properties.First(x => x.Name == nameof(XpmHeader.Width));
            property.SetValue(result, 10);

            property = properties.First(x => x.Name == nameof(XpmHeader.Height));
            property.SetValue(result, 10);

            property = properties.First(x => x.Name == nameof(XpmHeader.NumberOfColors));
            property.SetValue(result, noc);

            property = properties.First(x => x.Name == nameof(XpmHeader.CharactersPerPixel));
            property.SetValue(result, cpp);

            property = properties.First(x => x.Name == nameof(XpmHeader.Hotspot));
            property.SetValue(result, new Point(1, 1));

            property = properties.First(x => x.Name == nameof(XpmHeader.HasExtensions));
            property.SetValue(result, true);

            return result;
        }

        private XpmColor CreateColor(Color? c = null, Color? m = null, Color? g = null, String s = null)
        {
            XpmColor result = (XpmColor)Activator.CreateInstance(typeof(XpmColor), true);

            PropertyInfo property;

            PropertyInfo[] properties = result.GetType().GetProperties();

            property = properties.First(x => x.Name == nameof(XpmColor.PixelKey));
            property.SetValue(result, "XXX");

            property = properties.First(x => x.Name == nameof(XpmColor.Coloration));
            property.SetValue(result, c);

            property = properties.First(x => x.Name == nameof(XpmColor.Monochrome));
            property.SetValue(result, m);

            property = properties.First(x => x.Name == nameof(XpmColor.Grayscaled));
            property.SetValue(result, g);

            property = properties.First(x => x.Name == nameof(XpmColor.Symbolical));
            property.SetValue(result, s);

            return result;
        }

        #endregion
    }
}
