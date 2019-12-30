using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Abstract base class for elliptic curves.
	/// </summary>
	public abstract class EllipticCurve : ISignatureAlgorithm
	{
		/// <summary>
		/// http://waher.se/Schema/EllipticCurves.xsd
		/// </summary>
		public const string Namespace = "http://waher.se/Schema/EllipticCurves.xsd";

		/// <summary>
		/// 2
		/// </summary>
		public static readonly BigInteger Two = new BigInteger(2);

		/// <summary>
		/// "EllipticCurve"
		/// </summary>
		public const string ElementName = "EllipticCurve";

		/// <summary>
		/// Random number generator
		/// </summary>
		protected static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		/// <summary>
		/// Base point
		/// </summary>
		protected readonly PointOnCurve g;

		/// <summary>
		/// Order
		/// </summary>
		protected readonly BigInteger n;

		/// <summary>
		/// cofactor
		/// </summary>
		protected readonly int cofactor;

		/// <summary>
		/// Number of bits used for the order of the curve.
		/// </summary>
		protected readonly int orderBits;

		/// <summary>
		/// Number of bytes used for the order of the curve.
		/// </summary>
		protected readonly int orderBytes;

		/// <summary>
		/// Mask for most significant byte of scalars.
		/// </summary>
		protected readonly byte msbOrderMask;

		private byte[] secret;
		private byte[] privateKey;
		private byte[] publicKey;
		private byte[] additionalInfo;
		private PointOnCurve publicKeyPoint;

		/// <summary>
		/// Abstract base class for elliptic curves.
		/// </summary>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		public EllipticCurve(PointOnCurve BasePoint, BigInteger Order, int Cofactor)
			: this(BasePoint, Order, Cofactor, null)
		{
		}

		/// <summary>
		/// Abstract base class for elliptic curves.
		/// </summary>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="Secret">Secret.</param>
		public EllipticCurve(PointOnCurve BasePoint, BigInteger Order, int Cofactor,
			byte[] Secret)
		{
			this.g = BasePoint;
			this.n = Order;
			this.cofactor = Cofactor;
			this.secret = Secret;
			this.privateKey = null;
			this.publicKey = null;
			this.additionalInfo = null;

			this.orderBits = ModulusP.CalcBits(this.n);
			this.orderBytes = (this.orderBits + 7) >> 3;
			this.msbOrderMask = 0xff;

			int MaskBits = (8 - this.orderBits) & 7;
			if (MaskBits == 0)
			{
				this.orderBytes++;
				this.msbOrderMask = 0;
			}
			else
				this.msbOrderMask >>= MaskBits;
		}

		/// <summary>
		/// Method initiazing the elliptic curve properties.
		/// </summary>
		protected virtual void Init()
		{
			if (this.secret is null)
				this.secret = this.GenerateSecret();

			this.SetPrivateKey(this.secret);
		}

		/// <summary>
		/// Private key
		/// </summary>
		protected byte[] PrivateKey
		{
			get
			{
				if (this.privateKey is null)
					this.Init();

				return this.privateKey;
			}
		}

		/// <summary>
		/// Encoded public key
		/// </summary>
		public virtual byte[] PublicKey
		{
			get
			{
				if (this.publicKey is null)
					this.Init();

				return this.publicKey;
			}
		}

		/// <summary>
		/// Curve-specific additional information
		/// </summary>
		protected byte[] AdditionalInfo
		{
			get
			{
				if (this.additionalInfo is null)
					this.Init();

				return this.additionalInfo;
			}
		}

		/// <summary>
		/// Public key, as a point on the elliptic curve.
		/// </summary>
		public virtual PointOnCurve PublicKeyPoint
		{
			get
			{
				if (this.publicKey is null)
					this.Init();

				return this.publicKeyPoint;
			}

			internal set
			{
				this.publicKeyPoint = value;
			}
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public abstract string CurveName
		{
			get;
		}

		/// <summary>
		/// Order of curve.
		/// </summary>
		public BigInteger Order => this.n;

		/// <summary>
		/// Number of bytes required to represent the order of the curve.
		/// </summary>
		public int OrderBytes => this.orderBytes;

		/// <summary>
		/// Number of bits required to represent the order of the curve.
		/// </summary>
		public int OrderBits => this.orderBits;

		/// <summary>
		/// Cofactor of curve.
		/// </summary>
		public int Cofactor => this.cofactor;

		/// <summary>
		/// Base-point of curve.
		/// </summary>
		public PointOnCurve BasePoint => this.g;

		/// <summary>
		/// Generates a new secret.
		/// </summary>
		/// <returns>Generated secret.</returns>
		public abstract byte[] GenerateSecret();

		/// <summary>
		/// Sets the private key (and therefore also the public key) of the curve.
		/// </summary>
		/// <param name="Secret">Secret</param>
		public virtual void SetPrivateKey(byte[] Secret)
		{
			Tuple<byte[], byte[]> Info = this.CalculatePrivateKey(Secret);
			PointOnCurve P = this.ScalarMultiplication(Info.Item1, this.g, true);

			this.publicKey = this.Encode(P);
			this.publicKeyPoint = P;
			this.privateKey = Info.Item1;
			this.additionalInfo = Info.Item2;
			this.secret = Secret;
		}

		/// <summary>
		/// Calculates a private key from a secret.
		/// </summary>
		/// <param name="Secret">Binary secret.</param>
		/// <returns>Private key, and curve-specific information</returns>
		public virtual Tuple<byte[], byte[]> CalculatePrivateKey(byte[] Secret)
		{
			return new Tuple<byte[], byte[]>(Secret, null);
		}

		/// <summary>
		/// Encodes a point on the curve.
		/// </summary>
		/// <param name="Point">Normalized point to encode.</param>
		/// <returns>Encoded point.</returns>
		public virtual byte[] Encode(PointOnCurve Point)
		{
			byte[] X = Point.X.ToByteArray();
			byte[] Y = Point.Y.ToByteArray();

			if (X.Length != this.orderBytes)
				Array.Resize<byte>(ref X, this.orderBytes);

			if (Y.Length != this.orderBytes)
				Array.Resize<byte>(ref Y, this.orderBytes);

			byte[] Result = new byte[this.orderBytes << 1];

			Array.Copy(X, 0, Result, 0, this.orderBytes);
			Array.Copy(Y, 0, Result, this.orderBytes, this.orderBytes);

			return Result;
		}

		/// <summary>
		/// Decodes an encoded point on the curve.
		/// </summary>
		/// <param name="Point">Encoded point.</param>
		/// <returns>Decoded point.</returns>
		public virtual PointOnCurve Decode(byte[] Point)
		{
			if (Point.Length != this.orderBytes << 1)
				throw new ArgumentException("Invalid point.", nameof(Point));

			int XLen = this.orderBytes;
			int YLen = this.orderBytes;

			if ((Point[this.orderBytes - 1] & 0x80) != 0)
				XLen++;

			if ((Point[Point.Length - 1] & 0x80) != 0)
				YLen++;

			byte[] X = new byte[XLen];
			byte[] Y = new byte[YLen];

			Array.Copy(Point, 0, X, 0, this.orderBytes);
			Array.Copy(Point, this.orderBytes, Y, 0, this.orderBytes);

			return new PointOnCurve(new BigInteger(X), new BigInteger(Y));
		}

		/// <summary>
		/// Generates a new Private Key.
		/// </summary>
		public void GenerateKeys()
		{
			this.SetPrivateKey(this.GenerateSecret());
		}

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="P">Point</param>
		/// <param name="Normalize">If normalized output is expected.</param>
		/// <returns><paramref name="N"/>*<paramref name="P"/></returns>
		public PointOnCurve ScalarMultiplication(BigInteger N, PointOnCurve P, bool Normalize)
		{
			return this.ScalarMultiplication(N.ToByteArray(), P, Normalize);
		}

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
		/// </summary>
		/// <param name="N">Scalar, in binary, little-endian form.</param>
		/// <param name="P">Point</param>
		/// <param name="Normalize">If normalized output is expected.</param>
		/// <returns><paramref name="N"/>*<paramref name="P"/></returns>
		public virtual PointOnCurve ScalarMultiplication(byte[] N, PointOnCurve P, bool Normalize)
		{
			PointOnCurve Result = this.Zero;
			int i, c = N.Length;
			byte b, Bit;

			for (i = 0; i < c; i++)
			{
				b = N[i];

				for (Bit = 1; Bit != 0; Bit <<= 1)
				{
					if ((b & Bit) != 0)
						this.AddTo(ref Result, P);

					this.Double(ref P);
				}
			}

			return Result;
		}

		/// <summary>
		/// Neutral point.
		/// </summary>
		public virtual PointOnCurve Zero
		{
			get
			{
				return new PointOnCurve(BigInteger.Zero, BigInteger.Zero);
			}
		}

		/// <summary>
		/// Adds <paramref name="Q"/> to <paramref name="P"/>.
		/// </summary>
		/// <param name="P">Point 1.</param>
		/// <param name="Q">Point 2.</param>
		/// <returns>P+Q</returns>
		public abstract void AddTo(ref PointOnCurve P, PointOnCurve Q);

		/// <summary>
		/// Doubles a point on the curve.
		/// </summary>
		/// <param name="P">Point</param>
		public abstract void Double(ref PointOnCurve P);

		/// <summary>
		/// Gets a shared key using the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
		/// </summary>
		/// <param name="RemotePublicKey">Public key of the remote party.</param>
		/// <param name="HashFunction">A Hash function is applied to the derived key to generate the shared secret.
		/// The derived key, as a byte array of equal size as the order of the prime field, ordered by most significant byte first,
		/// is passed on to the hash function before being returned as the shared key.</param>
		/// <returns>Shared secret.</returns>
		public virtual byte[] GetSharedKey(byte[] RemotePublicKey, HashFunctionArray HashFunction)
		{
			return ECDH.GetSharedKey(this.PrivateKey, RemotePublicKey, HashFunction, this);
		}

		/// <summary>
		/// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <returns>Signature.</returns>
		public abstract byte[] Sign(byte[] Data);

		/// <summary>
		/// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <returns>Signature.</returns>
		public abstract byte[] Sign(Stream Data);

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		public abstract bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature);

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		public abstract bool Verify(Stream Data, byte[] PublicKey, byte[] Signature);

		/// <summary>
		/// Exports the curve parameters to XML.
		/// </summary>
		/// <param name="Output">Output</param>
		public virtual void Export(XmlWriter Output)
		{
			if (this.secret is null)
				this.Init();

			Output.WriteStartElement(ElementName, Namespace);
			Output.WriteAttributeString("type", this.GetType().FullName);
			Output.WriteAttributeString("d", Convert.ToBase64String(this.secret));
			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports the curve parameters to an XML string.
		/// </summary>
		public string Export()
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = false,
				OmitXmlDeclaration = true
			};
			StringBuilder sb = new StringBuilder();
			using (XmlWriter w = XmlWriter.Create(sb, Settings))
			{
				this.Export(w);
				w.Flush();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts a little-endian binary representation of a big integer to a 
		/// <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="Binary">Little-endian binary representation of a big integer</param>
		/// <returns><see cref="BigInteger"/></returns>
		public static BigInteger ToInt(byte[] Binary)
		{
			int c = Binary.Length;
			if ((Binary[c - 1] & 0x80) != 0)
				Array.Resize<byte>(ref Binary, c + 1);

			return new BigInteger(Binary);
		}
	}
}
