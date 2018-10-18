using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Anno1602Icons
{
    public class Program
    {
        public static Bitmap CalculateMedians(string dir, int width, int height)
        {
            var files = Directory.GetFiles(dir);
            var n = files.Length;
            var hueSums = new float[width, height][];
            var lightSums = new float[width, height][];
            var satSums = new float[width, height][];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                hueSums[x, y] = new float[n];
                lightSums[x, y] = new float[n];
                satSums[x, y] = new float[n];
            }

            for (var i = 0; i < n; i++)
            {
                var img = new Bitmap(files[i]);
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    hueSums[x, y][i] = img.GetPixel(x, y).GetHue();
                    lightSums[x, y][i] = img.GetPixel(x, y).GetBrightness();
                    satSums[x, y][i] = img.GetPixel(x, y).GetSaturation();
                }
            }

            var img2 = new Bitmap(width, height);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                Array.Sort(hueSums[x, y]);
                Array.Sort(lightSums[x, y]);
                Array.Sort(satSums[x, y]);
                var h = hueSums[x, y][n / 2];
                var l = lightSums[x, y][n / 2];
                var s = satSums[x, y][n / 2];
                img2.SetPixel(x, y, ColorFromAhsb(255, h, s, l));
            }

            return img2;
        }

        public static Bitmap RemoveBackground(Bitmap img, Bitmap median)
        {
            int x = 0, y = 0;
            var adj = new Queue<Point>();
            adj.Enqueue(new Point(x, y));
            var res = new Bitmap(img);
            while (adj.Count > 0)
            {
                var point = adj.Dequeue();
                x = point.X;
                y = point.Y;
                res.SetPixel(x, y, Color.Transparent);
                if (x < img.Width - 1 && ColorDifference(img.GetPixel(x + 1, y), median.GetPixel(x + 1, y)) < 0.1f &&
                    !adj.Contains(new Point(x + 1, y)))
                    adj.Enqueue(new Point(x + 1, y));
                if (y < img.Height - 1 && ColorDifference(img.GetPixel(x, y + 1), median.GetPixel(x, y + 1)) < 0.1f &&
                    !adj.Contains(new Point(x, y + 1)))
                    adj.Enqueue(new Point(x, y + 1));
                if (x > 0 && ColorDifference(img.GetPixel(x - 1, y), median.GetPixel(x - 1, y)) < 0.1f &&
                    !adj.Contains(new Point(x - 1, y)))
                    adj.Enqueue(new Point(x - 1, y));
                if (y > 0 && ColorDifference(img.GetPixel(x, y - 1), median.GetPixel(x, y - 1)) < 0.1f &&
                    !adj.Contains(new Point(x, y - 1)))
                    adj.Enqueue(new Point(x, y - 1));
            }

            return res;
        }

        public static float ColorDifference(Color c, Color m)
        {
            var diff = 0f;
            diff += Math.Abs((c.GetHue() - m.GetHue()) / 360);
            diff += Math.Abs(c.GetBrightness() - m.GetBrightness());
            diff += Math.Abs(c.GetSaturation() - m.GetSaturation());
            return diff / 3;
        }

        private static void Main(string[] args)
        {
            var dir = @"C:\Users\Jo\Pictures\resources\icons\games\anno1602\blue_background";
            var x = 45;
            var y = 42;
            var median = CalculateMedians(dir, x, y);
            var img2 = RemoveBackground(new Bitmap(dir + "\\" + "schwert.gif"), median);
            img2.Save(@"C:\Users\Jo\Pictures\resources\icons\games\anno1602\blue_background\test\nobg.png");
        }

        public static Color ColorFromAhsb(int a, float h, float s, float b)
        {
            if (0 > a || 255 < a) throw new ArgumentOutOfRangeException("a", a, "");
            if (0f > h || 360f < h)
                throw new ArgumentOutOfRangeException("h", h,
                    "");
            if (0f > s || 1f < s)
                throw new ArgumentOutOfRangeException("s", s,
                    "");
            if (0f > b || 1f < b)
                throw new ArgumentOutOfRangeException("b", b,
                    "");

            if (0 == s)
                return Color.FromArgb(a, Convert.ToInt32(b * 255),
                    Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b)
            {
                fMax = b - b * s + s;
                fMin = b + b * s - s;
            }
            else
            {
                fMax = b + b * s;
                fMin = b - b * s;
            }

            iSextant = (int) Math.Floor(h / 60f);
            if (300f <= h) h -= 360f;
            h /= 60f;
            h -= 2f * (float) Math.Floor((iSextant + 1f) % 6f / 2f);
            if (0 == iSextant % 2)
                fMid = h * (fMax - fMin) + fMin;
            else
                fMid = fMin - h * (fMax - fMin);

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        }
    }
}