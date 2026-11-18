using System;
using System.Numerics;

namespace Waher.Things.Matter.Crypto
{
	/// <summary>
	/// Affine point on the NIST P-256 (secp256r1) curve.
	/// </summary>
	public readonly struct P256Point
	{
		/// <summary>
		/// X-coordinate.
		/// </summary>
		public readonly BigInteger X;

		/// <summary>
		/// Y-coordinate.
		/// </summary>
		public readonly BigInteger Y;

		/// <summary>
		/// If the point is the point at infinity.
		/// </summary>
		public readonly bool IsInfinity;

		/// <summary>
		/// Affine point on the NIST P-256 (secp256r1) curve.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		public P256Point(BigInteger X, BigInteger Y)
		{
			this.X = X;
			this.Y = Y;
			this.IsInfinity = false;
		}

		private P256Point(bool Infinity)
		{
			this.X = BigInteger.Zero;
			this.Y = BigInteger.Zero;
			this.IsInfinity = Infinity;
		}

		/// <summary>
		/// Point at infinity.
		/// </summary>
		public static readonly P256Point Infinity = new P256Point(true);
	}

	/// <summary>
	/// Arithmetic on the NIST P-256 (secp256r1) elliptic curve, used for ECDH and SPAKE2+.
	/// </summary>
	public static class P256
	{
		/// <summary>
		/// Prime of the underlying field.
		/// </summary>
		public static readonly BigInteger P = Parse("ffffffff00000001000000000000000000000000ffffffffffffffffffffffff");

		/// <summary>
		/// Order of the base point.
		/// </summary>
		public static readonly BigInteger N = Parse("ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551");

		/// <summary>
		/// Curve parameter b.
		/// </summary>
		public static readonly BigInteger B = Parse("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b");

		/// <summary>
		/// Base point G.
		/// </summary>
		public static readonly P256Point G = new P256Point(
			Parse("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"),
			Parse("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5"));

		private static readonly BigInteger Three = new BigInteger(3);

		/// <summary>
		/// Parses a hexadecimal big-endian unsigned integer.
		/// </summary>
		/// <param name="Hex">Hexadecimal string.</param>
		/// <returns>Integer.</returns>
		public static BigInteger Parse(string Hex)
		{
			return FromBigEndian(MatterCrypto.FromHex(Hex));
		}

		/// <summary>
		/// Converts a big-endian unsigned byte array to an integer.
		/// </summary>
		/// <param name="Data">Big-endian bytes.</param>
		/// <returns>Integer.</returns>
		public static BigInteger FromBigEndian(byte[] Data)
		{
			byte[] Bin = new byte[Data.Length + 1];
			int i, c = Data.Length;

			for (i = 0; i < c; i++)
				Bin[i] = Data[c - 1 - i];

			return new BigInteger(Bin);
		}

		/// <summary>
		/// Converts a non-negative integer to a fixed-length big-endian byte array.
		/// </summary>
		/// <param name="Value">Value.</param>
		/// <param name="Length">Length of output.</param>
		/// <returns>Big-endian bytes.</returns>
		public static byte[] ToBigEndian(BigInteger Value, int Length)
		{
			if (Value.Sign < 0)
				throw new ArgumentException("Value must be non-negative.", nameof(Value));

			byte[] Bin = Value.ToByteArray();
			byte[] Result = new byte[Length];
			int i, c = Bin.Length;

			if (c > Length)
			{
				for (i = Length; i < c; i++)
				{
					if (Bin[i] != 0)
						throw new ArgumentException("Value too large.", nameof(Value));
				}

				c = Length;
			}

			for (i = 0; i < c; i++)
				Result[Length - 1 - i] = Bin[i];

			return Result;
		}

		private static BigInteger Mod(BigInteger a)
		{
			a %= P;
			if (a.Sign < 0)
				a += P;
			return a;
		}

		/// <summary>
		/// Computes a modulo N, as a non-negative number.
		/// </summary>
		/// <param name="a">Value.</param>
		/// <returns>a mod N</returns>
		public static BigInteger ModN(BigInteger a)
		{
			a %= N;
			if (a.Sign < 0)
				a += N;
			return a;
		}

		/// <summary>
		/// Checks if a point lies on the curve.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>If point is on curve.</returns>
		public static bool IsOnCurve(P256Point Point)
		{
			if (Point.IsInfinity)
				return false;

			if (Point.X.Sign < 0 || Point.X >= P || Point.Y.Sign < 0 || Point.Y >= P)
				return false;

			BigInteger Left = Mod(Point.Y * Point.Y);
			BigInteger Right = Mod(Point.X * Point.X * Point.X - Three * Point.X + B);

			return Left == Right;
		}

