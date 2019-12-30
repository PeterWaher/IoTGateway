using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Waher.Security.SHA3;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Edwards448 Elliptic Curve, as defined in RFC7748 and RFC8032:
    /// https://tools.ietf.org/html/rfc7748
    /// https://tools.ietf.org/html/rfc8032
    /// </summary>
    public class Edwards448 : EdwardsCurve
    {
        private static readonly BigInteger p0 = BigInteger.Pow(2, 448) - BigInteger.Pow(2, 224) - 1;
        private static readonly BigInteger d0 = p0 - 39081;
        private static readonly BigInteger n0 = BigInteger.Pow(2, 446) - BigInteger.Parse("008335dc163bb124b65129c96fde933d8d723a70aadc873d6d54a7bb0d", NumberStyles.HexNumber);
        private static readonly BigInteger BasePointX = BigInteger.Parse("224580040295924300187604334099896036246789641632564134246125461686950415467406032909029192869357953282578032075146446173674602635247710");
        private static readonly BigInteger BasePointY = BigInteger.Parse("298819210078481492676017930443930673437544040154080242095928241372331506189835876003536878655418784733982303233503462500531545062832660");
        private readonly SHAKE256 shake256_114;
        private readonly bool hashSecret;

        /// <summary>
        /// Edwards448 Elliptic Curve, as defined in RFC7748 and RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        public Edwards448()
            : this(null, true)
        {
        }

        /// <summary>
        /// Edwards448 Elliptic Curve, as defined in RFC7748 and RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        /// <param name="Secret">Secret.</param>
        public Edwards448(byte[] Secret)
            : this(Secret, true)
        {
        }

        /// <summary>
        /// Edwards448 Elliptic Curve, as defined in RFC7748 and RFC8032:
        /// https://tools.ietf.org/html/rfc7748
        /// https://tools.ietf.org/html/rfc8032
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <param name="HashSecret">If the secret should be hashed to create the private key.</param>
        public Edwards448(byte[] Secret, bool HashSecret)
            : base(p0, new PointOnCurve(BasePointX, BasePointY), d0, n0, 4, Secret)
        {
            this.shake256_114 = new SHAKE256(114 << 3);
            this.hashSecret = HashSecret;
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Edwards448";

        /// <summary>
        /// d coefficient of Edwards curve.
        /// </summary>
        protected override BigInteger D => d;

        /// <summary>
        /// Number of bits used to encode the y-coordinate.
        /// </summary>
        public override int CoordinateBits => 447;

        /// <summary>
        /// Calculates a private key from a secret.
        /// </summary>
        /// <param name="Secret">Binary secret.</param>
        /// <returns>Private key</returns>
        public override Tuple<byte[], byte[]> CalculatePrivateKey(byte[] Secret)
        {
            byte[] Bin = this.shake256_114.ComputeVariable(Secret);
            byte[] AdditionalInfo = new byte[57];
            byte[] PrivateKey = new byte[57];

            if (this.hashSecret)
                Array.Copy(Bin, 0, PrivateKey, 0, 57);
            else
                Array.Copy(Secret, 0, PrivateKey, 0, Math.Min(57, Secret.Length));

            Array.Copy(Bin, 57, AdditionalInfo, 0, 57);

            PrivateKey[0] &= 0xfc;
            PrivateKey[55] |= 0x80;
            PrivateKey[56] = 0;

            return new Tuple<byte[], byte[]>(PrivateKey, AdditionalInfo);
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            return EdDSA.Sign(Data, this.PrivateKey, this.AdditionalInfo, this.H_dom4, this);
        }


        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(Stream Data)
        {
            return EdDSA.Sign(Data, this.PrivateKey, this.AdditionalInfo, this.H_dom4, this);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature)
        {
            return EdDSA.Verify(Data, PublicKey, this.H_dom4, this, Signature);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(Stream Data, byte[] PublicKey, byte[] Signature)
        {
            return EdDSA.Verify(Data, PublicKey, this.H_dom4, this, Signature);
        }

        private byte[] H_dom4(byte[] Data)
        {
            int c = Data.Length;
            byte[] Bin = new byte[10 + c];

            Array.Copy(preamble, 0, Bin, 0, 8);
            Bin[8] = 0;                         // x=phflag=0
            Bin[9] = 0;                         // y=context=empty string
            Array.Copy(Data, 0, Bin, 10, c);

            return this.shake256_114.ComputeVariable(Bin);
        }

        private byte[] H_dom4(Stream Data)
        {
            using (TemporaryFile TempFile = new TemporaryFile())
            {
                TempFile.Write(preamble, 0, 8);
                TempFile.WriteByte(0);              // x=phflag=0
                TempFile.WriteByte(0);              // y=context=empty string

                Data.Position = 0;
                Data.CopyTo(TempFile);

                TempFile.Position = 0;
                return this.shake256_114.ComputeVariable(TempFile);
            }
        }

        private static readonly byte[] preamble = Encoding.ASCII.GetBytes("SigEd448");

    }
}
