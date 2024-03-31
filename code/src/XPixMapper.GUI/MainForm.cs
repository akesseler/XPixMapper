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

using Plexdata.Utilities.XPixMapper;
using Plexdata.XPixMapper.GUI.Controls;
using Plexdata.XPixMapper.GUI.Dialogs;
using Plexdata.XPixMapper.GUI.Extensions;
using Plexdata.XPixMapper.GUI.Helpers;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace Plexdata.XPixMapper.GUI
{
    public partial class MainForm : Form
    {
        #region Example
#if DEBUG
        private static readonly String[] example = new String[]
        {
            "16 16 6 1",
            ". c None",
            "# c Black",
            "- c LightBlue",
            "* c Yellow",
            ": c DarkGreen",
            "; c Khaki",
            "................",
            "................",
            "................",
            ".##############.",
            "#--------------#",
            "#--------#-----#",
            "#-------#*#----#",
            "#----#---#-----#",
            "#---#:#-----#--#",
            "#--#:::#---#;#-#",
            "#-#:::::#-#;;;##",
            "##:::::::#;;;;;#",
            ".##############.",
            "................",
            "................",
            "................"
        };
#endif
        #endregion

        #region Construction

        public MainForm()
            : base()
        {
            this.InitializeComponent();
            base.Icon = Properties.Resources.XPixMapper;
        }

        #endregion

        #region Messages

        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);

            this.btLeftToRight.ContextMenuStrip = RadioMenuStrip.Create();
            this.btRightToLeft.ContextMenuStrip = CheckMenuStrip.Create();
            this.txLinesEdit.ContextMenuStrip = new ContextMenuStrip();
            this.pbImageView.ContextMenuStrip = new ContextMenuStrip();

            this.txLinesEdit.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Open",        null, this.OnExecuteLinesOpen),
                new ToolStripMenuItem("Save",        null, this.OnExecuteLinesSave),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Copy",        null, this.OnExecuteLinesCopy),
                new ToolStripMenuItem("Copy As",     null, new ToolStripItem[]
                {
                    new ToolStripMenuItem("C#",      null,this.OnExecuteLinesCopyAs),
                    new ToolStripMenuItem("C/C++",   null,this.OnExecuteLinesCopyAs),
                    new ToolStripMenuItem("VB",      null,this.OnExecuteLinesCopyAs),
                }),
                new ToolStripMenuItem("Cut",         null, this.OnExecuteLinesCut),
                new ToolStripMenuItem("Paste",       null, this.OnExecuteLinesPaste),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Undo",        null, this.OnExecuteLinesUndo),
            });

            this.pbImageView.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Open",        null, this.OnExecuteImageOpen),
                new ToolStripMenuItem("Save",        null, this.OnExecuteImageSave),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Copy",        null, this.OnExecuteImageCopy),
                new ToolStripMenuItem("Copy As",     null, new ToolStripItem[]
                {
                    new ToolStripMenuItem("BASE 64", null,this.OnExecuteImageCopyAs) { Tag = ImageView.ImageType.B64 },
                    new ToolStripMenuItem("PNG",     null,this.OnExecuteImageCopyAs) { Tag = ImageView.ImageType.PNG },
                    new ToolStripMenuItem("BMP",     null,this.OnExecuteImageCopyAs) { Tag = ImageView.ImageType.BMP },
                }),
                new ToolStripMenuItem("Paste",       null, this.OnExecuteImagePaste),
                new ToolStripMenuItem("Paste As",    null,  new ToolStripItem[]
                {
                    new ToolStripMenuItem("BASE 64", null,this.OnExecuteImagePasteAs) { Tag = ImageView.ImageType.B64 },
                }),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Zoom In",     null, this.OnExecuteImageZoomIn),
                new ToolStripMenuItem("Zoom Out",    null, this.OnExecuteImageZoomOut),
                new ToolStripMenuItem("Entire View", null, this.OnExecuteImageEntireView),
                new ToolStripMenuItem("Native Size", null, this.OnExecuteImageNativeSize),
            });
#if DEBUG
            this.txLinesEdit.Lines = MainForm.example;
            this.txLinesEdit.SelectionStart = 0;
            this.txLinesEdit.SelectionLength = 0;
            this.btLeftToRight.PerformClick();
