using System;
using System.Collections.Generic;
using System.Text;

namespace NCNicoNico.Comment.Primitive
{
    public class Range
    {
        public double A;
        public double B;
        public double Length => this.B - this.A;

        public Range(double A,double B)
        {
            this.A = A;
            this.B = B;
        }

        public bool Check(Range range, bool AEqual = false,bool BEqual=false)
        {
            if ((range.A < this.B || (BEqual && range.A == this.B)) &&
                (this.A < range.B || (AEqual && range.B == this.A)))
            { return false; }
            return true;
        }
    }

    public struct NCSize
    {
        public NCSize(double w,double h)
        {
            this.Width = w;
            this.Height = h;
        }

        public double Width;
        public double Height;
    }

    public struct Coordinate
    {
        public Coordinate(double x, double y) { this.X = x; this.Y = y; }

        public double X;
        public double Y;
    }

    public struct NCColor
    {
        public int R;
        public int G;
        public int B;
        public double Alpha;

        public NCColor(int r, int g, int b, double alpha = 1.0)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.Alpha = alpha;
        }

        public NCColor(string text)
        {
            var color = Parse(text);
            if (color == null) { throw new ArgumentException(); }
            else { this = color.Value; }
        }

        public double AverageBrightness
        {
            get
            {
                return (R + G + B) / 3.0 * Alpha;
            }
        }

        public static NCColor? Parse(string text)
        {
            var regex = new System.Text.RegularExpressions.Regex("#(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})");
            var match = regex.Match(text);
            if (match.Success)
            {
                var hex = System.Globalization.NumberStyles.HexNumber;
                var r = int.Parse(match.Groups["r"].Value, hex);
                var g = int.Parse(match.Groups["g"].Value, hex);
                var b = int.Parse(match.Groups["b"].Value, hex);
                return new NCColor(r, g, b);
            }
            return null;
        }
    }

}