		/// <summary>
		/// Encodes a point in uncompressed form (0x04 || X || Y).
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>65-byte encoding.</returns>
		public static byte[] Encode(P256Point Point)
		{
			if (Point.IsInfinity)
				throw new ArgumentException("Cannot encode the point at infinity.", nameof(Point));

			byte[] Result = new byte[65];
			Result[0] = 0x04;
			Array.Copy(ToBigEndian(Point.X, 32), 0, Result, 1, 32);
			Array.Copy(ToBigEndian(Point.Y, 32), 0, Result, 33, 32);
			return Result;
		}

		/// <summary>
		/// Decodes a point, in uncompressed (65 bytes) or compressed (33 bytes) form.
		/// </summary>
		/// <param name="Encoded">Encoded point.</param>
		/// <returns>Point.</returns>
		/// <exception cref="MatterSecurityException">If the point is invalid.</exception>
		public static P256Point Decode(byte[] Encoded)
		{
			P256Point Result;

			if (!(Encoded is null) && Encoded.Length == 65 && Encoded[0] == 0x04)
			{
				Result = new P256Point(
					FromBigEndian(MatterCrypto.Slice(Encoded, 1, 32)),
					FromBigEndian(MatterCrypto.Slice(Encoded, 33, 32)));
			}
			else if (!(Encoded is null) && Encoded.Length == 33 && (Encoded[0] == 0x02 || Encoded[0] == 0x03))
			{
				BigInteger X = FromBigEndian(MatterCrypto.Slice(Encoded, 1, 32));
				BigInteger Y2 = Mod(X * X * X - Three * X + B);
				BigInteger Y = BigInteger.ModPow(Y2, (P + 1) / 4, P);

				if (Mod(Y * Y) != Y2)
					throw new MatterSecurityException("Invalid compressed point.");

				if (Y.IsEven != (Encoded[0] == 0x02))
					Y = P - Y;

				Result = new P256Point(X, Y);
			}
			else
				throw new MatterSecurityException("Invalid point encoding.");

			if (!IsOnCurve(Result))
				throw new MatterSecurityException("Point is not on the P-256 curve.");

			return Result;
		}

		/// <summary>
		/// Negates a point.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>-Point</returns>
		public static P256Point Negate(P256Point Point)
		{
			if (Point.IsInfinity)
				return Point;

			return new P256Point(Point.X, Point.Y.IsZero ? BigInteger.Zero : P - Point.Y);
		}

		/// <summary>
		/// Adds two points.
		/// </summary>
		/// <param name="A">First point.</param>
		/// <param name="B">Second point.</param>
		/// <returns>A+B</returns>
		public static P256Point Add(P256Point A, P256Point B)
		{
			return ToAffine(AddJ(ToJacobian(A), ToJacobian(B)));
		}

		/// <summary>
		/// Multiplies a point with a scalar.
		/// </summary>
		/// <param name="Scalar">Scalar.</param>
		/// <param name="Point">Point.</param>
		/// <returns>Scalar*Point</returns>
		public static P256Point Multiply(BigInteger Scalar, P256Point Point)
		{
			Scalar = ModN(Scalar);

			if (Scalar.IsZero || Point.IsInfinity)
				return P256Point.Infinity;

			// Montgomery ladder

			Jacobian R0 = Jacobian.Infinity;
			Jacobian R1 = ToJacobian(Point);
			byte[] Bits = ToBigEndian(Scalar, 32);
			int i, j;

			for (i = 0; i < 32; i++)
			{
				byte b = Bits[i];

				for (j = 7; j >= 0; j--)
				{
					if (((b >> j) & 1) != 0)
					{
						R0 = AddJ(R0, R1);
						R1 = DoubleJ(R1);
					}
					else
					{
						R1 = AddJ(R0, R1);
						R0 = DoubleJ(R0);
					}
				}
			}

			return ToAffine(R0);
		}

		/// <summary>
		/// Multiplies the base point with a scalar.
		/// </summary>
		/// <param name="Scalar">Scalar.</param>
		/// <returns>Scalar*G</returns>
		public static P256Point MultiplyBase(BigInteger Scalar)
		{
			return Multiply(Scalar, G);
		}

