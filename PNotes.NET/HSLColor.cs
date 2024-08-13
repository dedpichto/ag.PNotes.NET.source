// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

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

using System;
using System.Windows.Media;

namespace PNotes.NET
{
    internal class HSLColor
    {
        internal HSLColor(double h, double s, double l)
        {
            Hue = h;
            Saturation = s;
            Lightness = l;
        }

        internal double Hue { get; set; }

        internal double Lightness { get; set; }

        internal double Saturation { get; set; }

        internal RGBColor RGBColor()
        {
            double r, g, b, m2;
            if (Lightness <= 0.5)
                m2 = Lightness * (1 + Saturation);
            else
                m2 = Lightness + Saturation - Lightness * Saturation;

            double m1 = 2 * Lightness - m2;
            if (Math.Abs(Saturation - 0) < double.Epsilon)
            {
                r = Lightness;
                g = Lightness;
                b = Lightness;
            }
            else
            {
                r = RGB(Hue + 120, m1, m2);
                g = RGB(Hue, m1, m2);
                b = RGB(Hue - 120, m1, m2);
            }
            return new RGBColor(r * 255, g * 255, b * 255);
        }

        private double RGB(double h, double m1, double m2)
        {
            double value;

            if (h < 0)
                h += 360;
            else if (h > 360)
                h -= 360;
            if (h < 60)
                value = m1 + (m2 - m1) * h / 60.0;
            else if (60 <= h && h < 180)
                value = m2;
            else if (180 <= h && h < 240)
                value = m1 + (m2 - m1) * (240 - h) / 60.0;
            else //240 <= H && H <= 360
                value = m1;

            return value;
        }
    }

    internal class RGBColor
    {
        private double _r, _g, _b;

        internal RGBColor(double r, double g, double b)
        {
            _r = (r < 0) ? 0 : (r > 255) ? 255 : r;
            _g = (g < 0) ? 0 : (g > 255) ? 255 : g;
            _b = (b < 0) ? 0 : (b > 255) ? 255 : b;
        }

        internal RGBColor(Color c)
        {
            _r = c.R;
            _g = c.G;
            _b = c.B;
        }

        internal double R
        {
            get => _r;
            set => _r = (value < 0) ? 0 : (value > 255) ? 255 : value;
        }

        internal double G
        {
            get => _g;
            set => _g = (value < 0) ? 0 : (value > 255) ? 255 : value;
        }

        internal double B
        {
            get => _b;
            set => _b = (value < 0) ? 0 : (value > 255) ? 255 : value;
        }

        internal Color Color => Color.FromArgb(255, (byte)Math.Round(_r, MidpointRounding.AwayFromZero),
            (byte)Math.Round(_g, MidpointRounding.AwayFromZero),
            (byte)Math.Round(_b, MidpointRounding.AwayFromZero));
    }
}
