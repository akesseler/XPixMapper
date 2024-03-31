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

namespace Plexdata.Utilities.XPixMapper.Tests.Internals.Models
{
    [ExcludeFromCodeCoverage]
    [Category(TestType.UnitTest)]
    public class XPixMapTests
    {
        [Test]
        public void XPixMap_DefaultPropertyCheck_PropertiesAsExpected()
        {
            XPixMap instance = (XPixMap)Activator.CreateInstance(typeof(XPixMap), true);

            Assert.That(instance.IsValid, Is.False);
            Assert.That(instance.Header, Is.Null);
            Assert.That(instance.Colors, Is.Null);
            Assert.That(instance.Pixels, Is.Null);
            Assert.That(instance.Extensions, Is.Null);
        }

        [Test]
        public void Parse_SourceIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => XPixMap.Parse(null), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null."));
        }

        [Test]
        public void Parse_SourceIsEmpty_ThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => XPixMap.Parse(Array.Empty<String>()), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' must consist of at least one line."));
        }

        [Test]
        public void Parse_SourceWithOneNullLine_ThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => XPixMap.Parse(new String[] { "line1", null, "line3" }), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' contains at least one null line."));
        }

        [TestCase("!")]
        [TestCase(" !")]
        [TestCase("!xyz")]
        [TestCase(" !xyz")]
        public void Parse_SourceWithFileHeaderOnly_ThrowsArgumentOutOfRangeException(String line)
        {
            Assert.That(() => XPixMap.Parse(new String[] { line }), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .And.Message.StartsWith("Parameter 'source' must consist of at least the header."));
        }

        [Test]
        public void Parse_SourceIsValid_ResultIsValid()
        {
            String[] source = new String[]
            {
                "!XPM",
                "6 6 3 1 2 4 XPMEXT",
                "  c None g White m White s sym1",
                "+ c Yellow g LightGray m White s sym2",
                "- c Green g DarkGray m Black s sym3",
                " +- +-",
                "+- +- ",
                " +- +-",
                "+- +- ",
                " +- +-",
                "+- +- ",
                "XPMEXT ext1 val1",
                "XPMEXT ext2",
                "val2,",
                "val3,",
                "XPMENDEXT"
            };

            XPixMap actual = XPixMap.Parse(source);

            Assert.That(actual.IsValid, Is.True);
            Assert.That(actual.Header, Is.Not.Null);
            Assert.That(actual.Colors, Is.Not.Null);
            Assert.That(actual.Pixels, Is.Not.Null);
            Assert.That(actual.Extensions, Is.Not.Null);
        }

        [TestCaseSource(nameof(XPixMapTests.ToStringTestCaseData))]
        public void ToString_SourceIsValid_ResultAsExpected(String[] source, String expected)
        {
            String actual = XPixMap.Parse(source).ToString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> ToStringTestCaseData()
        {
            yield return new TestCaseData
            (
                new String[]
                {
                    "6   6   3   1",
                    "  c   None     g White       m White   s    sym1",
                    "+ c   Yellow   g LightGray   m White   s    sym2",
                    "- c   Green    g DarkGray    m Black   s    sym3",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- "
                },
                "6 6 3 1\r\n" +
                "  c #00FFFFFF m #FFFFFFFF g #FFFFFFFF s sym1\r\n" +
                "+ c #FFFFFF00 m #FFFFFFFF g #FFD3D3D3 s sym2\r\n" +
                "- c #FF00FF00 m #FF000000 g #FFA9A9A9 s sym3\r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n"
            )
            .SetArgDisplayNames("Minimal XPixMap");

            yield return new TestCaseData
            (
                new String[]
                {
                    "6   6   3   1   5   3",
                    "  c none   g White     m White s sym1",
                    "+ c Yellow g LightGray m White s sym2",
                    "- c Green  g DarkGray  m Black s sym3",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- "
                },
                "6 6 3 1 5 3\r\n" +
                "  c #00FFFFFF m #FFFFFFFF g #FFFFFFFF s sym1\r\n" +
                "+ c #FFFFFF00 m #FFFFFFFF g #FFD3D3D3 s sym2\r\n" +
                "- c #FF00FF00 m #FF000000 g #FFA9A9A9 s sym3\r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n"
            )
            .SetArgDisplayNames("Hotspot XPixMap");

            yield return new TestCaseData
            (
                new String[]
                {
                    "!XPM",
                    "6   6   3   1   XPMEXT",
                    " c NONE   g White     \tm White s sym1",
                    "+c Yellow g lightgray \tm White s sym2",
                    "-c Green  g DARKGRAY  \tm Black s sym3",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- ",
                    "XPMEXT ext1 val1",
                    "XPMEXT ext2",
                    "val2",
                    "val3",
                    "XPMENDEXT"
                },
                "6 6 3 1 XPMEXT\r\n" +
                "  c #00FFFFFF m #FFFFFFFF g #FFFFFFFF s sym1\r\n" +
                "+ c #FFFFFF00 m #FFFFFFFF g #FFD3D3D3 s sym2\r\n" +
                "- c #FF00FF00 m #FF000000 g #FFA9A9A9 s sym3\r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                "XPMEXT ext1 val1\r\n" +
                "XPMEXT ext2\r\n" +
                "XPMEXT val2\r\n" +
                "XPMEXT val3\r\n" +
                "XPMENDEXT\r\n"
            )
            .SetArgDisplayNames("Extension XPixMap");

            yield return new TestCaseData
            (
                new String[]
                {
                    "!XPM",
                    "6 6 3 1 2 4 XPMEXT",
                    "  c None g White m White s sym1",
                    "+ c Yellow g LightGray m White s sym2",
                    "- c Green g DarkGray m Black s sym3",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- ",
                    " +- +-",
                    "+- +- ",
                    "XPMEXT ext1 val1",
                    "XPMEXT ext2",
                    "val2",
                    "val3",
                    "XPMENDEXT"
                },
                "6 6 3 1 2 4 XPMEXT\r\n" +
                "  c #00FFFFFF m #FFFFFFFF g #FFFFFFFF s sym1\r\n" +
                "+ c #FFFFFF00 m #FFFFFFFF g #FFD3D3D3 s sym2\r\n" +
                "- c #FF00FF00 m #FF000000 g #FFA9A9A9 s sym3\r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                " +- +-\r\n" +
                "+- +- \r\n" +
                "XPMEXT ext1 val1\r\n" +
                "XPMEXT ext2\r\n" +
                "XPMEXT val2\r\n" +
                "XPMEXT val3\r\n" +
                "XPMENDEXT\r\n"
            )
            .SetArgDisplayNames("Maximal XPixMap");
        }
    }
}