#endif
        }

        protected override void OnHandleCreated(EventArgs args)
        {
            base.OnHandleCreated(args);

            IntPtr hSystemMenu = GetSystemMenu(this.Handle, false);

            MainForm.AppendMenu(hSystemMenu, MainForm.MF_SEPARATOR, IntPtr.Zero, String.Empty);
            MainForm.AppendMenu(hSystemMenu, MainForm.MF_STRING, MainForm.aboutMenuSystemId, "&About…");
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);

            if (msg.Msg == MainForm.WM_SYSCOMMAND)
            {
                if (msg.WParam == MainForm.aboutMenuSystemId)
                {
                    AboutDialog.Show(this);
                }
            }
        }

        #endregion

        #region Lines Edit Menu Events

        private void OnExecuteLinesOpen(Object sender, EventArgs args)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                FilterIndex = 0,
                Filter = "X11 Pix Maps (*.xpm)|*.xpm|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Open XPixMap"
            };

            if (DialogResult.OK != dialog.ShowDialog())
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    this.txLinesEdit.Lines = File.ReadAllLines(dialog.FileName, Encoding.ASCII);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, $"{this.Text} ({dialog.Title})");
            }
        }

        private void OnExecuteLinesSave(Object sender, EventArgs args)
        {
            if (this.txLinesEdit.TextLength < 1)
            {
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog()
            {
                FilterIndex = 0,
                Filter = "X11 Pix Maps (*.xpm)|*.xpm|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                RestoreDirectory = true,
                DefaultExt = ".xpm",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Save XPixMap"
            };

            if (DialogResult.OK != dialog.ShowDialog())
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    File.WriteAllLines(dialog.FileName, this.txLinesEdit.Lines, Encoding.ASCII);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, $"{this.Text} ({dialog.Title})");
            }
        }

        private void OnExecuteLinesCopy(Object sender, EventArgs args)
        {
            try
            {
                using (new WaitCursor(this))
                {
                    Int32 selectionStart = -1;

                    if (this.txLinesEdit.SelectionLength == 0)
                    {
                        selectionStart = this.txLinesEdit.SelectionStart;
                        this.txLinesEdit.SelectAll();
                    }

                    this.txLinesEdit.Copy();

                    if (selectionStart != -1)
                    {
                        this.txLinesEdit.SelectionLength = 0;
                        this.txLinesEdit.SelectionStart = selectionStart;
                    }
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteLinesCopyAs(Object sender, EventArgs args)
        {
            if (this.txLinesEdit.TextLength < 1 || sender is not ToolStripMenuItem menu)
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    StringBuilder builder = new StringBuilder(512);

                    String[] lines = this.txLinesEdit.Lines;

                    if (menu.Text == "C#")
                    {
                        builder.AppendLine("private static readonly string[] xpm = new string[] {");

                        for (Int32 index = 0; index < lines.Length; index++)
                        {
                            builder.AppendLine($"\t\"{lines[index]}\"{(index + 1 < lines.Length ? "," : "")}");
                        }

                        builder.AppendLine("};");
                    }
                    else if (menu.Text == "C/C++")
                    {
                        builder.AppendLine("static char *xpm[] = {");

                        for (Int32 index = 0; index < lines.Length; index++)
                        {
                            builder.AppendLine($"\t\"{lines[index]}\"{(index + 1 < lines.Length ? "," : "")}");
                        }

                        builder.AppendLine("};");
                    }
                    else if (menu.Text == "VB")
                    {
                        builder.AppendLine("Private Shared ReadOnly xpm As String() = {");

                        for (Int32 index = 0; index < lines.Length; index++)
                        {
                            builder.AppendLine($"\t\"{lines[index]}\"{(index + 1 < lines.Length ? "," : "")}");
                        }

                        builder.AppendLine("}");
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    Clipboard.SetText(builder.ToString(), TextDataFormat.Text);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteLinesCut(Object sender, EventArgs args)
        {
            try
            {
                using (new WaitCursor(this))
                {
                    this.txLinesEdit.Cut();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteLinesPaste(Object sender, EventArgs args)
        {
            try
            {
                using (new WaitCursor(this))
                {
                    this.txLinesEdit.Paste();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteLinesUndo(Object sender, EventArgs args)
        {
            try
            {
                using (new WaitCursor(this))
                {
                    this.txLinesEdit.Undo();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        #endregion

        #region Swap Buttons Click Events

        private void OnButtonLeftToRightClick(Object sender, EventArgs args)
        {
            if (this.txLinesEdit.TextLength < 1)
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    XPixMapType selected = (this.btLeftToRight.ContextMenuStrip as RadioMenuStrip)?.Selected ?? XPixMapType.BestFit;

                    this.pbImageView.Image = XPixMapParser.Parse(this.txLinesEdit.Lines, selected);
                    this.pbImageView.FitToSize();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnButtonRightToLeftClick(Object sender, EventArgs args)
        {
            if (this.pbImageView.Image is null)
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    XPixMapType selected = (this.btRightToLeft.ContextMenuStrip as CheckMenuStrip)?.Selected ?? XPixMapType.BestFit;

                    this.txLinesEdit.Lines = this.pbImageView.Image.Build(selected);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        #endregion

        #region Image View Menu Events

        private void OnExecuteImageOpen(Object sender, EventArgs args)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                FilterIndex = 0,
                Filter = "Image Files(*.png;*.bmp;*.jpg;*.gif)|*.png;*.bmp;*.jpg;*.gif|All Files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Open Picture"
            };

            if (DialogResult.OK != dialog.ShowDialog())
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    this.pbImageView.Image = Image.FromFile(dialog.FileName);
                    this.pbImageView.FitToSize();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, $"{this.Text} ({dialog.Title})");
            }
        }

        private void OnExecuteImageSave(Object sender, EventArgs args)
        {
            if (this.pbImageView.Image is null)
            {
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog()
            {
                FilterIndex = 0,
                Filter = "Image Files(*.png;*.bmp;*.jpg;*.gif)|*.png;*.bmp;*.jpg;*.gif|All Files (*.*)|*.*",
                RestoreDirectory = true,
                DefaultExt = ".png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Save Picture"
            };

            if (DialogResult.OK != dialog.ShowDialog())
            {
                return;
            }

            try
            {
                ImageFormat format;
                String ext = Path.GetExtension(dialog.FileName).TrimStart('.');

                if (String.Equals(ext, "png", StringComparison.InvariantCultureIgnoreCase))
                {
                    format = ImageFormat.Png;
                }
                else if (String.Equals(ext, "bmp", StringComparison.InvariantCultureIgnoreCase))
                {
                    format = ImageFormat.Bmp;
                }
                else if (String.Equals(ext, "jpg", StringComparison.InvariantCultureIgnoreCase))
                {
                    format = ImageFormat.Jpeg;
                }
                else if (String.Equals(ext, "gif", StringComparison.InvariantCultureIgnoreCase))
                {
                    format = ImageFormat.Gif;
                }
                else
                {
                    this.ShowError($"Picture format '{ext.ToUpper()}' is not supported.", $"{this.Text} ({dialog.Title})");
                    return;
                }

                using (new WaitCursor(this))
                {
                    this.pbImageView.Image.Save(dialog.FileName, format);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, $"{this.Text} ({dialog.Title})");
            }
        }

        private void OnExecuteImageCopy(Object sender, EventArgs args)
        {
            try
            {
                using (new WaitCursor(this))
                {
                    this.pbImageView.Copy();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteImageCopyAs(Object sender, EventArgs args)
        {
            if (sender is not ToolStripMenuItem menu)
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    this.pbImageView.Copy((ImageView.ImageType)menu.Tag);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteImagePaste(Object sender, EventArgs args)
        {
            try
            {
                using (new WaitCursor(this))
                {
                    this.pbImageView.Paste();
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteImagePasteAs(Object sender, EventArgs args)
        {
            if (sender is not ToolStripMenuItem menu)
            {
                return;
            }

            try
            {
                using (new WaitCursor(this))
                {
                    this.pbImageView.Paste((ImageView.ImageType)menu.Tag);
                }
            }
            catch (Exception exception)
            {
                this.ShowError(exception.Message, this.Text);
            }
        }

        private void OnExecuteImageZoomIn(Object sender, EventArgs args)
        {
            this.pbImageView.ZoomIn();
        }

        private void OnExecuteImageZoomOut(Object sender, EventArgs args)
        {
            this.pbImageView.ZoomOut();
        }

        private void OnExecuteImageEntireView(Object sender, EventArgs args)
        {
            this.pbImageView.FitToSize();
        }

        private void OnExecuteImageNativeSize(Object sender, EventArgs args)
        {
            this.pbImageView.ZoomReset();
        }

        #endregion

        #region Win32-API

        private const Int32 WM_SYSCOMMAND = 0x00000112;
        private const Int32 MF_STRING = 0x00000000;
        private const Int32 MF_SEPARATOR = 0x00000800;
        private static readonly IntPtr aboutMenuSystemId = new IntPtr(0x00000001);

        [DllImport("user32.dll", CharSet = CharSet.None, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, Boolean bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Boolean AppendMenu(IntPtr hMenu, UInt32 uFlags, IntPtr uIDNewItem, String lpNewItem);

        #endregion 
    }
}

#nullable restore