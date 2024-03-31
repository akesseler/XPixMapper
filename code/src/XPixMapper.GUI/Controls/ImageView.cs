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

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace Plexdata.XPixMapper.GUI.Controls
{
    public class ImageView : ScrollableControl
    {
        #region Defines

        public enum ImageType { Default, B64, PNG, BMP }

        #endregion

        #region Privates

        private Point dragOffset = Point.Empty;

        #endregion 

        #region Construction

        public ImageView()
            : base()
        {
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.AutoScroll = true;
        }

        #endregion

        #region BorderStyle

        private BorderStyle borderStyle = BorderStyle.Fixed3D;

        [Category("Appearance")]
        [Description("Indicates the border style of this control.")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle BorderStyle
        {
            get { return this.borderStyle; }
            set
            {
                if (this.borderStyle == value)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(BorderStyle), value))
                {
                    throw new InvalidEnumArgumentException(nameof(this.BorderStyle), (Int32)value, typeof(BorderStyle));
                }

                this.borderStyle = value;
                base.UpdateStyles();
                base.Invalidate();
                this.OnBorderStyleChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [Description("Occurs when the value of property BorderStyle has changed.")]
        public event EventHandler<EventArgs> BorderStyleChanged;

        protected virtual void OnBorderStyleChanged(EventArgs args)
        {
            this.BorderStyleChanged?.Invoke(this, args);
        }

        #endregion 

        #region Image

        private Bitmap image = null;
        private Image original = null;

        [Bindable(true)]
        [Localizable(true)]
        [DefaultValue(null)]
        [Category("Appearance")]
        [Description("The image to display in this control.")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Image Image
        {
            get { return this.original; }
            set
            {
                if (this.image == value)
                {
                    return;
                }

                this.image?.Dispose();
                this.image = null;
                this.original = null;

                if (value != null)
                {
                    using (Graphics graphics = this.CreateGraphics())
                    {
                        Bitmap helper = null;
                        try
                        {
                            helper = new Bitmap(value);
                            helper.SetResolution(graphics.DpiX, graphics.DpiY);

                            this.image = helper.Clone(new Rectangle(0, 0, value.Width, value.Height), PixelFormat.Format32bppPArgb);
                            this.original = value;
                        }
                        catch
                        {
                            this.image?.Dispose();
                            this.image = null;
                            this.original = null;
                        }
                        finally
                        {
                            helper?.Dispose();
                        }
                    }
                }

                this.SetZoomFactor(this.DefaultZoomFactor);
                this.UpdateScrollbars(true);
                this.OnImageChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [Description("Occurs when the value of property Image has changed.")]
        public event EventHandler<EventArgs> ImageChanged;

        protected virtual void OnImageChanged(EventArgs args)
        {
            this.ImageChanged?.Invoke(this, args);
        }

        #endregion

        #region ErrorText

        private String errorText = String.Empty;

        [Category("Appearance")]
        [Description("The error text to be used if no image is available.")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(typeof(String), "System.String.Empty")]
        public String ErrorText
        {
            get { return String.IsNullOrEmpty(this.errorText) ? String.Empty : this.errorText; }
            set
            {
                if (this.errorText == value)
                {
                    return;
                }

                this.errorText = (value ?? String.Empty).Trim();

                this.Invalidate();
                this.OnErrorTextChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [Description("Occurs when the value of property ErrorText has changed.")]
        public event EventHandler<EventArgs> ErrorTextChanged;

        protected virtual void OnErrorTextChanged(EventArgs args)
        {
            this.ErrorTextChanged?.Invoke(this, args);
        }

        #endregion 

        #region ZoomFactor

        private Int32 zoomFactor = ImageView.ZoomFactorDefaultValue;

        [DefaultValue(ImageView.ZoomFactorDefaultValue)]
        [Category("Zooming")]
        [Description("The current zoom factor in percent.")]
        [RefreshProperties(RefreshProperties.All)]
        public Int32 ZoomFactor
        {
            get { return this.zoomFactor; }
            set
            {
                if (this.zoomFactor == value)
                {
                    return;
                }

                if (!this.IsValidZoomFactor(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ZoomFactor), value,
                        $"Zoom factor must be in range of {this.minimumZoomFactor} up to {this.maximumZoomFactor}.");
                }

                this.InvokeZoomFactor(value);
            }
        }

        [Category("Zooming")]
        [Description("Occurs when the value of property ZoomFactor has changed.")]
        public event EventHandler<EventArgs> ZoomFactorChanged;

        protected virtual void OnZoomFactorChanged(EventArgs args)
        {
            this.ZoomFactorChanged?.Invoke(this, args);
        }

        #endregion 

        #region DefaultZoomFactor 

        private const Int32 ZoomFactorDefaultValue = 100;

        private Int32 defaultZoomFactor = ImageView.ZoomFactorDefaultValue;

        [DefaultValue(ImageView.ZoomFactorDefaultValue)]
        [Category("Zooming")]
        [Description("The current default zoom factor in percent.")]
        [RefreshProperties(RefreshProperties.All)]
        public Int32 DefaultZoomFactor
        {
            get { return this.defaultZoomFactor; }
            set
            {
                if (this.defaultZoomFactor == value)
                {
                    return;
                }

                if (!this.IsValidZoomFactor(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.DefaultZoomFactor), value,
                        $"Default zoom factor must be in range of {this.minimumZoomFactor} up to {this.maximumZoomFactor}.");
                }

                this.defaultZoomFactor = value;

                this.OnDefaultZoomFactorChanged(EventArgs.Empty);
            }
        }

        [Category("Zooming")]
        [Description("Occurs when the value of property DefaultZoomFactor has changed.")]
        public event EventHandler<EventArgs> DefaultZoomFactorChanged;

        protected virtual void OnDefaultZoomFactorChanged(EventArgs args)
        {
            this.DefaultZoomFactorChanged?.Invoke(this, args);
        }

        #endregion

        #region MinimumZoomFactor

        private const Int32 ZoomFactorMinimumValue = 10;

        private Int32 minimumZoomFactor = ImageView.ZoomFactorMinimumValue;

        [DefaultValue(ImageView.ZoomFactorMinimumValue)]
        [Category("Zooming")]
        [Description("The current minimum zoom factor in percent.")]
        [RefreshProperties(RefreshProperties.All)]
        public Int32 MinimumZoomFactor
        {
            get { return this.minimumZoomFactor; }
            set
            {
                if (this.minimumZoomFactor == value)
                {
                    return;
                }

                if (value <= 0 || value >= this.maximumZoomFactor)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.MinimumZoomFactor), value, $"Minimum zoom factor must be greater than zero and less than the maximum of {this.maximumZoomFactor}.");
                }

                this.minimumZoomFactor = value;

                if (this.zoomFactor < value) { this.SetZoomFactor(value); }

                if (this.defaultZoomFactor < value) { this.DefaultZoomFactor = value; }

                this.OnMinimumZoomFactorChanged(EventArgs.Empty);
            }
        }

        [Category("Zooming")]
        [Description("Occurs when the value of property MinimumZoomFactor has changed.")]
        public event EventHandler<EventArgs> MinimumZoomFactorChanged;

        protected virtual void OnMinimumZoomFactorChanged(EventArgs args)
        {
            this.MinimumZoomFactorChanged?.Invoke(this, args);
        }

        #endregion

        #region MaximumZoomFactor

        private const Int32 ZoomFactorMaximumValue = 6400;

        private Int32 maximumZoomFactor = ImageView.ZoomFactorMaximumValue;

        [DefaultValue(ImageView.ZoomFactorMaximumValue)]
        [Category("Zooming")]
        [Description("The current maximum zoom factor in percent.")]
        [RefreshProperties(RefreshProperties.All)]
        public Int32 MaximumZoomFactor
        {
            get { return this.maximumZoomFactor; }
            set
            {
                if (this.maximumZoomFactor == value)
                {
                    return;
                }

                if (value <= 0 || value <= this.minimumZoomFactor)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.MaximumZoomFactor), value,
                        $"Maximum zoom factor must be greater than zero and greater than the minimum of {this.minimumZoomFactor}.");
                }

                this.maximumZoomFactor = value;

                if (this.zoomFactor > value) { this.SetZoomFactor(value); }

                if (this.defaultZoomFactor > value) { this.DefaultZoomFactor = value; }

                this.OnMaximumZoomFactorChanged(EventArgs.Empty);
            }
        }

        [Category("Zooming")]
        [Description("Occurs when the value of property MaximumZoomFactor has changed.")]
        public event EventHandler<EventArgs> MaximumZoomFactorChanged;

        protected virtual void OnMaximumZoomFactorChanged(EventArgs args)
        {
            this.MaximumZoomFactorChanged?.Invoke(this, args);
        }

        #endregion 

        #region Interpolation

        private Boolean interpolation = true;

        [DefaultValue(true)]
        [Category("Appearance")]
        [Description("The interpolation mode to draw the image.")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Boolean Interpolation
        {
            get { return this.interpolation; }
            set
            {
                if (this.interpolation == value)
                {
                    return;
                }

                this.interpolation = value;

                this.Invalidate();
                this.OnInterpolationChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [Description("Occurs when the value of property Interpolation has changed.")]
        public event EventHandler<EventArgs> InterpolationChanged;

        protected virtual void OnInterpolationChanged(EventArgs args)
        {
            this.InterpolationChanged?.Invoke(this, args);
        }

        #endregion 

        #region SuppressScrollbars 

        private Boolean suppressScrollbars = false;

        [DefaultValue(true)]
        [Category("Appearance")]
        [Description("Enables or disables scrollbar suppressing mode.")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Boolean SuppressScrollbars
        {
            get { return this.suppressScrollbars; }
            set
            {
                if (this.suppressScrollbars == value)
                {
                    return;
                }

                this.suppressScrollbars = value;

                this.UpdateScrollbars(false);
                this.Invalidate();
                this.OnSuppressScrollbarsChanged(EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        [Description("Occurs when the value of property SuppressScrollbars has changed.")]
        public event EventHandler<EventArgs> SuppressScrollbarsChanged;

        protected virtual void OnSuppressScrollbarsChanged(EventArgs args)
        {
            this.SuppressScrollbarsChanged?.Invoke(this, args);
        }

        #endregion

        #region Inherited property reimplementation.

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Boolean AutoScroll
        {
            get { return base.AutoScroll; }
            set { base.AutoScroll = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override String Text
        {
            get { return this.ErrorText; }
            set { this.ErrorText = value; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Very smart because client rectangle is also adjusted! 
                // See also: http://support.microsoft.com/kb/316574
                const Int32 WS_BORDER = unchecked(0x00800000);
                const Int32 WS_EX_STATICEDGE = unchecked(0x00020000);

                CreateParams createParams = base.CreateParams;
                createParams.ExStyle &= (~WS_EX_STATICEDGE);
                createParams.Style &= (~WS_BORDER);

                switch (this.borderStyle)
                {
                    case BorderStyle.Fixed3D:
                        createParams.ExStyle |= WS_EX_STATICEDGE;
                        break;
                    case BorderStyle.FixedSingle:
                        createParams.Style |= WS_BORDER;
                        break;
                }

                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get { return new Size(300, 200); }
        }

        protected override Size DefaultMinimumSize
        {
            get { return new Size(150, 50); }
        }

        protected override Padding DefaultPadding
        {
            get { return new Padding(10); }
        }

        #endregion 

        #region Rotating 

        [Category("Behavior")]
        [Description("Occurs when the image has been rotated to the left.")]
        public event EventHandler<EventArgs> RotatedLeft;

        public void RotateLeft()
        {
            if (this.image != null)
            {
                this.image.RotateFlip(RotateFlipType.Rotate90FlipXY);
                this.FitToSize(); // This is what the Windows image view does.

                this.RotatedLeft?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Behavior")]
        [Description("Occurs when the image has been rotated to the right.")]
        public event EventHandler<EventArgs> RotatedRight;

        public void RotateRight()
        {
            if (this.image == null)
            {
                return;
            }

            this.image.RotateFlip(RotateFlipType.Rotate270FlipXY);
            this.FitToSize(); // This is what the Windows image view does.

            this.RotatedRight?.Invoke(this, EventArgs.Empty);
        }

        #endregion 

        #region Zooming 

        public Boolean IsValidZoomFactor(Int32 factor)
        {
            return this.minimumZoomFactor <= factor && factor <= this.maximumZoomFactor;
        }

        public void FitToSize()
        {
            if (this.image == null)
            {
                return;
            }

            Single cxImage = this.image.Width;
            Single cyImage = this.image.Height;
            Single cxClient = (Single)this.ClientSize.Width - this.Padding.Horizontal;
            Single cyClient = (Single)this.ClientSize.Height - this.Padding.Vertical;

            if (this.VScroll) { cxClient += SystemInformation.VerticalScrollBarWidth; }
            if (this.HScroll) { cyClient += SystemInformation.HorizontalScrollBarHeight; }

            Single xRatio = cxClient / cxImage;
            Single yRatio = cyClient / cyImage;

            // Do not round but just cut off decimals!
            Int32 factor = (Int32)(((xRatio < yRatio) ? xRatio : yRatio) * 100f);

            this.InvokeZoomFactor(Math.Max(factor, 1));
        }

        public void FitToWidth()
        {
            if (this.image == null)
            {
                return;
            }

            Single cxImage = this.image.Width;
            Single cxClient = (Single)this.ClientSize.Width - this.Padding.Horizontal;

            if (!this.VScroll) { cxClient -= SystemInformation.VerticalScrollBarWidth; }

            Single xRatio = cxClient / cxImage;

            // Do not round but just cut off decimals!
            Int32 factor = (Int32)(xRatio * 100f);

            this.InvokeZoomFactor(Math.Max(factor, 1));
        }

        public void ZoomReset()
        {
            this.SetZoomFactor(this.DefaultZoomFactor);
        }

        public void ZoomIn()
        {
            this.ZoomIn(new PointF(this.ClientSize.Width / 2f, this.ClientSize.Height / 2f));
        }

        public void ZoomIn(PointF point)
        {
            this.ZoomIn(point, this.zoomFactor * 2);
        }

        public void ZoomIn(Int32 factor)
        {
            this.ZoomIn(new PointF(this.ClientSize.Width / 2f, this.ClientSize.Height / 2f), factor);
        }

        public void ZoomIn(PointF point, Int32 factor)
        {
            this.Zoom(point, factor);
        }

        public void ZoomOut()
        {
            this.ZoomOut(new PointF(this.ClientSize.Width / 2f, this.ClientSize.Height / 2f));
        }

        public void ZoomOut(PointF point)
        {
            this.ZoomOut(point, this.zoomFactor / 2);
        }

        public void ZoomOut(Int32 factor)
        {
            this.ZoomOut(new PointF(this.ClientSize.Width / 2f, this.ClientSize.Height / 2f), factor);
        }

        public void ZoomOut(PointF point, Int32 factor)
        {
            this.Zoom(point, factor);
        }

        protected void SetZoomFactor(Int32 factor)
        {
            if (this.IsValidZoomFactor(factor))
            {
                this.ZoomFactor = factor;
            }
        }

        private void InvokeZoomFactor(Int32 factor)
        {
            if (factor > 0)
            {
                this.zoomFactor = factor;

                this.UpdateScrollbars(false);
                this.OnZoomFactorChanged(EventArgs.Empty);
            }
        }

        private void Zoom(PointF point, Int32 factor)
        {
            Single current = this.zoomFactor;

            if (current > this.maximumZoomFactor)
            {
                current = this.maximumZoomFactor;
            }

            if (current < this.minimumZoomFactor)
            {
                current = this.minimumZoomFactor;
            }

            if (factor > this.maximumZoomFactor)
            {
                factor = this.maximumZoomFactor;
            }

            if (factor < this.minimumZoomFactor)
            {
                factor = this.minimumZoomFactor;
            }

            if (factor == this.zoomFactor)
            {
                return;
            }

            this.BeginUpdate();

            try
            {
                Single ratio = factor / current;

                PointF offset = this.ScaledImageOffset;

                Single xScroll = (point.X - offset.X) * ratio - point.X + this.Padding.Left;
                Single yScroll = (point.Y - offset.Y) * ratio - point.Y + this.Padding.Top;

                RectangleF oldClient = this.ClientRectangle;

                this.SetZoomFactor(factor);

                RectangleF newClient = this.ClientRectangle;

                if (oldClient.Width > newClient.Width)
                {
                    xScroll += (oldClient.Width - newClient.Width) / ratio;
                }

                if (oldClient.Height > newClient.Height)
                {
                    yScroll += (oldClient.Height - newClient.Height) / ratio;
                }

                this.AutoScrollPosition = new Point((Int32)Math.Round(xScroll, 0), (Int32)Math.Round(yScroll, 0));
            }
            finally
            {
                this.EndUpdate();
            }
        }

        #endregion 

        #region Clipboard

        public void Copy()
        {
            this.Copy(ImageType.Default);
        }

        public void Copy(ImageType type)
        {
            if (this.Image == null)
            {
                return;
            }

            switch (type)
            {
                case ImageType.B64:
                    this.CopyAsB64();
                    break;
                case ImageType.BMP:
                    this.CopyAsBmp();
                    break;
                case ImageType.PNG:
                case ImageType.Default:
                default:
                    this.CopyAsPng();
                    break;
            }
        }

        private void CopyAsB64()
        {
            const Int32 extent = 80;

            using (MemoryStream stream = new MemoryStream())
            {
                this.Image.Save(stream, ImageFormat.Png);

                String result = Convert.ToBase64String(stream.ToArray());

                Int32 capacity = result.Length + ((result.Length / extent) + 1) * Environment.NewLine.Length;

                StringBuilder builder = new StringBuilder(capacity);

                for (Int32 offset = 0; offset < result.Length; offset += extent)
                {
                    Int32 length = (offset + extent) < result.Length ? extent : result.Length - offset;

                    builder.AppendLine(result.Substring(offset, length));
                }

                Clipboard.SetText(builder.ToString().TrimEnd(), TextDataFormat.Text);
            }
        }

        private void CopyAsPng()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                this.Image.Save(stream, ImageFormat.Png);

                IDataObject data = new DataObject();
                data.SetData(DataFormats.GetFormat("PNG").Name, false, stream);

                Clipboard.SetDataObject(data, true);
            }
        }

        private void CopyAsBmp()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                this.Image.Save(stream, ImageFormat.Bmp);

                Clipboard.SetImage(Image.FromStream(stream));
            }
        }

        public void Paste()
        {
            if (!Clipboard.ContainsImage())
            {
                return;
            }

            if (Clipboard.GetDataObject() != null && Clipboard.GetDataObject().GetDataPresent("PNG"))
            {
                // This works at least for images that come from paint.net via copy and paste!
                // But once the image was processed, it will no longer be copied back as a PNG.
                this.Image = Image.FromStream((Stream)Clipboard.GetData("PNG"));
            }
            else
            {
                this.Image = Clipboard.GetImage();
            }

            this.FitToSize();
        }

        public void Paste(ImageType type)
        {
            switch (type)
            {
                case ImageType.B64:
                    this.PasteAsB64();
                    break;
                default:
                    break;
            }
        }

        private void PasteAsB64()
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            String source = Clipboard.GetText();

            if (String.IsNullOrWhiteSpace(source))
            {
                return;
            }

            String value = String.Join(String.Empty, source.Split(new Char[] { '\r', '\n', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)).Trim();

            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(value)))
            {
                this.Image = Image.FromStream(stream);
            }

            this.FitToSize();
        }

        #endregion

        #region Drawing 

        protected override void OnPaintBackground(PaintEventArgs args)
        {
            if (this.Enabled)
            {
                base.OnPaintBackground(args);
            }
            else
            {
                args.Graphics.Clear(SystemColors.Control);
            }
        }

        protected override void OnPaint(PaintEventArgs args)
        {
            GraphicsState state = args.Graphics.Save();

            try
            {
                if (this.image == null)
                {
                    this.RenderError(args.Graphics);
                }
                else
                {
                    if (this.zoomFactor < 100)
                    {
                        args.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    }
                    else
                    {
                        if (this.interpolation)
                        {
                            args.Graphics.InterpolationMode = InterpolationMode.Bilinear;
                        }
                        else
                        {
                            args.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                        }
                    }

                    args.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

                    Matrix transform = args.Graphics.Transform;
                    this.RenderImage(args.Graphics);
                    args.Graphics.Transform = transform;
                }
            }
            finally
            {
                args.Graphics.Restore(state);
            }

            base.OnPaint(args);
        }

        private void RenderError(Graphics graphics)
        {
            if (String.IsNullOrEmpty(this.errorText))
            {
                return;
            }

            Color foreground = this.Enabled ? this.ForeColor : SystemColors.GrayText;

            using (Brush brush = new SolidBrush(foreground))
            {
                using (StringFormat format = new StringFormat())
                {
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Center;

                    graphics.DrawString(this.errorText, this.Font, brush, this.ClientRectangle, format);
                }
            }
        }

        private void RenderImage(Graphics graphics)
        {
            Single factor = this.zoomFactor / 100f;
            Matrix matrix = new Matrix();
            matrix.Scale(factor, factor, MatrixOrder.Append);
            graphics.Transform = matrix;

            PointF offset = this.ScaledImageOffset;
            Single xOffset = offset.X / factor;
            Single yOffset = offset.Y / factor;

            if (this.Enabled)
            {
                graphics.DrawImage(this.image, xOffset, yOffset);
            }
            else
            {
                ControlPaint.DrawImageDisabled(graphics, this.image, (Int32)Math.Round(xOffset, 0), (Int32)Math.Round(yOffset, 0), SystemColors.Control);
            }
        }

        private void RenderNonClientArea()
        {
            // This is actually enough for the moment, but it can be optimized later.

            if (this.BorderStyle == BorderStyle.None)
            {
                return;
            }

            IntPtr hDc = ImageView.GetWindowDC(this.Handle);

            try
            {
                using (Graphics graphics = Graphics.FromHdcInternal(hDc))
                {
                    Color color = base.Focused ? Color.FromArgb(unchecked((Int32)0XFF0078D7)) : Color.FromArgb(unchecked((Int32)0xff7a7a7a));
                    Rectangle bounds = new Rectangle(0, 0, base.Width - SystemInformation.BorderSize.Width, base.Height - SystemInformation.BorderSize.Height);

                    using (Pen pen = new Pen(color))
                    {
                        graphics.DrawRectangle(pen, bounds);
                    }
                }
            }
            finally
            {
                Int32 hResult = ImageView.ReleaseDC(base.Handle, hDc);
                Debug.WriteLineIf(hResult != 1, $"ReleaseDC() failed with error {Marshal.GetLastWin32Error()}");
            }
        }

        public void BeginUpdate()
        {
            ImageView.SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            ImageView.SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);

            this.Invalidate();
        }

        private void RefreshNonClientArea()
        {
            ImageView.RedrawWindow(base.Handle, IntPtr.Zero, IntPtr.Zero, ImageView.RDW_FRAME | ImageView.RDW_IUPDATENOW | ImageView.RDW_INVALIDATE);
        }

        #endregion

        #region Keyboard 

        private Boolean IsNoneKey
        {
            get
            {
                return Control.ModifierKeys == Keys.None;
            }
        }

        private Boolean IsShiftKey
        {
            get
            {
                return (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            }
        }

        private Boolean IsControlKey
        {
            get
            {
                return (Control.ModifierKeys & Keys.Control) == Keys.Control;
            }
        }

        public override Boolean PreProcessMessage(ref Message message)
        {
            const Int32 WM_KEYDOWN = 0x0100;

            if (message.Msg == WM_KEYDOWN && this.ProcessKeyDown(ref message))
            {
                return true;
            }
            else
            {
                return base.PreProcessMessage(ref message);
            }
        }

        private Boolean ProcessKeyDown(ref Message message)
        {
            Keys keys = (Keys)message.WParam.ToInt32();

            if (keys == Keys.Home)
            {
                this.ScrollHome();
                return true;
            }
            else if (keys == Keys.End)
            {
                this.ScrollEnd();
                return true;
            }
            else if (keys == Keys.Up)
            {
                if (this.IsControlKey)
                {
                    this.ScrollPageUp();
                }
                else
                {
                    this.ScrollUp();
                }
                return true;
            }
            else if (keys == Keys.Down)
            {
                if (this.IsControlKey)
                {
                    this.ScrollPageDown();
                }
                else
                {
                    this.ScrollDown();
                }
                return true;
            }
            else if (keys == Keys.Left)
            {
                if (this.IsControlKey)
                {
                    this.ScrollPageLeft();
                }
                else
                {
                    this.ScrollLeft();
                }
                return true;
            }
            else if (keys == Keys.Right)
            {
                if (this.IsControlKey)
                {
                    this.ScrollPageRight();
                }
                else
                {
                    this.ScrollRight();
                }
                return true;
            }
            else if (keys == Keys.PageUp)
            {
                if (this.IsControlKey)
                {
                    this.ScrollPageLeft();
                }
                else
                {
                    this.ScrollPageUp();
                }
                return true;
            }
            else if (keys == Keys.PageDown)
            {
                if (this.IsControlKey)
                {
                    this.ScrollPageRight();
                }
                else
                {
                    this.ScrollPageDown();
                }
                return true;
            }
            else if ((keys == Keys.Add || keys == Keys.Oemplus) && this.IsControlKey)
            {
                this.ZoomIn();
                return true;
            }
            else if ((keys == Keys.Subtract || keys == Keys.OemMinus) && this.IsControlKey)
            {
                this.ZoomOut();
                return true;
            }
            else if ((keys == Keys.D0 || keys == Keys.NumPad0) && this.IsControlKey)
            {
                this.SetZoomFactor(this.DefaultZoomFactor);
                return true;
            }
            else if ((keys == Keys.Enter || keys == Keys.Return) && this.IsControlKey)
            {
                this.FitToSize();
                return true;
            }
            else if ((keys == Keys.C || keys == Keys.Insert) && this.IsControlKey)
            {
                this.Copy();
                return true;
            }
            else if (keys == Keys.V && this.IsControlKey || keys == Keys.Insert && this.IsShiftKey)
            {
                this.Paste();
                return true;
            }
            else if (keys == Keys.Tab)
            {
                Control parent = this.Parent;
                while (parent != null)
                {
                    if (parent.CanSelect)
                    {
                        return parent.SelectNextControl(this, !this.IsShiftKey, true, true, true);
                    }
                    parent = parent.Parent;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        #endregion 

        #region Mouse 

        protected override void OnMouseDown(MouseEventArgs args)
        {
            if (!this.Focused) { this.Focus(); }

            if (args.Button == MouseButtons.Left)
            {
                this.dragOffset = new Point(args.Location.X, args.Location.Y);
            }

            base.OnMouseDown(args);
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                if (this.HScroll)
                {
                    if (this.dragOffset.X < args.Location.X)
                    {
                        this.ScrollLeft(args.Location.X - this.dragOffset.X);
                    }
                    else
                    {
                        this.ScrollRight(this.dragOffset.X - args.Location.X);
                    }
                }

                if (this.VScroll)
                {
                    if (this.dragOffset.Y < args.Location.Y)
                    {
                        this.ScrollUp(args.Location.Y - this.dragOffset.Y);
                    }
                    else
                    {
                        this.ScrollDown(this.dragOffset.Y - args.Location.Y);
                    }
                }

                this.dragOffset = new Point(args.Location.X, args.Location.Y);
            }

            base.OnMouseMove(args);
        }

        protected override void OnMouseUp(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.dragOffset = Point.Empty;
            }

            base.OnMouseUp(args);
        }

        protected override void OnMouseWheel(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.None)
            {
                if (this.IsNoneKey)
                {
                    if (args.Delta > 0)
                    {
                        this.ScrollUp();
                    }
                    else if (args.Delta < 0)
                    {
                        this.ScrollDown();
                    }
                }
                else if (this.IsShiftKey)
                {
                    if (args.Delta > 0)
                    {
                        this.ScrollLeft();
                    }
                    else if (args.Delta < 0)
                    {
                        this.ScrollRight();
                    }
                }
                else if (this.IsControlKey)
                {
                    if (args.Delta > 0)
                    {
                        this.ZoomIn(args.Location);
                    }
                    else if (args.Delta < 0)
                    {
                        this.ZoomOut(args.Location);
                    }
                }
            }

            // Inherit member function OnMouseWheel() cause some trouble when trying to scroll horizontal using
            // SHIFT key. In this case, that member function scrolls vertical too! Therefore, it was necessary
            // to "overwrite" the MouseWheel event declaration and to fire this event explicitly!
            this.MouseWheel?.Invoke(this, args);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public new event MouseEventHandler MouseWheel;

        #endregion 

        #region Scrolling 

        protected void ScrollHome()
        {
            Int32 x = Math.Abs(this.AutoScrollPosition.X);
            Int32 y = Math.Abs(this.AutoScrollPosition.Y);

            if (!this.IsShiftKey && !this.IsControlKey)
            {
                x = this.HorizontalScroll.Minimum;
            }
            else if (this.IsShiftKey && !this.IsControlKey)
            {
                y = this.VerticalScroll.Minimum;
            }
            else if (!this.IsShiftKey && this.IsControlKey)
            {
                x = this.HorizontalScroll.Minimum;
                y = this.VerticalScroll.Minimum;
            }
            else
            {
                return;
            }

            this.AutoScrollPosition = new Point(x, y);
        }

        protected void ScrollEnd()
        {
            Int32 x = Math.Abs(this.AutoScrollPosition.X);
            Int32 y = Math.Abs(this.AutoScrollPosition.Y);

            if (!this.IsShiftKey && !this.IsControlKey)
            {
                x = this.HorizontalScroll.Maximum;
            }
            else if (this.IsShiftKey && !this.IsControlKey)
            {
                y = this.VerticalScroll.Maximum;
            }
            else if (!this.IsShiftKey && this.IsControlKey)
            {
                x = this.HorizontalScroll.Maximum;
                y = this.VerticalScroll.Maximum;
            }
            else
            {
                return;
            }

            this.AutoScrollPosition = new Point(x, y);
        }

        protected void ScrollUp()
        {
            this.ScrollUp(this.VerticalScroll.SmallChange);
        }

        protected void ScrollUp(Int32 delta)
        {
            Int32 x = Math.Abs(this.AutoScrollPosition.X);
            Int32 y = Math.Abs(this.AutoScrollPosition.Y);

            y -= delta;

            this.AutoScrollPosition = new Point(x, y);
        }

        protected void ScrollDown()
        {
            this.ScrollDown(this.VerticalScroll.SmallChange);
        }

        protected void ScrollDown(Int32 delta)
        {
            Int32 x = Math.Abs(this.AutoScrollPosition.X);
            Int32 y = Math.Abs(this.AutoScrollPosition.Y);

            y += delta;

            this.AutoScrollPosition = new Point(x, y);
        }

        protected void ScrollLeft()
        {
            this.ScrollLeft(this.VerticalScroll.SmallChange);
        }

        protected void ScrollLeft(Int32 delta)
        {
            Int32 x = Math.Abs(this.AutoScrollPosition.X);
            Int32 y = Math.Abs(this.AutoScrollPosition.Y);

            x -= delta;

            this.AutoScrollPosition = new Point(x, y);
        }

        protected void ScrollRight()
        {
            this.ScrollRight(this.VerticalScroll.SmallChange);
        }

        protected void ScrollRight(Int32 delta)
        {
            Int32 x = Math.Abs(this.AutoScrollPosition.X);
            Int32 y = Math.Abs(this.AutoScrollPosition.Y);

            x += delta;

            this.AutoScrollPosition = new Point(x, y);
        }

        protected void ScrollPageUp()
        {
            this.ScrollUp(this.VerticalScroll.LargeChange);
        }

        protected void ScrollPageUp(Int32 pages)
        {
            this.ScrollUp(pages * this.VerticalScroll.LargeChange);
        }

        protected void ScrollPageDown()
        {
            this.ScrollDown(this.VerticalScroll.LargeChange);
        }

        protected void ScrollPageDown(Int32 pages)
        {
            this.ScrollDown(pages * this.VerticalScroll.LargeChange);
        }

        protected void ScrollPageLeft()
        {
            this.ScrollLeft(this.HorizontalScroll.LargeChange);
        }

        protected void ScrollPageLeft(Int32 pages)
        {
            this.ScrollLeft(pages * this.HorizontalScroll.LargeChange);
        }

        protected void ScrollPageRight()
        {
            this.ScrollRight(this.HorizontalScroll.LargeChange);
        }

        protected void ScrollPageRight(Int32 pages)
        {
            this.ScrollRight(pages * this.HorizontalScroll.LargeChange);
        }

        private void UpdateScrollbars(Boolean reset)
        {
            this.BeginUpdate();

            try
            {
                if (this.SuppressScrollbars)
                {
                    this.VScroll = false;
                    this.HScroll = false;
                    return;
                }

                if (reset)
                {
                    this.AutoScrollPosition = new Point(0, 0);
                }

                Size size = this.ScaledImageSize.ToSize();

                size.Width += this.Padding.Horizontal;
                size.Height += this.Padding.Vertical;

                this.AutoScrollMinSize = size;
            }
            finally
            {
                this.EndUpdate();
            }
        }

        #endregion 

        #region Others

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == WM_NCPAINT)
            {
                // Required to do this first! Otherwise scroll bars are not painted.
                base.WndProc(ref message);

                this.RenderNonClientArea();
                message.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref message);
        }

        protected override void OnPaddingChanged(EventArgs args)
        {
            base.OnPaddingChanged(args);
            this.UpdateScrollbars(false);
        }

        protected override void OnEnabledChanged(EventArgs args)
        {
            base.OnEnabledChanged(args);
            this.Invalidate();
        }

        protected override void OnGotFocus(EventArgs args)
        {
            base.OnGotFocus(args);
            this.RefreshNonClientArea();
        }

        protected override void OnLostFocus(EventArgs args)
        {
            base.OnLostFocus(args);
            this.RefreshNonClientArea();
        }

        protected override void OnSizeChanged(EventArgs args)
        {
            base.OnSizeChanged(args);
            this.RefreshNonClientArea();
        }

        #endregion

        #region Helpers

        private SizeF ScaledImageSize
        {
            get
            {
                SizeF result = SizeF.Empty;

                if (this.image != null)
                {
                    result = new SizeF(this.image.Width * (this.zoomFactor / 100f), this.image.Height * (this.zoomFactor / 100f));
                }

                return result;
            }
        }

        private PointF ScaledImageOffset
        {
            get
            {
                PointF result = PointF.Empty;

                if (this.image != null)
                {
                    Single xOffset = 0;
                    Single yOffset = 0;
                    Single factor = this.zoomFactor / 100f;

                    if (this.HScroll)
                    {
                        xOffset = this.AutoScrollPosition.X + this.Padding.Left;
                    }
                    else
                    {
                        xOffset = (this.ClientSize.Width - this.image.Width * factor) / 2f;
                    }

                    if (this.VScroll)
                    {
                        yOffset = this.AutoScrollPosition.Y + this.Padding.Top;
                    }
                    else
                    {
                        yOffset = (this.ClientSize.Height - this.image.Height * factor) / 2f;
                    }

                    result = new PointF(xOffset, yOffset);
                }

                return result;
            }
        }

        #endregion 

        #region Win32-API

        private const Int32 WM_SETREDRAW = 0x0000000B;
        private const Int32 WM_NCPAINT = 0x00000085;
        private const UInt32 RDW_INVALIDATE = 0x00000001;
        private const UInt32 RDW_IUPDATENOW = 0x00000100;
        private const UInt32 RDW_FRAME = 0x00000400;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 nMessage, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern Boolean RedrawWindow(IntPtr hWnd, IntPtr lprc, IntPtr hrgn, UInt32 flags);

        #endregion 
    }
}

#nullable restore