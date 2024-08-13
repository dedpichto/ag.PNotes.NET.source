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

namespace PNotes.NET
{

    public class PNSkinlessPreview : Control, INotifyPropertyChanged
    {
        static PNSkinlessPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PNSkinlessPreview), new FrameworkPropertyMetadata(typeof(PNSkinlessPreview)));
        }

        private string _CaptionText;
        private Brush _captionBackground;
        private FontFamily _CaptionFontFamily;
        private FontStyle _CaptionFontStyle;
        private FontWeight _CaptionFontWeight;
        private double _CaptionFontSize;
        private FontStretch _CaptionFontStretch;
        private Brush _CaptionForeground;
        private Brush _BodyForeground;
        private string _BodyText;
        private Brush _bodyBackground;
        private Brush _BodyBorderBrush;
        private FontFamily _BodyFontFamily;
        private FontStyle _BodyFontStyle;
        private FontWeight _BodyFontWeight;
        private double _BodyFontSize;
        private FontStretch _BodyFontStretch;
        public event PropertyChangedEventHandler PropertyChanged;

        public string CaptionText
        {
            get => _CaptionText;
            set
            {
                if (value == _CaptionText) return;
                _CaptionText = value;
                OnPropertyChanged();
            }
        }

        public Brush CaptionBackground
        {
            get => _captionBackground;
            set
            {
                if (Equals(value, _captionBackground)) return;
                _captionBackground = value;
                OnPropertyChanged();
            }
        }

        public Brush BodyBackground
        {
            get => _bodyBackground;
            set
            {
                if (Equals(value, _bodyBackground)) return;
                _bodyBackground = value;
                OnPropertyChanged();
            }
        }

        public FontFamily BodyFontFamily
        {
            get => _BodyFontFamily;
            set
            {
                if (Equals(value, _BodyFontFamily)) return;
                _BodyFontFamily = value;
                OnPropertyChanged();
            }
        }

        public FontStyle BodyFontStyle
        {
            get => _BodyFontStyle;
            set
            {
                if (value.Equals(_BodyFontStyle)) return;
                _BodyFontStyle = value;
                OnPropertyChanged();
            }
        }

        public FontWeight BodyFontWeight
        {
            get => _BodyFontWeight;
            set
            {
                if (value.Equals(_BodyFontWeight)) return;
                _BodyFontWeight = value;
                OnPropertyChanged();
            }
        }

        public double BodyFontSize
        {
            get => _BodyFontSize;
            set
            {
                if (value.Equals(_BodyFontSize)) return;
                _BodyFontSize = value;
                OnPropertyChanged();
            }
        }

        public FontStretch BodyFontStretch
        {
            get => _BodyFontStretch;
            set
            {
                if (value.Equals(_BodyFontStretch)) return;
                _BodyFontStretch = value;
                OnPropertyChanged();
            }
        }

        public FontFamily CaptionFontFamily
        {
            get => _CaptionFontFamily;
            set
            {
                if (Equals(value, _CaptionFontFamily)) return;
                _CaptionFontFamily = value;
                OnPropertyChanged();
            }
        }

        public FontStyle CaptionFontStyle
        {
            get => _CaptionFontStyle;
            set
            {
                if (value.Equals(_CaptionFontStyle)) return;
                _CaptionFontStyle = value;
                OnPropertyChanged();
            }
        }

        public FontWeight CaptionFontWeight
        {
            get => _CaptionFontWeight;
            set
            {
                if (value.Equals(_CaptionFontWeight)) return;
                _CaptionFontWeight = value;
                OnPropertyChanged();
            }
        }

        public double CaptionFontSize
        {
            get => _CaptionFontSize;
            set
            {
                if (value.Equals(_CaptionFontSize)) return;
                _CaptionFontSize = value;
                OnPropertyChanged();
            }
        }

        public FontStretch CaptionFontStretch
        {
            get => _CaptionFontStretch;
            set
            {
                if (value.Equals(_CaptionFontStretch)) return;
                _CaptionFontStretch = value;
                OnPropertyChanged();
            }
        }

        public string BodyText
        {
            get => _BodyText;
            set
            {
                if (value == _BodyText) return;
                _BodyText = value;
                OnPropertyChanged();
            }
        }

        public Brush CaptionForeground
        {
            get => _CaptionForeground;
            set
            {
                if (Equals(value, _CaptionForeground)) return;
                _CaptionForeground = value;
                OnPropertyChanged();
            }
        }

        public Brush BodyForeground
        {
            get => _BodyForeground;
            set
            {
                if (Equals(value, _BodyForeground)) return;
                _BodyForeground = value;
                OnPropertyChanged();
            }
        }

        public Brush BodyBorderBrush
        {
            get => _BodyBorderBrush;
            set
            {
                if (Equals(value, _BodyBorderBrush)) return;
                _BodyBorderBrush = value;
                OnPropertyChanged();
            }
        }

        internal void SetProperties(PNGroup gr)
        {
            SetProperties(gr, gr.Skinless);
        }

        internal void SetProperties(PNGroup gr, PNSkinlessDetails skl)
        {
            try
            {
                var ctc = new ColorBrightnessToColorConverter();
                var btc = new BrushBrightnessToColorConverter();

                BodyBorderBrush =
                    new SolidColorBrush((Color)ctc.Convert(skl.BackColor, null, "0.8", PNRuntimes.Instance.CultureInvariant));
                BodyBackground = new SolidColorBrush(skl.BackColor);
                var clr1 = (Color)btc.Convert(BodyBackground, null, "1.25", PNRuntimes.Instance.CultureInvariant);
                var clr2 = (Color)btc.Convert(BodyBackground, null, "0.8", PNRuntimes.Instance.CultureInvariant);
                var gradients = new GradientStopCollection { new GradientStop(clr1, 0), new GradientStop(clr2, 1) };
                CaptionBackground = new LinearGradientBrush(gradients)
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1)
                };
                CaptionForeground = new SolidColorBrush(skl.CaptionColor);
                CaptionFontFamily = skl.CaptionFont.FontFamily;
                CaptionFontSize = skl.CaptionFont.FontSize;
                CaptionFontStretch = skl.CaptionFont.FontStretch;
                CaptionFontStyle = skl.CaptionFont.FontStyle;
                CaptionFontWeight = skl.CaptionFont.FontWeight;
                CaptionText = PNLang.Instance.GetControlText("previewCaption", "Caption");
                BodyForeground =
                        new SolidColorBrush(Color.FromArgb(gr.FontColor.A, gr.FontColor.R, gr.FontColor.G, gr.FontColor.B));
                BodyText = PNLang.Instance.GetControlText("lblFontSample", "The quick brown fox jumps over the lazy dog");
                var font = PNStatic.FromLogFont(gr.Font);
                BodyFontFamily = font.FontFamily;
                BodyFontSize = font.FontSize;
                BodyFontStretch = font.FontStretch;
                BodyFontStyle = font.FontStyle;
                BodyFontWeight = font.FontWeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
