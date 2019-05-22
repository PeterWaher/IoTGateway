using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Base class of Elliptic curves over a prime field.
    /// </summary>
    public abstract class CurvePrimeField : ModulusP
    {
        /// <summary>
        /// http://waher.se/Schema/EllipticCurves.xsd
        /// </summary>
        public const string Namespace = "http://waher.se/Schema/EllipticCurves.xsd";

        /// <summary>
        /// "EllipticCurve"
        /// </summary>
        public const string ElementName = "EllipticCurve";

        /// <summary>
        /// Random number generator
        /// </summary>
        protected static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

        /// <summary>
        /// Private key
        /// </summary>
		protected BigInteger privateKey;

        /// <summary>
        /// Public key
        /// </summary>
        protected PointOnCurve publicKey;

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

        private readonly ModulusP modN;
        private readonly PointOnCurve g;
        private readonly BigInteger n;
        private readonly int cofactor;

        /// <summary>
        /// Base class of Elliptic curves over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="OrderBits">Number of bits used to encode order.</param>
        public CurvePrimeField(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order,
            int OrderBits)
            : this(Prime, BasePoint, Order, OrderBits, (BigInteger?)null)
        {
        }

        /// <summary>
        /// Base class of Elliptic curves over a prime field.
        /// </summary>
        /// <param name="Prime">Prime base of field.</param>
        /// <param name="BasePoint">Base-point.</param>
        /// <param name="Order">Order of base-point.</param>
        /// <param name="Cofactor">Cofactor of curve.</param>
        /// <param name="D">Private key.</param>
        public CurvePrimeField(BigInteger Prime, PointOnCurve BasePoint, BigInteger Order,
            int Cofactor, BigInteger? D)
            : base(Prime)
        {
            if (Prime <= BigInteger.One)
                throw new ArgumentException("Invalid prime base.", nameof(Prime));

            this.g = BasePoint;
            this.n = Order;
            this.cofactor = Cofactor;
            this.orderBits = CalcBits(this.n);
            this.orderBytes = (this.orderBits + 7) >> 3;
            this.modN = new ModulusP(Order);

            this.msbOrderMask = 0xff;

            int MaskBits = (8 - this.orderBits) & 7;
            if (MaskBits == 0)
            {
                this.orderBytes++;
                this.msbOrderMask = 0;
            }
            else
                this.msbOrderMask >>= MaskBits;

            if (D.HasValue)
                this.SetPrivateKey(D.Value);
            else
                this.GenerateKeys();
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
        /// Cofactor of curve.
        /// </summary>
        public int Cofactor => this.cofactor;

        /// <summary>
        /// Base-point of curve.
        /// </summary>
        public PointOnCurve BasePoint => this.g;

        /// <summary>
        /// Prime of curve.
        /// </summary>
        public BigInteger Prime => this.p;

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
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="P">Point</param>
        /// <returns><paramref name="N"/>*<paramref name="P"/></returns>
        public virtual PointOnCurve ScalarMultiplication(BigInteger N, PointOnCurve P)
        {
            PointOnCurve Result = this.Zero;
            byte[] Bin = N.ToByteArray();
            int i, c = Bin.Length;
            byte b, Bit;

            for (i = 0; i < c; i++)
            {
                b = Bin[i];

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
        /// Negates a point on the curve.
        /// </summary>
        /// <param name="P">Point</param>
        public void Negate(ref PointOnCurve P)
        {
            P.Y = this.p - P.Y;
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
        /// Generates a new Private Key.
        /// </summary>
        public void GenerateKeys()
        {
            this.SetPrivateKey(this.NextRandomNumber());
        }

        /// <summary>
        /// Sets the private key (and therefore also the public key) of the curve.
        /// </summary>
        /// <param name="D">Private key.</param>
        public virtual void SetPrivateKey(BigInteger D)
        {
            this.publicKey = this.ScalarMultiplication(D, this.g);
            this.publicKey.Normalize(this);
            this.privateKey = D;
        }

        /// <summary>
        /// Returns the next random number, in the range [1, n-1].
        /// </summary>
        /// <returns>Random number.</returns>
        public BigInteger NextRandomNumber()
        {
            byte[] B = new byte[this.orderBytes];
            BigInteger D;

            do
            {
                lock (rnd)
                {
                    rnd.GetBytes(B);
                }

                B[this.orderBytes - 1] &= this.msbOrderMask;

                D = new BigInteger(B);
            }
            while (D.IsZero || D >= this.n);

            return D;
        }

        /// <summary>
        /// Public key.
        /// </summary>
        public virtual PointOnCurve PublicKey
        {
            get => this.publicKey;
        }

        /// <summary>
        /// Arithmetic modulus n (the order)
        /// </summary>
        public ModulusP ModulusN => this.modN;

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
            PointOnCurve P = this.ScalarMultiplication(this.privateKey, RemotePublicKey);
            P.Normalize(this);
            byte[] B = P.X.ToByteArray();

            if (B.Length != this.orderBytes)
                Array.Resize<byte>(ref B, this.orderBytes);

            Array.Reverse(B);   // Most significant byte first.

            return HashFunction(B);
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="HashFunction">Hash function to use.</param>
        /// <returns>Signature.</returns>
        public abstract byte[] Sign(byte[] Data);

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public abstract bool Verify(byte[] Data, PointOnCurve PublicKey, byte[] Signature);

        /// <summary>
        /// Exports the curve parameters to XML.
        /// </summary>
        /// <param name="Output">Output</param>
        public virtual void Export(XmlWriter Output)
        {
            Output.WriteStartElement("EllipticCurve", Namespace);
            Output.WriteAttributeString("type", this.GetType().FullName);
            Output.WriteAttributeString("d", this.privateKey.ToString());
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

    }
}
