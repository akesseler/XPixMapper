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

using Plexdata.Utilities.XPixMapper;
using System.Text;

namespace Plexdata.XPixMapper.GUI.Extensions
{
    internal static class BuilderExtension
    {
        #region Private Fields

        private static readonly String keyItemPool = "abcdefghijklmnopqrstuvwxyz";

        #endregion

        #region Public Methods

        public static String[] Build(this Image source)
        {
            return source.Build(XPixMapType.Default);
        }

        public static String[] Build(this Image source, XPixMapType type)
        {
            if (source is null)
            {
                return Array.Empty<String>();
            }

            if (type == XPixMapType.Default)
            {
                return source.Build(XPixMapType.Colored);
            }

            if (type == XPixMapType.BestFit)
            {
                return source.Build(XPixMapType.Colored | XPixMapType.Grayscale | XPixMapType.Monochrome);
            }

            Bitmap bitmap = new Bitmap(source);

            IDictionary<Color, String> colors = BuilderExtension.GetColorAssignments(bitmap);

            List<String> result = new List<String>();

            BuilderExtension.AddHeader(ref result, bitmap, colors);
            BuilderExtension.AddColors(ref result, colors, type);
            BuilderExtension.AddPixels(ref result, bitmap, colors);

            return result.ToArray();
        }

        #endregion

        #region Private Methods

        private static void AddHeader(ref List<String> result, Bitmap bitmap, IDictionary<Color, String> colors)
        {
            StringBuilder builder = new StringBuilder(128);

            builder.AppendFormat("{0} {1} {2} {3}", bitmap.Width, bitmap.Height, colors.Count, BuilderExtension.GetKeyLength(colors.Count));

            result.Add(builder.ToString().TrimEnd());
        }

        private static void AddColors(ref List<String> result, IDictionary<Color, String> colors, XPixMapType type)
        {
            foreach (KeyValuePair<Color, String> color in colors)
            {
                StringBuilder builder = new StringBuilder(128);

                builder.AppendFormat("{0} ", color.Value);

                if (type == XPixMapType.Default || type.HasFlag(XPixMapType.Colored))
                {
                    builder.AppendFormat("c #{0} ", color.Key.ToArgb().ToString("X8"));
                }

                if (type.HasFlag(XPixMapType.Grayscale))
                {
                    builder.AppendFormat("g #{0} ", color.Key.ToGrayscaled().ToArgb().ToString("X8"));
                }

                if (type.HasFlag(XPixMapType.Monochrome))
                {
                    builder.AppendFormat("m #{0} ", color.Key.ToMonochrome().ToArgb().ToString("X8"));
                }

                result.Add(builder.ToString().TrimEnd());
            }
        }

        private static void AddPixels(ref List<String> result, Bitmap bitmap, IDictionary<Color, String> colors)
        {
            for (Int32 y = 0; y < bitmap.Height; y++)
            {
                StringBuilder builder = new StringBuilder(128);

                for (Int32 x = 0; x < bitmap.Width; x++)
                {
                    builder.Append(colors[bitmap.GetPixel(x, y)]);
                }

                result.Add(builder.ToString());
            }
        }

        private static IDictionary<Color, String> GetColorAssignments(Bitmap bitmap)
        {
            Dictionary<Color, String> result = new Dictionary<Color, String>();

            List<Color> colors = BuilderExtension.GetUniquePixelColors(bitmap);

            List<String> keys = BuilderExtension.GetUniqueColorKeys(colors.Count);

            if (colors.Count != keys.Count)
            {
                throw new InvalidOperationException("The calculated number of colors does not match the number of color keys.");
            }

            for (Int32 index = 0; index < colors.Count; index++)
            {
                result.Add(colors[index], keys[index]);
            }

            return result;
        }

        private static List<Color> GetUniquePixelColors(Bitmap bitmap)
        {
            HashSet<Color> colors = new HashSet<Color>();

            for (Int32 y = 0; y < bitmap.Height; y++)
            {
                for (Int32 x = 0; x < bitmap.Width; x++)
                {
                    colors.Add(bitmap.GetPixel(x, y));
                }
            }

            return colors.ToList();
        }

        private static Int32 GetKeyLength(Int32 colorCount)
        {
            // "UInt32.MaxValue (0xFFFFFFFF)" is theoretically the maximum of possible colors for ARGB
            // colors (which, for example, could be put in a square bitmap with size 65,536 x 65,536).
            // But every list count is of type Int32, so it doesn't matter.

            // The length of the color key depends on the number of colors and the number of key items
            // in key item pool. Here an example: a^x=b
            //    => a: number of key pool items,
            //    => b: actual number of colors,
            //    => x: exponent indicating key length.
            // The challenge is to determine the exponent! And that's what's being done here.

            // Calculate the additional key length for the transition from one exponent to the next.
            Int32 additionalKeyLength = colorCount % BuilderExtension.keyItemPool.Length == 0 ? 0 : 1;

            return Convert.ToInt32(Math.Truncate(
                Math.Log(colorCount) / Math.Log(BuilderExtension.keyItemPool.Length) + additionalKeyLength
            ));
        }

        private static List<String> GetUniqueColorKeys(Int32 colorCount)
        {
            Int32 keyLength = BuilderExtension.GetKeyLength(colorCount);

            List<String> colorKeys = new List<String>();

            BuilderExtension.BuildAndAddColorKeys(ref colorKeys, String.Empty, keyLength, colorCount);

            return colorKeys;
        }

        private static void BuildAndAddColorKeys(ref List<String> keyNames, String baseKey, Int32 keyLength, Int32 colorCount)
        {
            for (Int32 index = 0; index < BuilderExtension.keyItemPool.Length && keyNames.Count < colorCount; index++)
            {
                String key = baseKey + BuilderExtension.keyItemPool[index];

                if (key.Length < keyLength)
                {
                    BuilderExtension.BuildAndAddColorKeys(ref keyNames, key, keyLength, colorCount);
                }
                else
                {
                    keyNames.Add(key);
                }
            }
        }

        private static Color ToGrayscaled(this Color color)
        {
            Double luminance = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);

            return Color.FromArgb(color.A, (Int32)luminance, (Int32)luminance, (Int32)luminance);
        }

        private static Color ToMonochrome(this Color color)
        {
            if (color.A < 0xFF)
            {
                return Color.White;
            }

            if (color.R + color.G + color.B <= 127 * 3)
            {
                return Color.Black;
            }

            return Color.White;
        }

        #endregion
    }
}
