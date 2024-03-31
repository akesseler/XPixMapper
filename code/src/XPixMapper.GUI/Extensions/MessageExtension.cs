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

#nullable disable

namespace Plexdata.XPixMapper.GUI.Extensions
{
    internal static class MessageExtension
    {
        #region Public Methods

        public static void ShowError(this Object source, String message)
        {
            source.ShowError(message, null, null);
        }

        public static void ShowError(this Object source, String message, Exception exception)
        {
            source.ShowError(message, null, exception);
        }

        public static void ShowError(this Object source, String message, String caption)
        {
            source.ShowError(message, caption, null);
        }

        public static void ShowError(this Object source, Exception exception)
        {
            source.ShowError(null, null, exception);
        }

        public static void ShowError(this Object source, Exception exception, String caption)
        {
            source.ShowError(null, caption, exception);
        }

        public static void ShowError(this Object source, String message, String caption, Exception exception)
        {
            IWin32Window parent = source.GetParent();

            caption = source.GetCaption(caption, "Error");

            message = source.GetMessage(message, exception);

            MessageBox.Show(parent, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(this Object source, String message)
        {
            source.ShowWarning(message, null);
        }

        public static void ShowWarning(this Object source, String message, String caption)
        {
            IWin32Window parent = source.GetParent();

            caption = source.GetCaption(caption, "Warning");

            message = source.GetMessage(message, null);

            MessageBox.Show(parent, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static Boolean ShowQuestion(this Object source, String message)
        {
            return source.ShowQuestion(message, null);
        }

        public static Boolean ShowQuestion(this Object source, String message, String caption)
        {
            IWin32Window parent = source.GetParent();

            caption = source.GetCaption(caption, "Question");

            message = source.GetMessage(message, null);

            return MessageBox.Show(parent, message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static void ShowMessage(this Object source, String message)
        {
            source.ShowMessage(message, null);
        }

        public static void ShowMessage(this Object source, String message, String caption)
        {
            IWin32Window parent = source.GetParent();

            caption = source.GetCaption(caption, "Message");

            message = source.GetMessage(message, null);

            MessageBox.Show(parent, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Private Methods

        private static IWin32Window GetParent(this Object source)
        {
            return source as IWin32Window;
        }

        private static String GetCaption(this Object source, String caption, String fallback)
        {
            if (!String.IsNullOrWhiteSpace(caption))
            {
                return caption;
            }

            if (source is Control control)
            {
                return control.Text;
            }

            return fallback;
        }

        private static String GetMessage(this Object _, String message, Exception exception)
        {
            String result = String.Format("{1}{0}{0}{2}", Environment.NewLine, message?.Trim(), exception?.ToString()).Trim();

            if (String.IsNullOrWhiteSpace(result))
            {
                result = "An unspecified issue has occurred.";
            }

            return result;
        }

        #endregion
    }
}

#nullable restore