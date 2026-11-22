namespace AntWpf
{
    using System;
    using System.Windows.Media;

    public static class ColorPalette
    {
        #region Fields

        private const int hueStep = 2;

        private const int saturationStep = 16;

        private const int saturationStep2 = 5;

        private const int brightnessStep1 = 5;

        private const int brightnessStep2 = 15;

        private const int lightColorCount = 5;

        private const int darkColorCount = 4;

        // Opacity of the base color (light palette index 6) in the dark palette.
        private const double darkBaseOpacity = 0.85;

        // @ant-design/colors darkColorMap: light palette index (0-based) mixed over the background with the opacity.
        private static readonly (int Index, double Opacity)[] darkColorMap =
        {
            (7, 0.15), (6, 0.25), (5, 0.3), (5, 0.45), (5, 0.65), (5, 0.85), (4, 0.9), (3, 0.95), (2, 0.97), (1, 0.98)
        };

        // Ant Design v5 dark algorithm (generateColorPalettes): color token 1..10 -> dark palette index.
        private static readonly int[] darkTokenMap = { 0, 1, 2, 3, 6, 5, 4, 6, 5, 4 };

        #endregion

        #region Public Methods

        public static Color Toning(Color color, int index)
        {
            bool isLight = index <= 6;

            HSV hsv = Color2Hsv(color);
            int i = isLight ? lightColorCount + 1 - index : index - lightColorCount - 1;

            return Hsv2Color(GetHue(hsv, i, isLight), GetSaturation(hsv, i, isLight), GetValue(hsv, i, isLight), color.A);
        }

        /// <summary>
        /// Dark theme counterpart of <see cref="Toning"/>, following the Ant Design v5 dark algorithm.
        /// </summary>
        /// <param name="color">A dark theme color, e.g. the dark PrimaryColor (#1668DC).</param>
        /// <param name="index">Same meaning as in <see cref="Toning"/>: 5 = hover, 6 = base, 7 = active.</param>
        /// <param name="background">The dark container background the palette is mixed over (#141414).</param>
        /// <remarks>
        /// The dark color is mix(background, seed, 0.85) of a light seed color. The seed is recovered, its
        /// light palette is generated with <see cref="Toning"/>, mixed over <paramref name="background"/>
        /// and picked the way the v5 dark algorithm maps color tokens. Because the dark color is already
        /// rounded, results can differ from Ant Design by 1 per channel.
        /// </remarks>
        public static Color ToningDark(Color color, int index, Color background)
        {
            if (index < 1 || index > darkTokenMap.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 1 and 10.");
            }

            var seed = new Color
            {
                A = color.A,
                R = Unmix(background.R, color.R, darkBaseOpacity),
                G = Unmix(background.G, color.G, darkBaseOpacity),
                B = Unmix(background.B, color.B, darkBaseOpacity)
            };

            var (lightIndex, opacity) = darkColorMap[darkTokenMap[index - 1]];
            var light = Toning(seed, lightIndex + 1);

            return new Color
            {
                A = color.A,
                R = Mix(background.R, light.R, opacity),
                G = Mix(background.G, light.G, opacity),
                B = Mix(background.B, light.B, opacity)
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// tinycolor.mix: background + (color - background) * amount, per channel.
        /// </summary>
        private static byte Mix(byte background, byte color, double amount)
        {
            return ToByte(background + (color - background) * amount);
        }

        /// <summary>
        /// Inverse of <see cref="Mix"/>: the color that mixed over background with amount gives mixed.
        /// </summary>
        private static byte Unmix(byte background, byte mixed, double amount)
        {
            return ToByte(background + (mixed - background) / amount);
        }

        private static byte ToByte(double value)
        {
            return (byte)Math.Max(0, Math.Min(255, Math.Round(value, MidpointRounding.AwayFromZero)));
        }

        private static double GetHue(HSV hsv, int i, bool isLight)
        {
            double hue;

            if (hsv.H >= 60 && hsv.H <= 240)
            {
                hue = isLight ? hsv.H - hueStep * i : hsv.H + hueStep * i;
            }
            else
            {
                hue = isLight ? hsv.H + hueStep * i : hsv.H - hueStep * i;
            }

            if (hue < 0)
            {
                hue += 360;
            }
            else if (hue >= 360)
            {
                hue -= 360;
            }

            return Math.Round(hue);
        }
        private static double GetSaturation(HSV hsv, int i, bool isLight)
        {
            double saturation;

            if (isLight)
            {
                saturation = Math.Round(hsv.S * 100) - saturationStep * i;
            }
            else if (i == darkColorCount)
            {
                saturation = Math.Round(hsv.S * 100) + saturationStep;
            }
            else
            {
                saturation = Math.Round(hsv.S * 100) + saturationStep2 * i;
            }

            if (saturation > 100)
            {
                saturation = 100;
            }

            if (isLight && i == lightColorCount && saturation > 10)
            {
                saturation = 10;
            }

            if (saturation < 6)
            {
                saturation = 6;
            }

            return Math.Round(saturation);
        }

        private static double GetValue(HSV hsv, int i, bool isLight)
        {
            if (isLight)
            {
                return Math.Round(hsv.V * 100) + brightnessStep1 * i;
            }
            return Math.Round(hsv.V * 100) - brightnessStep2 * i;
        }

        private static Color Hsv2Color(double h, double s, double v, byte a)
        {
            h = Bound(h, 360) * 6;
            s = Bound(s, 100);
            v = Bound(v, 100);

            double i = Math.Floor(h);
            double f = h - i;

            double p = v * (1 - s),
                   q = v * (1 - f * s),
                   t = v * (1 - (1 - f) * s);

            int mod = (int)(i % 6);
            double r = new double[6] { v, q, p, p, t, v }[mod],
                   g = new double[6] { t, v, v, q, p, p }[mod],
                   b = new double[6] { p, p, t, v, v, q }[mod];

            r = Math.Min(255, Math.Max(0, r * 255));
            g = Math.Min(255, Math.Max(0, g * 255));
            b = Math.Min(255, Math.Max(0, b * 255));

            return new Color() { R = (byte)Math.Round(r), G = (byte)Math.Round(g), B = (byte)Math.Round(b), A = a };
        }

        private static HSV Color2Hsv(Color color)
        {
            double r = Bound(color.R, 255),
                   g = Bound(color.G, 255),
                   b = Bound(color.B, 255);

            double max = Math.Max(r, Math.Max(g, b)),
                   min = Math.Min(r, Math.Min(g, b));

            double h = 0,
                   v = max,
                   d = max - min;
            double s = max == 0 ? 0 : d / max;

            if (max != min)
            {
                if (max == r)
                {
                    h = (g - b) / d + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / d + 2;
                }
                else if (max == b)
                {
                    h = (r - g) / d + 4;
                }

                h /= 6;
            }

            return new HSV() { H = h * 360, S = s, V = v };
        }

        /// <summary>
        /// Take input from [0, n] and return it as [0, 1]
        /// </summary>
        /// <param name="n"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static double Bound(double n, double max)
        {
            n = Math.Min(max, Math.Max(0, n));

            // Handle floating point rounding errors
            if ((Math.Abs(n - max) < 0.000001))
            {
                return 1;
            }

            // Convert into [0, 1] range if it isn't already
            return (n % max) / max;
        }

        #endregion
    }

    internal struct HSV
    {
        public double H;

        public double S;

        public double V;
    }
}
