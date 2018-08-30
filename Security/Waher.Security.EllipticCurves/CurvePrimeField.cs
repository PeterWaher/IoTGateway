using System;
using System.Security.Cryptography;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field.
	/// </summary>
	public abstract class CurvePrimeField
	{
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private static readonly BigInteger Two = new BigInteger(2);
		private readonly BigInteger p;
		private readonly PointOnCurve g;
		private readonly BigInteger a;
		private readonly BigInteger n;
		private BigInteger d;
		private PointOnCurve publicKey;
		private readonly int orderBits;
		private readonly int orderBytes;
		private readonly byte msbMask;

		/// <summary>
		/// Base class of Elliptic curves over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">a Coefficient in the definition of the curve E:	y^2=x^3+a*x+b</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="OrderBits">Number of bits used to encode order.</param>
		public CurvePrimeField(BigInteger Prime, PointOnCurve BasePoint, BigInteger A, BigInteger Order, int OrderBits)
		{
			if (Prime <= BigInteger.One)
				throw new ArgumentException("Invalid prime base.", nameof(Prime));

			this.p = Prime;
			this.g = BasePoint;
			this.a = A;
			this.n = Order;
			this.orderBits = OrderBits;
			this.orderBytes = (OrderBits + 7) >> 3;

			this.msbMask = 0xff;
			int MaskBits = (8 - OrderBits) & 7;
			if (MaskBits == 0)
			{
				this.orderBytes++;
				this.msbMask = 0;
			}
			else
				this.msbMask >>= MaskBits;

			this.GenerateKeys();
		}

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="P">Point</param>
		/// <returns><paramref name="N"/>*<paramref name="P"/></returns>
		public PointOnCurve ScalarMultiplication(BigInteger N, PointOnCurve P)
		{
			PointOnCurve Result = PointOnCurve.Zero;
			byte[] Bin = N.ToByteArray();
			int i;
			int c = Math.Min(this.orderBits, Bin.Length << 3);

			P = P.Copy();
			for (i = 0; i < c; i++)
			{
				if ((Bin[i >> 3] & (1 << (i & 7))) != 0)
					this.AddTo(Result, P);

				this.Double(P);
			}

			return Result;
		}

		/// <summary>
		/// Adds <paramref name="Q"/> to <paramref name="P"/>.
		/// </summary>
		/// <param name="P">Point 1.</param>
		/// <param name="Q">Point 2.</param>
		/// <returns>P+Q</returns>
		public void AddTo(PointOnCurve P, PointOnCurve Q)
		{
			if (P.IsZero)
			{
				P.X = Q.X;
				P.Y = Q.Y;
				P.IsZero = Q.IsZero;
			}
			else if (!Q.IsZero)
			{
				BigInteger sDividend = this.Subtract(P.Y, Q.Y);
				BigInteger sDivisor = this.Subtract(P.X, Q.X);
				BigInteger s, xR, yR;

				if (sDivisor.IsZero)
				{
					if (sDividend.IsZero)   // P=Q
						this.Double(P);
					else
					{
						P.X = BigInteger.Zero;
						P.Y = BigInteger.Zero;
						P.IsZero = true;
					}
				}
				else
				{
					s = this.Divide(sDividend, sDivisor);
					xR = this.Subtract(this.Multiply(s, s), this.Add(P.X, Q.X));
					yR = this.Add(P.Y, this.Multiply(s, this.Subtract(xR, P.X)));

					P.X = xR;
					P.Y = this.p - yR;
				}
			}
		}

		/// <summary>
		/// Doubles a point on the curve.
		/// </summary>
		/// <param name="P">Point</param>
		public void Double(PointOnCurve P)
		{
			if (!P.IsZero)
			{
				BigInteger sDividend = this.Add(3 * this.Multiply(P.X, P.X), this.a);
				BigInteger sDivisor = this.Multiply(Two, P.Y);

				BigInteger s = this.Divide(sDividend, sDivisor);
				BigInteger xR = this.Subtract(this.Multiply(s, s), this.Add(P.X, P.X));
				BigInteger yR = this.Add(P.Y, this.Multiply(s, this.Subtract(xR, P.X)));

				P.X = xR;
				P.Y = this.p - yR;
			}
		}

		private BigInteger Add(BigInteger a, BigInteger b)
		{
			BigInteger Sum = a + b;
			if (Sum >= this.p)
				return BigInteger.Remainder(Sum, this.p);
			else
				return Sum;
		}

		private BigInteger Subtract(BigInteger a, BigInteger b)
		{
			BigInteger Diff = a - b;
			if (Diff < BigInteger.Zero)
				Diff += this.p;
			if (Diff >= this.p)
				return BigInteger.Remainder(Diff, this.p);
			else
				return Diff;
		}

		private BigInteger Multiply(BigInteger a, BigInteger b)
		{
			return BigInteger.Remainder(a * b, this.p);
		}

		private BigInteger Divide(BigInteger a, BigInteger b)
		{
			b = this.Invert(b);
			return BigInteger.Remainder(a * b, this.p);
		}

		/// <summary>
		/// Generates a new Private Key.
		/// </summary>
		public void GenerateKeys()
		{
			byte[] B = new byte[this.orderBytes];
			BigInteger D;

			do
			{
				lock (rnd)
				{
					rnd.GetBytes(B);
				}

				B[this.orderBytes - 1] &= this.msbMask;

				D = new BigInteger(B);
			}
			while (D.IsZero || D >= this.n);

			this.publicKey = this.ScalarMultiplication(D, this.g);

			this.d = D;
		}

		/// <summary>
		/// Calculates the number of bits used.
		/// </summary>
		/// <param name="n">Value</param>
		/// <returns>Number of bits used by value.</returns>
		protected static int CalcBits(BigInteger n)
		{
			if (n.IsZero)
				return 0;

			int i = 0;

			do
			{
				i++;
				n /= Two;
			}
			while (!n.IsZero);

			return i;
		}

		/// <summary>
		/// Converts a sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="BigEndianDWords">Sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>, most significant word first.</param>
		/// <returns><see cref="BigInteger"/> value.</returns>
		protected static BigInteger ToBigInteger(uint[] BigEndianDWords)
		{
			int i, c = BigEndianDWords.Length;
			int j = c << 2;
			byte[] B = new byte[((BigEndianDWords[0] & 0x80000000) != 0) ? j + 1 : j];
			uint k;

			for (i = 0; i < c; i++)
			{
				k = BigEndianDWords[i];

				B[j - 4] = (byte)k;
				k >>= 8;
				B[j - 3] = (byte)k;
				k >>= 8;
				B[j - 2] = (byte)k;
				k >>= 8;
				B[j - 1] = (byte)k;

				j -= 4;
			}

			return new BigInteger(B);
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

			return BigInteger.Remainder(y2, this.p);
		}

		/// <summary>
		/// Public key.
		/// </summary>
		public PointOnCurve PublicKey => this.publicKey;

		/// <summary>
		/// Gets a shared key using the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
		/// </summary>
		/// <param name="RemotePublicKey">Public key of the remote party.</param>
		/// <param name="HashFunction">A Hash function is applied to the derived key to generate the shared secret.
		/// The derived key, as a byte array of equal size as the order of the prime field, ordered by most significant byte first,
		/// is passed on to the hash function before being returned as the shared key.</param>
		/// <returns>Shared secret.</returns>
		public byte[] GetSharedKey(PointOnCurve RemotePublicKey, HashFunction HashFunction)
		{
			PointOnCurve P = this.ScalarMultiplication(this.d, RemotePublicKey);
			byte[] B = P.X.ToByteArray();

			if (B.Length != this.orderBytes)
				Array.Resize<byte>(ref B, this.orderBytes);

			Array.Reverse(B);   // Most significant byte first.

			return Hashes.ComputeHash(HashFunction, B);
		}

	}
}
