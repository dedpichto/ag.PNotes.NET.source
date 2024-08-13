// PNotes.NET - open source desktop notes manager
// Copyright (C) 2016 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using PNotes.NET.Annotations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    public class PNSkinPreview : Control, INotifyPropertyChanged
    {
        private ImageSource _SkinBitmap;
        private string _SkinText;
        private Thickness _TextMargin;
        private double _TextWidth;
        private double _TextHeight;

        static PNSkinPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PNSkinPreview),
                new FrameworkPropertyMetadata(typeof(PNSkinPreview)));
        }

        public ImageSource SkinBitmap
        {
            get => _SkinBitmap;
            set
            {
                if (Equals(_SkinBitmap, value)) return;
                _SkinBitmap = value;
                OnPropertyChanged();
            }
        }

        public string SkinText
        {
            get => _SkinText;
            set
            {
                if (string.Equals(_SkinText, value, StringComparison.Ordinal)) return;
                _SkinText = value;
                OnPropertyChanged();
            }
        }

        public Thickness TextMargin
        {
            get => _TextMargin;
            set { if (_TextMargin == value)return; _TextMargin = value;
                OnPropertyChanged();
            }
        }

        public double TextWidth
        {
            get => _TextWidth;
            set { if (Math.Abs(_TextWidth - value) < double.Epsilon)return; _TextWidth = value;
                OnPropertyChanged();
            }
        }

        public double TextHeight
        {
            get => _TextHeight;
            set { if (Math.Abs(_TextHeight - value) < double.Epsilon)return; _TextHeight = value;
                OnPropertyChanged();
            }
        }

        internal void EraseProperties()
        {
            SkinBitmap = null;
            SkinText = "";
            TextWidth = TextHeight = 0;
        }

        internal void SetProperties(PNGroup gr, PNSkinDetails skn, double imageWidth, double imageHeight)
        {
            try
            {
                var bitmap = (System.Drawing.Bitmap)skn.BitmapSkin.Clone();
                var xFactor = imageWidth / bitmap.Width;
                var yFactor = imageHeight / bitmap.Height;

                var font = PNStatic.FromLogFont(gr.Font);
                var text = PNLang.Instance.GetControlText("lblFontSample", "The quick brown fox jumps over the lazy dog");
                var brush = new SolidColorBrush(Color.FromArgb(gr.FontColor.A, gr.FontColor.R, gr.FontColor.G,
                    gr.FontColor.B));

                this.SetFont(font);

                TextWidth = skn.PositionEdit.Width * xFactor;
                TextHeight = skn.PositionEdit.Height * yFactor;
                TextMargin = new Thickness(skn.PositionEdit.Width / TextWidth * xFactor, skn.PositionEdit.Height / TextHeight * yFactor, 0, 0);
                SkinText = text;
                Foreground = brush;

                bitmap.MakeTransparent(skn.MaskColor);
                var bodyImage = PNStatic.ImageFromDrawingImage(bitmap);
                var drawingVisual = new DrawingVisual();
                var drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawImage(bodyImage, new Rect(0, 0, imageWidth, imageHeight));
                drawingContext.Close();
                var bmp = new RenderTargetBitmap(Convert.ToInt32(imageWidth), Convert.ToInt32(imageHeight), 96, 96, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);
                SkinBitmap = bmp;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
