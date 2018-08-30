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
			if (x <= BigInteger.Zero || x >= this.p)
				throw new ArgumentException("Number not invertible.", nameof(x));

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
	}
}
