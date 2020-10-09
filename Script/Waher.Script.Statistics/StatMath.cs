using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Waher.Script.Statistics
{
	/// <summary>
	/// Contains Numerical Methods to compute mathematical functions needed for probabilistic computations.
	/// </summary>
	public static class StatMath
	{
		#region erf

		/// <summary>
		/// Error function erf(x)
		/// </summary>
		/// <param name="x">Argument</param>
		/// <returns>erf(x)</returns>
		public static double Erf(double x)
		{
			double mz2 = -x * x;
			double Sum = x;
			double Product = 1;
			double Term;
			int n = 0;

			do
			{
				n++;
				Product *= mz2 / n;
				Term = x * Product / (2 * n + 1);
				Sum += Term;
			}
			while (Math.Abs(Term) > 1e-10);

			Sum *= erfC;

			return Sum;
		}

		/// <summary>
		/// Error function erf(z)
		/// </summary>
		/// <param name="z">Argument</param>
		/// <returns>erf(z)</returns>
		public static Complex Erf(Complex z)
		{
			Complex mz2 = -z * z;
			Complex Sum = z;
			Complex Product = 1;
			Complex Term;
			int n = 0;

			do
			{
				n++;
				Product *= mz2 / n;
				Term = z * Product / (2 * n + 1);
				Sum += Term;
			}
			while (Complex.Abs(Term) > 1e-10);

			Sum *= erfC;

			return Sum;
		}

		private static readonly double erfC = 2 / Math.Sqrt(Math.PI);

		#endregion

		#region Γ

		/// <summary>
		/// Gamma function Γ(x), for real-valued arguments.
		/// </summary>
		/// <param name="x">Real-valued argument</param>
		/// <returns>Γ(x)</returns>
		public static double Γ(double x)
		{
			// References: 
			// https://rosettacode.org/wiki/Gamma_function#C.23

			if (x < 0.5)
				return Math.PI / (Math.Sin(Math.PI * x) * Γ(1 - x));    // 5.5.3: https://dlmf.nist.gov/5.5
			else
			{
				// 5.11.3: https://dlmf.nist.gov/5.11

				double v = x + 6.5;
				double w = Math.Pow(v, x - 0.5);
				double u = 0.99999999999980993;

				u += 676.5203681218851 / x++;
				u += -1259.1392167224028 / x++;
				u += 771.32342877765313 / x++;
				u += -176.61502916214059 / x++;
				u += 12.507343278686905 / x++;
				u += -0.13857109526572012 / x++;
				u += 9.9843695780195716e-6 / x++;
				u += 1.5056327351493116e-7 / x++;

				return gammaC * w * Math.Exp(-v) * u;
			}
		}

		/// <summary>
		/// Gamma function Γ(x), for real-valued arguments.
		/// </summary>
		/// <param name="z">Real-valued argument</param>
		/// <returns>Γ(x)</returns>
		public static Complex Γ(Complex z)
		{
			// References: 
			// https://rosettacode.org/wiki/Gamma_function#C.23

			if (z.Real < 0.5)
				return Math.PI / (Complex.Sin(Math.PI * z) * Γ(1 - z));    // 5.5.3: https://dlmf.nist.gov/5.5
			else
			{
				// 5.11.3: https://dlmf.nist.gov/5.11

				Complex v = z + 6.5;
				Complex w = Complex.Pow(v, z - 0.5);
				Complex u = 0.99999999999980993;

				u += 676.5203681218851 / z;
				u += -1259.1392167224028 / (z + 1);
				u += 771.32342877765313 / (z + 2);
				u += -176.61502916214059 / (z + 3);
				u += 12.507343278686905 / (z + 4);
				u += -0.13857109526572012 / (z + 5);
				u += 9.9843695780195716e-6 / (z + 6);
				u += 1.5056327351493116e-7 / (z + 7);

				return gammaC * w * Complex.Exp(-v) * u;
			}
		}

		private static readonly double gammaC = Math.Sqrt(2 * Math.PI);

		/// <summary>
		/// Incomplete gamma function γ(a,x)→Γ(a),x→∞
		/// </summary>
		/// <param name="a">First argument</param>
		/// <param name="x">Second argument</param>
		/// <returns>γ(a,x)</returns>
		public static double γ(double a, double x)
		{
			if (x == 0)
				return 0;

			double c = Math.Abs(a);
			if (c > 1.1 && Math.Abs(x) > c)
				return Γ(a) - Γ(a, x);

			return γ(a, x, 1e-10);
		}

		private static double γ(double a, double x, double eps)
		{
			double c = Math.Pow(x, a) * Math.Exp(-x);
			double n = 1;
			double d = a++;
			double Term = n / d;
			double Sum = Term;

			do
			{
				n *= x;
				d *= a++;
				Term = n / d;
				Sum += Term;
			}
			while (Math.Abs(Term) > eps);

			return c * Sum;
		}

		/// <summary>
		/// Incomplete gamma function γ(a,z)→Γ(a),z→∞
		/// </summary>
		/// <param name="a">First argument</param>
		/// <param name="z">Second argument</param>
		/// <returns>γ(a,z)</returns>
		public static Complex γ(Complex a, Complex z)
		{
			if (z == Complex.Zero)
				return Complex.Zero;

			double c = Complex.Abs(a);
			if (c > 1.1 && Complex.Abs(z) > c)
				return Γ(a) - Γ(a, z);

			return γ(a, z, 1e-10);
		}

		private static Complex γ(Complex a, Complex z, double eps)
		{
			Complex c = Complex.Pow(z, a) * Complex.Exp(-z);
			Complex n = 1;
			Complex d = a;
			Complex Term = n / d;
			Complex Sum = Term;

			a += 1;
			do
			{
				n *= z;
				d *= a;
				a += 1;
				Term = n / d;
				Sum += Term;
			}
			while (Complex.Abs(Term) > eps);

			return c * Sum;
		}

		/// <summary>
		/// Incomplete gamma function Γ(a,x), γ(a,x)+Γ(a,x)=Γ(a)
		/// </summary>
		/// <param name="a">First argument</param>
		/// <param name="x">Second argument</param>
		/// <returns>Γ(a,x)</returns>
		public static double Γ(double a, double x)
		{
			if (x == 0)
				return Γ(a);

			double c = Math.Abs(a);
			if (c <= 1.1 || Math.Abs(x) <= c)
				return Γ(a) - γ(a, x);

			return Γ(a, x, 60);
		}

		private static double Γ(double a, double x, int N)
		{
			double n, d, q;
			int i;

			q = 0;
			for (i = N; i > 0; i--)
			{
				d = q + 1 + 2 * i + x - a;
				n = i * (a - i);
				q = n / d;
			}

			n = Math.Pow(x, a) * Math.Exp(-x);
			d = 1 + x - a + q;
			return n / d;
		}

		/// <summary>
		/// Incomplete gamma function Γ(a,z), γ(a,z)+Γ(a,z)=Γ(a)
		/// </summary>
		/// <param name="a">First argument</param>
		/// <param name="z">Second argument</param>
		/// <returns>Γ(a,z)</returns>
		public static Complex Γ(Complex a, Complex z)
		{
			if (z == Complex.Zero)
				return Γ(a);

			double c = Complex.Abs(a);
			if (c <= 1.1 || Complex.Abs(z) <= c)
				return Γ(a) - γ(a, z);

			return Γ(a, z, 60);
		}

		private static Complex Γ(Complex a, Complex z, int N)
		{
			Complex n, d, q;
			int i;

			q = 0;
			for (i = N; i > 0; i--)
			{
				d = q + 1 + 2 * i + z - a;
				n = i * (a - i);
				q = n / d;
			}

			n = Complex.Pow(z, a) * Complex.Exp(-z);
			d = 1 + z - a + q;
			return n / d;
		}

		#endregion

		#region Β

		/// <summary>
		/// Beta-function Β(a,b)
		/// </summary>
		/// <param name="a">First argument</param>
		/// <param name="b">Second argument</param>
		/// <returns>Β(a,b)</returns>
		public static double Β(double a, double b)
		{
			return Γ(a) * Γ(b) / Γ(a + b);  // 5.12.1: https://dlmf.nist.gov/5.12
		}

		/// <summary>
		/// Beta-function Β(a,b)
		/// </summary>
		/// <param name="a">First argument</param>
		/// <param name="b">Second argument</param>
		/// <returns>Β(a,b)</returns>
		public static Complex Β(Complex a, Complex b)
		{
			return Γ(a) * Γ(b) / Γ(a + b);  // 5.12.1: https://dlmf.nist.gov/5.12
		}

		#endregion
	}
}
