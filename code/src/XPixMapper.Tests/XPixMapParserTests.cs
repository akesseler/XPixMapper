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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Plexdata.Utilities.XPixMapper.Tests
{
    [ExcludeFromCodeCoverage]
    [Category(TestType.IntegrationTest)]
    public class XPixMapParserTests
    {
        private String[] testSourceArray;

        [SetUp]
        public void SetUp()
        {
            this.testSourceArray = new String[]
            {
                "  !XPM",
                "10 10 3 1",
                "  C NONE",
                "+ C #FFF",
                "- C #000",
                " +- +- +- ",
                "+- +- +- +",
                "- +- +- +-",
                " +- +- +- ",
                "+- +- +- +",
                "- +- +- +-",
                " +- +- +- ",
                "+- +- +- +",
                "- +- +- +-",
                "----------"
            };
        }

        #region Source As Stream

        [Test]
        public void Parse_StreamOnlyStreamIsNull_ThrowsArgumentNullException()
        {
            Stream source = null;
            Assert.That(() => XPixMapParser.Parse(source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null."));
        }

        [Test]
        public void Parse_StreamOnlyStreamIsEmpty_ThrowsArgumentOutOfRangeException()
        {
            using (MemoryStream source = new MemoryStream())
            {
                Assert.That(() => XPixMapParser.Parse(source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                    .And.Message.StartsWith("Parameter 'source' must not be null, empty or white space."));
            }
        }

        [Test]
        public void Parse_StreamOnlyStreamIsValid_ThrowsNothing()
        {
            using (MemoryStream source = new MemoryStream(Encoding.UTF8.GetBytes(this.GetSourceAsString(this.testSourceArray))))
            {
                Assert.That(() => XPixMapParser.Parse(source), Throws.Nothing);
            }
        }

        [Test]
        public void Parse_StreamAndTypeStreamIsNull_ThrowsArgumentNullException()
        {
            Stream source = null;
            Assert.That(() => XPixMapParser.Parse(source, XPixMapType.BestFit), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null."));
        }

        [Test]
        public void Parse_StreamAndTypeStreamIsEmpty_ThrowsArgumentOutOfRangeException()
        {
            using (MemoryStream source = new MemoryStream())
            {
                Assert.That(() => XPixMapParser.Parse(source, XPixMapType.BestFit), Throws.InstanceOf<ArgumentOutOfRangeException>()
                    .And.Message.StartsWith("Parameter 'source' must not be null, empty or white space."));
            }
        }

        [Test]
        public void Parse_StreamAndTypeStreamIsValid_ThrowsNothing()
        {
            using (MemoryStream source = new MemoryStream(Encoding.UTF8.GetBytes(this.GetSourceAsString(this.testSourceArray))))
            {
                Assert.That(() => XPixMapParser.Parse(source, XPixMapType.BestFit), Throws.Nothing);
            }
        }

        [Test]
        public void Parse_StreamAndTypeTypeIsInvalid_ThrowsNotSupportedException()
        {
            using (MemoryStream source = new MemoryStream(Encoding.UTF8.GetBytes(this.GetSourceAsString(this.testSourceArray))))
            {
                Assert.That(() => XPixMapParser.Parse(source, unchecked((XPixMapType)0x00000008)), Throws.InstanceOf<NotSupportedException>()
                    .And.Message.EqualTo("Color for type '8' is not supported."));
            }
        }

        #endregion

        #region Source As String

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Parse_StringOnlyStringIsInvalid_ThrowsArgumentOutOfRangeException(String source)
        {
            Assert.That(() => XPixMapParser.Parse(source), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' must not be null, empty or white space."));
        }

        [Test]
        public void Parse_StringOnlyStringIsValid_ThrowsNothing()
        {
            String source = this.GetSourceAsString(this.testSourceArray);

            Assert.That(() => XPixMapParser.Parse(source), Throws.Nothing);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Parse_StringAndTypeStringIsInvalid_ThrowsArgumentOutOfRangeException(String source)
        {
            Assert.That(() => XPixMapParser.Parse(source, XPixMapType.BestFit), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' must not be null, empty or white space."));
        }

        [Test]
        public void Parse_StringAndTypeStringIsValid_ThrowsNothing()
        {
            String source = this.GetSourceAsString(this.testSourceArray);

            Assert.That(() => XPixMapParser.Parse(source, XPixMapType.BestFit), Throws.Nothing);
        }

        [Test]
        public void Parse_StringAndTypeTypeIsInvalid_ThrowsNotSupportedException()
        {
            String source = this.GetSourceAsString(this.testSourceArray);

            Assert.That(() => XPixMapParser.Parse(source, unchecked((XPixMapType)0x00000008)), Throws.InstanceOf<NotSupportedException>()
                .And.Message.EqualTo("Color for type '8' is not supported."));
        }

        #endregion

        #region Source As Array

        [Test]
        public void Parse_ArrayOnlyArrayIsValid_ThrowsNothing()
        {
            Assert.That(() => XPixMapParser.Parse(this.testSourceArray), Throws.Nothing);
        }

        [Test]
        public void Parse_ArrayAndTypeArrayIsValid_ThrowsNothing()
        {
            Assert.That(() => XPixMapParser.Parse(this.testSourceArray, XPixMapType.BestFit), Throws.Nothing);
        }

        [Test]
        public void Parse_ArrayAndTypeTypeIsInvalid_ThrowsNotSupportedException()
        {
            Assert.That(() => XPixMapParser.Parse(this.testSourceArray, unchecked((XPixMapType)0x00000008)), Throws.InstanceOf<NotSupportedException>()
                .And.Message.EqualTo("Color for type '8' is not supported."));
        }

        #endregion

        #region Pixel Validation

        [Test]
        public void Parse_SourceArrayWithInvalidPixel_ThrowsKeyNotFoundException()
        {
            this.testSourceArray[14] = "----F-----";

            Assert.That(() => XPixMapParser.Parse(this.testSourceArray), Throws.InstanceOf<KeyNotFoundException>()
                .And.Message.EqualTo("Color key 'F' not found."));
        }

        [Test]
        public void Parse_SourceArrayWithTypeDefault_PixelColorsAsExpected()
        {
            this.testSourceArray = new String[]
            {
                "2 2 2 1",
                "+ c #f00 g #aaa m #000",
                "- c #00f g #bbb m #fff",
                "++",
                "--"
            };

            Bitmap actual = (Bitmap)XPixMapParser.Parse(this.testSourceArray, XPixMapType.Default);

            Assert.That(actual.GetPixel(0, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFF0000))));
            Assert.That(actual.GetPixel(1, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFF0000))));
            Assert.That(actual.GetPixel(0, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF0000FF))));
            Assert.That(actual.GetPixel(1, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF0000FF))));
        }

        [Test]
        public void Parse_SourceArrayWithTypeBestFit_PixelColorsAsExpected()
        {
            this.testSourceArray = new String[]
            {
                "2 2 2 1",
                "+ c #f00 g #aaa m #000",
                "- c #00f g #bbb m #fff",
                "++",
                "--"
            };

            Bitmap actual = (Bitmap)XPixMapParser.Parse(this.testSourceArray, XPixMapType.BestFit);

            Assert.That(actual.GetPixel(0, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFF0000))));
            Assert.That(actual.GetPixel(1, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFF0000))));
            Assert.That(actual.GetPixel(0, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF0000FF))));
            Assert.That(actual.GetPixel(1, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF0000FF))));
        }

        [Test]
        public void Parse_SourceArrayWithTypeColored_PixelColorsAsExpected()
        {
            this.testSourceArray = new String[]
            {
                "2 2 2 1",
                "+ c #f00 g #aaa m #000",
                "- c #00f g #bbb m #fff",
                "++",
                "--"
            };

            Bitmap actual = (Bitmap)XPixMapParser.Parse(this.testSourceArray, XPixMapType.Colored);

            Assert.That(actual.GetPixel(0, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFF0000))));
            Assert.That(actual.GetPixel(1, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFF0000))));
            Assert.That(actual.GetPixel(0, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF0000FF))));
            Assert.That(actual.GetPixel(1, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF0000FF))));
        }

        [Test]
        public void Parse_SourceArrayWithTypeGrayscale_PixelColorsAsExpected()
        {
            this.testSourceArray = new String[]
            {
                "2 2 2 1",
                "+ c #f00 g #aaa m #000",
                "- c #00f g #bbb m #fff",
                "++",
                "--"
            };

            Bitmap actual = (Bitmap)XPixMapParser.Parse(this.testSourceArray, XPixMapType.Grayscale);

            Assert.That(actual.GetPixel(0, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFAAAAAA))));
            Assert.That(actual.GetPixel(1, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFAAAAAA))));
            Assert.That(actual.GetPixel(0, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFBBBBBB))));
            Assert.That(actual.GetPixel(1, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFBBBBBB))));
        }

        [Test]
        public void Parse_SourceArrayWithTypeMonochrome_PixelColorsAsExpected()
        {
            this.testSourceArray = new String[]
            {
                "2 2 2 1",
                "+ c #f00 g #aaa m #000",
                "- c #00f g #bbb m #fff",
                "++",
                "--"
            };

            Bitmap actual = (Bitmap)XPixMapParser.Parse(this.testSourceArray, XPixMapType.Monochrome);

            Assert.That(actual.GetPixel(0, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF000000))));
            Assert.That(actual.GetPixel(1, 0), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFF000000))));
            Assert.That(actual.GetPixel(0, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFFFFFF))));
            Assert.That(actual.GetPixel(1, 1), Is.EqualTo(Color.FromArgb(unchecked((Int32)0xFFFFFFFF))));
        }

        #endregion

        private String GetSourceAsString(String[] source)
        {
            StringBuilder builder = new StringBuilder();

            for (Int32 line = 0; line < source.Length; line++)
            {
                builder.Append(source[line]);

                if (line > 0 && line % 4 == 0)
                {
                    builder.Append("\n\r");
                }
                else if (line > 0 && line % 3 == 0)
                {
                    builder.Append("\r\n");
                }
                else if (line > 0 && line % 2 == 0)
                {
                    builder.Append('\n');
                }
                else
                {
                    builder.Append('\r');
                }
            }

            return builder.ToString();
        }
    }
}
