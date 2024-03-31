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

using Plexdata.Utilities.XPixMapper.Internals.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Plexdata.Utilities.XPixMapper
{
    public static class XPixMapParser
    {
        #region Public Methods

        public static Image Parse(Stream source)
        {
            return XPixMapParser.Parse(source, XPixMapType.Default);
        }

        public static Image Parse(Stream source, XPixMapType type)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), $"Parameter '{nameof(source)}' must not be null.");
            }

            using (StreamReader reader = new StreamReader(source))
            {
                return XPixMapParser.Parse(reader.ReadToEnd(), type);
            }
        }

        public static Image Parse(String source)
        {
            return XPixMapParser.Parse(source, XPixMapType.Default);
        }

        public static Image Parse(String source, XPixMapType type)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentOutOfRangeException(nameof(source), $"Parameter '{nameof(source)}' must not be null, empty or white space.");
            }

            return XPixMapParser.Parse(source.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), type);
        }

        public static Image Parse(String[] source)
        {
            return XPixMapParser.Parse(source, XPixMapType.Default);
        }

        public static Image Parse(String[] source, XPixMapType type)
        {
            return XPixMap.Parse(source).Create(type);
        }

        private static Image Create(this XPixMap xpm, XPixMapType type)
        {
            Int32 w = xpm.Header.Width;
            Int32 h = xpm.Header.Height;

            Bitmap bitmap = new Bitmap(w, h);

            for (Int32 x = 0; x < w; x++)
            {
                for (Int32 y = 0; y < h; y++)
                {
                    Color color = xpm.GetColor(x, y, type);

                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }

        #endregion

        #region Private Methods

        private static Color GetColor(this XPixMap xpm, Int32 col, Int32 row, XPixMapType type)
        {
            String key = xpm.Pixels.Values[col, row];

            if (!xpm.Colors.TryGetValue(key, out XpmColor color))
            {
                throw new KeyNotFoundException($"Color key '{key}' not found.");
            }

            return color.GetColor(type);
        }

        private static Color GetColor(this XpmColor color, XPixMapType type)
        {
            if (type == XPixMapType.Default)
            {
                return color.GetColor(XPixMapType.Colored);
            }

            if (type == XPixMapType.BestFit)
            {
                return color.GetColor(XPixMapType.Colored | XPixMapType.Grayscale | XPixMapType.Monochrome);
            }

            if (type.HasFlag(XPixMapType.Colored) && color.Coloration.HasValue)
            {
                return color.Coloration.Value;
            }

            if (type.HasFlag(XPixMapType.Grayscale) && color.Grayscaled.HasValue)
            {
                return color.Grayscaled.Value;
            }

            if (type.HasFlag(XPixMapType.Monochrome) && color.Monochrome.HasValue)
            {
                return color.Monochrome.Value;
            }

            throw new NotSupportedException($"Color for type '{type}' is not supported.");
        }

        #endregion
    }
}
