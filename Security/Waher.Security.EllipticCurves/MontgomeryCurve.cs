using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Montgomery curves, with biratinal Ewards equivalent 
	/// over a prime field.
	/// </summary>
	public abstract class MontgomeryCurve : CurvePrimeField
	{
		/// <summary>
		/// Base class of Montgomery curves, with biratinal Ewards equivalent 
		/// over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point in (U,V) coordinates.</param>
		/// <param name="A">a Coefficient in the definition of the curve E:	v^2=u^3+A*u^2+u</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		public MontgomeryCurve(BigInteger Prime, PointOnCurve BasePoint,
			BigInteger A, BigInteger Order, int Cofactor)
			: base(Prime, BasePoint, A, Order, Cofactor)
		{
		}

		/// <summary>
		/// Base class of Montgomery curves, with biratinal Ewards equivalent 
		/// over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point in (U,V) coordinates.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="D">Private key.</param>
		public MontgomeryCurve(BigInteger Prime, PointOnCurve BasePoint,
			BigInteger A, BigInteger Order, int Cofactor, BigInteger D)
			: base(Prime, BasePoint, A, Order, Cofactor, D)
		{
		}

		/// <summary>
		/// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
		/// in the birational Edwards curve.
		/// </summary>
		/// <param name="UV">(U,V) coordinates.</param>
		/// <returns>(X,Y) coordinates.</returns>
		public abstract PointOnCurve ToXY(PointOnCurve UV);

		/// <summary>
		/// Converts a pair of (X,Y) coordinates for the birational Edwards curve
		/// to a pair of (U,V) coordinates.
		/// </summary>
		/// <param name="XY">(X,Y) coordinates.</param>
		/// <returns>(U,V) coordinates.</returns>
		public abstract PointOnCurve ToUV(PointOnCurve XY);

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="U">U-coordinate of point</param>
		/// <returns><paramref name="N"/>*<paramref name="U"/></returns>
		public abstract BigInteger ScalarMultiplication(BigInteger N, BigInteger U);

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="P">Point</param>
		/// <returns><paramref name="N"/>*<paramref name="P"/></returns>
		public override PointOnCurve ScalarMultiplication(BigInteger N, PointOnCurve P)
		{
			return new PointOnCurve(this.ScalarMultiplication(N, P.X), BigInteger.Zero);
		}

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="U">U-coordinate of point.</param>
		/// <param name="A24">(A-2)/4</param>
		/// <param name="p">Prime</param>
		/// <param name="Bits">Number of bits</param>
		/// <returns><paramref name="N"/>*<paramref name="U"/></returns>
		public static BigInteger ScalarMultiplication(BigInteger N, BigInteger U,
			BigInteger A24, BigInteger p, int Bits)
		{
			byte[] k = N.ToByteArray();
			BigInteger x1 = U;
			BigInteger x2 = BigInteger.One;
			BigInteger z2 = BigInteger.Zero;
			BigInteger x3 = U;
			BigInteger z3 = BigInteger.One;
			BigInteger A, AA, B, BB, E, C, D, DA, CB;
			int kt;
			int swap = 0;

			kt = Bits >> 3;
			if (k.Length <= kt)
				Array.Resize<byte>(ref k, kt + 1);

			while (--Bits >= 0)
			{
				kt = (k[Bits >> 3] >> (Bits & 7)) & 1;
				swap ^= kt;
				ConditionalSwap(swap, ref x2, ref x3);
				ConditionalSwap(swap, ref z2, ref z3);
				swap = kt;

				A = BigInteger.Remainder(x2 + z2, p);
				AA = BigInteger.ModPow(A, Two, p);
				B = BigInteger.Remainder(x2 - z2, p);
				BB = BigInteger.ModPow(B, Two, p);
				E = BigInteger.Remainder(AA - BB, p);
				C = BigInteger.Remainder(x3 + z3, p);
				D = BigInteger.Remainder(x3 - z3, p);
				DA = BigInteger.Remainder(D * A, p);
				CB = BigInteger.Remainder(C * B, p);

				x3 = BigInteger.ModPow(DA + CB, Two, p);
				z3 = BigInteger.Remainder(x1 * BigInteger.ModPow(DA - CB, Two, p), p);
				x2 = BigInteger.Remainder(AA * BB, p);
				z2 = BigInteger.Remainder(E * (AA + BigInteger.Remainder(A24 * E, p)), p);
			}

			ConditionalSwap(swap, ref x2, ref x3);
			ConditionalSwap(swap, ref z2, ref z3);

			BigInteger Result = BigInteger.Remainder(x2 * BigInteger.ModPow(z2, p - Two, p), p);
			if (Result.Sign < 0)
				Result += p;

			return Result;
		}

		/// <summary>
		/// Swaps <paramref name="I2"/> and <paramref name="I3"/> if 
		/// <paramref name="swap"/> != 0, in a way that minimizes the risk
		/// of a side-channel attack measuring CPU timing, to deduce the bits
		/// used in private keys.
		/// </summary>
		/// <param name="swap">If integers are to be swapped.</param>
		/// <param name="I2">First integer.</param>
		/// <param name="I3">Second integer.</param>
		private static void ConditionalSwap(int swap, ref BigInteger I2, ref BigInteger I3)
		{
			byte[] x2 = I2.ToByteArray();
			byte[] x3 = I3.ToByteArray();
			int i, c = x2.Length, d = x3.Length;
			byte Dummy;
			byte Mask;
			bool Sign;

			if (c < d)
			{
				Sign = (x2[c - 1] & 0x80) != 0;
				Array.Resize<byte>(ref x2, d);

				if (Sign)
				{
					while (c < d)
						x2[c++] = 0xff;
				}
				else
					c = d;
			}
			else if (d < c)
			{
				Sign = (x3[d - 1] & 0x80) != 0;
				Array.Resize<byte>(ref x3, c);

				if (Sign)
				{
					while (d < c)
						x3[d++] = 0xff;
				}
				//else
				//	d = c;
			}

			Mask = (byte)(0xff * swap);

			for (i = 0; i < c; i++)
			{
				Dummy = (byte)(Mask & (x2[i] ^ x3[i]));
				x2[i] ^= Dummy;
				x3[i] ^= Dummy;
			}

			I2 = new BigInteger(x2);
			I3 = new BigInteger(x3);
		}

	}
}
