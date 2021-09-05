using System;
using SkiaSharp;
using Waher.Script.Graphs;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS
{
    internal enum ColorMode
    {
        Rgba,
        Hsl
    }

    internal class FlameState
    {
        internal ScriptNode node;
        internal int[] frequency;
        internal double[] reds;
        internal double[] greens;
        internal double[] blues;
        internal double x;
        internal double y;
        internal double red;
        internal double green;
        internal double blue;
        internal double xMin;
        internal double xMax;
        internal double yMin;
        internal double yMax;
        internal double sx;
        internal double sy;
        internal long N;
		internal long N0;
        internal long NrCalculated = 0;
        internal ColorMode ColorMode;
        internal int superSampling;
        internal int width;
        internal int height;
        internal int size;

        internal FlameState(Random Gen, double xMin, double xMax, double yMin, double yMax,
            int Width, int Height, int SuperSampling, long N, ColorMode ColorMode, ScriptNode Node)
        {
            this.node = Node;
            this.x = Gen.NextDouble();
            this.y = Gen.NextDouble();
            this.red = 0;
            this.green = 0;
            this.blue = 0;
            this.ColorMode = ColorMode;

            this.N = N;
			this.N0 = N;
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.superSampling = SuperSampling;
            this.width = Width * SuperSampling;
            this.height = Height * SuperSampling;

            this.size = this.width * this.height;
            this.sx = this.width / (xMax - xMin);
            this.sy = this.height / (yMax - yMin);

            this.frequency = new int[this.size];
            this.reds = new double[this.size];
            this.greens = new double[this.size];
            this.blues = new double[this.size];
        }

        internal bool IncHistogram()
        {
            if (this.x < this.xMin || this.x > this.xMax || this.y < this.yMin || this.y > this.yMax)
                return --N > 0;

            int xi = (int)((this.x - this.xMin) * this.sx + 0.5);
            int yi = this.height - 1 - (int)((this.y - this.yMin) * this.sy + 0.5);

            if (xi < 0 || xi >= this.width || yi < 0 || yi >= this.height)
                return --N > 0;

            yi *= this.width;
            yi += xi;

            xi = this.frequency[yi];
            if (xi < int.MaxValue)
                this.frequency[yi] = xi + 1;

            switch (this.ColorMode)
            {
                case ColorMode.Rgba:
                    this.reds[yi] = (this.reds[yi] + this.red) * 0.5;
                    this.greens[yi] = (this.greens[yi] + this.green) * 0.5;
                    this.blues[yi] = (this.blues[yi] + this.blue) * 0.5;
                    break;

                case ColorMode.Hsl:
                    double d;
                    double Hue = this.reds[yi];

                    if (this.red < Hue)                           // H
                    {
                        d = Hue - this.red;
                        if (d < 0.5)
                            Hue -= d * 0.5;
                        else
                        {
                            d = 1 - d;
                            Hue += d * 0.5;
                            if (Hue >= 1)
                                Hue -= 1;
                        }
                    }
                    else
                    {
                        d = this.red - Hue;
                        if (d < 0.5)
                            Hue += d * 0.5;
                        else
                        {
                            d = 1 - d;
                            Hue -= d * 0.5;
                            if (Hue < 0)
                                Hue += 1;
                        }
                    }

                    this.reds[yi] = Hue;
                    this.greens[yi] = (this.greens[yi] + this.green) * 0.5; // S
                    this.blues[yi] = (this.blues[yi] + this.blue) * 0.5;    // L
                    break;
            }
            return --N > 0;
        }

        internal void Add(FlameState State)
        {
            int f1, f2;
            long f;
            int i;

            for (i = 0; i < this.size; i++)
            {
                f2 = State.frequency[i];
                if (f2 == 0)
                    continue;

                f1 = this.frequency[i];

                if (f1 == 0)
                {
                    this.frequency[i] = f2;
                    this.reds[i] = State.reds[i];
                    this.greens[i] = State.greens[i];
                    this.blues[i] = State.blues[i];
                }
                else
                {
                    f = f1;
                    f += f2;

                    if (f > int.MaxValue)
                        this.frequency[i] = int.MaxValue;
                    else
                        this.frequency[i] = (int)f;

                    this.reds[i] = (f1 * this.reds[i] + f2 * State.reds[i]) / f;
                    this.greens[i] = (f1 * this.greens[i] + f2 * State.greens[i]) / f;
                    this.blues[i] = (f1 * this.blues[i] + f2 * State.blues[i]) / f;
                }
            }
        }

        internal PixelInformation RenderBitmapRgba(double Gamma, double Vibrancy, bool Preview, SKColor? Background)
        {
            int[] Frequency;
            double[] Reds;
            double[] Greens;
            double[] Blues;

            if (Preview)
            {
                Frequency = (int[])this.frequency.Clone();
                Reds = (double[])this.reds.Clone();
                Greens = (double[])this.greens.Clone();
                Blues = (double[])this.blues.Clone();
            }
            else
            {
                Frequency = this.frequency;
                Reds = this.reds;
                Greens = this.greens;
                Blues = this.blues;
            }

            int Width = this.width / this.superSampling;
            int Height = this.height / this.superSampling;
            int size = Width * Height * 4;
            byte[] rgb = new byte[size];
            int x, y;
            int dst = 0;
            int srcY;
            int src;
            int si, a;
            int rowStep = Width * this.superSampling;
            int rowStep2 = rowStep - this.superSampling;
            int srcYStep = rowStep * this.superSampling;
            int dx, dy;
            int i, j;
            double r, g, b, s, s2;
            int freq, maxfreq = 0;
            double GammaComponent = Gamma * (1 - Vibrancy) + Vibrancy;
            double GammaAlpha = Gamma * Vibrancy + (1 - Vibrancy);
			bool HasBg = Background.HasValue;
			byte BgR, BgG, BgB;

			if (HasBg)
			{
				BgR = Background.Value.Red;
				BgG = Background.Value.Green;
				BgB = Background.Value.Blue;
			}
			else
				BgR = BgG = BgB = 0;

			s2 = Math.Pow(255.0, GammaComponent);

            if (this.superSampling > 1)
            {
                for (y = srcY = 0; y < Height; y++, srcY += srcYStep)
                {
                    for (x = 0, src = srcY; x < Width; x++, src += this.superSampling)
                    {
                        si = src;
                        r = g = b = 0.0;
                        freq = 0;

                        for (dy = 0; dy < this.superSampling; dy++)
                        {
                            for (dx = 0; dx < this.superSampling; dx++, si++)
                            {
                                j = Frequency[si];
                                if (j > 0)
                                {
                                    freq += j;
                                    r += j * Reds[si];
                                    g += j * Greens[si];
                                    b += j * Blues[si];
                                }
                            }

                            si += rowStep2;
                        }

                        if (freq == 0)
                            Frequency[src] = 0;
                        else
                        {
                            if (freq > maxfreq)
                                maxfreq = freq;

                            s = s2 / freq;
                            Frequency[src] = freq;
                            Reds[src] = r * s;
                            Greens[src] = g * s;
                            Blues[src] = b * s;
                        }
                    }
                }
            }
            else
            {
                maxfreq = 0;
                foreach (int F in Frequency)
                {
                    if (F > maxfreq)
                        maxfreq = F;
                }
            }

            if (maxfreq == 0)
                s = 1;
            else
                s = Math.Pow(255.0, GammaAlpha) / Math.Log(maxfreq);

            GammaComponent = 1.0 / GammaComponent;
            GammaAlpha = 1.0 / GammaAlpha;

            if (Vibrancy == 1)          // Only gamma correction on the alpha-channel
            {
                for (y = srcY = 0; y < Height; y++, srcY += srcYStep)
                {
                    for (x = 0, src = srcY; x < Width; x++, src += this.superSampling)
                    {
                        i = Frequency[src];

						if (i == 0)
						{
							if (HasBg)
							{
								rgb[dst++] = BgB;
								rgb[dst++] = BgG;
								rgb[dst++] = BgR;
								rgb[dst++] = 0xff;
							}
							else
							{
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
							}
						}
						else
						{
							if (HasBg)
							{
								a = (int)(Math.Pow(Math.Log(i) * s, GammaAlpha));

								si = (int)Blues[src];
								si = (si * a + BgB * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Greens[src];
								si = (si * a + BgG * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Reds[src];
								si = (si * a + BgR * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								rgb[dst++] = 255;
							}
							else
							{
								si = (int)Blues[src];
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Greens[src];
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Reds[src];
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								a = (int)(Math.Pow(Math.Log(i) * s, GammaAlpha));
								if (a < 0)
									rgb[dst++] = 0;
								else if (a > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)a;
							}
						}
                    }
                }
            }
            else if (Vibrancy == 0)     // Only gamma correction on the individual components.
            {
                for (y = srcY = 0; y < Height; y++, srcY += srcYStep)
                {
                    for (x = 0, src = srcY; x < Width; x++, src += this.superSampling)
                    {
                        i = Frequency[src];

						if (i == 0)
						{
							if (HasBg)
							{
								rgb[dst++] = BgB;
								rgb[dst++] = BgG;
								rgb[dst++] = BgR;
								rgb[dst++] = 0xff;
							}
							else
							{
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
							}
						}
						else
						{
							if (HasBg)
							{
								a = (int)(Math.Log(i) * s);

								si = (int)Math.Pow(Blues[src], GammaComponent);
								si = (si * a + BgB * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Greens[src], GammaComponent);
								si = (si * a + BgG * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Reds[src], GammaComponent);
								si = (si * a + BgR * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								rgb[dst++] = 255;
							}
							else
							{
								si = (int)Math.Pow(Blues[src], GammaComponent);
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Greens[src], GammaComponent);
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Reds[src], GammaComponent);
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								a = (int)(Math.Log(i) * s);
								if (a < 0)
									rgb[dst++] = 0;
								else if (a > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)a;
							}
						}
                    }
                }
            }
            else                        // Interpolated gamma correction on both the components and the alpha channel.
            {
                for (y = srcY = 0; y < Height; y++, srcY += srcYStep)
                {
                    for (x = 0, src = srcY; x < Width; x++, src += this.superSampling)
                    {
                        i = Frequency[src];

						if (i == 0)
						{
							if (HasBg)
							{
								rgb[dst++] = BgB;
								rgb[dst++] = BgG;
								rgb[dst++] = BgR;
								rgb[dst++] = 0xff;
							}
							else
							{
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
							}
						}
						else
						{
							if (HasBg)
							{
								a = (int)(Math.Pow(Math.Log(i) * s, GammaAlpha));

								si = (int)Math.Pow(Blues[src], GammaComponent);
								si = (si * a + BgB * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Greens[src], GammaComponent);
								si = (si * a + BgG * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Reds[src], GammaComponent);
								si = (si * a + BgR * (255 - a) + 128) / 255;
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								rgb[dst++] = 255;
							}
							else
							{
								si = (int)Math.Pow(Blues[src], GammaComponent);
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Greens[src], GammaComponent);
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								si = (int)Math.Pow(Reds[src], GammaComponent);
								if (si < 0)
									rgb[dst++] = 0;
								else if (si > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)si;

								a = (int)(Math.Pow(Math.Log(i) * s, GammaAlpha));
								if (a < 0)
									rgb[dst++] = 0;
								else if (a > 255)
									rgb[dst++] = 255;
								else
									rgb[dst++] = (byte)a;
							}
						}
                    }
                }
            }

            return new PixelInformationRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
        }

        internal PixelInformation RenderBitmapHsl(double Gamma, double LightFactor, bool Preview, SKColor? Background)
        {
            int[] Frequency;
            double[] Hues;
            double[] Saturations;
            double[] Lights;

            if (Preview)
            {
                Frequency = (int[])this.frequency.Clone();
                Hues = (double[])this.reds.Clone();
                Saturations = (double[])this.greens.Clone();
                Lights = (double[])this.blues.Clone();
            }
            else
            {
                Frequency = this.frequency;
                Hues = this.reds;
                Saturations = this.greens;
                Lights = this.blues;
            }

            int Width = this.width / this.superSampling;
            int Height = this.height / this.superSampling;
            int size = Width * Height * 4;
            byte[] rgb = new byte[size];
            int x, y;
            int dst = 0;
            int srcY;
            int src;
            int si;
            int rowStep = Width * this.superSampling;
            int rowStep2 = rowStep - this.superSampling;
            int srcYStep = rowStep * this.superSampling;
            int dx, dy;
            int i, j;
            double H, S, L;
            int freq, maxfreq;
            long R2, G2, B2;
			SKColor cl;
			bool HasBg = Background.HasValue;
			byte BgR, BgG, BgB;

			if (HasBg)
			{
				BgR = Background.Value.Red;
				BgG = Background.Value.Green;
				BgB = Background.Value.Blue;
			}
			else
				BgR = BgG = BgB = 0;

			maxfreq = 0;
            foreach (int F in Frequency)
            {
                if (F > maxfreq)
                    maxfreq = F;
            }

            Gamma = 1.0 / Gamma;

            if (this.superSampling == 1)
            {
                for (y = srcY = 0; y < Height; y++, srcY += srcYStep)
                {
                    for (x = 0, src = srcY; x < Width; x++, src++)
                    {
                        i = Frequency[src];

						if (i == 0)
						{
							if (HasBg)
							{
								rgb[dst++] = BgB;
								rgb[dst++] = BgG;
								rgb[dst++] = BgR;
							}
							else
							{
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
							}

							rgb[dst++] = 0xff;
						}
						else
						{
							H = Hues[src] * 360;
							S = Math.Pow(Saturations[src], Gamma);
							L = Math.Pow((Lights[src] * i) / maxfreq, Gamma) * LightFactor;

							if (S > 1)
								S = 1;

							if (L > 1)
								L = 1;

							cl = Graph.ToColorHSL(H, S, L);
							rgb[dst++] = cl.Blue;
							rgb[dst++] = cl.Green;
							rgb[dst++] = cl.Red;
							rgb[dst++] = 0xff;
						}
                    }
                }
            }
            else
            {
                for (y = srcY = 0; y < Height; y++, srcY += srcYStep)
                {
                    for (x = 0, src = srcY; x < Width; x++, src += this.superSampling)
                    {
                        si = src;
                        R2 = G2 = B2 = 0;
                        freq = 0;

                        for (dy = 0; dy < this.superSampling; dy++)
                        {
                            for (dx = 0; dx < this.superSampling; dx++, si++)
                            {
                                j = Frequency[si];
                                if (j > 0)
                                {
                                    H = Hues[si] * 360;
                                    S = Math.Pow(Saturations[si], Gamma);
                                    L = Math.Pow((Lights[si] * j) / maxfreq, Gamma) * LightFactor;

									if (S > 1)
										S = 1;

									if (L > 1)
										L = 1;

									cl = Graph.ToColorHSL(H, S, L);

                                    freq += j;
                                    R2 += j * cl.Red;
                                    G2 += j * cl.Green;
                                    B2 += j * cl.Blue;
                                }
                            }

                            si += rowStep2;
                        }

                        if (freq == 0)
                        {
							if (HasBg)
							{
								rgb[dst++] = BgB;
								rgb[dst++] = BgG;
								rgb[dst++] = BgR;
							}
							else
							{
								rgb[dst++] = 0;
								rgb[dst++] = 0;
								rgb[dst++] = 0;
							}

							rgb[dst++] = 0xff;
                        }
                        else
                        {
                            R2 /= freq;
                            if (R2 > 255)
                                R2 = 255;

                            G2 /= freq;
                            if (G2 > 255)
                                G2 = 255;

                            B2 /= freq;
                            if (B2 > 255)
                                B2 = 255;

                            rgb[dst++] = (byte)B2;
                            rgb[dst++] = (byte)G2;
                            rgb[dst++] = (byte)R2;
                            rgb[dst++] = 0xff;
                        }
                    }
                }
            }

            return new PixelInformationRaw(SKColorType.Bgra8888, rgb, Width, Height, Width << 2);
		}

	}
}