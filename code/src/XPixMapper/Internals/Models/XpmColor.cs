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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Plexdata.Utilities.XPixMapper.Internals.Models
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal class XpmColor
    {
        #region Construction

        private XpmColor() { }

        #endregion

        #region Properties

        public String PixelKey { get; private set; } = null;

        public Color? Coloration { get; private set; } = null;

        public Color? Grayscaled { get; private set; } = null;

        public Color? Monochrome { get; private set; } = null;

        public String Symbolical { get; private set; } = null;

        #endregion

        #region Methods

        public static IDictionary<String, XpmColor> Parse(XpmHeader header, String[] source)
        {
            if (header is null)
            {
                throw new ArgumentNullException(nameof(header),
                    $"Parameter '{nameof(header)}' must not be null.");
            }

            if (header.CharactersPerPixel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(header),
                    $"Parameter '{nameof(header)}' does not contain expected number of characters per pixel.");
            }

            if (!source?.Any() ?? true)
            {
                throw new ArgumentNullException(nameof(source),
                    $"Parameter '{nameof(source)}' must not be null or empty.");
            }

            String[] colors = source.Take(header.NumberOfColors).ToArray();

            if (colors.Length < header.NumberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(source),
                    $"Parameter '{nameof(source)}' does not contain expected number of color lines. " +
                    $"Expected: '{header.NumberOfColors}', Actual: '{colors.Length}");
            }

            Dictionary<String, XpmColor> result = new Dictionary<String, XpmColor>();

            for (Int32 index = 0; index < colors.Length; index++)
            {
                XpmColor color = XpmColor.Parse(colors[index], index, header.CharactersPerPixel);

                if (result.ContainsKey(color.PixelKey))
                {
                    throw new FormatException($"Color definition line at '{index + 1}' is present twice.");
                }

                result.Add(color.PixelKey, color);
            }

            return result;
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder(128);

            builder.Append($"{this.PixelKey} ");

            if (this.Coloration.HasValue)
            {
                builder.Append($"c #{this.Coloration.Value.ToArgb():X8} ");
            }

            if (this.Monochrome.HasValue)
            {
                builder.Append($"m #{this.Monochrome.Value.ToArgb():X8} ");
            }

            if (this.Grayscaled.HasValue)
            {
                builder.Append($"g #{this.Grayscaled.Value.ToArgb():X8} ");
            }

            if (!String.IsNullOrWhiteSpace(this.Symbolical))
            {
                builder.Append($"s {this.Symbolical} ");
            }

            builder.Length--; // Cut off last space...

            return builder.ToString();
        }

        private static XpmColor Parse(String source, Int32 index, Int32 charactersPerPixel)
        {
            // s:       Represents a symbolic name and is therefore not a color.
            // c:       Is the color definition for colored image coloring.
            // m:       Is the color definition for monochrome image coloring.
            // g/g4:    Is the color definition for grayscaled image coloring.
            //
            // Therefore, each of those (except the symbolic name) represents a specific color
            // for a specific purpose. Furthermore, each color definition can consist either of
            // an RGB value (#XXXXXX), or an HSV value (%??????), or the name of a known color.
            // 
            // The symbolic name instead serves as color accessor at runtime, which in turn
            // requires a color definition table.
            // 
            // Finally, each color definition can consist of multiple key-color-pairs, each
            // of them with a specific purpose.
            // 
            // Additionally note, each "key-color-pair" must consist of two parts. Therefore,
            // modulo two of the total length must be zero!
            //
            // What is not supported at the moment?
            // * HSV color values.
            // * Colors with known names.
            // * Color table to resolve symbolic names.
            // * Any grayscaled coloring.

            if (String.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentOutOfRangeException(nameof(source),
                    $"Color definition line at '{index + 1}' must not be null, empty or white space.");
            }

            if (source.Length < charactersPerPixel)
            {
                throw new FormatException($"Color definition '{source}' at '{index + 1}' is less than minimal required.");
            }

            // The PixelKey (="chars") is represented by the leading number of Characters
            // Per Pixel, whether they are spaces or other whitespace characters.

            String pixelKey = source
                .Substring(0, charactersPerPixel);

            // The color definition is instead represented by the rest of the
            // current source line. However, an empty color definition is not
            // supported.

            String[] colors = source
                .Substring(charactersPerPixel, source.Length - charactersPerPixel)
                .Split()
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToArray();

            if (colors.Length < 1)
            {
                throw new FormatException($"Color definition '{source}' at '{index + 1}' does not contain any valid definition.");
            }

            if (colors.Length % 2 != 0)
            {
                throw new FormatException($"Color definition '{source}' at '{index + 1}' does not contain a processable color definition.");
            }

            XpmColor result = new XpmColor()
            {
                PixelKey = pixelKey
            };

            for (Int32 offset = 0; offset < colors.Length; offset += 2)
            {
                String label = colors[offset + 0];
                String color = colors[offset + 1];

                if (String.Equals("s", label, StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Symbolical = color;
                }
                else if (String.Equals("c", label, StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Coloration = XpmColor.GetColor(color, index);
                }
                else if (String.Equals("m", label, StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Monochrome = XpmColor.GetColor(color, index);
                }
                else if (String.Equals("g", label, StringComparison.InvariantCultureIgnoreCase) || String.Equals("g4", label, StringComparison.InvariantCultureIgnoreCase))
                {
                    // First come first serve...
                    if (!result.Grayscaled.HasValue) { result.Grayscaled = XpmColor.GetColor(color, index); }
                }
                else
                {
                    throw new NotSupportedException($"Color key '{label}' with color value '{color}' is not supported.");
                }
            }

            return result;
        }

        private static Color GetColor(String source, Int32 index)
        {
            String color = source;

            if (String.Equals("None", color, StringComparison.InvariantCultureIgnoreCase))
            {
                return Color.Transparent;
            }

            // Check #RGB color
            if (color.StartsWith("#"))
            {
                color = color.TrimStart('#');

                switch (color.Length)
                {
                    case 3:
                        color = $"FF{String.Join(String.Empty, color.Select(x => $"{x}{x}"))}";
                        break;
                    case 6:
                        color = $"FF{color}";
                        break;
                    case 8:
                        break;
                    default:
                        throw new FormatException($"RGB color value '#{color}' at '{index + 1}' is invalid.");
                }

                if (!Int32.TryParse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out Int32 result))
                {
                    throw new FormatException($"Unable to convert RGB value '{source}' at '{index + 1}' into a valid color.");
                }

                return Color.FromArgb(result);
            }

            // Check %HSV color
            if (color.StartsWith("%"))
            {
                // TODO: Implement HSV value support.
                // See here: https://en.wikipedia.org/wiki/X11_color_names#Numbered_variants
                // But unfortunately it isn't clear how to format those value within the xpm
                // color section!
                throw new NotSupportedException("The HSV color space is not supported.");
            }

            // Check for named color
            color = XpmColor.GetColorName(color);

            if (!XpmColor.namedColors.ContainsKey(color))
            {
                throw new NotSupportedException($"The color name '{source}' is not supported.");
            }

            return XpmColor.namedColors[color];
        }

        private static String GetColorName(String color)
        {
            StringBuilder result = new StringBuilder(128);

            foreach (Char current in color)
            {
                if (Char.IsLetter(current))
                {
                    result.Append(current);
                }
            }

            return result.ToString();
        }

        [ExcludeFromCodeCoverage]
        private String GetDebuggerDisplay()
        {
            return $"{nameof(this.PixelKey)}=\"{this.PixelKey}\", " +
                   $"{nameof(this.Coloration)}=\"{this.Coloration?.Name}\", " +
                   $"{nameof(this.Monochrome)}=\"{this.Monochrome?.Name}\", " +
                   $"{nameof(this.Grayscaled)}=\"{this.Grayscaled?.Name}\", " +
                   $"{nameof(this.Symbolical)}=\"{this.Symbolical}\"";
        }

        #endregion

        #region Fields

        // This list has been found on the Internet under URL https://en.wikipedia.org/wiki/X11_color_names, which 
        // is actually almost identical to the list of named Windows colors. But it is actually not guaranteed that 
        // all RGB values of each corresponding Windows colors is identical to the RGB values defined in this list.
        private static readonly IDictionary<String, Color> namedColors = new Dictionary<String, Color>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "AliceBlue",            Color.FromArgb(unchecked((Int32)0xFFF0F8FF)) },
            { "AntiqueWhite",         Color.FromArgb(unchecked((Int32)0xFFFAEBD7)) },
            { "Aqua",                 Color.FromArgb(unchecked((Int32)0xFF00FFFF)) },
            { "Aquamarine",           Color.FromArgb(unchecked((Int32)0xFF7FFFD4)) },
            { "Azure",                Color.FromArgb(unchecked((Int32)0xFFF0FFFF)) },
            { "Beige",                Color.FromArgb(unchecked((Int32)0xFFF5F5DC)) },
            { "Bisque",               Color.FromArgb(unchecked((Int32)0xFFFFE4C4)) },
            { "Black",                Color.FromArgb(unchecked((Int32)0xFF000000)) },
            { "BlanchedAlmond",       Color.FromArgb(unchecked((Int32)0xFFFFEBCD)) },
            { "Blue",                 Color.FromArgb(unchecked((Int32)0xFF0000FF)) },
            { "BlueViolet",           Color.FromArgb(unchecked((Int32)0xFF8A2BE2)) },
            { "Brown",                Color.FromArgb(unchecked((Int32)0xFFA52A2A)) },
            { "Burlywood",            Color.FromArgb(unchecked((Int32)0xFFDEB887)) },
            { "CadetBlue",            Color.FromArgb(unchecked((Int32)0xFF5F9EA0)) },
            { "Chartreuse",           Color.FromArgb(unchecked((Int32)0xFF7FFF00)) },
            { "Chocolate",            Color.FromArgb(unchecked((Int32)0xFFD2691E)) },
            { "Coral",                Color.FromArgb(unchecked((Int32)0xFFFF7F50)) },
            { "CornflowerBlue",       Color.FromArgb(unchecked((Int32)0xFF6495ED)) },
            { "Cornsilk",             Color.FromArgb(unchecked((Int32)0xFFFFF8DC)) },
            { "Crimson",              Color.FromArgb(unchecked((Int32)0xFFDC143C)) },
            { "Cyan",                 Color.FromArgb(unchecked((Int32)0xFF00FFFF)) },
            { "DarkBlue",             Color.FromArgb(unchecked((Int32)0xFF00008B)) },
            { "DarkCyan",             Color.FromArgb(unchecked((Int32)0xFF008B8B)) },
            { "DarkGoldenrod",        Color.FromArgb(unchecked((Int32)0xFFB8860B)) },
            { "DarkGray",             Color.FromArgb(unchecked((Int32)0xFFA9A9A9)) },
            { "DarkGreen",            Color.FromArgb(unchecked((Int32)0xFF006400)) },
            { "DarkKhaki",            Color.FromArgb(unchecked((Int32)0xFFBDB76B)) },
            { "DarkMagenta",          Color.FromArgb(unchecked((Int32)0xFF8B008B)) },
            { "DarkOliveGreen",       Color.FromArgb(unchecked((Int32)0xFF556B2F)) },
            { "DarkOrange",           Color.FromArgb(unchecked((Int32)0xFFFF8C00)) },
            { "DarkOrchid",           Color.FromArgb(unchecked((Int32)0xFF9932CC)) },
            { "DarkRed",              Color.FromArgb(unchecked((Int32)0xFF8B0000)) },
            { "DarkSalmon",           Color.FromArgb(unchecked((Int32)0xFFE9967A)) },
            { "DarkSeaGreen",         Color.FromArgb(unchecked((Int32)0xFF8FBC8F)) },
            { "DarkSlateBlue",        Color.FromArgb(unchecked((Int32)0xFF483D8B)) },
            { "DarkSlateGray",        Color.FromArgb(unchecked((Int32)0xFF2F4F4F)) },
            { "DarkTurquoise",        Color.FromArgb(unchecked((Int32)0xFF00CED1)) },
            { "DarkViolet",           Color.FromArgb(unchecked((Int32)0xFF9400D3)) },
            { "DeepPink",             Color.FromArgb(unchecked((Int32)0xFFFF1493)) },
            { "DeepSkyBlue",          Color.FromArgb(unchecked((Int32)0xFF00BFFF)) },
            { "DimGray",              Color.FromArgb(unchecked((Int32)0xFF696969)) },
            { "DodgerBlue",           Color.FromArgb(unchecked((Int32)0xFF1E90FF)) },
            { "Firebrick",            Color.FromArgb(unchecked((Int32)0xFFB22222)) },
            { "FloralWhite",          Color.FromArgb(unchecked((Int32)0xFFFFFAF0)) },
            { "ForestGreen",          Color.FromArgb(unchecked((Int32)0xFF228B22)) },
            { "Fuchsia",              Color.FromArgb(unchecked((Int32)0xFFFF00FF)) },
            { "Gainsboro*",           Color.FromArgb(unchecked((Int32)0xFFDCDCDC)) },
            { "GhostWhite",           Color.FromArgb(unchecked((Int32)0xFFF8F8FF)) },
            { "Gold",                 Color.FromArgb(unchecked((Int32)0xFFFFD700)) },
            { "Goldenrod",            Color.FromArgb(unchecked((Int32)0xFFDAA520)) },
            { "Gray",                 Color.FromArgb(unchecked((Int32)0xFFBEBEBE)) },
            { "WebGray",              Color.FromArgb(unchecked((Int32)0xFF808080)) },
            { "Green",                Color.FromArgb(unchecked((Int32)0xFF00FF00)) },
            { "WebGreen",             Color.FromArgb(unchecked((Int32)0xFF008000)) },
            { "GreenYellow",          Color.FromArgb(unchecked((Int32)0xFFADFF2F)) },
            { "Honeydew",             Color.FromArgb(unchecked((Int32)0xFFF0FFF0)) },
            { "HotPink",              Color.FromArgb(unchecked((Int32)0xFFFF69B4)) },
            { "IndianRed",            Color.FromArgb(unchecked((Int32)0xFFCD5C5C)) },
            { "Indigo",               Color.FromArgb(unchecked((Int32)0xFF4B0082)) },
            { "Ivory",                Color.FromArgb(unchecked((Int32)0xFFFFFFF0)) },
            { "Khaki",                Color.FromArgb(unchecked((Int32)0xFFF0E68C)) },
            { "Lavender",             Color.FromArgb(unchecked((Int32)0xFFE6E6FA)) },
            { "LavenderBlush",        Color.FromArgb(unchecked((Int32)0xFFFFF0F5)) },
            { "LawnGreen",            Color.FromArgb(unchecked((Int32)0xFF7CFC00)) },
            { "LemonChiffon",         Color.FromArgb(unchecked((Int32)0xFFFFFACD)) },
            { "LightBlue",            Color.FromArgb(unchecked((Int32)0xFFADD8E6)) },
            { "LightCoral",           Color.FromArgb(unchecked((Int32)0xFFF08080)) },
            { "LightCyan",            Color.FromArgb(unchecked((Int32)0xFFE0FFFF)) },
            { "LightGoldenrod",       Color.FromArgb(unchecked((Int32)0xFFFAFAD2)) },
            { "LightGoldenrodYellow", Color.FromArgb(unchecked((Int32)0xFFFAFAD2)) }, // Windows exception
            { "LightGray",            Color.FromArgb(unchecked((Int32)0xFFD3D3D3)) },
            { "LightGreen",           Color.FromArgb(unchecked((Int32)0xFF90EE90)) },
            { "LightPink",            Color.FromArgb(unchecked((Int32)0xFFFFB6C1)) },
            { "LightSalmon",          Color.FromArgb(unchecked((Int32)0xFFFFA07A)) },
            { "LightSeaGreen",        Color.FromArgb(unchecked((Int32)0xFF20B2AA)) },
            { "LightSkyBlue",         Color.FromArgb(unchecked((Int32)0xFF87CEFA)) },
            { "LightSlateGray",       Color.FromArgb(unchecked((Int32)0xFF778899)) },
            { "LightSteelBlue",       Color.FromArgb(unchecked((Int32)0xFFB0C4DE)) },
            { "LightYellow",          Color.FromArgb(unchecked((Int32)0xFFFFFFE0)) },
            { "Lime",                 Color.FromArgb(unchecked((Int32)0xFF00FF00)) },
            { "LimeGreen",            Color.FromArgb(unchecked((Int32)0xFF32CD32)) },
            { "Linen",                Color.FromArgb(unchecked((Int32)0xFFFAF0E6)) },
            { "Magenta",              Color.FromArgb(unchecked((Int32)0xFFFF00FF)) },
            { "Maroon",               Color.FromArgb(unchecked((Int32)0xFFB03060)) },
            { "WebMaroon",            Color.FromArgb(unchecked((Int32)0xFF800000)) },
            { "MediumAquamarine",     Color.FromArgb(unchecked((Int32)0xFF66CDAA)) },
            { "MediumBlue",           Color.FromArgb(unchecked((Int32)0xFF0000CD)) },
            { "MediumOrchid",         Color.FromArgb(unchecked((Int32)0xFFBA55D3)) },
            { "MediumPurple",         Color.FromArgb(unchecked((Int32)0xFF9370DB)) },
            { "MediumSeaGreen",       Color.FromArgb(unchecked((Int32)0xFF3CB371)) },
            { "MediumSlateBlue",      Color.FromArgb(unchecked((Int32)0xFF7B68EE)) },
            { "MediumSpringGreen",    Color.FromArgb(unchecked((Int32)0xFF00FA9A)) },
            { "MediumTurquoise",      Color.FromArgb(unchecked((Int32)0xFF48D1CC)) },
            { "MediumVioletRed",      Color.FromArgb(unchecked((Int32)0xFFC71585)) },
            { "MidnightBlue",         Color.FromArgb(unchecked((Int32)0xFF191970)) },
            { "MintCream",            Color.FromArgb(unchecked((Int32)0xFFF5FFFA)) },
            { "MistyRose",            Color.FromArgb(unchecked((Int32)0xFFFFE4E1)) },
            { "Moccasin",             Color.FromArgb(unchecked((Int32)0xFFFFE4B5)) },
            { "NavajoWhite",          Color.FromArgb(unchecked((Int32)0xFFFFDEAD)) },
            { "Navy",                 Color.FromArgb(unchecked((Int32)0xFF000080)) }, // Windows exception
            { "NavyBlue",             Color.FromArgb(unchecked((Int32)0xFF000080)) },
            { "OldLace",              Color.FromArgb(unchecked((Int32)0xFFFDF5E6)) },
            { "Olive",                Color.FromArgb(unchecked((Int32)0xFF808000)) },
            { "OliveDrab",            Color.FromArgb(unchecked((Int32)0xFF6B8E23)) },
            { "Orange",               Color.FromArgb(unchecked((Int32)0xFFFFA500)) },
            { "OrangeRed",            Color.FromArgb(unchecked((Int32)0xFFFF4500)) },
            { "Orchid",               Color.FromArgb(unchecked((Int32)0xFFDA70D6)) },
            { "PaleGoldenrod",        Color.FromArgb(unchecked((Int32)0xFFEEE8AA)) },
            { "PaleGreen",            Color.FromArgb(unchecked((Int32)0xFF98FB98)) },
            { "PaleTurquoise",        Color.FromArgb(unchecked((Int32)0xFFAFEEEE)) },
            { "PaleVioletRed",        Color.FromArgb(unchecked((Int32)0xFFDB7093)) },
            { "PapayaWhip",           Color.FromArgb(unchecked((Int32)0xFFFFEFD5)) },
            { "PeachPuff",            Color.FromArgb(unchecked((Int32)0xFFFFDAB9)) },
            { "Peru",                 Color.FromArgb(unchecked((Int32)0xFFCD853F)) },
            { "Pink",                 Color.FromArgb(unchecked((Int32)0xFFFFC0CB)) },
            { "Plum",                 Color.FromArgb(unchecked((Int32)0xFFDDA0DD)) },
            { "PowderBlue",           Color.FromArgb(unchecked((Int32)0xFFB0E0E6)) },
            { "Purple",               Color.FromArgb(unchecked((Int32)0xFFA020F0)) },
            { "WebPurple",            Color.FromArgb(unchecked((Int32)0xFF800080)) },
            { "RebeccaPurple",        Color.FromArgb(unchecked((Int32)0xFF663399)) },
            { "Red",                  Color.FromArgb(unchecked((Int32)0xFFFF0000)) },
            { "RosyBrown",            Color.FromArgb(unchecked((Int32)0xFFBC8F8F)) },
            { "RoyalBlue",            Color.FromArgb(unchecked((Int32)0xFF4169E1)) },
            { "SaddleBrown",          Color.FromArgb(unchecked((Int32)0xFF8B4513)) },
            { "Salmon",               Color.FromArgb(unchecked((Int32)0xFFFA8072)) },
            { "SandyBrown",           Color.FromArgb(unchecked((Int32)0xFFF4A460)) },
            { "SeaGreen",             Color.FromArgb(unchecked((Int32)0xFF2E8B57)) },
            { "Seashell",             Color.FromArgb(unchecked((Int32)0xFFFFF5EE)) },
            { "Sienna",               Color.FromArgb(unchecked((Int32)0xFFA0522D)) },
            { "Silver",               Color.FromArgb(unchecked((Int32)0xFFC0C0C0)) },
            { "SkyBlue",              Color.FromArgb(unchecked((Int32)0xFF87CEEB)) },
            { "SlateBlue",            Color.FromArgb(unchecked((Int32)0xFF6A5ACD)) },
            { "SlateGray",            Color.FromArgb(unchecked((Int32)0xFF708090)) },
            { "Snow",                 Color.FromArgb(unchecked((Int32)0xFFFFFAFA)) },
            { "SpringGreen",          Color.FromArgb(unchecked((Int32)0xFF00FF7F)) },
            { "SteelBlue",            Color.FromArgb(unchecked((Int32)0xFF4682B4)) },
            { "Tan",                  Color.FromArgb(unchecked((Int32)0xFFD2B48C)) },
            { "Teal",                 Color.FromArgb(unchecked((Int32)0xFF008080)) },
            { "Thistle",              Color.FromArgb(unchecked((Int32)0xFFD8BFD8)) },
            { "Tomato",               Color.FromArgb(unchecked((Int32)0xFFFF6347)) },
            { "Turquoise",            Color.FromArgb(unchecked((Int32)0xFF40E0D0)) },
            { "Violet",               Color.FromArgb(unchecked((Int32)0xFFEE82EE)) },
            { "Wheat",                Color.FromArgb(unchecked((Int32)0xFFF5DEB3)) },
            { "White",                Color.FromArgb(unchecked((Int32)0xFFFFFFFF)) },
            { "WhiteSmoke",           Color.FromArgb(unchecked((Int32)0xFFF5F5F5)) },
            { "Yellow",               Color.FromArgb(unchecked((Int32)0xFFFFFF00)) },
            { "YellowGreen",          Color.FromArgb(unchecked((Int32)0xFF9ACD32)) },
        };

        #endregion
    }
}
