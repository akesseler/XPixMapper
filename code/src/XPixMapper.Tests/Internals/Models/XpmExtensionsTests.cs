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
using System.Reflection;

namespace Plexdata.Utilities.XPixMapper.Tests.Internals.Models
{
    [ExcludeFromCodeCoverage]
    [Category(TestType.UnitTest)]
    public class XpmExtensionsTests
    {
        #region Parse

        [Test]
        public void Parse_HeaderIsNull_ThrowsArgumentNullException()
        {
            XpmHeader header = null;
            String[] source = new String[] { "name1 value1" };

            Assert.That(() => XpmExtensions.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'header' must not be null."));
        }

        [Test]
        public void Parse_HasExtensionsIsFalse_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader(ext: false);
            String[] source = new String[] { "name1 value1" };

            XpmExtensions actual = XpmExtensions.Parse(header, source);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Lines, Is.Empty);
        }

        [Test]
        public void Parse_SourceIsNull_ThrowsArgumentNullException()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = null;

            Assert.That(() => XpmExtensions.Parse(header, source), Throws.ArgumentNullException
                .And.Message.StartsWith("Parameter 'source' must not be null."));
        }

        [Test]
        public void Parse_SourceIsEmpty_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = Array.Empty<String>();

            XpmExtensions actual = XpmExtensions.Parse(header, source);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Lines, Is.Empty);
        }

        [Test]
        public void Parse_SourceWithInvalidData_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = new String[] { "name1 value1", "name2 value2" };
            String[] expected = new String[] { "name1 value1", "name2 value2" };

            XpmExtensions actual = XpmExtensions.Parse(header, source);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Lines, Is.Not.Empty);
            Assert.That(actual.Lines.SequenceEqual(expected), Is.True);
        }

        [Test]
        public void Parse_SourceWithValidData_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = new String[]
            {
                "xpmext name1 value1",
                "XPMEXT name2 value2",
                "XpmExt name3 value3",
                "XPMEXT group1",
                "group1 value1",
                "group1 value2",
                "XpmExt group2",
                "group2 value1",
                "group2 value2",
                "XpmEndExt",
                "other data"
            };

            String[] expected = new String[]
            {
                "name1 value1",
                "name2 value2",
                "name3 value3",
                "group1",
                "group1 value1",
                "group1 value2",
                "group2",
                "group2 value1",
                "group2 value2"
            };

            XpmExtensions actual = XpmExtensions.Parse(header, source);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Lines, Is.Not.Empty);
            Assert.That(actual.Lines.SequenceEqual(expected), Is.True);
        }

        #endregion

        #region ToString

        [Test]
        public void ToString_NoLines_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = Array.Empty<String>();

            XpmExtensions actual = XpmExtensions.Parse(header, source);

            Assert.That(actual.ToString(), Is.Empty);
        }

        [Test]
        public void ToString_SomeLines_ResultAsExpected()
        {
            XpmHeader header = this.CreateHeader();
            String[] source = new String[]
            {
                "xpmext name1 value1",
                "XPMEXT name2 value2",
                "XpmExt name3 value3",
                "XPMEXT group1",
                "group1 value1",
                "group1 value2",
                "XpmExt group2",
                "group2 value1",
                "group2 value2",
                "XpmEndExt",
                "other data"
            };

            String expected = $"XPMEXT name1 value1{Environment.NewLine}" +
                              $"XPMEXT name2 value2{Environment.NewLine}" +
                              $"XPMEXT name3 value3{Environment.NewLine}" +
                              $"XPMEXT group1{Environment.NewLine}" +
                              $"XPMEXT group1 value1{Environment.NewLine}" +
                              $"XPMEXT group1 value2{Environment.NewLine}" +
                              $"XPMEXT group2{Environment.NewLine}" +
                              $"XPMEXT group2 value1{Environment.NewLine}" +
                              $"XPMEXT group2 value2{Environment.NewLine}" +
                              "XPMENDEXT";

            XpmExtensions actual = XpmExtensions.Parse(header, source);

            Assert.That(actual.ToString().SequenceEqual(expected), Is.True);
        }

        #endregion

        #region Helpers

        private XpmHeader CreateHeader(Boolean ext = true)
        {
            XpmHeader result = (XpmHeader)Activator.CreateInstance(typeof(XpmHeader), true);

            PropertyInfo property;

            PropertyInfo[] properties = result.GetType().GetProperties();

            property = properties.First(x => x.Name == nameof(XpmHeader.HasExtensions));
            property.SetValue(result, ext);

            return result;
        }

        #endregion
    }
}