		/// <summary>
		/// Computes an ECDH shared secret (X-coordinate of the shared point).
		/// </summary>
		/// <param name="PrivateKey">Private key (32 bytes, big-endian).</param>
		/// <param name="PeerPublicKey">Public key of peer (uncompressed, 65 bytes).</param>
		/// <returns>32-byte shared secret.</returns>
		public static byte[] Ecdh(byte[] PrivateKey, byte[] PeerPublicKey)
		{
			P256Point Peer = Decode(PeerPublicKey);
			P256Point Shared = Multiply(FromBigEndian(PrivateKey), Peer);

			if (Shared.IsInfinity)
				throw new MatterSecurityException("Invalid ECDH result.");

			return ToBigEndian(Shared.X, 32);
		}

		/// <summary>
		/// Generates a random scalar in the range [1, N-1].
		/// </summary>
		/// <returns>Random scalar.</returns>
		public static BigInteger RandomScalar()
		{
			while (true)
			{
				byte[] Bin = MatterCrypto.RandomBytes(32);
				BigInteger k = FromBigEndian(Bin);

				if (!k.IsZero && k < N)
					return k;
			}
		}

		#region Jacobian coordinates

		private readonly struct Jacobian
		{
			public readonly BigInteger X;
			public readonly BigInteger Y;
			public readonly BigInteger Z;

			public Jacobian(BigInteger X, BigInteger Y, BigInteger Z)
			{
				this.X = X;
				this.Y = Y;
				this.Z = Z;
			}

			public bool IsInfinity => this.Z.IsZero;

			public static readonly Jacobian Infinity = new Jacobian(BigInteger.One, BigInteger.One, BigInteger.Zero);
		}

		private static Jacobian ToJacobian(P256Point P)
		{
			if (P.IsInfinity)
				return Jacobian.Infinity;
			else
				return new Jacobian(P.X, P.Y, BigInteger.One);
		}

		private static P256Point ToAffine(Jacobian J)
		{
			if (J.IsInfinity)
				return P256Point.Infinity;

			BigInteger ZInv = BigInteger.ModPow(J.Z, P - 2, P);
			BigInteger ZInv2 = Mod(ZInv * ZInv);
			BigInteger ZInv3 = Mod(ZInv2 * ZInv);

			return new P256Point(Mod(J.X * ZInv2), Mod(J.Y * ZInv3));
		}

		private static Jacobian DoubleJ(Jacobian A)
		{
			if (A.IsInfinity || A.Y.IsZero)
				return Jacobian.Infinity;

			// dbl-2001-b (a = -3)

			BigInteger Delta = Mod(A.Z * A.Z);
			BigInteger Gamma = Mod(A.Y * A.Y);
			BigInteger Beta = Mod(A.X * Gamma);
			BigInteger Alpha = Mod(Three * (A.X - Delta) * (A.X + Delta));
			BigInteger X3 = Mod(Alpha * Alpha - 8 * Beta);
			BigInteger Z3 = Mod((A.Y + A.Z) * (A.Y + A.Z) - Gamma - Delta);
			BigInteger Y3 = Mod(Alpha * (4 * Beta - X3) - 8 * Gamma * Gamma);

			return new Jacobian(X3, Y3, Z3);
		}

		private static Jacobian AddJ(Jacobian A, Jacobian B)
		{
			if (A.IsInfinity)
				return B;

			if (B.IsInfinity)
				return A;

			// add-2007-bl

			BigInteger Z1Z1 = Mod(A.Z * A.Z);
			BigInteger Z2Z2 = Mod(B.Z * B.Z);
			BigInteger U1 = Mod(A.X * Z2Z2);
			BigInteger U2 = Mod(B.X * Z1Z1);
			BigInteger S1 = Mod(A.Y * B.Z * Z2Z2);
			BigInteger S2 = Mod(B.Y * A.Z * Z1Z1);
			BigInteger H = Mod(U2 - U1);
			BigInteger r = Mod(2 * (S2 - S1));

			if (H.IsZero)
			{
				if (r.IsZero)
					return DoubleJ(A);
				else
					return Jacobian.Infinity;
			}

			BigInteger I = Mod(4 * H * H);
			BigInteger J = Mod(H * I);
			BigInteger V = Mod(U1 * I);
			BigInteger X3 = Mod(r * r - J - 2 * V);
			BigInteger Y3 = Mod(r * (V - X3) - 2 * S1 * J);
			BigInteger Z3 = Mod(((A.Z + B.Z) * (A.Z + B.Z) - Z1Z1 - Z2Z2) * H);

			return new Jacobian(X3, Y3, Z3);
		}

		#endregion
	}
}
