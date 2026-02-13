using System;
using System.IO;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Implements the Elliptic Curve Digital Signature Algorithm (ECDSA).
	/// </summary>
	public static class ECDSA
	{
		/// <summary>
		/// Signs data using the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Data to be signed.</param>
		/// <param name="BigEndian">Indicates if the signature should be in big-endian format.</param>
		/// <param name="PrivateKey">Private key.</param>
		/// <param name="HashFunction">Hash function to use</param>
		/// <param name="OrderBytes">Number of bytes used to encode an integer.</param>
		/// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
		/// <param name="MsbMask">Mask for most significant byte.</param>
		/// <param name="Curve">Elliptic curve</param>
		/// <returns>Signature</returns>
		public static byte[] Sign(byte[] Data, bool BigEndian, byte[] PrivateKey, 
			HashFunctionArray HashFunction, int OrderBytes, int ScalarBytes, byte MsbMask, 
			PrimeFieldCurve Curve)
		{
			BigInteger e = CalcE(Data, HashFunction, ScalarBytes, MsbMask);
			BigInteger r, s, PrivateKeyInt = EllipticCurve.ToInt(PrivateKey);
			PointOnCurve P1;
			byte[] k;

			do
			{
				do
				{
					k = Curve.GenerateSecret();
					P1 = Curve.ScalarMultiplication(k, Curve.BasePoint, true);
				}
				while (P1.IsXZero);

				r = BigInteger.Remainder(P1.X, Curve.Order);
				s = Curve.ModulusN.Divide(Curve.ModulusN.Add(e,
					Curve.ModulusN.Multiply(r, PrivateKeyInt)), EllipticCurve.ToInt(k));
			}
			while (s.IsZero);

			if (r.Sign < 0)
				r += Curve.Prime;

			P1.Normalize(Curve);

			byte[] Signature = new byte[OrderBytes << 1];

			byte[] S = r.ToByteArray();     // Little endian
			if (S.Length != OrderBytes)
				Array.Resize(ref S, OrderBytes);

			if (BigEndian)
				Array.Reverse(S);

			Buffer.BlockCopy(S, 0, Signature, 0, OrderBytes);

			S = s.ToByteArray();		// Little endian
			if (S.Length != OrderBytes)
				Array.Resize(ref S, OrderBytes);

			if (BigEndian)
				Array.Reverse(S);

			Buffer.BlockCopy(S, 0, Signature, OrderBytes, OrderBytes);

			return Signature;
		}

		private static BigInteger CalcE(byte[] Data, HashFunctionArray HashFunction,
			int ScalarBytes, byte MsbMask)
		{
			byte[] Hash = HashFunction(Data);
			int c = Hash.Length;

			if (c != ScalarBytes)
				Array.Resize(ref Hash, ScalarBytes);

			Hash[ScalarBytes - 1] &= MsbMask;

			return EllipticCurve.ToInt(Hash);
		}

		/// <summary>
		/// Signs data using the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Data to be signed.</param>
		/// <param name="BigEndian">Indicates if the signature should be in big-endian format.</param>
		/// <param name="PrivateKey">Private key.</param>
		/// <param name="HashFunction">Hash function to use</param>
		/// <param name="OrderBytes">Number of bytes used to encode an integer.</param>
		/// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
		/// <param name="MsbMask">Mask for most significant byte.</param>
		/// <param name="Curve">Elliptic curve</param>
		/// <returns>Signature</returns>
		public static byte[] Sign(Stream Data, bool BigEndian, byte[] PrivateKey, HashFunctionStream HashFunction,
			int OrderBytes, int ScalarBytes, byte MsbMask, PrimeFieldCurve Curve)
		{
			BigInteger e = CalcE(Data, HashFunction, ScalarBytes, MsbMask);
			BigInteger r, s, PrivateKeyInt = EllipticCurve.ToInt(PrivateKey);
			PointOnCurve P1;
			byte[] k;

			do
			{
				do
				{
					k = Curve.GenerateSecret();
					P1 = Curve.ScalarMultiplication(k, Curve.BasePoint, true);
				}
				while (P1.IsXZero);

				r = BigInteger.Remainder(P1.X, Curve.Order);
				s = Curve.ModulusN.Divide(Curve.ModulusN.Add(e,
					Curve.ModulusN.Multiply(r, PrivateKeyInt)), EllipticCurve.ToInt(k));
			}
			while (s.IsZero);

			if (r.Sign < 0)
				r += Curve.Prime;

			P1.Normalize(Curve);

			byte[] Signature = new byte[OrderBytes << 1];

			byte[] S = r.ToByteArray();		// Little endian
			if (S.Length != OrderBytes)
				Array.Resize(ref S, OrderBytes);

			if (BigEndian)
				Array.Reverse(S);			// Big endian

			Buffer.BlockCopy(S, 0, Signature, 0, OrderBytes);

			S = s.ToByteArray();			// Little endian
			if (S.Length != OrderBytes)
				Array.Resize(ref S, OrderBytes);
			
			if (BigEndian)
				Array.Reverse(S);			// Big endian

			Buffer.BlockCopy(S, 0, Signature, OrderBytes, OrderBytes);

			return Signature;
		}

		private static BigInteger CalcE(Stream Data, HashFunctionStream HashFunction,
			int ScalarBytes, byte MsbMask)
		{
			byte[] Hash = HashFunction(Data);
			int c = Hash.Length;

			if (c != ScalarBytes)
				Array.Resize(ref Hash, ScalarBytes);

			Hash[ScalarBytes - 1] &= MsbMask;

			return EllipticCurve.ToInt(Hash);
		}

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="BigEndian">Indicates if the public key is in big-endian format.</param>
		/// <param name="HashFunction">Hash function to use.</param>
		/// <param name="Curve">Elliptic curve</param>
		/// <param name="OrderBytes">Number of bytes used to encode an integer.</param>
		/// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
		/// <param name="MsbMask">Mask for most significant byte.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		public static bool Verify(byte[] Data, byte[] PublicKey, bool BigEndian,
			HashFunctionArray HashFunction, int OrderBytes, int ScalarBytes, byte MsbMask,
			PrimeFieldCurve Curve, byte[] Signature)
		{
			int c = Signature.Length;
			if (c != OrderBytes << 1)
				return false;

			c >>= 1;

			byte[] Bin = new byte[c];
			Buffer.BlockCopy(Signature, 0, Bin, 0, c);

			if (BigEndian)
				Array.Reverse(Bin);

			BigInteger r = EllipticCurve.ToInt(Bin);

			Bin = new byte[c];
			Buffer.BlockCopy(Signature, c, Bin, 0, c);

			if (BigEndian)
				Array.Reverse(Bin);

			BigInteger s = EllipticCurve.ToInt(Bin);
			PointOnCurve PublicKeyPoint = Curve.Decode(PublicKey, BigEndian);

			if (!PublicKeyPoint.NonZero || r.IsZero || s.IsZero || r >= Curve.Order || s >= Curve.Order)
				return false;

			BigInteger e = CalcE(Data, HashFunction, ScalarBytes, MsbMask);
			BigInteger w = Curve.ModulusN.Invert(s);
			BigInteger u1 = Curve.ModulusN.Multiply(e, w);
			BigInteger u2 = Curve.ModulusN.Multiply(r, w);
			PointOnCurve P2 = Curve.ScalarMultiplication(u1, Curve.BasePoint, true);
			PointOnCurve P3 = Curve.ScalarMultiplication(u2, PublicKeyPoint, true);
			Curve.AddTo(ref P2, P3);

			if (!P2.NonZero)
				return false;

			P2.Normalize(Curve);

			BigInteger Compare = BigInteger.Remainder(P2.X, Curve.Order);
			if (Compare.Sign < 0)
				Compare += Curve.Order;

			return Compare == r;
		}

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="BigEndian">Indicates if the public key is in big-endian format.</param>
		/// <param name="HashFunction">Hash function to use.</param>
		/// <param name="Curve">Elliptic curve</param>
		/// <param name="OrderBytes">Number of bytes used to encode an integer.</param>
		/// <param name="ScalarBytes">Number of bytes to use for scalars.</param>
		/// <param name="MsbMask">Mask for most significant byte.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		public static bool Verify(Stream Data, byte[] PublicKey, bool BigEndian,
			HashFunctionStream HashFunction, int OrderBytes, int ScalarBytes, byte MsbMask,
			PrimeFieldCurve Curve, byte[] Signature)
		{
			int c = Signature.Length;
			if (c != OrderBytes << 1)
				return false;

			c >>= 1;

			byte[] Bin = new byte[c];
			Buffer.BlockCopy(Signature, 0, Bin, 0, c);

			BigInteger r = EllipticCurve.ToInt(Bin);

			Bin = new byte[c];
			Buffer.BlockCopy(Signature, c, Bin, 0, c);

			BigInteger s = EllipticCurve.ToInt(Bin);
			PointOnCurve PublicKeyPoint = Curve.Decode(PublicKey, BigEndian);

			if (!PublicKeyPoint.NonZero || r.IsZero || s.IsZero || r >= Curve.Order || s >= Curve.Order)
				return false;

			BigInteger e = CalcE(Data, HashFunction, ScalarBytes, MsbMask);
			BigInteger w = Curve.ModulusN.Invert(s);
			BigInteger u1 = Curve.ModulusN.Multiply(e, w);
			BigInteger u2 = Curve.ModulusN.Multiply(r, w);
			PointOnCurve P2 = Curve.ScalarMultiplication(u1, Curve.BasePoint, true);
			PointOnCurve P3 = Curve.ScalarMultiplication(u2, PublicKeyPoint, true);
			Curve.AddTo(ref P2, P3);

			if (!P2.NonZero)
				return false;

			P2.Normalize(Curve);

			BigInteger Compare = BigInteger.Remainder(P2.X, Curve.Order);
			if (Compare.Sign < 0)
				Compare += Curve.Order;

			return Compare == r;
		}

	}
}
