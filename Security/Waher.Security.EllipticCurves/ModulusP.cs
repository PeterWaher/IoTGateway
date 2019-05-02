using System;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Integer arithmetic, modulus a prime.
	/// </summary>
	public class ModulusP
	{
		/// <summary>
		/// 2
		/// </summary>
		public static readonly BigInteger Two = new BigInteger(2);

		/// <summary>
		/// Base prime.
		/// </summary>
		protected readonly BigInteger p;

		/// <summary>
		/// Integer arithmetic, modulus a prime.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		public ModulusP(BigInteger Prime)
		{
			this.p = Prime;
		}

		/// <summary>
		/// Adds two numbers, modulus p
		/// </summary>
		/// <param name="a">Number 1</param>
		/// <param name="b">Number 2</param>
		/// <returns>a+b mod p</returns>
		public BigInteger Add(BigInteger a, BigInteger b)
		{
			BigInteger Sum = a + b;

			while (Sum >= this.p)
				Sum -= this.p;

			return Sum;
		}

		/// <summary>
		/// Subtracts two numbers, modulus p
		/// </summary>
		/// <param name="a">Number 1</param>
		/// <param name="b">Number 2</param>
		/// <returns>a-b mod p</returns>
		public BigInteger Subtract(BigInteger a, BigInteger b)
		{
			BigInteger Diff = a - b;
			if (Diff < BigInteger.Zero)
				return Diff + this.p;
			else if (Diff >= this.p)
				return Diff - this.p;
			else
				return Diff;
		}

		/// <summary>
		/// Multiplies two numbers, modulus p
		/// </summary>
		/// <param name="a">Number 1</param>
		/// <param name="b">Number 2</param>
		/// <returns>a*b mod p</returns>
		public BigInteger Multiply(BigInteger a, BigInteger b)
		{
			return BigInteger.Remainder(a * b, this.p);
		}

		/// <summary>
		/// Divides two numbers, modulus p
		/// </summary>
		/// <param name="a">Number 1</param>
		/// <param name="b">Number 2</param>
		/// <returns>a/b mod p</returns>
		public BigInteger Divide(BigInteger a, BigInteger b)
		{
			b = this.Invert(b);
			return BigInteger.Remainder(a * b, this.p);
		}

		/// <summary>
		/// Inverts a number in the field Z[p].
		/// </summary>
		/// <param name="x">Number to invert.</param>
		/// <returns>x^-1 mod p</returns>
		public BigInteger Invert(BigInteger x)
		{
			if (x < BigInteger.Zero)
			{
				x = BigInteger.Remainder(x, p);
				if (x < BigInteger.Zero)
					x += p;
			}
			else if (x >= p)
				x = BigInteger.Remainder(x, p);

			BigInteger i = this.p;
			BigInteger j = x;
			BigInteger y1 = BigInteger.One;
			BigInteger y2 = BigInteger.Zero;
			BigInteger q, y;

			do
			{
				q = BigInteger.DivRem(i, j, out BigInteger r);
				y = y2 - y1 * q;
				i = j;
				j = r;
				y2 = y1;
				y1 = y;
			}
			while (!j.IsZero);

			if (!i.IsOne)
				throw new ArgumentException("Number not invertible.", nameof(x));

			BigInteger Result = BigInteger.Remainder(y2, this.p);
			if (Result < BigInteger.Zero)
				Result += this.p;

			return Result;
		}

		/// <summary>
		/// Computes sqrt(N) mod p.
		/// </summary>
		/// <param name="N">Number</param>
		/// <returns>Square root of <paramref name="N"/> mod p.</returns>
		public BigInteger SqrtModP(BigInteger N)
		{
			return SqrtModP(N, this.p);
		}

		/// <summary>
		/// Computes sqrt(N) mod p.
		/// </summary>
		/// <param name="N">Number</param>
		/// <param name="p">Prime</param>
		/// <returns>Square root of <paramref name="N"/> mod p.</returns>
		public static BigInteger SqrtModP(BigInteger N, BigInteger p)
		{
			// See: https://en.wikipedia.org/wiki/Tonelli–Shanks_algorithm

			if (N < BigInteger.Zero)
			{
				N = BigInteger.Remainder(N, p);
				if (N < BigInteger.Zero)
					N += p;
			}
			else if (N >= p)
				N = BigInteger.Remainder(N, p);

			BigInteger pm1d2 = (p - 1) / 2;
			if (BigInteger.ModPow(N, pm1d2, p) != BigInteger.One)
				throw new ArgumentException("No root available.", nameof(N));

			BigInteger z = p - 1;
			BigInteger m = z;
			int s = 0;

			while (m.IsEven)
			{
				s++;
				m >>= 1;
			}

			while (BigInteger.ModPow(z, pm1d2, p) == BigInteger.One)
			{
				z--;
				if (z.IsZero)
					throw new InvalidOperationException("Nonresidue not found.");
			}

			BigInteger c = BigInteger.ModPow(z, m, p);
			BigInteger c2 = BigInteger.Remainder(c * c, p);
			BigInteger u = BigInteger.ModPow(N, m, p);
			BigInteger v = BigInteger.ModPow(N, (m + 1) / 2, p);

			while (--s > 0)
			{
				if (BigInteger.ModPow(u, BigInteger.Pow(2, s - 1), p) != BigInteger.One)
				{
					u = BigInteger.Remainder(u * c2, p);
					v = BigInteger.Remainder(v * c, p);
				}

				c = c2;
				c2 = BigInteger.Remainder(c * c, p);
			}

			return v;
		}

		/// <summary>
		/// Calculates the number of bits used.
		/// </summary>
		/// <param name="n">Value</param>
		/// <returns>Number of bits used by value.</returns>
		public static int CalcBits(BigInteger n)
		{
			if (n.IsZero)
				return 0;

			byte[] A = n.ToByteArray();

			int c = A.Length - 1;
			int i = c << 3;
			byte b = A[c];

			while (b>0)
			{
				i++;
				b >>= 1;
			}

			return i;
		}

	}
}
